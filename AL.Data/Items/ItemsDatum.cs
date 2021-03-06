using System.Linq;
using AL.Core.Json.Converters;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.Data.Items
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GItem>))]
    public class ItemsDatum : DatumBase<GItem>
    {
        [JsonProperty("5bucks")]
        public GItem _5bucks { get; init; } = null!;
        public GItem Ale { get; init; } = null!;
        public GItem Amuletofm { get; init; } = null!;
        public GItem Angelwings { get; init; } = null!;
        public GItem Apiercingscroll { get; init; } = null!;
        public GItem Apologybox { get; init; } = null!;
        public GItem Armorbox { get; init; } = null!;
        public GItem Armorring { get; init; } = null!;
        public GItem Armorscroll { get; init; } = null!;
        public GItem Ascale { get; init; } = null!;
        public GItem Axe3 { get; init; } = null!;
        public GItem Bandages { get; init; } = null!;
        public GItem Basher { get; init; } = null!;
        public GItem Basketofeggs { get; init; } = null!;
        public GItem Bataxe { get; init; } = null!;
        public GItem Bcandle { get; init; } = null!;
        public GItem Bcape { get; init; } = null!;
        public GItem Beewings { get; init; } = null!;
        public GItem Bfang { get; init; } = null!;
        public GItem Bfur { get; init; } = null!;
        public GItem Bkey { get; init; } = null!;
        public GItem Blade { get; init; } = null!;
        public GItem Blue { get; init; } = null!;
        public GItem Bottleofxp { get; init; } = null!;
        public GItem Bow { get; init; } = null!;
        public GItem Bow4 { get; init; } = null!;
        public GItem Bowofthedead { get; init; } = null!;
        public GItem Bronzeingot { get; init; } = null!;
        public GItem Bronzenugget { get; init; } = null!;
        public GItem Brownegg { get; init; } = null!;
        public GItem Btusk { get; init; } = null!;
        public GItem Bugbountybox { get; init; } = null!;
        public GItem Bunnyears { get; init; } = null!;
        public GItem Bunnyelixir { get; init; } = null!;
        public GItem Bwing { get; init; } = null!;
        public GItem Cake { get; init; } = null!;
        public GItem Candy0 { get; init; } = null!;
        public GItem Candy0V2 { get; init; } = null!;
        public GItem Candy0V3 { get; init; } = null!;
        public GItem Candy1 { get; init; } = null!;
        public GItem Candy1V2 { get; init; } = null!;
        public GItem Candy1V3 { get; init; } = null!;
        public GItem Candycane { get; init; } = null!;
        public GItem Candycanesword { get; init; } = null!;
        public GItem Candypop { get; init; } = null!;
        public GItem Cape { get; init; } = null!;
        public GItem Carrot { get; init; } = null!;
        public GItem Carrotsword { get; init; } = null!;
        public GItem Cclaw { get; init; } = null!;
        public GItem Cdarktristone { get; init; } = null!;
        public GItem Cdragon { get; init; } = null!;
        public GItem Cearring { get; init; } = null!;
        public GItem Charmer { get; init; } = null!;
        public GItem Chrysalis0 { get; init; } = null!;
        public GItem Claw { get; init; } = null!;
        public GItem Coal { get; init; } = null!;
        public GItem Coat { get; init; } = null!;
        public GItem Coat1 { get; init; } = null!;
        public GItem Cocoon { get; init; } = null!;
        public GItem Computer { get; init; } = null!;
        public GItem Confetti { get; init; } = null!;
        public GItem Cosmo0 { get; init; } = null!;
        public GItem Cosmo1 { get; init; } = null!;
        public GItem Cosmo2 { get; init; } = null!;
        public GItem Cosmo3 { get; init; } = null!;
        public GItem Cosmo4 { get; init; } = null!;
        public GItem Crabclaw { get; init; } = null!;
        public GItem Cring { get; init; } = null!;
        public GItem Critscroll { get; init; } = null!;
        public GItem Crossbow { get; init; } = null!;
        public GItem Cryptkey { get; init; } = null!;
        public GItem Cscale { get; init; } = null!;
        public GItem Cscroll0 { get; init; } = null!;
        public GItem Cscroll1 { get; init; } = null!;
        public GItem Cscroll2 { get; init; } = null!;
        public GItem Cscroll3 { get; init; } = null!;
        public GItem Cshell { get; init; } = null!;
        public GItem Ctristone { get; init; } = null!;
        public GItem Cupid { get; init; } = null!;
        public GItem Cxjar { get; init; } = null!;
        public GItem Cyber { get; init; } = null!;
        public GItem Dagger { get; init; } = null!;
        public GItem Daggerofthedead { get; init; } = null!;
        public GItem Darktristone { get; init; } = null!;
        public GItem Dartgun { get; init; } = null!;
        public GItem Dexamulet { get; init; } = null!;
        public GItem Dexbelt { get; init; } = null!;
        public GItem Dexearring { get; init; } = null!;
        public GItem Dexearringx { get; init; } = null!;
        public GItem Dexring { get; init; } = null!;
        public GItem Dexscroll { get; init; } = null!;
        public GItem Dkey { get; init; } = null!;
        public GItem Dragondagger { get; init; } = null!;
        public GItem Drapes { get; init; } = null!;
        public GItem Dreturnscroll { get; init; } = null!;
        public GItem Dstones { get; init; } = null!;
        public GItem Ecape { get; init; } = null!;
        public GItem Ectoplasm { get; init; } = null!;
        public GItem Eears { get; init; } = null!;
        public GItem Egg0 { get; init; } = null!;
        public GItem Egg1 { get; init; } = null!;
        public GItem Egg2 { get; init; } = null!;
        public GItem Egg3 { get; init; } = null!;
        public GItem Egg4 { get; init; } = null!;
        public GItem Egg5 { get; init; } = null!;
        public GItem Egg6 { get; init; } = null!;
        public GItem Egg7 { get; init; } = null!;
        public GItem Egg8 { get; init; } = null!;
        public GItem Eggnog { get; init; } = null!;
        public GItem Electronics { get; init; } = null!;
        public GItem Elixirdex0 { get; init; } = null!;
        public GItem Elixirdex1 { get; init; } = null!;
        public GItem Elixirdex2 { get; init; } = null!;
        [JsonProperty("elixirfires")]
        public GItem Elixirfireres { get; init; } = null!;
        [JsonProperty("elixirfzres")]
        public GItem Elixirfreezeres { get; init; } = null!;
        public GItem Elixirint0 { get; init; } = null!;
        public GItem Elixirint1 { get; init; } = null!;
        public GItem Elixirint2 { get; init; } = null!;
        public GItem Elixirluck { get; init; } = null!;
        public GItem Elixirpnres { get; init; } = null!;
        public GItem Elixirstr0 { get; init; } = null!;
        public GItem Elixirstr1 { get; init; } = null!;
        public GItem Elixirstr2 { get; init; } = null!;
        public GItem Elixirvit0 { get; init; } = null!;
        public GItem Elixirvit1 { get; init; } = null!;
        public GItem Elixirvit2 { get; init; } = null!;
        public GItem Emotionjar { get; init; } = null!;
        public GItem Emptyheart { get; init; } = null!;
        public GItem Emptyjar { get; init; } = null!;
        public GItem Epyjamas { get; init; } = null!;
        public GItem Eslippers { get; init; } = null!;
        public GItem Espresso { get; init; } = null!;
        public GItem Essenceofether { get; init; } = null!;
        public GItem Essenceoffire { get; init; } = null!;
        public GItem Essenceoffrost { get; init; } = null!;
        public GItem Essenceofgreed { get; init; } = null!;
        public GItem Essenceoflife { get; init; } = null!;
        public GItem Essenceofnature { get; init; } = null!;
        public GItem Evasionscroll { get; init; } = null!;
        public GItem Exoarm { get; init; } = null!;
        public GItem Fallen { get; init; } = null!;
        public GItem Fcape { get; init; } = null!;
        public GItem Fclaw { get; init; } = null!;
        public GItem Feather0 { get; init; } = null!;
        public GItem Fieldgen0 { get; init; } = null!;
        public GItem Fierygloves { get; init; } = null!;
        public GItem Figurine { get; init; } = null!;
        public GItem Fireblade { get; init; } = null!;
        public GItem Firebow { get; init; } = null!;
        public GItem Firecrackers { get; init; } = null!;
        public GItem Firestaff { get; init; } = null!;
        public GItem Firestars { get; init; } = null!;
        public GItem Flute { get; init; } = null!;
        public GItem Forscroll { get; init; } = null!;
        public GItem Frankypants { get; init; } = null!;
        public GItem Frequencyscroll { get; init; } = null!;
        public GItem Frogt { get; init; } = null!;
        public GItem Frostbow { get; init; } = null!;
        public GItem Froststaff { get; init; } = null!;
        public GItem Frozenkey { get; init; } = null!;
        public GItem Frozenstone { get; init; } = null!;
        public GItem Fsword { get; init; } = null!;
        public GItem Ftrinket { get; init; } = null!;
        public GItem Funtoken { get; init; } = null!;
        public GItem Fury { get; init; } = null!;
        public GItem Gbow { get; init; } = null!;
        public GItem Gcape { get; init; } = null!;
        public GItem Gem0 { get; init; } = null!;
        public GItem Gem1 { get; init; } = null!;
        public GItem Gem2 { get; init; } = null!;
        public GItem Gem3 { get; init; } = null!;
        public GItem Gemfragment { get; init; } = null!;
        public GItem Ghatb { get; init; } = null!;
        public GItem Ghatp { get; init; } = null!;
        public GItem Gift0 { get; init; } = null!;
        public GItem Gift1 { get; init; } = null!;
        public GItem Glitch { get; init; } = null!;
        public GItem Gloves { get; init; } = null!;
        public GItem Gloves1 { get; init; } = null!;
        public GItem Goldbooster { get; init; } = null!;
        public GItem Goldenegg { get; init; } = null!;
        public GItem Goldenpowerglove { get; init; } = null!;
        public GItem Goldingot { get; init; } = null!;
        public GItem Goldnugget { get; init; } = null!;
        public GItem Goldring { get; init; } = null!;
        public GItem Goldscroll { get; init; } = null!;
        public GItem Gphelmet { get; init; } = null!;
        public GItem Greenbomb { get; init; } = null!;
        public GItem Greenenvelope { get; init; } = null!;
        public GItem Gslime { get; init; } = null!;
        public GItem Gstaff { get; init; } = null!;
        public GItem Gum { get; init; } = null!;
        public GItem Hammer { get; init; } = null!;
        public GItem Handofmidas { get; init; } = null!;
        public GItem Harbringer { get; init; } = null!;
        public GItem Harmor { get; init; } = null!;
        public GItem Hboots { get; init; } = null!;
        public GItem Hbow { get; init; } = null!;
        public GItem Hdagger { get; init; } = null!;
        public GItem Heartwood { get; init; } = null!;
        public GItem Helmet { get; init; } = null!;
        public GItem Helmet1 { get; init; } = null!;
        public GItem Hgloves { get; init; } = null!;
        public GItem Hhelmet { get; init; } = null!;
        public GItem Hotchocolate { get; init; } = null!;
        public GItem Hpamulet { get; init; } = null!;
        public GItem Hpants { get; init; } = null!;
        public GItem Hpbelt { get; init; } = null!;
        public GItem Hpot0 { get; init; } = null!;
        public GItem Hpot1 { get; init; } = null!;
        public GItem Hpotx { get; init; } = null!;
        public GItem Iceskates { get; init; } = null!;
        public GItem Ijx { get; init; } = null!;
        public GItem Ink { get; init; } = null!;
        public GItem Intamulet { get; init; } = null!;
        public GItem Intbelt { get; init; } = null!;
        public GItem Intearring { get; init; } = null!;
        public GItem Intring { get; init; } = null!;
        public GItem Intscroll { get; init; } = null!;
        public GItem Jacko { get; init; } = null!;
        public GItem Jewellerybox { get; init; } = null!;
        public GItem Kitty1 { get; init; } = null!;
        public GItem Lantern { get; init; } = null!;
        public GItem Lbelt { get; init; } = null!;
        public GItem Leather { get; init; } = null!;
        public GItem Ledger { get; init; } = null!;
        public GItem Licence { get; init; } = null!;
        public GItem Lifestealscroll { get; init; } = null!;
        public GItem Lmace { get; init; } = null!;
        public GItem Lostearring { get; init; } = null!;
        public GItem Lotusf { get; init; } = null!;
        public GItem Lspores { get; init; } = null!;
        public GItem Luckbooster { get; init; } = null!;
        public GItem Luckscroll { get; init; } = null!;
        public GItem Luckyt { get; init; } = null!;
        public GItem Mace { get; init; } = null!;
        public GItem Maceofthedead { get; init; } = null!;
        public GItem Mageshood { get; init; } = null!;
        public GItem Manastealscroll { get; init; } = null!;
        public GItem Mbelt { get; init; } = null!;
        public GItem Mbones { get; init; } = null!;
        public GItem Mcape { get; init; } = null!;
        public GItem Mcarmor { get; init; } = null!;
        public GItem Mcboots { get; init; } = null!;
        public GItem Mcgloves { get; init; } = null!;
        public GItem Mchat { get; init; } = null!;
        public GItem Mcpants { get; init; } = null!;
        public GItem Mearring { get; init; } = null!;
        public GItem Merry { get; init; } = null!;
        public GItem Mistletoe { get; init; } = null!;
        public GItem Mittens { get; init; } = null!;
        public GItem Mmarmor { get; init; } = null!;
        public GItem Mmgloves { get; init; } = null!;
        public GItem Mmhat { get; init; } = null!;
        public GItem Mmpants { get; init; } = null!;
        public GItem Mmshoes { get; init; } = null!;
        public GItem Molesteeth { get; init; } = null!;
        public GItem Monsterbox { get; init; } = null!;
        public GItem Monstertoken { get; init; } = null!;
        public GItem Mparmor { get; init; } = null!;
        public GItem Mpcostscroll { get; init; } = null!;
        public GItem Mpgloves { get; init; } = null!;
        public GItem Mphat { get; init; } = null!;
        public GItem Mpot0 { get; init; } = null!;
        public GItem Mpot1 { get; init; } = null!;
        public GItem Mpotx { get; init; } = null!;
        public GItem Mppants { get; init; } = null!;
        public GItem Mpshoes { get; init; } = null!;
        public GItem Mpxamulet { get; init; } = null!;
        public GItem Mpxbelt { get; init; } = null!;
        public GItem Mpxgloves { get; init; } = null!;
        public GItem Mrarmor { get; init; } = null!;
        public GItem Mrboots { get; init; } = null!;
        public GItem Mrgloves { get; init; } = null!;
        public GItem Mrhood { get; init; } = null!;
        public GItem Mrnarmor { get; init; } = null!;
        public GItem Mrnboots { get; init; } = null!;
        public GItem Mrngloves { get; init; } = null!;
        public GItem Mrnhat { get; init; } = null!;
        public GItem Mrnpants { get; init; } = null!;
        public GItem Mrpants { get; init; } = null!;
        public GItem Mshield { get; init; } = null!;
        public GItem Mushroomstaff { get; init; } = null!;
        public GItem Mwarmor { get; init; } = null!;
        public GItem Mwboots { get; init; } = null!;
        public GItem Mwgloves { get; init; } = null!;
        public GItem Mwhelmet { get; init; } = null!;
        public GItem Mwpants { get; init; } = null!;
        public GItem Mysterybox { get; init; } = null!;
        public GItem Networkcard { get; init; } = null!;
        public GItem Nheart { get; init; } = null!;
        public GItem Northstar { get; init; } = null!;
        public GItem Offering { get; init; } = null!;
        public GItem Offeringp { get; init; } = null!;
        public GItem Offeringx { get; init; } = null!;
        public GItem Oozingterror { get; init; } = null!;
        public GItem Orbg { get; init; } = null!;
        public GItem Orbofdex { get; init; } = null!;
        public GItem Orbofint { get; init; } = null!;
        public GItem Orbofsc { get; init; } = null!;
        public GItem Orbofstr { get; init; } = null!;
        public GItem Orbofvit { get; init; } = null!;
        public GItem Ornament { get; init; } = null!;
        public GItem Ornamentstaff { get; init; } = null!;
        public GItem Outputscroll { get; init; } = null!;
        public GItem Oxhelmet { get; init; } = null!;
        public GItem Pants { get; init; } = null!;
        public GItem Pants1 { get; init; } = null!;
        public GItem Partyhat { get; init; } = null!;
        public GItem Phelmet { get; init; } = null!;
        public GItem Pickaxe { get; init; } = null!;
        public GItem Pico { get; init; } = null!;
        public GItem Pinkie { get; init; } = null!;
        public GItem Placeholder { get; init; } = null!;
        [JsonProperty("placeholder_m")]
        public GItem PlaceholderM { get; init; } = null!;
        public GItem Platinumingot { get; init; } = null!;
        public GItem Platinumnugget { get; init; } = null!;
        public GItem Pleather { get; init; } = null!;
        public GItem Pmace { get; init; } = null!;
        public GItem Poison { get; init; } = null!;
        public GItem Poker { get; init; } = null!;
        public GItem Pouchbow { get; init; } = null!;
        public GItem Powerglove { get; init; } = null!;
        public GItem Pstem { get; init; } = null!;
        public GItem Pumpkinspice { get; init; } = null!;
        public GItem Puppy1 { get; init; } = null!;
        public GItem Puppyer { get; init; } = null!;
        public GItem Pvptoken { get; init; } = null!;
        public GItem Pyjamas { get; init; } = null!;
        public GItem Qubics { get; init; } = null!;
        public GItem Quiver { get; init; } = null!;
        public GItem Rabbitsfoot { get; init; } = null!;
        public GItem Rapier { get; init; } = null!;
        public GItem Rattail { get; init; } = null!;
        public GItem Redenvelope { get; init; } = null!;
        public GItem Redenvelopev2 { get; init; } = null!;
        public GItem Redenvelopev3 { get; init; } = null!;
        public GItem Rednose { get; init; } = null!;
        public GItem Reflectionscroll { get; init; } = null!;
        public GItem Resistancering { get; init; } = null!;
        public GItem Resistancescroll { get; init; } = null!;
        public GItem Rfangs { get; init; } = null!;
        public GItem Rfur { get; init; } = null!;
        public GItem Ringofluck { get; init; } = null!;
        public GItem Ringsj { get; init; } = null!;
        public GItem Rod { get; init; } = null!;
        public GItem Rpiercingscroll { get; init; } = null!;
        public GItem Sanguine { get; init; } = null!;
        public GItem Santasbelt { get; init; } = null!;
        public GItem Scroll0 { get; init; } = null!;
        public GItem Scroll1 { get; init; } = null!;
        public GItem Scroll2 { get; init; } = null!;
        public GItem Scroll3 { get; init; } = null!;
        public GItem Scroll4 { get; init; } = null!;
        public GItem Scythe { get; init; } = null!;
        public GItem Seashell { get; init; } = null!;
        public GItem Shadowstone { get; init; } = null!;
        public GItem Shield { get; init; } = null!;
        public GItem Shoes { get; init; } = null!;
        public GItem Shoes1 { get; init; } = null!;
        public GItem Slimestaff { get; init; } = null!;
        public GItem Smoke { get; init; } = null!;
        public GItem Smush { get; init; } = null!;
        public GItem Snakefang { get; init; } = null!;
        public GItem Snakeoil { get; init; } = null!;
        public GItem Snowball { get; init; } = null!;
        public GItem Snowboots { get; init; } = null!;
        public GItem Snowflakes { get; init; } = null!;
        public GItem Snring { get; init; } = null!;
        public GItem Solitaire { get; init; } = null!;
        public GItem Spear { get; init; } = null!;
        public GItem Spearofthedead { get; init; } = null!;
        public GItem Speedscroll { get; init; } = null!;
        public GItem Spidersilk { get; init; } = null!;
        public GItem Spores { get; init; } = null!;
        public GItem Sshield { get; init; } = null!;
        public GItem Sstinger { get; init; } = null!;
        public GItem Staff { get; init; } = null!;
        public GItem Staff2 { get; init; } = null!;
        public GItem Staff3 { get; init; } = null!;
        public GItem Staff4 { get; init; } = null!;
        public GItem Staffofthedead { get; init; } = null!;
        public GItem Stand0 { get; init; } = null!;
        public GItem Stand1 { get; init; } = null!;
        public GItem Starkillers { get; init; } = null!;
        public GItem Stealthcape { get; init; } = null!;
        public GItem Stick { get; init; } = null!;
        public GItem Stinger { get; init; } = null!;
        public GItem Stonekey { get; init; } = null!;
        public GItem Stoneofgold { get; init; } = null!;
        public GItem Stoneofluck { get; init; } = null!;
        public GItem Stoneofxp { get; init; } = null!;
        public GItem Storagebox { get; init; } = null!;
        public GItem Stramulet { get; init; } = null!;
        public GItem Strbelt { get; init; } = null!;
        public GItem Strearring { get; init; } = null!;
        public GItem Strring { get; init; } = null!;
        public GItem Strscroll { get; init; } = null!;
        public GItem Suckerpunch { get; init; } = null!;
        public GItem Supermittens { get; init; } = null!;
        public GItem Svenom { get; init; } = null!;
        public GItem Swifty { get; init; } = null!;
        public GItem Swirlipop { get; init; } = null!;
        public GItem Sword { get; init; } = null!;
        public GItem Swordofthedead { get; init; } = null!;
        public GItem T2Bow { get; init; } = null!;
        public GItem T2dexamulet { get; init; } = null!;
        public GItem T2Intamulet { get; init; } = null!;
        public GItem T2Quiver { get; init; } = null!;
        public GItem T2Stramulet { get; init; } = null!;
        public GItem T3Bow { get; init; } = null!;
        public GItem Talkingskull { get; init; } = null!;
        public GItem Test { get; init; } = null!;
        public GItem Test2 { get; init; } = null!;
        [JsonProperty("test_orb")]
        public GItem TestOrb { get; init; } = null!;
        public GItem Throwingstars { get; init; } = null!;
        public GItem Tombkey { get; init; } = null!;
        public GItem Tracker { get; init; } = null!;
        public GItem Trigger { get; init; } = null!;
        public GItem Trinkets { get; init; } = null!;
        public GItem Tristone { get; init; } = null!;
        public GItem Troll { get; init; } = null!;
        public GItem Tshell { get; init; } = null!;
        public GItem Tshirt0 { get; init; } = null!;
        public GItem Tshirt1 { get; init; } = null!;
        public GItem Tshirt2 { get; init; } = null!;
        public GItem Tshirt3 { get; init; } = null!;
        public GItem Tshirt4 { get; init; } = null!;
        public GItem Tshirt6 { get; init; } = null!;
        public GItem Tshirt7 { get; init; } = null!;
        public GItem Tshirt8 { get; init; } = null!;
        public GItem Tshirt88 { get; init; } = null!;
        public GItem Tshirt9 { get; init; } = null!;
        public GItem Ukey { get; init; } = null!;
        public GItem Vattire { get; init; } = null!;
        public GItem Vblood { get; init; } = null!;
        public GItem Vboots { get; init; } = null!;
        public GItem Vcape { get; init; } = null!;
        public GItem Vdagger { get; init; } = null!;
        public GItem Vgloves { get; init; } = null!;
        public GItem Vhammer { get; init; } = null!;
        public GItem Vitearring { get; init; } = null!;
        public GItem Vitring { get; init; } = null!;
        public GItem Vitscroll { get; init; } = null!;
        public GItem Vorb { get; init; } = null!;
        public GItem Vring { get; init; } = null!;
        public GItem Vstaff { get; init; } = null!;
        public GItem Vsword { get; init; } = null!;
        public GItem Wand { get; init; } = null!;
        public GItem Warmscarf { get; init; } = null!;
        public GItem Warpvest { get; init; } = null!;
        public GItem Watercore { get; init; } = null!;
        public GItem Wattire { get; init; } = null!;
        public GItem Wbasher { get; init; } = null!;
        public GItem Wblade { get; init; } = null!;
        public GItem Wbook0 { get; init; } = null!;
        public GItem Wbook1 { get; init; } = null!;
        public GItem Wbreeches { get; init; } = null!;
        public GItem Wcap { get; init; } = null!;
        public GItem Weaponbox { get; init; } = null!;
        public GItem Weaver { get; init; } = null!;
        public GItem Wgloves { get; init; } = null!;
        public GItem Whiskey { get; init; } = null!;
        public GItem Whiteegg { get; init; } = null!;
        public GItem Wine { get; init; } = null!;
        public GItem Wingedboots { get; init; } = null!;
        public GItem Woodensword { get; init; } = null!;
        public GItem Wshield { get; init; } = null!;
        public GItem Wshoes { get; init; } = null!;
        public GItem X0 { get; init; } = null!;
        public GItem X1 { get; init; } = null!;
        public GItem X2 { get; init; } = null!;
        public GItem X3 { get; init; } = null!;
        public GItem X4 { get; init; } = null!;
        public GItem X5 { get; init; } = null!;
        public GItem X6 { get; init; } = null!;
        public GItem X7 { get; init; } = null!;
        public GItem X8 { get; init; } = null!;
        public GItem Xarmor { get; init; } = null!;
        public GItem Xboots { get; init; } = null!;
        public GItem Xbox { get; init; } = null!;
        public GItem Xgloves { get; init; } = null!;
        public GItem Xhelmet { get; init; } = null!;
        public GItem Xmace { get; init; } = null!;
        public GItem Xmashat { get; init; } = null!;
        public GItem Xmaspants { get; init; } = null!;
        public GItem Xmasshoes { get; init; } = null!;
        public GItem Xmassweater { get; init; } = null!;
        public GItem Xpants { get; init; } = null!;
        public GItem Xpbooster { get; init; } = null!;
        public GItem Xpscroll { get; init; } = null!;
        public GItem Xptome { get; init; } = null!;
        public GItem Xshield { get; init; } = null!;
        public GItem Xshot { get; init; } = null!;
        public GItem Zapper { get; init; } = null!;

        internal override void BuildLookupTable()
        {
            base.BuildLookupTable();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var item) in this.Reverse().DistinctBy(kvp => kvp.Value.Name))
                item.Accessor = accessor;
        }
    }
}