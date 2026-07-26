#region
using System;
using System.IO;
using AL.Client;
using NLog;
#endregion

namespace AL.Tests;

public class AssemblyInit
{
    public const string IMAGE_DIR = "images";
    public const string PATH_IMAGES_DIR = @"images\path";

    [Before(Assembly)]
    public static void Init()
    {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        if (!Directory.Exists(IMAGE_DIR))
            Directory.CreateDirectory(IMAGE_DIR);

        if (!Directory.Exists(PATH_IMAGES_DIR))
            Directory.CreateDirectory(PATH_IMAGES_DIR);

        ALClientSettings.UseDefaultLoggingConfiguration();
        ALClientSettings.SetLogLevel(LogLevel.Debug);
    }
}