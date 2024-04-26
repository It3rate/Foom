using Numbers.Commands;
using Numbers.Drawing;
using Numbers.Mappers;
using Numbers.Renderer;
using Numbers.Utils;
using NumbersAPI.CommandEngine;
using NumbersAPI.Motion;
using NumbersCore.Operations;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using NumbersSkia.Agent;
using SkiaSharp;

namespace Numbers.Agent;

public class MouseAgent : CommandAgent, IMouseAgent
{
    #region Properties
    public static Dictionary<int, SKWorkspaceMapper> WorkspaceMappers = new Dictionary<int, SKWorkspaceMapper>();
    public SKWorkspaceMapper WorkspaceMapper
    {
        get
        {
            WorkspaceMappers.TryGetValue(Workspace.Id, out var result);
            return result;
        }
    }

    public CoreRenderer Renderer { get; }
    public Runner Runner;
    public string Text = "";

    public IDemos Demos { get; }
    public bool IsPaused { get; set; } = true;
    public bool IsDown { get; private set; }
    public bool IsDragging { get; private set; }

    public event EventHandler OnModeChange;
    //public event EventHandler OnDisplayModeChange;
    public event EventHandler OnSelectionChange;

    private readonly Highlight _highlight = new Highlight();
    public HighlightSet SelBegin { get; } = new HighlightSet();
    public HighlightSet SelCurrent { get; } = new HighlightSet();
    public HighlightSet SelHighlight { get; } = new HighlightSet();
    public HighlightSet SelSelection { get; } = new HighlightSet();

    public bool DoSyncMatchingBasis { get; set; } = true;

    private ColorTheme _colorTheme = ColorTheme.Normal;
    public ColorTheme ColorTheme
    {
        get => _colorTheme;
        set
        {
            if (_colorTheme != value)
            {
                _colorTheme = value;
                Renderer.GeneratePens(_colorTheme);
            }
        }
    }

    private SKPoint _rawMousePoint;
    private float _minDragDistance = 4f;
    private Focal _lastDragFocal = null;
    public float ScaleTickSize { get; set; } = 0.2f;
    public Keys CurrentKey { get; private set; }
    private UIMode _uiMode = UIMode.Any;
    public UIMode UIMode
    {
        get => _uiMode;
        set
        {
            if (value != _uiMode)
            {
                _uiMode = value;
                OnModeChange?.Invoke(this, new EventArgs());
                SetSelectable(UIMode);
            }
        }
    }

    private Dictionary<int, PRange> SavedNumbers { get; } = new Dictionary<int, PRange>();
    #endregion

    public MouseAgent(Workspace workspace, CoreRenderer renderer, IDemos demos) : base(workspace)
    {
        Renderer = renderer;
        Renderer.Agent = this;
        Runner = new Runner(this);
        Stack.LastTime.SetWith(Runner.CurrentMS);

        Demos = demos;// new Demos(Brain, Renderer);

        ClearMouse();
    }

    #region Mode
    private void SetSelectable(UIMode uiMode)
    {
        switch (uiMode)
        {
            case UIMode.None:
            case UIMode.Any:
                //_selectableKind = ElementKind.Any;
                break;
            //case UIMode.CreateEntity:
            //    _selectableKind = ElementKind.Any;
            //    break;
            //case UIMode.CreateTrait:
            //    _selectableKind = ElementKind.TraitPart;
            //    break;
            //case UIMode.CreateFocal:
            //    _selectableKind = _isControlDown ? ElementKind.TraitPart : ElementKind.FocalPart;
            //    break;
            //case UIMode.CreateBond:
            //    _selectableKind = _isControlDown ? ElementKind.FocalPart : ElementKind.BondPart;
            //    break;
            case UIMode.SetUnit:
                //_selectableKind = ElementKind.Focal;
                break;
                //case UIMode.Equal:
                //    _selectableKind = ElementKind.Focal;
                //    Data.Selected.Clear();
                //    break;
        }
    }
    #endregion

    #region Mouse
    private Number _initialSelectionNum;
    public Number DragMultiplyBasis;
    public SKSegment DragHighlight;
    public SKPoint DragPoint;

    public bool IsCreatingDomain = false;
    public bool IsDraggingBasis = false;
    public bool IsCreatingNumber = false;
    public bool IsManualMultiply = false;
    public bool IsCreatingInvertedNumber = false;
    public SKNumberMapper ActiveNumberMapper;
    public SKDomainMapper ActiveDomainMapper;
    public SKTransformMapper ActiveTransformMapper;
    public SKPathMapper ActivePathMapper;

    public bool MouseDown(MouseArgs e)
    {
        if (IsPaused) { return false; }

        // Add to selection if ctrl down etc.
        _rawMousePoint = e.Location;
        var mousePoint = GetTransformedPoint(_rawMousePoint);
        WorkspaceMapper.GetSnapPoint(_highlight, SelCurrent, mousePoint);
        SelHighlight.Set(_highlight);

        ActiveNumberMapper = _highlight.GetNumberMapper();
        ActiveDomainMapper = _highlight.GetRelatedDomainMapper();
        ActiveTransformMapper = _highlight.GetRelatedTransformMapper(WorkspaceMapper);

        if (e.Button == MouseButtons.Middle)
        {
            StartPan();
        }
        else if (IsBasisAdjustKey)
        {
            // create domain
            IsDraggingBasis = true;
            SelBegin.Set(_highlight.Clone());
        }
        else if (IsCreateDomainKey)
        {
            // create domain
            IsCreatingDomain = true;
            SelBegin.Set(_highlight.Clone());
        }
        else if (CurrentKey == Keys.Space && ActiveDomainMapper != null)
        {
            // dragging full domain
            SelBegin.Set(_highlight.Clone());
            SelBegin.OriginalSegment = ActiveDomainMapper.Guideline.Clone();
        }
        else if (IsCreateNumberKey && ActiveDomainMapper != null)
        {
            // create number
            SelBegin.Set(_highlight.Clone());
            SelBegin.ActiveHighlight.SnapPoint = ActiveDomainMapper.Guideline.ProjectPointOnto(mousePoint, true);
            IsCreatingNumber = true;
            IsCreatingInvertedNumber = _isAltDown; // alt N creates an inverted number
        }
        else if (IsDrawingPathKey)
        {
            // Drawing
            _isDrawing = true;
            var cmd = new AddSKPathCommand(this, WorkspaceMapper.DefaultDrawPen);
            Stack.Do(cmd);
            ActivePathMapper = cmd.PathMapper;
            ActivePathMapper.BeginRecord();
            ActivePathMapper.AddPosition(_rawMousePoint);
        }
        else
        {
            if (_highlight.IsSet)
            {
                SelBegin.Set(_highlight.Clone());
                if (_highlight.Mapper is SKNumberMapper nm && !nm.IsBasis)
                {
                    //SelectNumber(nm);
                    SelSelection.Clear();
                    SelSelection.Set(_highlight.Clone());
                    nm.OnSelect();
                }
            }
            else
            {
                SelSelection.Clear();
            }
        }

        UpdateText(_highlight);
        IsDown = true;
        return true;
    }

    public bool MouseMove(MouseArgs e)
    {
        if (IsPaused) { return false; }

        var result = false;
        _rawMousePoint = e.Location;
        var mousePoint = GetTransformedPoint(_rawMousePoint);
        if (!(IsDown && !IsDragging))
        {
            WorkspaceMapper.GetSnapPoint(_highlight, SelCurrent, mousePoint);
            SelHighlight.Set(_highlight);
        }

        if (IsDown)
        {
            result = MouseDrag(mousePoint);
        }
        else
        {
            UpdateText(_highlight);
        }
        return true;
    }

    private bool _isDrawing = false;
    public bool MouseDrag(SKPoint mousePoint)
    {
        if (IsPaused) { return false; }

        if (_isDrawing)
        {
            ActivePathMapper.AddPosition(_rawMousePoint);
        }
        else if (IsCreatingDomain)
        {
            DragPoint = SelBegin.SnapPosition;
            DragHighlight = new SKSegment(SelBegin.SnapPosition, mousePoint);
        }
        else if (IsCreatingNumber)
        {
            DragPoint = SelBegin.SnapPosition;
            var dm = SelBegin.ActiveHighlight?.GetRelatedDomainMapper();
            var endpoint = dm?.Guideline.ProjectPointOnto(mousePoint, true) ?? mousePoint;
            DragHighlight = new SKSegment(SelBegin.SnapPosition, endpoint);
        }
        else if (SelBegin.HasHighlight && !IsDragging)
        {
            var dist = (mousePoint - SelBegin.Position).Length;
            if (dist > _minDragDistance)
            {
                IsDragging = true;
                SelCurrent.Set(_highlight.Clone());
                if (IsDraggingBasis)
                {
                    var bm = ActiveDomainMapper.BasisNumberMapper;
                    SelCurrent.ActiveHighlight.Mapper = bm;
                    SelBegin.OriginalSegment = bm.Guideline.Clone();
                    SelBegin.OriginalFocal = bm.Number.Focal.Clone();
                }
                else if (SelCurrent.ActiveHighlight?.Mapper is SKNumberMapper nm)
                {
                    SelBegin.OriginalSegment = nm.Guideline.Clone();
                    SelBegin.OriginalFocal = nm.Number.Focal.Clone();
                    if (nm.IsBasis)
                    {
                        SaveNumberValues(SavedNumbers);
                        // test for drag from unit multiplication
                        if (SelSelection.ActiveHighlight?.Mapper is SKNumberMapper snm && !snm.Number.IsBasis) // must have selection
                        {
                            _initialSelectionNum = snm.Number.Clone(false);
                            DragMultiplyBasis = snm.Number.PolarityBasis(SelCurrent.ActiveHighlight.Kind.IsAligned());// snm.Number.Domain.BasisNumber.Clone(false); 
                        }
                    }
                }
            }
        }

        if (IsDragging)
        {
            var activeHighlight = SelCurrent.ActiveHighlight;
            var activeKind = activeHighlight.Kind;
            if (activeHighlight.Mapper is SKPathMapper pm)
            {
            }
            else if (activeHighlight.Mapper is SKNumberMapper nm)
            {
                if (activeKind.IsLine() && !IsDragMultiplyKey)
                {
                    if (nm.IsBasis)
                    {
                        if (!SelSelection.HasHighlight || IsDraggingBasis)
                        {
                            var curT = nm.DomainMapper.Guideline.TFromPoint(_highlight.OriginalPoint, false).Item1;
                            var orgT = nm.DomainMapper.Guideline.TFromPoint(activeHighlight.OriginalPoint, false).Item1;
                            nm.MoveBasisSegmentByT(SelBegin.OriginalSegment, curT - orgT);
                            AdjustBasis(nm);
                        }
                    }
                    else
                    {
                        var basisSeg = nm.GetBasisSegment();
                        var clickT = basisSeg.TFromPoint(activeHighlight.SnapPoint, false).Item1;
                        var curT = basisSeg.TFromPoint(_highlight.OriginalPoint, false).Item1;
                        if (_isShiftDown && SelBegin.ActiveHighlight?.OriginalValue is PRange orgVal)
                        {
                            // expand from center
                            var scale = (curT - clickT) / 2f;
                            nm.Number.Value = new PRange(orgVal.Start + scale, orgVal.End + scale, nm.Number.Polarity == Polarity.Aligned);
                        }
                        else
                        {
                            // drag segment
                            nm.MoveSegmentByT(SelBegin.OriginalSegment, curT - clickT);
                        }

                        if (nm.Number.Focal != _lastDragFocal)
                        {
                            nm.OnChange();
                            _lastDragFocal = nm.Number.Focal.Clone();
                        }
                    }
                }
                else if (activeKind.IsBasis())
                {
                    // set basis with new number
                    if (IsDraggingBasis)
                    {
                        nm.SetValueByKind(_highlight.SnapPoint, activeKind);
                        AdjustBasis(nm);
                    }
                    // drag multiply from basis tip
                    else if (SelSelection.ActiveHighlight?.Mapper is SKNumberMapper snm && activeKind.IsMajor())
                    {
                        IsManualMultiply = true;
                        var g = SelCurrent.ActiveHighlight.Mapper.Guideline;
                        var (t, pt) = g.TFromPoint(_highlight.SnapPoint, false);

                        DragMultiplyBasis.EndValue = DragMultiplyBasis.IsAligned ? t : -t; // dragging is always in render perspective, so account for direction change
                        snm.Number.SetWith(_initialSelectionNum);
                        snm.Number.Multiply(DragMultiplyBasis);
                        DragPoint = pt;
                        DragHighlight = new SKSegment(g.StartPoint, mousePoint);
                    }
                }
                else // expanding number
                {
                    nm.SetValueByKind(_highlight.SnapPoint, activeKind);
                    if (nm.Number.Focal != _lastDragFocal)
                    {
                        nm.OnChange();
                        _lastDragFocal = nm.Number.Focal.Clone();
                    }
                }
            }
            else if (activeHighlight.Mapper is SKDomainMapper dm)
            {
                if (activeKind.IsDomainPoint())
                {
                    if (IsRotateDomainKey)
                    {
                        dm.RotateGuidelineByPoint(_highlight.SnapPoint, activeKind);
                    }
                    else if (CurrentKey == Keys.Space && SelBegin.OriginalSegment != null)
                    {
                        var diff = _highlight.SnapPoint - activeHighlight.SnapPoint;
                        var orgSeg = SelBegin.OriginalSegment;
                        var p0 = orgSeg.StartPoint + diff;
                        dm.SetValueByKind(p0, UIKind.None);
                        var p1 = orgSeg.EndPoint + diff;
                        dm.SetValueByKind(p1, UIKind.Major);
                    }
                    else
                    {
                        dm.SetValueByKind(_highlight.SnapPoint, activeKind);
                    }
                }
                else if (activeKind.IsBoldTick())
                {
                    //dm.SetValueByHighlight(_highlight);
                }
            }
            UpdateText(activeHighlight);
        }
        return true;
    }

    public bool MouseUp(MouseArgs e)
    {
        if (IsPaused) { return false; }

        _rawMousePoint = e.Location;
        var mousePoint = GetTransformedPoint(_rawMousePoint);

        if (UIMode == UIMode.Pan)
        {
            //_startMatrix = Data.Matrix;
            if (_lastKeyUp != null)
            {
                KeyUp(_lastKeyUp);
            }
            else
            {
                UIMode = PreviousMode;
            }
        }
        else if (_isDrawing)
        {
            ActivePathMapper.AddPosition(_rawMousePoint);
            ActivePathMapper.EndRecord();
        }
        else if (IsCreatingDomain)
        {
            var dm = CreateDomain(new SKSegment(SelBegin.SnapPosition, mousePoint));
            dm.Guideline.SnapAngleToStep(5);
            dm.BasisNumberMapper.Reset(dm.SegmentAlongGuideline(dm.UnitRangeOnDomainLine));
        }
        else if (IsCreatingNumber)
        {
            var dm = SelBegin.ActiveHighlight?.GetRelatedDomainMapper();
            if (dm != null && DragHighlight != null)
            {
                var nm = CreateNumber(dm, DragHighlight, IsCreatingInvertedNumber);
                SelectNumber(nm);
            }
        }

        OnSelectionChange?.Invoke(this, new EventArgs());
        ClearMouse();
        UpdateText(SelBegin.ActiveHighlight);

        return true;
    }
    private void SelectNumber(SKNumberMapper nm)
    {
        _highlight.Reset();
        _highlight.Mapper = nm;
        SelSelection.Clear();
        SelSelection.Set(_highlight.Clone());
        nm.OnSelect();
        ActiveNumberMapper = _highlight.GetNumberMapper();
        ActiveDomainMapper = _highlight.GetRelatedDomainMapper();
        ActiveTransformMapper = _highlight.GetRelatedTransformMapper(WorkspaceMapper);
    }
    public bool MouseDoubleClick(MouseArgs e)
    {
        if (IsPaused) { return false; }

        if (CurrentKey == Keys.Space && e.Button == MouseButtons.Left)
        {
            //Data.ResetZoom();
        }
        return true;
    }
    public bool MouseWheel(MouseArgs e)
    {
        if (IsPaused) { return false; }

        var scale = 1f + (Math.Sign(e.Delta) * ScaleTickSize);
        var rawMousePoint = e.Location;
        var mousePoint = GetTransformedPoint(_rawMousePoint);
        //Data.SetPanAndZoom(Data.Matrix, mousePoint, new SKPoint(0, 0), scale);
        return true;
    }
    public void ClearMouse()
    {
        IsDown = false;
        IsDragging = false;
        _isDrawing = false;
        SavedNumbers.Clear();
        SetSelectable(UIMode);
        if (Workspace != null)
        {
            SelBegin?.Reset();
            SelCurrent?.Reset();
        }
        DragMultiplyBasis = null;
        _initialSelectionNum = null;
        _lastDragFocal = null;
        IsCreatingDomain = false;
        IsDraggingBasis = false;
        IsCreatingNumber = false;
        IsManualMultiply = false;
        IsCreatingInvertedNumber = false;
        DragHighlight = null;
        DragPoint = SKPoint.Empty;
    }
    #endregion

    #region Commands
    private SKDomainMapper CreateDomain(SKSegment seg)
    {
        long unitTicks = WorkspaceMapper.DefaultDomainTicks;
        long rangeTicks = unitTicks * WorkspaceMapper.DefaultDomainRange;
        var cdc = new AddSKDomainCommand(Brain.GetLastTrait(), 0, unitTicks, -rangeTicks, rangeTicks, seg, null, "userCreated");
        Stack.Do(cdc);
        return cdc.DomainMapper;
    }
    private SKNumberMapper CreateNumber(SKDomainMapper dm, SKSegment seg, bool isInverted)
    {
        var range = dm.RangeFromSegment(seg);
        range.Start = -range.Start;
        if (isInverted)
        {
            range.InvertPolarityAndRange();
        }
        var anc = new AddSKNumberCommand(dm, range);
        Stack.Do(anc);
        return anc.NumberMapper;
    }
    private void AdjustBasis(SKNumberMapper nm)
    {
        if (IsControlDown())
        {
            nm.AdjustBySegmentChange(SelBegin);
        }

        if (DoSyncMatchingBasis)
        {
            WorkspaceMapper.SyncMatchingBasis(nm.DomainMapper, nm.Number.BasisFocal);
        }
    }
    private void SetSelectedAsBasis()
    {
        var nm = SelSelection.GetNumberMapper();
        if (nm != null && !nm.IsBasis)
        {
            nm.Number.Domain.SetBasisWithNumber(nm.Number);
        }
    }
    private void Reverse()
    {
        var nm = SelSelection.GetNumberMapper();
        if (nm != null)
        {
            nm.Number.Reverse();
        }
    }
    private void NegateSelection()
    {
        var nm = SelSelection.GetNumberMapper();
        if (nm != null)
        {
            nm.Number.Negate();
        }
    }
    private void FlipBasis() // todo: do this domain flip differently, or delete
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.FlipRenderPerspective();
        }
    }
    private void FlipSelected()
    {
        if (SelSelection.ActiveHighlight?.Mapper is SKNumberMapper nm)
        {
            nm.Number.InvertAndReverse();
        }
    }
    private void FlipSelectedPolarity()
    {
        if (SelSelection.ActiveHighlight?.Mapper is SKNumberMapper nm)
        {
            nm.Number.InvertPolarity();
        }
    }
    private void DeleteSelected()
    {
        if (ActiveNumberMapper != null)
        {
            var delCommand = new RemoveSKNumberCommand(ActiveNumberMapper);
            ActiveNumberMapper = null;
            Stack.Do(delCommand);
            ClearHighlights(); // todo: Add the selection things to commands.
            delCommand.NumberMapper?.OnChange();
        }
    }
    private void ClearPaths()
    {
        WorkspaceMapper.ClearPathMappers();
    }
    public void AddTick()
    {
        var sz = _isShiftDown ? 4 : 1;
        if (ActiveDomainMapper != null)
        {
            Workspace.AdjustFocalTickSizeBy(ActiveDomainMapper.Domain, sz);
        }
    }
    public void SubtractTick()
    {
        var sz = _isShiftDown ? 4 : 1;
        if (ActiveDomainMapper != null)
        {
            Workspace.AdjustFocalTickSizeBy(ActiveDomainMapper.Domain, -sz);
        }
    }
    public void ExpandMinMax()
    {
        var sz = _isShiftDown ? 4 : 1;
        if (ActiveDomainMapper != null)
        {
            Workspace.AdjustMinMaxBy(ActiveDomainMapper.Domain, sz);
            ActiveDomainMapper.Recalibrate();
        }
    }
    public void ContractMinMax()
    {
        var sz = _isShiftDown ? 4 : 1;
        if (ActiveDomainMapper != null)
        {
            Workspace.AdjustMinMaxBy(ActiveDomainMapper.Domain, -sz);
            ActiveDomainMapper.Recalibrate();
        }
    }
    #endregion

    public bool IsControlDown() => (Keyboard.ModifierKeys & Keys.Control) == Keys.Control;

    #region Keyboard
    private bool _isControlDown;
    private bool _isShiftDown;
    private bool _isAltDown;
    public bool IsDragMultiplyKey => CurrentKey == Keys.M || CurrentKey == Keys.J;
    public bool IsCreateNumberKey => CurrentKey == Keys.N;
    public bool IsBasisAdjustKey => CurrentKey == Keys.B;
    public bool IsCreateDomainKey => CurrentKey == Keys.D;
    public bool IsDrawingPathKey => CurrentKey == Keys.Q;
    public bool IsRotateDomainKey => CurrentKey == Keys.R;

    private UIMode PreviousMode = UIMode.Any;
    //private SKMatrix _startMatrix;
    private KeyArgs _lastKeyUp;
    public bool KeyDown(KeyArgs e)
    {
        if (CurrentKey == Keys.Escape)
        {
            IsPaused = !IsPaused;
        }
        if (IsPaused) { return false; }


        if (e.KeyCode != Keys.Control && e.KeyCode != Keys.Shift && e.KeyCode != Keys.Alt)
        {
            CurrentKey = e.KeyCode;
        }
        _isControlDown = e.Control;
        _isShiftDown = e.Shift;
        _isAltDown = e.Alt;

        var curMode = UIMode;
        switch (CurrentKey)
        {
            // B: Adjust Basis segment (ctrl to lock tick positions).
            case Keys.B:
                // Adjust Basis
                if (_isControlDown)
                {
                    SetSelectedAsBasis();
                }
                break;
            case Keys.D:
                // Create Domain
                break;
            // D: Create domain
            case Keys.Delete:
                DeleteSelected();
                break;
            case Keys.Escape:
                ClearPaths();
                break;
            case Keys.F:
                FlipSelected();
                break;
            case Keys.F5:
            case Keys.Down:
                Demos.Reload(this);
                break;
            case Keys.I:
                FlipSelectedPolarity();
                break;
            case Keys.K:
                ColorTheme = ColorTheme == ColorTheme.Normal ? ColorTheme.Dark : ColorTheme.Normal;
                break;
            case Keys.Left:
                Demos.PreviousTest(this);
                break;
            case Keys.M:
                // Drag Multiply
                break;
            case Keys.O:
                ToggleDomainNumberOffsets();
                break;
            // N: Create Number
            case Keys.P:
                Runner.TogglePause();
                break;
            case Keys.Q:
                // Drawing
                break;
            case Keys.Right:
                Demos.NextTest(this);
                break;
            // R: Rotate Domain
            case Keys.R:
                if (_isControlDown)
                {
                    Stack.Redo();
                }
                else
                {
                    Reverse();
                }
                break;
            case Keys.S:
                if (_isShiftDown)
                {
                    WorkspaceMapper.ShowSeparatedSegment = !WorkspaceMapper.ShowSeparatedSegment;
                }
                break;
            case Keys.Space:
                StartPan();
                break;
            case Keys.U:
                UIMode = UIMode.SetUnit;
                break;
            //case Keys.Oemplus:
            //    UIMode = UIMode.Equal;
            //    break;
            case Keys.Z:
                if (_isShiftDown && _isControlDown)
                {
                    Stack.Redo();
                    ClearHighlights();
                }
                else if (_isControlDown)
                {
                    Stack.Undo();
                    ClearHighlights();
                }
                break;
            case Keys.D1:
                if (_isShiftDown && ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.NAND);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Nand;
                }
                else
                {
                    ToggleShowNumbers();
                }
                break;
            case Keys.D2:
                if (_isShiftDown)
                {
                    WorkspaceMapper.ShowFractions = !WorkspaceMapper.ShowFractions;
                }
                else
                {
                    ToggleBasisVisible();
                }
                break;
            case Keys.D3:
                ToggleShowPolarity();
                break;
            case Keys.D4:
                ToggleGradientNumberline();
                break;
            case Keys.D5:
                ToggleShowMinorTicks();
                break;
            case Keys.Oemplus:
                if (ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.Add);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Add;
                }
                break;
            case Keys.Subtract:
            case Keys.OemMinus:
                if (ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.Subtract);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Subtract;
                }
                else
                {
                    NegateSelection();
                }
                break;
            case Keys.D8:
            case Keys.Multiply:
                if (ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.Multiply);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Multiply;
                }
                break;
            case Keys.OemBackslash:
            case Keys.Divide:
                if (ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.Divide);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Divide;
                }
                break;
            case Keys.OemPipe:
                if (ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.OR);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.Or;
                }
                break;
            case Keys.D7:
                if (_isShiftDown && ActiveTransformMapper != null)
                {
                    WorkspaceMapper.SetTransformKind(ActiveTransformMapper.Transform.Right, OperationKind.AND);
                    //ActiveTransformMapper.Transform.OperationKind = OperationKind.And;
                }
                break;
            case Keys.Oemtilde:
                FlipBasis();
                break;
            case Keys.Oemcomma: // <
                SubtractTick();
                break;
            case Keys.OemPeriod: // >
                AddTick();
                break;
            case Keys.OemOpenBrackets: // [
                ContractMinMax();
                break;
            case Keys.OemCloseBrackets: // ]
                ExpandMinMax();
                break;
        }
        SetSelectable(UIMode);
        if (curMode != UIMode)
        {
            PreviousMode = curMode;
        }

        UpdateText(_highlight);
        return true;
    }
    public bool KeyUp(KeyArgs e)
    {
        bool result = true;
        if (UIMode == UIMode.Pan && IsDown && _lastKeyUp == null)
        {
            _lastKeyUp = e;
            result = false;
        }
        else
        {
            _lastKeyUp = null;
            CurrentKey = Keys.None;
            _isControlDown = e.Control;
            _isShiftDown = e.Shift;
            //_isAltDown = e.Alt;
            //_startMatrix = Data.Matrix;
            //if (UIMode.IsMomentary())
            //{
            //    UIMode = PreviousMode;
            //}
            SetSelectable(UIMode);
        }
        return result;
    }
    #endregion

    #region Render
    private void UpdateText(Highlight highlight)
    {
        Text = highlight?.GetNumberMapper()?.Number.ToString() ?? "";
        Runner.SetEquationText(Text);
    }
    public void ToggleBasisVisible()
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.ShowBasis = !dm.ShowBasis;
            dm.ShowBasisMarkers = !dm.ShowBasisMarkers;
        }
    }
    public void ToggleShowPolarity()
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.ShowPolarity = !dm.ShowPolarity;
        }
    }
    public void ToggleGradientNumberline()
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.ShowGradientNumberLine = !dm.ShowGradientNumberLine;
        }
    }
    public void ToggleShowMinorTicks()
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.ShowMinorTicks = !dm.ShowMinorTicks;
        }
    }
    public void ToggleShowNumbers()
    {
        foreach (var dm in WorkspaceMapper.DomainMappers())
        {
            dm.ShowValueMarkers = !dm.ShowValueMarkers;
            dm.ShowTicks = !dm.ShowTicks;
            dm.ShowMinorTicks = !dm.ShowMinorTicks;
        }
    }
    private void ToggleDomainNumberOffsets()
    {
        if (ActiveDomainMapper != null)
        {
            ActiveDomainMapper.ShowNumbersOffset = !ActiveDomainMapper.ShowNumbersOffset;
        }
    }
    private void StartPan()
    {
        if (UIMode != UIMode.Pan)
        {
            //_startMatrix = Data.Matrix;
        }
        UIMode = UIMode.Pan;
    }
    public void Draw()
    {
        HighlightSet sel = SelHighlight;
        if (sel.HasHighlight)
        {
            var pen = sel.ActiveHighlight.Kind.IsLine() ? Renderer.Pens.HighlightPen : Renderer.Pens.HoverPen;
            Renderer.Canvas.DrawPath(sel.ActiveHighlight.HighlightPath(), pen);
        }
    }
    public SKPoint GetTransformedPoint(SKPoint point) => point; // will be matrix etc
    #endregion

    #region Serialize
    public void SaveNumberValues(Dictionary<int, PRange> numValues, params int[] ignoreIds)
    {
        numValues.Clear();
        //foreach (var kvp in Workspace.AddDomains)
        //{
        // if (!ignoreIds.Contains(kvp.Key))
        // {
        //  numValues.Add(kvp.Key, kvp.Value.Value);
        // }
        //}
    }
    public void RestoreNumberValues(Dictionary<int, PRange> numValues, params int[] ignoreIds)
    {
        //foreach (var kvp in numValues)
        //{
        // var id = kvp.Key;
        // var storedValue = kvp.Value;
        // if (!ignoreIds.Contains(id))
        // {
        //  Brain.NumberStore[id].Value = storedValue;
        // }
        //}
    }
    #endregion

    public void ClearHighlights()
    {
        Text = "";
        _highlight.Reset();
        SelBegin.Clear();
        SelCurrent.Clear();
        SelHighlight.Clear();
        SelSelection.Clear();
        ActiveNumberMapper = null;
        ActiveDomainMapper = null;
        ActiveTransformMapper = null;
        ActivePathMapper = null;
    }
    public override void ClearAll()
    {
        base.ClearAll();
        ClearMouse();
        ClearHighlights();
        foreach (var workspaceMapper in WorkspaceMappers.Values)
        {
            workspaceMapper.ClearAll();
        }
        WorkspaceMappers.Clear();
        Brain.ClearAll();
        Runner.Clear();

        Brain.Workspaces.Add(Workspace);
    }
}
