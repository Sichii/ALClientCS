#region
using System.Linq;
using System.Text.Json.Serialization;
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
    [JsonPropertyName("5bucks")]
    public GItem _5Bucks { get; init; } = null!;

    [JsonProperty("ale")]
    [JsonPropertyName("ale")]
    public GItem Ale { get; init; } = null!;

    [JsonProperty("alloyquiver")]
    [JsonPropertyName("alloyquiver")]
    public GItem Alloyquiver { get; init; } = null!;

    [JsonProperty("amuletofm")]
    [JsonPropertyName("amuletofm")]
    public GItem Amuletofm { get; init; } = null!;

    [JsonProperty("angelwings")]
    [JsonPropertyName("angelwings")]
    public GItem Angelwings { get; init; } = null!;

    [JsonProperty("apiercingscroll")]
    [JsonPropertyName("apiercingscroll")]
    public GItem Apiercingscroll { get; init; } = null!;

    [JsonProperty("apologybox")]
    [JsonPropertyName("apologybox")]
    public GItem Apologybox { get; init; } = null!;

    [JsonProperty("armorbox")]
    [JsonPropertyName("armorbox")]
    public GItem Armorbox { get; init; } = null!;

    [JsonProperty("armorring")]
    [JsonPropertyName("armorring")]
    public GItem Armorring { get; init; } = null!;

    [JsonProperty("armorscroll")]
    [JsonPropertyName("armorscroll")]
    public GItem Armorscroll { get; init; } = null!;

    [JsonProperty("ascale")]
    [JsonPropertyName("ascale")]
    public GItem Ascale { get; init; } = null!;

    [JsonProperty("axe3")]
    [JsonPropertyName("axe3")]
    public GItem Axe3 { get; init; } = null!;

    [JsonProperty("bandages")]
    [JsonPropertyName("bandages")]
    public GItem Bandages { get; init; } = null!;

    [JsonProperty("basher")]
    [JsonPropertyName("basher")]
    public GItem Basher { get; init; } = null!;

    [JsonProperty("basketofeggs")]
    [JsonPropertyName("basketofeggs")]
    public GItem Basketofeggs { get; init; } = null!;

    [JsonProperty("bataxe")]
    [JsonPropertyName("bataxe")]
    public GItem Bataxe { get; init; } = null!;

    [JsonProperty("bcandle")]
    [JsonPropertyName("bcandle")]
    public GItem Bcandle { get; init; } = null!;

    [JsonProperty("bcape")]
    [JsonPropertyName("bcape")]
    public GItem Bcape { get; init; } = null!;

    [JsonProperty("beewings")]
    [JsonPropertyName("beewings")]
    public GItem Beewings { get; init; } = null!;

    [JsonProperty("bfang")]
    [JsonPropertyName("bfang")]
    public GItem Bfang { get; init; } = null!;

    [JsonProperty("bfangamulet")]
    [JsonPropertyName("bfangamulet")]
    public GItem Bfangamulet { get; init; } = null!;

    [JsonProperty("bfur")]
    [JsonPropertyName("bfur")]
    public GItem Bfur { get; init; } = null!;

    [JsonProperty("bkey")]
    [JsonPropertyName("bkey")]
    public GItem Bkey { get; init; } = null!;

    [JsonProperty("blade")]
    [JsonPropertyName("blade")]
    public GItem Blade { get; init; } = null!;

    [JsonProperty("blue")]
    [JsonPropertyName("blue")]
    public GItem Blue { get; init; } = null!;

    [JsonProperty("bottleofxp")]
    [JsonPropertyName("bottleofxp")]
    public GItem Bottleofxp { get; init; } = null!;

    [JsonProperty("bow")]
    [JsonPropertyName("bow")]
    public GItem Bow { get; init; } = null!;

    [JsonProperty("bow4")]
    [JsonPropertyName("bow4")]
    public GItem Bow4 { get; init; } = null!;

    [JsonProperty("bowofthedead")]
    [JsonPropertyName("bowofthedead")]
    public GItem Bowofthedead { get; init; } = null!;

    [JsonProperty("bronzeingot")]
    [JsonPropertyName("bronzeingot")]
    public GItem Bronzeingot { get; init; } = null!;

    [JsonProperty("bronzenugget")]
    [JsonPropertyName("bronzenugget")]
    public GItem Bronzenugget { get; init; } = null!;

    [JsonProperty("broom")]
    [JsonPropertyName("broom")]
    public GItem Broom { get; init; } = null!;

    [JsonProperty("brownegg")]
    [JsonPropertyName("brownegg")]
    public GItem Brownegg { get; init; } = null!;

    [JsonProperty("brownenvelope")]
    [JsonPropertyName("brownenvelope")]
    public GItem Brownenvelope { get; init; } = null!;

    [JsonProperty("btusk")]
    [JsonPropertyName("btusk")]
    public GItem Btusk { get; init; } = null!;

    [JsonProperty("bugbountybox")]
    [JsonPropertyName("bugbountybox")]
    public GItem Bugbountybox { get; init; } = null!;

    [JsonProperty("bunnyears")]
    [JsonPropertyName("bunnyears")]
    public GItem Bunnyears { get; init; } = null!;

    [JsonProperty("bunnyelixir")]
    [JsonPropertyName("bunnyelixir")]
    public GItem Bunnyelixir { get; init; } = null!;

    [JsonProperty("bwing")]
    [JsonPropertyName("bwing")]
    public GItem Bwing { get; init; } = null!;

    [JsonProperty("cake")]
    [JsonPropertyName("cake")]
    public GItem Cake { get; init; } = null!;

    [JsonProperty("candy0")]
    [JsonPropertyName("candy0")]
    public GItem Candy0 { get; init; } = null!;

    [JsonProperty("candy0v2")]
    [JsonPropertyName("candy0v2")]
    public GItem Candy0V2 { get; init; } = null!;

    [JsonProperty("candy0v3")]
    [JsonPropertyName("candy0v3")]
    public GItem Candy0V3 { get; init; } = null!;

    [JsonProperty("candy1")]
    [JsonPropertyName("candy1")]
    public GItem Candy1 { get; init; } = null!;

    [JsonProperty("candy1v2")]
    [JsonPropertyName("candy1v2")]
    public GItem Candy1V2 { get; init; } = null!;

    [JsonProperty("candy1v3")]
    [JsonPropertyName("candy1v3")]
    public GItem Candy1V3 { get; init; } = null!;

    [JsonProperty("candycane")]
    [JsonPropertyName("candycane")]
    public GItem Candycane { get; init; } = null!;

    [JsonProperty("candycanesword")]
    [JsonPropertyName("candycanesword")]
    public GItem Candycanesword { get; init; } = null!;

    [JsonProperty("candypop")]
    [JsonPropertyName("candypop")]
    public GItem Candypop { get; init; } = null!;

    [JsonProperty("cape")]
    [JsonPropertyName("cape")]
    public GItem Cape { get; init; } = null!;

    [JsonProperty("carrot")]
    [JsonPropertyName("carrot")]
    public GItem Carrot { get; init; } = null!;

    [JsonProperty("carrotsword")]
    [JsonPropertyName("carrotsword")]
    public GItem Carrotsword { get; init; } = null!;

    [JsonProperty("cclaw")]
    [JsonPropertyName("cclaw")]
    public GItem Cclaw { get; init; } = null!;

    [JsonProperty("cdarktristone")]
    [JsonPropertyName("cdarktristone")]
    public GItem Cdarktristone { get; init; } = null!;

    [JsonProperty("cdragon")]
    [JsonPropertyName("cdragon")]
    public GItem Cdragon { get; init; } = null!;

    [JsonProperty("cearring")]
    [JsonPropertyName("cearring")]
    public GItem Cearring { get; init; } = null!;

    [JsonProperty("charmer")]
    [JsonPropertyName("charmer")]
    public GItem Charmer { get; init; } = null!;

    [JsonProperty("chrysalis0")]
    [JsonPropertyName("chrysalis0")]
    public GItem Chrysalis0 { get; init; } = null!;

    [JsonProperty("claw")]
    [JsonPropertyName("claw")]
    public GItem Claw { get; init; } = null!;

    [JsonProperty("coal")]
    [JsonPropertyName("coal")]
    public GItem Coal { get; init; } = null!;

    [JsonProperty("coat")]
    [JsonPropertyName("coat")]
    public GItem Coat { get; init; } = null!;

    [JsonProperty("coat1")]
    [JsonPropertyName("coat1")]
    public GItem Coat1 { get; init; } = null!;

    [JsonProperty("cocoon")]
    [JsonPropertyName("cocoon")]
    public GItem Cocoon { get; init; } = null!;

    [JsonProperty("computer")]
    [JsonPropertyName("computer")]
    public GItem Computer { get; init; } = null!;

    [JsonProperty("confetti")]
    [JsonPropertyName("confetti")]
    public GItem Confetti { get; init; } = null!;

    [JsonProperty("cosmo0")]
    [JsonPropertyName("cosmo0")]
    public GItem Cosmo0 { get; init; } = null!;

    [JsonProperty("cosmo1")]
    [JsonPropertyName("cosmo1")]
    public GItem Cosmo1 { get; init; } = null!;

    [JsonProperty("cosmo2")]
    [JsonPropertyName("cosmo2")]
    public GItem Cosmo2 { get; init; } = null!;

    [JsonProperty("cosmo3")]
    [JsonPropertyName("cosmo3")]
    public GItem Cosmo3 { get; init; } = null!;

    [JsonProperty("cosmo4")]
    [JsonPropertyName("cosmo4")]
    public GItem Cosmo4 { get; init; } = null!;

    [JsonProperty("crabclaw")]
    [JsonPropertyName("crabclaw")]
    public GItem Crabclaw { get; init; } = null!;

    [JsonProperty("cring")]
    [JsonPropertyName("cring")]
    public GItem Cring { get; init; } = null!;

    [JsonProperty("critscroll")]
    [JsonPropertyName("critscroll")]
    public GItem Critscroll { get; init; } = null!;

    [JsonProperty("crossbow")]
    [JsonPropertyName("crossbow")]
    public GItem Crossbow { get; init; } = null!;

    [JsonProperty("cryptkey")]
    [JsonPropertyName("cryptkey")]
    public GItem Cryptkey { get; init; } = null!;

    [JsonProperty("cscale")]
    [JsonPropertyName("cscale")]
    public GItem Cscale { get; init; } = null!;

    [JsonProperty("cscroll0")]
    [JsonPropertyName("cscroll0")]
    public GItem Cscroll0 { get; init; } = null!;

    [JsonProperty("cscroll1")]
    [JsonPropertyName("cscroll1")]
    public GItem Cscroll1 { get; init; } = null!;

    [JsonProperty("cscroll2")]
    [JsonPropertyName("cscroll2")]
    public GItem Cscroll2 { get; init; } = null!;

    [JsonProperty("cscroll3")]
    [JsonPropertyName("cscroll3")]
    public GItem Cscroll3 { get; init; } = null!;

    [JsonProperty("cshell")]
    [JsonPropertyName("cshell")]
    public GItem Cshell { get; init; } = null!;

    [JsonProperty("ctristone")]
    [JsonPropertyName("ctristone")]
    public GItem Ctristone { get; init; } = null!;

    [JsonProperty("cupid")]
    [JsonPropertyName("cupid")]
    public GItem Cupid { get; init; } = null!;

    [JsonProperty("cxjar")]
    [JsonPropertyName("cxjar")]
    public GItem Cxjar { get; init; } = null!;

    [JsonProperty("cyber")]
    [JsonPropertyName("cyber")]
    public GItem Cyber { get; init; } = null!;

    [JsonProperty("dagger")]
    [JsonPropertyName("dagger")]
    public GItem Dagger { get; init; } = null!;

    [JsonProperty("daggerofthedead")]
    [JsonPropertyName("daggerofthedead")]
    public GItem Daggerofthedead { get; init; } = null!;

    [JsonProperty("darktristone")]
    [JsonPropertyName("darktristone")]
    public GItem Darktristone { get; init; } = null!;

    [JsonProperty("dartgun")]
    [JsonPropertyName("dartgun")]
    public GItem Dartgun { get; init; } = null!;

    [JsonProperty("dexamulet")]
    [JsonPropertyName("dexamulet")]
    public GItem Dexamulet { get; init; } = null!;

    [JsonProperty("dexbelt")]
    [JsonPropertyName("dexbelt")]
    public GItem Dexbelt { get; init; } = null!;

    [JsonProperty("dexearring")]
    [JsonPropertyName("dexearring")]
    public GItem Dexearring { get; init; } = null!;

    [JsonProperty("dexearringx")]
    [JsonPropertyName("dexearringx")]
    public GItem Dexearringx { get; init; } = null!;

    [JsonProperty("dexring")]
    [JsonPropertyName("dexring")]
    public GItem Dexring { get; init; } = null!;

    [JsonProperty("dexscroll")]
    [JsonPropertyName("dexscroll")]
    public GItem Dexscroll { get; init; } = null!;

    [JsonProperty("dkey")]
    [JsonPropertyName("dkey")]
    public GItem Dkey { get; init; } = null!;

    [JsonProperty("dragondagger")]
    [JsonPropertyName("dragondagger")]
    public GItem Dragondagger { get; init; } = null!;

    [JsonProperty("drapes")]
    [JsonPropertyName("drapes")]
    public GItem Drapes { get; init; } = null!;

    [JsonProperty("dreturnscroll")]
    [JsonPropertyName("dreturnscroll")]
    public GItem Dreturnscroll { get; init; } = null!;

    [JsonProperty("dstones")]
    [JsonPropertyName("dstones")]
    public GItem Dstones { get; init; } = null!;

    [JsonProperty("ecape")]
    [JsonPropertyName("ecape")]
    public GItem Ecape { get; init; } = null!;

    [JsonProperty("ectoplasm")]
    [JsonPropertyName("ectoplasm")]
    public GItem Ectoplasm { get; init; } = null!;

    [JsonProperty("eears")]
    [JsonPropertyName("eears")]
    public GItem Eears { get; init; } = null!;

    [JsonProperty("egg0")]
    [JsonPropertyName("egg0")]
    public GItem Egg0 { get; init; } = null!;

    [JsonProperty("egg1")]
    [JsonPropertyName("egg1")]
    public GItem Egg1 { get; init; } = null!;

    [JsonProperty("egg2")]
    [JsonPropertyName("egg2")]
    public GItem Egg2 { get; init; } = null!;

    [JsonProperty("egg3")]
    [JsonPropertyName("egg3")]
    public GItem Egg3 { get; init; } = null!;

    [JsonProperty("egg4")]
    [JsonPropertyName("egg4")]
    public GItem Egg4 { get; init; } = null!;

    [JsonProperty("egg5")]
    [JsonPropertyName("egg5")]
    public GItem Egg5 { get; init; } = null!;

    [JsonProperty("egg6")]
    [JsonPropertyName("egg6")]
    public GItem Egg6 { get; init; } = null!;

    [JsonProperty("egg7")]
    [JsonPropertyName("egg7")]
    public GItem Egg7 { get; init; } = null!;

    [JsonProperty("egg8")]
    [JsonPropertyName("egg8")]
    public GItem Egg8 { get; init; } = null!;

    [JsonProperty("eggnog")]
    [JsonPropertyName("eggnog")]
    public GItem Eggnog { get; init; } = null!;

    [JsonProperty("electronics")]
    [JsonPropertyName("electronics")]
    public GItem Electronics { get; init; } = null!;

    [JsonProperty("elixirdex0")]
    [JsonPropertyName("elixirdex0")]
    public GItem Elixirdex0 { get; init; } = null!;

    [JsonProperty("elixirdex1")]
    [JsonPropertyName("elixirdex1")]
    public GItem Elixirdex1 { get; init; } = null!;

    [JsonProperty("elixirdex2")]
    [JsonPropertyName("elixirdex2")]
    public GItem Elixirdex2 { get; init; } = null!;

    [JsonProperty("elixirfires")]
    [JsonPropertyName("elixirfires")]
    public GItem Elixirfireres { get; init; } = null!;

    [JsonProperty("elixirfzres")]
    [JsonPropertyName("elixirfzres")]
    public GItem Elixirfreezeres { get; init; } = null!;

    [JsonProperty("elixirint0")]
    [JsonPropertyName("elixirint0")]
    public GItem Elixirint0 { get; init; } = null!;

    [JsonProperty("elixirint1")]
    [JsonPropertyName("elixirint1")]
    public GItem Elixirint1 { get; init; } = null!;

    [JsonProperty("elixirint2")]
    [JsonPropertyName("elixirint2")]
    public GItem Elixirint2 { get; init; } = null!;

    [JsonProperty("elixirluck")]
    [JsonPropertyName("elixirluck")]
    public GItem Elixirluck { get; init; } = null!;

    [JsonProperty("elixirpnres")]
    [JsonPropertyName("elixirpnres")]
    public GItem Elixirpnres { get; init; } = null!;

    [JsonProperty("elixirstr0")]
    [JsonPropertyName("elixirstr0")]
    public GItem Elixirstr0 { get; init; } = null!;

    [JsonProperty("elixirstr1")]
    [JsonPropertyName("elixirstr1")]
    public GItem Elixirstr1 { get; init; } = null!;

    [JsonProperty("elixirstr2")]
    [JsonPropertyName("elixirstr2")]
    public GItem Elixirstr2 { get; init; } = null!;

    [JsonProperty("elixirvit0")]
    [JsonPropertyName("elixirvit0")]
    public GItem Elixirvit0 { get; init; } = null!;

    [JsonProperty("elixirvit1")]
    [JsonPropertyName("elixirvit1")]
    public GItem Elixirvit1 { get; init; } = null!;

    [JsonProperty("elixirvit2")]
    [JsonPropertyName("elixirvit2")]
    public GItem Elixirvit2 { get; init; } = null!;

    [JsonProperty("emotionjar")]
    [JsonPropertyName("emotionjar")]
    public GItem Emotionjar { get; init; } = null!;

    [JsonProperty("emptyheart")]
    [JsonPropertyName("emptyheart")]
    public GItem Emptyheart { get; init; } = null!;

    [JsonProperty("emptyjar")]
    [JsonPropertyName("emptyjar")]
    public GItem Emptyjar { get; init; } = null!;

    [JsonProperty("epyjamas")]
    [JsonPropertyName("epyjamas")]
    public GItem Epyjamas { get; init; } = null!;

    [JsonProperty("eslippers")]
    [JsonPropertyName("eslippers")]
    public GItem Eslippers { get; init; } = null!;

    [JsonProperty("espresso")]
    [JsonPropertyName("espresso")]
    public GItem Espresso { get; init; } = null!;

    [JsonProperty("essenceofether")]
    [JsonPropertyName("essenceofether")]
    public GItem Essenceofether { get; init; } = null!;

    [JsonProperty("essenceoffire")]
    [JsonPropertyName("essenceoffire")]
    public GItem Essenceoffire { get; init; } = null!;

    [JsonProperty("essenceoffrost")]
    [JsonPropertyName("essenceoffrost")]
    public GItem Essenceoffrost { get; init; } = null!;

    [JsonProperty("essenceofgreed")]
    [JsonPropertyName("essenceofgreed")]
    public GItem Essenceofgreed { get; init; } = null!;

    [JsonProperty("essenceoflife")]
    [JsonPropertyName("essenceoflife")]
    public GItem Essenceoflife { get; init; } = null!;

    [JsonProperty("essenceofnature")]
    [JsonPropertyName("essenceofnature")]
    public GItem Essenceofnature { get; init; } = null!;

    [JsonProperty("evasionscroll")]
    [JsonPropertyName("evasionscroll")]
    public GItem Evasionscroll { get; init; } = null!;

    [JsonProperty("exoarm")]
    [JsonPropertyName("exoarm")]
    public GItem Exoarm { get; init; } = null!;

    [JsonProperty("fallen")]
    [JsonPropertyName("fallen")]
    public GItem Fallen { get; init; } = null!;

    [JsonProperty("fcape")]
    [JsonPropertyName("fcape")]
    public GItem Fcape { get; init; } = null!;

    [JsonProperty("fclaw")]
    [JsonPropertyName("fclaw")]
    public GItem Fclaw { get; init; } = null!;

    [JsonProperty("feather0")]
    [JsonPropertyName("feather0")]
    public GItem Feather0 { get; init; } = null!;

    [JsonProperty("feather1")]
    [JsonPropertyName("feather1")]
    public GItem Feather1 { get; init; } = null!;

    [JsonProperty("fieldgen0")]
    [JsonPropertyName("fieldgen0")]
    public GItem Fieldgen0 { get; init; } = null!;

    [JsonProperty("fierygloves")]
    [JsonPropertyName("fierygloves")]
    public GItem Fierygloves { get; init; } = null!;

    [JsonProperty("figurine")]
    [JsonPropertyName("figurine")]
    public GItem Figurine { get; init; } = null!;

    [JsonProperty("fireblade")]
    [JsonPropertyName("fireblade")]
    public GItem Fireblade { get; init; } = null!;

    [JsonProperty("firebow")]
    [JsonPropertyName("firebow")]
    public GItem Firebow { get; init; } = null!;

    [JsonProperty("firecrackers")]
    [JsonPropertyName("firecrackers")]
    public GItem Firecrackers { get; init; } = null!;

    [JsonProperty("firestaff")]
    [JsonPropertyName("firestaff")]
    public GItem Firestaff { get; init; } = null!;

    [JsonProperty("firestars")]
    [JsonPropertyName("firestars")]
    public GItem Firestars { get; init; } = null!;

    [JsonProperty("flute")]
    [JsonPropertyName("flute")]
    public GItem Flute { get; init; } = null!;

    [JsonProperty("forscroll")]
    [JsonPropertyName("forscroll")]
    public GItem Forscroll { get; init; } = null!;

    [JsonProperty("frankypants")]
    [JsonPropertyName("frankypants")]
    public GItem Frankypants { get; init; } = null!;

    [JsonProperty("frequencyscroll")]
    [JsonPropertyName("frequencyscroll")]
    public GItem Frequencyscroll { get; init; } = null!;

    [JsonProperty("friendtoken")]
    [JsonPropertyName("friendtoken")]
    public GItem Friendtoken { get; init; } = null!;

    [JsonProperty("frogt")]
    [JsonPropertyName("frogt")]
    public GItem Frogt { get; init; } = null!;

    [JsonProperty("frostbow")]
    [JsonPropertyName("frostbow")]
    public GItem Frostbow { get; init; } = null!;

    [JsonProperty("froststaff")]
    [JsonPropertyName("froststaff")]
    public GItem Froststaff { get; init; } = null!;

    [JsonProperty("frozenkey")]
    [JsonPropertyName("frozenkey")]
    public GItem Frozenkey { get; init; } = null!;

    [JsonProperty("frozenstone")]
    [JsonPropertyName("frozenstone")]
    public GItem Frozenstone { get; init; } = null!;

    [JsonProperty("fsword")]
    [JsonPropertyName("fsword")]
    public GItem Fsword { get; init; } = null!;

    [JsonProperty("ftrinket")]
    [JsonPropertyName("ftrinket")]
    public GItem Ftrinket { get; init; } = null!;

    [JsonProperty("funtoken")]
    [JsonPropertyName("funtoken")]
    public GItem Funtoken { get; init; } = null!;

    [JsonProperty("fury")]
    [JsonPropertyName("fury")]
    public GItem Fury { get; init; } = null!;

    [JsonProperty("gbow")]
    [JsonPropertyName("gbow")]
    public GItem Gbow { get; init; } = null!;

    [JsonProperty("gcape")]
    [JsonPropertyName("gcape")]
    public GItem Gcape { get; init; } = null!;

    [JsonProperty("gem0")]
    [JsonPropertyName("gem0")]
    public GItem Gem0 { get; init; } = null!;

    [JsonProperty("gem1")]
    [JsonPropertyName("gem1")]
    public GItem Gem1 { get; init; } = null!;

    [JsonProperty("gem2")]
    [JsonPropertyName("gem2")]
    public GItem Gem2 { get; init; } = null!;

    [JsonProperty("gem3")]
    [JsonPropertyName("gem3")]
    public GItem Gem3 { get; init; } = null!;

    [JsonProperty("gemfragment")]
    [JsonPropertyName("gemfragment")]
    public GItem Gemfragment { get; init; } = null!;

    [JsonProperty("ghatb")]
    [JsonPropertyName("ghatb")]
    public GItem Ghatb { get; init; } = null!;

    [JsonProperty("ghatp")]
    [JsonPropertyName("ghatp")]
    public GItem Ghatp { get; init; } = null!;

    [JsonProperty("gift0")]
    [JsonPropertyName("gift0")]
    public GItem Gift0 { get; init; } = null!;

    [JsonProperty("gift1")]
    [JsonPropertyName("gift1")]
    public GItem Gift1 { get; init; } = null!;

    [JsonProperty("glitch")]
    [JsonPropertyName("glitch")]
    public GItem Glitch { get; init; } = null!;

    [JsonProperty("glolipop")]
    [JsonPropertyName("glolipop")]
    public GItem Glolipop { get; init; } = null!;

    [JsonProperty("gloves")]
    [JsonPropertyName("gloves")]
    public GItem Gloves { get; init; } = null!;

    [JsonProperty("gloves1")]
    [JsonPropertyName("gloves1")]
    public GItem Gloves1 { get; init; } = null!;

    [JsonProperty("goldbooster")]
    [JsonPropertyName("goldbooster")]
    public GItem Goldbooster { get; init; } = null!;

    [JsonProperty("goldenegg")]
    [JsonPropertyName("goldenegg")]
    public GItem Goldenegg { get; init; } = null!;

    [JsonProperty("goldenpowerglove")]
    [JsonPropertyName("goldenpowerglove")]
    public GItem Goldenpowerglove { get; init; } = null!;

    [JsonProperty("goldingot")]
    [JsonPropertyName("goldingot")]
    public GItem Goldingot { get; init; } = null!;

    [JsonProperty("goldnugget")]
    [JsonPropertyName("goldnugget")]
    public GItem Goldnugget { get; init; } = null!;

    [JsonProperty("goldring")]
    [JsonPropertyName("goldring")]
    public GItem Goldring { get; init; } = null!;

    [JsonProperty("goldscroll")]
    [JsonPropertyName("goldscroll")]
    public GItem Goldscroll { get; init; } = null!;

    [JsonProperty("gphelmet")]
    [JsonPropertyName("gphelmet")]
    public GItem Gphelmet { get; init; } = null!;

    [JsonProperty("greenbomb")]
    [JsonPropertyName("greenbomb")]
    public GItem Greenbomb { get; init; } = null!;

    [JsonProperty("greenenvelope")]
    [JsonPropertyName("greenenvelope")]
    public GItem Greenenvelope { get; init; } = null!;

    [JsonProperty("gslime")]
    [JsonPropertyName("gslime")]
    public GItem Gslime { get; init; } = null!;

    [JsonProperty("gstaff")]
    [JsonPropertyName("gstaff")]
    public GItem Gstaff { get; init; } = null!;

    [JsonProperty("gum")]
    [JsonPropertyName("gum")]
    public GItem Gum { get; init; } = null!;

    [JsonProperty("hammer")]
    [JsonPropertyName("hammer")]
    public GItem Hammer { get; init; } = null!;

    [JsonProperty("handofmidas")]
    [JsonPropertyName("handofmidas")]
    public GItem Handofmidas { get; init; } = null!;

    [JsonProperty("harbringer")]
    [JsonPropertyName("harbringer")]
    public GItem Harbringer { get; init; } = null!;

    [JsonProperty("harmor")]
    [JsonPropertyName("harmor")]
    public GItem Harmor { get; init; } = null!;

    [JsonProperty("harpybow")]
    [JsonPropertyName("harpybow")]
    public GItem Harpybow { get; init; } = null!;

    [JsonProperty("hboots")]
    [JsonPropertyName("hboots")]
    public GItem Hboots { get; init; } = null!;

    [JsonProperty("hbow")]
    [JsonPropertyName("hbow")]
    public GItem Hbow { get; init; } = null!;

    [JsonProperty("hdagger")]
    [JsonPropertyName("hdagger")]
    public GItem Hdagger { get; init; } = null!;

    [JsonProperty("heartwood")]
    [JsonPropertyName("heartwood")]
    public GItem Heartwood { get; init; } = null!;

    [JsonProperty("helmet")]
    [JsonPropertyName("helmet")]
    public GItem Helmet { get; init; } = null!;

    [JsonProperty("helmet1")]
    [JsonPropertyName("helmet1")]
    public GItem Helmet1 { get; init; } = null!;

    [JsonProperty("hgloves")]
    [JsonPropertyName("hgloves")]
    public GItem Hgloves { get; init; } = null!;

    [JsonProperty("hhelmet")]
    [JsonPropertyName("hhelmet")]
    public GItem Hhelmet { get; init; } = null!;

    [JsonProperty("horsecape")]
    [JsonPropertyName("horsecape")]
    public GItem Horsecape { get; init; } = null!;

    [JsonProperty("horsecapeg")]
    [JsonPropertyName("horsecapeg")]
    public GItem Horsecapeg { get; init; } = null!;

    [JsonProperty("hotchocolate")]
    [JsonPropertyName("hotchocolate")]
    public GItem Hotchocolate { get; init; } = null!;

    [JsonProperty("hpamulet")]
    [JsonPropertyName("hpamulet")]
    public GItem Hpamulet { get; init; } = null!;

    [JsonProperty("hpants")]
    [JsonPropertyName("hpants")]
    public GItem Hpants { get; init; } = null!;

    [JsonProperty("hpbelt")]
    [JsonPropertyName("hpbelt")]
    public GItem Hpbelt { get; init; } = null!;

    [JsonProperty("hpot0")]
    [JsonPropertyName("hpot0")]
    public GItem Hpot0 { get; init; } = null!;

    [JsonProperty("hpot1")]
    [JsonPropertyName("hpot1")]
    public GItem Hpot1 { get; init; } = null!;

    [JsonProperty("hpotx")]
    [JsonPropertyName("hpotx")]
    public GItem Hpotx { get; init; } = null!;

    [JsonProperty("iceskates")]
    [JsonPropertyName("iceskates")]
    public GItem Iceskates { get; init; } = null!;

    [JsonProperty("ijx")]
    [JsonPropertyName("ijx")]
    public GItem Ijx { get; init; } = null!;

    [JsonProperty("ink")]
    [JsonPropertyName("ink")]
    public GItem Ink { get; init; } = null!;

    [JsonProperty("intamulet")]
    [JsonPropertyName("intamulet")]
    public GItem Intamulet { get; init; } = null!;

    [JsonProperty("intbelt")]
    [JsonPropertyName("intbelt")]
    public GItem Intbelt { get; init; } = null!;

    [JsonProperty("intearring")]
    [JsonPropertyName("intearring")]
    public GItem Intearring { get; init; } = null!;

    [JsonProperty("intring")]
    [JsonPropertyName("intring")]
    public GItem Intring { get; init; } = null!;

    [JsonProperty("intscroll")]
    [JsonPropertyName("intscroll")]
    public GItem Intscroll { get; init; } = null!;

    [JsonProperty("jacko")]
    [JsonPropertyName("jacko")]
    public GItem Jacko { get; init; } = null!;

    [JsonProperty("jewellerybox")]
    [JsonPropertyName("jewellerybox")]
    public GItem Jewellerybox { get; init; } = null!;

    [JsonProperty("kitty1")]
    [JsonPropertyName("kitty1")]
    public GItem Kitty1 { get; init; } = null!;

    [JsonProperty("lantern")]
    [JsonPropertyName("lantern")]
    public GItem Lantern { get; init; } = null!;

    [JsonProperty("lbelt")]
    [JsonPropertyName("lbelt")]
    public GItem Lbelt { get; init; } = null!;

    [JsonProperty("leather")]
    [JsonPropertyName("leather")]
    public GItem Leather { get; init; } = null!;

    [JsonProperty("ledger")]
    [JsonPropertyName("ledger")]
    public GItem Ledger { get; init; } = null!;

    [JsonProperty("licence")]
    [JsonPropertyName("licence")]
    public GItem Licence { get; init; } = null!;

    [JsonProperty("lifestealscroll")]
    [JsonPropertyName("lifestealscroll")]
    public GItem Lifestealscroll { get; init; } = null!;

    [JsonProperty("lmace")]
    [JsonPropertyName("lmace")]
    public GItem Lmace { get; init; } = null!;

    [JsonProperty("lostearring")]
    [JsonPropertyName("lostearring")]
    public GItem Lostearring { get; init; } = null!;

    [JsonProperty("lotusf")]
    [JsonPropertyName("lotusf")]
    public GItem Lotusf { get; init; } = null!;

    [JsonProperty("lspores")]
    [JsonPropertyName("lspores")]
    public GItem Lspores { get; init; } = null!;

    [JsonProperty("luckbooster")]
    [JsonPropertyName("luckbooster")]
    public GItem Luckbooster { get; init; } = null!;

    [JsonProperty("luckscroll")]
    [JsonPropertyName("luckscroll")]
    public GItem Luckscroll { get; init; } = null!;

    [JsonProperty("luckyt")]
    [JsonPropertyName("luckyt")]
    public GItem Luckyt { get; init; } = null!;

    [JsonProperty("mace")]
    [JsonPropertyName("mace")]
    public GItem Mace { get; init; } = null!;

    [JsonProperty("maceofthedead")]
    [JsonPropertyName("maceofthedead")]
    public GItem Maceofthedead { get; init; } = null!;

    [JsonProperty("mageshood")]
    [JsonPropertyName("mageshood")]
    public GItem Mageshood { get; init; } = null!;

    [JsonProperty("manastealscroll")]
    [JsonPropertyName("manastealscroll")]
    public GItem Manastealscroll { get; init; } = null!;

    [JsonProperty("mbelt")]
    [JsonPropertyName("mbelt")]
    public GItem Mbelt { get; init; } = null!;

    [JsonProperty("mbones")]
    [JsonPropertyName("mbones")]
    public GItem Mbones { get; init; } = null!;

    [JsonProperty("mcape")]
    [JsonPropertyName("mcape")]
    public GItem Mcape { get; init; } = null!;

    [JsonProperty("mcarmor")]
    [JsonPropertyName("mcarmor")]
    public GItem Mcarmor { get; init; } = null!;

    [JsonProperty("mcboots")]
    [JsonPropertyName("mcboots")]
    public GItem Mcboots { get; init; } = null!;

    [JsonProperty("mcgloves")]
    [JsonPropertyName("mcgloves")]
    public GItem Mcgloves { get; init; } = null!;

    [JsonProperty("mchat")]
    [JsonPropertyName("mchat")]
    public GItem Mchat { get; init; } = null!;

    [JsonProperty("mcpants")]
    [JsonPropertyName("mcpants")]
    public GItem Mcpants { get; init; } = null!;

    [JsonProperty("mearring")]
    [JsonPropertyName("mearring")]
    public GItem Mearring { get; init; } = null!;

    [JsonProperty("merry")]
    [JsonPropertyName("merry")]
    public GItem Merry { get; init; } = null!;

    [JsonProperty("mistletoe")]
    [JsonPropertyName("mistletoe")]
    public GItem Mistletoe { get; init; } = null!;

    [JsonProperty("mittens")]
    [JsonPropertyName("mittens")]
    public GItem Mittens { get; init; } = null!;

    [JsonProperty("mmarmor")]
    [JsonPropertyName("mmarmor")]
    public GItem Mmarmor { get; init; } = null!;

    [JsonProperty("mmgloves")]
    [JsonPropertyName("mmgloves")]
    public GItem Mmgloves { get; init; } = null!;

    [JsonProperty("mmhat")]
    [JsonPropertyName("mmhat")]
    public GItem Mmhat { get; init; } = null!;

    [JsonProperty("mmpants")]
    [JsonPropertyName("mmpants")]
    public GItem Mmpants { get; init; } = null!;

    [JsonProperty("mmshoes")]
    [JsonPropertyName("mmshoes")]
    public GItem Mmshoes { get; init; } = null!;

    [JsonProperty("molesteeth")]
    [JsonPropertyName("molesteeth")]
    public GItem Molesteeth { get; init; } = null!;

    [JsonProperty("monsterbox")]
    [JsonPropertyName("monsterbox")]
    public GItem Monsterbox { get; init; } = null!;

    [JsonProperty("monstertoken")]
    [JsonPropertyName("monstertoken")]
    public GItem Monstertoken { get; init; } = null!;

    [JsonProperty("mparmor")]
    [JsonPropertyName("mparmor")]
    public GItem Mparmor { get; init; } = null!;

    [JsonProperty("mpcostscroll")]
    [JsonPropertyName("mpcostscroll")]
    public GItem Mpcostscroll { get; init; } = null!;

    [JsonProperty("mpgloves")]
    [JsonPropertyName("mpgloves")]
    public GItem Mpgloves { get; init; } = null!;

    [JsonProperty("mphat")]
    [JsonPropertyName("mphat")]
    public GItem Mphat { get; init; } = null!;

    [JsonProperty("mpot0")]
    [JsonPropertyName("mpot0")]
    public GItem Mpot0 { get; init; } = null!;

    [JsonProperty("mpot1")]
    [JsonPropertyName("mpot1")]
    public GItem Mpot1 { get; init; } = null!;

    [JsonProperty("mpotx")]
    [JsonPropertyName("mpotx")]
    public GItem Mpotx { get; init; } = null!;

    [JsonProperty("mppants")]
    [JsonPropertyName("mppants")]
    public GItem Mppants { get; init; } = null!;

    [JsonProperty("mpshoes")]
    [JsonPropertyName("mpshoes")]
    public GItem Mpshoes { get; init; } = null!;

    [JsonProperty("mpxamulet")]
    [JsonPropertyName("mpxamulet")]
    public GItem Mpxamulet { get; init; } = null!;

    [JsonProperty("mpxbelt")]
    [JsonPropertyName("mpxbelt")]
    public GItem Mpxbelt { get; init; } = null!;

    [JsonProperty("mpxgloves")]
    [JsonPropertyName("mpxgloves")]
    public GItem Mpxgloves { get; init; } = null!;

    [JsonProperty("mrarmor")]
    [JsonPropertyName("mrarmor")]
    public GItem Mrarmor { get; init; } = null!;

    [JsonProperty("mrboots")]
    [JsonPropertyName("mrboots")]
    public GItem Mrboots { get; init; } = null!;

    [JsonProperty("mrgloves")]
    [JsonPropertyName("mrgloves")]
    public GItem Mrgloves { get; init; } = null!;

    [JsonProperty("mrhood")]
    [JsonPropertyName("mrhood")]
    public GItem Mrhood { get; init; } = null!;

    [JsonProperty("mrnarmor")]
    [JsonPropertyName("mrnarmor")]
    public GItem Mrnarmor { get; init; } = null!;

    [JsonProperty("mrnboots")]
    [JsonPropertyName("mrnboots")]
    public GItem Mrnboots { get; init; } = null!;

    [JsonProperty("mrngloves")]
    [JsonPropertyName("mrngloves")]
    public GItem Mrngloves { get; init; } = null!;

    [JsonProperty("mrnhat")]
    [JsonPropertyName("mrnhat")]
    public GItem Mrnhat { get; init; } = null!;

    [JsonProperty("mrnpants")]
    [JsonPropertyName("mrnpants")]
    public GItem Mrnpants { get; init; } = null!;

    [JsonProperty("mrpants")]
    [JsonPropertyName("mrpants")]
    public GItem Mrpants { get; init; } = null!;

    [JsonProperty("mshield")]
    [JsonPropertyName("mshield")]
    public GItem Mshield { get; init; } = null!;

    [JsonProperty("mushroomstaff")]
    [JsonPropertyName("mushroomstaff")]
    public GItem Mushroomstaff { get; init; } = null!;

    [JsonProperty("mwarmor")]
    [JsonPropertyName("mwarmor")]
    public GItem Mwarmor { get; init; } = null!;

    [JsonProperty("mwboots")]
    [JsonPropertyName("mwboots")]
    public GItem Mwboots { get; init; } = null!;

    [JsonProperty("mwgloves")]
    [JsonPropertyName("mwgloves")]
    public GItem Mwgloves { get; init; } = null!;

    [JsonProperty("mwhelmet")]
    [JsonPropertyName("mwhelmet")]
    public GItem Mwhelmet { get; init; } = null!;

    [JsonProperty("mwpants")]
    [JsonPropertyName("mwpants")]
    public GItem Mwpants { get; init; } = null!;

    [JsonProperty("mysterybox")]
    [JsonPropertyName("mysterybox")]
    public GItem Mysterybox { get; init; } = null!;

    [JsonProperty("networkcard")]
    [JsonPropertyName("networkcard")]
    public GItem Networkcard { get; init; } = null!;

    [JsonProperty("nheart")]
    [JsonPropertyName("nheart")]
    public GItem Nheart { get; init; } = null!;

    [JsonProperty("northstar")]
    [JsonPropertyName("northstar")]
    public GItem Northstar { get; init; } = null!;

    [JsonProperty("offering")]
    [JsonPropertyName("offering")]
    public GItem Offering { get; init; } = null!;

    [JsonProperty("offeringp")]
    [JsonPropertyName("offeringp")]
    public GItem Offeringp { get; init; } = null!;

    [JsonProperty("offeringx")]
    [JsonPropertyName("offeringx")]
    public GItem Offeringx { get; init; } = null!;

    [JsonProperty("ololipop")]
    [JsonPropertyName("ololipop")]
    public GItem Ololipop { get; init; } = null!;

    [JsonProperty("oozingterror")]
    [JsonPropertyName("oozingterror")]
    public GItem Oozingterror { get; init; } = null!;

    [JsonProperty("orba")]
    [JsonPropertyName("orba")]
    public GItem Orba { get; init; } = null!;

    [JsonProperty("orbg")]
    [JsonPropertyName("orbg")]
    public GItem Orbg { get; init; } = null!;

    [JsonProperty("orbofdex")]
    [JsonPropertyName("orbofdex")]
    public GItem Orbofdex { get; init; } = null!;

    [JsonProperty("orboffire")]
    [JsonPropertyName("orboffire")]
    public GItem Orboffire { get; init; } = null!;

    [JsonProperty("orboffrost")]
    [JsonPropertyName("orboffrost")]
    public GItem Orboffrost { get; init; } = null!;

    [JsonProperty("orbofint")]
    [JsonPropertyName("orbofint")]
    public GItem Orbofint { get; init; } = null!;

    [JsonProperty("orbofplague")]
    [JsonPropertyName("orbofplague")]
    public GItem Orbofplague { get; init; } = null!;

    [JsonProperty("orbofresolve")]
    [JsonPropertyName("orbofresolve")]
    public GItem Orbofresolve { get; init; } = null!;

    [JsonProperty("orbofsc")]
    [JsonPropertyName("orbofsc")]
    public GItem Orbofsc { get; init; } = null!;

    [JsonProperty("orbofstr")]
    [JsonPropertyName("orbofstr")]
    public GItem Orbofstr { get; init; } = null!;

    [JsonProperty("orboftemporal")]
    [JsonPropertyName("orboftemporal")]
    public GItem Orboftemporal { get; init; } = null!;

    [JsonProperty("orbofvit")]
    [JsonPropertyName("orbofvit")]
    public GItem Orbofvit { get; init; } = null!;

    [JsonProperty("ornament")]
    [JsonPropertyName("ornament")]
    public GItem Ornament { get; init; } = null!;

    [JsonProperty("ornamentstaff")]
    [JsonPropertyName("ornamentstaff")]
    public GItem Ornamentstaff { get; init; } = null!;

    [JsonProperty("outputscroll")]
    [JsonPropertyName("outputscroll")]
    public GItem Outputscroll { get; init; } = null!;

    [JsonProperty("oxhelmet")]
    [JsonPropertyName("oxhelmet")]
    public GItem Oxhelmet { get; init; } = null!;

    [JsonProperty("pants")]
    [JsonPropertyName("pants")]
    public GItem Pants { get; init; } = null!;

    [JsonProperty("pants1")]
    [JsonPropertyName("pants1")]
    public GItem Pants1 { get; init; } = null!;

    [JsonProperty("partyhat")]
    [JsonPropertyName("partyhat")]
    public GItem Partyhat { get; init; } = null!;

    [JsonProperty("pclaw")]
    [JsonPropertyName("pclaw")]
    public GItem Pclaw { get; init; } = null!;

    [JsonProperty("phelmet")]
    [JsonPropertyName("phelmet")]
    public GItem Phelmet { get; init; } = null!;

    [JsonProperty("pickaxe")]
    [JsonPropertyName("pickaxe")]
    public GItem Pickaxe { get; init; } = null!;

    [JsonProperty("pico")]
    [JsonPropertyName("pico")]
    public GItem Pico { get; init; } = null!;

    [JsonProperty("pinkie")]
    [JsonPropertyName("pinkie")]
    public GItem Pinkie { get; init; } = null!;

    [JsonProperty("placeholder")]
    [JsonPropertyName("placeholder")]
    public GItem Placeholder { get; init; } = null!;

    [JsonProperty("placeholder_m")]
    [JsonPropertyName("placeholder_m")]
    public GItem PlaceholderM { get; init; } = null!;

    [JsonProperty("platinumingot")]
    [JsonPropertyName("platinumingot")]
    public GItem Platinumingot { get; init; } = null!;

    [JsonProperty("platinumnugget")]
    [JsonPropertyName("platinumnugget")]
    public GItem Platinumnugget { get; init; } = null!;

    [JsonProperty("pleather")]
    [JsonPropertyName("pleather")]
    public GItem Pleather { get; init; } = null!;

    [JsonProperty("pmace")]
    [JsonPropertyName("pmace")]
    public GItem Pmace { get; init; } = null!;

    [JsonProperty("pmaceofthedead")]
    [JsonPropertyName("pmaceofthedead")]
    public GItem Pmaceofthedead { get; init; } = null!;

    [JsonProperty("poison")]
    [JsonPropertyName("poison")]
    public GItem Poison { get; init; } = null!;

    [JsonProperty("poker")]
    [JsonPropertyName("poker")]
    public GItem Poker { get; init; } = null!;

    [JsonProperty("pouchbow")]
    [JsonPropertyName("pouchbow")]
    public GItem Pouchbow { get; init; } = null!;

    [JsonProperty("powerglove")]
    [JsonPropertyName("powerglove")]
    public GItem Powerglove { get; init; } = null!;

    [JsonProperty("pstem")]
    [JsonPropertyName("pstem")]
    public GItem Pstem { get; init; } = null!;

    [JsonProperty("pumpkinspice")]
    [JsonPropertyName("pumpkinspice")]
    public GItem Pumpkinspice { get; init; } = null!;

    [JsonProperty("puppy1")]
    [JsonPropertyName("puppy1")]
    public GItem Puppy1 { get; init; } = null!;

    [JsonProperty("puppyer")]
    [JsonPropertyName("puppyer")]
    public GItem Puppyer { get; init; } = null!;

    [JsonProperty("pvptoken")]
    [JsonPropertyName("pvptoken")]
    public GItem Pvptoken { get; init; } = null!;

    [JsonProperty("pyjamas")]
    [JsonPropertyName("pyjamas")]
    public GItem Pyjamas { get; init; } = null!;

    [JsonProperty("qubics")]
    [JsonPropertyName("qubics")]
    public GItem Qubics { get; init; } = null!;

    [JsonProperty("quiver")]
    [JsonPropertyName("quiver")]
    public GItem Quiver { get; init; } = null!;

    [JsonProperty("rabbitsfoot")]
    [JsonPropertyName("rabbitsfoot")]
    public GItem Rabbitsfoot { get; init; } = null!;

    [JsonProperty("rapier")]
    [JsonPropertyName("rapier")]
    public GItem Rapier { get; init; } = null!;

    [JsonProperty("rattail")]
    [JsonPropertyName("rattail")]
    public GItem Rattail { get; init; } = null!;

    [JsonProperty("redenvelope")]
    [JsonPropertyName("redenvelope")]
    public GItem Redenvelope { get; init; } = null!;

    [JsonProperty("redenvelopev2")]
    [JsonPropertyName("redenvelopev2")]
    public GItem Redenvelopev2 { get; init; } = null!;

    [JsonProperty("redenvelopev3")]
    [JsonPropertyName("redenvelopev3")]
    public GItem Redenvelopev3 { get; init; } = null!;

    [JsonProperty("redenvelopev4")]
    [JsonPropertyName("redenvelopev4")]
    public GItem Redenvelopev4 { get; init; } = null!;

    [JsonProperty("rednose")]
    [JsonPropertyName("rednose")]
    public GItem Rednose { get; init; } = null!;

    [JsonProperty("reflectionscroll")]
    [JsonPropertyName("reflectionscroll")]
    public GItem Reflectionscroll { get; init; } = null!;

    [JsonProperty("resistancering")]
    [JsonPropertyName("resistancering")]
    public GItem Resistancering { get; init; } = null!;

    [JsonProperty("resistancescroll")]
    [JsonPropertyName("resistancescroll")]
    public GItem Resistancescroll { get; init; } = null!;

    [JsonProperty("rfangs")]
    [JsonPropertyName("rfangs")]
    public GItem Rfangs { get; init; } = null!;

    [JsonProperty("rfur")]
    [JsonPropertyName("rfur")]
    public GItem Rfur { get; init; } = null!;

    [JsonProperty("ringhs")]
    [JsonPropertyName("ringhs")]
    public GItem Ringhs { get; init; } = null!;

    [JsonProperty("ringofluck")]
    [JsonPropertyName("ringofluck")]
    public GItem Ringofluck { get; init; } = null!;

    [JsonProperty("ringsj")]
    [JsonPropertyName("ringsj")]
    public GItem Ringsj { get; init; } = null!;

    [JsonProperty("rod")]
    [JsonPropertyName("rod")]
    public GItem Rod { get; init; } = null!;

    [JsonProperty("rpiercingscroll")]
    [JsonPropertyName("rpiercingscroll")]
    public GItem Rpiercingscroll { get; init; } = null!;

    [JsonProperty("sanguine")]
    [JsonPropertyName("sanguine")]
    public GItem Sanguine { get; init; } = null!;

    [JsonProperty("santasbelt")]
    [JsonPropertyName("santasbelt")]
    public GItem Santasbelt { get; init; } = null!;

    [JsonProperty("sbelt")]
    [JsonPropertyName("sbelt")]
    public GItem Sbelt { get; init; } = null!;

    [JsonProperty("scroll0")]
    [JsonPropertyName("scroll0")]
    public GItem Scroll0 { get; init; } = null!;

    [JsonProperty("scroll1")]
    [JsonPropertyName("scroll1")]
    public GItem Scroll1 { get; init; } = null!;

    [JsonProperty("scroll2")]
    [JsonPropertyName("scroll2")]
    public GItem Scroll2 { get; init; } = null!;

    [JsonProperty("scroll3")]
    [JsonPropertyName("scroll3")]
    public GItem Scroll3 { get; init; } = null!;

    [JsonProperty("scroll4")]
    [JsonPropertyName("scroll4")]
    public GItem Scroll4 { get; init; } = null!;

    [JsonProperty("scythe")]
    [JsonPropertyName("scythe")]
    public GItem Scythe { get; init; } = null!;

    [JsonProperty("seashell")]
    [JsonPropertyName("seashell")]
    public GItem Seashell { get; init; } = null!;

    [JsonProperty("shadowstone")]
    [JsonPropertyName("shadowstone")]
    public GItem Shadowstone { get; init; } = null!;

    [JsonProperty("shield")]
    [JsonPropertyName("shield")]
    public GItem Shield { get; init; } = null!;

    [JsonProperty("shoes")]
    [JsonPropertyName("shoes")]
    public GItem Shoes { get; init; } = null!;

    [JsonProperty("shoes1")]
    [JsonPropertyName("shoes1")]
    public GItem Shoes1 { get; init; } = null!;

    [JsonProperty("skullamulet")]
    [JsonPropertyName("skullamulet")]
    public GItem Skullamulet { get; init; } = null!;

    [JsonProperty("slimestaff")]
    [JsonPropertyName("slimestaff")]
    public GItem Slimestaff { get; init; } = null!;

    [JsonProperty("smoke")]
    [JsonPropertyName("smoke")]
    public GItem Smoke { get; init; } = null!;

    [JsonProperty("smush")]
    [JsonPropertyName("smush")]
    public GItem Smush { get; init; } = null!;

    [JsonProperty("snakefang")]
    [JsonPropertyName("snakefang")]
    public GItem Snakefang { get; init; } = null!;

    [JsonProperty("snakeoil")]
    [JsonPropertyName("snakeoil")]
    public GItem Snakeoil { get; init; } = null!;

    [JsonProperty("snowball")]
    [JsonPropertyName("snowball")]
    public GItem Snowball { get; init; } = null!;

    [JsonProperty("snowboots")]
    [JsonPropertyName("snowboots")]
    public GItem Snowboots { get; init; } = null!;

    [JsonProperty("snowflakes")]
    [JsonPropertyName("snowflakes")]
    public GItem Snowflakes { get; init; } = null!;

    [JsonProperty("snring")]
    [JsonPropertyName("snring")]
    public GItem Snring { get; init; } = null!;

    [JsonProperty("solitaire")]
    [JsonPropertyName("solitaire")]
    public GItem Solitaire { get; init; } = null!;

    [JsonProperty("sparkstaff")]
    [JsonPropertyName("sparkstaff")]
    public GItem Sparkstaff { get; init; } = null!;

    [JsonProperty("spear")]
    [JsonPropertyName("spear")]
    public GItem Spear { get; init; } = null!;

    [JsonProperty("spearofthedead")]
    [JsonPropertyName("spearofthedead")]
    public GItem Spearofthedead { get; init; } = null!;

    [JsonProperty("speedscroll")]
    [JsonPropertyName("speedscroll")]
    public GItem Speedscroll { get; init; } = null!;

    [JsonProperty("spiderkey")]
    [JsonPropertyName("spiderkey")]
    public GItem Spiderkey { get; init; } = null!;

    [JsonProperty("spidersilk")]
    [JsonPropertyName("spidersilk")]
    public GItem Spidersilk { get; init; } = null!;

    [JsonProperty("spikedhelmet")]
    [JsonPropertyName("spikedhelmet")]
    public GItem Spikedhelmet { get; init; } = null!;

    [JsonProperty("spookyamulet")]
    [JsonPropertyName("spookyamulet")]
    public GItem Spookyamulet { get; init; } = null!;

    [JsonProperty("spores")]
    [JsonPropertyName("spores")]
    public GItem Spores { get; init; } = null!;

    [JsonProperty("sshield")]
    [JsonPropertyName("sshield")]
    public GItem Sshield { get; init; } = null!;

    [JsonProperty("sstinger")]
    [JsonPropertyName("sstinger")]
    public GItem Sstinger { get; init; } = null!;

    [JsonProperty("staff")]
    [JsonPropertyName("staff")]
    public GItem Staff { get; init; } = null!;

    [JsonProperty("staff2")]
    [JsonPropertyName("staff2")]
    public GItem Staff2 { get; init; } = null!;

    [JsonProperty("staff3")]
    [JsonPropertyName("staff3")]
    public GItem Staff3 { get; init; } = null!;

    [JsonProperty("staff4")]
    [JsonPropertyName("staff4")]
    public GItem Staff4 { get; init; } = null!;

    [JsonProperty("staffofthedead")]
    [JsonPropertyName("staffofthedead")]
    public GItem Staffofthedead { get; init; } = null!;

    [JsonProperty("stand0")]
    [JsonPropertyName("stand0")]
    public GItem Stand0 { get; init; } = null!;

    [JsonProperty("stand1")]
    [JsonPropertyName("stand1")]
    public GItem Stand1 { get; init; } = null!;

    [JsonProperty("starkillers")]
    [JsonPropertyName("starkillers")]
    public GItem Starkillers { get; init; } = null!;

    [JsonProperty("stealthcape")]
    [JsonPropertyName("stealthcape")]
    public GItem Stealthcape { get; init; } = null!;

    [JsonProperty("stick")]
    [JsonPropertyName("stick")]
    public GItem Stick { get; init; } = null!;

    [JsonProperty("stinger")]
    [JsonPropertyName("stinger")]
    public GItem Stinger { get; init; } = null!;

    [JsonProperty("stonekey")]
    [JsonPropertyName("stonekey")]
    public GItem Stonekey { get; init; } = null!;

    [JsonProperty("stoneofgold")]
    [JsonPropertyName("stoneofgold")]
    public GItem Stoneofgold { get; init; } = null!;

    [JsonProperty("stoneofluck")]
    [JsonPropertyName("stoneofluck")]
    public GItem Stoneofluck { get; init; } = null!;

    [JsonProperty("stoneofxp")]
    [JsonPropertyName("stoneofxp")]
    public GItem Stoneofxp { get; init; } = null!;

    [JsonProperty("storagebox")]
    [JsonPropertyName("storagebox")]
    public GItem Storagebox { get; init; } = null!;

    [JsonProperty("stramulet")]
    [JsonPropertyName("stramulet")]
    public GItem Stramulet { get; init; } = null!;

    [JsonProperty("strbelt")]
    [JsonPropertyName("strbelt")]
    public GItem Strbelt { get; init; } = null!;

    [JsonProperty("strearring")]
    [JsonPropertyName("strearring")]
    public GItem Strearring { get; init; } = null!;

    [JsonProperty("strring")]
    [JsonPropertyName("strring")]
    public GItem Strring { get; init; } = null!;

    [JsonProperty("strscroll")]
    [JsonPropertyName("strscroll")]
    public GItem Strscroll { get; init; } = null!;

    [JsonProperty("suckerpunch")]
    [JsonPropertyName("suckerpunch")]
    public GItem Suckerpunch { get; init; } = null!;

    [JsonProperty("supercomputer")]
    [JsonPropertyName("supercomputer")]
    public GItem Supercomputer { get; init; } = null!;

    [JsonProperty("supermittens")]
    [JsonPropertyName("supermittens")]
    public GItem Supermittens { get; init; } = null!;

    [JsonProperty("svenom")]
    [JsonPropertyName("svenom")]
    public GItem Svenom { get; init; } = null!;

    [JsonProperty("sweaterhs")]
    [JsonPropertyName("sweaterhs")]
    public GItem Sweaterhs { get; init; } = null!;

    [JsonProperty("swifty")]
    [JsonPropertyName("swifty")]
    public GItem Swifty { get; init; } = null!;

    [JsonProperty("swirlipop")]
    [JsonPropertyName("swirlipop")]
    public GItem Swirlipop { get; init; } = null!;

    [JsonProperty("sword")]
    [JsonPropertyName("sword")]
    public GItem Sword { get; init; } = null!;

    [JsonProperty("swordofthedead")]
    [JsonPropertyName("swordofthedead")]
    public GItem Swordofthedead { get; init; } = null!;

    [JsonProperty("t2bow")]
    [JsonPropertyName("t2bow")]
    public GItem T2Bow { get; init; } = null!;

    [JsonProperty("t2dexamulet")]
    [JsonPropertyName("t2dexamulet")]
    public GItem T2Dexamulet { get; init; } = null!;

    [JsonProperty("t2intamulet")]
    [JsonPropertyName("t2intamulet")]
    public GItem T2Intamulet { get; init; } = null!;

    [JsonProperty("t2quiver")]
    [JsonPropertyName("t2quiver")]
    public GItem T2Quiver { get; init; } = null!;

    [JsonProperty("t2stramulet")]
    [JsonPropertyName("t2stramulet")]
    public GItem T2Stramulet { get; init; } = null!;

    [JsonProperty("t3bow")]
    [JsonPropertyName("t3bow")]
    public GItem T3Bow { get; init; } = null!;

    [JsonProperty("talkingskull")]
    [JsonPropertyName("talkingskull")]
    public GItem Talkingskull { get; init; } = null!;

    [JsonProperty("test")]
    [JsonPropertyName("test")]
    public GItem Test { get; init; } = null!;

    [JsonProperty("test2")]
    [JsonPropertyName("test2")]
    public GItem Test2 { get; init; } = null!;

    [JsonProperty("test_orb")]
    [JsonPropertyName("test_orb")]
    public GItem TestOrb { get; init; } = null!;

    [JsonProperty("throwingstars")]
    [JsonPropertyName("throwingstars")]
    public GItem Throwingstars { get; init; } = null!;

    [JsonProperty("tigercape")]
    [JsonPropertyName("tigercape")]
    public GItem Tigercape { get; init; } = null!;

    [JsonProperty("tigerhelmet")]
    [JsonPropertyName("tigerhelmet")]
    public GItem Tigerhelmet { get; init; } = null!;

    [JsonProperty("tigershield")]
    [JsonPropertyName("tigershield")]
    public GItem Tigershield { get; init; } = null!;

    [JsonProperty("tigerstone")]
    [JsonPropertyName("tigerstone")]
    public GItem Tigerstone { get; init; } = null!;

    [JsonProperty("tombkey")]
    [JsonPropertyName("tombkey")]
    public GItem Tombkey { get; init; } = null!;

    [JsonProperty("tracker")]
    [JsonPropertyName("tracker")]
    public GItem Tracker { get; init; } = null!;

    [JsonProperty("trigger")]
    [JsonPropertyName("trigger")]
    public GItem Trigger { get; init; } = null!;

    [JsonProperty("trinkets")]
    [JsonPropertyName("trinkets")]
    public GItem Trinkets { get; init; } = null!;

    [JsonProperty("tristone")]
    [JsonPropertyName("tristone")]
    public GItem Tristone { get; init; } = null!;

    [JsonProperty("troll")]
    [JsonPropertyName("troll")]
    public GItem Troll { get; init; } = null!;

    [JsonProperty("tshell")]
    [JsonPropertyName("tshell")]
    public GItem Tshell { get; init; } = null!;

    [JsonProperty("tshirt0")]
    [JsonPropertyName("tshirt0")]
    public GItem Tshirt0 { get; init; } = null!;

    [JsonProperty("tshirt1")]
    [JsonPropertyName("tshirt1")]
    public GItem Tshirt1 { get; init; } = null!;

    [JsonProperty("tshirt2")]
    [JsonPropertyName("tshirt2")]
    public GItem Tshirt2 { get; init; } = null!;

    [JsonProperty("tshirt3")]
    [JsonPropertyName("tshirt3")]
    public GItem Tshirt3 { get; init; } = null!;

    [JsonProperty("tshirt4")]
    [JsonPropertyName("tshirt4")]
    public GItem Tshirt4 { get; init; } = null!;

    [JsonProperty("tshirt6")]
    [JsonPropertyName("tshirt6")]
    public GItem Tshirt6 { get; init; } = null!;

    [JsonProperty("tshirt7")]
    [JsonPropertyName("tshirt7")]
    public GItem Tshirt7 { get; init; } = null!;

    [JsonProperty("tshirt8")]
    [JsonPropertyName("tshirt8")]
    public GItem Tshirt8 { get; init; } = null!;

    [JsonProperty("tshirt88")]
    [JsonPropertyName("tshirt88")]
    public GItem Tshirt88 { get; init; } = null!;

    [JsonProperty("tshirt9")]
    [JsonPropertyName("tshirt9")]
    public GItem Tshirt9 { get; init; } = null!;

    [JsonProperty("ukey")]
    [JsonPropertyName("ukey")]
    public GItem Ukey { get; init; } = null!;

    [JsonProperty("vattire")]
    [JsonPropertyName("vattire")]
    public GItem Vattire { get; init; } = null!;

    [JsonProperty("vblood")]
    [JsonPropertyName("vblood")]
    public GItem Vblood { get; init; } = null!;

    [JsonProperty("vboots")]
    [JsonPropertyName("vboots")]
    public GItem Vboots { get; init; } = null!;

    [JsonProperty("vcape")]
    [JsonPropertyName("vcape")]
    public GItem Vcape { get; init; } = null!;

    [JsonProperty("vdagger")]
    [JsonPropertyName("vdagger")]
    public GItem Vdagger { get; init; } = null!;

    [JsonProperty("vgloves")]
    [JsonPropertyName("vgloves")]
    public GItem Vgloves { get; init; } = null!;

    [JsonProperty("vhammer")]
    [JsonPropertyName("vhammer")]
    public GItem Vhammer { get; init; } = null!;

    [JsonProperty("vitearring")]
    [JsonPropertyName("vitearring")]
    public GItem Vitearring { get; init; } = null!;

    [JsonProperty("vitring")]
    [JsonPropertyName("vitring")]
    public GItem Vitring { get; init; } = null!;

    [JsonProperty("vitscroll")]
    [JsonPropertyName("vitscroll")]
    public GItem Vitscroll { get; init; } = null!;

    [JsonProperty("vorb")]
    [JsonPropertyName("vorb")]
    public GItem Vorb { get; init; } = null!;

    [JsonProperty("vring")]
    [JsonPropertyName("vring")]
    public GItem Vring { get; init; } = null!;

    [JsonProperty("vstaff")]
    [JsonPropertyName("vstaff")]
    public GItem Vstaff { get; init; } = null!;

    [JsonProperty("vsword")]
    [JsonPropertyName("vsword")]
    public GItem Vsword { get; init; } = null!;

    [JsonProperty("wand")]
    [JsonPropertyName("wand")]
    public GItem Wand { get; init; } = null!;

    [JsonProperty("warmscarf")]
    [JsonPropertyName("warmscarf")]
    public GItem Warmscarf { get; init; } = null!;

    [JsonProperty("warpvest")]
    [JsonPropertyName("warpvest")]
    public GItem Warpvest { get; init; } = null!;

    [JsonProperty("watercore")]
    [JsonPropertyName("watercore")]
    public GItem Watercore { get; init; } = null!;

    [JsonProperty("wattire")]
    [JsonPropertyName("wattire")]
    public GItem Wattire { get; init; } = null!;

    [JsonProperty("wbasher")]
    [JsonPropertyName("wbasher")]
    public GItem Wbasher { get; init; } = null!;

    [JsonProperty("wblade")]
    [JsonPropertyName("wblade")]
    public GItem Wblade { get; init; } = null!;

    [JsonProperty("wbook0")]
    [JsonPropertyName("wbook0")]
    public GItem Wbook0 { get; init; } = null!;

    [JsonProperty("wbook1")]
    [JsonPropertyName("wbook1")]
    public GItem Wbook1 { get; init; } = null!;

    [JsonProperty("wbookhs")]
    [JsonPropertyName("wbookhs")]
    public GItem Wbookhs { get; init; } = null!;

    [JsonProperty("wbreeches")]
    [JsonPropertyName("wbreeches")]
    public GItem Wbreeches { get; init; } = null!;

    [JsonProperty("wcap")]
    [JsonPropertyName("wcap")]
    public GItem Wcap { get; init; } = null!;

    [JsonProperty("weaponbox")]
    [JsonPropertyName("weaponbox")]
    public GItem Weaponbox { get; init; } = null!;

    [JsonProperty("weaver")]
    [JsonPropertyName("weaver")]
    public GItem Weaver { get; init; } = null!;

    [JsonProperty("wgloves")]
    [JsonPropertyName("wgloves")]
    public GItem Wgloves { get; init; } = null!;

    [JsonProperty("whiskey")]
    [JsonPropertyName("whiskey")]
    public GItem Whiskey { get; init; } = null!;

    [JsonProperty("whiteegg")]
    [JsonPropertyName("whiteegg")]
    public GItem Whiteegg { get; init; } = null!;

    [JsonProperty("wine")]
    [JsonPropertyName("wine")]
    public GItem Wine { get; init; } = null!;

    [JsonProperty("wingedboots")]
    [JsonPropertyName("wingedboots")]
    public GItem Wingedboots { get; init; } = null!;

    [JsonProperty("woodensword")]
    [JsonPropertyName("woodensword")]
    public GItem Woodensword { get; init; } = null!;

    [JsonProperty("wshield")]
    [JsonPropertyName("wshield")]
    public GItem Wshield { get; init; } = null!;

    [JsonProperty("wshoes")]
    [JsonPropertyName("wshoes")]
    public GItem Wshoes { get; init; } = null!;

    [JsonProperty("x0")]
    [JsonPropertyName("x0")]
    public GItem X0 { get; init; } = null!;

    [JsonProperty("x1")]
    [JsonPropertyName("x1")]
    public GItem X1 { get; init; } = null!;

    [JsonProperty("x2")]
    [JsonPropertyName("x2")]
    public GItem X2 { get; init; } = null!;

    [JsonProperty("x3")]
    [JsonPropertyName("x3")]
    public GItem X3 { get; init; } = null!;

    [JsonProperty("x4")]
    [JsonPropertyName("x4")]
    public GItem X4 { get; init; } = null!;

    [JsonProperty("x5")]
    [JsonPropertyName("x5")]
    public GItem X5 { get; init; } = null!;

    [JsonProperty("x6")]
    [JsonPropertyName("x6")]
    public GItem X6 { get; init; } = null!;

    [JsonProperty("x7")]
    [JsonPropertyName("x7")]
    public GItem X7 { get; init; } = null!;

    [JsonProperty("x8")]
    [JsonPropertyName("x8")]
    public GItem X8 { get; init; } = null!;

    [JsonProperty("xarmor")]
    [JsonPropertyName("xarmor")]
    public GItem Xarmor { get; init; } = null!;

    [JsonProperty("xboots")]
    [JsonPropertyName("xboots")]
    public GItem Xboots { get; init; } = null!;

    [JsonProperty("xbox")]
    [JsonPropertyName("xbox")]
    public GItem Xbox { get; init; } = null!;

    [JsonProperty("xgloves")]
    [JsonPropertyName("xgloves")]
    public GItem Xgloves { get; init; } = null!;

    [JsonProperty("xhelmet")]
    [JsonPropertyName("xhelmet")]
    public GItem Xhelmet { get; init; } = null!;

    [JsonProperty("xmace")]
    [JsonPropertyName("xmace")]
    public GItem Xmace { get; init; } = null!;

    [JsonProperty("xmashat")]
    [JsonPropertyName("xmashat")]
    public GItem Xmashat { get; init; } = null!;

    [JsonProperty("xmaspants")]
    [JsonPropertyName("xmaspants")]
    public GItem Xmaspants { get; init; } = null!;

    [JsonProperty("xmasshoes")]
    [JsonPropertyName("xmasshoes")]
    public GItem Xmasshoes { get; init; } = null!;

    [JsonProperty("xmassweater")]
    [JsonPropertyName("xmassweater")]
    public GItem Xmassweater { get; init; } = null!;

    [JsonProperty("xpants")]
    [JsonPropertyName("xpants")]
    public GItem Xpants { get; init; } = null!;

    [JsonProperty("xpbooster")]
    [JsonPropertyName("xpbooster")]
    public GItem Xpbooster { get; init; } = null!;

    [JsonProperty("xpscroll")]
    [JsonPropertyName("xpscroll")]
    public GItem Xpscroll { get; init; } = null!;

    [JsonProperty("xptome")]
    [JsonPropertyName("xptome")]
    public GItem Xptome { get; init; } = null!;

    [JsonProperty("xshield")]
    [JsonPropertyName("xshield")]
    public GItem Xshield { get; init; } = null!;

    [JsonProperty("xshot")]
    [JsonPropertyName("xshot")]
    public GItem Xshot { get; init; } = null!;

    [JsonProperty("zapper")]
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