#nullable disable

namespace AL.Data.Games
{
    public class GamesDatum
    {
        public object Dice { get; set; }
        public Slots Slots { get; set; }
        public Tarot Tarot { get; set; }
        public Wheel Wheel { get; set; }
    }
}