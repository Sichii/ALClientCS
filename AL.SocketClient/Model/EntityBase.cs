using System;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Core.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Provides a base for <see cref="Player" />s and <see cref="Monster" />s.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    /// <seealso cref="ILocation" />
    /// <seealso cref="IMutable{TMutator}" />
    /// <remarks>Mutated by <see cref="EntityBase" />, <see cref="Mutation" /></remarks>
    public abstract record EntityBase : AttributedRecordBase, ILocation, IMutable<EntityBase>, IMutable<Mutation>,
        IDeltaUpdateable
    {
        /// <summary>
        ///     TODO: what's this?
        /// </summary>
        [JsonProperty]
        public bool ABS { get; protected set; }

        /// <summary>
        ///     If moving, this is the angle they are moving at. (in degrees +/- 180)
        /// </summary>
        [JsonProperty]
        public float Angle { get; protected set; }

        /// <summary>
        ///     The conditions this entity has.
        /// </summary>
        [JsonProperty("s")]
        public AwaitableDictionary<Core.Definitions.Condition, Condition> Conditions { get; protected set; } = new();

        /// <summary>
        ///     A unique ID for this version of the entity's data. <br />
        ///     This number starts at 0 and iterates by 1 every time a new version of this entity's data is sent.
        /// </summary>
        [JsonProperty("cid")]
        public int ContinuousId { get; init; }
        public long Delta { get; set; } = DeltaTime.Value;

        /// <summary>
        ///     If this entity is moving, this is the X coordinate they are moving to.
        /// </summary>
        [JsonProperty("going_x")]
        public float GoingX { get; protected set; }

        /// <summary>
        ///     If this entity is moving, this is the Y coordinate they are moving to.
        /// </summary>
        [JsonProperty("going_y")]
        public float GoingY { get; protected set; }

        /// <summary>
        ///     <see cref="Player" /> name, or <see cref="Monster" /> unique id.
        /// </summary>
        [JsonProperty]
        public string Id { get; init; } = null!;

        [JsonProperty]
        public int Level { get; protected set; }

        [JsonProperty("map")]
        public string Map { get; protected set; } = null!;


        [JsonProperty("max_hp")]
        public float MaxHP { get; protected set; }

        /// <summary>
        ///     The number of individual movements this entity has done.
        /// </summary>
        [JsonProperty("move_num")]
        public ulong MoveNum { get; protected set; }

        [JsonProperty]
        public bool Moving { get; protected set; }

        /// <summary>
        ///     If populated, the <see cref="Id" /> of this entity's target.
        /// </summary>
        [JsonProperty]
        public string? Target { get; init; }

        [JsonProperty]
        public float X { get; protected set; }

        [JsonProperty]
        public float Y { get; protected set; }

        public virtual bool Equals(EntityBase? other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public virtual void Mutate(EntityBase mutator)
        {
            if (Id != mutator.Id)
                throw new InvalidOperationException(
                    $"Attempting to update entity with ID: {Id}, with data for entity with ID: {mutator.Id}");

            ABS = mutator.ABS;
            Angle = mutator.Angle;
            Armor = mutator.Armor;
            GoingX = mutator.GoingX;
            GoingY = mutator.GoingY;
            HP = mutator.HP;
            MaxHP = mutator.MaxHP;
            Level = mutator.Level;
            MoveNum = mutator.MoveNum;
            Moving = mutator.Moving;
            Conditions = mutator.Conditions;
            Speed = mutator.Speed;
            X = mutator.X;
            Y = mutator.Y;
            XP = mutator.XP;
            Attack = mutator.Attack;
            Frequency = mutator.Frequency;
            MP = mutator.MP;
            Resistance = mutator.Resistance;
        }

        public void Mutate(Mutation mutator)
        {
            if (mutator.Attribute == ALAttribute.Hp)
                HP += (int) mutator.Mutator;
        }

        public void Update(long delta)
        {
            Delta += delta;

            //if not moving, or less than 1ms has passed, or we're already where we need to be, then dont update
            if (!Moving
                || (delta == 0)
                || (X.NearlyEquals(GoingX, CONSTANTS.EPSILON) && Y.NearlyEquals(GoingY, CONSTANTS.EPSILON)))
                return;

            var going = new Point(GoingX, GoingY);
            var speed = Speed / 1000 * delta;
            var distance = this.Distance(going);

            if (distance > speed)
                distance = speed;
            else
            {
                Moving = false;
                X = GoingX;
                Y = GoingY;

                return;
            }

            (var newX, var newY) = this.AngularOffset(Angle, distance);
            X = newX;
            Y = newY;
        }
    }
}