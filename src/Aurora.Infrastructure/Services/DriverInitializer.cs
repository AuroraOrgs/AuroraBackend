using Aurora.Infrastructure.Config;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public class DriverInitializer
    {
        private readonly SeleniumConfig _options;

        public DriverInitializer(IOptions<SeleniumConfig> options)
        {
            _options = options.Value;
        }

        public Task<RemoteWebDriver> Initialize()
        {
            var seleniumLocation = _options.SeleniumLocation;
            var uri = new Uri(seleniumLocation);
            var firefoxOptions = new FirefoxOptions();
            var driver = new RemoteWebDriver(uri, firefoxOptions.ToCapabilities());
            return Task.FromResult(driver);
        }
    }
}
