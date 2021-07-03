using System;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public abstract record EntityBase : AttributedRecordBase, ILocation, IMutable<EntityBase>, IDeltaUpdateable
    {
        //TODO: what's this?
        [JsonProperty]
        public bool ABS { get; protected set; }

        [JsonProperty]
        public float Angle { get; protected set; }

        [JsonProperty]
        public int CID { get; init; }

        [JsonProperty("s")]
        public AwaitableDictionary<Core.Definitions.Condition, Condition> Conditions { get; protected set; } = new();
        public long Delta { get; set; } = DeltaTime.Value;

        [JsonProperty("going_x")]
        public float GoingX { get; protected set; }

        [JsonProperty("going_y")]
        public float GoingY { get; protected set; }

        [JsonProperty]
        public string Id { get; init; }

        [JsonProperty]
        public int Level { get; protected set; }

        [JsonProperty("map")]
        public string Map { get; init; }

        [JsonProperty("max_hp")]
        public int MaxHP { get; protected set; }

        [JsonProperty]
        public bool Moving { get; protected set; }

        [JsonProperty("move_num")]
        public ulong StepCount { get; protected set; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public float X { get; protected set; }

        [JsonProperty]
        public float Y { get; protected set; }

        public virtual bool Equals(EntityBase other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public virtual void Mutate(EntityBase other)
        {
            if (Id != other.Id)
                throw new InvalidOperationException(
                    $"Attempting to update entity with ID: {Id}, with data for entity with ID: {other.Id}");

            ABS = other.ABS;
            Angle = other.Angle;
            Armor = other.Armor;
            GoingX = other.GoingX;
            GoingY = other.GoingY;
            HP = other.HP;
            MaxHP = other.MaxHP;
            Level = other.Level;
            StepCount = other.StepCount;
            Moving = other.Moving;
            Conditions = other.Conditions;
            Speed = other.Speed;
            X = other.X;
            Y = other.Y;
            XP = other.XP;
            Attack = other.Attack;
            Frequency = other.Frequency;
            MP = other.MP;
            Resistance = other.Resistance;
        }

        public void Mutate(object other)
        {
            if (other is EntityBase entity)
                Mutate(entity);
        }

        public void Update(long delta)
        {
            Delta += delta;

            //if not moving, or less than 1ms has passed, or we're already where we need to be, then dont update
            if (!Moving
                || delta == 0
                || X.NearlyEquals(GoingX, CONSTANTS.EPSILON) && Y.NearlyEquals(GoingY, CONSTANTS.EPSILON))
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