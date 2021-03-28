using NSubstitute;
using OpenKh.Game.Events;
using OpenKh.Game.Field;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2.Ard;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class Kh2EventTests
    {
        [Theory]
        [InlineData(0, -309.26f, -2446.09f, -1473.91f)]
        [InlineData(1, -309.26f, -2446.09f, -1473.91f)]
        [InlineData(2, -307.93f, -2445.79f, -1473.73f)]
        [InlineData(3, -304.03f, -2444.88f, -1473.18f)]
        [InlineData(4, -297.68f, -2443.38f, -1472.28f)]
        [InlineData(5, -289.03f, -2441.28f, -1471.01f)]
        [InlineData(6, -278.22f, -2438.59f, -1469.39f)]
        [InlineData(7, -265.37f, -2435.33f, -1467.42f)]
        [InlineData(8, -250.63f, -2431.49f, -1465.11f)]
        [InlineData(9, -234.12f, -2427.09f, -1462.45f)]
        [InlineData(10, -216.00f, -2422.12f, -1459.45f)]
        [InlineData(11, -196.38f, -2416.59f, -1456.12f)]
        [InlineData(12, -175.41f, -2410.52f, -1452.45f)]
        [InlineData(13, -153.22f, -2403.90f, -1448.46f)]
        [InlineData(14, -129.95f, -2396.74f, -1444.13f)]
        [InlineData(15, -105.73f, -2389.05f, -1439.49f)]
        [InlineData(16, -80.70f, -2380.84f, -1434.52f)]
        [InlineData(17, -55.00f, -2372.10f, -1429.24f)]
        [InlineData(18, -28.76f, -2362.85f, -1423.65f)]
        [InlineData(19, -2.11f, -2353.09f, -1417.75f)]
        [InlineData(20, 24.81f, -2342.83f, -1411.54f)]
        [InlineData(21, 51.86f, -2332.07f, -1405.03f)]
        [InlineData(22, 78.90f, -2320.82f, -1398.22f)]
        [InlineData(23, 105.81f, -2309.08f, -1391.12f)]
        [InlineData(24, 132.44f, -2296.87f, -1383.73f)]
        [InlineData(25, 158.66f, -2284.18f, -1376.04f)]
        [InlineData(26, 184.34f, -2271.03f, -1368.08f)]
        [InlineData(27, 209.34f, -2257.42f, -1359.83f)]
        [InlineData(28, 233.52f, -2243.35f, -1351.30f)]
        [InlineData(29, 256.74f, -2228.83f, -1342.50f)]
        [InlineData(30, 278.88f, -2213.87f, -1333.43f)]
        [InlineData(31, 299.80f, -2198.47f, -1324.08f)]
        [InlineData(32, 319.35f, -2182.64f, -1314.48f)]
        [InlineData(33, 337.42f, -2166.39f, -1304.61f)]
        [InlineData(34, 353.85f, -2149.71f, -1294.49f)]
        [InlineData(35, 368.51f, -2132.63f, -1284.11f)]
        [InlineData(36, 381.27f, -2115.13f, -1273.49f)]
        [InlineData(37, 392.00f, -2097.24f, -1262.61f)]
        [InlineData(38, 400.55f, -2078.94f, -1251.49f)]
        [InlineData(39, 406.79f, -2060.26f, -1240.14f)]
        [InlineData(40, 410.59f, -2041.20f, -1228.54f)]
        [InlineData(41, 411.81f, -2021.76f, -1216.71f)]
        [InlineData(42, 410.06f, -2001.95f, -1204.65f)]
        [InlineData(43, 405.21f, -1981.77f, -1192.37f)]
        [InlineData(44, 397.45f, -1961.23f, -1179.86f)]
        [InlineData(45, 386.95f, -1940.34f, -1167.14f)]
        [InlineData(46, 373.90f, -1919.10f, -1154.19f)]
        [InlineData(47, 358.49f, -1897.52f, -1141.04f)]
        [InlineData(48, 340.89f, -1875.60f, -1127.67f)]
        [InlineData(49, 321.28f, -1853.35f, -1114.10f)]
        [InlineData(50, 299.86f, -1830.78f, -1100.33f)]
        [InlineData(51, 276.80f, -1807.89f, -1086.35f)]
        [InlineData(52, 252.29f, -1784.69f, -1072.18f)]
        [InlineData(53, 226.504f, -1761.18f, -1057.82f)]
        [InlineData(54, 199.63f, -1737.38f, -1043.28f)]
        [InlineData(55, 171.86f, -1713.27f, -1028.54f)]
        [InlineData(56, 143.36f, -1688.89f, -1013.63f)]
        [InlineData(57, 114.32f, -1664.21f, -998.53f)]
        [InlineData(58, 84.93f, -1639.27f, -983.26f)]
        [InlineData(59, 55.36f, -1614.05f, -967.82f)]
        [InlineData(60, 25.80f, -1588.56f, -952.21f)]
        [InlineData(61, -3.57f, -1562.82f, -936.44f)]
        [InlineData(62, -32.56f, -1536.83f, -920.51f)]
        [InlineData(63, -60.99f, -1510.59f, -904.41f)]
        [InlineData(64, -88.68f, -1484.11f, -888.16f)]
        [InlineData(65, -115.45f, -1457.40f, -871.77f)]
        [InlineData(66, -141.12f, -1430.45f, -855.22f)]
        [InlineData(67, -165.49f, -1403.29f, -838.53f)]
        [InlineData(68, -188.40f, -1375.90f, -821.70f)]
        [InlineData(69, -209.64f, -1348.31f, -804.74f)]
        [InlineData(70, -229.05f, -1320.51f, -787.64f)]
        [InlineData(71, -246.44f, -1292.51f, -770.41f)]
        [InlineData(72, -261.63f, -1264.32f, -753.05f)]
        [InlineData(73, -274.43f, -1235.95f, -735.57f)]
        [InlineData(74, -284.66f, -1207.39f, -717.97f)]
        [InlineData(75, -292.14f, -1178.66f, -700.25f)]
        [InlineData(76, -296.69f, -1149.75f, -682.42f)]
        [InlineData(77, -298.12f, -1120.69f, -664.48f)]
        [InlineData(78, -297.33f, -1091.47f, -646.43f)]
        [InlineData(79, -295.19f, -1062.09f, -628.28f)]
        [InlineData(80, -291.75f, -1032.58f, -610.04f)]
        [InlineData(81, -287.08f, -1002.92f, -591.69f)]
        [InlineData(82, -281.28f, -973.13f, -573.25f)]
        [InlineData(83, -274.39f, -943.21f, -554.73f)]
        [InlineData(84, -266.51f, -913.17f, -536.12f)]
        [InlineData(85, -257.69f, -883.02f, -517.42f)]
        [InlineData(86, -248.01f, -852.75f, -498.65f)]
        [InlineData(87, -237.55f, -822.38f, -479.80f)]
        [InlineData(88, -226.38f, -791.92f, -460.88f)]
        [InlineData(89, -214.57f, -761.36f, -441.90f)]
        [InlineData(90, -202.19f, -730.72f, -422.84f)]
        [InlineData(91, -189.31f, -700.00f, -403.73f)]
        [InlineData(92, -176.01f, -669.20f, -384.56f)]
        [InlineData(93, -162.36f, -638.34f, -365.33f)]
        [InlineData(94, -148.43f, -607.41f, -346.05f)]
        [InlineData(95, -134.29f, -576.43f, -326.72f)]
        [InlineData(96, -120.02f, -545.40f, -307.35f)]
        [InlineData(97, -106.03f, -515.05f, -288.40f)]
        [InlineData(98, -106.03f, -515.05f, -288.40f)]
        [InlineData(99, -106.03f, -515.05f, -288.40f)]
        public void InterpolateCameraTest(float time, float eyex, float eyey, float eyez)
        {
            var fakeField = Substitute.For<IField>();
            var eventPlayer = new EventPlayer(fakeField, new List<Event.IEventEntry>
            {
                new Event.SetEndFrame
                {
                    EndFrame = 100
                },
                new Event.SetupEvent(),
                new Event.SetCameraData
                {
                    CameraId = 0,
                    PositionX = CameraDataSingleValue(-309.26492f),
                    PositionY = CameraDataSingleValue(2446.094f),
                    PositionZ = CameraDataSingleValue(1473.9126f),
                    LookAtX = CameraDataSingleValue(0f),
                    LookAtY = CameraDataSingleValue(113.279144f),
                    LookAtZ = CameraDataSingleValue(20.983765f),
                    Roll = CameraDataSingleValue(0f),
                    FieldOfView = CameraDataSingleValue(48.41243f),
                },
                new Event.SetCameraData
                {
                    CameraId = 1,
                    PositionX = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(-309.26492f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(411.81467f, Kh2.Motion.Interpolation.Hermite, 341, -0.6224821f, -0.6224821f),
                        CameraDataValue(-298.11935f, Kh2.Motion.Interpolation.Hermite, 648, -0.8131355f, -0.8131355f),
                        CameraDataValue(-106.03006f, Kh2.Motion.Interpolation.Linear, 819, 860.1719f, 860.1719f),
                        CameraDataValue(-106.03006f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    PositionY = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(2446.094f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(515.0501f, Kh2.Motion.Interpolation.Linear, 819, -1865.6649f, -1865.6649f),
                        CameraDataValue(515.0501f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    PositionZ = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(1473.9126f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(288.39706f, Kh2.Motion.Interpolation.Linear, 819, -1165.8065f, -1165.8065f),
                        CameraDataValue(288.39706f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    LookAtX = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 341, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 648, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 819, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    LookAtY = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(113.279144f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(113.279144f, Kh2.Motion.Interpolation.Hermite, 341, 0f, 0f),
                        CameraDataValue(113.279144f, Kh2.Motion.Interpolation.Hermite, 648, 0f, 0f),
                        CameraDataValue(113.279144f, Kh2.Motion.Interpolation.Linear, 819, 0f, 0f),
                        CameraDataValue(113.279144f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    LookAtZ = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(20.983765f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(20.983765f, Kh2.Motion.Interpolation.Hermite, 341, 0f, 0f),
                        CameraDataValue(20.983765f, Kh2.Motion.Interpolation.Hermite, 648, 0f, 0f),
                        CameraDataValue(20.983765f, Kh2.Motion.Interpolation.Linear, 819, 0f, 0f),
                        CameraDataValue(20.983765f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    Roll = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 341, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Hermite, 648, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 819, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                    FieldOfView = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(48.41243f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(48.41243f, Kh2.Motion.Interpolation.Hermite, 341, 0f, 0f),
                        CameraDataValue(48.41243f, Kh2.Motion.Interpolation.Hermite, 648, 0f, 0f),
                        CameraDataValue(48.41243f, Kh2.Motion.Interpolation.Linear, 819, 0f, 0f),
                        CameraDataValue(48.41243f, Kh2.Motion.Interpolation.Linear, 1024, 0f, 0f),
                    },
                },
                new Event.SeqCamera
                {
                    CameraId = 0,
                    FrameStart = 0,
                    FrameEnd = 100,
                },
                new Event.SeqCamera
                {
                    CameraId = 1,
                    FrameStart = 1,
                    FrameEnd = 101,
                },
            });

            eventPlayer.Initialize();
            eventPlayer.Update(1 / 60.0 * time);

            var setCameraCall = fakeField.ReceivedCalls().FirstOrDefault();
            Assert.NotNull(setCameraCall);

            var actualEye = (System.Numerics.Vector3)setCameraCall.GetArguments()[0];
            var actualCenter = (System.Numerics.Vector3)setCameraCall.GetArguments()[1];
            var actualFov = (float)setCameraCall.GetArguments()[2];
            var actualRoll = (float)setCameraCall.GetArguments()[3];
            Assert.Equal(eyex, actualEye.X, 0);
            Assert.Equal(eyey, -actualEye.Y, 0);
            Assert.Equal(eyez, -actualEye.Z, 0);
            Assert.Equal(0, actualCenter.X, 0);
            Assert.Equal(-113.28f, -actualCenter.Y, 0);
            Assert.Equal(-20.98f, -actualCenter.Z, 0);
            Assert.Equal(0f, actualRoll, 0);
            Assert.Equal(48.41f, actualFov, 0);
        }

        [Theory]
        [InlineData(122, -123.61f, -122.43f)]
        [InlineData(140, -123.68f, -122.80f)]
        [InlineData(180, -126.95f, -125.76f)]
        [InlineData(200, -129.38f, -128.20f)]
        [InlineData(438, -176.39, -175.20)]
        [InlineData(444, -177.35, -176.17)]
        public void InterpolateCameraSecondTest(float time, float eyey, float centery)
        {
            var fakeField = Substitute.For<IField>();
            var eventPlayer = new EventPlayer(fakeField, new List<Event.IEventEntry>
            {
                new Event.SetEndFrame
                {
                    EndFrame = 300
                },
                new Event.SetupEvent(),
                new Event.SetCameraData
                {
                    CameraId = 0,
                    PositionX = CameraDataSingleValue(40.186035f),
                    PositionY = CameraDataSingleValue(271.22748f),
                    PositionZ = CameraDataSingleValue(527.7301f),
                    LookAtX = CameraDataSingleValue(-79.38527f),
                    LookAtY = CameraDataSingleValue(90.874115f),
                    LookAtZ = CameraDataSingleValue(-78.414696f),
                    Roll = CameraDataSingleValue(0f),
                    FieldOfView = CameraDataSingleValue(41.092915f),
                },
                new Event.SetCameraData
                {
                    CameraId = 1,
                    PositionX = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(-317.25266f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(-317.25266f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(-317.25266f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    PositionY = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(123.61203f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(185.76723f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(185.76723f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    PositionZ = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(-79.360405f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(-79.360405f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(-79.360405f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    LookAtX = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(-357.23502f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(-357.23502f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(-357.23502f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    LookAtY = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(122.42929f, Kh2.Motion.Interpolation.Hermite, 0, 0, 0),
                        CameraDataValue(184.58449f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(184.58449f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    LookAtZ = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(-217.77927f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(-217.77927f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(-217.77927f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    Roll = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(0f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                    FieldOfView = new List<Event.SetCameraData.CameraKeys>
                    {
                        CameraDataValue(42.026005f, Kh2.Motion.Interpolation.Linear, 0, 0f, 0f),
                        CameraDataValue(42.026005f, Kh2.Motion.Interpolation.Linear, 3584, 0f, 0f),
                        CameraDataValue(42.026005f, Kh2.Motion.Interpolation.Linear, 3754, 0f, 0f),
                    },
                },
                new Event.SeqCamera
                {
                    CameraId = 0,
                    FrameStart = 15,
                    FrameEnd = 75,
                },
                new Event.SeqCamera
                {
                    CameraId = 1,
                    FrameStart = 76,
                    FrameEnd = 286,
                },
            });

            eventPlayer.Initialize();
            eventPlayer.Update(1 / 60.0 * (time + 30f));

            var setCameraCall = fakeField.ReceivedCalls().FirstOrDefault();
            Assert.NotNull(setCameraCall);

            var actualEye = (System.Numerics.Vector3)setCameraCall.GetArguments()[0];
            var actualCenter = (System.Numerics.Vector3)setCameraCall.GetArguments()[1];
            var actualFov = (float)setCameraCall.GetArguments()[2];
            var actualRoll = (float)setCameraCall.GetArguments()[3];
            Assert.Equal(-317.25266f, actualEye.X, 0);
            Assert.Equal(eyey, -actualEye.Y, 0);
            Assert.Equal(79.360405f, -actualEye.Z, 0);
            Assert.Equal(-357.23502f, actualCenter.X, 0);
            Assert.Equal(centery, -actualCenter.Y, 0);
            Assert.Equal(217.77927f, -actualCenter.Z, 0);
            Assert.Equal(0f, actualRoll, 0);
            Assert.Equal(42.026005f, actualFov, 0);
        }

        [Theory]
        [InlineData(30, false)]
        [InlineData(60, true)]
        [InlineData(120, true)]
        [InlineData(180, false)]
        public void ActorSceneVisibility(float time, bool expectedActorVisibility)
        {
            const int ActorId = 0;
            const int FrameActorAppear = 50;
            const string MotionPath = "anm_ex/motion";

            var fakeField = Substitute.For<IField>();
            var eventPlayer = new EventPlayer(fakeField, new List<Event.IEventEntry>
            {
                new Event.SetEndFrame
                {
                    EndFrame = 1000
                },
                new Event.ReadAssets
                {
                    FrameStart = -32767,
                    FrameEnd = 0,
                    Unk06 = 0,
                    Set = new List<Event.IEventEntry>
                    {
                        new Event.ReadActor
                        {
                            ObjectId = 123,
                            ActorId = ActorId,
                            Name = "main_actor"
                        },
                        new Event.ReadMotion
                        {
                            ObjectId = 123,
                            ActorId = ActorId,
                            Name = MotionPath,
                            UnknownIndex = 1,
                        },
                    }
                },
                new Event.SetActor
                {
                    ObjectEntry = 123,
                    ActorId = ActorId,
                    Name = "main_actor"
                },
                new Event.SeqPlayAnimation
                {
                    FrameStart = FrameActorAppear,
                    FrameEnd = 100,
                    ActorId = ActorId,
                    Path = MotionPath
                },
                new Event.SeqActorLeave
                {
                    Frame = 150,
                    ActorId = ActorId
                }
            });

            bool? lastVisibility = null;

            fakeField.SetActorVisibility(Arg.Is(ActorId), Arg.Do<bool>(x => lastVisibility = x));
            eventPlayer.Initialize();
            for (var i = 0; i < time; i++)
                eventPlayer.Update(1f / 30.0f);

            Assert.Equal(expectedActorVisibility, lastVisibility);
        }

        private static Event.SetCameraData.CameraKeys CameraDataValue(
            float value, Kh2.Motion.Interpolation interpolation, int keyFrame, float left, float right) =>
            new Event.SetCameraData.CameraKeys
            {
                Interpolation = interpolation,
                KeyFrame = keyFrame,
                Value = value,
                TangentEaseIn = left,
                TangentEaseOut = right
            };

        private static List<Event.SetCameraData.CameraKeys> CameraDataSingleValue(float value) =>
            new List<Event.SetCameraData.CameraKeys>()
            {
                CameraDataValue(value, Kh2.Motion.Interpolation.Hermite, 0, 0, 0)
            };
    }
}
