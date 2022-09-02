using Aurora.Shared.Extensions;

namespace Aurora.Shared.Tests.Extensions;

public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(List<>), typeof(List<>))]
    [InlineData(typeof(List<>), typeof(List<int>))]
    [InlineData(typeof(List<>), typeof(ListInheritor<>))]
    [InlineData(typeof(List<>), typeof(ListInheritor<int>))]
    [InlineData(typeof(List<>), typeof(ListInheritorImpl))]
    public void IsAssignableToGenericType_ShouldBeAssignable_WhenInherits(Type genericType, Type givenType) =>
        givenType.IsAssignableToGenericType(genericType).Should().BeTrue();

    private class ListInheritor<T> : List<T>
    {

    }

    private class ListInheritorImpl : ListInheritor<int>
    {

    }

    [Theory]
    [InlineData(typeof(List<>), typeof(IEnumerable<>))]
    [InlineData(typeof(List<>), typeof(IEnumerable<int>))]
    [InlineData(typeof(List<>), typeof(HashSet<>))]
    public void IsAssignableToGenericType_ShouldNotBeAssignable_WhenDifferentTypes(Type genericType, Type givenType) =>
        givenType.IsAssignableToGenericType(genericType).Should().BeFalse();
}
