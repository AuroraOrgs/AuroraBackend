using Aurora.Domain.Enums;
using Aurora.Scrapers.Extensions;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Scrapers.Tests.Extensions;

public class SupportedWebsiteExtensionsTest
{
    [Fact]
    public void GetWebsite_ShouldWork_ForAllWebsites()
    {
        //Arrange
        var websites = Enum.GetValues<SupportedWebsite>();
        var tests = new List<Action>();
        //Act
        foreach (var website in websites)
        {
            tests.Add(() =>
            {
                website.GetBaseUrl();
            });
        }
        //Assert
        foreach (var test in tests)
        {
            test.Should().NotThrow();
        }
    }
}
