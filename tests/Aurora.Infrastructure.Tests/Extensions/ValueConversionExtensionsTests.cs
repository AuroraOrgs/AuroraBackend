using Aurora.Infrastructure.Extensions;
using Aurora.Infrastructure.Tests.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Infrastructure.Tests.Extensions;

public class ValueConversionExtensionsTests
{
    [Fact]
    public void Parse_ShouldGetNull_WhenEmptyStringIsProvided()
    {
        //Arrange
        var str = "";

        //Act 
        var obj = str.ParseNullableJson();

        //Assert
        Assert.Null(obj);
    }

    [Fact]
    public void GetData_ShouldPreserveType_WhenChildTypeIsUsed()
    {
        //Arrange
        var obj = new TestResultData(69);

        //Act 
        JsonSerializerSettings jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        var converter = JsonSerializer.Create(jsonSettings);
        var jobj = JObject.FromObject(obj, converter);
        var str = jobj.ConvertToString();
        var jobj2 = str.ParseNullableJson();
        var result = jobj2.GetData();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<TestResultData>(result);
    }
}