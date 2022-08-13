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
        var obj = new Child(69, 420);
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        //Act 
        var json = JsonConvert.SerializeObject(obj, settings);
        var result = JsonConvert.DeserializeObject<Parent>(json, settings);

        //Assert
        Assert.IsType<Child>(result);
    }

    [Fact]
    public void WhenConvertingWithoutTypeNameHandling_ShouldNotPreserveChildType()
    {
        //Arrange
        var obj = new Child(69, 420);

        //Act 
        var json = JsonConvert.SerializeObject(obj);
        var result = JsonConvert.DeserializeObject<Parent>(json);

        //Assert
        Assert.IsType<Parent>(result);
    }

    private class Parent
    {
        public Parent(int parentProp)
        {
            ParentProp = parentProp;
        }

        public int ParentProp { get; }
    }

    private class Child : Parent
    {
        public Child(int childProp, int parentProp) : base(parentProp)
        {
            ChildProp = childProp;
        }

        public int ChildProp { get; set; }
    }
}
