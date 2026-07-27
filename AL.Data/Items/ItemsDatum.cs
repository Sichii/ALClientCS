#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Items;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class ItemsDatum : DatumBase<GItem>
{
    [JsonPropertyName("5bucks")]
    public GItem _5Bucks { get; init; } = null!;

    [JsonPropertyName("ale")]
    public GItem Ale { get; init; } = null!;

    [JsonPropertyName("alloyquiver")]
    public GItem Alloyquiver { get; init; } = null!;

    [JsonPropertyName("amuletofm")]
    public GItem Amuletofm { get; init; } = null!;

    [JsonPropertyName("angelwings")]
    public GItem Angelwings { get; init; } = null!;

    [JsonPropertyName("apiercingscroll")]
    public GItem Apiercingscroll { get; init; } = null!;

    [JsonPropertyName("apologybox")]
    public GItem Apologybox { get; init; } = null!;

    [JsonPropertyName("armorbox")]
    public GItem Armorbox { get; init; } = null!;

    [JsonPropertyName("armorring")]
    public GItem Armorring { get; init; } = null!;

    [JsonPropertyName("armorscroll")]
    public GItem Armorscroll { get; init; } = null!;

    [JsonPropertyName("ascale")]
    public GItem Ascale { get; init; } = null!;

    [JsonPropertyName("axe3")]
    public GItem Axe3 { get; init; } = null!;

    [JsonPropertyName("bandages")]
    public GItem Bandages { get; init; } = null!;

    [JsonPropertyName("basher")]
    public GItem Basher { get; init; } = null!;

    [JsonPropertyName("basketofeggs")]
    public GItem Basketofeggs { get; init; } = null!;

    [JsonPropertyName("bataxe")]
    public GItem Bataxe { get; init; } = null!;

    [JsonPropertyName("bcandle")]
    public GItem Bcandle { get; init; } = null!;

    [JsonPropertyName("bcape")]
    public GItem Bcape { get; init; } = null!;

    [JsonPropertyName("beewings")]
    public GItem Beewings { get; init; } = null!;

    [JsonPropertyName("bfang")]
    public GItem Bfang { get; init; } = null!;

    [JsonPropertyName("bfangamulet")]
    public GItem Bfangamulet { get; init; } = null!;

    [JsonPropertyName("bfur")]
    public GItem Bfur { get; init; } = null!;

    [JsonPropertyName("bkey")]
    public GItem Bkey { get; init; } = null!;

    [JsonPropertyName("blade")]
    public GItem Blade { get; init; } = null!;

    [JsonPropertyName("blue")]
    public GItem Blue { get; init; } = null!;

    [JsonPropertyName("bottleofxp")]
    public GItem Bottleofxp { get; init; } = null!;

    [JsonPropertyName("bow")]
    public GItem Bow { get; init; } = null!;

    [JsonPropertyName("bow4")]
    public GItem Bow4 { get; init; } = null!;

    [JsonPropertyName("bowofthedead")]
    public GItem Bowofthedead { get; init; } = null!;

    [JsonPropertyName("bronzeingot")]
    public GItem Bronzeingot { get; init; } = null!;

    [JsonPropertyName("bronzenugget")]
    public GItem Bronzenugget { get; init; } = null!;

    [JsonPropertyName("broom")]
    public GItem Broom { get; init; } = null!;

    [JsonPropertyName("brownegg")]
    public GItem Brownegg { get; init; } = null!;

    [JsonPropertyName("brownenvelope")]
    public GItem Brownenvelope { get; init; } = null!;

    [JsonPropertyName("btusk")]
    public GItem Btusk { get; init; } = null!;

    [JsonPropertyName("bugbountybox")]
    public GItem Bugbountybox { get; init; } = null!;

    [JsonPropertyName("bunnyears")]
    public GItem Bunnyears { get; init; } = null!;

    [JsonPropertyName("bunnyelixir")]
    public GItem Bunnyelixir { get; init; } = null!;

    [JsonPropertyName("bwing")]
    public GItem Bwing { get; init; } = null!;

    [JsonPropertyName("cake")]
    public GItem Cake { get; init; } = null!;

    [JsonPropertyName("candy0")]
    public GItem Candy0 { get; init; } = null!;

    [JsonPropertyName("candy0v2")]
    public GItem Candy0V2 { get; init; } = null!;

    [JsonPropertyName("candy0v3")]
    public GItem Candy0V3 { get; init; } = null!;

    [JsonPropertyName("candy1")]
    public GItem Candy1 { get; init; } = null!;

    [JsonPropertyName("candy1v2")]
    public GItem Candy1V2 { get; init; } = null!;

    [JsonPropertyName("candy1v3")]
    public GItem Candy1V3 { get; init; } = null!;

    [JsonPropertyName("candycane")]
    public GItem Candycane { get; init; } = null!;

    [JsonPropertyName("candycanesword")]
    public GItem Candycanesword { get; init; } = null!;

    [JsonPropertyName("candypop")]
    public GItem Candypop { get; init; } = null!;

    [JsonPropertyName("cape")]
    public GItem Cape { get; init; } = null!;

    [JsonPropertyName("carrot")]
    public GItem Carrot { get; init; } = null!;

    [JsonPropertyName("carrotsword")]
    public GItem Carrotsword { get; init; } = null!;

    [JsonPropertyName("cclaw")]
    public GItem Cclaw { get; init; } = null!;

    [JsonPropertyName("cdarktristone")]
    public GItem Cdarktristone { get; init; } = null!;

    [JsonPropertyName("cdragon")]
    public GItem Cdragon { get; init; } = null!;

    [JsonPropertyName("cearring")]
    public GItem Cearring { get; init; } = null!;

    [JsonPropertyName("charmer")]
    public GItem Charmer { get; init; } = null!;

    [JsonPropertyName("chrysalis0")]
    public GItem Chrysalis0 { get; init; } = null!;

    [JsonPropertyName("claw")]
    public GItem Claw { get; init; } = null!;

    [JsonPropertyName("coal")]
    public GItem Coal { get; init; } = null!;

    [JsonPropertyName("coat")]
    public GItem Coat { get; init; } = null!;

    [JsonPropertyName("coat1")]
    public GItem Coat1 { get; init; } = null!;

    [JsonPropertyName("cocoon")]
    public GItem Cocoon { get; init; } = null!;

    [JsonPropertyName("computer")]
    public GItem Computer { get; init; } = null!;

    [JsonPropertyName("confetti")]
    public GItem Confetti { get; init; } = null!;

    [JsonPropertyName("cosmo0")]
    public GItem Cosmo0 { get; init; } = null!;

    [JsonPropertyName("cosmo1")]
    public GItem Cosmo1 { get; init; } = null!;

    [JsonPropertyName("cosmo2")]
    public GItem Cosmo2 { get; init; } = null!;

    [JsonPropertyName("cosmo3")]
    public GItem Cosmo3 { get; init; } = null!;

    [JsonPropertyName("cosmo4")]
    public GItem Cosmo4 { get; init; } = null!;

    [JsonPropertyName("crabclaw")]
    public GItem Crabclaw { get; init; } = null!;

    [JsonPropertyName("cring")]
    public GItem Cring { get; init; } = null!;

    [JsonPropertyName("critscroll")]
    public GItem Critscroll { get; init; } = null!;

    [JsonPropertyName("crossbow")]
    public GItem Crossbow { get; init; } = null!;

    [JsonPropertyName("cryptkey")]
    public GItem Cryptkey { get; init; } = null!;

    [JsonPropertyName("cscale")]
    public GItem Cscale { get; init; } = null!;

    [JsonPropertyName("cscroll0")]
    public GItem Cscroll0 { get; init; } = null!;

    [JsonPropertyName("cscroll1")]
    public GItem Cscroll1 { get; init; } = null!;

    [JsonPropertyName("cscroll2")]
    public GItem Cscroll2 { get; init; } = null!;

    [JsonPropertyName("cscroll3")]
    public GItem Cscroll3 { get; init; } = null!;

    [JsonPropertyName("cshell")]
    public GItem Cshell { get; init; } = null!;

    [JsonPropertyName("ctristone")]
    public GItem Ctristone { get; init; } = null!;

    [JsonPropertyName("cupid")]
    public GItem Cupid { get; init; } = null!;

    [JsonPropertyName("cxjar")]
    public GItem Cxjar { get; init; } = null!;

    [JsonPropertyName("cyber")]
    public GItem Cyber { get; init; } = null!;

    [JsonPropertyName("dagger")]
    public GItem Dagger { get; init; } = null!;

    [JsonPropertyName("daggerofthedead")]
    public GItem Daggerofthedead { get; init; } = null!;

    [JsonPropertyName("darktristone")]
    public GItem Darktristone { get; init; } = null!;

    [JsonPropertyName("dartgun")]
    public GItem Dartgun { get; init; } = null!;

    [JsonPropertyName("dexamulet")]
    public GItem Dexamulet { get; init; } = null!;

    [JsonPropertyName("dexbelt")]
    public GItem Dexbelt { get; init; } = null!;

    [JsonPropertyName("dexearring")]
    public GItem Dexearring { get; init; } = null!;

    [JsonPropertyName("dexearringx")]
    public GItem Dexearringx { get; init; } = null!;

    [JsonPropertyName("dexring")]
    public GItem Dexring { get; init; } = null!;

    [JsonPropertyName("dexscroll")]
    public GItem Dexscroll { get; init; } = null!;

    [JsonPropertyName("dkey")]
    public GItem Dkey { get; init; } = null!;

    [JsonPropertyName("dragondagger")]
    public GItem Dragondagger { get; init; } = null!;

    [JsonPropertyName("drapes")]
    public GItem Drapes { get; init; } = null!;

    [JsonPropertyName("dreturnscroll")]
    public GItem Dreturnscroll { get; init; } = null!;

    [JsonPropertyName("dstones")]
    public GItem Dstones { get; init; } = null!;

    [JsonPropertyName("ecape")]
    public GItem Ecape { get; init; } = null!;

    [JsonPropertyName("ectoplasm")]
    public GItem Ectoplasm { get; init; } = null!;

    [JsonPropertyName("eears")]
    public GItem Eears { get; init; } = null!;

    [JsonPropertyName("egg0")]
    public GItem Egg0 { get; init; } = null!;

    [JsonPropertyName("egg1")]
    public GItem Egg1 { get; init; } = null!;

    [JsonPropertyName("egg2")]
    public GItem Egg2 { get; init; } = null!;

    [JsonPropertyName("egg3")]
    public GItem Egg3 { get; init; } = null!;

    [JsonPropertyName("egg4")]
    public GItem Egg4 { get; init; } = null!;

    [JsonPropertyName("egg5")]
    public GItem Egg5 { get; init; } = null!;

    [JsonPropertyName("egg6")]
    public GItem Egg6 { get; init; } = null!;

    [JsonPropertyName("egg7")]
    public GItem Egg7 { get; init; } = null!;

    [JsonPropertyName("egg8")]
    public GItem Egg8 { get; init; } = null!;

    [JsonPropertyName("eggnog")]
    public GItem Eggnog { get; init; } = null!;

    [JsonPropertyName("electronics")]
    public GItem Electronics { get; init; } = null!;

    [JsonPropertyName("elixirdex0")]
    public GItem Elixirdex0 { get; init; } = null!;

    [JsonPropertyName("elixirdex1")]
    public GItem Elixirdex1 { get; init; } = null!;

    [JsonPropertyName("elixirdex2")]
    public GItem Elixirdex2 { get; init; } = null!;

    [JsonPropertyName("elixirfires")]
    public GItem Elixirfireres { get; init; } = null!;

    [JsonPropertyName("elixirfzres")]
    public GItem Elixirfreezeres { get; init; } = null!;

    [JsonPropertyName("elixirint0")]
    public GItem Elixirint0 { get; init; } = null!;

    [JsonPropertyName("elixirint1")]
    public GItem Elixirint1 { get; init; } = null!;

    [JsonPropertyName("elixirint2")]
    public GItem Elixirint2 { get; init; } = null!;

    [JsonPropertyName("elixirluck")]
    public GItem Elixirluck { get; init; } = null!;

    [JsonPropertyName("elixirpnres")]
    public GItem Elixirpnres { get; init; } = null!;

    [JsonPropertyName("elixirstr0")]
    public GItem Elixirstr0 { get; init; } = null!;

    [JsonPropertyName("elixirstr1")]
    public GItem Elixirstr1 { get; init; } = null!;

    [JsonPropertyName("elixirstr2")]
    public GItem Elixirstr2 { get; init; } = null!;

    [JsonPropertyName("elixirvit0")]
    public GItem Elixirvit0 { get; init; } = null!;

    [JsonPropertyName("elixirvit1")]
    public GItem Elixirvit1 { get; init; } = null!;

    [JsonPropertyName("elixirvit2")]
    public GItem Elixirvit2 { get; init; } = null!;

    [JsonPropertyName("emotionjar")]
    public GItem Emotionjar { get; init; } = null!;

    [JsonPropertyName("emptyheart")]
    public GItem Emptyheart { get; init; } = null!;

    [JsonPropertyName("emptyjar")]
    public GItem Emptyjar { get; init; } = null!;

    [JsonPropertyName("epyjamas")]
    public GItem Epyjamas { get; init; } = null!;

    [JsonPropertyName("eslippers")]
    public GItem Eslippers { get; init; } = null!;

    [JsonPropertyName("espresso")]
    public GItem Espresso { get; init; } = null!;

    [JsonPropertyName("essenceofether")]
    public GItem Essenceofether { get; init; } = null!;

    [JsonPropertyName("essenceoffire")]
    public GItem Essenceoffire { get; init; } = null!;

    [JsonPropertyName("essenceoffrost")]
    public GItem Essenceoffrost { get; init; } = null!;

    [JsonPropertyName("essenceofgreed")]
    public GItem Essenceofgreed { get; init; } = null!;

    [JsonPropertyName("essenceoflife")]
    public GItem Essenceoflife { get; init; } = null!;

    [JsonPropertyName("essenceofnature")]
    public GItem Essenceofnature { get; init; } = null!;

    [JsonPropertyName("evasionscroll")]
    public GItem Evasionscroll { get; init; } = null!;

    [JsonPropertyName("exoarm")]
    public GItem Exoarm { get; init; } = null!;

    [JsonPropertyName("fallen")]
    public GItem Fallen { get; init; } = null!;

    [JsonPropertyName("fcape")]
    public GItem Fcape { get; init; } = null!;

    [JsonPropertyName("fclaw")]
    public GItem Fclaw { get; init; } = null!;

    [JsonPropertyName("feather0")]
    public GItem Feather0 { get; init; } = null!;

    [JsonPropertyName("feather1")]
    public GItem Feather1 { get; init; } = null!;

    [JsonPropertyName("fieldgen0")]
    public GItem Fieldgen0 { get; init; } = null!;

    [JsonPropertyName("fierygloves")]
    public GItem Fierygloves { get; init; } = null!;

    [JsonPropertyName("figurine")]
    public GItem Figurine { get; init; } = null!;

    [JsonPropertyName("fireblade")]
    public GItem Fireblade { get; init; } = null!;

    [JsonPropertyName("firebow")]
    public GItem Firebow { get; init; } = null!;

    [JsonPropertyName("firecrackers")]
    public GItem Firecrackers { get; init; } = null!;

    [JsonPropertyName("firestaff")]
    public GItem Firestaff { get; init; } = null!;

    [JsonPropertyName("firestars")]
    public GItem Firestars { get; init; } = null!;

    [JsonPropertyName("flute")]
    public GItem Flute { get; init; } = null!;

    [JsonPropertyName("forscroll")]
    public GItem Forscroll { get; init; } = null!;

    [JsonPropertyName("frankypants")]
    public GItem Frankypants { get; init; } = null!;

    [JsonPropertyName("frequencyscroll")]
    public GItem Frequencyscroll { get; init; } = null!;

    [JsonPropertyName("friendtoken")]
    public GItem Friendtoken { get; init; } = null!;

    [JsonPropertyName("frogt")]
    public GItem Frogt { get; init; } = null!;

    [JsonPropertyName("frostbow")]
    public GItem Frostbow { get; init; } = null!;

    [JsonPropertyName("froststaff")]
    public GItem Froststaff { get; init; } = null!;

    [JsonPropertyName("frozenkey")]
    public GItem Frozenkey { get; init; } = null!;

    [JsonPropertyName("frozenstone")]
    public GItem Frozenstone { get; init; } = null!;

    [JsonPropertyName("fsword")]
    public GItem Fsword { get; init; } = null!;

    [JsonPropertyName("ftrinket")]
    public GItem Ftrinket { get; init; } = null!;

    [JsonPropertyName("funtoken")]
    public GItem Funtoken { get; init; } = null!;

    [JsonPropertyName("fury")]
    public GItem Fury { get; init; } = null!;

    [JsonPropertyName("gbow")]
    public GItem Gbow { get; init; } = null!;

    [JsonPropertyName("gcape")]
    public GItem Gcape { get; init; } = null!;

    [JsonPropertyName("gem0")]
    public GItem Gem0 { get; init; } = null!;

    [JsonPropertyName("gem1")]
    public GItem Gem1 { get; init; } = null!;

    [JsonPropertyName("gem2")]
    public GItem Gem2 { get; init; } = null!;

    [JsonPropertyName("gem3")]
    public GItem Gem3 { get; init; } = null!;

    [JsonPropertyName("gemfragment")]
    public GItem Gemfragment { get; init; } = null!;

    [JsonPropertyName("ghatb")]
    public GItem Ghatb { get; init; } = null!;

    [JsonPropertyName("ghatp")]
    public GItem Ghatp { get; init; } = null!;

    [JsonPropertyName("gift0")]
    public GItem Gift0 { get; init; } = null!;

    [JsonPropertyName("gift1")]
    public GItem Gift1 { get; init; } = null!;

    [JsonPropertyName("glitch")]
    public GItem Glitch { get; init; } = null!;

    [JsonPropertyName("glolipop")]
    public GItem Glolipop { get; init; } = null!;

    [JsonPropertyName("gloves")]
    public GItem Gloves { get; init; } = null!;

    [JsonPropertyName("gloves1")]
    public GItem Gloves1 { get; init; } = null!;

    [JsonPropertyName("goldbooster")]
    public GItem Goldbooster { get; init; } = null!;

    [JsonPropertyName("goldenegg")]
    public GItem Goldenegg { get; init; } = null!;

    [JsonPropertyName("goldenpowerglove")]
    public GItem Goldenpowerglove { get; init; } = null!;

    [JsonPropertyName("goldingot")]
    public GItem Goldingot { get; init; } = null!;

    [JsonPropertyName("goldnugget")]
    public GItem Goldnugget { get; init; } = null!;

    [JsonPropertyName("goldring")]
    public GItem Goldring { get; init; } = null!;

    [JsonPropertyName("goldscroll")]
    public GItem Goldscroll { get; init; } = null!;

    [JsonPropertyName("gphelmet")]
    public GItem Gphelmet { get; init; } = null!;

    [JsonPropertyName("greenbomb")]
    public GItem Greenbomb { get; init; } = null!;

    [JsonPropertyName("greenenvelope")]
    public GItem Greenenvelope { get; init; } = null!;

    [JsonPropertyName("gslime")]
    public GItem Gslime { get; init; } = null!;

    [JsonPropertyName("gstaff")]
    public GItem Gstaff { get; init; } = null!;

    [JsonPropertyName("gum")]
    public GItem Gum { get; init; } = null!;

    [JsonPropertyName("hammer")]
    public GItem Hammer { get; init; } = null!;

    [JsonPropertyName("handofmidas")]
    public GItem Handofmidas { get; init; } = null!;

    [JsonPropertyName("harbringer")]
    public GItem Harbringer { get; init; } = null!;

    [JsonPropertyName("harmor")]
    public GItem Harmor { get; init; } = null!;

    [JsonPropertyName("harpybow")]
    public GItem Harpybow { get; init; } = null!;

    [JsonPropertyName("hboots")]
    public GItem Hboots { get; init; } = null!;

    [JsonPropertyName("hbow")]
    public GItem Hbow { get; init; } = null!;

    [JsonPropertyName("hdagger")]
    public GItem Hdagger { get; init; } = null!;

    [JsonPropertyName("heartwood")]
    public GItem Heartwood { get; init; } = null!;

    [JsonPropertyName("helmet")]
    public GItem Helmet { get; init; } = null!;

    [JsonPropertyName("helmet1")]
    public GItem Helmet1 { get; init; } = null!;

    [JsonPropertyName("hgloves")]
    public GItem Hgloves { get; init; } = null!;

    [JsonPropertyName("hhelmet")]
    public GItem Hhelmet { get; init; } = null!;

    [JsonPropertyName("horsecape")]
    public GItem Horsecape { get; init; } = null!;

    [JsonPropertyName("horsecapeg")]
    public GItem Horsecapeg { get; init; } = null!;

    [JsonPropertyName("hotchocolate")]
    public GItem Hotchocolate { get; init; } = null!;

    [JsonPropertyName("hpamulet")]
    public GItem Hpamulet { get; init; } = null!;

    [JsonPropertyName("hpants")]
    public GItem Hpants { get; init; } = null!;

    [JsonPropertyName("hpbelt")]
    public GItem Hpbelt { get; init; } = null!;

    [JsonPropertyName("hpot0")]
    public GItem Hpot0 { get; init; } = null!;

    [JsonPropertyName("hpot1")]
    public GItem Hpot1 { get; init; } = null!;

    [JsonPropertyName("hpotx")]
    public GItem Hpotx { get; init; } = null!;

    [JsonPropertyName("iceskates")]
    public GItem Iceskates { get; init; } = null!;

    [JsonPropertyName("ijx")]
    public GItem Ijx { get; init; } = null!;

    [JsonPropertyName("ink")]
    public GItem Ink { get; init; } = null!;

    [JsonPropertyName("intamulet")]
    public GItem Intamulet { get; init; } = null!;

    [JsonPropertyName("intbelt")]
    public GItem Intbelt { get; init; } = null!;

    [JsonPropertyName("intearring")]
    public GItem Intearring { get; init; } = null!;

    [JsonPropertyName("intring")]
    public GItem Intring { get; init; } = null!;

    [JsonPropertyName("intscroll")]
    public GItem Intscroll { get; init; } = null!;

    [JsonPropertyName("jacko")]
    public GItem Jacko { get; init; } = null!;

    [JsonPropertyName("jewellerybox")]
    public GItem Jewellerybox { get; init; } = null!;

    [JsonPropertyName("kitty1")]
    public GItem Kitty1 { get; init; } = null!;

    [JsonPropertyName("lantern")]
    public GItem Lantern { get; init; } = null!;

    [JsonPropertyName("lbelt")]
    public GItem Lbelt { get; init; } = null!;

    [JsonPropertyName("leather")]
    public GItem Leather { get; init; } = null!;

    [JsonPropertyName("ledger")]
    public GItem Ledger { get; init; } = null!;

    [JsonPropertyName("licence")]
    public GItem Licence { get; init; } = null!;

    [JsonPropertyName("lifestealscroll")]
    public GItem Lifestealscroll { get; init; } = null!;

    [JsonPropertyName("lmace")]
    public GItem Lmace { get; init; } = null!;

    [JsonPropertyName("lostearring")]
    public GItem Lostearring { get; init; } = null!;

    [JsonPropertyName("lotusf")]
    public GItem Lotusf { get; init; } = null!;

    [JsonPropertyName("lspores")]
    public GItem Lspores { get; init; } = null!;

    [JsonPropertyName("luckbooster")]
    public GItem Luckbooster { get; init; } = null!;

    [JsonPropertyName("luckscroll")]
    public GItem Luckscroll { get; init; } = null!;

    [JsonPropertyName("luckyt")]
    public GItem Luckyt { get; init; } = null!;

    [JsonPropertyName("mace")]
    public GItem Mace { get; init; } = null!;

    [JsonPropertyName("maceofthedead")]
    public GItem Maceofthedead { get; init; } = null!;

    [JsonPropertyName("mageshood")]
    public GItem Mageshood { get; init; } = null!;

    [JsonPropertyName("manastealscroll")]
    public GItem Manastealscroll { get; init; } = null!;

    [JsonPropertyName("mbelt")]
    public GItem Mbelt { get; init; } = null!;

    [JsonPropertyName("mbones")]
    public GItem Mbones { get; init; } = null!;

    [JsonPropertyName("mcape")]
    public GItem Mcape { get; init; } = null!;

    [JsonPropertyName("mcarmor")]
    public GItem Mcarmor { get; init; } = null!;

    [JsonPropertyName("mcboots")]
    public GItem Mcboots { get; init; } = null!;

    [JsonPropertyName("mcgloves")]
    public GItem Mcgloves { get; init; } = null!;

    [JsonPropertyName("mchat")]
    public GItem Mchat { get; init; } = null!;

    [JsonPropertyName("mcpants")]
    public GItem Mcpants { get; init; } = null!;

    [JsonPropertyName("mearring")]
    public GItem Mearring { get; init; } = null!;

    [JsonPropertyName("merry")]
    public GItem Merry { get; init; } = null!;

    [JsonPropertyName("mistletoe")]
    public GItem Mistletoe { get; init; } = null!;

    [JsonPropertyName("mittens")]
    public GItem Mittens { get; init; } = null!;

    [JsonPropertyName("mmarmor")]
    public GItem Mmarmor { get; init; } = null!;

    [JsonPropertyName("mmgloves")]
    public GItem Mmgloves { get; init; } = null!;

    [JsonPropertyName("mmhat")]
    public GItem Mmhat { get; init; } = null!;

    [JsonPropertyName("mmpants")]
    public GItem Mmpants { get; init; } = null!;

    [JsonPropertyName("mmshoes")]
    public GItem Mmshoes { get; init; } = null!;

    [JsonPropertyName("molesteeth")]
    public GItem Molesteeth { get; init; } = null!;

    [JsonPropertyName("monsterbox")]
    public GItem Monsterbox { get; init; } = null!;

    [JsonPropertyName("monstertoken")]
    public GItem Monstertoken { get; init; } = null!;

    [JsonPropertyName("mparmor")]
    public GItem Mparmor { get; init; } = null!;

    [JsonPropertyName("mpcostscroll")]
    public GItem Mpcostscroll { get; init; } = null!;

    [JsonPropertyName("mpgloves")]
    public GItem Mpgloves { get; init; } = null!;

    [JsonPropertyName("mphat")]
    public GItem Mphat { get; init; } = null!;

    [JsonPropertyName("mpot0")]
    public GItem Mpot0 { get; init; } = null!;

    [JsonPropertyName("mpot1")]
    public GItem Mpot1 { get; init; } = null!;

    [JsonPropertyName("mpotx")]
    public GItem Mpotx { get; init; } = null!;

    [JsonPropertyName("mppants")]
    public GItem Mppants { get; init; } = null!;

    [JsonPropertyName("mpshoes")]
    public GItem Mpshoes { get; init; } = null!;

    [JsonPropertyName("mpxamulet")]
    public GItem Mpxamulet { get; init; } = null!;

    [JsonPropertyName("mpxbelt")]
    public GItem Mpxbelt { get; init; } = null!;

    [JsonPropertyName("mpxgloves")]
    public GItem Mpxgloves { get; init; } = null!;

    [JsonPropertyName("mrarmor")]
    public GItem Mrarmor { get; init; } = null!;

    [JsonPropertyName("mrboots")]
    public GItem Mrboots { get; init; } = null!;

    [JsonPropertyName("mrgloves")]
    public GItem Mrgloves { get; init; } = null!;

    [JsonPropertyName("mrhood")]
    public GItem Mrhood { get; init; } = null!;

    [JsonPropertyName("mrnarmor")]
    public GItem Mrnarmor { get; init; } = null!;

    [JsonPropertyName("mrnboots")]
    public GItem Mrnboots { get; init; } = null!;

    [JsonPropertyName("mrngloves")]
    public GItem Mrngloves { get; init; } = null!;

    [JsonPropertyName("mrnhat")]
    public GItem Mrnhat { get; init; } = null!;

    [JsonPropertyName("mrnpants")]
    public GItem Mrnpants { get; init; } = null!;

    [JsonPropertyName("mrpants")]
    public GItem Mrpants { get; init; } = null!;

    [JsonPropertyName("mshield")]
    public GItem Mshield { get; init; } = null!;

    [JsonPropertyName("mushroomstaff")]
    public GItem Mushroomstaff { get; init; } = null!;

    [JsonPropertyName("mwarmor")]
    public GItem Mwarmor { get; init; } = null!;

    [JsonPropertyName("mwboots")]
    public GItem Mwboots { get; init; } = null!;

    [JsonPropertyName("mwgloves")]
    public GItem Mwgloves { get; init; } = null!;

    [JsonPropertyName("mwhelmet")]
    public GItem Mwhelmet { get; init; } = null!;

    [JsonPropertyName("mwpants")]
    public GItem Mwpants { get; init; } = null!;

    [JsonPropertyName("mysterybox")]
    public GItem Mysterybox { get; init; } = null!;

    [JsonPropertyName("networkcard")]
    public GItem Networkcard { get; init; } = null!;

    [JsonPropertyName("nheart")]
    public GItem Nheart { get; init; } = null!;

    [JsonPropertyName("northstar")]
    public GItem Northstar { get; init; } = null!;

    [JsonPropertyName("offering")]
    public GItem Offering { get; init; } = null!;

    [JsonPropertyName("offeringp")]
    public GItem Offeringp { get; init; } = null!;

    [JsonPropertyName("offeringx")]
    public GItem Offeringx { get; init; } = null!;

    [JsonPropertyName("ololipop")]
    public GItem Ololipop { get; init; } = null!;

    [JsonPropertyName("oozingterror")]
    public GItem Oozingterror { get; init; } = null!;

    [JsonPropertyName("orba")]
    public GItem Orba { get; init; } = null!;

    [JsonPropertyName("orbg")]
    public GItem Orbg { get; init; } = null!;

    [JsonPropertyName("orbofdex")]
    public GItem Orbofdex { get; init; } = null!;

    [JsonPropertyName("orboffire")]
    public GItem Orboffire { get; init; } = null!;

    [JsonPropertyName("orboffrost")]
    public GItem Orboffrost { get; init; } = null!;

    [JsonPropertyName("orbofint")]
    public GItem Orbofint { get; init; } = null!;

    [JsonPropertyName("orbofplague")]
    public GItem Orbofplague { get; init; } = null!;

    [JsonPropertyName("orbofresolve")]
    public GItem Orbofresolve { get; init; } = null!;

    [JsonPropertyName("orbofsc")]
    public GItem Orbofsc { get; init; } = null!;

    [JsonPropertyName("orbofstr")]
    public GItem Orbofstr { get; init; } = null!;

    [JsonPropertyName("orboftemporal")]
    public GItem Orboftemporal { get; init; } = null!;

    [JsonPropertyName("orbofvit")]
    public GItem Orbofvit { get; init; } = null!;

    [JsonPropertyName("ornament")]
    public GItem Ornament { get; init; } = null!;

    [JsonPropertyName("ornamentstaff")]
    public GItem Ornamentstaff { get; init; } = null!;

    [JsonPropertyName("outputscroll")]
    public GItem Outputscroll { get; init; } = null!;

    [JsonPropertyName("oxhelmet")]
    public GItem Oxhelmet { get; init; } = null!;

    [JsonPropertyName("pants")]
    public GItem Pants { get; init; } = null!;

    [JsonPropertyName("pants1")]
    public GItem Pants1 { get; init; } = null!;

    [JsonPropertyName("partyhat")]
    public GItem Partyhat { get; init; } = null!;

    [JsonPropertyName("pclaw")]
    public GItem Pclaw { get; init; } = null!;

    [JsonPropertyName("phelmet")]
    public GItem Phelmet { get; init; } = null!;

    [JsonPropertyName("pickaxe")]
    public GItem Pickaxe { get; init; } = null!;

    [JsonPropertyName("pico")]
    public GItem Pico { get; init; } = null!;

    [JsonPropertyName("pinkie")]
    public GItem Pinkie { get; init; } = null!;

    [JsonPropertyName("placeholder")]
    public GItem Placeholder { get; init; } = null!;

    [JsonPropertyName("placeholder_m")]
    public GItem PlaceholderM { get; init; } = null!;

    [JsonPropertyName("platinumingot")]
    public GItem Platinumingot { get; init; } = null!;

    [JsonPropertyName("platinumnugget")]
    public GItem Platinumnugget { get; init; } = null!;

    [JsonPropertyName("pleather")]
    public GItem Pleather { get; init; } = null!;

    [JsonPropertyName("pmace")]
    public GItem Pmace { get; init; } = null!;

    [JsonPropertyName("pmaceofthedead")]
    public GItem Pmaceofthedead { get; init; } = null!;

    [JsonPropertyName("poison")]
    public GItem Poison { get; init; } = null!;

    [JsonPropertyName("poker")]
    public GItem Poker { get; init; } = null!;

    [JsonPropertyName("pouchbow")]
    public GItem Pouchbow { get; init; } = null!;

    [JsonPropertyName("powerglove")]
    public GItem Powerglove { get; init; } = null!;

    [JsonPropertyName("pstem")]
    public GItem Pstem { get; init; } = null!;

    [JsonPropertyName("pumpkinspice")]
    public GItem Pumpkinspice { get; init; } = null!;

    [JsonPropertyName("puppy1")]
    public GItem Puppy1 { get; init; } = null!;

    [JsonPropertyName("puppyer")]
    public GItem Puppyer { get; init; } = null!;

    [JsonPropertyName("pvptoken")]
    public GItem Pvptoken { get; init; } = null!;

    [JsonPropertyName("pyjamas")]
    public GItem Pyjamas { get; init; } = null!;

    [JsonPropertyName("qubics")]
    public GItem Qubics { get; init; } = null!;

    [JsonPropertyName("quiver")]
    public GItem Quiver { get; init; } = null!;

    [JsonPropertyName("rabbitsfoot")]
    public GItem Rabbitsfoot { get; init; } = null!;

    [JsonPropertyName("rapier")]
    public GItem Rapier { get; init; } = null!;

    [JsonPropertyName("rattail")]
    public GItem Rattail { get; init; } = null!;

    [JsonPropertyName("redenvelope")]
    public GItem Redenvelope { get; init; } = null!;

    [JsonPropertyName("redenvelopev2")]
    public GItem Redenvelopev2 { get; init; } = null!;

    [JsonPropertyName("redenvelopev3")]
    public GItem Redenvelopev3 { get; init; } = null!;

    [JsonPropertyName("redenvelopev4")]
    public GItem Redenvelopev4 { get; init; } = null!;

    [JsonPropertyName("rednose")]
    public GItem Rednose { get; init; } = null!;

    [JsonPropertyName("reflectionscroll")]
    public GItem Reflectionscroll { get; init; } = null!;

    [JsonPropertyName("resistancering")]
    public GItem Resistancering { get; init; } = null!;

    [JsonPropertyName("resistancescroll")]
    public GItem Resistancescroll { get; init; } = null!;

    [JsonPropertyName("rfangs")]
    public GItem Rfangs { get; init; } = null!;

    [JsonPropertyName("rfur")]
    public GItem Rfur { get; init; } = null!;

    [JsonPropertyName("ringhs")]
    public GItem Ringhs { get; init; } = null!;

    [JsonPropertyName("ringofluck")]
    public GItem Ringofluck { get; init; } = null!;

    [JsonPropertyName("ringsj")]
    public GItem Ringsj { get; init; } = null!;

    [JsonPropertyName("rod")]
    public GItem Rod { get; init; } = null!;

    [JsonPropertyName("rpiercingscroll")]
    public GItem Rpiercingscroll { get; init; } = null!;

    [JsonPropertyName("sanguine")]
    public GItem Sanguine { get; init; } = null!;

    [JsonPropertyName("santasbelt")]
    public GItem Santasbelt { get; init; } = null!;

    [JsonPropertyName("sbelt")]
    public GItem Sbelt { get; init; } = null!;

    [JsonPropertyName("scroll0")]
    public GItem Scroll0 { get; init; } = null!;

    [JsonPropertyName("scroll1")]
    public GItem Scroll1 { get; init; } = null!;

    [JsonPropertyName("scroll2")]
    public GItem Scroll2 { get; init; } = null!;

    [JsonPropertyName("scroll3")]
    public GItem Scroll3 { get; init; } = null!;

    [JsonPropertyName("scroll4")]
    public GItem Scroll4 { get; init; } = null!;

    [JsonPropertyName("scythe")]
    public GItem Scythe { get; init; } = null!;

    [JsonPropertyName("seashell")]
    public GItem Seashell { get; init; } = null!;

    [JsonPropertyName("shadowstone")]
    public GItem Shadowstone { get; init; } = null!;

    [JsonPropertyName("shield")]
    public GItem Shield { get; init; } = null!;

    [JsonPropertyName("shoes")]
    public GItem Shoes { get; init; } = null!;

    [JsonPropertyName("shoes1")]
    public GItem Shoes1 { get; init; } = null!;

    [JsonPropertyName("skullamulet")]
    public GItem Skullamulet { get; init; } = null!;

    [JsonPropertyName("slimestaff")]
    public GItem Slimestaff { get; init; } = null!;

    [JsonPropertyName("smoke")]
    public GItem Smoke { get; init; } = null!;

    [JsonPropertyName("smush")]
    public GItem Smush { get; init; } = null!;

    [JsonPropertyName("snakefang")]
    public GItem Snakefang { get; init; } = null!;

    [JsonPropertyName("snakeoil")]
    public GItem Snakeoil { get; init; } = null!;

    [JsonPropertyName("snowball")]
    public GItem Snowball { get; init; } = null!;

    [JsonPropertyName("snowboots")]
    public GItem Snowboots { get; init; } = null!;

    [JsonPropertyName("snowflakes")]
    public GItem Snowflakes { get; init; } = null!;

    [JsonPropertyName("snring")]
    public GItem Snring { get; init; } = null!;

    [JsonPropertyName("solitaire")]
    public GItem Solitaire { get; init; } = null!;

    [JsonPropertyName("sparkstaff")]
    public GItem Sparkstaff { get; init; } = null!;

    [JsonPropertyName("spear")]
    public GItem Spear { get; init; } = null!;

    [JsonPropertyName("spearofthedead")]
    public GItem Spearofthedead { get; init; } = null!;

    [JsonPropertyName("speedscroll")]
    public GItem Speedscroll { get; init; } = null!;

    [JsonPropertyName("spiderkey")]
    public GItem Spiderkey { get; init; } = null!;

    [JsonPropertyName("spidersilk")]
    public GItem Spidersilk { get; init; } = null!;

    [JsonPropertyName("spikedhelmet")]
    public GItem Spikedhelmet { get; init; } = null!;

    [JsonPropertyName("spookyamulet")]
    public GItem Spookyamulet { get; init; } = null!;

    [JsonPropertyName("spores")]
    public GItem Spores { get; init; } = null!;

    [JsonPropertyName("sshield")]
    public GItem Sshield { get; init; } = null!;

    [JsonPropertyName("sstinger")]
    public GItem Sstinger { get; init; } = null!;

    [JsonPropertyName("staff")]
    public GItem Staff { get; init; } = null!;

    [JsonPropertyName("staff2")]
    public GItem Staff2 { get; init; } = null!;

    [JsonPropertyName("staff3")]
    public GItem Staff3 { get; init; } = null!;

    [JsonPropertyName("staff4")]
    public GItem Staff4 { get; init; } = null!;

    [JsonPropertyName("staffofthedead")]
    public GItem Staffofthedead { get; init; } = null!;

    [JsonPropertyName("stand0")]
    public GItem Stand0 { get; init; } = null!;

    [JsonPropertyName("stand1")]
    public GItem Stand1 { get; init; } = null!;

    [JsonPropertyName("starkillers")]
    public GItem Starkillers { get; init; } = null!;

    [JsonPropertyName("stealthcape")]
    public GItem Stealthcape { get; init; } = null!;

    [JsonPropertyName("stick")]
    public GItem Stick { get; init; } = null!;

    [JsonPropertyName("stinger")]
    public GItem Stinger { get; init; } = null!;

    [JsonPropertyName("stonekey")]
    public GItem Stonekey { get; init; } = null!;

    [JsonPropertyName("stoneofgold")]
    public GItem Stoneofgold { get; init; } = null!;

    [JsonPropertyName("stoneofluck")]
    public GItem Stoneofluck { get; init; } = null!;

    [JsonPropertyName("stoneofxp")]
    public GItem Stoneofxp { get; init; } = null!;

    [JsonPropertyName("storagebox")]
    public GItem Storagebox { get; init; } = null!;

    [JsonPropertyName("stramulet")]
    public GItem Stramulet { get; init; } = null!;

    [JsonPropertyName("strbelt")]
    public GItem Strbelt { get; init; } = null!;

    [JsonPropertyName("strearring")]
    public GItem Strearring { get; init; } = null!;

    [JsonPropertyName("strring")]
    public GItem Strring { get; init; } = null!;

    [JsonPropertyName("strscroll")]
    public GItem Strscroll { get; init; } = null!;

    [JsonPropertyName("suckerpunch")]
    public GItem Suckerpunch { get; init; } = null!;

    [JsonPropertyName("supercomputer")]
    public GItem Supercomputer { get; init; } = null!;

    [JsonPropertyName("supermittens")]
    public GItem Supermittens { get; init; } = null!;

    [JsonPropertyName("svenom")]
    public GItem Svenom { get; init; } = null!;

    [JsonPropertyName("sweaterhs")]
    public GItem Sweaterhs { get; init; } = null!;

    [JsonPropertyName("swifty")]
    public GItem Swifty { get; init; } = null!;

    [JsonPropertyName("swirlipop")]
    public GItem Swirlipop { get; init; } = null!;

    [JsonPropertyName("sword")]
    public GItem Sword { get; init; } = null!;

    [JsonPropertyName("swordofthedead")]
    public GItem Swordofthedead { get; init; } = null!;

    [JsonPropertyName("t2bow")]
    public GItem T2Bow { get; init; } = null!;

    [JsonPropertyName("t2dexamulet")]
    public GItem T2Dexamulet { get; init; } = null!;

    [JsonPropertyName("t2intamulet")]
    public GItem T2Intamulet { get; init; } = null!;

    [JsonPropertyName("t2quiver")]
    public GItem T2Quiver { get; init; } = null!;

    [JsonPropertyName("t2stramulet")]
    public GItem T2Stramulet { get; init; } = null!;

    [JsonPropertyName("t3bow")]
    public GItem T3Bow { get; init; } = null!;

    [JsonPropertyName("talkingskull")]
    public GItem Talkingskull { get; init; } = null!;

    [JsonPropertyName("test")]
    public GItem Test { get; init; } = null!;

    [JsonPropertyName("test2")]
    public GItem Test2 { get; init; } = null!;

    [JsonPropertyName("test_orb")]
    public GItem TestOrb { get; init; } = null!;

    [JsonPropertyName("throwingstars")]
    public GItem Throwingstars { get; init; } = null!;

    [JsonPropertyName("tigercape")]
    public GItem Tigercape { get; init; } = null!;

    [JsonPropertyName("tigerhelmet")]
    public GItem Tigerhelmet { get; init; } = null!;

    [JsonPropertyName("tigershield")]
    public GItem Tigershield { get; init; } = null!;

    [JsonPropertyName("tigerstone")]
    public GItem Tigerstone { get; init; } = null!;

    [JsonPropertyName("tombkey")]
    public GItem Tombkey { get; init; } = null!;

    [JsonPropertyName("tracker")]
    public GItem Tracker { get; init; } = null!;

    [JsonPropertyName("trigger")]
    public GItem Trigger { get; init; } = null!;

    [JsonPropertyName("trinkets")]
    public GItem Trinkets { get; init; } = null!;

    [JsonPropertyName("tristone")]
    public GItem Tristone { get; init; } = null!;

    [JsonPropertyName("troll")]
    public GItem Troll { get; init; } = null!;

    [JsonPropertyName("tshell")]
    public GItem Tshell { get; init; } = null!;

    [JsonPropertyName("tshirt0")]
    public GItem Tshirt0 { get; init; } = null!;

    [JsonPropertyName("tshirt1")]
    public GItem Tshirt1 { get; init; } = null!;

    [JsonPropertyName("tshirt2")]
    public GItem Tshirt2 { get; init; } = null!;

    [JsonPropertyName("tshirt3")]
    public GItem Tshirt3 { get; init; } = null!;

    [JsonPropertyName("tshirt4")]
    public GItem Tshirt4 { get; init; } = null!;

    [JsonPropertyName("tshirt6")]
    public GItem Tshirt6 { get; init; } = null!;

    [JsonPropertyName("tshirt7")]
    public GItem Tshirt7 { get; init; } = null!;

    [JsonPropertyName("tshirt8")]
    public GItem Tshirt8 { get; init; } = null!;

    [JsonPropertyName("tshirt88")]
    public GItem Tshirt88 { get; init; } = null!;

    [JsonPropertyName("tshirt9")]
    public GItem Tshirt9 { get; init; } = null!;

    [JsonPropertyName("ukey")]
    public GItem Ukey { get; init; } = null!;

    [JsonPropertyName("vattire")]
    public GItem Vattire { get; init; } = null!;

    [JsonPropertyName("vblood")]
    public GItem Vblood { get; init; } = null!;

    [JsonPropertyName("vboots")]
    public GItem Vboots { get; init; } = null!;

    [JsonPropertyName("vcape")]
    public GItem Vcape { get; init; } = null!;

    [JsonPropertyName("vdagger")]
    public GItem Vdagger { get; init; } = null!;

    [JsonPropertyName("vgloves")]
    public GItem Vgloves { get; init; } = null!;

    [JsonPropertyName("vhammer")]
    public GItem Vhammer { get; init; } = null!;

    [JsonPropertyName("vitearring")]
    public GItem Vitearring { get; init; } = null!;

    [JsonPropertyName("vitring")]
    public GItem Vitring { get; init; } = null!;

    [JsonPropertyName("vitscroll")]
    public GItem Vitscroll { get; init; } = null!;

    [JsonPropertyName("vorb")]
    public GItem Vorb { get; init; } = null!;

    [JsonPropertyName("vring")]
    public GItem Vring { get; init; } = null!;

    [JsonPropertyName("vstaff")]
    public GItem Vstaff { get; init; } = null!;

    [JsonPropertyName("vsword")]
    public GItem Vsword { get; init; } = null!;

    [JsonPropertyName("wand")]
    public GItem Wand { get; init; } = null!;

    [JsonPropertyName("warmscarf")]
    public GItem Warmscarf { get; init; } = null!;

    [JsonPropertyName("warpvest")]
    public GItem Warpvest { get; init; } = null!;

    [JsonPropertyName("watercore")]
    public GItem Watercore { get; init; } = null!;

    [JsonPropertyName("wattire")]
    public GItem Wattire { get; init; } = null!;

    [JsonPropertyName("wbasher")]
    public GItem Wbasher { get; init; } = null!;

    [JsonPropertyName("wblade")]
    public GItem Wblade { get; init; } = null!;

    [JsonPropertyName("wbook0")]
    public GItem Wbook0 { get; init; } = null!;

    [JsonPropertyName("wbook1")]
    public GItem Wbook1 { get; init; } = null!;

    [JsonPropertyName("wbookhs")]
    public GItem Wbookhs { get; init; } = null!;

    [JsonPropertyName("wbreeches")]
    public GItem Wbreeches { get; init; } = null!;

    [JsonPropertyName("wcap")]
    public GItem Wcap { get; init; } = null!;

    [JsonPropertyName("weaponbox")]
    public GItem Weaponbox { get; init; } = null!;

    [JsonPropertyName("weaver")]
    public GItem Weaver { get; init; } = null!;

    [JsonPropertyName("wgloves")]
    public GItem Wgloves { get; init; } = null!;

    [JsonPropertyName("whiskey")]
    public GItem Whiskey { get; init; } = null!;

    [JsonPropertyName("whiteegg")]
    public GItem Whiteegg { get; init; } = null!;

    [JsonPropertyName("wine")]
    public GItem Wine { get; init; } = null!;

    [JsonPropertyName("wingedboots")]
    public GItem Wingedboots { get; init; } = null!;

    [JsonPropertyName("woodensword")]
    public GItem Woodensword { get; init; } = null!;

    [JsonPropertyName("wshield")]
    public GItem Wshield { get; init; } = null!;

    [JsonPropertyName("wshoes")]
    public GItem Wshoes { get; init; } = null!;

    [JsonPropertyName("x0")]
    public GItem X0 { get; init; } = null!;

    [JsonPropertyName("x1")]
    public GItem X1 { get; init; } = null!;

    [JsonPropertyName("x2")]
    public GItem X2 { get; init; } = null!;

    [JsonPropertyName("x3")]
    public GItem X3 { get; init; } = null!;

    [JsonPropertyName("x4")]
    public GItem X4 { get; init; } = null!;

    [JsonPropertyName("x5")]
    public GItem X5 { get; init; } = null!;

    [JsonPropertyName("x6")]
    public GItem X6 { get; init; } = null!;

    [JsonPropertyName("x7")]
    public GItem X7 { get; init; } = null!;

    [JsonPropertyName("x8")]
    public GItem X8 { get; init; } = null!;

    [JsonPropertyName("xarmor")]
    public GItem Xarmor { get; init; } = null!;

    [JsonPropertyName("xboots")]
    public GItem Xboots { get; init; } = null!;

    [JsonPropertyName("xbox")]
    public GItem Xbox { get; init; } = null!;

    [JsonPropertyName("xgloves")]
    public GItem Xgloves { get; init; } = null!;

    [JsonPropertyName("xhelmet")]
    public GItem Xhelmet { get; init; } = null!;

    [JsonPropertyName("xmace")]
    public GItem Xmace { get; init; } = null!;

    [JsonPropertyName("xmashat")]
    public GItem Xmashat { get; init; } = null!;

    [JsonPropertyName("xmaspants")]
    public GItem Xmaspants { get; init; } = null!;

    [JsonPropertyName("xmasshoes")]
    public GItem Xmasshoes { get; init; } = null!;

    [JsonPropertyName("xmassweater")]
    public GItem Xmassweater { get; init; } = null!;

    [JsonPropertyName("xpants")]
    public GItem Xpants { get; init; } = null!;

    [JsonPropertyName("xpbooster")]
    public GItem Xpbooster { get; init; } = null!;

    [JsonPropertyName("xpscroll")]
    public GItem Xpscroll { get; init; } = null!;

    [JsonPropertyName("xptome")]
    public GItem Xptome { get; init; } = null!;

    [JsonPropertyName("xshield")]
    public GItem Xshield { get; init; } = null!;

    [JsonPropertyName("xshot")]
    public GItem Xshot { get; init; } = null!;

    [JsonPropertyName("zapper")]
    public GItem Zapper { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var item) in Entries.Reverse())
            if (string.IsNullOrEmpty(item.Accessor))
                item.Accessor = accessor;
    }
}