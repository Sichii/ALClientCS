﻿using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents a character. Contains additional information that the owner of a player would have.
    /// </summary>
    /// <remarks>Mutated by <see cref="NewMapData" />, <see cref="ILocation" /></remarks>
    /// <seealso cref="Player" />
    /// <seealso cref="IMutable{TMutator}" />
    [JsonConverter(typeof(CharacterConverter<Character>))]
    public record Character : Player, IMutable<ILocation>, IMutable<NewMapData>
    {
        /// <summary>
        ///     The number of entities currently targeting this player.
        /// </summary>
        [JsonProperty("targets")]
        public int AggroTargets { get; init; }

        /// <summary>
        ///     If populated, this character is inside of a bank. <br />
        ///     This property only populates while you are inside of a bank, and hold information about gold and items this
        ///     character has in the bank.
        /// </summary>
        [JsonProperty("user"), JsonConverter(typeof(BankDataConverter))]
        public BankInfo? Bank { get; init; }

        /// <summary>
        ///     TODO: unknown
        /// </summary>
        [JsonProperty]
        public int Cache { get; init; }

        /// <summary>
        ///     If populated, this contains all of the code executing for this character.
        /// </summary>
        [JsonProperty]
        public new string? Code { get; init; }

        /// <summary>
        ///     A value indicating the impact of the network calls this character is making. <br />
        ///     Not all network calls are equal in value. This number resets to 0 every second. <br />
        ///     <b>If this number goes above 200, you will be disconnected.</b>
        /// </summary>
        [JsonProperty("cc")]
        public float CodeCost { get; init; }

        /// <summary>
        ///     TODO: not too familiar with this data right now.
        /// </summary>
        [JsonProperty("emx")]
        public IReadOnlyDictionary<Emotion, float> Emotion { get; init; } = new Dictionary<Emotion, float>();

        /// <summary>
        ///     The number of empty slots in this character's inventory.
        /// </summary>
        [JsonProperty("esize")]
        public int EmptySlots { get; init; }

        /// <summary>
        ///     While you are not attacking, your range increases by 5/second, up to a max of 25.
        /// </summary>
        [JsonProperty("xrange")]
        public float ExtraRange { get; init; }

        /// <summary>
        ///     When the number of monsters you are attacking exceeds the courage value for that type of monster, you start to gain
        ///     fear. <br />
        ///     This value is the number of monsters you are exceeding your courage by.
        /// </summary>
        [JsonProperty]
        public float Fear { get; init; }

        /// <summary>
        ///     A list of characters on your friend's like
        ///     TODO: gather more info about this
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<string> Friends { get; init; } = new List<string>();

        /// <summary>
        ///     The character's inventory.
        /// </summary>
        [JsonProperty("items")]
        public IReadOnlyList<InventoryItem> Inventory { get; internal set; } = new List<InventoryItem>();

        /// <summary>
        ///     The maximum size of this character's inventory.
        /// </summary>
        [JsonProperty("isize")]
        public int InventorySize { get; init; }

        /// <summary>
        ///     The number of times this character has changed maps.
        /// </summary>
        [JsonProperty("m")]
        public int MapChangeCount { get; protected set; }

        /// <summary>
        ///     A collection of all of the cosmetics owned by this character.
        /// </summary>
        [JsonProperty("acx")]
        public IReadOnlyDictionary<string, int> OwnedCosmetics { get; init; } = new Dictionary<string, int>();

        /// <summary>
        ///     When you buy or sell an item, there is a fraction of that item's value you pay in taxes. <br />
        ///     For merchants, this tax is converted into earned xp.
        /// </summary>
        [JsonProperty]
        public float Tax { get; init; }

        public virtual bool Equals(Character? other) => base.Equals(other);

        public override int GetHashCode() => base.GetHashCode();

        public void Mutate(NewMapData mutator)
        {
            X = mutator.X;
            Y = mutator.Y;
            Map = mutator.Map;
            MapChangeCount = mutator.MapChangeCount;
        }

        public void Mutate(ILocation mutator)
        {
            X = mutator.X;
            Y = mutator.Y;
            Map = mutator.Map;
        }
    }
}