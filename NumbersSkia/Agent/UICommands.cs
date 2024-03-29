namespace NumbersSkia.Agent;

/*
 
SelectMode
    Select
    Deselect
    AddToSelection
    RemoveFromSelection
    ClearSelection
    SelectAreaContained
    SelectAreaTouched
        longdrag on blank is create domain, long drag on domain is create number? drag selected points/lines adjusts

DomainMode
    Create, EditMinMax (expand, contract, set), Duplicate, Delete
    EditDomainGuideline (p0, p1, line, rotate)
BasisMode
    BasisFromNumber, EditBasisLocation, EditBasisTicks (add, subtract, toOne, set)
    EditBasisView (one, line), FlipBasis
NumberMode
    Create, ReplaceFocal, EditFocal (p0, p1, line, fromCenter), FlipPolarity, Reverse, Negate, Duplicate, Delete
EquationMode
    AddEquation, EditEquation (ops), AddToEquation, RemoveFromEquation, RemoveEquationAndContent
StretchMode
    MultiplyFromUnit, MultiplyFromUnot
DrawMode
    AddPath, LockPath, EditPath (stroke, fill, points), RemovePath, AddImage, EditImage (rect, source, alpha, etc), RemoveImage
bjectMode
    AddShape, EditEquation, EditFocus, SampleProperties, RemoveShape
File
    Load, Save, SaveAs, Record, Playback, Step, Pause, NextPage, Reload, PreviousPage
Edit
    Undo, Redo, Merge, RepeatLast
View
    ZoomIn, ZoomOut, ZoomToFit, Pan, ResetView, SetColorTheme

Visual Settings
    ToggleDomainNumberOffsets();
    WorkspaceMapper.ShowSeparatedSegment = !WorkspaceMapper.ShowSeparatedSegment;
    ToggleShowNumbers();
    WorkspaceMapper.ShowFractions = !WorkspaceMapper.ShowFractions;
    ToggleBasisVisible();
    ToggleShowPolarity();
    ToggleGradientNumberline();
    ToggleShowMinorTicks();

*/
public class UICommands
{
    #region Selection
    public void Select()
    {

    }
    public void Deselect()
    {

    }
    public void AddToSelection()
    {

    }
    public void RemoveFromSelection()
    {

    }
    public void ClearSelection()
    {

    }
    public void DeleteSelection()
    {
        //if (ActiveNumberMapper != null)
        //{
        //    var delCommand = new RemoveSKNumberCommand(ActiveNumberMapper);
        //    ActiveNumberMapper = null;
        //    Stack.Do(delCommand);
        //    ClearHighlights(); // todo: Add the selection things to commands.
        //    delCommand.NumberMapper?.OnChange();
        //}
    }
    public void SelectAreaContained()
    {

    }
    public void SelectAreaTouched()
    {

    }
    #endregion
}