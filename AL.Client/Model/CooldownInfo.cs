using System;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.SocketClient.Interfaces;

namespace AL.Client.Model
{
    /// <summary>
    ///     Represents the instant in time in which the server confirmed that a skill was used.
    /// </summary>
    public class CooldownInfo : IPingCompensated
    {
        private long Delta = DeltaTime.Value;
        /// <summary>
        ///     The cooldown of the skill.
        /// </summary>
        public float CooldownMS { get; init; }
        public bool IsCompensated { get; private set; }

        //TODO: maybe make this the skill name, but there's really no point
        string IMutable.Id => string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CooldownInfo" /> class.
        /// </summary>
        /// <param name="cooldownMS">The cooldown of the skill.</param>
        public CooldownInfo(float cooldownMS) => CooldownMS = cooldownMS;

        /// <summary>
        ///     Whether or not the skill can be used.
        /// </summary>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the skill can be used, otherwise <c>false</c>.
        /// </returns>
        public bool CanUse() => ElapsedMS > CooldownMS;

        /// <summary>
        ///     Gets the amount of milliseconds that have elapsed since a skill was used.
        /// </summary>
        public float ElapsedMS => DeltaTime.Value - Delta;

        /// <summary>
        ///     Gets the remaining cooldown in milliseconds.
        /// </summary>
        public float RemainingMS => Math.Min(0, CooldownMS - ElapsedMS);
        
        public void CompensateOnce(int minimumOffsetMS)
        {
            if (IsCompensated)
                throw new InvalidOperationException("Object already compensated.");

            IsCompensated = true;
            Delta -= minimumOffsetMS;
        }
    }
}