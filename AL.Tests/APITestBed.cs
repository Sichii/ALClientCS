using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests
{
    [TestClass]
    public abstract class APITestBed
    {
        protected static ALAPIClient APIClient { get; private set; } = null!;
        protected static readonly SemaphoreSlim Sync = new(1, 1);
        
        [TestInitialize]
        public virtual async Task Init()
        {
            await Sync.WaitAsync();

            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (APIClient == null)
                {
                    var lines = await File.ReadAllLinesAsync("TestCredentials.txt").ConfigureAwait(false);

                    if (lines.Length < 2)
                        throw new Exception("Put login info in TestCredentials.txt to perform tests.");

                    var email = lines[0];
                    var pw = lines[1];

                    APIClient = await AL.APIClient.ALAPIClient.LoginAsync(email, pw);
                }
            } finally
            {
                Sync.Release();
            }
        }
    }
}