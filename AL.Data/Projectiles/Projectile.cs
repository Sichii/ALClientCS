namespace AL.Data.Projectiles
{
    public record Projectile
    {
        public bool Pure { get; init; }
        public bool Ray { get; init; }
        public float Speed { get; init; }
    }
}