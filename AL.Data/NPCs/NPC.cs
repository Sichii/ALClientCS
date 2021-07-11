using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;

namespace AL.Data.NPCs
{
    /// <summary>
    ///     <inheritdoc cref="AttributedRecordBase" /> <br />
    ///     Represents the static data for an NPC.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record NPC : AttributedRecordBase
    {
        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this NPC has an aura, and that aura gives these attributes.
        /// </summary>
        public IReadOnlyDictionary<ALAttribute, float>? Aura { get; init; }

        /// <summary>
        ///     TODO: Unknown, probably GUI related if populated.
        /// </summary>
        public string? Color { get; init; }

        /// <summary>
        ///     The id of the monster. (seems to always be the same as the accessor)
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     If this is true, this is bad/old data that should be ignored.
        /// </summary>
        public bool Ignore { get; init; }

        /// <summary>
        ///     TODO: Unknown
        /// </summary>
        public float Interval { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated this NPC sells items, and these are the items this NPC sells.
        /// </summary>
        public IReadOnlyList<string>? Items { get; init; }

        /// <summary>
        ///     The level of this NPC.
        /// </summary>
        public float Level { get; init; }

        /// <summary>
        ///     The name of this NPC as seen on the GUI.
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     If this is a bank NPC, this is their bank pack number.
        /// </summary>
        public BankPack Pack { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this NPC is a transporter,
        ///     and this dictionary contains the places (mapName : spawnId) that this NPC can take you.
        /// </summary>
        public IReadOnlyDictionary<string, int>? Places { get; init; }

        /// <summary>
        ///     The quest tag for this NPC.
        /// </summary>
        public Quest Quest { get; init; }

        /// <summary>
        ///     The role tag for this NPC.
        /// </summary>
        public NPCRole Role { get; init; }

        /// <summary>
        ///     If populated, the stand this NPC is using. (GUI related)
        /// </summary>
        public string? Stand { get; init; }

        /// <summary>
        ///     The type of token this NPC is related to.
        /// </summary>
        public Token Token { get; init; }
    }
}