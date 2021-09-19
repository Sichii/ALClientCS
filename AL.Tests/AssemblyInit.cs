using System.IO;
using AL.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace AL.Tests
{
    [TestClass]
    public class AssemblyInit
    {
        public const string IMAGE_DIR = "images";
        public const string PATH_IMAGES_DIR = @"images\path";

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            if (!Directory.Exists(IMAGE_DIR))
                Directory.CreateDirectory(IMAGE_DIR);

            if (!Directory.Exists(PATH_IMAGES_DIR))
                Directory.CreateDirectory(PATH_IMAGES_DIR);

            ALClientSettings.UseDefaultLoggingConfiguration();
            ALClientSettings.SetLogLevel(LogLevel.Debug);
        }
    }
}