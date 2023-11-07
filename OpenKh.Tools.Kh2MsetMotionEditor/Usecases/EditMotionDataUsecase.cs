using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class EditMotionDataUsecase
    {
        private readonly LoadedModel _loadedModel;

        public EditMotionDataUsecase(LoadedModel loadedModel)
        {
            _loadedModel = loadedModel;
        }

        internal short AssignTangentId(short index, float data)
        {
            if (_loadedModel.MotionData?.KeyTangents is List<float> list)
            {
                var idx = (short)list.Count;
                list.Add(data);
                return idx;
            }
            else
            {
                return 0;
            }
        }

        internal short AssignTimeId(short index, float data)
        {
            if (_loadedModel.MotionData?.KeyTimes is List<float> list)
            {
                var idx = (short)list.Count;
                list.Add(data);
                return idx;
            }
            else
            {
                return 0;
            }
        }

        internal short AssignValueId(short index, float data)
        {
            if (_loadedModel.MotionData?.KeyValues is List<float> list)
            {
                var idx = (short)list.Count;
                list.Add(data);
                return idx;
            }
            else
            {
                return 0;
            }
        }
    }
}
