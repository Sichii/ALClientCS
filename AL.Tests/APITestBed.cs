#region
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL.APIClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests;

[TestClass]
public abstract class APITestBed
{
    protected static readonly SemaphoreSlim Sync = new(1, 1);
    protected static ALAPIClient APIClient { get; private set; } = null!;

    [TestInitialize]
    public virtual async Task Init()
    {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        await Sync.WaitAsync();

        try
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (APIClient == null)
            {
                var lines = await File.ReadAllLinesAsync("TestCredentials.txt");

                if (lines.Length < 2)
                    throw new Exception("Put login info in TestCredentials.txt to perform tests.");

                var email = lines[0];
                var pw = lines[1];

                APIClient = await ALAPIClient.LoginAsync(email, pw);
            }
        } finally
        {
            Sync.Release();
        }
    }
}