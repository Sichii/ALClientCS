using System;
using System.IO;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Client;
using AL.Data;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace AL.Tests
{
    [TestClass]
    public class AssemblyInit
    {
        [AssemblyInitialize]
        public static async Task Init(TestContext context)
        {
            if (!Directory.Exists("images"))
                Directory.CreateDirectory("images");

            ALClientSettings.UseDefaultLoggingConfiguration();
            ALClientSettings.SetLogLevel(LogLevel.Debug);
        }
    }
}