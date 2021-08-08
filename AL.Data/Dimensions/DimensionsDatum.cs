using System.Collections.Generic;
using Newtonsoft.Json;

namespace AL.Data.Dimensions
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class DimensionsDatum : DatumBase<IReadOnlyList<float>>
    {
        public IReadOnlyList<float> Arcticbee { get; set; } = null!;
        public IReadOnlyList<float> Armadillo { get; set; } = null!;
        public IReadOnlyList<float> Bat { get; set; } = null!;
        public IReadOnlyList<float> Bbpompom { get; set; } = null!;
        public IReadOnlyList<float> Bee { get; set; } = null!;
        public IReadOnlyList<float> Bigbird { get; set; } = null!;
        public IReadOnlyList<float> Boar { get; set; } = null!;
        public IReadOnlyList<float> Bscorpion { get; set; } = null!;
        public IReadOnlyList<float> Cgoo { get; set; } = null!;
        public IReadOnlyList<float> Crabx { get; set; } = null!;
        public IReadOnlyList<float> Croc { get; set; } = null!;
        [JsonProperty("default_character")]
        public IReadOnlyList<float> DefaultCharacter { get; set; } = null!;
        public IReadOnlyList<float> Dknight2 { get; set; } = null!;
        public IReadOnlyList<float> Ent { get; set; } = null!;
        public IReadOnlyList<float> Fireroamer { get; set; } = null!;
        public IReadOnlyList<float> Frog { get; set; } = null!;
        public IReadOnlyList<float> Ghost { get; set; } = null!;
        public IReadOnlyList<float> Goldenbat { get; set; } = null!;
        public IReadOnlyList<float> Goo { get; set; } = null!;
        public IReadOnlyList<float> Grinch { get; set; } = null!;
        public IReadOnlyList<float> Gscorpion { get; set; } = null!;
        public IReadOnlyList<float> Hen { get; set; } = null!;
        public IReadOnlyList<float> Iceroamer { get; set; } = null!;
        public IReadOnlyList<float> Jrat { get; set; } = null!;
        public IReadOnlyList<float> Kitty1 { get; set; } = null!;
        public IReadOnlyList<float> Kitty2 { get; set; } = null!;
        public IReadOnlyList<float> Kitty3 { get; set; } = null!;
        public IReadOnlyList<float> Kitty4 { get; set; } = null!;
        public IReadOnlyList<float> Mechagnome { get; set; } = null!;
        public IReadOnlyList<float> Minimush { get; set; } = null!;
        public IReadOnlyList<float> Mole { get; set; } = null!;
        public IReadOnlyList<float> Oneeye { get; set; } = null!;
        public IReadOnlyList<float> Osnake { get; set; } = null!;
        public IReadOnlyList<float> Phoenix { get; set; } = null!;
        public IReadOnlyList<float> Pinkgoblin { get; set; } = null!;
        public IReadOnlyList<float> Pinkgoo { get; set; } = null!;
        public IReadOnlyList<float> Plantoid { get; set; } = null!;
        public IReadOnlyList<float> Poisio { get; set; } = null!;
        public IReadOnlyList<float> Pppompom { get; set; } = null!;
        public IReadOnlyList<float> Prat { get; set; } = null!;
        public IReadOnlyList<float> Puppy1 { get; set; } = null!;
        public IReadOnlyList<float> Puppy2 { get; set; } = null!;
        public IReadOnlyList<float> Puppy3 { get; set; } = null!;
        public IReadOnlyList<float> Puppy4 { get; set; } = null!;
        public IReadOnlyList<float> Rat { get; set; } = null!;
        public IReadOnlyList<float> Rooster { get; set; } = null!;
        public IReadOnlyList<float> Rudolph { get; set; } = null!;
        public IReadOnlyList<float> Scorpion { get; set; } = null!;
        public IReadOnlyList<float> Skeletor { get; set; } = null!;
        public IReadOnlyList<float> Snake { get; set; } = null!;
        public IReadOnlyList<float> Spider { get; set; } = null!;
        public IReadOnlyList<float> Squig { get; set; } = null!;
        public IReadOnlyList<float> Squigtoad { get; set; } = null!;
        public IReadOnlyList<float> Stoneworm { get; set; } = null!;
        public IReadOnlyList<float> Tortoise { get; set; } = null!;
        public IReadOnlyList<float> Wabbit { get; set; } = null!;
        public IReadOnlyList<float> Wolf { get; set; } = null!;
        public IReadOnlyList<float> Wolfie { get; set; } = null!;
        public IReadOnlyList<float> Xscorpion { get; set; } = null!;
    }
}