using System;
using AL.Core.Abstractions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents a buff or debuff.
    /// </summary>
    /// <seealso cref="AttributedObjectBase" />
    [JsonConverter(typeof(AttributedObjectConverter<Condition>))]
    public record Condition : AttributedRecordBase, IPingCompensated
    {
        /// <summary>
        ///     TODO: something related to monsterhunt
        /// </summary>
        [JsonProperty]
        public bool DL { get; init; }

        /// <summary>
        ///     If populated, <br />
        ///     this could be the name of the monster you need to kill for
        ///     <see cref="AL.Core.Definitions.Condition.MonsterHunt" />
        ///     <b>OR</b> the ID of a coop boss this player is fighting.
        /// </summary>
        [JsonProperty("id")]
        public string? Id { get; init; }

        /// <summary>
        ///     The intensity of the <see cref="AL.Core.Definitions.Condition.Burned" /> condition.
        /// </summary>
        [JsonProperty]
        public float Intensity { get; init; }
        public bool IsCompensated { get; private set; }
        /// <summary>
        ///     Whether or not this condition is from a monster ability.
        /// </summary>
        [JsonProperty("ability")]
        public bool IsMonsterAbility { get; init; }

        /// <summary>
        ///     If populated, this is the proportion of contribution this character has made towards a coop boss. <br />
        ///     See <see cref="Id" /> for the ID of the boss.
        /// </summary>
        [JsonProperty("p")]
        public float? Proportion { get; init; }

        /// <summary>
        ///     The remaining number of monsters you need to kill to complete the
        ///     <see cref="AL.Core.Definitions.Condition.MonsterHunt" />.
        /// </summary>
        [JsonProperty("c")]
        public float RemainingMonsters { get; init; }

        /// <summary>
        ///     How long before this condition expires in milliseconds.
        /// </summary>
        [JsonProperty("ms")]
        public float RemainingMS { get; private set; }

        /// <summary>
        ///     If populated, the id of the server for this <see cref="AL.Core.Definitions.Condition.MonsterHunt" />.
        /// </summary>
        [JsonProperty("sn")]
        public string? ServerKey { get; init; }

        /// <summary>
        ///     If populated, the Id of the merchant who cast this <see cref="AL.Core.Definitions.Condition.MLuck" />.
        /// </summary>
        [JsonProperty("f")]
        public string? SourceId { get; init; }

        /// <summary>
        ///     If true, this <see cref="AL.Core.Definitions.Condition.MLuck" /> was cast by a merchant owned by that user. <br />
        ///     If false, this <see cref="AL.Core.Definitions.Condition.MLuck" /> can be overwritten by any other merchant.
        /// </summary>
        [JsonProperty]
        public bool Strong { get; init; }

        string IMutable.Id => string.Empty;

        public void CompensateOnce(int minimumOffsetMS)
        {
            if (IsCompensated)
                throw new InvalidOperationException("Object already compensated.");

            IsCompensated = true;
            RemainingMS -= minimumOffsetMS;
        }
    }
}