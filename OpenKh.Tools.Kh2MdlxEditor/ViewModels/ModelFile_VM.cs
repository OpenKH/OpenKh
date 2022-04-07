using OpenKh.Kh2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static OpenKh.Kh2.Mdlx;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class ModelFile_VM
    {
        public Mdlx ModelFile { get; set; }
        public ObservableCollection<SubModelWrapper> subModelList { get; set; }

        public ModelFile_VM() { }
        public ModelFile_VM(Mdlx modelFile)
        {
            ModelFile = modelFile;

            subModelList = SubModelWrapper.getObservable(ModelFile.SubModels);
        }

        public class SubModelWrapper
        {
            public SubModel subModel { get; set; }

            public string TypeName
            {
                get
                {
                    switch (subModel.Type)
                    {
                        case 2:
                            return "Map";
                            break;
                        case 3:
                            return "Entity";
                            break;
                        case 4:
                            return "Shadow";
                            break;
                        default:
                            return "Other";
                            break;
                    }
                }
            }

            public SubModelWrapper(SubModel subModel)
            {
                this.subModel = subModel;
            }

            public static ObservableCollection<SubModelWrapper> getObservable(List<SubModel> submodelList)
            {
                ObservableCollection<SubModelWrapper> wrappedList = new ObservableCollection<SubModelWrapper>();
                foreach (SubModel subModel in submodelList)
                {
                    wrappedList.Add(new SubModelWrapper(subModel));
                }
                return wrappedList;
            }
        }
    }
}
