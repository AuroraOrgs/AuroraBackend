using Aurora.Shared.Models;
using System.Text.Json;

namespace Aurora.Shared.Tests.Models;

public class OneOfConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters =
        {
            new OneOfJsonConverterFactory()
        }
    };

    [Fact]
    public void Converter_ShouldSerializeIntoOnlyOne_WhenOneOfIsProvided()
    {
        //Arrange
        var value = new OneOf<string, int>(69);
        //Act 
        var json = JsonSerializer.Serialize(value, _options);
        //Assert
        json.Should().BeEquivalentTo("69");
    }

    [Fact]
    public void Converter_ShouldDesirializeIntoOneOf_WhenOnlyOneIsProvided()
    {
        //Arrange
        var json = "\"something went wrong when getting a person\"";
        var json2 = "{\"FirstName\":\"John\", \"LastName\":\"Doe\"}";
        //Act 
        var actual = JsonSerializer.Deserialize<OneOf<Person, string>>(json, _options);
        var actual2 = JsonSerializer.Deserialize<OneOf<Person, string>>(json2, _options);
        //Assert
        actual!.Match(
            x => x.Should().NotBeCalled(),
            x => x.Should().Be("something went wrong when getting a person")
            );
        actual2!.Match(
           x => x.Should().BeEquivalentTo(new Person("John", "Doe")),
           x => x.Should().NotBeCalled()
           );
    }

    private record Person(string FirstName, string LastName);
}
