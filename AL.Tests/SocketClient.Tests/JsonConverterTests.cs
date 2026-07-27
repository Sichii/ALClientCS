#region
using AL.APIClient.Model;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

public class JsonConverterTests
{
    [Test]
    public void DeserializeAchievementProgressDataTest()
    {
        const string ACHIEVEMENT_PROGRESS_DATA = @"{
   ""name"":""firehazard"",
   ""count"":""25"",
   ""needed"":""19975""
}";

        var obj = TestJson.Socket<AchievementProgressData>(ACHIEVEMENT_PROGRESS_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeActionDataTest()
    {
        const string ACTION_DATA = @"{
   ""attacker"":""2144160"",
   ""target"":""Moneybaggers"",
   ""type"":""attack"",
   ""source"":""attack"",
   ""x"":595.7417319170224,
   ""y"":1091.179435638155,
   ""eta"":400,
   ""m"":361,
   ""pid"":""wMhQBT"",
   ""projectile"":""stone"",
   ""damage"":25
}";

        var obj = TestJson.Socket<ActionData>(ACTION_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeCharacterDataTest()
    {
        var obj = TestJson.Socket<CharacterData>(Fixture.ReadCommittedSnapshot("character-frame.json")!);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeChestOpenedDataTest()
    {
        const string CHEST_OPENED_DATA = @"{
   ""id"":""lUHVxKFHEl85OVZf5weJNBii0pip9x"",
   ""goldm"":1.01,
   ""opener"":""makiz"",
   ""items"":[
      {
         ""name"":""ringsj"",
         ""level"":0,
         ""looter"":""makiz""
      },
      {
         ""name"":""seashell"",
         ""q"":1,
         ""looter"":""makiz""
      }
   ],
   ""gold"":1655
}";

        var obj = TestJson.Socket<ChestOpenedData>(CHEST_OPENED_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeCooperativeDeathTest()
    {
        // a cooperative-boss death sends 'points' as a per-contributor OBJECT, not a scalar
        const string DEATH_DATA = @"{
   ""id"":""12345"",
   ""luckm"":1,
   ""points"":{""alice"":500,""bob"":300}
}";

        var obj = TestJson.Socket<DeathData>(DEATH_DATA);

        obj.Should()
           .NotBeNull();

        obj.Points
           .Should()
           .NotBeNull();

        obj.Points!.Count
           .Should()
           .Be(2);

        obj.Points["alice"]
           .Should()
           .Be(500f);
    }

    [Test]
    public void DeserializeCorrectionDataTest()
    {
        const string CORRECTION_DATA = @"{
   ""x"":64.123,
   ""y"":792.456,
}";

        var obj = TestJson.Socket<CorrectionData>(CORRECTION_DATA);

        obj.Should()
           .NotBeNull();

        (new Point(64.123f, 792.456f).Distance(obj) < 10).Should()
                                                         .BeTrue();
    }

    [Test]
    public void DeserializeDeathDataTest()
    {
        const string DEATH_DATA = @"{
   ""id"":""43923109""
}";

        var obj = TestJson.Socket<DeathData>(DEATH_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeDisappearDataTest()
    {
        const string DISAPPEAR_DATA = @"{
   ""id"":""CeeNote"",
   ""reason"":""transport"",
   ""s"":1
}";

        const string DISAPPEAR_DATA2 = @"{
   ""id"":""CeeNote"",
   ""reason"":""transport"",
   ""s"":[5, 10, 1]
}";

        var obj = TestJson.Socket<DisappearData>(DISAPPEAR_DATA);
        var obj2 = TestJson.Socket<DisappearData>(DISAPPEAR_DATA2);

        obj.Should()
           .NotBeNull();

        obj2.Should()
            .NotBeNull();
    }

    [Test]
    public void DeserializeDisappearingTextData()
    {
        const string DISAPPEARING_TEXT_DATA = @"{
   ""message"":""+100"",
   ""x"":15.280000219733335,
   ""y"":-413.5399998351999,
   ""id"":""Ploob"",
   ""args"":{
      ""c"":""#006AA9"",
      ""s"":""mp""
   }
}";

        var obj = TestJson.Socket<DisappearingTextData>(DISAPPEARING_TEXT_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeDropDataTest()
    {
        const string DROP_DATA = @"{
   ""x"":64,
   ""y"":792,
   ""items"":0,
   ""chest"":""chest3"",
   ""id"":""pcgm7Iavbtug7zO8xQoumIXaIVFZPL"",
   ""map"":""main""
}";

        var obj = TestJson.Socket<DropData>(DROP_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeEntitiesDataTest()
    {
        var obj = TestJson.Socket<EntitiesData>(Fixture.ReadCommittedSnapshot("t11-entities-frame.json")!);

        obj.Should()
           .NotBeNull();

        obj.Players
           .Any()
           .Should()
           .BeTrue();

        obj.Monsters
           .Any()
           .Should()
           .BeTrue();
    }

    [Test]
    public void DeserializeEvalDataTest()
    {
        const string EVAL_DATA = @"{
   ""code"":""skill_timeout('attack',1096.1809388171202)""
}";

        var obj = TestJson.Socket<EvalData>(EVAL_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeEventAndBossDataTest()
    {
        const string EVENT_AND_BOSS_DATA = @"{
   ""icegolem"":{
      ""live"":true,
      ""map"":""winterland"",
      ""hp"":16000000,
      ""max_hp"":16000000,
      ""x"":808.9124940370274,
      ""y"":407.6040564394661
   },
   ""snowman"":{
      ""live"":true,
      ""map"":""winterland"",
      ""hp"":1200,
      ""max_hp"":1200,
      ""x"":1111.7317564125299,
      ""y"":-785.8382420118533
   },
   ""franky"":{
      ""live"":true,
      ""map"":""level2w"",
      ""hp"":120000000,
      ""max_hp"":120000000,
      ""x"":-278.0075274742135,
      ""y"":187.81118535586882
   }
}";

        var obj = TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeGameLogDataTest()
    {
        const string GAME_LOG_DATA = "\"Stored 37,500,711 gold\"";

        var obj = TestJson.Socket<string>(GAME_LOG_DATA);

        obj.Should()
           .Be("Stored 37,500,711 gold");
    }

    [Test]
    public void DeserializeGameResponseDataTest()
    {
        const string STRING_GAME_RESPONSE_DATA = @"""ex_condition""";

        const string GAME_RESPONSE_DATA = @"{
   ""response"":""ex_condition"",
   ""name"":""charging""
}";

        var obj = TestJson.Socket<GameResponseData>(GAME_RESPONSE_DATA);

        obj.Should()
           .NotBeNull();

        obj.Name
           .Should()
           .NotBeNull();
        obj = TestJson.Socket<GameResponseData>(STRING_GAME_RESPONSE_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeHitDataTest()
    {
        const string HIT_DATA = @"{
   ""attacker"":""2144160"",
   ""target"":""Moneybaggers"",
   ""type"":""attack"",
   ""source"":""attack"",
   ""x"":595.7417319170224,
   ""y"":1091.179435638155,
   ""eta"":400,
   ""m"":361,
   ""pid"":""wMhQBT"",
   ""projectile"":""stone"",
   ""damage"":25
}";

        var obj = TestJson.Socket<HitData>(HIT_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeInviteDataTest()
    {
        const string INVITE_DATA = @"{
   ""name"":""earthMer""
}";

        var obj = TestJson.Socket<InviteData>(INVITE_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeMailItemStringAndObjectTest()
    {
        // the mail item is polymorphic: a JSON object, or a JSON-stringified object (simplify_item)
        const string MAIL_OBJECT = @"{ ""item"": { ""name"":""hpamulet"", ""level"":2, ""q"":3 } }";
        const string MAIL_STRING = @"{ ""item"": ""{\""name\"":\""hpamulet\"",\""level\"":2}"" }";

        var fromObject = TestJson.Api<Mail>(MAIL_OBJECT);
        var fromString = TestJson.Api<Mail>(MAIL_STRING);

        fromObject!.Item
                   .Should()
                   .NotBeNull();

        fromObject.Item!.Name
                  .Should()
                  .Be("hpamulet");

        fromObject.Item
                  .Quantity
                  .Should()
                  .Be(3);

        fromString!.Item
                   .Should()
                   .NotBeNull();

        fromString.Item!.Name
                  .Should()
                  .Be("hpamulet");

        fromString.Item
                  .Level
                  .Should()
                  .Be(2);
    }

    [Test]
    public void DeserializeNewMapDataTest()
    {
        const string NEW_MAP_DATA = @"{
   ""name"":""bank"",
   ""in"":""bank"",
   ""x"":0,
   ""y"":-37,
   ""direction"":3,
   ""effect"":0,
   ""info"":{
      
   },
   ""m"":1,
   ""entities"":{
      ""players"":[
         {
            ""hp"":7618,
            ""max_hp"":7618,
            ""mp"":4003,
            ""max_mp"":4445,
            ""xp"":391935615,
            ""attack"":123,
            ""frequency"":1.074238046844754,
            ""speed"":86,
            ""range"":25,
            ""armor"":167,
            ""resistance"":471,
            ""level"":76,
            ""rip"":false,
            ""code"":false,
            ""afk"":false,
            ""s"":{
               
            },
            ""c"":{
               
            },
            ""q"":{
               
            },
            ""age"":833,
            ""pdps"":0.002169416492913066,
            ""id"":""CeeNote"",
            ""x"":0,
            ""y"":-328.2299951754007,
            ""moving"":false,
            ""going_x"":0,
            ""going_y"":-328.2299951754007,
            ""abs"":false,
            ""move_num"":17365217,
            ""angle"":-90,
            ""cid"":1161,
            ""stand"":false,
            ""skin"":""marmor2g"",
            ""cx"":{
               ""hair"":""hairdo417"",
               ""head"":""mmakeup00""
            },
            ""slots"":{
               ""ring1"":{
                  ""l"":""l"",
                  ""name"":""ringsj"",
                  ""level"":4
               },
               ""ring2"":{
                  ""name"":""ringsj"",
                  ""level"":4
               },
               ""earring1"":{
                  ""name"":""intearring"",
                  ""level"":4
               },
               ""earring2"":{
                  ""name"":""vitearring"",
                  ""level"":3,
                  ""l"":""l""
               },
               ""belt"":{
                  ""name"":""santasbelt"",
                  ""level"":3
               },
               ""mainhand"":null,
               ""offhand"":null,
               ""helmet"":{
                  ""name"":""eears"",
                  ""level"":9
               },
               ""chest"":{
                  ""level"":9,
                  ""stat_type"":""dex"",
                  ""name"":""wattire""
               },
               ""pants"":{
                  ""level"":9,
                  ""stat_type"":""int"",
                  ""name"":""wbreeches""
               },
               ""shoes"":{
                  ""name"":""wingedboots"",
                  ""level"":8
               },
               ""gloves"":null,
               ""amulet"":{
                  ""name"":""warmscarf"",
                  ""level"":9
               },
               ""orb"":null,
               ""elixir"":null,
               ""cape"":{
                  ""level"":0,
                  ""name"":""stealthcape""
               },
               ""trade1"":null,
               ""trade2"":null,
               ""trade3"":null,
               ""trade4"":null
            },
            ""ctype"":""merchant"",
            ""owner"":""4667600492560384""
         },
         {
            ""hp"":7826,
            ""max_hp"":7826,
            ""mp"":2020,
            ""max_mp"":2060,
            ""xp"":10375841,
            ""attack"":310,
            ""frequency"":0.670161149825784,
            ""speed"":75,
            ""range"":32,
            ""armor"":141,
            ""resistance"":221,
            ""level"":54,
            ""rip"":false,
            ""afk"":false,
            ""target"":""2156802"",
            ""focus"":null,
            ""s"":{
               ""mluck"":{
                  ""ms"":709000,
                  ""f"":""Dinger""
               }
            },
            ""c"":{
               
            },
            ""q"":{
               
            },
            ""age"":69,
            ""pdps"":0,
            ""id"":""sichi"",
            ""x"":0,
            ""y"":-37,
            ""moving"":false,
            ""going_x"":151.07654357633953,
            ""going_y"":-144.9999999,
            ""abs"":false,
            ""move_num"":17364945,
            ""angle"":-64.12607348356174,
            ""cid"":8,
            ""stand"":false,
            ""skin"":""sarmor2c"",
            ""cx"":{
               ""hat"":""hat322""
            },
            ""slots"":{
               ""ring1"":{
                  ""name"":""vitring"",
                  ""level"":3
               },
               ""ring2"":{
                  ""name"":""ringsj"",
                  ""level"":3
               },
               ""earring1"":{
                  ""level"":0,
                  ""name"":""vitearring""
               },
               ""earring2"":{
                  ""name"":""vitearring"",
                  ""level"":1
               },
               ""belt"":{
                  ""level"":0,
                  ""m"":""Chonk003"",
                  ""name"":""hpbelt""
               },
               ""mainhand"":{
                  ""name"":""carrotsword"",
                  ""level"":7
               },
               ""offhand"":null,
               ""helmet"":{
                  ""stat_type"":""vit"",
                  ""name"":""eears"",
                  ""level"":7
               },
               ""chest"":{
                  ""level"":7,
                  ""stat_type"":""vit"",
                  ""name"":""epyjamas""
               },
               ""pants"":{
                  ""level"":3,
                  ""stat_type"":""vit"",
                  ""name"":""wbreeches""
               },
               ""shoes"":{
                  ""level"":8,
                  ""stat_type"":""vit"",
                  ""name"":""eslippers""
               },
               ""gloves"":{
                  ""level"":7,
                  ""stat_type"":""vit"",
                  ""name"":""wgloves""
               },
               ""amulet"":{
                  ""level"":6,
                  ""name"":""warmscarf""
               },
               ""orb"":{
                  ""level"":0,
                  ""name"":""test_orb""
               },
               ""elixir"":null,
               ""cape"":{
                  ""level"":5,
                  ""stat_type"":""vit"",
                  ""name"":""angelwings""
               },
               ""trade1"":null,
               ""trade2"":null,
               ""trade3"":null,
               ""trade4"":{
                  ""name"":""staff"",
                  ""price"":40000000,
                  ""rid"":""TfCh"",
                  ""level"":9
               }
            },
            ""ctype"":""merchant"",
            ""owner"":""6000633860063232""
         }
      ],
      ""monsters"":[
         
      ],
      ""type"":""all"",
      ""in"":""bank"",
      ""map"":""bank""
   }
}";

        var obj = TestJson.Socket<NewMapData>(NEW_MAP_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializePartyUpdateDataTest()
    {
        const string PARTY_UPDATE_DATA = @"{
   ""list"":[
      ""earthMer"",
      ""myranger"",
      ""sichi""
   ],
   ""party"":{
      ""earthMer"":{
         ""skin"":""marmor12a"",
         ""level"":74,
         ""type"":""merchant"",
         ""x"":153.3969219854852,
         ""y"":-124.03322858515753,
         ""in"":""main"",
         ""map"":""main"",
         ""share"":0,
         ""pdps"":2.403978062438949e-15,
         ""l"":1,
         ""xp"":0,
         ""luck"":10,
         ""gold"":5,
         ""cx"":{
            ""hair"":""hairdo520"",
            ""head"":""fmakeup01""
         }
      },
      ""myranger"":{
         ""skin"":""marmor5a"",
         ""level"":44,
         ""type"":""ranger"",
         ""x"":-7.871881376000971,
         ""y"":57.14241936867256,
         ""in"":""main"",
         ""map"":""main"",
         ""share"":0.9999999899997225,
         ""pdps"":0,
         ""l"":1,
         ""xp"":0,
         ""luck"":10,
         ""gold"":5,
         ""cx"":{
            ""hair"":""hairdo106"",
            ""head"":""makeup117""
         }
      },
      ""sichi"":{
         ""skin"":""sarmor2c"",
         ""level"":54,
         ""type"":""merchant"",
         ""x"":-86.51955384018893,
         ""y"":-95.46821799675385,
         ""in"":""main"",
         ""map"":""main"",
         ""share"":0,
         ""pdps"":0,
         ""l"":1,
         ""xp"":0,
         ""luck"":10,
         ""gold"":5,
         ""cx"":{
            ""hat"":""hat322""
         }
      }
   },
   ""message"":""sichi joined the party""
}";

        var obj = TestJson.Socket<PartyUpdateData>(PARTY_UPDATE_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializePartyUpdateLeaveTest()
    {
        const string PARTY_UPDATE = @"{
   ""message"":""Bob left the party"",
   ""leave"":1
}";

        var obj = TestJson.Socket<PartyUpdateData>(PARTY_UPDATE);

        obj.Should()
           .NotBeNull();

        obj.Leave
           .Should()
           .BeTrue();
    }

    [Test]
    public void DeserializePingAckDataTest()
    {
        const string PING_ACK_DATA = @"{
   ""id"":""aKHAz""
}";

        var obj = TestJson.Socket<PingAckData>(PING_ACK_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializePredictionFailureTest()
    {
        // the server sends 'failure' as boolean true (not a numeric level) on a revealed-fail prediction
        const string PREDICTION = @"{
   ""name"":""hpamulet"",
   ""level"":7,
   ""chance"":0.4,
   ""scroll"":""scroll0"",
   ""failure"":true,
   ""nums"":[1,2]
}";

        var obj = TestJson.Socket<Prediction>(PREDICTION);

        obj.Should()
           .NotBeNull();

        obj.Failure
           .Should()
           .Be(true);
    }

    [Test]
    public void DeserializeQueueActionDataTest()
    {
        const string QUEUED_ACTION_DATA = @"{
   ""q"":{
      ""upgrade"":{
         ""ms"":161,
         ""len"":500,
         ""num"":1
      }
   },
   ""num"":1,
   ""p"":{
      ""chance"":0.9999999,
      ""name"":""blade"",
      ""level"":0,
      ""scroll"":""scroll0"",
      ""nums"":[
         1,
         0
      ]
   }
}";

        var obj = TestJson.Socket<QueuedActionData>(QUEUED_ACTION_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeQueuedActionResultDataTest()
    {
        const string QUEUED_ACTION_RESULT_DATA = @"{
   ""type"":""compound"",
   ""success"":1
}";

        var obj = TestJson.Socket<QueuedActionResultData>(QUEUED_ACTION_RESULT_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeSecondHandsDataTest()
    {
        const string SECONDHANDS_DATA = @"[
   {
      ""q"":9999,
      ""rid"":""AbUEw"",
      ""name"":""vitscroll""
   },
   {
      ""acc"":1,
      ""rid"":""vZCHK"",
      ""level"":3,
      ""name"":""spear""
   },
   {
      ""rid"":""HhP6B"",
      ""level"":4,
      ""name"":""spear""
   },
   {
      ""rid"":""Pg120"",
      ""m"":""MageMain"",
      ""name"":""hpbelt"",
      ""level"":0
   },
   {
      ""rid"":""vwKKn"",
      ""stat_type"":""int"",
      ""name"":""helmet1"",
      ""level"":4
   },
   {
      ""rid"":""o10eu"",
      ""name"":""fieldgen0""
   }
]";

        var obj = TestJson.Socket<TradeItem[]>(SECONDHANDS_DATA);

        obj.Should()
           .NotBeNull();

        //one item per distinct key-set the live feed produces - the shapes are the coverage, the volume was not
        obj.Length
           .Should()
           .Be(6);
    }

    [Test]
    public void DeserializeStartDataTest()
    {
        var obj = TestJson.Socket<StartData>(Fixture.ReadCommittedSnapshot("t11-start-frame.json")!);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeTrackDataArrayTest()
    {
        const string TRACK = @"[
   {""sound"":""rr"",""dist"":12.5,""invis"":true},
   {""sound"":""wmp"",""dist"":200.0}
]";

        var obj = TestJson.Socket<List<TrackData>>(TRACK);

        obj.Should()
           .NotBeNull();

        obj.Count
           .Should()
           .Be(2);

        obj[0]
            .Sound
            .Should()
            .Be("rr");

        obj[0]
            .Invis
            .Should()
            .BeTrue();

        obj[1]
            .Invis
            .Should()
            .BeFalse();
    }

    [Test]
    public void DeserializeTradeHistoryTest()
    {
        // the giveaway row's 4th element is null on the wire (giveaways carry no price) - the real payload shape
        const string TRADE_HISTORY = @"[
   [""sell"",""Bob"",{""name"":""hppot"",""q"":5},1000],
   [""giveaway"",""Alice"",{""name"":""hpamulet""},null]
]";

        var obj = TestJson.Socket<TradeHistoryEntry[]>(TRADE_HISTORY);

        obj.Should()
           .NotBeNull();

        obj.Length
           .Should()
           .Be(2);

        obj[0]
            .Event
            .Should()
            .Be("sell");

        obj[0]
            .PartnerName
            .Should()
            .Be("Bob");

        obj[0]
            .Price
            .Should()
            .Be(1000L);

        obj[0]
            .Item
            .Should()
            .NotBeNull();

        obj[1]
            .Event
            .Should()
            .Be("giveaway");

        obj[1]
            .Price
            .Should()
            .BeNull("a giveaway entry carries a null price");
    }

    [Test]
    public void DeserializeUIDataTest()
    {
        const string UI_DATA = @"{
   ""type"":""+$"",
   ""id"":""scrolls"",
   ""name"":""earthMag2"",
   ""item"":{
      ""name"":""mpot1"",
      ""q"":1
   }
}";

        var obj = TestJson.Socket<UIData>(UI_DATA);

        obj.Should()
           .NotBeNull();
    }

    [Test]
    public void DeserializeWelcomeDataTest()
    {
        const string WELCOME_DATA = @"{
   ""region"":""US"",
   ""name"":""III"",
   ""pvp"":false,
   ""gameplay"":""normal"",
   ""info"":{
      
   },
   ""x"":-274,
   ""y"":-1177,
   ""map"":""desertland"",
   ""in"":""desertland""
}";

        var obj = TestJson.Socket<WelcomeData>(WELCOME_DATA);

        obj.Should()
           .NotBeNull();
    }
}