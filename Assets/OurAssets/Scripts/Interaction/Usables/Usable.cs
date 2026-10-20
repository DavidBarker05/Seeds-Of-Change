public struct UseInfo
{
    public bool FinishedUsing { get; set; }
    public object[] OutArguments { get; set; }
}

public abstract class Usable : Holdable
{
    public abstract UseInfo Use(Interactable targetInteractable);
}