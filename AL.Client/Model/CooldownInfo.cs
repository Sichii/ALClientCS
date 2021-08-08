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
        private readonly long Delta = DeltaTime.Value;
        /// <summary>
        ///     The cooldown of the skill.
        /// </summary>
        public float CooldownMS { get; private set; }
        public bool IsCompensated { get; private set; }

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
        public bool CanUse() => DeltaTime.Value - Delta > CooldownMS;

        public void CompensateOnce(int minimumOffsetMS)
        {
            if (IsCompensated)
                throw new InvalidOperationException("Object already compensated.");

            IsCompensated = true;
            CooldownMS -= minimumOffsetMS;
        }
    }
}