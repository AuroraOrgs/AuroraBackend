using Aurora.Domain.ValueObjects;

namespace Aurora.Application.Tests.ValueObjects;

public class SearchOptionTermTests
{
    [Fact]
    public void Create_ShouldOrderTerms_IfNotOrdered()
    {
        //Arrange
        var terms = new[] { "zbc", "abc" }; //not ordered
        //Act 
        var expected = "abc,zbc";
        var actual = SearchOptionTerm.CreateAnd(terms).ToString();
        //Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Create_ShoulNotAddDelimeter_WhenOneTermIsProvided()
    {
        //Arrange
        var terms = new[] { "abc" }; //not ordered
        //Act 
        var expected = terms[0];
        var actual = SearchOptionTerm.CreateAnd(terms).ToString();
        //Assert
        actual.Should().BeEquivalentTo(expected);
    }
}