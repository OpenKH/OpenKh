using McMaster.Extensions.CommandLineUtils;
using OpenKh.Bbs;
//using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenKh.Command.KHAnimationConverter
{
    public class KHFormat
    {
        public Pam pam;
        public List<AnimationBinary> anbs;
        // TODO: KH1 MotionSet

        public static bool ExportFile(KHFormat file, string format, string filePath)
        {
            bool status = true;
            format = format.ToLower();
            if (format == "anbs")
            {
                if (file.anbs != null)
                {
                    if (!Directory.Exists(filePath))
                    {
                        if (File.Exists(filePath))
                        {
                            throw new Exception("Error: Exporing as ANBs requires saves to a directory, but the specified output file path is an existing file");
                        }
                        Directory.CreateDirectory(filePath);
                    }

                    for (int i = 0; i < file.anbs.Count; i++)
                    {
                        string anbFileName = $"anim-{i:0000}.anb";
                        string anbFilePath = Path.Join(filePath, anbFileName);
                        using (FileStream fs = new FileStream(anbFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (BinaryWriter bw = new BinaryWriter(fs))
                            {

                                AnimationBinary anb = file.anbs[i];
                                Bar.Entry MotionEntry = new Bar.Entry();
                                MotionEntry.Type = Bar.EntryType.Motion;
                                MotionEntry.Index = anb.MotionIndex;
                                MotionEntry.Name = anb.MotionName;
                                MotionEntry.Stream = anb.MotionFile.toStream();

                                Bar.Entry TriggerEntry = new Bar.Entry();
                                TriggerEntry.Type = Bar.EntryType.MotionTriggers;
                                TriggerEntry.Index = anb.TriggerIndex;
                                TriggerEntry.Name = anb.TriggerName;
                                TriggerEntry.Stream = anb.MotionTriggerFile != null ? anb.MotionTriggerFile.toStream() : new MemoryStream();

                                Bar.Write(bw.BaseStream, new List<Bar.Entry> { MotionEntry, TriggerEntry });
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: The file to export is not an ANB file(s)");
                    status = false;
                }
            }
            // TODO - Support writing other file types
            /*
            else if (format == "pam")
            {
                status = false;
            }
            //*/
            else
            {
                Console.WriteLine($"Error: Failed to write file to disc - Unsupported Format: {format}");
                status = false;
            }
            return status;
        }

        public static List<AnimationBinary> PAMtoANBs(Pam pam, float anbFramerate = 60f)
        {
            List<AnimationBinary> anbs = new List<AnimationBinary>();
            // Maps PAM Channels to ANB Channels
            List<short> pamChannelsToAnbChannels = new List<short>(){6, 7, 8, 3, 4, 5, 0, 1, 2};
            float pamToanbTranslationScaleFactor = 100f;
            for (int i = 0; i < pam.header.AnimationCount; i++)
            {
                Pam.AnimationInfo animInfo = pam.animList[i];
                Pam.AnimationEntry animEntry = animInfo.AnimEntry;

                if (animEntry.AnimationOffset == 0)
                {
                    continue;
                }

                // Create the subfiles that make up an anb
                Motion.InterpolatedMotion im = Motion.InterpolatedMotion.CreateEmpty();
                MotionTrigger mt = new MotionTrigger()
                {
                    FrameTriggerList = new List<MotionTrigger.FrameTrigger>(),
                    RangeTriggerList = new List<MotionTrigger.RangeTrigger>()
                };

                Pam.AnimationHeader animHeader = animInfo.AnimHeader;
                List<Pam.BoneChannel> boneChannels = animInfo.BoneChannels;

                int keysCount = 0;

                List<short> keysList = new List<short>();
                List<float> valuesList = new List<float>();

                int boneCount = animHeader.BoneCount;
                float pamFramerate = animHeader.Framerate;
                float framerateRatio = anbFramerate / pamFramerate;
                im.InterpolatedMotionHeader.BoneCount = (short)boneCount;
                im.InterpolatedMotionHeader.FrameCount = (int)(animHeader.FrameCount * framerateRatio);
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinX = -100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinY = -100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinZ = -100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinW = 1;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxX = 100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxY = 100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxZ = 100;
                im.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxW = 1;
                im.InterpolatedMotionHeader.FrameData.FrameStart = 0f;
                im.InterpolatedMotionHeader.FrameData.FrameEnd = animHeader.FrameCount * framerateRatio;
                im.InterpolatedMotionHeader.FrameData.FramesPerSecond = anbFramerate;
                im.InterpolatedMotionHeader.FrameData.FrameReturn = animHeader.ReturnFrame * framerateRatio;
                for (int j = 0; j < boneCount; j++)
                {
                    Pam.BoneChannel boneChannel = boneChannels[j];
                    List<Pam.AnimationData> channels = new List<Pam.AnimationData> {
                        boneChannel.TranslationX,
                        boneChannel.TranslationY,
                        boneChannel.TranslationZ,
                        boneChannel.RotationX,
                        boneChannel.RotationY,
                        boneChannel.RotationZ,
                        boneChannel.ScaleX,
                        boneChannel.ScaleY,
                        boneChannel.ScaleZ
                    };

                    Motion.Joint joint = new Motion.Joint()
                    {
                        JointId = (short)j
                    };
                    im.Joints.Add(joint);

                    List<float> prevBoneChannelValues = new List<float>() { float.NaN, float.NaN, float.NaN };

                    for (int k = 0; k < channels.Count; k++)
                    {
                        Pam.AnimationData animData = channels[k];

                        if (animData == null)
                        {
                            continue;
                        }

                        short anbChannel = pamChannelsToAnbChannels[k];
                        int keyframesCount = animData.Header.KeyframeCount_8bits + animData.Header.KeyframeCount_16bits;

                        // Check if this is an fcurve (vs an initial pose)
                        if (keyframesCount > 1)
                        {
                            Motion.FCurve fcurve = new Motion.FCurve()
                            {
                                JointId = (short)j,
                                Channel = (byte)anbChannel,
                                Pre = 0,
                                Post = 0,
                                KeyCount = (byte)keyframesCount, // TODO: Figure out supporting keyframe counts > 255
                                KeyStartId = (short)keysCount
                            };
                            im.FCurvesForward.Add(fcurve);
                            keysCount += keyframesCount;
                        }

                        for (int l = 0; l < keyframesCount; l++)
                        {
                            Pam.KeyframeEntry entry = animData.Keyframes?[l];
                            short pamKeyframeID = entry != null ? (short)(entry.FrameID_8bits + entry.FrameID_16bits) : (short)0;
                            float pamChannelValue = EvaluatePAMChannel(animData.Keyframes, pamKeyframeID / pamFramerate, keyframesCount, animData.Header.MaxValue, animData.Header.MinValue, pamFramerate);
                            float anbChannelValue = pamChannelValue;
                            if (3 <= anbChannel && anbChannel <= 5)
                            {
                                // Check if there was a previous value
                                if (l > 0)
                                {
                                    float prevChannelValue = prevBoneChannelValues[anbChannel - 3];
                                    anbChannelValue = UnwrapAngle(prevChannelValue, anbChannelValue);
                                }
                                prevBoneChannelValues[anbChannel - 3] = anbChannelValue;
                            }
                            else if (6 <= anbChannel && anbChannel <= 8)
                            {
                                // Adjust Units
                                anbChannelValue *= pamToanbTranslationScaleFactor;
                            }
                            // Truncate to save space if there are similar values
                            anbChannelValue = (float)Math.Round(anbChannelValue, 3);

                            // Check if this is an initial pose
                            if (keyframesCount == 1)
                            {
                                Motion.InitialPose initialPose = new Motion.InitialPose()
                                {
                                    BoneId = (short)j,
                                    Channel = anbChannel,
                                    Value = anbChannelValue
                                };
                                im.InitialPoses.Add(initialPose);
                            }
                            else // This is an fcurve
                            {
                                short anbKeyframeID = (short)(pamKeyframeID * framerateRatio);
                                keysList.Add(anbKeyframeID);
                                valuesList.Add(anbChannelValue);
                            }
                        }
                    }
                }

                List<short> keysListUnique = keysList.Distinct().ToList();
                List<float> valuesListUnique = valuesList.Distinct().ToList();
                keysListUnique.Sort();
                valuesListUnique.Sort();

                int fcurveInd = 0;
                int keyframeInd = 0;
                for (int j = 0; j < boneCount; j++)
                {
                    Pam.BoneChannel boneChannel = boneChannels[j];
                    List<Pam.AnimationData> channels = new List<Pam.AnimationData> {
                        boneChannel.TranslationX,
                        boneChannel.TranslationY,
                        boneChannel.TranslationZ,
                        boneChannel.RotationX,
                        boneChannel.RotationY,
                        boneChannel.RotationZ,
                        boneChannel.ScaleX,
                        boneChannel.ScaleY,
                        boneChannel.ScaleZ
                    };
                    
                    for (int k = 0; k < channels.Count; k++)
                    {
                        Pam.AnimationData animData = channels[k];

                        if (animData == null)
                        {
                            continue;
                        }

                        short anbChannel = pamChannelsToAnbChannels[k];
                        int keyframesCount = animData.Header.KeyframeCount_8bits + animData.Header.KeyframeCount_16bits;

                        // Check if it is a fcurve
                        if (keyframesCount > 1)
                        {
                            for (int l = 0; l < keyframesCount; l++)
                            {
                                short anbKeyframeID = keysList[keyframeInd];
                                float anbChannelValue = valuesList[keyframeInd];

                                short keyframeIndex = (short)((keysListUnique.IndexOf(anbKeyframeID) * 4) | 1);
                                short valueIndex = (short)valuesListUnique.IndexOf(anbChannelValue);

                                Motion.Key key = new Motion.Key()
                                {
                                    Type_Time = keyframeIndex,
                                    ValueId = valueIndex,
                                    LeftTangentId = 0,
                                    RightTangentId = 0
                                };

                                if (j == 0 && l == keyframesCount - 1)
                                {
                                    // Update Root Position
                                    switch (anbChannel)
                                    {
                                        case 3:
                                        case 4:
                                        case 5:
                                            im.RootPosition.FCurveId[anbChannel] = fcurveInd;
                                            im.RootPosition.NotUnit = 1;
                                            break;
                                        case 6:
                                            im.RootPosition.TranslateX = anbChannelValue;
                                            im.RootPosition.FCurveId[anbChannel] = fcurveInd;
                                            im.RootPosition.NotUnit = 1;
                                            break;
                                        case 7:
                                            im.RootPosition.TranslateY = anbChannelValue;
                                            im.RootPosition.FCurveId[anbChannel] = fcurveInd;
                                            im.RootPosition.NotUnit = 1;
                                            break;
                                        case 8:
                                            im.RootPosition.TranslateZ = anbChannelValue;
                                            im.RootPosition.FCurveId[anbChannel] = fcurveInd;
                                            im.RootPosition.NotUnit = 1;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                im.FCurveKeys.Add(key);

                                keyframeInd++;
                            }
                            fcurveInd++;
                        }
                    }
                }

                for (int k = 0; k < keysListUnique.Count; k++)
                {
                    im.KeyTimes.Add(keysListUnique[k]);
                }

                for (int k = 0; k < valuesListUnique.Count; k++)
                {
                    im.KeyValues.Add(valuesListUnique[k]);
                }

                // Create the motion triggers
                MotionTrigger.RangeTrigger rt = new MotionTrigger.RangeTrigger()
                {
                    StartFrame = 0,
                    EndFrame = -1,
                    Trigger = 0,
                    ParamSize = 0
                };
                mt.RangeTriggerList.Add(rt);
                MotionTrigger.FrameTrigger ft = new MotionTrigger.FrameTrigger()
                {
                    Frame = 56,
                    Trigger = 19,
                    ParamSize = 1,
                    Param1 = 9
                };
                mt.FrameTriggerList.Add(ft);
                ft = new MotionTrigger.FrameTrigger()
                { 
                    Frame = 112,
                    Trigger = 19,
                    ParamSize = 1,
                    Param1 = 8
                };
                mt.FrameTriggerList.Add(ft);

                // Create the anb from the interpolated motion and motion trigger
                AnimationBinary anb = new AnimationBinary(im, mt, 0, "OpKH", 0, "OpKH");
                anbs.Add(anb);
            }

            return anbs;
        }

        public static float EvaluatePAMChannel(List<Pam.KeyframeEntry> keys, double time, int frameCnt, float max, float min, double framerate=30f)
        {
            if (keys is null || keys.Count == 0)
            {
                if (frameCnt == 1)
                {
                    return min;
                }
                else
                {
                    return float.NaN;
                }
            }

            double frametick = 1.0 / framerate;

            Pam.KeyframeEntry entry = keys.First();
            double keyframe = entry.FrameID_8bits + entry.FrameID_16bits;
            double keyframeTime = Math.Round(frametick * keyframe, 3);

            time = Math.Round(time, 3);

            ushort value = 0;
            // Before the first key.
            if (time <= keyframeTime)
            {
                value = keys.First().Value;
            }
            else
            {

                entry = keys.Last();
                keyframe = entry.FrameID_8bits + entry.FrameID_16bits;
                keyframeTime = Math.Round(frametick * keyframe, 3);

                // After the last key.
                if (time >= keyframeTime)
                {
                    value = keys.Last().Value;
                }
                else
                {
                    // Find the two keys between which 'time' falls.
                    if (keys.Count > 1)
                    {
                        for (int i = 0; i < keys.Count - 1; i++)
                        {
                            Pam.KeyframeEntry first = keys[i], second = keys[i + 1];
                            double keyframeTime0 = first.FrameID_8bits + first.FrameID_16bits;
                            double keyframeTime1 = second.FrameID_8bits + second.FrameID_16bits;
                            double t0 = Math.Round(keyframeTime0 * frametick, 3);
                            double t1 = Math.Round(keyframeTime1 * frametick, 3);
                            if (time >= t0 && time <= t1)
                            {
                                float v0 = keys[i].Value;
                                float v1 = keys[i + 1].Value;
                                float factor = (float)((time - t0) / (t1 - t0));
                                value = (ushort)(v0 + factor * (v1 - v0));
                                break;
                            }
                        }
                    }
                    else
                    {
                        value = keys.Last().Value; // fallback
                    }
                }
            }

            // Dequantize the value using min and max
            float range = max - min;
            return min + range * (value / (float)UInt16.MaxValue);
        }

        public static float UnwrapAngle(float prevAngle, float currentAngle)
        {
            float diff = currentAngle - prevAngle;
            float twoPi = (float)(Math.PI * 2);
            while (diff < -Math.PI)
            {
                currentAngle += twoPi;
                diff = currentAngle - prevAngle;
            }
            while(diff > Math.PI)
            {
                currentAngle -= twoPi;
                diff = currentAngle - prevAngle;
            }
            return currentAngle;
        }
    }

    [Command("OpenKh.Command.PAMtoANBConverter")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "Input File", "The PAM animation set to use for the conversion.")]
        public string Input_FileName { get; }

        [Required]
        [Argument(1, "Output File", "The path to save the converted animations to.")]
        public string Output_FileName { get; }

        [Option("-r", "The target framerate for the converted animation", CommandOptionType.SingleValue)]
        public float Target_Framerate { get; } = 60;

        private void OnExecute()
        {
            // TODO: These should be parameters when multiple formats are supported
            string input_format_id = "pam";
            string output_format_id = "anbs";
            Convert(Input_FileName, input_format_id, Output_FileName, output_format_id, Target_Framerate);
        }

        private static void Convert(string Input_Path, string Input_Format_ID, string Output_Path, string Output_Format_ID, float targetFramerate)
        {
            Input_Format_ID = Input_Format_ID.ToLower();
            Output_Format_ID = Output_Format_ID.ToLower();

            Stream inputStream = File.OpenRead(Input_Path);
            KHFormat input = new KHFormat();
            KHFormat output = new KHFormat();

            Console.WriteLine($"Reading from {Input_Path}");
            // TODO - Support other input formats
            if (Input_Format_ID == "pam")
                input.pam = Pam.Read(inputStream);
            else
                Console.WriteLine($"Error: Unsupported Input Format: {Input_Format_ID}");

            Console.WriteLine($"Converting {Input_Format_ID} to {Output_Format_ID}");
            // TODO - Support other output formats
            if (Output_Format_ID == "anbs")
            {
                if (Input_Format_ID == "pam")
                    output.anbs = KHFormat.PAMtoANBs(input.pam, targetFramerate);
                else
                    Console.WriteLine($"Error: Unsupported Conversion from {Input_Format_ID} to {Output_Format_ID}");
            }
            else
            {
                Console.WriteLine($"Error: Unsupported Output Format: {Output_Format_ID}");
            }

            inputStream.Close();

            Console.WriteLine("Exporting {0}", Output_Path);
            bool status = KHFormat.ExportFile(output, Output_Format_ID, Output_Path);
            if (status)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Failed!");
            }
        }
    }
}
