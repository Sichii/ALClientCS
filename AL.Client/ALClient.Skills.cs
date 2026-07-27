#region
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

public abstract partial class ALClient
{
    /// <summary>
    ///     Uses a skill that sends a single projectile at a target, and returns that projectile.
    /// </summary>
    /// <param name="skillName">
    ///     The name of the skill as the server knows it.
    /// </param>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <param name="completion">
    ///     The signal to await. Defaults to the projectile itself, which is how
    ///     <c>
    ///         commence_attack
    ///     </c>
    ///     answers.
    /// </param>
    /// <param name="extraFailure">
    ///     Inspects each response for a failure specific to this skill, returning the reason or
    ///     <c>
    ///         null
    ///     </c>
    ///     .
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the item this skill consumes, if it consumes one.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     The projectile this skill produced.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '{skillName}' on {targetId}. ({reason})
    /// </exception>
    protected async Task<ActionData> UseProjectileSkillAsync(
        string skillName,
        string targetId,
        SkillCompletion? completion = null,
        Func<GameResponseData, string?>? extraFailure = null,
        int? inventorySlot = null)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var actions = await UseSkillCoreAsync(
            skillName,
            targetId,
            completion ?? SkillCompletion.Action,
            extraFailure,
            true,
            inventorySlot);

        if (actions.Count == 0)
            throw new InvalidOperationException($"The server acknowledged '{skillName}' on {targetId} but sent no projectile.");

        return actions[0];
    }

    /// <summary>
    ///     Uses a skill by name, for the skills with no dedicated method of their own.
    /// </summary>
    /// <param name="skillName">
    ///     The name of the skill as the server knows it.
    /// </param>
    /// <param name="targetId">
    ///     The id of the entity to use the skill on, for skills that take one.
    /// </param>
    /// <remarks>
    ///     This awaits the contract the majority of skills use. A skill the server answers in some other way will throw
    ///     <see cref="TimeoutException" /> even though the cast landed — prefer the dedicated method where one exists.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     skillName
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '{skillName}'. ({reason})
    /// </exception>
    /// <exception cref="TimeoutException">
    ///     The server never acknowledged the skill.
    /// </exception>
    public Task UseSkillAsync(string skillName, string? targetId = null) => UseSkillCoreAsync(skillName, targetId);

    /// <summary>
    ///     Emits a skill and waits for the server to acknowledge it.
    /// </summary>
    /// <param name="skillName">
    ///     The name of the skill as the server knows it, e.g. <c>hardshell</c>.
    /// </param>
    /// <param name="targetId">
    ///     The id of the entity to use the skill on, for skills that take one.
    /// </param>
    /// <param name="completion">
    ///     The signal to await. Defaults to whatever <see cref="SkillCompletion.ForSkill" /> derives from G.
    /// </param>
    /// <param name="extraFailure">
    ///     Inspects each response for a failure specific to this skill, returning the reason or <c>null</c>. The four
    ///     failures every skill shares are handled already.
    /// </param>
    /// <param name="collectActions">
    ///     Whether to collect the projectiles this skill produces.
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the item this skill consumes. Left unset, the server takes the last matching item in the
    ///     inventory, which may well be one you meant to keep.
    /// </param>
    /// <param name="payload">
    ///     The complete emit body, for the few skills whose arguments are not <c>name</c>, <c>id</c> and <c>num</c>. Must
    ///     carry <c>name</c> itself.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" /> of <see cref="ActionData" />
    ///     <br />
    ///     The projectiles produced, or empty when <paramref name="collectActions" /> is <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     skillName
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '{skillName}'. ({reason})
    /// </exception>
    /// <exception cref="TimeoutException">
    ///     The server never acknowledged the skill.
    /// </exception>
    protected async Task<List<ActionData>> UseSkillCoreAsync(
        string skillName,
        string? targetId = null,
        SkillCompletion? completion = null,
        Func<GameResponseData, string?>? extraFailure = null,
        bool collectActions = false,
        int? inventorySlot = null,
        object? payload = null)
    {
        if (string.IsNullOrEmpty(skillName))
            throw new ArgumentNullException(nameof(skillName));

        //an empty target is the same as none, and collapsing it here keeps every later check a plain null test
        if (string.IsNullOrEmpty(targetId))
            targetId = null;

        var strategy = completion ?? SkillCompletion.ForSkill(skillName);
        var actions = new List<ActionData>();

        var emitPayload = payload
                          ?? (targetId, inventorySlot) switch
                          {
                              (null, null) => new
                              {
                                  name = skillName
                              },
                              (not null, null) => (object)new
                              {
                                  name = skillName,
                                  id = targetId
                              },
                              (null, not null) => new
                              {
                                  name = skillName,
                                  num = inventorySlot
                              },
                              _ => new
                              {
                                  name = skillName,
                                  id = targetId,
                                  num = inventorySlot
                              }
                          };

        //nothing comes back for these, so there is no point building the subscriptions
        if (strategy.Kind == SkillCompletionKind.Immediate)
        {
            await Socket.EmitAsync(ALSocketEmitType.Skill, emitPayload);

            return actions;
        }

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        //sampled before the emit, because the condition strategy proves the cast by the presence changing either way
        var conditionWasPresent = (strategy.Kind == SkillCompletionKind.Condition)
                                  && Character.Conditions.ContainsKey(strategy.RequiredCondition);

        var failurePrefix = targetId == null ? $"Failed to use '{skillName}'." : $"Failed to use '{skillName}' on {targetId}.";

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //a replay of an already-settled operation must not complete this await
                if (data.Stale)
                    return TaskCache.FALSE;

                var extraReason = extraFailure?.Invoke(data);

                if (extraReason != null)
                    return Task.FromResult(source.TrySetResult($"{failurePrefix} ({extraReason})"));

                var result = data.ResponseType switch
                {
                    //attack_failed carries only an id, never a place, so the target is the only discriminator there is
                    GameResponseType.AttackFailed when (targetId != null) && targetId.EqualsI(data.TargetId!) => source.TrySetResult(
                        $"{failurePrefix} (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"{failurePrefix} (disabled)"),
                    GameResponseType.Cooldown when skillName.EqualsI(data.Place!) => source.TrySetResult($"{failurePrefix} (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"{failurePrefix} (level too low)"),
                    GameResponseType.NoMP => source.TrySetResult($"{failurePrefix} (no mp)"),
                    GameResponseType.TooFar when (targetId != null) && targetId.EqualsI(data.TargetId!) && skillName.EqualsI(data.Place!) =>
                        source.TrySetResult($"{failurePrefix} (too far)"),

                    //the server collapses some volleys into one frame that carries no "success"
                    GameResponseType.Data when (strategy.Kind == SkillCompletionKind.ResponseData)
                                               && !data.Failed
                                               && skillName.EqualsI(data.Place!) => source.TrySetResult(Expectation.Success),

                    _ when data.Failed && skillName.EqualsI(data.Place!) => source.TrySetResult(
                        $"{failurePrefix} ({data.Reason ?? data.ResponseType.ToString()})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

        using var actionCallback = collectActions || (strategy.Kind == SkillCompletionKind.Action)
            ? Socket.On<ActionData>(
                ALSocketMessageType.Action,
                data =>
                {
                    //a few skills name themselves in "type" rather than "source"
                    if (!data.AttackerId.EqualsI(Character.Id) || !skillName.EqualsI((data.Source ?? data.Type)!))
                        return TaskCache.FALSE;

                    //a targeted skill produces one projectile, and filtering keeps a concurrent cast from stealing it
                    if ((targetId != null) && !targetId.EqualsI(data.Target))
                        return TaskCache.FALSE;

                    actions.Add(data);

                    //a commence_attack skill is acknowledged by the projectile and nothing else
                    if (strategy.Kind == SkillCompletionKind.Action)
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.TRUE;
                })
            : null;

        using var skillTimeoutCallback = strategy.Kind == SkillCompletionKind.Timeout
            ? Socket.On<SkillTimeoutData>(
                ALSocketMessageType.SkillTimeout,
                data =>
                {
                    //calculate_player_stats pushes an attack_ms correction through this same event; it acknowledges no cast
                    if (string.IsNullOrEmpty(data.Reason)
                        && SkillCompletion.ResolveTimeoutName(skillName)
                                          .EqualsI(data.SkillName))
                        source.TrySetResult(Expectation.Success);

                    //never swallow the frame - the standing subscriber behind this one is what records the cooldown
                    return TaskCache.FALSE;
                })
            : null;

        using var characterCallback = strategy.Kind == SkillCompletionKind.Condition
            ? Socket.On<CharacterData>(
                ALSocketMessageType.Character,
                data =>
                {
                    //a toggle like mshield flips the condition off, so the transition matters rather than the value
                    if (data.Conditions.ContainsKey(strategy.RequiredCondition) != conditionWasPresent)
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.FALSE;
                })
            : null;

        await Socket.EmitAsync(ALSocketEmitType.Skill, emitPayload);

        var expectation = await source.Task.WithNetworkTimeout(skillName);
        expectation.ThrowIfUnsuccessful();

        return actions;
    }

    #region Item skills
    //These are not class skills - the server gates them on an equipped or consumed item, so any character can use one.

    /// <summary>
    ///     Asynchronously uses Charm on a target, requiring an equipped charmer.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'charm' on {targetId}. ({reason})
    /// </exception>
    public Task CharmAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("charm", targetId);
    }

    /// <summary>
    ///     Asynchronously uses Power, requiring an equipped powerglove.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'power'. ({reason})
    /// </exception>
    public Task PowerAsync() => UseSkillCoreAsync("power");

    /// <summary>
    ///     Asynchronously uses Scare, breaking the aggro of every monster targeting you.
    /// </summary>
    /// <remarks>
    ///     Requires an equipped jack-o-lantern. Monsters flagged immune shrug it off and are reported separately.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'scare'. ({reason})
    /// </exception>
    public Task ScareAsync() => UseSkillCoreAsync("scare");

    /// <summary>
    ///     Asynchronously throws a Snowball at a target.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the snowball to throw. Left unset, the server picks the last one in your inventory.
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
    ///     Failed to use 'snowball' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> SnowballAsync(string targetId, int? inventorySlot = null)
        => UseProjectileSkillAsync("snowball", targetId, inventorySlot: inventorySlot);

    /// <summary>
    ///     Asynchronously uses Tangle on a target, rooting it in place. Requires an equipped heartwood.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'tangle' on {targetId}. ({reason})
    /// </exception>
    public Task TangleAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("tangle", targetId);
    }

    /// <summary>
    ///     Asynchronously uses Warp, jumping to any point on a map. Requires an equipped warpvest.
    /// </summary>
    /// <param name="x">
    ///     The x coordinate to warp to.
    /// </param>
    /// <param name="y">
    ///     The y coordinate to warp to.
    /// </param>
    /// <param name="instance">
    ///     The instance to warp into. Defaults to the one you are already in — the server would otherwise send you to
    ///     main.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'warp'. ({reason})
    /// </exception>
    public Task WarpAsync(float x, float y, string? instance = null)
        => UseSkillCoreAsync(
            "warp",
            payload: new
            {
                name = "warp",
                x,
                y,
                @in = instance ?? Character.In
            });

    /// <summary>
    ///     Asynchronously uses XPower, requiring an equipped golden powerglove.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'xpower'. ({reason})
    /// </exception>
    public Task XPowerAsync() => UseSkillCoreAsync("xpower");

    /// <summary>
    ///     Asynchronously uses ZapperZap on a target, requiring an equipped zapper ring.
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
    ///     Failed to use 'zapperzap' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> ZapperZapAsync(string targetId) => UseProjectileSkillAsync("zapperzap", targetId);
    #endregion
}