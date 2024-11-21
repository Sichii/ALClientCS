#region
using System.Linq;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Items;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
[JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GItem>))]
public class ItemsDatum : DatumBase<GItem>
{
    [JsonProperty("5bucks")]
    public GItem _5Bucks { get; init; } = null!;

    [JsonProperty("ale")]
    public GItem Ale { get; init; } = null!;

    [JsonProperty("amuletofm")]
    public GItem Amuletofm { get; init; } = null!;

    [JsonProperty("angelwings")]
    public GItem Angelwings { get; init; } = null!;

    [JsonProperty("apiercingscroll")]
    public GItem Apiercingscroll { get; init; } = null!;

    [JsonProperty("apologybox")]
    public GItem Apologybox { get; init; } = null!;

    [JsonProperty("armorbox")]
    public GItem Armorbox { get; init; } = null!;

    [JsonProperty("armorring")]
    public GItem Armorring { get; init; } = null!;

    [JsonProperty("armorscroll")]
    public GItem Armorscroll { get; init; } = null!;

    [JsonProperty("ascale")]
    public GItem Ascale { get; init; } = null!;

    [JsonProperty("axe3")]
    public GItem Axe3 { get; init; } = null!;

    [JsonProperty("bandages")]
    public GItem Bandages { get; init; } = null!;

    [JsonProperty("basher")]
    public GItem Basher { get; init; } = null!;

    [JsonProperty("basketofeggs")]
    public GItem Basketofeggs { get; init; } = null!;

    [JsonProperty("bataxe")]
    public GItem Bataxe { get; init; } = null!;

    [JsonProperty("bcandle")]
    public GItem Bcandle { get; init; } = null!;

    [JsonProperty("bcape")]
    public GItem Bcape { get; init; } = null!;

    [JsonProperty("beewings")]
    public GItem Beewings { get; init; } = null!;

    [JsonProperty("bfang")]
    public GItem Bfang { get; init; } = null!;

    [JsonProperty("bfangamulet")]
    public GItem Bfangamulet { get; init; } = null!;

    [JsonProperty("bfur")]
    public GItem Bfur { get; init; } = null!;

    [JsonProperty("bkey")]
    public GItem Bkey { get; init; } = null!;

    [JsonProperty("blade")]
    public GItem Blade { get; init; } = null!;

    [JsonProperty("blue")]
    public GItem Blue { get; init; } = null!;

    [JsonProperty("bottleofxp")]
    public GItem Bottleofxp { get; init; } = null!;

    [JsonProperty("bow")]
    public GItem Bow { get; init; } = null!;

    [JsonProperty("bow4")]
    public GItem Bow4 { get; init; } = null!;

    [JsonProperty("bowofthedead")]
    public GItem Bowofthedead { get; init; } = null!;

    [JsonProperty("bronzeingot")]
    public GItem Bronzeingot { get; init; } = null!;

    [JsonProperty("bronzenugget")]
    public GItem Bronzenugget { get; init; } = null!;

    [JsonProperty("broom")]
    public GItem Broom { get; init; } = null!;

    [JsonProperty("brownegg")]
    public GItem Brownegg { get; init; } = null!;

    [JsonProperty("btusk")]
    public GItem Btusk { get; init; } = null!;

    [JsonProperty("bugbountybox")]
    public GItem Bugbountybox { get; init; } = null!;

    [JsonProperty("bunnyears")]
    public GItem Bunnyears { get; init; } = null!;

    [JsonProperty("bunnyelixir")]
    public GItem Bunnyelixir { get; init; } = null!;

    [JsonProperty("bwing")]
    public GItem Bwing { get; init; } = null!;

    [JsonProperty("cake")]
    public GItem Cake { get; init; } = null!;

    [JsonProperty("candy0")]
    public GItem Candy0 { get; init; } = null!;

    [JsonProperty("candy0v2")]
    public GItem Candy0V2 { get; init; } = null!;

    [JsonProperty("candy0v3")]
    public GItem Candy0V3 { get; init; } = null!;

    [JsonProperty("candy1")]
    public GItem Candy1 { get; init; } = null!;

    [JsonProperty("candy1v2")]
    public GItem Candy1V2 { get; init; } = null!;

    [JsonProperty("candy1v3")]
    public GItem Candy1V3 { get; init; } = null!;

    [JsonProperty("candycane")]
    public GItem Candycane { get; init; } = null!;

    [JsonProperty("candycanesword")]
    public GItem Candycanesword { get; init; } = null!;

    [JsonProperty("candypop")]
    public GItem Candypop { get; init; } = null!;

    [JsonProperty("cape")]
    public GItem Cape { get; init; } = null!;

    [JsonProperty("carrot")]
    public GItem Carrot { get; init; } = null!;

    [JsonProperty("carrotsword")]
    public GItem Carrotsword { get; init; } = null!;

    [JsonProperty("cclaw")]
    public GItem Cclaw { get; init; } = null!;

    [JsonProperty("cdarktristone")]
    public GItem Cdarktristone { get; init; } = null!;

    [JsonProperty("cdragon")]
    public GItem Cdragon { get; init; } = null!;

    [JsonProperty("cearring")]
    public GItem Cearring { get; init; } = null!;

    [JsonProperty("charmer")]
    public GItem Charmer { get; init; } = null!;

    [JsonProperty("chrysalis0")]
    public GItem Chrysalis0 { get; init; } = null!;

    [JsonProperty("claw")]
    public GItem Claw { get; init; } = null!;

    [JsonProperty("coal")]
    public GItem Coal { get; init; } = null!;

    [JsonProperty("coat")]
    public GItem Coat { get; init; } = null!;

    [JsonProperty("coat1")]
    public GItem Coat1 { get; init; } = null!;

    [JsonProperty("cocoon")]
    public GItem Cocoon { get; init; } = null!;

    [JsonProperty("computer")]
    public GItem Computer { get; init; } = null!;

    [JsonProperty("confetti")]
    public GItem Confetti { get; init; } = null!;

    [JsonProperty("cosmo0")]
    public GItem Cosmo0 { get; init; } = null!;

    [JsonProperty("cosmo1")]
    public GItem Cosmo1 { get; init; } = null!;

    [JsonProperty("cosmo2")]
    public GItem Cosmo2 { get; init; } = null!;

    [JsonProperty("cosmo3")]
    public GItem Cosmo3 { get; init; } = null!;

    [JsonProperty("cosmo4")]
    public GItem Cosmo4 { get; init; } = null!;

    [JsonProperty("crabclaw")]
    public GItem Crabclaw { get; init; } = null!;

    [JsonProperty("cring")]
    public GItem Cring { get; init; } = null!;

    [JsonProperty("critscroll")]
    public GItem Critscroll { get; init; } = null!;

    [JsonProperty("crossbow")]
    public GItem Crossbow { get; init; } = null!;

    [JsonProperty("cryptkey")]
    public GItem Cryptkey { get; init; } = null!;

    [JsonProperty("cscale")]
    public GItem Cscale { get; init; } = null!;

    [JsonProperty("cscroll0")]
    public GItem Cscroll0 { get; init; } = null!;

    [JsonProperty("cscroll1")]
    public GItem Cscroll1 { get; init; } = null!;

    [JsonProperty("cscroll2")]
    public GItem Cscroll2 { get; init; } = null!;

    [JsonProperty("cscroll3")]
    public GItem Cscroll3 { get; init; } = null!;

    [JsonProperty("cshell")]
    public GItem Cshell { get; init; } = null!;

    [JsonProperty("ctristone")]
    public GItem Ctristone { get; init; } = null!;

    [JsonProperty("cupid")]
    public GItem Cupid { get; init; } = null!;

    [JsonProperty("cxjar")]
    public GItem Cxjar { get; init; } = null!;

    [JsonProperty("cyber")]
    public GItem Cyber { get; init; } = null!;

    [JsonProperty("dagger")]
    public GItem Dagger { get; init; } = null!;

    [JsonProperty("daggerofthedead")]
    public GItem Daggerofthedead { get; init; } = null!;

    [JsonProperty("darktristone")]
    public GItem Darktristone { get; init; } = null!;

    [JsonProperty("dartgun")]
    public GItem Dartgun { get; init; } = null!;

    [JsonProperty("dexamulet")]
    public GItem Dexamulet { get; init; } = null!;

    [JsonProperty("dexbelt")]
    public GItem Dexbelt { get; init; } = null!;

    [JsonProperty("dexearring")]
    public GItem Dexearring { get; init; } = null!;

    [JsonProperty("dexearringx")]
    public GItem Dexearringx { get; init; } = null!;

    [JsonProperty("dexring")]
    public GItem Dexring { get; init; } = null!;

    [JsonProperty("dexscroll")]
    public GItem Dexscroll { get; init; } = null!;

    [JsonProperty("dkey")]
    public GItem Dkey { get; init; } = null!;

    [JsonProperty("dragondagger")]
    public GItem Dragondagger { get; init; } = null!;

    [JsonProperty("drapes")]
    public GItem Drapes { get; init; } = null!;

    [JsonProperty("dreturnscroll")]
    public GItem Dreturnscroll { get; init; } = null!;

    [JsonProperty("dstones")]
    public GItem Dstones { get; init; } = null!;

    [JsonProperty("ecape")]
    public GItem Ecape { get; init; } = null!;

    [JsonProperty("ectoplasm")]
    public GItem Ectoplasm { get; init; } = null!;

    [JsonProperty("eears")]
    public GItem Eears { get; init; } = null!;

    [JsonProperty("egg0")]
    public GItem Egg0 { get; init; } = null!;

    [JsonProperty("egg1")]
    public GItem Egg1 { get; init; } = null!;

    [JsonProperty("egg2")]
    public GItem Egg2 { get; init; } = null!;

    [JsonProperty("egg3")]
    public GItem Egg3 { get; init; } = null!;

    [JsonProperty("egg4")]
    public GItem Egg4 { get; init; } = null!;

    [JsonProperty("egg5")]
    public GItem Egg5 { get; init; } = null!;

    [JsonProperty("egg6")]
    public GItem Egg6 { get; init; } = null!;

    [JsonProperty("egg7")]
    public GItem Egg7 { get; init; } = null!;

    [JsonProperty("egg8")]
    public GItem Egg8 { get; init; } = null!;

    [JsonProperty("eggnog")]
    public GItem Eggnog { get; init; } = null!;

    [JsonProperty("electronics")]
    public GItem Electronics { get; init; } = null!;

    [JsonProperty("elixirdex0")]
    public GItem Elixirdex0 { get; init; } = null!;

    [JsonProperty("elixirdex1")]
    public GItem Elixirdex1 { get; init; } = null!;

    [JsonProperty("elixirdex2")]
    public GItem Elixirdex2 { get; init; } = null!;

    [JsonProperty("elixirfires")]
    public GItem Elixirfireres { get; init; } = null!;

    [JsonProperty("elixirfzres")]
    public GItem Elixirfreezeres { get; init; } = null!;

    [JsonProperty("elixirint0")]
    public GItem Elixirint0 { get; init; } = null!;

    [JsonProperty("elixirint1")]
    public GItem Elixirint1 { get; init; } = null!;

    [JsonProperty("elixirint2")]
    public GItem Elixirint2 { get; init; } = null!;

    [JsonProperty("elixirluck")]
    public GItem Elixirluck { get; init; } = null!;

    [JsonProperty("elixirpnres")]
    public GItem Elixirpnres { get; init; } = null!;

    [JsonProperty("elixirstr0")]
    public GItem Elixirstr0 { get; init; } = null!;

    [JsonProperty("elixirstr1")]
    public GItem Elixirstr1 { get; init; } = null!;

    [JsonProperty("elixirstr2")]
    public GItem Elixirstr2 { get; init; } = null!;

    [JsonProperty("elixirvit0")]
    public GItem Elixirvit0 { get; init; } = null!;

    [JsonProperty("elixirvit1")]
    public GItem Elixirvit1 { get; init; } = null!;

    [JsonProperty("elixirvit2")]
    public GItem Elixirvit2 { get; init; } = null!;

    [JsonProperty("emotionjar")]
    public GItem Emotionjar { get; init; } = null!;

    [JsonProperty("emptyheart")]
    public GItem Emptyheart { get; init; } = null!;

    [JsonProperty("emptyjar")]
    public GItem Emptyjar { get; init; } = null!;

    [JsonProperty("epyjamas")]
    public GItem Epyjamas { get; init; } = null!;

    [JsonProperty("eslippers")]
    public GItem Eslippers { get; init; } = null!;

    [JsonProperty("espresso")]
    public GItem Espresso { get; init; } = null!;

    [JsonProperty("essenceofether")]
    public GItem Essenceofether { get; init; } = null!;

    [JsonProperty("essenceoffire")]
    public GItem Essenceoffire { get; init; } = null!;

    [JsonProperty("essenceoffrost")]
    public GItem Essenceoffrost { get; init; } = null!;

    [JsonProperty("essenceofgreed")]
    public GItem Essenceofgreed { get; init; } = null!;

    [JsonProperty("essenceoflife")]
    public GItem Essenceoflife { get; init; } = null!;

    [JsonProperty("essenceofnature")]
    public GItem Essenceofnature { get; init; } = null!;

    [JsonProperty("evasionscroll")]
    public GItem Evasionscroll { get; init; } = null!;

    [JsonProperty("exoarm")]
    public GItem Exoarm { get; init; } = null!;

    [JsonProperty("fallen")]
    public GItem Fallen { get; init; } = null!;

    [JsonProperty("fcape")]
    public GItem Fcape { get; init; } = null!;

    [JsonProperty("fclaw")]
    public GItem Fclaw { get; init; } = null!;

    [JsonProperty("feather0")]
    public GItem Feather0 { get; init; } = null!;

    [JsonProperty("feather1")]
    public GItem Feather1 { get; init; } = null!;

    [JsonProperty("fieldgen0")]
    public GItem Fieldgen0 { get; init; } = null!;

    [JsonProperty("fierygloves")]
    public GItem Fierygloves { get; init; } = null!;

    [JsonProperty("figurine")]
    public GItem Figurine { get; init; } = null!;

    [JsonProperty("fireblade")]
    public GItem Fireblade { get; init; } = null!;

    [JsonProperty("firebow")]
    public GItem Firebow { get; init; } = null!;

    [JsonProperty("firecrackers")]
    public GItem Firecrackers { get; init; } = null!;

    [JsonProperty("firestaff")]
    public GItem Firestaff { get; init; } = null!;

    [JsonProperty("firestars")]
    public GItem Firestars { get; init; } = null!;

    [JsonProperty("flute")]
    public GItem Flute { get; init; } = null!;

    [JsonProperty("forscroll")]
    public GItem Forscroll { get; init; } = null!;

    [JsonProperty("frankypants")]
    public GItem Frankypants { get; init; } = null!;

    [JsonProperty("frequencyscroll")]
    public GItem Frequencyscroll { get; init; } = null!;

    [JsonProperty("friendtoken")]
    public GItem Friendtoken { get; init; } = null!;

    [JsonProperty("frogt")]
    public GItem Frogt { get; init; } = null!;

    [JsonProperty("frostbow")]
    public GItem Frostbow { get; init; } = null!;

    [JsonProperty("froststaff")]
    public GItem Froststaff { get; init; } = null!;

    [JsonProperty("frozenkey")]
    public GItem Frozenkey { get; init; } = null!;

    [JsonProperty("frozenstone")]
    public GItem Frozenstone { get; init; } = null!;

    [JsonProperty("fsword")]
    public GItem Fsword { get; init; } = null!;

    [JsonProperty("ftrinket")]
    public GItem Ftrinket { get; init; } = null!;

    [JsonProperty("funtoken")]
    public GItem Funtoken { get; init; } = null!;

    [JsonProperty("fury")]
    public GItem Fury { get; init; } = null!;

    [JsonProperty("gbow")]
    public GItem Gbow { get; init; } = null!;

    [JsonProperty("gcape")]
    public GItem Gcape { get; init; } = null!;

    [JsonProperty("gem0")]
    public GItem Gem0 { get; init; } = null!;

    [JsonProperty("gem1")]
    public GItem Gem1 { get; init; } = null!;

    [JsonProperty("gem2")]
    public GItem Gem2 { get; init; } = null!;

    [JsonProperty("gem3")]
    public GItem Gem3 { get; init; } = null!;

    [JsonProperty("gemfragment")]
    public GItem Gemfragment { get; init; } = null!;

    [JsonProperty("ghatb")]
    public GItem Ghatb { get; init; } = null!;

    [JsonProperty("ghatp")]
    public GItem Ghatp { get; init; } = null!;

    [JsonProperty("gift0")]
    public GItem Gift0 { get; init; } = null!;

    [JsonProperty("gift1")]
    public GItem Gift1 { get; init; } = null!;

    [JsonProperty("glitch")]
    public GItem Glitch { get; init; } = null!;

    [JsonProperty("glolipop")]
    public GItem Glolipop { get; init; } = null!;

    [JsonProperty("gloves")]
    public GItem Gloves { get; init; } = null!;

    [JsonProperty("gloves1")]
    public GItem Gloves1 { get; init; } = null!;

    [JsonProperty("goldbooster")]
    public GItem Goldbooster { get; init; } = null!;

    [JsonProperty("goldenegg")]
    public GItem Goldenegg { get; init; } = null!;

    [JsonProperty("goldenpowerglove")]
    public GItem Goldenpowerglove { get; init; } = null!;

    [JsonProperty("goldingot")]
    public GItem Goldingot { get; init; } = null!;

    [JsonProperty("goldnugget")]
    public GItem Goldnugget { get; init; } = null!;

    [JsonProperty("goldring")]
    public GItem Goldring { get; init; } = null!;

    [JsonProperty("goldscroll")]
    public GItem Goldscroll { get; init; } = null!;

    [JsonProperty("gphelmet")]
    public GItem Gphelmet { get; init; } = null!;

    [JsonProperty("greenbomb")]
    public GItem Greenbomb { get; init; } = null!;

    [JsonProperty("greenenvelope")]
    public GItem Greenenvelope { get; init; } = null!;

    [JsonProperty("gslime")]
    public GItem Gslime { get; init; } = null!;

    [JsonProperty("gstaff")]
    public GItem Gstaff { get; init; } = null!;

    [JsonProperty("gum")]
    public GItem Gum { get; init; } = null!;

    [JsonProperty("hammer")]
    public GItem Hammer { get; init; } = null!;

    [JsonProperty("handofmidas")]
    public GItem Handofmidas { get; init; } = null!;

    [JsonProperty("harbringer")]
    public GItem Harbringer { get; init; } = null!;

    [JsonProperty("harmor")]
    public GItem Harmor { get; init; } = null!;

    [JsonProperty("harpybow")]
    public GItem Harpybow { get; init; } = null!;

    [JsonProperty("hboots")]
    public GItem Hboots { get; init; } = null!;

    [JsonProperty("hbow")]
    public GItem Hbow { get; init; } = null!;

    [JsonProperty("hdagger")]
    public GItem Hdagger { get; init; } = null!;

    [JsonProperty("heartwood")]
    public GItem Heartwood { get; init; } = null!;

    [JsonProperty("helmet")]
    public GItem Helmet { get; init; } = null!;

    [JsonProperty("helmet1")]
    public GItem Helmet1 { get; init; } = null!;

    [JsonProperty("hgloves")]
    public GItem Hgloves { get; init; } = null!;

    [JsonProperty("hhelmet")]
    public GItem Hhelmet { get; init; } = null!;

    [JsonProperty("hotchocolate")]
    public GItem Hotchocolate { get; init; } = null!;

    [JsonProperty("hpamulet")]
    public GItem Hpamulet { get; init; } = null!;

    [JsonProperty("hpants")]
    public GItem Hpants { get; init; } = null!;

    [JsonProperty("hpbelt")]
    public GItem Hpbelt { get; init; } = null!;

    [JsonProperty("hpot0")]
    public GItem Hpot0 { get; init; } = null!;

    [JsonProperty("hpot1")]
    public GItem Hpot1 { get; init; } = null!;

    [JsonProperty("hpotx")]
    public GItem Hpotx { get; init; } = null!;

    [JsonProperty("iceskates")]
    public GItem Iceskates { get; init; } = null!;

    [JsonProperty("ijx")]
    public GItem Ijx { get; init; } = null!;

    [JsonProperty("ink")]
    public GItem Ink { get; init; } = null!;

    [JsonProperty("intamulet")]
    public GItem Intamulet { get; init; } = null!;

    [JsonProperty("intbelt")]
    public GItem Intbelt { get; init; } = null!;

    [JsonProperty("intearring")]
    public GItem Intearring { get; init; } = null!;

    [JsonProperty("intring")]
    public GItem Intring { get; init; } = null!;

    [JsonProperty("intscroll")]
    public GItem Intscroll { get; init; } = null!;

    [JsonProperty("jacko")]
    public GItem Jacko { get; init; } = null!;

    [JsonProperty("jewellerybox")]
    public GItem Jewellerybox { get; init; } = null!;

    [JsonProperty("kitty1")]
    public GItem Kitty1 { get; init; } = null!;

    [JsonProperty("lantern")]
    public GItem Lantern { get; init; } = null!;

    [JsonProperty("lbelt")]
    public GItem Lbelt { get; init; } = null!;

    [JsonProperty("leather")]
    public GItem Leather { get; init; } = null!;

    [JsonProperty("ledger")]
    public GItem Ledger { get; init; } = null!;

    [JsonProperty("licence")]
    public GItem Licence { get; init; } = null!;

    [JsonProperty("lifestealscroll")]
    public GItem Lifestealscroll { get; init; } = null!;

    [JsonProperty("lmace")]
    public GItem Lmace { get; init; } = null!;

    [JsonProperty("lostearring")]
    public GItem Lostearring { get; init; } = null!;

    [JsonProperty("lotusf")]
    public GItem Lotusf { get; init; } = null!;

    [JsonProperty("lspores")]
    public GItem Lspores { get; init; } = null!;

    [JsonProperty("luckbooster")]
    public GItem Luckbooster { get; init; } = null!;

    [JsonProperty("luckscroll")]
    public GItem Luckscroll { get; init; } = null!;

    [JsonProperty("luckyt")]
    public GItem Luckyt { get; init; } = null!;

    [JsonProperty("mace")]
    public GItem Mace { get; init; } = null!;

    [JsonProperty("maceofthedead")]
    public GItem Maceofthedead { get; init; } = null!;

    [JsonProperty("mageshood")]
    public GItem Mageshood { get; init; } = null!;

    [JsonProperty("manastealscroll")]
    public GItem Manastealscroll { get; init; } = null!;

    [JsonProperty("mbelt")]
    public GItem Mbelt { get; init; } = null!;

    [JsonProperty("mbones")]
    public GItem Mbones { get; init; } = null!;

    [JsonProperty("mcape")]
    public GItem Mcape { get; init; } = null!;

    [JsonProperty("mcarmor")]
    public GItem Mcarmor { get; init; } = null!;

    [JsonProperty("mcboots")]
    public GItem Mcboots { get; init; } = null!;

    [JsonProperty("mcgloves")]
    public GItem Mcgloves { get; init; } = null!;

    [JsonProperty("mchat")]
    public GItem Mchat { get; init; } = null!;

    [JsonProperty("mcpants")]
    public GItem Mcpants { get; init; } = null!;

    [JsonProperty("mearring")]
    public GItem Mearring { get; init; } = null!;

    [JsonProperty("merry")]
    public GItem Merry { get; init; } = null!;

    [JsonProperty("mistletoe")]
    public GItem Mistletoe { get; init; } = null!;

    [JsonProperty("mittens")]
    public GItem Mittens { get; init; } = null!;

    [JsonProperty("mmarmor")]
    public GItem Mmarmor { get; init; } = null!;

    [JsonProperty("mmgloves")]
    public GItem Mmgloves { get; init; } = null!;

    [JsonProperty("mmhat")]
    public GItem Mmhat { get; init; } = null!;

    [JsonProperty("mmpants")]
    public GItem Mmpants { get; init; } = null!;

    [JsonProperty("mmshoes")]
    public GItem Mmshoes { get; init; } = null!;

    [JsonProperty("molesteeth")]
    public GItem Molesteeth { get; init; } = null!;

    [JsonProperty("monsterbox")]
    public GItem Monsterbox { get; init; } = null!;

    [JsonProperty("monstertoken")]
    public GItem Monstertoken { get; init; } = null!;

    [JsonProperty("mparmor")]
    public GItem Mparmor { get; init; } = null!;

    [JsonProperty("mpcostscroll")]
    public GItem Mpcostscroll { get; init; } = null!;

    [JsonProperty("mpgloves")]
    public GItem Mpgloves { get; init; } = null!;

    [JsonProperty("mphat")]
    public GItem Mphat { get; init; } = null!;

    [JsonProperty("mpot0")]
    public GItem Mpot0 { get; init; } = null!;

    [JsonProperty("mpot1")]
    public GItem Mpot1 { get; init; } = null!;

    [JsonProperty("mpotx")]
    public GItem Mpotx { get; init; } = null!;

    [JsonProperty("mppants")]
    public GItem Mppants { get; init; } = null!;

    [JsonProperty("mpshoes")]
    public GItem Mpshoes { get; init; } = null!;

    [JsonProperty("mpxamulet")]
    public GItem Mpxamulet { get; init; } = null!;

    [JsonProperty("mpxbelt")]
    public GItem Mpxbelt { get; init; } = null!;

    [JsonProperty("mpxgloves")]
    public GItem Mpxgloves { get; init; } = null!;

    [JsonProperty("mrarmor")]
    public GItem Mrarmor { get; init; } = null!;

    [JsonProperty("mrboots")]
    public GItem Mrboots { get; init; } = null!;

    [JsonProperty("mrgloves")]
    public GItem Mrgloves { get; init; } = null!;

    [JsonProperty("mrhood")]
    public GItem Mrhood { get; init; } = null!;

    [JsonProperty("mrnarmor")]
    public GItem Mrnarmor { get; init; } = null!;

    [JsonProperty("mrnboots")]
    public GItem Mrnboots { get; init; } = null!;

    [JsonProperty("mrngloves")]
    public GItem Mrngloves { get; init; } = null!;

    [JsonProperty("mrnhat")]
    public GItem Mrnhat { get; init; } = null!;

    [JsonProperty("mrnpants")]
    public GItem Mrnpants { get; init; } = null!;

    [JsonProperty("mrpants")]
    public GItem Mrpants { get; init; } = null!;

    [JsonProperty("mshield")]
    public GItem Mshield { get; init; } = null!;

    [JsonProperty("mushroomstaff")]
    public GItem Mushroomstaff { get; init; } = null!;

    [JsonProperty("mwarmor")]
    public GItem Mwarmor { get; init; } = null!;

    [JsonProperty("mwboots")]
    public GItem Mwboots { get; init; } = null!;

    [JsonProperty("mwgloves")]
    public GItem Mwgloves { get; init; } = null!;

    [JsonProperty("mwhelmet")]
    public GItem Mwhelmet { get; init; } = null!;

    [JsonProperty("mwpants")]
    public GItem Mwpants { get; init; } = null!;

    [JsonProperty("mysterybox")]
    public GItem Mysterybox { get; init; } = null!;

    [JsonProperty("networkcard")]
    public GItem Networkcard { get; init; } = null!;

    [JsonProperty("nheart")]
    public GItem Nheart { get; init; } = null!;

    [JsonProperty("northstar")]
    public GItem Northstar { get; init; } = null!;

    [JsonProperty("offering")]
    public GItem Offering { get; init; } = null!;

    [JsonProperty("offeringp")]
    public GItem Offeringp { get; init; } = null!;

    [JsonProperty("offeringx")]
    public GItem Offeringx { get; init; } = null!;

    [JsonProperty("ololipop")]
    public GItem Ololipop { get; init; } = null!;

    [JsonProperty("oozingterror")]
    public GItem Oozingterror { get; init; } = null!;

    [JsonProperty("orbg")]
    public GItem Orbg { get; init; } = null!;

    [JsonProperty("orbofdex")]
    public GItem Orbofdex { get; init; } = null!;

    [JsonProperty("orbofint")]
    public GItem Orbofint { get; init; } = null!;

    [JsonProperty("orbofsc")]
    public GItem Orbofsc { get; init; } = null!;

    [JsonProperty("orbofstr")]
    public GItem Orbofstr { get; init; } = null!;

    [JsonProperty("orbofvit")]
    public GItem Orbofvit { get; init; } = null!;

    [JsonProperty("ornament")]
    public GItem Ornament { get; init; } = null!;

    [JsonProperty("ornamentstaff")]
    public GItem Ornamentstaff { get; init; } = null!;

    [JsonProperty("outputscroll")]
    public GItem Outputscroll { get; init; } = null!;

    [JsonProperty("oxhelmet")]
    public GItem Oxhelmet { get; init; } = null!;

    [JsonProperty("pants")]
    public GItem Pants { get; init; } = null!;

    [JsonProperty("pants1")]
    public GItem Pants1 { get; init; } = null!;

    [JsonProperty("partyhat")]
    public GItem Partyhat { get; init; } = null!;

    [JsonProperty("phelmet")]
    public GItem Phelmet { get; init; } = null!;

    [JsonProperty("pickaxe")]
    public GItem Pickaxe { get; init; } = null!;

    [JsonProperty("pico")]
    public GItem Pico { get; init; } = null!;

    [JsonProperty("pinkie")]
    public GItem Pinkie { get; init; } = null!;

    [JsonProperty("placeholder")]
    public GItem Placeholder { get; init; } = null!;

    [JsonProperty("placeholder_m")]
    public GItem PlaceholderM { get; init; } = null!;

    [JsonProperty("platinumingot")]
    public GItem Platinumingot { get; init; } = null!;

    [JsonProperty("platinumnugget")]
    public GItem Platinumnugget { get; init; } = null!;

    [JsonProperty("pleather")]
    public GItem Pleather { get; init; } = null!;

    [JsonProperty("pmace")]
    public GItem Pmace { get; init; } = null!;

    [JsonProperty("pmaceofthedead")]
    public GItem Pmaceofthedead { get; init; } = null!;

    [JsonProperty("poison")]
    public GItem Poison { get; init; } = null!;

    [JsonProperty("poker")]
    public GItem Poker { get; init; } = null!;

    [JsonProperty("pouchbow")]
    public GItem Pouchbow { get; init; } = null!;

    [JsonProperty("powerglove")]
    public GItem Powerglove { get; init; } = null!;

    [JsonProperty("pstem")]
    public GItem Pstem { get; init; } = null!;

    [JsonProperty("pumpkinspice")]
    public GItem Pumpkinspice { get; init; } = null!;

    [JsonProperty("puppy1")]
    public GItem Puppy1 { get; init; } = null!;

    [JsonProperty("puppyer")]
    public GItem Puppyer { get; init; } = null!;

    [JsonProperty("pvptoken")]
    public GItem Pvptoken { get; init; } = null!;

    [JsonProperty("pyjamas")]
    public GItem Pyjamas { get; init; } = null!;

    [JsonProperty("qubics")]
    public GItem Qubics { get; init; } = null!;

    [JsonProperty("quiver")]
    public GItem Quiver { get; init; } = null!;

    [JsonProperty("rabbitsfoot")]
    public GItem Rabbitsfoot { get; init; } = null!;

    [JsonProperty("rapier")]
    public GItem Rapier { get; init; } = null!;

    [JsonProperty("rattail")]
    public GItem Rattail { get; init; } = null!;

    [JsonProperty("redenvelope")]
    public GItem Redenvelope { get; init; } = null!;

    [JsonProperty("redenvelopev2")]
    public GItem Redenvelopev2 { get; init; } = null!;

    [JsonProperty("redenvelopev3")]
    public GItem Redenvelopev3 { get; init; } = null!;

    [JsonProperty("redenvelopev4")]
    public GItem Redenvelopev4 { get; init; } = null!;

    [JsonProperty("rednose")]
    public GItem Rednose { get; init; } = null!;

    [JsonProperty("reflectionscroll")]
    public GItem Reflectionscroll { get; init; } = null!;

    [JsonProperty("resistancering")]
    public GItem Resistancering { get; init; } = null!;

    [JsonProperty("resistancescroll")]
    public GItem Resistancescroll { get; init; } = null!;

    [JsonProperty("rfangs")]
    public GItem Rfangs { get; init; } = null!;

    [JsonProperty("rfur")]
    public GItem Rfur { get; init; } = null!;

    [JsonProperty("ringhs")]
    public GItem Ringhs { get; init; } = null!;

    [JsonProperty("ringofluck")]
    public GItem Ringofluck { get; init; } = null!;

    [JsonProperty("ringsj")]
    public GItem Ringsj { get; init; } = null!;

    [JsonProperty("rod")]
    public GItem Rod { get; init; } = null!;

    [JsonProperty("rpiercingscroll")]
    public GItem Rpiercingscroll { get; init; } = null!;

    [JsonProperty("sanguine")]
    public GItem Sanguine { get; init; } = null!;

    [JsonProperty("santasbelt")]
    public GItem Santasbelt { get; init; } = null!;

    [JsonProperty("sbelt")]
    public GItem Sbelt { get; init; } = null!;

    [JsonProperty("scroll0")]
    public GItem Scroll0 { get; init; } = null!;

    [JsonProperty("scroll1")]
    public GItem Scroll1 { get; init; } = null!;

    [JsonProperty("scroll2")]
    public GItem Scroll2 { get; init; } = null!;

    [JsonProperty("scroll3")]
    public GItem Scroll3 { get; init; } = null!;

    [JsonProperty("scroll4")]
    public GItem Scroll4 { get; init; } = null!;

    [JsonProperty("scythe")]
    public GItem Scythe { get; init; } = null!;

    [JsonProperty("seashell")]
    public GItem Seashell { get; init; } = null!;

    [JsonProperty("shadowstone")]
    public GItem Shadowstone { get; init; } = null!;

    [JsonProperty("shield")]
    public GItem Shield { get; init; } = null!;

    [JsonProperty("shoes")]
    public GItem Shoes { get; init; } = null!;

    [JsonProperty("shoes1")]
    public GItem Shoes1 { get; init; } = null!;

    [JsonProperty("skullamulet")]
    public GItem Skullamulet { get; init; } = null!;

    [JsonProperty("slimestaff")]
    public GItem Slimestaff { get; init; } = null!;

    [JsonProperty("smoke")]
    public GItem Smoke { get; init; } = null!;

    [JsonProperty("smush")]
    public GItem Smush { get; init; } = null!;

    [JsonProperty("snakefang")]
    public GItem Snakefang { get; init; } = null!;

    [JsonProperty("snakeoil")]
    public GItem Snakeoil { get; init; } = null!;

    [JsonProperty("snowball")]
    public GItem Snowball { get; init; } = null!;

    [JsonProperty("snowboots")]
    public GItem Snowboots { get; init; } = null!;

    [JsonProperty("snowflakes")]
    public GItem Snowflakes { get; init; } = null!;

    [JsonProperty("snring")]
    public GItem Snring { get; init; } = null!;

    [JsonProperty("solitaire")]
    public GItem Solitaire { get; init; } = null!;

    [JsonProperty("spear")]
    public GItem Spear { get; init; } = null!;

    [JsonProperty("spearofthedead")]
    public GItem Spearofthedead { get; init; } = null!;

    [JsonProperty("speedscroll")]
    public GItem Speedscroll { get; init; } = null!;

    [JsonProperty("spidersilk")]
    public GItem Spidersilk { get; init; } = null!;

    [JsonProperty("spookyamulet")]
    public GItem Spookyamulet { get; init; } = null!;

    [JsonProperty("spores")]
    public GItem Spores { get; init; } = null!;

    [JsonProperty("sshield")]
    public GItem Sshield { get; init; } = null!;

    [JsonProperty("sstinger")]
    public GItem Sstinger { get; init; } = null!;

    [JsonProperty("staff")]
    public GItem Staff { get; init; } = null!;

    [JsonProperty("staff2")]
    public GItem Staff2 { get; init; } = null!;

    [JsonProperty("staff3")]
    public GItem Staff3 { get; init; } = null!;

    [JsonProperty("staff4")]
    public GItem Staff4 { get; init; } = null!;

    [JsonProperty("staffofthedead")]
    public GItem Staffofthedead { get; init; } = null!;

    [JsonProperty("stand0")]
    public GItem Stand0 { get; init; } = null!;

    [JsonProperty("stand1")]
    public GItem Stand1 { get; init; } = null!;

    [JsonProperty("starkillers")]
    public GItem Starkillers { get; init; } = null!;

    [JsonProperty("stealthcape")]
    public GItem Stealthcape { get; init; } = null!;

    [JsonProperty("stick")]
    public GItem Stick { get; init; } = null!;

    [JsonProperty("stinger")]
    public GItem Stinger { get; init; } = null!;

    [JsonProperty("stonekey")]
    public GItem Stonekey { get; init; } = null!;

    [JsonProperty("stoneofgold")]
    public GItem Stoneofgold { get; init; } = null!;

    [JsonProperty("stoneofluck")]
    public GItem Stoneofluck { get; init; } = null!;

    [JsonProperty("stoneofxp")]
    public GItem Stoneofxp { get; init; } = null!;

    [JsonProperty("storagebox")]
    public GItem Storagebox { get; init; } = null!;

    [JsonProperty("stramulet")]
    public GItem Stramulet { get; init; } = null!;

    [JsonProperty("strbelt")]
    public GItem Strbelt { get; init; } = null!;

    [JsonProperty("strearring")]
    public GItem Strearring { get; init; } = null!;

    [JsonProperty("strring")]
    public GItem Strring { get; init; } = null!;

    [JsonProperty("strscroll")]
    public GItem Strscroll { get; init; } = null!;

    [JsonProperty("suckerpunch")]
    public GItem Suckerpunch { get; init; } = null!;

    [JsonProperty("supercomputer")]
    public GItem Supercomputer { get; init; } = null!;

    [JsonProperty("supermittens")]
    public GItem Supermittens { get; init; } = null!;

    [JsonProperty("svenom")]
    public GItem Svenom { get; init; } = null!;

    [JsonProperty("sweaterhs")]
    public GItem Sweaterhs { get; init; } = null!;

    [JsonProperty("swifty")]
    public GItem Swifty { get; init; } = null!;

    [JsonProperty("swirlipop")]
    public GItem Swirlipop { get; init; } = null!;

    [JsonProperty("sword")]
    public GItem Sword { get; init; } = null!;

    [JsonProperty("swordofthedead")]
    public GItem Swordofthedead { get; init; } = null!;

    [JsonProperty("t2bow")]
    public GItem T2Bow { get; init; } = null!;

    [JsonProperty("t2dexamulet")]
    public GItem T2Dexamulet { get; init; } = null!;

    [JsonProperty("t2intamulet")]
    public GItem T2Intamulet { get; init; } = null!;

    [JsonProperty("t2quiver")]
    public GItem T2Quiver { get; init; } = null!;

    [JsonProperty("t2stramulet")]
    public GItem T2Stramulet { get; init; } = null!;

    [JsonProperty("t3bow")]
    public GItem T3Bow { get; init; } = null!;

    [JsonProperty("talkingskull")]
    public GItem Talkingskull { get; init; } = null!;

    [JsonProperty("test")]
    public GItem Test { get; init; } = null!;

    [JsonProperty("test2")]
    public GItem Test2 { get; init; } = null!;

    [JsonProperty("test_orb")]
    public GItem TestOrb { get; init; } = null!;

    [JsonProperty("throwingstars")]
    public GItem Throwingstars { get; init; } = null!;

    [JsonProperty("tigercape")]
    public GItem Tigercape { get; init; } = null!;

    [JsonProperty("tigerhelmet")]
    public GItem Tigerhelmet { get; init; } = null!;

    [JsonProperty("tigershield")]
    public GItem Tigershield { get; init; } = null!;

    [JsonProperty("tigerstone")]
    public GItem Tigerstone { get; init; } = null!;

    [JsonProperty("tombkey")]
    public GItem Tombkey { get; init; } = null!;

    [JsonProperty("tracker")]
    public GItem Tracker { get; init; } = null!;

    [JsonProperty("trigger")]
    public GItem Trigger { get; init; } = null!;

    [JsonProperty("trinkets")]
    public GItem Trinkets { get; init; } = null!;

    [JsonProperty("tristone")]
    public GItem Tristone { get; init; } = null!;

    [JsonProperty("troll")]
    public GItem Troll { get; init; } = null!;

    [JsonProperty("tshell")]
    public GItem Tshell { get; init; } = null!;

    [JsonProperty("tshirt0")]
    public GItem Tshirt0 { get; init; } = null!;

    [JsonProperty("tshirt1")]
    public GItem Tshirt1 { get; init; } = null!;

    [JsonProperty("tshirt2")]
    public GItem Tshirt2 { get; init; } = null!;

    [JsonProperty("tshirt3")]
    public GItem Tshirt3 { get; init; } = null!;

    [JsonProperty("tshirt4")]
    public GItem Tshirt4 { get; init; } = null!;

    [JsonProperty("tshirt6")]
    public GItem Tshirt6 { get; init; } = null!;

    [JsonProperty("tshirt7")]
    public GItem Tshirt7 { get; init; } = null!;

    [JsonProperty("tshirt8")]
    public GItem Tshirt8 { get; init; } = null!;

    [JsonProperty("tshirt88")]
    public GItem Tshirt88 { get; init; } = null!;

    [JsonProperty("tshirt9")]
    public GItem Tshirt9 { get; init; } = null!;

    [JsonProperty("ukey")]
    public GItem Ukey { get; init; } = null!;

    [JsonProperty("vattire")]
    public GItem Vattire { get; init; } = null!;

    [JsonProperty("vblood")]
    public GItem Vblood { get; init; } = null!;

    [JsonProperty("vboots")]
    public GItem Vboots { get; init; } = null!;

    [JsonProperty("vcape")]
    public GItem Vcape { get; init; } = null!;

    [JsonProperty("vdagger")]
    public GItem Vdagger { get; init; } = null!;

    [JsonProperty("vgloves")]
    public GItem Vgloves { get; init; } = null!;

    [JsonProperty("vhammer")]
    public GItem Vhammer { get; init; } = null!;

    [JsonProperty("vitearring")]
    public GItem Vitearring { get; init; } = null!;

    [JsonProperty("vitring")]
    public GItem Vitring { get; init; } = null!;

    [JsonProperty("vitscroll")]
    public GItem Vitscroll { get; init; } = null!;

    [JsonProperty("vorb")]
    public GItem Vorb { get; init; } = null!;

    [JsonProperty("vring")]
    public GItem Vring { get; init; } = null!;

    [JsonProperty("vstaff")]
    public GItem Vstaff { get; init; } = null!;

    [JsonProperty("vsword")]
    public GItem Vsword { get; init; } = null!;

    [JsonProperty("wand")]
    public GItem Wand { get; init; } = null!;

    [JsonProperty("warmscarf")]
    public GItem Warmscarf { get; init; } = null!;

    [JsonProperty("warpvest")]
    public GItem Warpvest { get; init; } = null!;

    [JsonProperty("watercore")]
    public GItem Watercore { get; init; } = null!;

    [JsonProperty("wattire")]
    public GItem Wattire { get; init; } = null!;

    [JsonProperty("wbasher")]
    public GItem Wbasher { get; init; } = null!;

    [JsonProperty("wblade")]
    public GItem Wblade { get; init; } = null!;

    [JsonProperty("wbook0")]
    public GItem Wbook0 { get; init; } = null!;

    [JsonProperty("wbook1")]
    public GItem Wbook1 { get; init; } = null!;

    [JsonProperty("wbookhs")]
    public GItem Wbookhs { get; init; } = null!;

    [JsonProperty("wbreeches")]
    public GItem Wbreeches { get; init; } = null!;

    [JsonProperty("wcap")]
    public GItem Wcap { get; init; } = null!;

    [JsonProperty("weaponbox")]
    public GItem Weaponbox { get; init; } = null!;

    [JsonProperty("weaver")]
    public GItem Weaver { get; init; } = null!;

    [JsonProperty("wgloves")]
    public GItem Wgloves { get; init; } = null!;

    [JsonProperty("whiskey")]
    public GItem Whiskey { get; init; } = null!;

    [JsonProperty("whiteegg")]
    public GItem Whiteegg { get; init; } = null!;

    [JsonProperty("wine")]
    public GItem Wine { get; init; } = null!;

    [JsonProperty("wingedboots")]
    public GItem Wingedboots { get; init; } = null!;

    [JsonProperty("woodensword")]
    public GItem Woodensword { get; init; } = null!;

    [JsonProperty("wshield")]
    public GItem Wshield { get; init; } = null!;

    [JsonProperty("wshoes")]
    public GItem Wshoes { get; init; } = null!;

    [JsonProperty("x0")]
    public GItem X0 { get; init; } = null!;

    [JsonProperty("x1")]
    public GItem X1 { get; init; } = null!;

    [JsonProperty("x2")]
    public GItem X2 { get; init; } = null!;

    [JsonProperty("x3")]
    public GItem X3 { get; init; } = null!;

    [JsonProperty("x4")]
    public GItem X4 { get; init; } = null!;

    [JsonProperty("x5")]
    public GItem X5 { get; init; } = null!;

    [JsonProperty("x6")]
    public GItem X6 { get; init; } = null!;

    [JsonProperty("x7")]
    public GItem X7 { get; init; } = null!;

    [JsonProperty("x8")]
    public GItem X8 { get; init; } = null!;

    [JsonProperty("xarmor")]
    public GItem Xarmor { get; init; } = null!;

    [JsonProperty("xboots")]
    public GItem Xboots { get; init; } = null!;

    [JsonProperty("xbox")]
    public GItem Xbox { get; init; } = null!;

    [JsonProperty("xgloves")]
    public GItem Xgloves { get; init; } = null!;

    [JsonProperty("xhelmet")]
    public GItem Xhelmet { get; init; } = null!;

    [JsonProperty("xmace")]
    public GItem Xmace { get; init; } = null!;

    [JsonProperty("xmashat")]
    public GItem Xmashat { get; init; } = null!;

    [JsonProperty("xmaspants")]
    public GItem Xmaspants { get; init; } = null!;

    [JsonProperty("xmasshoes")]
    public GItem Xmasshoes { get; init; } = null!;

    [JsonProperty("xmassweater")]
    public GItem Xmassweater { get; init; } = null!;

    [JsonProperty("xpants")]
    public GItem Xpants { get; init; } = null!;

    [JsonProperty("xpbooster")]
    public GItem Xpbooster { get; init; } = null!;

    [JsonProperty("xpscroll")]
    public GItem Xpscroll { get; init; } = null!;

    [JsonProperty("xptome")]
    public GItem Xptome { get; init; } = null!;

    [JsonProperty("xshield")]
    public GItem Xshield { get; init; } = null!;

    [JsonProperty("xshot")]
    public GItem Xshot { get; init; } = null!;

    [JsonProperty("zapper")]
    public GItem Zapper { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var item) in this.Reverse()
                                                 .DistinctBy(kvp => kvp.Value.Name))
            item.Accessor = accessor;
    }
}