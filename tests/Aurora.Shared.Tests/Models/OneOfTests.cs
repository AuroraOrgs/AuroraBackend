using Aurora.Shared.Models;

namespace Aurora.Shared.Tests.Models;

public class OneOfTests
{
    [Fact]
    public void ctor_ShouldResolveToFirst_WhenFirstIsSet()
    {
        var number = 123;
        var oneOf = new OneOf<string, int>(number);
        oneOf.Match(str => str.Should().NotBeCalled(), integer => integer.Should().Be(number));
    }

    [Fact]
    public void ctor_ShouldResolveToSecond_WhenSecondIsSet()
    {
        var value = "string";
        var oneOf = new OneOf<string, int>(value);
        oneOf.Match(str => str.Should().Be(value), integer => integer.Should().NotBeCalled());
    }
}
