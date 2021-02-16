using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MotionSetTests
    {
        const int IDLE = 0;
        const int WALK = 1;
        const int RUN = 2;
        const int FALL = 4;
        const int NOT_EXISTING = 5;

        const bool ANIM = true;
        const bool DUMMY = false;

        const bool IN_BATTLE = true;
        const bool OUT_BATTLE = false;
        const bool HAS_WEAPON = true;
        const bool NO_WEAPON = false;


        [Theory]
        [InlineData(IDLE, IN_BATTLE, HAS_WEAPON, 0)]
        [InlineData(IDLE, OUT_BATTLE, NO_WEAPON, 2)]
        [InlineData(IDLE, OUT_BATTLE, HAS_WEAPON, 2)]
        [InlineData(WALK, IN_BATTLE, HAS_WEAPON, 4)]
        [InlineData(WALK, OUT_BATTLE, NO_WEAPON, 6)]
        [InlineData(WALK, OUT_BATTLE, HAS_WEAPON, 7)]
        [InlineData(RUN, IN_BATTLE, HAS_WEAPON, 8)]
        [InlineData(RUN, OUT_BATTLE, NO_WEAPON, 10)]
        [InlineData(RUN, OUT_BATTLE, HAS_WEAPON, 11)]
        [InlineData(FALL, OUT_BATTLE, NO_WEAPON, 18)]
        [InlineData(FALL, OUT_BATTLE, HAS_WEAPON, 18)]
        [InlineData(FALL, IN_BATTLE, HAS_WEAPON, 18)]
        [InlineData(NOT_EXISTING, IN_BATTLE, HAS_WEAPON, -1)]
        [InlineData(NOT_EXISTING, IN_BATTLE, NO_WEAPON, -1)]
        [InlineData(NOT_EXISTING, OUT_BATTLE, HAS_WEAPON, -1)]
        [InlineData(NOT_EXISTING, OUT_BATTLE, NO_WEAPON, -1)]
        public void GetValidMotionSetIndex(
            int animId,
            bool isBattle,
            bool hasWeapon,
            int expectedMsetId)
        {
            var fakeBar = GenerateFakeBar(new bool[]
            {
                // A000
                ANIM,
                DUMMY,
                ANIM,
                DUMMY,
                // A001
                ANIM,
                DUMMY,
                ANIM,
                ANIM,
                // A002
                ANIM,
                DUMMY,
                ANIM,
                ANIM,
                // A003
                DUMMY,
                DUMMY,
                ANIM,
                DUMMY,
                // A004
                DUMMY,
                DUMMY,
                ANIM,
                DUMMY,
                // A005
                DUMMY,
                DUMMY,
                DUMMY,
                DUMMY,
            });

            var actualMsetId = MotionSet.GetMotionSetIndex(
                fakeBar, (MotionSet.MotionName)animId, isBattle, hasWeapon);

            Assert.Equal(expectedMsetId, actualMsetId);
        }

        [Theory]
        [InlineData(IN_BATTLE, HAS_WEAPON, 0)]
        [InlineData(IN_BATTLE, NO_WEAPON, 1)]
        [InlineData(OUT_BATTLE, NO_WEAPON, 2)]
        [InlineData(OUT_BATTLE, HAS_WEAPON, 3)]
        public void GetAbsoluteMotionSetIndex(
            bool isBattle,
            bool hasWeapon,
            int expectedMsetId)
        {
            var fakeBar = GenerateFakeBar(new bool[]
            {
                ANIM,
                ANIM,
                ANIM,
                ANIM,
            });

            var actualMsetId = MotionSet.GetMotionSetIndex(
                fakeBar, MotionSet.MotionName.IDLE, isBattle, hasWeapon);

            Assert.Equal(expectedMsetId, actualMsetId);
        }

        private static List<Bar.Entry> GenerateFakeBar(IEnumerable<bool> hasAnimationArray) =>
            hasAnimationArray.Select((hasAnimation, i) => new Bar.Entry
            {
                Index = 0,
                Name = $"A{i/4:D03}",
                Type = Bar.EntryType.Anb,
                Stream = new MemoryStream(hasAnimation ? new byte[1] : new byte[0])
            }).ToList();
    }
}
