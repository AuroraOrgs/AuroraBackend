using Aurora.Shared.Tests.Base;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Shared.Tests.Dependencies;

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
        var obj = new Child(69, 420);
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        //Act 
        var json = JsonConvert.SerializeObject(obj, settings);
        var result = JsonConvert.DeserializeObject<Parent>(json, settings);

        //Assert
        result.Should().BeAssignableTo<Child>();
    }

    [Fact]
    public void WhenSerializingNull_ShouldReturnNullString()
    {
        //Arrange
        JObject? obj = null;
        //Act 
        var str = JsonConvert.SerializeObject(obj);
        //Assert
        str.Should().BeEquivalentTo("null");
    }
}
