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
            string seleniumLocation = _options.SeleniumLocation;
            Uri uri = new Uri(seleniumLocation);
            var options = new FirefoxOptions();
            RemoteWebDriver driver = new RemoteWebDriver(uri, options.ToCapabilities());
            return Task.FromResult(driver);
        }
    }
}
