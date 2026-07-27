#region
using AL.APIClient;
#endregion

namespace AL.Tests;

public abstract class APITestBed
{
    protected static readonly SemaphoreSlim Sync = new(1, 1);
    protected static AlApiClient APIClient { get; private set; } = null!;

    //each level of the test-bed chain declares its own hook rather than overriding: TUnit collects them
    //base-first through the hierarchy, and an override carrying the same attribute is error TUnit0074
    [Before(Test)]
    public async Task LogInToApiAsync()
    {
        await Sync.WaitAsync();

        try
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse, ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (APIClient == null)
            {
                var lines = await File.ReadAllLinesAsync("TestCredentials.txt");

                if (lines.Length < 2)
                    throw new Exception("Put login info in TestCredentials.txt to perform tests.");

                var email = lines[0];
                var pw = lines[1];

                APIClient = await AlApiClient.LoginAsync(email, pw);
            }
        } finally
        {
            Sync.Release();
        }
    }
}