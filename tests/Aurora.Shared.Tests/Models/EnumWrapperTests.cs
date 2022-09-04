using Aurora.Shared.Models;

namespace Aurora.Shared.Tests.Models;

public class EnumWrapperTests
{
    [Fact]
    public void Create_ShouldCreateValue_WhenSupportedValue()
    {
        //Arrange
        var enumValue = SearchOption.TopDirectoryOnly;
        //Act 
        var wrapped = EnumWrapper<SearchOption>.Create(enumValue);
        //Assert
        wrapped.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldCreateError_WhenNotSupportedValue()
    {
        //Arrange
        var enumValue = (SearchOption)420;
        //Act 
        var wrapped = EnumWrapper<SearchOption>.Create(enumValue);
        //Assert
        wrapped.HasValue.Should().BeFalse();
    }
}