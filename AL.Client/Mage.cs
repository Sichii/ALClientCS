#region
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains mage specific functionality.
/// </summary>
public class Mage : ALClient
{
    /// <summary>
    ///     The most targets the server reads out of a single cburst. Anything past this is silently dropped.
    /// </summary>
    private const int MAX_CBURST_TARGETS = 16;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Mage" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the mage.
    /// </param>
    /// <param name="apiClient">
    ///     An API client implementation.
    /// </param>
    /// <param name="socketClient">
    ///     A socket client implementation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     name
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     socketClient
    /// </exception>
    public Mage(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses Alchemy, drinking the potion you are standing on.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'alchemy'. ({reason})
    /// </exception>
    public Task AlchemyAsync() => UseSkillCoreAsync("alchemy");

    /// <summary>
    ///     Asynchronously uses Blink, teleporting anywhere on the current map.
    /// </summary>
    /// <param name="x">
    ///     The x coordinate to teleport to.
    /// </param>
    /// <param name="y">
    ///     The y coordinate to teleport to.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'blink'. ({reason})
    /// </exception>
    public Task BlinkAsync(float x, float y)
        => UseSkillCoreAsync(
            "blink",
            payload: new
            {
                name = "blink",
                x,
                y
            });

    /// <summary>
    ///     Asynchronously uses Burst on a target, spending mp for extra damage.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'burst' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> BurstAsync(string targetId) => UseProjectileSkillAsync("burst", targetId);

    /// <summary>
    ///     Asynchronously uses CBurst, bursting several targets at once for a chosen amount of mp each.
    /// </summary>
    /// <param name="targets">
    ///     The targets to burst and the mp to spend on each. The server reads at most
    ///     <see cref="MAX_CBURST_TARGETS" /> of them and ignores a target repeated within the same cast.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targets
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'cburst'. ({reason})
    /// </exception>
    public Task CBurstAsync(IEnumerable<(string TargetId, int Mp)> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);

        var pairs = targets.Take(MAX_CBURST_TARGETS)
                           .Select(static target => new object[]
                           {
                               target.TargetId,
                               target.Mp
                           })
                           .ToArray();

        return UseSkillCoreAsync(
            "cburst",
            payload: new
            {
                name = "cburst",
                targets = pairs
            });
    }

    /// <summary>
    ///     Asynchronously uses Energize on a target, giving them mp and raising their attack speed.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'energize' on {targetId}. ({reason})
    /// </exception>
    public Task EnergizeAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("energize", targetId);
    }

    /// <summary>
    ///     Asynchronously uses Entangle on a target, rooting it in place.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the essence of nature to use. Left unset, the server picks the last one in your inventory.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'entangle' on {targetId}. ({reason})
    /// </exception>
    public Task EntangleAsync(string targetId, int? inventorySlot = null)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("entangle", targetId, inventorySlot: inventorySlot);
    }

    /// <summary>
    ///     Asynchronously uses Light, revealing invisible entities nearby.
    /// </summary>
    /// <remarks>
    ///     Light has no cooldown, so the server acknowledges it with nothing and this returns as soon as the emit is
    ///     written.
    /// </remarks>
    public Task LightAsync() => UseSkillCoreAsync("light", completion: SkillCompletion.Immediate);

    /// <summary>
    ///     Asynchronously uses Magiport on a target, offering to pull them to your location.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <remarks>
    ///     This only sends the offer — the target has to accept it before they are moved. Magiport has no cooldown, so the
    ///     server acknowledges the offer with nothing and this returns as soon as the emit is written.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    public Task MagiportAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("magiport", targetId, SkillCompletion.Immediate);
    }

    /// <summary>
    ///     Asynchronously uses Reflection on a target, making them reflect some of the damage they take.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'reflection' on {targetId}. ({reason})
    /// </exception>
    public Task ReflectionAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("reflection", targetId);
    }

    /// <summary>
    ///     Asynchronously creates a Mage client and connects.
    ///     <br />
    /// </summary>
    /// <param name="characterName">
    ///     The name of the character to log in as.
    /// </param>
    /// <param name="region">
    ///     The region to log into.
    /// </param>
    /// <param name="identifier">
    ///     The identifier suffic for the region.
    /// </param>
    /// <param name="apiClient">
    ///     An <see cref="IAlApiClient" /> with your authorization credentials.
    /// </param>
    /// <returns>
    ///     <see cref="Mage" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Mage> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Mage(name, api, socket));
}