#region
using AL.Client;
using NLog;
#endregion

namespace AL.Tests;

public class AssemblyInit
{
    [Before(Assembly)]
    public static void Init()
    {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        ALClientSettings.UseDefaultLoggingConfiguration();
        ALClientSettings.SetLogLevel(LogLevel.Debug);
    }
}