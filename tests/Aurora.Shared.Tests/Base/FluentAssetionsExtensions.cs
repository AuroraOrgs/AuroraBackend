using FluentAssertions.Numeric;
using FluentAssertions.Primitives;

namespace Aurora.Shared.Tests.Base
{
    public static class FluentAssetionsExtensions
    {
        public static AndConstraint<TAssertions> NotBeCalled<TSubject, TAssertions>(this ReferenceTypeAssertions<TSubject, TAssertions> assertion, string because = "", params object[] becauseArgs) where TAssertions : ReferenceTypeAssertions<TSubject, TAssertions>
        {
            return assertion.Match(_ => false, $"Should not be called {because}", becauseArgs);
        }

        public static AndConstraint<NumericAssertions<TAssertions>> NotBeCalled<TAssertions>(this NumericAssertions<TAssertions, NumericAssertions<TAssertions>> assertion, string because = "", params object[] becauseArgs) where TAssertions : struct, IComparable<TAssertions>
        {
            return assertion.Match(_ => false, $"Should not be called {because}", becauseArgs);
        }
    }
}
