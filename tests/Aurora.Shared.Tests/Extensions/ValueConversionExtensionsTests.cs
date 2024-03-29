using Aurora.Shared.Extensions;

namespace Aurora.Shared.Tests.Extensions;

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
        obj.Should().BeNull();
    }

    [Fact]
    public void GetData_ShouldPreserveType_WhenChildTypeIsUsed()
    {
        //Arrange
        var obj = new Child(69, 420);

        //Act
        var jobj = obj.ToJObject();
        var str = jobj.ConvertToString();
        var jobj2 = str.ParseNullableJson();
        var result = jobj2.ToData<Parent>();

        //Assert
        result.Should().NotBeNull().And.BeAssignableTo<Child>().And.BeEquivalentTo(obj);
    }
}