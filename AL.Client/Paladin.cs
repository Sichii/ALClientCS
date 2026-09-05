#region
using System.Net;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Helpers;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains paladin specific functionality.
/// </summary>
public class Paladin : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Paladin" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the paladin.
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
    public Paladin(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously toggles Aether Shield, which lets magical damage through to health but restores mana from
    ///     the health lost. It replaces Mana Shield: the two cannot be worn together.
    /// </summary>
    /// <remarks>
    ///     A toggle sharing Mana Shield's zero cooldown, so the call completes on the shield condition changing —
    ///     whichever way it went. Needs level 60.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'aether_shield'. ({reason})
    /// </exception>
    public Task AetherShieldAsync() => UseSkillCoreAsync("aether_shield", completion: SkillCompletion.OnCondition(Condition.AetherShield));

    /// <summary>
    ///     Asynchronously uses Beacon of Resolve, giving every friendly player within 480 15 fortitude and a point of
    ///     each courage for 8 seconds.
    /// </summary>
    /// <remarks>
    ///     640 mana on a 60 second cooldown, needs level 70. No target: the server picks the audience.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'beacon_of_resolve'. ({reason})
    /// </exception>
    public Task BeaconOfResolveAsync() => UseSkillCoreAsync("beacon_of_resolve");

    /// <summary>
    ///     Asynchronously uses Cleansing Light on an ally, lifting every harmful combat condition from them.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the ally. Never the paladin itself: the skill refuses self.
    /// </param>
    /// <remarks>
    ///     320 mana on a 24 second cooldown at a fixed 240 range, needs level 30. Which conditions it lifts is
    ///     <see cref="AL.Data.Conditions.GCondition.Cleansable" />.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'cleansing_light' on {targetId}. ({reason})
    /// </exception>
    public Task CleansingLightAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("cleansing_light", targetId);
    }

    /// <summary>
    ///     Asynchronously uses Guardian's Oath on an ally, taking 35% of their damage for 8 seconds and restoring
    ///     mana from the health this paladin loses to it.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the ally. Never the paladin itself: the skill refuses self.
    /// </param>
    /// <remarks>
    ///     320 mana on a 24 second cooldown at a fixed 240 range, needs level 50. The link holds to 360.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'guardians_oath' on {targetId}. ({reason})
    /// </exception>
    public Task GuardiansOathAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("guardians_oath", targetId);
    }

    /// <summary>
    ///     Asynchronously toggles MShield, trading damage for damage reduction.
    /// </summary>
    /// <remarks>
    ///     This is a toggle and the server sends no cooldown for it, so the call completes on the shield condition
    ///     changing — whichever way it went.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'mshield'. ({reason})
    /// </exception>
    public Task MShieldAsync() => UseSkillCoreAsync("mshield", completion: SkillCompletion.OnCondition(Condition.MShield));

    /// <summary>
    ///     Asynchronously sets the paladin's aura to the given form. One aura is carried at a time and strengthens
    ///     every friendly within 320; casting again with another form changes it.
    /// </summary>
    /// <param name="form">
    ///     The form to carry.
    /// </param>
    /// <remarks>
    ///     Free, on a 500ms cooldown, needs level 60. The official client sends the form where a target id would
    ///     go, and so does this.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'paladin_aura'. ({reason})
    /// </exception>
    public Task PaladinAuraAsync(PaladinAuraForm form)
        => UseSkillCoreAsync(
            "paladin_aura",
            payload: new
            {
                name = "paladin_aura",
                id = form.ToString()
                         .ToLowerInvariant()
            });

    /// <summary>
    ///     Asynchronously uses Purify on a target.
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
    ///     Failed to use 'purify' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> PurifyAsync(string targetId) => UseProjectileSkillAsync("purify", targetId);

    /// <summary>
    ///     Asynchronously uses SelfHeal, healing yourself.
    /// </summary>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'selfheal'. ({reason})
    /// </exception>
    public async Task<ActionData> SelfHealAsync()
    {
        //the server heals the paladin by attacking them, so this answers with a projectile aimed at yourself
        var actions = await UseSkillCoreAsync("selfheal", completion: SkillCompletion.Action);

        if (actions.Count == 0)
            throw new InvalidOperationException("The server acknowledged 'selfheal' but sent no projectile.");

        return actions[0];
    }

    /// <summary>
    ///     Asynchronously uses Shield Slam on a target: physical damage of three times attack plus twelve times
    ///     armour, the armour counted up to 1000. It pierces immunity, never crits and triggers no item effect.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <remarks>
    ///     2000 mana on a 600ms cooldown, needs level 60 and a shield in the offhand.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'shield_slam' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> ShieldSlamAsync(string targetId) => UseProjectileSkillAsync("shield_slam", targetId);

    /// <summary>
    ///     Asynchronously uses Smash on a target.
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
    ///     Failed to use 'smash' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> SmashAsync(string targetId) => UseProjectileSkillAsync("smash", targetId);

    /// <summary>
    ///     Asynchronously creates a Paladin client and connects.
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
    /// <param name="proxy">
    ///     The proxy to reach the game through, or null for the machine's own connection.
    /// </param>
    /// <returns>
    ///     <see cref="Paladin" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Paladin> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient,
        IWebProxy? proxy = null)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Paladin(name, api, socket),
            proxy);
}