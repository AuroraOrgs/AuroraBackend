using Aurora.Scrapers.Extensions;

namespace Aurora.Scrapers.Tests;

public class StringExtensionsTest
{
    [Fact]
    public void ExtractAllNumbers_ShouldGetAll_WhenTheyArePresent()
    {
        //Arrange
        var str = "showing pages 10-20 out of 88";
        //Act 
        var expectedNumber = new long[] { 10, 20, 88 };
        var actualNums = str.ExtractAllNumbers();

        //Assert
        expectedNumber.Should().BeEquivalentTo(actualNums);
    }

    [Fact]
    public void ExtractAllNumbers_ShouldGetNone_WhenNoneArePresent()
    {
        //Arrange
        var str = "showing pages!";
        //Act 
        var expectedNumber = new long[] {};
        var actualNums = str.ExtractAllNumbers();

        //Assert
        expectedNumber.Should().BeEquivalentTo(actualNums);
    }
}