using Aurora.Application.Models;
using Aurora.Infrastructure.Tests.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Tests.Dependencies;

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

    [Fact]
    public void WhenConvertingWithTypeNameHandling_ShouldPreserveChildType()
    {
        //Arrange
        var obj = new TestResultData(69);
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        //Act 
        var json = JsonConvert.SerializeObject(obj, settings);
        var result = JsonConvert.DeserializeObject<SearchResultData>(json, settings);

        //Assert
        Assert.IsType<TestResultData>(result);
    }

    [Fact]
    public void WhenSerializingNull_ShouldReturnNullString()
    {
        //Arrange
        JObject? obj = null;
        //Act 
        var str = JsonConvert.SerializeObject(obj);
        //Assert
        Assert.Equal("null", str);
    }
}
