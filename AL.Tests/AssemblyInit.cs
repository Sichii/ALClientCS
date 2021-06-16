using System;
using System.IO;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Data;
using ALClientCS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace AL.Tests
{
    [TestClass]
    public class AssemblyInit
    {
        public static ALAPIClient APIClient { get; set; }
        
        [AssemblyInitialize]
        public static async Task Init(TestContext context)
        {
            if (!Directory.Exists("images"))
                Directory.CreateDirectory("images");
            
            ALClientSettings.UseDefaultLoggingConfiguration();
            ALClientSettings.SetLogLevel(LogLevel.Trace);

            var lines = await File.ReadAllLinesAsync("TestCredentials.txt");

            if (lines.Length < 2)
                throw new Exception("Put login info in TestCredentials.txt to perform tests.");
            
            var email = lines[0];
            var pw = lines[1];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw))
                throw new Exception("Put in login info to perform tests");

            APIClient = await ALAPIClient.LoginAsync(email, pw);

            await GameData.PopulateAsync();
        }
    }
}