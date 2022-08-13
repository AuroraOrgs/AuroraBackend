using Aurora.Infrastructure.Extensions;

namespace Aurora.Infrastructure.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void ShouldGetNull_WhenEmptyStringIsProvided()
    {
        //Arrange
        var str = "";

        //Act 
        var obj = str.ParseNullableJson();

        //Assert
        Assert.Null(obj);
    }
}