using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class IMExExcelUsecase
    {
        private readonly LoadedModel _loadedModel;

        public IMExExcelUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public void ExportTo(string saveTo, Action<IMExProgress>? onProgress = null)
        {
            var root = _loadedModel.MotionData ?? throw new Exception("motion data not yet loaded");
            var exchanger = new DataExchange();
            DoDataExchange(root, exchanger);
            using var stream = File.Create(saveTo);
            exchanger.ExportTo(stream, onProgress);
        }

        public ImportResult ImportFrom(string loadFrom, Action<IMExProgress>? onProgress = null)
        {
            var root = _loadedModel.MotionData ?? throw new Exception("motion data not yet loaded");
            var exchanger = new DataExchange();
            DoDataExchange(root, exchanger);
            using var stream = File.OpenRead(loadFrom);
            var result = new ImportResult();
            exchanger.ImportFrom(stream, onProgress, result.Errors.Add, result.Results.Add);
            return result;
        }

        private void DoDataExchange(Motion.InterpolatedMotion root, DataExchange exchanger)
        {
            exchanger.Sheet(
                "ConstraintActivations",
                root.ConstraintActivations,
                sheetDef => sheetDef
                    .Column(nameof(Motion.ConstraintActivation.Time), (row, cell) => cell.SetCellValue(row.Time), (row, cell) => row.Time = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.ConstraintActivation.Active), (row, cell) => cell.SetCellValue(row.Active), (row, cell) => row.Active = (int)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "Constraints",
                root.Constraints,
                sheetDef => sheetDef
                    .Column(nameof(Motion.Constraint.Type), (row, cell) => cell.SetCellValue(row.Type), (row, cell) => row.Type = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.TemporaryActiveFlag), (row, cell) => cell.SetCellValue(row.TemporaryActiveFlag), (row, cell) => row.TemporaryActiveFlag = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.ConstrainedJointId), (row, cell) => cell.SetCellValue(row.ConstrainedJointId), (row, cell) => row.ConstrainedJointId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.SourceJointId), (row, cell) => cell.SetCellValue(row.SourceJointId), (row, cell) => row.SourceJointId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.LimiterId), (row, cell) => cell.SetCellValue(row.LimiterId), (row, cell) => row.LimiterId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.ActivationCount), (row, cell) => cell.SetCellValue(row.ActivationCount), (row, cell) => row.ActivationCount = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Constraint.ActivationStartId), (row, cell) => cell.SetCellValue(row.ActivationStartId), (row, cell) => row.ActivationStartId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "ExpressionNodes",
                root.ExpressionNodes,
                sheetDef => sheetDef
                    .Column(nameof(Motion.ExpressionNode.Element), (row, cell) => cell.SetCellValue(row.Element), (row, cell) => row.Element = (int)cell.NumericCellValue)
                    .Column(nameof(Motion.ExpressionNode.Type), (row, cell) => cell.SetCellValue(row.Type), (row, cell) => row.Type = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.ExpressionNode.IsGlobal), (row, cell) => cell.SetCellValue(row.IsGlobal), (row, cell) => row.IsGlobal = cell.BooleanCellValue)

                    .Column(nameof(Motion.ExpressionNode.Value), (row, cell) => cell.SetCellValue(row.Value), (row, cell) => row.Value = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.ExpressionNode.CAR), (row, cell) => cell.SetCellValue(row.CAR), (row, cell) => row.CAR = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.ExpressionNode.CDR), (row, cell) => cell.SetCellValue(row.CDR), (row, cell) => row.CDR = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "Expressions",
                root.Expressions,
                sheetDef => sheetDef
                    .Column(nameof(Motion.Expression.TargetId), (row, cell) => cell.SetCellValue(row.TargetId), (row, cell) => row.TargetId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Expression.TargetChannel), (row, cell) => cell.SetCellValue(row.TargetChannel), (row, cell) => row.TargetChannel = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Expression.Reserved), (row, cell) => cell.SetCellValue(row.Reserved), (row, cell) => row.Reserved = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Expression.NodeId), (row, cell) => cell.SetCellValue(row.NodeId), (row, cell) => row.NodeId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "ExternalEffectors",
                root.ExternalEffectors,
                sheetDef => sheetDef
                    .Column(nameof(Motion.ExternalEffector.JointId), (row, cell) => cell.SetCellValue(row.JointId), (row, cell) => row.JointId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "FCurveKeys",
                root.FCurveKeys,
                sheetDef => sheetDef
                    .Column(nameof(Motion.Key.Type), (row, cell) => cell.SetCellValue((double)row.Type), (row, cell) => row.Type = (Motion.Interpolation)cell.NumericCellValue)
                    .Column(nameof(Motion.Key.Time), (row, cell) => cell.SetCellValue(row.Time), (row, cell) => row.Time = (short)cell.NumericCellValue)

                    .Column(nameof(Motion.Key.ValueId), (row, cell) => cell.SetCellValue(row.ValueId), (row, cell) => row.ValueId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Key.LeftTangentId), (row, cell) => cell.SetCellValue(row.LeftTangentId), (row, cell) => row.LeftTangentId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.Key.RightTangentId), (row, cell) => cell.SetCellValue(row.RightTangentId), (row, cell) => row.RightTangentId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "FCurvesForward",
                root.FCurvesForward,
                sheetDef => sheetDef
                    .Column(nameof(Motion.FCurve.JointId), (row, cell) => cell.SetCellValue(row.JointId), (row, cell) => row.JointId = (short)cell.NumericCellValue)

                    .Column(nameof(Motion.FCurve.ChannelValue), (row, cell) => cell.SetCellValue((int)row.ChannelValue), (row, cell) => row.ChannelValue = (Motion.Channel)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.Pre), (row, cell) => cell.SetCellValue((int)row.Pre), (row, cell) => row.Pre = (Motion.CycleType)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.Post), (row, cell) => cell.SetCellValue((int)row.Post), (row, cell) => row.Post = (Motion.CycleType)cell.NumericCellValue)

                    .Column(nameof(Motion.FCurve.KeyCount), (row, cell) => cell.SetCellValue(row.KeyCount), (row, cell) => row.KeyCount = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.KeyStartId), (row, cell) => cell.SetCellValue(row.KeyStartId), (row, cell) => row.KeyStartId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "FCurvesInverse",
                root.FCurvesInverse,
                sheetDef => sheetDef
                    .Column(nameof(Motion.FCurve.JointId), (row, cell) => cell.SetCellValue(row.JointId), (row, cell) => row.JointId = (short)cell.NumericCellValue)

                    .Column(nameof(Motion.FCurve.ChannelValue), (row, cell) => cell.SetCellValue((int)row.ChannelValue), (row, cell) => row.ChannelValue = (Motion.Channel)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.Pre), (row, cell) => cell.SetCellValue((int)row.Pre), (row, cell) => row.Pre = (Motion.CycleType)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.Post), (row, cell) => cell.SetCellValue((int)row.Post), (row, cell) => row.Post = (Motion.CycleType)cell.NumericCellValue)

                    .Column(nameof(Motion.FCurve.KeyCount), (row, cell) => cell.SetCellValue(row.KeyCount), (row, cell) => row.KeyCount = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.FCurve.KeyStartId), (row, cell) => cell.SetCellValue(row.KeyStartId), (row, cell) => row.KeyStartId = (short)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "IKHelpers",
                root.IKHelpers,
                sheetDef => sheetDef
                    .Column(nameof(Motion.IKHelper.Index), (row, cell) => cell.SetCellValue(row.Index), (row, cell) => row.Index = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.SiblingId), (row, cell) => cell.SetCellValue(row.SiblingId), (row, cell) => row.SiblingId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.ParentId), (row, cell) => cell.SetCellValue(row.ParentId), (row, cell) => row.ParentId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.ChildId), (row, cell) => cell.SetCellValue(row.ChildId), (row, cell) => row.ChildId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.Reserved), (row, cell) => cell.SetCellValue(row.Reserved), (row, cell) => row.Reserved = (int)cell.NumericCellValue)

                    .Column(nameof(Motion.IKHelper.Unknown), (row, cell) => cell.SetCellValue(row.Unknown), (row, cell) => row.Unknown = (int)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.Terminate), (row, cell) => cell.SetCellValue(row.Terminate), (row, cell) => row.Terminate = cell.BooleanCellValue)
                    .Column(nameof(Motion.IKHelper.Below), (row, cell) => cell.SetCellValue(row.Below), (row, cell) => row.Below = cell.BooleanCellValue)
                    .Column(nameof(Motion.IKHelper.EnableBias), (row, cell) => cell.SetCellValue(row.EnableBias), (row, cell) => row.EnableBias = cell.BooleanCellValue)

                    .Column(nameof(Motion.IKHelper.ScaleX), (row, cell) => cell.SetCellValue(row.ScaleX), (row, cell) => row.ScaleX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.ScaleY), (row, cell) => cell.SetCellValue(row.ScaleY), (row, cell) => row.ScaleY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.ScaleZ), (row, cell) => cell.SetCellValue(row.ScaleZ), (row, cell) => row.ScaleZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.ScaleW), (row, cell) => cell.SetCellValue(row.ScaleW), (row, cell) => row.ScaleW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.RotateX), (row, cell) => cell.SetCellValue(row.RotateX), (row, cell) => row.RotateX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.RotateY), (row, cell) => cell.SetCellValue(row.RotateY), (row, cell) => row.RotateY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.RotateZ), (row, cell) => cell.SetCellValue(row.RotateZ), (row, cell) => row.RotateZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.RotateW), (row, cell) => cell.SetCellValue(row.RotateW), (row, cell) => row.RotateW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.TranslateX), (row, cell) => cell.SetCellValue(row.TranslateX), (row, cell) => row.TranslateX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.TranslateY), (row, cell) => cell.SetCellValue(row.TranslateY), (row, cell) => row.TranslateY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.TranslateZ), (row, cell) => cell.SetCellValue(row.TranslateZ), (row, cell) => row.TranslateZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.IKHelper.TranslateW), (row, cell) => cell.SetCellValue(row.TranslateW), (row, cell) => row.TranslateW = (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "InitialPoses",
                root.InitialPoses,
                sheetDef => sheetDef
                    .Column(nameof(Motion.InitialPose.BoneId), (row, cell) => cell.SetCellValue(row.BoneId), (row, cell) => row.BoneId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InitialPose.Channel), (row, cell) => cell.SetCellValue(row.Channel), (row, cell) => row.Channel = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InitialPose.Value), (row, cell) => cell.SetCellValue(row.Value), (row, cell) => row.Value = (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "Joints",
                root.Joints,
                sheetDef => sheetDef
                    .Column(nameof(Motion.Joint.JointId), (row, cell) => cell.SetCellValue(row.JointId), (row, cell) => row.JointId = (short)cell.NumericCellValue)

                    .Column(nameof(Motion.Joint.IK), (row, cell) => cell.SetCellValue(row.IK), (row, cell) => row.IK = (byte)cell.NumericCellValue)
                    .Column(nameof(Motion.Joint.ExtEffector), (row, cell) => cell.SetCellValue(row.ExtEffector), (row, cell) => row.ExtEffector = cell.BooleanCellValue)
                    .Column(nameof(Motion.Joint.CalcMatrix2Rot), (row, cell) => cell.SetCellValue(row.CalcMatrix2Rot), (row, cell) => row.CalcMatrix2Rot = cell.BooleanCellValue)
                    .Column(nameof(Motion.Joint.Calculated), (row, cell) => cell.SetCellValue(row.Calculated), (row, cell) => row.Calculated = cell.BooleanCellValue)
                    .Column(nameof(Motion.Joint.Fixed), (row, cell) => cell.SetCellValue(row.Fixed), (row, cell) => row.Fixed = cell.BooleanCellValue)
                    .Column(nameof(Motion.Joint.Rotation), (row, cell) => cell.SetCellValue(row.Rotation), (row, cell) => row.Rotation = cell.BooleanCellValue)
                    .Column(nameof(Motion.Joint.Trans), (row, cell) => cell.SetCellValue(row.Trans), (row, cell) => row.Trans = cell.BooleanCellValue)

                    .Column(nameof(Motion.Joint.Reserved), (row, cell) => cell.SetCellValue(row.Reserved), (row, cell) => row.Reserved = (byte)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "KeyTangents",
                root.KeyTangents,
                sheetDef => sheetDef
                    .Single("Value", (row, cell) => cell.SetCellValue(row), (cell) => (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "KeyTimes",
                root.KeyTimes,
                sheetDef => sheetDef
                    .Single("Value", (row, cell) => cell.SetCellValue(row), (cell) => (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "KeyValues",
                root.KeyValues,
                sheetDef => sheetDef
                    .Single("Value", (row, cell) => cell.SetCellValue(row), (cell) => (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "Limiters",
                root.Limiters,
                sheetDef => sheetDef
                    .Column(nameof(Motion.Limiter.Type), (row, cell) => cell.SetCellValue((double)row.Type), (row, cell) => row.Type = (Motion.LimiterType)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.HasXMin), (row, cell) => cell.SetCellValue(row.HasXMin), (row, cell) => row.HasXMin = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.HasXMax), (row, cell) => cell.SetCellValue(row.HasXMax), (row, cell) => row.HasXMax = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.HasYMin), (row, cell) => cell.SetCellValue(row.HasYMin), (row, cell) => row.HasYMin = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.HasYMax), (row, cell) => cell.SetCellValue(row.HasYMax), (row, cell) => row.HasYMax = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.HasZMin), (row, cell) => cell.SetCellValue(row.HasZMin), (row, cell) => row.HasZMin = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.HasZMax), (row, cell) => cell.SetCellValue(row.HasZMax), (row, cell) => row.HasZMax = cell.BooleanCellValue)
                    .Column(nameof(Motion.Limiter.Global), (row, cell) => cell.SetCellValue(row.Global), (row, cell) => row.Global = cell.BooleanCellValue)

                    .Column(nameof(Motion.Limiter.DampingWidth), (row, cell) => cell.SetCellValue(row.DampingWidth), (row, cell) => row.DampingWidth = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.DampingStrength), (row, cell) => cell.SetCellValue(row.DampingStrength), (row, cell) => row.DampingStrength = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MinX), (row, cell) => cell.SetCellValue(row.MinX), (row, cell) => row.MinX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MinY), (row, cell) => cell.SetCellValue(row.MinY), (row, cell) => row.MinY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MinZ), (row, cell) => cell.SetCellValue(row.MinZ), (row, cell) => row.MinZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MinW), (row, cell) => cell.SetCellValue(row.MinW), (row, cell) => row.MinW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MaxX), (row, cell) => cell.SetCellValue(row.MaxX), (row, cell) => row.MaxX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MaxY), (row, cell) => cell.SetCellValue(row.MaxY), (row, cell) => row.MaxY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MaxZ), (row, cell) => cell.SetCellValue(row.MaxZ), (row, cell) => row.MaxZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.Limiter.MaxW), (row, cell) => cell.SetCellValue(row.MaxW), (row, cell) => row.MaxW = (float)cell.NumericCellValue)
            );
            exchanger.Sheet(
                "InterpolatedMotionHeader",
                new List<Motion.InterpolatedMotionHeader>(new Motion.InterpolatedMotionHeader[] { root.InterpolatedMotionHeader }),
                sheetDef => sheetDef
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoneCount), (row, cell) => cell.SetCellValue(row.BoneCount), (row, cell) => row.BoneCount = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.TotalBoneCount), (row, cell) => cell.SetCellValue(row.TotalBoneCount), (row, cell) => row.TotalBoneCount = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.FrameCount), (row, cell) => cell.SetCellValue(row.FrameCount), (row, cell) => row.FrameCount = (int)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinX), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMinX), (row, cell) => row.BoundingBox.BoundingBoxMinX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinY), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMinY), (row, cell) => row.BoundingBox.BoundingBoxMinY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinZ), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMinZ), (row, cell) => row.BoundingBox.BoundingBoxMinZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinW), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMinW), (row, cell) => row.BoundingBox.BoundingBoxMinW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxX), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMaxX), (row, cell) => row.BoundingBox.BoundingBoxMaxX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxY), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMaxY), (row, cell) => row.BoundingBox.BoundingBoxMaxY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxZ), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMaxZ), (row, cell) => row.BoundingBox.BoundingBoxMaxZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxW), (row, cell) => cell.SetCellValue(row.BoundingBox.BoundingBoxMaxW), (row, cell) => row.BoundingBox.BoundingBoxMaxW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.FrameData.FrameStart), (row, cell) => cell.SetCellValue(row.FrameData.FrameStart), (row, cell) => row.FrameData.FrameStart = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.FrameData.FrameEnd), (row, cell) => cell.SetCellValue(row.FrameData.FrameEnd), (row, cell) => row.FrameData.FrameEnd = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.FrameData.FramesPerSecond), (row, cell) => cell.SetCellValue(row.FrameData.FramesPerSecond), (row, cell) => row.FrameData.FramesPerSecond = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.InterpolatedMotionHeader.FrameData.FrameReturn), (row, cell) => cell.SetCellValue(row.FrameData.FrameReturn), (row, cell) => row.FrameData.FrameReturn = (float)cell.NumericCellValue)
                    .Column("Reserved0", (row, cell) => cell.SetCellValue(row.Reserved[0]), (row, cell) => row.Reserved[0] = (int)cell.NumericCellValue)
                    .Column("Reserved1", (row, cell) => cell.SetCellValue(row.Reserved[1]), (row, cell) => row.Reserved[1] = (int)cell.NumericCellValue)
                    .Column("Reserved2", (row, cell) => cell.SetCellValue(row.Reserved[2]), (row, cell) => row.Reserved[2] = (int)cell.NumericCellValue),
                singleRow: true
            );
            exchanger.Sheet(
                "RootPosition",
                new List<Motion.RootPosition>(new Motion.RootPosition[] { root.RootPosition }),
                sheetDef => sheetDef
                    .Column(nameof(Motion.RootPosition.ScaleX), (row, cell) => cell.SetCellValue(row.ScaleX), (row, cell) => row.ScaleX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.ScaleY), (row, cell) => cell.SetCellValue(row.ScaleY), (row, cell) => row.ScaleY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.ScaleZ), (row, cell) => cell.SetCellValue(row.ScaleZ), (row, cell) => row.ScaleZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.NotUnit), (row, cell) => cell.SetCellValue(row.NotUnit), (row, cell) => row.NotUnit = (int)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.RotateX), (row, cell) => cell.SetCellValue(row.RotateX), (row, cell) => row.RotateX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.RotateY), (row, cell) => cell.SetCellValue(row.RotateY), (row, cell) => row.RotateY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.RotateZ), (row, cell) => cell.SetCellValue(row.RotateZ), (row, cell) => row.RotateZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.RotateW), (row, cell) => cell.SetCellValue(row.RotateW), (row, cell) => row.RotateW = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.TranslateX), (row, cell) => cell.SetCellValue(row.TranslateX), (row, cell) => row.TranslateX = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.TranslateY), (row, cell) => cell.SetCellValue(row.TranslateY), (row, cell) => row.TranslateY = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.TranslateZ), (row, cell) => cell.SetCellValue(row.TranslateZ), (row, cell) => row.TranslateZ = (float)cell.NumericCellValue)
                    .Column(nameof(Motion.RootPosition.TranslateW), (row, cell) => cell.SetCellValue(row.TranslateW), (row, cell) => row.TranslateW = (float)cell.NumericCellValue)
                    .Column("FCurveId0", (row, cell) => cell.SetCellValue(row.FCurveId[0]), (row, cell) => row.FCurveId[0] = (int)cell.NumericCellValue)
                    .Column("FCurveId1", (row, cell) => cell.SetCellValue(row.FCurveId[1]), (row, cell) => row.FCurveId[1] = (int)cell.NumericCellValue)
                    .Column("FCurveId2", (row, cell) => cell.SetCellValue(row.FCurveId[2]), (row, cell) => row.FCurveId[2] = (int)cell.NumericCellValue)
                    .Column("FCurveId3", (row, cell) => cell.SetCellValue(row.FCurveId[3]), (row, cell) => row.FCurveId[3] = (int)cell.NumericCellValue)
                    .Column("FCurveId4", (row, cell) => cell.SetCellValue(row.FCurveId[4]), (row, cell) => row.FCurveId[4] = (int)cell.NumericCellValue)
                    .Column("FCurveId5", (row, cell) => cell.SetCellValue(row.FCurveId[5]), (row, cell) => row.FCurveId[5] = (int)cell.NumericCellValue)
                    .Column("FCurveId6", (row, cell) => cell.SetCellValue(row.FCurveId[6]), (row, cell) => row.FCurveId[6] = (int)cell.NumericCellValue)
                    .Column("FCurveId7", (row, cell) => cell.SetCellValue(row.FCurveId[7]), (row, cell) => row.FCurveId[7] = (int)cell.NumericCellValue)
                    .Column("FCurveId8", (row, cell) => cell.SetCellValue(row.FCurveId[8]), (row, cell) => row.FCurveId[8] = (int)cell.NumericCellValue),
                singleRow: true
            );
        }

        private class DataExchange
        {
            private readonly List<ISheetDef> _sheetDefs = new List<ISheetDef>();

            public void ExportTo(Stream stream, Action<IMExProgress>? onProgress)
            {
                var book = new XSSFWorkbook();

                foreach (var (sheetDef, index) in _sheetDefs.SelectWithIndex())
                {
                    onProgress?.Invoke(new IMExProgress(index, _sheetDefs.Count(), sheetDef.SheetName));
                    sheetDef.ExportTo(book);
                }

                book.Write(stream);
            }

            public void ImportFrom(Stream stream, Action<IMExProgress>? onProgress, Action<Exception>? onError, Action<ImportSheetResult>? onResult)
            {
                var book = new XSSFWorkbook(stream);

                foreach (var (sheetDef, index) in _sheetDefs.SelectWithIndex())
                {
                    onProgress?.Invoke(new IMExProgress(index, _sheetDefs.Count(), sheetDef.SheetName));
                    var result = sheetDef.ImportFrom(book, onError);
                    onResult?.Invoke(result);
                }
            }

            public void Sheet<RowType>(
                string sheetName,
                List<RowType> list,
                Action<SheetDef<RowType>> onDefine,
                bool singleRow = false
            )
                where RowType : new()
            {
                var sheetDef = new SheetDef<RowType>(sheetName, list, singleRow);
                onDefine(sheetDef);
                _sheetDefs.Add(sheetDef);
            }

        }

        private record ColumnDef(
            string Header,
            Action<object, ICell> Exporter,
            Action<object, ICell> Importer
        )
        {

        }

        private record SingleDef(
            string Header,
            Action<object, ICell> Exporter,
            Func<ICell, object> Importer
        )
        {

        }

        private interface ISheetDef
        {
            void ExportTo(XSSFWorkbook book);
            ImportSheetResult ImportFrom(XSSFWorkbook book, Action<Exception>? onError);

            string SheetName { get; }
        }

        private record SheetDef<RowType>(
            string SheetName,
            List<RowType> List,
            bool SingleRow
        ) : ISheetDef
            where RowType : new()
        {
            internal List<ColumnDef> Columns { get; set; } = new List<ColumnDef>();
            internal SingleDef? SingleDef { get; set; }

            public SheetDef<RowType> Single(
                string header,
                Action<RowType, ICell> exporter,
                Func<ICell, RowType> importer
            )
            {
                SingleDef = (
                    new SingleDef(
                        header,
                        (any, cell) => exporter((RowType)any, cell),
                        (cell) => importer(cell)!
                    )
                );

                return this;
            }

            public SheetDef<RowType> Column(
                string header,
                Action<RowType, ICell> exporter,
                Action<RowType, ICell> importer
            )
            {
                Columns.Add(
                    new ColumnDef(
                        header,
                        (any, cell) => exporter((RowType)any, cell),
                        (any, cell) => importer((RowType)any, cell)
                    )
                );

                return this;
            }

            public void ExportTo(XSSFWorkbook book)
            {
                var xlsxSheet = book.CreateSheet(SheetName);
                {
                    var xlsxHeader = xlsxSheet.CreateRow(0);
                    {
                        var cell = xlsxHeader.CreateCell(0);
                        cell.SetCellValue("#");
                    }
                    if (SingleDef != null)
                    {
                        var cell = xlsxHeader.CreateCell(1);
                        cell.SetCellValue(SingleDef.Header);
                    }
                    else
                    {
                        foreach (var (column, index) in Columns.SelectWithIndex())
                        {
                            var cell = xlsxHeader.CreateCell(1 + index);
                            cell.SetCellValue(column.Header);
                        }
                    }
                }
                foreach (var (row, rowIndex) in List.SelectWithIndex())
                {
                    var xlsxData = xlsxSheet.CreateRow(1 + rowIndex);
                    {
                        var cell = xlsxData.CreateCell(0);
                        cell.SetCellValue(rowIndex);
                    }
                    if (SingleDef != null)
                    {
                        var cell = xlsxData.CreateCell(1);
                        SingleDef.Exporter(row!, cell);
                    }
                    else
                    {
                        foreach (var (column, cellIndex) in Columns.SelectWithIndex())
                        {
                            var cell = xlsxData.CreateCell(1 + cellIndex);
                            column.Exporter(row!, cell);
                        }
                    }
                }
                xlsxSheet.CreateFreezePane(1, 1);
            }

            public ImportSheetResult ImportFrom(XSSFWorkbook book, Action<Exception>? onError)
            {
                var xlsxSheet = book.GetSheet(SheetName);
                if (xlsxSheet == null)
                {
                    return new ImportSheetResult($"The sheet \"{SheetName}\" is not found. Skipped.", IsWarning: true);
                }

                var headerNames = new List<string>();

                var addError = onError ?? (ex => { });

                {
                    var xlsxHeader = xlsxSheet.GetRow(0);
                    if (xlsxHeader == null)
                    {
                        return new ImportSheetResult($"The sheet \"{SheetName}\" has no header line. Skipped.", IsWarning: true);
                    }

                    {
                        var xlsxCell = xlsxHeader.GetCell(0);
                        if (xlsxCell == null || xlsxCell.StringCellValue != "#")
                        {
                            return new ImportSheetResult($"The sheet \"{SheetName}\" has no first header column: \"#\". Skipped.", IsWarning: true);
                        }
                    }

                    for (int x = 1; ; x++)
                    {
                        var xlsxCell = xlsxHeader.GetCell(x);
                        if (xlsxCell == null)
                        {
                            break;
                        }

                        headerNames.Add(xlsxCell.StringCellValue);
                    }
                }

                if (!headerNames.Any())
                {
                    return new ImportSheetResult($"The sheet \"{SheetName}\" has no header columns to be imported. Skipped.", IsWarning: true);
                }

                int y = 0;
                for (; ; y++)
                {
                    var xlsxData = xlsxSheet.GetRow(1 + y);
                    if (xlsxData == null)
                    {
                        break;
                    }

                    {
                        var xlsxSharp = xlsxData.GetCell(0);
                        if (false
                            || xlsxSharp == null
                            || xlsxSharp.CellType != CellType.Numeric
                            || xlsxSharp.NumericCellValue != y
                        )
                        {
                            break;
                        }
                    }

                    if (List.Count <= y)
                    {
                        if (SingleRow)
                        {
                            addError(new Exception($"{SheetName} sheet is a single row table. The row cannot be increased."));
                            return new ImportSheetResult($"The sheet \"{SheetName}\" has critical error. Skipped.", IsWarning: true);
                        }

                        List.Add(new RowType());
                    }

                    for (int x = 0; x < headerNames.Count; x++)
                    {
                        try
                        {
                            var xlsxCell = xlsxData.GetCell(1 + x);
                            if (xlsxCell == null)
                            {
                                continue;
                            }

                            if (SingleDef != null)
                            {
                                if (SingleDef.Header == headerNames[x])
                                {
                                    List[y] = (RowType)SingleDef.Importer(xlsxCell);
                                }
                            }
                            else
                            {
                                var columnDef = Columns.FirstOrDefault(it => it.Header == headerNames[x]);
                                if (columnDef == null)
                                {
                                    continue;
                                }

                                columnDef.Importer(List[y]!, xlsxCell);
                            }
                        }
                        catch (Exception ex)
                        {
                            addError(new Exception($"A {SheetName} sheet {headerNames[x]} cell at {new CellAddress(2 + y, 2 + x)} cannot be imported due to error.", ex));
                        }
                    }
                }

                while (y < List.Count)
                {
                    if (SingleRow)
                    {
                        addError(new Exception($"{SheetName} sheet is a single row table. The row cannot be decreased."));
                        return new ImportSheetResult($"The sheet \"{SheetName}\" has critical error. Skipped.", IsWarning: true);
                    }

                    List.RemoveAt(List.Count - 1);
                }

                return new ImportSheetResult($"The sheet \"{SheetName}\" supplied {List.Count} items.");
            }
        }
    }
}
