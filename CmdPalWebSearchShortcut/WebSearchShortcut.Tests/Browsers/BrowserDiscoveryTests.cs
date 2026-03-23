using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSearchShortcut.Browser;

namespace WebSearchShortcut.Tests.Helpers
{
    [TestClass]
    public class BrowserProgIdFinderTests
    {
        [TestMethod]
        public void FindUniqueHttpUrlAssociationProgIdsShouldPrintResults()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            BrowsersDiscovery.Logger = loggerFactory.CreateLogger("Test");

            var browserInfos = BrowsersDiscovery.GetInstalledBrowsers();

            foreach (var browserInfo in browserInfos)
            {
                Console.WriteLine($"Found browser: {browserInfo.Name}({browserInfo.Id}) - {browserInfo.Path} {browserInfo.ArgumentsPattern}");
            }

            Console.WriteLine($"Total browsers: {browserInfos.Count}");
        }
    }
}
