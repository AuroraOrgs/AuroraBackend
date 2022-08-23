namespace Aurora.Shared.Tests.Base;

public class Child : Parent
{
    public Child(int parentProp, int childProp) : base(parentProp)
    {
        ParentProp = parentProp;
        ChildProp = childProp;
    }

    public int ChildProp { get; }
}
