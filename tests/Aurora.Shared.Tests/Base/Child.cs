namespace Aurora.Shared.Tests.Base;

public record Child(int ChildProp, int ParentProp) : Parent(ParentProp);
