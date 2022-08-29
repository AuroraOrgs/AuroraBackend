using Aurora.Application.ValueObjects;

namespace Aurora.Application.Tests.ValueObjects;

public class SearchOptionTermTests
{
    [Fact]
    public void Create_ShouldOrderTest_IfNotOrdered()
    {
        //Arrange
        var terms = new[] { "zbc", "abc" }; //not ordered
        //Act 
        var expected = "abc,zbc";
        var actual = SearchOptionTerm.CreateAnd(terms).ToString();
        //Assert
        actual.Should().BeEquivalentTo(expected);
    }
}