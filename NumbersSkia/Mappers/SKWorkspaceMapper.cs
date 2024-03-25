using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Numbers.Agent;
using Numbers.Drawing;
using Numbers.Renderer;
using Numbers.Utils;
using NumbersCore.Primitives;
using NumbersSkia.Agent;

//using OpenTK.Graphics.ES30;
using SkiaSharp;

namespace Numbers.Mappers;

public class SKWorkspaceMapper : SKMapper
{
    private readonly Dictionary<int, SKDomainMapper> _domainMappers = new Dictionary<int, SKDomainMapper>();
    private readonly Dictionary<int, SKTransformMapper> _transformMappers = new Dictionary<int, SKTransformMapper>();
    private readonly Dictionary<int, SKPathMapper> _pathMappers = new Dictionary<int, SKPathMapper>();
    private readonly Dictionary<int, SKTextMapper> _textMappers = new Dictionary<int, SKTextMapper>();
    private readonly Dictionary<int, SKImageMapper> _imageMappers = new Dictionary<int, SKImageMapper>();
    private readonly Dictionary<int, IDrawableElement> _uiControls = new Dictionary<int, IDrawableElement>();
    private int _nextOffset = 0;
    public const float SnapDistance = 5.0f;

    public SKWorkspaceMapper(MouseAgent agent, float left, float top, float width, float height) : base(agent, agent.Workspace)
    {
        //Id = idCounter++;
        MouseAgent.WorkspaceMappers[Id] = this;
        Reset(new SKPoint(left, top), new SKPoint(left + width, top + height));
        DefaultTextPen = DefaultWorkspaceText;
        DefaultGhostPen = DefaultWorkspaceGhostText;
        DefaultBorderPen = Pens.TickBoldPen;
    }

    #region Domains
    public SKDomainMapper GetDomainMapper(Domain domain) => GetOrCreateDomainMapper(domain);
    public IEnumerable<SKDomainMapper> DomainMappers()
    {
        foreach (var dm in _domainMappers.Values)
        {
            yield return dm;
        }
    }
    public IEnumerable<SKNumberMapper> AllNumberMappers(bool reverse = false)
    {
        //foreach (var tm in GetTransformMappers(reverse))
        //{
        //    yield return tm.Transform.Result;
        //}
        foreach (var dm in GetDomainMappers(reverse))
        {
            foreach (var nm in dm.GetNumberMappers(reverse))
            {
                yield return nm;
            }
        }
    }
    public SKDomainMapper DomainMapperAt(int index)
    {
        return (index >= 0 && index < _domainMappers.Count) ? _domainMappers.Values.ElementAtOrDefault(index) : null;
    }
    public SKDomainMapper LastDomainMapper() => DomainMapperAt(_domainMappers.Count - 1);
    public long DefaultDomainTicks { get; set; } = 4;
    public long DefaultDomainRange { get; set; } = 5;
    public SKDomainMapper GetOrCreateDomainMapper(Domain domain, SKSegment line = null, SKSegment unitLine = null, string label = "")
    {
        if (!_domainMappers.TryGetValue(domain.Id, out var result))
        {
            Workspace.AddDomains(true, domain);
            line = line ?? NextDefaultLine();
            var len = (float)domain.MinMaxFocal.AbsLengthInTicks;
            var tickSize = 1.0f / len;
            var zeroPt = (domain.BasisFocal.StartPosition - domain.MinMaxFocal.StartPosition) / len;
            var uSeg = unitLine ?? line.SegmentAlongLine(zeroPt, zeroPt + (tickSize * domain.BasisFocal.LengthInTicks));
            result = new SKDomainMapper(Agent, domain, line, uSeg);
            SetDomainToDefaults(result);
            if(label != "")
            {
                result.Label = label;
            }
            _domainMappers[domain.Id] = result;
        }
        return (SKDomainMapper)result;
    }
    public SKDomainMapper AddDomain(Domain domain, float precentOffset, bool isHorizontal = true, int margins = 50)
    {
        Agent.Workspace.AddDomains(true, domain);
        var seg = isHorizontal ? GetHorizontalSegment(precentOffset, margins) : GetVerticalSegment(precentOffset, margins);
        var dm = GetOrCreateDomainMapper(domain, seg);
        SetDomainToDefaults(dm);
        return dm;
    }
    public SKDomainMapper AddDomain(Domain domain, SKSegment seg)
    {
        Agent.Workspace.AddDomains(true, domain);
        var dm = GetOrCreateDomainMapper(domain, seg);
        SetDomainToDefaults(dm);
        return dm;
    }
    public SKDomainMapper AddHorizontal(Domain domain, int margins = 50)
    {
        return AddDomain(domain, 0.5f, true, margins);
    }
    public SKDomainMapper AddVertical(Domain domain, int margins = 50)
    {
        return AddDomain(domain, 0.5f, false, margins);
    }
    public IEnumerable<SKDomainMapper> GetDomainMappers(bool reverse = false)
    {
        var vals = reverse ? _domainMappers.Values.Reverse() : _domainMappers.Values;
        foreach (var mapper in vals)
        {
            if (mapper is SKDomainMapper dm)
            {
                yield return dm;
            }
        }
    }
    public SKDomainMapper DomainMapperByIndex(int index)
    {
        SKDomainMapper result = null;
        if (index < _domainMappers.Count)
        {
            foreach (var dm in _domainMappers.Values)
            {
                if (--index < 0)
                {
                    result = dm;
                    break;
                }
            }
        }
        return result;
    }
    public bool RemoveDomainMapper(SKDomainMapper domainMapper)
    {
        var domain = domainMapper.Domain;
        domain.IsVisible = false;
        domain.Trait.RemoveDomain(domain);
        var result = _domainMappers.Remove(domainMapper.Domain.Id);
        Workspace.RemoveDomains(true, domainMapper.Domain);
        return result;
    }

    public (SKDomainMapper, SKNumberMapper) CreateLinkedNumber(Number linkTarget)
    {
        var result = GetOrCreateDomainMapper(linkTarget.Domain);
        linkTarget.Domain.IsVisible = true;
        if (linkTarget.Domain.Name != "")
        {
            result.Label = linkTarget.Domain.Name;
        }
        result.BasisNumber.Focal = linkTarget.Domain.BasisNumber.Focal;
        var num = result.CreateNumber(linkTarget.Focal);
        return (result, num);
    }
    #endregion
    #region Transforms
    public IEnumerable<SKTransformMapper> GetTransformMappers(bool reverse = false)
    {
        var vals = reverse ? _transformMappers.Values.Reverse() : _transformMappers.Values;
        foreach (var mapper in vals)
        {
            if (mapper is SKTransformMapper tm)
            {
                yield return tm;
            }
        }
    }
    public SKTransformMapper GetOrCreateTransformMapper(int id)
    {
        return GetOrCreateTransformMapper(Brain.TransformStore[id]);
    }
    public SKTransformMapper GetOrCreateTransformMapper(Transform transform, bool doRender = false)
    {
        if (!_transformMappers.TryGetValue(transform.Id, out var result))
        {
            result = new SKTransformMapper(Agent, transform);
            _transformMappers[transform.Id] = result;
            result.Do2DRender = doRender;
        }
        return (SKTransformMapper)result;
    }
    public SKTransformMapper TransformMapperInvolving(Number num)
    {
        SKTransformMapper result = null;
        foreach (var tm in _transformMappers.Values)
        {
            if (tm.Transform.Involves(num))
            {
                result = tm;
                break;
            }
        }
        return result;
    }
    public SKTransformMapper[] TransformMappersWithInput(Number num)
    {
        var result = new List<SKTransformMapper>();
        foreach (var tm in _transformMappers.Values)
        {
            if (tm.Transform.Right == num)
            {
                result.Add(tm);
            }
        }
        return result.ToArray();
    }
    public void SetTransformKind(Number num, OperationKind kind)
    {
        var xforms = TransformMappersWithInput(num);
        foreach(var xform in xforms)
        {
            xform.Transform.OperationKind = kind;
        }
    }
    public SKTransformMapper TransformMapper(Transform transform) => GetOrCreateTransformMapper(transform);
    public IEnumerable<SKTransformMapper> TransformMappers()
    {
        foreach (var tm in _transformMappers.Values)
        {
            yield return tm;
        }
    }
    #endregion
    #region Text
    public static SKPaint DefaultWorkspaceText = CorePens.GetText(SKColor.Parse("#6060D0"), 16);
    public static SKPaint DefaultWorkspaceGhostText = CorePens.GetText(SKColor.Parse("#A0A0F0"), 16);
    public SKPaint DefaultTextPen { get; set; }
    public SKPaint DefaultGhostPen { get; set; }
    public SKPaint DefaultDrawPen { get; set; }
    public void GhostOldText(SKTextMapper tm = null, int index = -1)
    {
        tm = tm ?? LastTextMapper();
        tm?.GhostTextBefore(tm.TextElement.Lines.Count, DefaultGhostPen);
    }
    public void AppendText(params string[] lines)
    {
        var ltm = LastTextMapper();
        if (ltm != null)
        {
            GhostOldText();
            ltm.TextElement.Lines.AddRange(lines);
        }
    }
    public SKTextMapper LastTextMapper() => TextMapperAt(_textMappers.Count - 1);
    public SKTextMapper TextMapperAt(int index)
    {
        return (index >= 0 && index < _textMappers.Count) ? _textMappers.Values.ElementAtOrDefault(index) : null;
    }
    public IEnumerable<SKTextMapper> TextMappers()
    {
        foreach (var tm in _textMappers.Values)
        {
            yield return tm;
        }
    }

    public SKTextMapper CreateTextMapper(string[] lines, SKSegment line = null)
    {
        var textMapper = new SKTextMapper(Agent, lines, line);
        textMapper.Pen = DefaultTextPen;
        _textMappers.Add(textMapper.Id, textMapper);
        return textMapper;
    }
    public void AddTextMapper(SKTextMapper textMapper)
    {
        _textMappers.Add(textMapper.Id, textMapper);
    }
    public void RemoveTextMapper(SKTextMapper textMapper)
    {
        _textMappers.Remove(textMapper.Id);
    }
    #endregion
    #region Paths
    public IEnumerable<SKPathMapper> PathMappers()
    {
        foreach (var pm in _pathMappers.Values)
        {
            yield return pm;
        }
    }
    public SKPathMapper CreatePathMapper(SKSegment line = null)
    {
        var pathMapper = new SKPathMapper(Agent, line);
        if(DefaultDrawPen != null)
        {
            pathMapper.Pen = DefaultDrawPen;
        }
        AddPathMapper(pathMapper);
        return pathMapper;
    }
    public void AddPathMapper(SKPathMapper pathMapper)
    {
        _pathMappers.Add(pathMapper.Id, pathMapper);
    }
    public void RemovePathMapper(SKPathMapper pathMapper)
    {
        _pathMappers.Remove(pathMapper.Id);
    }
    public void ClearPathMappers()
    {
        //var keysToRemove = _pathMappers.Where(kv => !kv.Value.IsPermanent).Select(kv => kv.Key);

        //foreach (int key in keysToRemove)
        //{
        //    _pathMappers.Remove(key);
        //}
        List<int> keysToRemove = new List<int>();

        foreach (int key in _pathMappers.Keys)
        {
            if (!_pathMappers[key].IsPermanent)
            {
                keysToRemove.Add(key);
            }
        }

        foreach (int key in keysToRemove)
        {
            _pathMappers.Remove(key);
        }
    }
    public SKPathMapper PathMapperAt(int index)
    {
        return (index >= 0 && index < _pathMappers.Count) ? _pathMappers.Values.ElementAtOrDefault(index) : null;
    }
    public SKPathMapper LastPathMapper() => PathMapperAt(_pathMappers.Count - 1);
    #endregion
    #region Controls
    public IEnumerable<IDrawableElement> UIControls()
    {
        foreach (var pm in _uiControls.Values)
        {
            yield return pm;
        }
    }
    public void AddUIControl(IDrawableElement control)
    {
        _uiControls.Add(control.Id, control);
    }
    public void RemoveUIControl(IDrawableElement control)
    {
        _uiControls.Remove(control.Id);
    }
    #endregion
    #region Images
    public SKPaint DefaultBorderPen { get; set; }
    public SKImageMapper CreateImageMapper(string path, SKSegment topLine)
    {
        var imageMapper = new SKImageMapper(Agent, path, topLine);
        imageMapper.BorderPen = DefaultBorderPen;
        AddImageMapper(imageMapper);
        return imageMapper;
    }
    public void AddImageMapper(SKImageMapper imageMapper)
    {
        _imageMappers.Add(imageMapper.Id, imageMapper);
    }
    public void RemoveImageMapper(SKImageMapper imageMapper)
    {
        _imageMappers.Remove(imageMapper.Id);
    }
    public SKImageMapper LastImageMapper() => _imageMappers.Values.LastOrDefault();
    #endregion

    public override SKPath GetHighlightAt(Highlight highlight)
	    {
		    return Renderer.GetRectPath(TopLeft, BottomRight);
	    }
    public Highlight GetSnapPoint(Highlight highlight, HighlightSet ignoreSet, SKPoint input, float maxDist = SnapDistance * 2f)
    {
        highlight.Reset();
        highlight.OriginalPoint = input;

        // manually create number on domain line
        if (Agent.IsCreateNumberKey)
        {
            foreach (var dm in _domainMappers.Values)
            {
                if (dm.Guideline.DistanceTo(input, true) < maxDist)
                {
                    var kind = UIKind.Line | UIKind.Domain;
                    var t = dm.Guideline.TFromPoint(input, false).Item1;
                    highlight.Set(input, input, dm, t, kind);
                    goto Found;
                }
            }
        }

        // number segments and units
        foreach (var nm in AllNumberMappers(true))
        {
            if (nm.RenderSegment != null)
            {
                var seg = nm.RenderSegment;
                // todo: selection depends on added order, but should be render order (basis last). When switching basis the order isn't the same.
                var isSameMapper = ignoreSet.ActiveHighlight != null && ignoreSet.ActiveHighlight.Mapper == nm;
                var kind = UIKind.Number | (nm.IsBasis ? UIKind.Basis : UIKind.None);
                if (nm.IsBasis && !Agent.IsBasisAdjustKey && !Agent.IsDragMultiplyKey)
                {
                    continue; // only adjust basis when B is down
                }
                if (!nm.IsBasis && Agent.IsDragMultiplyKey)
                {
                    continue; // help with selecting unit drag multiply when M pressed
                }
		            else if (!isSameMapper && Agent.CurrentKey != Keys.Space && input.DistanceTo(seg.EndPoint) < maxDist) // prefer endpoint if zero length
                {
                    highlight.Set(input, seg.EndPoint, nm, 1, kind | UIKind.Point | UIKind.Major, nm.Number.Value);
			            goto Found;
		            }
		            if (!isSameMapper && Agent.CurrentKey != Keys.Space && input.DistanceTo(seg.StartPoint) < maxDist) // space always drags line
		            {
			            highlight.Set(input, seg.StartPoint, nm, 0, kind | UIKind.Point, nm.Number.Value);
			            goto Found;
		            }
		            else if (!isSameMapper && seg.DistanceTo(input, true) < maxDist && !Agent.IsDragMultiplyKey)
		            {
			            var t = nm.DomainMapper.BasisSegment.TFromPoint(input, false).Item1;
			            highlight.Set(input, input, nm, t, kind | UIKind.Line, nm.Number.Value);
                    goto Found;
                }
	            }
        }

        foreach (var dm in _domainMappers.Values)
        {
            // Domain segment endpoints
            for (int i = 0; i < dm.EndPoints.Length; i++)
            {
                var dmPt = dm.EndPoints[i];
                if (input.DistanceTo(dmPt) < maxDist)
                {
                    var kind = UIKind.Domain | UIKind.Point | (i > 0 ? UIKind.Major : UIKind.None);
                    highlight.Set(input, dmPt, dm, (float)i, kind);
                    goto Found;
                }
            }
            // domain number line bold ticks
            foreach (var dmTickPoint in dm.TickPoints)
            {
                if (input.DistanceTo(dmTickPoint) < maxDist / 2f)
                {
                    var invertedBasis = dm.InvertedBasisSegment;
                    if (Agent.IsDragMultiplyKey && input.DistanceTo(invertedBasis.EndPoint) < maxDist)
                    {
                        //if(Agent.CurrentKey == Keys.J) // temporary always drag multiply as positive unit for demo purposes
                        //{
                        //    var kind = UIKind.Number | UIKind.Basis | UIKind.Major;
                        //    highlight.Set(input, invertedBasis.EndPoint, dm.BasisNumberMapper, 1, kind);
                        //}
                        //else
                        //{
                        var kind = UIKind.Number | UIKind.Basis | UIKind.Major | UIKind.Inverted;
                        highlight.Set(input, invertedBasis.EndPoint, dm.BasisNumberMapper, 1, kind);
                        //}
                        goto Found;
                    }
                    else
                    {
                        var kind = UIKind.Tick | UIKind.Major;
                        var (t, _) = dm.Guideline.TFromPoint(dmTickPoint, false);
                        highlight.Set(input, dmTickPoint, dm, t, kind);
                        goto Found;
                    }
                }
            }
        }

        Found:
        return highlight;
    }
    public SKSegment GetHorizontalSegment(float t, int margins)
    {
        var offset = Height * t;
        var result = TopSegment + new SKPoint(0, offset);
        return result.InsetSegment(margins);
    }
    public SKSegment GetHorizontalSegment(int offset, int margins = 0)
    {
        var result = TopSegment + new SKPoint(0, offset);
        return result.InsetSegment(margins);
    }
    public SKSegment GetVerticalSegment(float t, int margins)
    {
	        var offset = Width * t;
	        var result = LeftSegment + new SKPoint(offset, 0);
	        return result.InsetSegment(margins);
    }
    public void OffsetNextLineBy(int offsetToAdd)
    {
        _nextOffset += offsetToAdd;
    }
    public int LineOffsetSize = 40;
    public SKSegment NextDefaultLine()
    {
        var result = GetHorizontalSegment(_nextOffset);
        _nextOffset += LineOffsetSize;
	        return result;
    }
    public void SyncMatchingBasis(SKDomainMapper domainMapper, Focal focal)
    {
        var nbRange = domainMapper.UnitRangeOnDomainLine;
        foreach (var sibDomain in Workspace.ActiveSiblingDomains(domainMapper.Domain))
        {
            if (sibDomain.BasisFocal.Id == focal.Id)
            {
                GetDomainMapper(sibDomain).UnitRangeOnDomainLine = nbRange;
            }
        }
    }

    #region Rendering

    public SKPoint TopLeft
    {
        get => Guideline.StartPoint;
        set => Guideline.StartPoint = value;
    }
    public SKPoint BottomRight
    {
        get => Guideline.EndPoint;
        set => Guideline.EndPoint = value;
    }
    public SKPoint Center => TopLeft.Midpoint(BottomRight);
    public float Width => BottomRight.X - TopLeft.X;
    public float Height => BottomRight.Y - TopLeft.Y;
    // these are pointing clockwise 
    public SKSegment TopSegment => new SKSegment(TopLeft.X, TopLeft.Y, BottomRight.X, TopLeft.Y);
    public SKSegment RightSegment => new SKSegment(BottomRight.X, TopLeft.Y, BottomRight.X, BottomRight.Y);
    public SKSegment BottomSegment => new SKSegment(BottomRight.X, BottomRight.Y, TopLeft.X, BottomRight.Y);
    public SKSegment LeftSegment => new SKSegment(TopLeft.X, BottomRight.Y, TopLeft.X, TopLeft.Y);


    public bool ShowFractions
    {
        get => DefaultShowFractions;
        set
        {
            DefaultShowFractions = value;
            foreach (var dm in DomainMappers())
            {
                dm.ShowFractions = DefaultShowFractions;
            }
        }
    }
    public bool ShowSeparatedSegment
    {
        get => DefaultShowSeparatedSegment;
        set
        {
            DefaultShowSeparatedSegment = value;
            foreach (var dm in DomainMappers())
            {
                dm.ShowSeparatedSegment = DefaultShowSeparatedSegment;
            }
        }
    }
    public bool DefaultShowInfoOnTop { get; set; } = true;
    public bool DefaultShowGradientNumberLine { get; set; } = true;
    public bool DefaultShowPolarity { get; set; } = true;
    public bool DefaultShowBasis { get; set; } = true;
    public bool DefaultShowBasisMarkers { get; set; } = true;
    public bool DefaultShowTicks { get; set; } = true;
    public bool DefaultShowMinorTicks { get; set; } = true;
    public bool DefaultShowFractions { get; set; } = true;
    public bool DefaultShowMaxMinValues { get; set; } = false;
    public bool DefaultShowNumbersOffset { get; set; } = false;
    public bool DefaultShowSeparatedSegment { get; set; } = false;
    public bool DefaultShowValueMarkers { get; set; } = true;
    
    public void ShowAll()
    {
        DefaultShowGradientNumberLine = true;
        DefaultShowPolarity = true;
        DefaultShowBasis = true;
        DefaultShowBasisMarkers = true;
        DefaultShowTicks = true;
        DefaultShowMinorTicks = true;
        DefaultShowFractions = true;
        DefaultShowMaxMinValues = true;
    }
    public void ShowNone()
    {
        DefaultShowGradientNumberLine = false;
        DefaultShowPolarity = false;
        DefaultShowBasis = false;
        DefaultShowBasisMarkers = false;
        DefaultShowTicks = false;
        DefaultShowMinorTicks = false;
        DefaultShowFractions = false;
        DefaultShowMaxMinValues = false;
    }
    private void SetDomainToDefaults(SKDomainMapper dm)
    {
        dm.ShowInfoOnTop = DefaultShowInfoOnTop;
        dm.ShowGradientNumberLine = DefaultShowGradientNumberLine;
        dm.ShowPolarity = DefaultShowPolarity;
        dm.ShowBasis = DefaultShowBasis;
        dm.ShowBasisMarkers = DefaultShowBasisMarkers;
        dm.ShowTicks = DefaultShowTicks;
        dm.ShowMinorTicks = DefaultShowMinorTicks;
        dm.ShowFractions = DefaultShowFractions;
        dm.ShowMaxMinValues = DefaultShowMaxMinValues;
        dm.ShowNumbersOffset = DefaultShowNumbersOffset;
        dm.ShowValueMarkers = DefaultShowValueMarkers;
    }
    public bool Default { get; set; } = true;
    public override void Draw()
    {
	        EnsureRenderers();
	        if (Workspace.IsActive)
        {
            foreach (var imageMapper in _imageMappers.Values)
            {
                imageMapper.Draw();
            }
            foreach (var control in _uiControls.Values)
            {
                control.Draw();
            }
            foreach (var textMapper in _textMappers.Values)
            {
                textMapper.Draw();
            }

            foreach (var pathMapper in _pathMappers.Values)
            {
                pathMapper.Draw();
            }

	            foreach (var transformMapper in _transformMappers.Values)
		        {
			        transformMapper.Draw();
            }

            foreach (var domainMapper in _domainMappers.Values)
            {
                domainMapper.Draw();
            }

            if (Agent.DragHighlight != null)
            {
                if (Agent.IsCreatingDomain)
                {

                    Renderer.DrawGradientNumberLine(Agent.DragHighlight, true, 5);
                }
                else if (Agent.IsCreatingNumber)
                {
                    Renderer.DrawDirectedLine(Agent.DragHighlight, Renderer.Pens.SegPens[0]);
                    var pen = Agent.IsCreatingInvertedNumber ? Renderer.Pens.UnotInlinePen : Renderer.Pens.UnitInlinePen;
                    Renderer.DrawDirectedLine(Agent.DragHighlight, pen);
                }
                else if(Agent.IsManualMultiply && Agent.ActiveDomainMapper != null)
                {
                    Renderer.DrawSegment(Agent.DragHighlight, Pens.ThickHighlightPen);
                    var pt = Agent.DragHighlight.EndPoint + new SKPoint(0, -6);
                    var isUnit = Agent.SelCurrent.ActiveHighlight.Kind.IsAligned();
                    Agent.ActiveDomainMapper.DrawEndFraction(Agent.DragMultiplyBasis, isUnit, pt);
                }
                else
                {
                    Renderer.DrawSegment(Agent.DragHighlight, Pens.ThickHighlightPen);
                }
            }
            if (Agent.DragPoint != SKPoint.Empty && Agent.SelSelection.ActiveHighlight?.Mapper is SKNumberMapper snm)
            {
                var pen = snm.Number.IsAligned ? Pens.UnitPenLight : Pens.UnotPenLight;
                Renderer.Canvas.DrawPath(Renderer.GetCirclePath(Agent.DragPoint, 4), pen);
            }
        }
    }
    public void EnsureRenderers()
    {
        foreach (var trait in Brain.TraitStore.Values)
	        {
		        foreach (var domain in trait.DomainStore.Values)
		        {
                if (domain.IsVisible)
                {
			            var dm = GetOrCreateDomainMapper(domain);
			            foreach (var number in domain.Numbers())
			            {
				            var nm = dm.GetOrCreateNumberMapper(number);
			            }
                }
		        }
		        foreach (var transform in Brain.TransformStore.Values)
		        {
			        GetOrCreateTransformMapper(transform);
		        }
	        }
    }
    #endregion

    public void ClearAll()
    {
	        _domainMappers.Clear();
	        _transformMappers.Clear();
        _nextOffset = 0;
    }
}
