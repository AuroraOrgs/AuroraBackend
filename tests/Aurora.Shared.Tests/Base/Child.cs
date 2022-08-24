namespace Aurora.Shared.Tests.Base;

public sealed record Child(int ChildProp, int ParentProp) : Parent(ParentProp);
