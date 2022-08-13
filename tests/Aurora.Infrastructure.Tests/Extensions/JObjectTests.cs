using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Tests.Extensions;

public class JObjectTests
{
    [Fact]
    public void Parse_WhenEmptyString_FailsToParse()
    {
        //Arrange
        var str = "";

        //Act 
        var test = () => JObject.Parse(str);

        //Assert
        Assert.Throws<JsonReaderException>(test);
    }
}
