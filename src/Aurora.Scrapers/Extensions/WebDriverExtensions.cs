using Aurora.Shared.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Aurora.Scrapers.Extensions
{
    public static class WebDriverExtensions
    {
        public static async Task ScrollToTheBottomOfThePage(this IJavaScriptExecutor executor)
        {
            long intialLength = (long)executor.ExecuteScript("return document.body.scrollHeight");
            while (true)
            {
                executor.ExecuteScript("window.scrollTo(0,document.body.scrollHeight)");
                await Task.Delay(500);

                long currentLength = (long)executor.ExecuteScript("return document.body.scrollHeight");
                if (intialLength == currentLength)
                {
                    break;
                }
                intialLength = currentLength;
            }
        }

        public static bool PageContainsElement(this ISearchContext context, By by)
        {
            bool result = true;
            try
            {
                context.FindElement(by);
            }
            catch (Exception exc)
            {
                if (IsExpected(exc))
                {
                    result = false;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        public static IWebElement FindElementOrNull(this ISearchContext context, By by)
        {
            IWebElement result;
            try
            {
                result = context.FindElement(by);
            }
            catch (Exception exc)
            {
                if (IsExpected(exc))
                {
                    result = null;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        private static bool IsExpected(Exception exc)
        {
            return exc is NoSuchElementException || exc is ElementNotInteractableException;
        }

        public static void Type(this IWebDriver driver, string text)
        {
            Actions actionProvider = new Actions(driver);
            actionProvider.SendKeys(text).Build().Perform();
        }

        public static async Task SaveScreenshot(this ITakesScreenshot driver, string folderLocation)
        {
            var screen = driver.GetScreenshot();
            var fileName = Path.GetRandomFileName();
            var fullFileLocation = Path.Combine(folderLocation, fileName);
            var properLocation = Path.ChangeExtension(fullFileLocation, "png");
            var file = screen.AsByteArray;
            await File.WriteAllBytesAsync(properLocation, file);
        }

        public static void FindAndClickIfExists(this ISearchContext context, By by)
        {
            var element = context.FindElementOrNull(by);
            if (element.IsNotNull())
            {
                try
                {
                    element.Click();
                }
                catch (ElementNotInteractableException)
                {
                    //ignore
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void ExecuteScripts(this IJavaScriptExecutor executor, params string[] scripts)
        {
            foreach (var script in scripts)
            {
                executor.ExecuteScript(script);
            }
        }

        public static void ClickProgrammatically(this IJavaScriptExecutor executor, IWebElement element)
        {
            executor.ExecuteScript("arguments[0].click();", element);
        }
    }
}
