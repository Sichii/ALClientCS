using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AL.Client.Helpers;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PATHFINDING_CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;

namespace AL.Tests.Client.Tests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void CalculateDamageMultiplierTest()
        {
            var defense = -160;
            var points = new List<Point>();

            for (; defense < 2000; defense += 5)
            {
                var damageMult = Utilities.CalculateDamageMultiplier(defense);
                points.Add(new Point(defense, Convert.ToInt32(damageMult * 100f)));
            }

            Assert.IsTrue(points[0] == new Point(-160, 112));
            Assert.IsTrue(points.Last() == new Point(1995, 5));
        }

        [TestMethod]
        public async Task DynamicDelayTest()
        {
            var delay = new DynamicDelay();

            var delayTask = delay.WaitAsync(5000);

            await Task.Delay(2000).ConfigureAwait(false);
            await delay.SetDelayAsync(10000).ConfigureAwait(false);
            var timer = Stopwatch.StartNew();
            await delayTask.ConfigureAwait(false);
            timer.Stop();

            var elapsed = timer.ElapsedMilliseconds;

            Assert.IsTrue(elapsed > 9000);
        }

        [TestMethod]
        public void ShallowMergeIntoTest()
        {
            const string CHARACTER_DATA = @"{
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
   ""s"":{
      ""mluck"":{
         ""ms"":1484195,
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
   ""x"":40.40316655490353,
   ""y"":541.1426134776386,
   ""cid"":2,
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
   ""owner"":""6000633860063232"",
   ""int"":90,
   ""str"":19,
   ""dex"":52,
   ""vit"":89,
   ""for"":3,
   ""mp_cost"":65,
   ""mp_reduction"":20,
   ""max_xp"":13000000,
   ""goldm"":1.01,
   ""xpm"":1.01,
   ""luckm"":1.17,
   ""map"":""main"",
   ""in"":""main"",
   ""isize"":42,
   ""esize"":26,
   ""gold"":37500000,
   ""cash"":427,
   ""targets"":0,
   ""m"":0,
   ""evasion"":4.45,
   ""miss"":0,
   ""reflection"":0,
   ""lifesteal"":0,
   ""manasteal"":0,
   ""rpiercing"":13,
   ""apiercing"":13,
   ""crit"":0,
   ""critdamage"":0,
   ""dreturn"":0,
   ""tax"":0.03,
   ""xrange"":25,
   ""pnresistance"":0,
   ""firesistance"":20,
   ""fzresistance"":20,
   ""stun"":0,
   ""blast"":0,
   ""explosion"":0,
   ""courage"":1,
   ""mcourage"":0,
   ""pcourage"":0,
   ""fear"":0,
   ""items"":[
      {
         ""name"":""stand0""
      },
      {
         ""name"":""blade"",
         ""level"":0
      },
      {
         ""q"":42,
         ""name"":""gemfragment""
      },
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      {
         ""q"":50,
         ""name"":""cscroll0""
      },
      {
         ""q"":26,
         ""name"":""cscroll1""
      },
      null,
      null,
      null,
      null,
      null,
      {
         ""q"":50,
         ""name"":""scroll0""
      },
      {
         ""q"":25,
         ""name"":""scroll1""
      },
      {
         ""q"":16,
         ""name"":""scroll2""
      },
      {
         ""name"":""tracker""
      },
      null,
      null,
      null,
      {
         ""q"":151,
         ""name"":""intscroll""
      },
      {
         ""q"":50,
         ""name"":""dexscroll""
      },
      {
         ""q"":62,
         ""name"":""strscroll""
      },
      {
         ""q"":9000,
         ""name"":""mpot0""
      },
      {
         ""q"":9000,
         ""name"":""hpot0""
      },
      {
         ""q"":9000,
         ""name"":""hpot1""
      },
      {
         ""q"":9000,
         ""name"":""mpot1""
      }
   ],
   ""cc"":1
}";

            var emptyCharacters = Enumerable.Range(0, 100000).Select(_ => new Character()).ToArray();
            var obj = JsonConvert.DeserializeObject<CharacterData>(CHARACTER_DATA);

            var timer = Stopwatch.StartNew();
            var defaultBase = PATHFINDING_CONSTANTS.DEFAULT_BOUNDING_BASE;

            foreach (var emptyChar in emptyCharacters)
            {
                emptyChar.SetBoundingBase(defaultBase);
                ShallowMerge<Character>.Merge(obj!, emptyChar);
                Assert.AreEqual(obj, emptyChar);
                Assert.AreEqual(emptyChar.HalfWidth, defaultBase.HalfWidth);
                Assert.AreEqual(emptyChar.VerticalNorth, defaultBase.VerticalNorth);
                Assert.AreEqual(emptyChar.VerticalNotNorth, defaultBase.VerticalNotNorth);
            }

            timer.Stop();
            var elapsed = timer.ElapsedMilliseconds;
            //this takes like 60ms on my machine. if this goes above 500 on any machine, there must be a problem.
            Assert.IsTrue(elapsed < 500);
        }
    }
}