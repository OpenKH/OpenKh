using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public class DAE : Model
    {
        public XmlNodeList images;
        public XmlNodeList materials;
        public XmlNodeList effects;
        public XmlNodeList geometries;
        public XmlNodeList controllers;
        public XmlNodeList visual_scenes;
        
        XmlDocument Document;
        
        public DAE(string filename) : base(filename)
        {
            if (this.RootCopy != null)
                return;
            byte[] fileData = File.ReadAllBytes(filename);

            for (int i = 0; i < 500; i++)
            {
                if (fileData[i + 0] != 0x78) continue;
                if (fileData[i + 1] != 0x6D) continue;
                if (fileData[i + 2] != 0x6C) continue;
                if (fileData[i + 3] != 0x6E) continue;
                if (fileData[i + 4] != 0x73) continue;
                fileData[i + 0] = 0x77;
            }

            this.Document = new XmlDocument();
            try
            {
                this.Document.LoadXml(Encoding.ASCII.GetString(fileData));
            }
            catch
            {
                try
                {
                    this.Document.LoadXml(Encoding.UTF8.GetString(fileData));
                }
                catch
                {
                    this.Document.LoadXml(Encoding.Unicode.GetString(fileData));
                }
            }

            this.images = this.Document.SelectNodes("descendant::library_images/image");
            this.materials = this.Document.SelectNodes("descendant::library_materials/material");
            this.effects = this.Document.SelectNodes("descendant::library_effects/effect");
            this.geometries = this.Document.SelectNodes("descendant::library_geometries/geometry");
            this.controllers = this.Document.SelectNodes("descendant::library_controllers/controller");
            this.visual_scenes = this.Document.SelectNodes("descendant::library_visual_scenes/visual_scene");

            this.ImagesIDs = new List<string>(0);
            this.ImagesFilenames = new List<string>(0);

            this.PerGeometryMaterials = new List<string>(0);
            this.MaterialsIDs = new List<string>(0);
            this.MaterialsEffectIDs = new List<string>(0);
            this.EffectsIDs = new List<string>(0);
            this.EffectsImageIDs = new List<string>(0);
            this.GeometryIDs = new List<string>(0);

            this.GeometryDataVertex = new List<Vector3[]>(0);
            this.GeometryDataTexcoordinates = new List<Vector2[]>(0);
            this.GeometryDataNormals = new List<Vector3[]>(0);
            this.GeometryDataColors = new List<Color4[]>(0);

            this.GeometryDataVertex_i = new List<List<int>>(0);
            this.GeometryDataTexcoordinates_i = new List<List<int>>(0);
            this.GeometryDataNormals_i = new List<List<int>>(0);
            this.GeometryDataColors_i = new List<List<int>>(0);

            this.ControllerDataJoints_i = new List<List<List<int>>>(0);
            this.ControllerDataMatrices_i = new List<List<List<int>>>(0);
            this.ControllerDataWeights_i = new List<List<List<int>>>(0);

            this.ControllersIDs = new List<string>(0);
            this.PerControllerGeometry = new List<string>(0);

            this.ShapeMatrices = new List<Matrix4 >(0);
            this.ControllerDataJoints = new List<string[]>(0);
            this.ControllerDataJoints_indices = new List<int[]>(0);
            this.ControllerDataMatrices = new List<Matrix4 []>(0);
            this.ControllerDataWeights = new List<float[]>(0);
            this.VisualScenesIDs = new List<string>(0);

            this.JointsIDs = new List<List<string>>(0);
            this.JointsMatrices = new List<List<Matrix4 >>(0);
            this.SurfacesIDs = new List<List<string>>(0);
            this.SurfacesMaterialsID = new List<List<string>>(0);

            this.Parse();
        }
        
        

        public enum DisplayMode
        {
            Normal = 0,
            Color = 1
        }
       

        
        
        

        public unsafe void Parse()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = StaticConstants.en;
            System.Threading.Thread.CurrentThread.CurrentCulture = StaticConstants.en;

            int maxOffset = -1;
            float currVal = 0;
            int valCount = 0;
            int valIndex = 0;
            char separator = ' ';

            #region Parsing Joints


            XmlNodeList joints = this.Document.SelectNodes("descendant::library_visual_scenes//node[translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");

            this.Skeleton = new Skeleton(joints.Count);

            for (int i = 0; i < joints.Count; i++)
            {
                string jointName = joints[i].Attributes["name"].Value;
                this.Skeleton.Joints[i] = new Joint(jointName, this.ParseMatrices(joints[i].SelectNodes("matrix")[0].InnerText, 1)[0]);
            }

            for (int i = 0; i < joints.Count; i++)
            {
                XmlNode parent = joints[i].ParentNode;
                if (parent.Attributes["type"] != null && parent.Attributes["type"].Value.ToLower() == "joint")
                {
                    string parentJointName = parent.Attributes["name"].Value;
                    for (int p = 0; p < this.Skeleton.Joints.Length; p++)
                    {
                        if (this.Skeleton.Joints[p].Name == parentJointName)
                        {
                            this.Skeleton.Joints[i].Parent = p;
                            break;
                        }
                    }
                }
            }
            this.Skeleton.ComputeMatrices(Matrix4.CreateScale(1f));


            #endregion

            #region Parsing Images
            for (int i = 0; i < this.images.Count; i++)
            {
                var initFromNode = this.images[i].SelectNodes("init_from");
                if (initFromNode.Count > 0)
                {
                    this.ImagesIDs.Add(this.images[i].Attributes["id"].Value);
                    this.ImagesFilenames.Add(initFromNode[0].InnerText);
                }
            }
			/*string[] pngs = System.IO.Directory.GetFiles(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08", "*.png");
			ResourceLoader.EmptyBMP.SetPixel(0, 0, System.Drawing.Color.Red);
			for (int i = 0; i < pngs.Length; i++)
			{
				ResourceLoader.EmptyBMP.Save(pngs[i]);
			}*/
                    /*string[] pngs = System.IO.Directory.GetFiles(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08","*.png");
                    for (int i=0;i< pngs.Length;i++)
                    {
                        bool has_ = false;
                        for (int j = 0; j < this.ImagesFilenames.Count; j++)
                        {
                            if (this.ImagesFilenames[j] == Path.GetFileName(pngs[i]))
                            {
                                has_ = true;
                                break;
                            }

                        }
                        if (!has_)
                        {
                            File.Move(pngs[i], pngs[i].Replace(".png", ".deleteme"));
                        }
                    }*/
#endregion

                    #region Parsing Materials
                    for (int i = 0; i < this.materials.Count; i++)
            {
                var instanceEffectNode = this.materials[i].SelectNodes("instance_effect");
                if (instanceEffectNode.Count > 0)
                {
                    string url = instanceEffectNode[0].Attributes["url"].Value;
                    if (url.Length > 0 && url[0] == '#')
                        url = url.Remove(0, 1);

                    this.MaterialsEffectIDs.Add(url);
                }
                else
                {
                    this.MaterialsEffectIDs.Add("");
                }
                string matID = this.materials[i].Attributes["id"].Value;
                /*var nodesProp = this.materials[i].SelectNodes("descendant::user_properties");
                if (nodesProp.Count>0 &&
                    this.Document.SelectNodes("//*[@target='#" + matID + "']").Count == 0 &&
                    this.Document.SelectNodes("//*[@material='" + matID + "']").Count == 0)
                    matID = nodesProp[0].InnerText;*/

                this.MaterialsIDs.Add(matID);
            }
            #endregion

            #region Parsing Effects
            for (int i = 0; i < this.effects.Count; i++)
            {
                this.EffectsIDs.Add(this.effects[i].Attributes["id"].Value);
                var textureNode = this.effects[i].SelectNodes("descendant::texture");
                if (textureNode.Count > 0)
                {
                    string corresponding_imageID = textureNode[0].Attributes["texture"].Value;

                    if (!ImagesIDs.Contains(corresponding_imageID))
                    {
                        var rech = this.effects[i].SelectNodes("descendant::*[@id='" + corresponding_imageID + "' or @sid='" + corresponding_imageID + "']");
                        if (rech.Count>0)
                        {
                            rech = rech[0].SelectNodes("descendant::source");
                            if (rech.Count > 0)
                            {
                                rech = this.effects[i].SelectNodes("descendant::*[@id='" + rech[0].InnerText + "' or @sid='" + rech[0].InnerText + "']");
                                if (rech.Count > 0)
                                {
                                    rech = rech[0].SelectNodes("descendant::init_from");
                                    if (rech.Count > 0)
                                    {
                                        corresponding_imageID = rech[0].InnerText;
                                    }
                                }
                            }
                        }
                    }

                    this.EffectsImageIDs.Add(corresponding_imageID);
                }
                else
                {
                    this.EffectsImageIDs.Add("");
                }
            }
            #endregion

            #region Parsing Geometries
            for (int i = 0; i < this.geometries.Count; i++)
            {
                this.GeometryIDs.Add(this.geometries[i].Attributes["id"].Value);
                string position_SourceID = "";
                string normal_SourceID = "";
                string texcoord_SourceID = "";
                string color_SourceID = "";

                int position_SourceOffset = 0;
                int normal_SourceOffset = -1;
                int texcoord_SourceOffset = -1;
                int color_SourceOffset = -1;

                var verticesNode = this.geometries[i].SelectNodes("descendant::vertices");
                var trianglesNode = this.geometries[i].SelectNodes("descendant::triangles");
                if (trianglesNode.Count == 0)
                {
                    trianglesNode = this.geometries[i].SelectNodes("descendant::polylist");
                }

                if (trianglesNode.Count>0)
                {
                    int countTri = int.Parse(trianglesNode[0].Attributes["count"].Value);
                    var rootPNode = trianglesNode[0].SelectNodes("descendant::p")[0];
                    for (int t=1;t< trianglesNode.Count;t++)
                    {
                        int count_ = int.Parse(trianglesNode[t].Attributes["count"].Value);
                        string inner = trianglesNode[t].SelectNodes("descendant::p")[0].InnerText;
                        while (inner[0]==separator)
                        {
                            inner = inner.Remove(0, 1);
                        }
                        inner = " " + inner;
                        rootPNode.InnerText += inner;
                        countTri += count_;
                    }
                }
                string matID = trianglesNode[0].Attributes["material"].Value;

                if (!MaterialsIDs.Contains(matID))
                {
                    string geoID = GeometryIDs[GeometryIDs.Count-1];
                    var fixMatID = this.Document.SelectNodes("//instance_geometry[@url='#" + geoID + "' or @target='#" + geoID + "']");
                    if (fixMatID.Count>0)
                    {
                        fixMatID = fixMatID[0].SelectNodes("descendant::instance_material/@target");
                        if (fixMatID.Count > 0)
                        {
                            string newMatID = fixMatID[0].InnerText.Remove(0, 1);
                            if (MaterialsIDs.Contains(newMatID))
                            matID = newMatID;
                        }
                    }
                }
                this.PerGeometryMaterials.Add(matID);

                var vertexSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='vertex']");
                if (vertexSemanticNode.Count > 0)
                {
                    var offsetAttribute = vertexSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        position_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vertexSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        position_SourceID = sourceID_Attribute.Value;
                }
                if (verticesNode.Count > 0)
                {
                    position_SourceID = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='position']")[0].Attributes["source"].Value;

                    var normal_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='normal']");
                    if (normal_SourceNode.Count > 0)
                    {
                        normal_SourceID = normal_SourceNode[0].Attributes["source"].Value;
                        normal_SourceOffset = position_SourceOffset;
                    }

                    var texcoord_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='texcoord']");
                    if (texcoord_SourceNode.Count > 0)
                    {
                        texcoord_SourceID = texcoord_SourceNode[0].Attributes["source"].Value;
                        texcoord_SourceOffset = position_SourceOffset;
                    }

                    var color_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='color']");
                    if (color_SourceNode.Count > 0)
                    {
                        color_SourceID = color_SourceNode[0].Attributes["source"].Value;
                        color_SourceOffset = position_SourceOffset;
                    }
                }

                var texcoordSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='texcoord']");
                if (texcoordSemanticNode.Count > 0)
                {
                    var offsetAttribute = texcoordSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        texcoord_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = texcoordSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        texcoord_SourceID = sourceID_Attribute.Value;
                }

                var normalSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='normal']");
                if (normalSemanticNode.Count > 0)
                {
                    var offsetAttribute = normalSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        normal_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = normalSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        normal_SourceID = sourceID_Attribute.Value;
                }

                var colorSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='color']");
                if (colorSemanticNode.Count > 0)
                {
                    var offsetAttribute = colorSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        color_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = colorSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        color_SourceID = sourceID_Attribute.Value;
                }


                if (position_SourceID.Length > 0 && position_SourceID[0] == '#')
                    position_SourceID = position_SourceID.Remove(0, 1);
                if (normal_SourceID.Length > 0 && normal_SourceID[0] == '#')
                    normal_SourceID = normal_SourceID.Remove(0, 1);
                if (texcoord_SourceID.Length > 0 && texcoord_SourceID[0] == '#')
                    texcoord_SourceID = texcoord_SourceID.Remove(0, 1);
                if (color_SourceID.Length > 0 && color_SourceID[0] == '#')
                    color_SourceID = color_SourceID.Remove(0, 1);

                Vector3[] Vertices = new Vector3[0];
                Vector2[] TexCoordinates = new Vector2[0];
                Vector3[] Normals = new Vector3[0];
                Color4[] Colors = new Color4[0];

                #region Parsing POSITION-Array
                XmlNode source = this.geometries[i].SelectNodes("descendant::source[@id='" + position_SourceID + "']")[0];
                XmlNode accessor = source.SelectNodes("descendant::accessor")[0];
                int count = int.Parse(accessor.Attributes["count"].Value);
                string floatArray = source.SelectNodes("float_array")[0].InnerText;

                for (int j = 2; j < floatArray.Length && j < 20; j++)
                {
                    if (floatArray[j] == 9 ||
                        floatArray[j] == 32 ||
                        floatArray[j] == 160)
                    {
                        separator = floatArray[j];
                        break;
                    }
                }

                Vertices = new Vector3[count];
                string[] split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                valIndex = 0;

                for (int j = 0; j < split.Length; j++)
                {
                    if (Single.TryParse(split[j], out currVal))
                    {
                        if (valCount % 3 == 0)
                        {
                            Vertices[valIndex].X = currVal;// -currVal; kokodayo
                        }
                        if (valCount % 3 == 1)
                        {
                            Vertices[valIndex].Y = currVal;// -currVal;
                        }
                        if (valCount % 3 == 2)
                        {
                            Vertices[valIndex].Z = currVal;
                            valIndex++;
                        }
                        valCount++;
                    }
                }
                #endregion

                #region Parsing TEXCOORD-Array
                if (texcoord_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + texcoord_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    TexCoordinates = new Vector2[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 2 == 0)
                            {
                                TexCoordinates[valIndex].X = currVal;
                            }
                            if (valCount % 2 == 1)
                            {
                                TexCoordinates[valIndex].Y = currVal;
                                //currVal = 1 - currVal;
                                valIndex++;
                            }
                            valCount++;
                        }
                    }
                }
                #endregion

                #region Parsing NORMAL-Array
                if (normal_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + normal_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Normals = new Vector3[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 3 == 0)
                            {
                                Normals[valIndex].X = currVal;
                            }
                            if (valCount % 3 == 1)
                            {
                                Normals[valIndex].Y = currVal;
                            }
                            if (valCount % 3 == 2)
                            {
                                Normals[valIndex].Z = currVal;
                                valIndex++;
                            }
                            valCount++;
                        }
                    }
                }
                #endregion

                Vector4 currV4 = Vector4.Zero;

                #region Parsing COLOR-Array
                if (color_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + color_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Colors = new Color4[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 4 == 0)
                            {
                                currV4.X = currVal;
                            }
                            if (valCount % 4 == 1)
                            {
                                currV4.Y = currVal;
                            }
                            if (valCount % 4 == 2)
                            {
                                currV4.Z = currVal;
                            }
                            if (valCount % 4 == 3)
                            {
                                currV4.W = currVal;
                                Colors[valIndex] = new Color4(currV4.X, currV4.Y, currV4.Z, currV4.W);
                                valIndex++;
                            }
                            valCount++;
                        }
                    }
                }
                #endregion

                #region Parsing Triangles-Array
                string TriangleIndices = trianglesNode[0].SelectNodes("p")[0].InnerText;
                split = TriangleIndices.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                maxOffset = -1;
                if (position_SourceOffset > maxOffset)
                    maxOffset = position_SourceOffset;
                if (texcoord_SourceOffset > maxOffset)
                    maxOffset = texcoord_SourceOffset;
                if (normal_SourceOffset > maxOffset)
                    maxOffset = normal_SourceOffset;
                if (color_SourceOffset > maxOffset)
                    maxOffset = color_SourceOffset;
                maxOffset++;
                valCount = 0;
                int currInt = 0;



                this.GeometryDataVertex.Add(Vertices);
                this.GeometryDataTexcoordinates.Add(TexCoordinates);
                this.GeometryDataNormals.Add(Normals);
                this.GeometryDataColors.Add(Colors);

                this.GeometryDataVertex_i.Add(new List<int>(0));
                this.GeometryDataTexcoordinates_i.Add(new List<int>(0));
                this.GeometryDataNormals_i.Add(new List<int>(0));
                this.GeometryDataColors_i.Add(new List<int>(0));

                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        if (valCount % maxOffset == position_SourceOffset)
                        {
                            this.GeometryDataVertex_i[this.GeometryDataVertex.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == texcoord_SourceOffset)
                        {
                            this.GeometryDataTexcoordinates_i[this.GeometryDataTexcoordinates.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == normal_SourceOffset)
                        {
                            this.GeometryDataNormals_i[this.GeometryDataNormals.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == color_SourceOffset)
                        {
                            this.GeometryDataColors_i[this.GeometryDataColors.Count - 1].Add(currInt);
                        }
                        valCount++;
                    }
                }
                #endregion

            }
            #endregion

            #region Parse Controllers

            for (int i = 0; i < this.controllers.Count; i++)
            {
                string matID = this.PerGeometryMaterials[i];

                if (!MaterialsIDs.Contains(matID))
                {
                    string contID = this.controllers[i].Attributes["id"].Value;
                    var fixMatID = this.controllers[i].SelectNodes("//instance_controller[@url='#" + contID + "' or @target='#" + contID + "']");
                    if (fixMatID.Count > 0)
                    {
                        fixMatID = fixMatID[0].SelectNodes("descendant::instance_material/@target");
                        if (fixMatID.Count > 0)
                        {
                            string newMatID = fixMatID[0].InnerText.Remove(0, 1);
                            if (MaterialsIDs.Contains(newMatID))
                                this.PerGeometryMaterials[i] = newMatID;
                        }
                    }
                }

                Matrix4  shapeMatrix = Matrix4 .Identity;

                string joints_SourceID = "";
                string matrices_SourceID = "";
                string weights_SourceID = "";

                int joints_SourceOffset = -1;
                int matrices_SourceOffset = -1;
                int weights_SourceOffset = -1;

                var shapeMatrixNode = this.controllers[i].SelectNodes("descendant::skin/bind_shape_matrix");
                if (shapeMatrixNode.Count > 0)
                    shapeMatrix = Matrix4 .Identity;// ParseMatrices(shapeMatrixNode[0].InnerText, 1)[0];

                var jointsNode = this.controllers[i].SelectNodes("descendant::joints");
                var vertexWeightsNode = this.controllers[i].SelectNodes("descendant::vertex_weights");

                var vwJointSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");
                if (vwJointSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwJointSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        joints_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwJointSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        joints_SourceID = sourceID_Attribute.Value;
                }

                if (jointsNode.Count > 0)
                {
                    var jointSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");
                    if (jointSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = jointSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                            joints_SourceID = sourceID_Attribute.Value;
                    }

                    var weightSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='weight']");
                    if (weightSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = weightSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                        {
                            weights_SourceID = sourceID_Attribute.Value;
                            weights_SourceOffset = joints_SourceOffset;
                        }
                    }

                    var matriceSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='inv_bind_matrix']");
                    if (matriceSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = matriceSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                        {
                            matrices_SourceID = sourceID_Attribute.Value;
                            matrices_SourceOffset = joints_SourceOffset;
                        }
                    }
                }

                var vwWeightSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='weight']");
                if (vwWeightSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwWeightSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        weights_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwWeightSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        weights_SourceID = sourceID_Attribute.Value;
                }

                var vwMatrixSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='inv_bind_matrix']");
                if (vwMatrixSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwMatrixSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        matrices_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwMatrixSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        matrices_SourceID = sourceID_Attribute.Value;
                }

                if (joints_SourceID.Length > 0 && joints_SourceID[0] == '#')
                    joints_SourceID = joints_SourceID.Remove(0, 1);
                if (matrices_SourceID.Length > 0 && matrices_SourceID[0] == '#')
                    matrices_SourceID = matrices_SourceID.Remove(0, 1);
                if (weights_SourceID.Length > 0 && weights_SourceID[0] == '#')
                    weights_SourceID = weights_SourceID.Remove(0, 1);

                #region Parsing JOINT-Array
                XmlNode source = this.controllers[i].SelectNodes("descendant::source[@id='" + joints_SourceID + "']")[0];
                XmlNode accessor = source.SelectNodes("descendant::accessor")[0];
                int count = int.Parse(accessor.Attributes["count"].Value);
                string nameArray = source.SelectNodes("Name_array")[0].InnerText;
                valIndex = 0;

                string[] Joints = new string[count];
                Matrix4 [] Matrices = new Matrix4 [0];
                float[] Weights = new float[0];

                string[] split = nameArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                for (int j = 0; j < split.Length; j++)
                {
                    for (int k = 0; k < this.Skeleton.Joints.Length; k++)
                        if (this.Skeleton.Joints[k].Name ==  split[j])
                        {
                            Joints[valIndex] = split[j];
                            valIndex++;
                            break;
                        }
                }
                #endregion

                #region Parsing Matrices-Array

                if (matrices_SourceOffset > -1)
                {
                    source = this.controllers[i].SelectNodes("descendant::source[@id='" + matrices_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);

                    string floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Matrices = ParseMatrices(floatArray, count);
                }
                #endregion

                #region Parsing Weights-Array

                if (weights_SourceOffset > -1)
                {
                    source = this.controllers[i].SelectNodes("descendant::source[@id='" + weights_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);

                    string floatArray = source.SelectNodes("float_array")[0].InnerText;

                    valIndex = 0;
                    Weights = new float[count];

                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            Weights[valIndex] = currVal;
                            valIndex++;
                        }
                    }
                }
                #endregion


                #region Parsing Vertex-Weights
                string vcount = vertexWeightsNode[0].SelectNodes("vcount")[0].InnerText;
                string v = vertexWeightsNode[0].SelectNodes("v")[0].InnerText;


                maxOffset = -1;
                if (joints_SourceOffset > maxOffset)
                    maxOffset = joints_SourceOffset;
                if (matrices_SourceOffset > maxOffset)
                    maxOffset = matrices_SourceOffset;
                if (weights_SourceOffset > maxOffset)
                    maxOffset = weights_SourceOffset;
                maxOffset++;
                

                this.ControllerDataJoints.Add(Joints);
                int[] ji_array = new int[Joints.Length];

                for (int ji=0;ji< Joints.Length;ji++)
                {
                    for (int s = 0; s < this.Skeleton.Joints.Length; s++)
                        if (this.Skeleton.Joints[s].Name == Joints[ji])
                        {
                            ji_array[ji] = s;
                            break;
                        }
                }
                this.ControllerDataJoints_indices.Add(ji_array);

                this.ControllerDataMatrices.Add(Matrices);
                this.ControllerDataWeights.Add(Weights);

                this.ControllerDataJoints_i.Add(new List<List<int>>(0));
                this.ControllerDataMatrices_i.Add(new List<List<int>>(0));
                this.ControllerDataWeights_i.Add(new List<List<int>>(0));


                split = vcount.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                List<int> vcountInt = new List<int>(0);

                int currInt = 0;

                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        this.ControllerDataJoints_i[this.ControllerDataJoints_i.Count - 1].Add(new List<int>(0));
                        this.ControllerDataMatrices_i[this.ControllerDataMatrices_i.Count - 1].Add(new List<int>(0));
                        this.ControllerDataWeights_i[this.ControllerDataWeights_i.Count - 1].Add(new List<int>(0));
                        vcountInt.Add(currInt);
                    }
                }

                valCount = 0;
                currInt = 0;
                int currWeightIndex = -1;
                int currvCountIndex = 0;

                split = v.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        if (valCount % maxOffset == joints_SourceOffset)
                        {
                            this.ControllerDataJoints_i[this.ControllerDataJoints_i.Count - 1][currvCountIndex].Add(currInt);
                        }
                        if (valCount % maxOffset == matrices_SourceOffset)
                        {
                            this.ControllerDataMatrices_i[this.ControllerDataMatrices_i.Count - 1][currvCountIndex].Add(currInt);
                        }
                        if (valCount % maxOffset == weights_SourceOffset)
                        {
                            this.ControllerDataWeights_i[this.ControllerDataWeights_i.Count - 1][currvCountIndex].Add(currInt);
                            currWeightIndex++;
                            if (currWeightIndex == vcountInt[currvCountIndex] - 1)
                            {
                                currWeightIndex = -1;
                                currvCountIndex++;
                            }
                        }
                        valCount++;
                    }
                }
                #endregion

                this.ShapeMatrices.Add(shapeMatrix);
                this.ControllersIDs.Add(this.controllers[i].Attributes["id"].Value);
                string pcg = "";
                var node = this.controllers[i].SelectSingleNode("skin");
                if (node !=null && node.Attributes["source"] !=null)
                {
                    pcg = node.Attributes["source"].Value.Remove(0, 1);
                }
                this.PerControllerGeometry.Add(pcg);
            }

            #endregion
            #region Get Per-Geometry Textures
            for (int i = 0; i < this.PerGeometryMaterials.Count; i++)
            {
                //string currEffectID = this.MaterialsEffectIDs[MaterialsIDs.IndexOf(this.PerGeometryMaterials[i])];
                //string currImageID = this.EffectsImageIDs[this.EffectsIDs.IndexOf(currEffectID)];

                string currEffectID = "";

                int indmID = MaterialsIDs.IndexOf(this.PerGeometryMaterials[i]);
                if (indmID > -1 && indmID < this.MaterialsEffectIDs.Count)
                    currEffectID = this.MaterialsEffectIDs[indmID];

                string currImageID = "";
                int indmEID = this.EffectsIDs.IndexOf(currEffectID);
                if (indmEID > -1 && indmEID < this.EffectsImageIDs.Count)
                    currImageID = this.EffectsImageIDs[indmEID];

                string currImageFileName = "";

                int ind = this.ImagesIDs.IndexOf(currImageID);
                if (ind>-1)
                {
                    currImageFileName = this.ImagesFilenames[ind];
                }
            }

            #endregion



            this.textures = new int[this.GeometryDataVertex_i.Count];
            this.texturesAlpha = new bool[this.GeometryDataVertex_i.Count];


            Color4 lastColor = Color4.White;
            Vector3 lastNormal = Vector3.Zero;

            int cCountPass = 0;
            int nCountPass = 0;

            int vCountPass = 0;
            int rvCountPass = 0;
            this.MeshOffsets = new int[this.GeometryDataVertex_i.Count][];
            this.meshesColor = new bool[this.GeometryDataVertex_i.Count];
            this.meshesNormal = new bool[this.GeometryDataVertex_i.Count];

            for (int i = 0; i < this.GeometryDataVertex_i.Count; i++)
            {
                int indexof_cont = -1;

                for (int pcg = 0; pcg < this.PerControllerGeometry.Count; pcg++)
                {
                    if (this.PerControllerGeometry[pcg] == this.GeometryIDs[i])
                    {
                        indexof_cont = pcg;
                        break;
                    }
                }

                int[] offset = new int[] { vCountPass ,0};

                bool hasColor = false;
                bool hasNormal = false;

                bool hasColorIndices = this.GeometryDataColors_i[i].Count > 0;
                bool hasNormalIndices = this.GeometryDataNormals_i[i].Count > 0;

                for (int j = 0; j < this.GeometryDataVertex_i[i].Count; j++)
                {
                    int vertexIndex = this.GeometryDataVertex_i[i][j];
                    if (hasColorIndices)
                    {
                        int colorIndex = this.GeometryDataColors_i[i][j];
                        Color4 col = this.GeometryDataColors[i][colorIndex];
                        if (col.ToArgb() != lastColor.ToArgb())
                        {
                            hasColor = true;
                            cCountPass++;
                            lastColor = col;
                        }
                    }
                    if (hasNormalIndices)
                    {
                        int normalIndex = this.GeometryDataNormals_i[i][j];
                        Vector3 normal = this.GeometryDataNormals[i][normalIndex];
                        if (Vector3.Distance(normal, lastNormal) > 0)
                        {
                            hasNormal = true;
                            nCountPass++;
                            lastNormal = normal;
                        }
                    }

                    vCountPass++;
                    byte infCount = (byte)this.ControllerDataJoints_i[indexof_cont][vertexIndex].Count;

                    for (int k = 0; k < infCount; k++)
                    {
                        int weightIndex = this.ControllerDataWeights_i[indexof_cont][vertexIndex][k];
                        float weight = this.ControllerDataWeights[indexof_cont][weightIndex];
                        if (weight > 0f)
                        {
                            rvCountPass++;
                        }
                    }
                }

                this.meshesNormal[i] = hasNormal;
                this.meshesColor[i] = hasColor;

                offset[1] = vCountPass - offset[0];
                this.MeshOffsets[i] = offset;
            }

            this.vertices = new Vector3[vCountPass];
            this.textureCoordinates = new Vector2[vCountPass];
            this.colors = new Color4[cCountPass];
            this.normals = new Vector3[nCountPass];

            this.colorsNew = new bool[vCountPass];
            this.normalsNew = new bool[vCountPass];

            this.weightedVertices = new WeightedVertex[rvCountPass];
            this.influences = new int[rvCountPass];

            vCountPass = 0;
            rvCountPass = 0;
            nCountPass = 0;
            cCountPass = 0;

            lastColor = Color4.White;
            lastNormal = Vector3.Zero;

            for (int i=0;i<this.GeometryDataVertex_i.Count;i++)
            {
                textures[i] = StaticConstants.whiteTexture_1x1;
                texturesAlpha[i] = false;
                if (i < PerGeometryMaterials.Count)
                {
                    string materialID = PerGeometryMaterials[i];
                    int effectIndex = MaterialsIDs.IndexOf(materialID);
                    if (effectIndex > -1)
                    {
                        string effectID = MaterialsEffectIDs[effectIndex];
                        int imageIndex = EffectsIDs.IndexOf(effectID);
                        if (imageIndex > -1)
                        {
                            string imageID = EffectsImageIDs[imageIndex];
                            int fnameIndex = ImagesIDs.IndexOf(imageID);
                            if (fnameIndex > -1)
                            {
                                string[] possible_extenstions = new string[] { string.Empty, ".png", ".jpg", ".jpeg", ".dds" };
                                for (int pe = 0; pe < possible_extenstions.Length; pe++)
                                {
                                    string fname = ImagesFilenames[fnameIndex];
                                    if (pe > 0)
                                    {
                                        fname += possible_extenstions[pe];
                                    }
                                    int subDir = 0;

                                    string[] split = fname.Split(new char[] { '\\', '/' });
                                    if (split.Length > 0 && split[0].Length > 0)
                                    {
                                        int countPoint = 0;
                                        for (int sp = 0; sp < split[0].Length; sp++)
                                        {
                                            if (split[0][sp] == '.')
                                            {
                                                countPoint++;
                                            }
                                        }
                                        if (countPoint == split[0].Length)
                                        {
                                            while (fname[0] == '.')
                                            {
                                                fname = fname.Remove(0, 1);
                                                subDir++;
                                            }
                                            fname = fname.Remove(0, 1);
                                            string dir = this.Directory;

                                            while (subDir > 0)
                                            {
                                                for (int sp = dir.Length - 1; sp > 0; sp--)
                                                {
                                                    if (dir[sp] == '\\')
                                                    {
                                                        dir = dir.Substring(0, sp);
                                                        break;
                                                    }
                                                }
                                                subDir--;
                                            }
                                            if (File.Exists(dir + @"\" + fname))
                                            {
                                                fname = dir + @"\" + fname;
                                            }
                                            else if (File.Exists(this.Directory + @"\" + fname))
                                            {
                                                fname = this.Directory + @"\" + fname;
                                            }
                                        }
                                    }


                                    Uri uri;

                                    if (Uri.TryCreate(fname, UriKind.Absolute, out uri))
                                    {
                                        fname = uri.AbsolutePath;
                                    }

                                    if (!File.Exists(fname))
                                    {
                                        int fileslashIndex = fname.IndexOf("file://");
                                        if (fileslashIndex > -1)
                                        {
                                            fname = fname.Remove(fileslashIndex, 7);
                                        }
                                        fname = fname.Replace("/", "\\");
                                        if (!File.Exists(fname))
                                        {
                                            if (!fname.Contains(":\\") && File.Exists(this.Directory + @"\" + fname))
                                            {
                                                fname = this.Directory + @"\" + fname;
                                            }
                                            else
                                            {
                                                if (File.Exists(this.Directory + @"\" + Path.GetFileName(fname)))
                                                {
                                                    fname = this.Directory + @"\" + Path.GetFileName(fname);
                                                }
                                            }
                                        }

                                    }
                                    if (File.Exists(fname))
                                    {


                                        textures[i] = GraphicFunctions.LoadTexture((System.Drawing.Bitmap)System.Drawing.Image.FromFile(fname));
                                        texturesAlpha[i] = GraphicFunctions.LastTextureAlpha;

                                        int ind = fname.IndexOf(this.Directory + @"\");
                                        if (ind > -1)
                                        {
                                            fname = fname.Remove(ind, this.Directory.Length + 1);
                                        }
                                        fname = fname.Replace("%20", " ");
                                        ImagesFilenames[fnameIndex] = fname;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                int indexof_cont = -1;

                for (int pcg = 0; pcg < this.PerControllerGeometry.Count; pcg++)
                {
                    if (this.PerControllerGeometry[pcg] == this.GeometryIDs[i])
                    {
                        indexof_cont = pcg;
                        break;
                    }
                }


                bool hasColorIndices = this.GeometryDataColors_i[i].Count > 0;
                bool hasNormalIndices = this.GeometryDataNormals_i[i].Count > 0;

                for (int j = 0; j < this.GeometryDataVertex_i[i].Count;j++)
                {
                    int vertexIndex = this.GeometryDataVertex_i[i][j];

                    int texCoordIndex = -1;
                    if (j < this.GeometryDataTexcoordinates_i[i].Count)
                        texCoordIndex = this.GeometryDataTexcoordinates_i[i][j];
                    
                    if (texCoordIndex > -1)
                    {
                        textureCoordinates[vCountPass] = this.GeometryDataTexcoordinates[i][texCoordIndex];
                    }

                    this.colorsNew[vCountPass] = false;
                    if (hasColorIndices)
                    {
                        int colorIndex = this.GeometryDataColors_i[i][j];
                        Color4 col = this.GeometryDataColors[i][colorIndex];
                        if (col.ToArgb() != lastColor.ToArgb())
                        {
                            this.colorsNew[vCountPass] = true;
                            this.colors[cCountPass] = col;
                            cCountPass++;
                            lastColor = col;
                        }
                    }
                    this.normalsNew[vCountPass] = false;
                    if (hasNormalIndices)
                    {
                        int normalIndex = this.GeometryDataNormals_i[i][j];
                        Vector3 normal = this.GeometryDataNormals[i][normalIndex];
                        if (Vector3.Distance(normal, lastNormal) > 0)
                        {
                            this.normalsNew[vCountPass] = true;
                            this.normals[nCountPass] = normal;
                            nCountPass++;
                            lastNormal = normal;
                        }
                    }

                    byte infCount = (byte)this.ControllerDataJoints_i[indexof_cont][vertexIndex].Count;
                    byte realInfCount = 0;

                    for (int k=0;k< infCount; k++)
                    {
                        int weightIndex = this.ControllerDataWeights_i[indexof_cont][vertexIndex][k];
                        float weight = this.ControllerDataWeights[indexof_cont][weightIndex];
                        if (weight>0f)
                        {
                            realInfCount++;
                        }
                    }

                    Vector4 Vertex = Vector4.Zero;
                    Vertex.X = this.GeometryDataVertex[i][vertexIndex].X;
                    Vertex.Y = this.GeometryDataVertex[i][vertexIndex].Y;
                    Vertex.Z = this.GeometryDataVertex[i][vertexIndex].Z;
                    Vertex.W = 1f;

                    for (int k = 0; k < infCount; k++)
                    {
                        int weightIndex = this.ControllerDataWeights_i[indexof_cont][vertexIndex][k];
                        float weight = this.ControllerDataWeights[indexof_cont][weightIndex];
                        if (weight>0f)
                        {
                            int jointIndex = this.ControllerDataJoints_i[indexof_cont][vertexIndex][k];
                            int matrixIndex = this.ControllerDataMatrices_i[indexof_cont][vertexIndex][k];
                            
                            this.influences[rvCountPass] = this.ControllerDataJoints_indices[indexof_cont][jointIndex];
                            
                            Vector4 reverseVertex = Vector4.Transform(Vertex* weight, Matrix4.Invert(this.Skeleton.Joints[this.influences[rvCountPass]].TransformModel));


                            this.weightedVertices[rvCountPass] = new WeightedVertex(reverseVertex, k == infCount-1);

                            rvCountPass++;
                        }
                    }
                    
                    vCountPass ++;
                }
            }
            computingComplexity = this.Skeleton.Joints.Length * this.weightedVertices.Length / 100000;
        }
        

        readonly List<string> ImagesIDs;
        readonly List<string> ImagesFilenames;
        readonly List<string> PerGeometryMaterials;
        List<string> MaterialsIDs;
        readonly List<string> MaterialsEffectIDs; /* Data is corresponding effect ID (an URL, with #) */
        readonly List<string> EffectsIDs;
        readonly List<string> EffectsImageIDs; /* Data is corresponding image ID */

        public readonly List<string> GeometryIDs;
        public readonly List<Vector3[]> GeometryDataVertex;
        public readonly List<Vector2[]> GeometryDataTexcoordinates;
        public readonly List<Vector3[]> GeometryDataNormals;
        public readonly List<Color4[]> GeometryDataColors;
        public readonly List<List<int>> GeometryDataVertex_i;
        public readonly List<List<int>> GeometryDataTexcoordinates_i;
        public readonly List<List<int>> GeometryDataNormals_i;
        public readonly List<List<int>> GeometryDataColors_i;

        readonly List<Matrix4 > ShapeMatrices;
        public readonly List<string> ControllersIDs;
        public readonly List<string> PerControllerGeometry;
        public readonly List<string[]> ControllerDataJoints;
        public readonly List<int[]> ControllerDataJoints_indices;
        public readonly List<Matrix4 []> ControllerDataMatrices;
        public readonly List<float[]> ControllerDataWeights;
        public readonly List<List<List<int>>> ControllerDataJoints_i;
        public readonly List<List<List<int>>> ControllerDataMatrices_i;
        public readonly List<List<List<int>>> ControllerDataWeights_i;
        readonly List<string> VisualScenesIDs;
        readonly List<List<string>> JointsIDs;
        readonly List<List<Matrix4 >> JointsMatrices;
        readonly List<List<string>> SurfacesIDs;
        readonly List<List<string>> SurfacesMaterialsID;

        public void GiveUntitledName()
        {
            this.Name = "Untitled";
            this.Directory = System.IO.Directory.GetCurrentDirectory();
        }

        public Matrix4 [] ParseMatrices(string content, int count)
        {
            Matrix4 [] output = new Matrix4 [count];
            char separator = ' ';

            for (int j = 2; j < content.Length && j < 20; j++)
            {
                if (content[j] == 9 ||
                    content[j] == 32 ||
                    content[j] == 160)
                {
                    separator = content[j];
                    break;
                }
            }

            string[] split = content.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
            float currVal = 0;
            int valCount = 0;
            int valIndex = 0;

            for (int j = 0; j < split.Length; j++)
            {
                if (Single.TryParse(split[j], out currVal))
                {
                    if (valCount % 16 == 0)
                        output[valIndex].M11 = currVal;
                    if (valCount % 16 == 1)
                        output[valIndex].M21 = currVal;
                    if (valCount % 16 == 2)
                        output[valIndex].M31 = currVal;
                    if (valCount % 16 == 3)
                        output[valIndex].M41 = currVal;

                    if (valCount % 16 == 4)
                        output[valIndex].M12 = currVal;
                    if (valCount % 16 == 5)
                        output[valIndex].M22 = currVal;
                    if (valCount % 16 == 6)
                        output[valIndex].M32 = currVal;
                    if (valCount % 16 == 7)
                        output[valIndex].M42 = currVal;

                    if (valCount % 16 == 8)
                        output[valIndex].M13 = currVal;
                    if (valCount % 16 == 9)
                        output[valIndex].M23 = currVal;
                    if (valCount % 16 == 10)
                        output[valIndex].M33 = currVal;
                    if (valCount % 16 == 11)
                        output[valIndex].M43 = currVal;

                    if (valCount % 16 == 12)
                        output[valIndex].M14 = currVal;
                    if (valCount % 16 == 13)
                        output[valIndex].M24 = currVal;
                    if (valCount % 16 == 14)
                        output[valIndex].M34 = currVal;
                    if (valCount % 16 == 15)
                    {
                        output[valIndex].M44 = currVal;


                        valIndex++;
                    }
                    valCount++;
                }
            }
            return output;
        }


        public static string ToString(Matrix4  m)
        {
            string s = "";
            s += m.M11.ToString("0.000000") + " " + m.M21.ToString("0.000000") + " " + m.M31.ToString("0.000000") + " " + m.M41.ToString("0.000000") + "\r\n";
            s += m.M12.ToString("0.000000") + " " + m.M22.ToString("0.000000") + " " + m.M32.ToString("0.000000") + " " + m.M42.ToString("0.000000") + "\r\n";
            s += m.M13.ToString("0.000000") + " " + m.M23.ToString("0.000000") + " " + m.M33.ToString("0.000000") + " " + m.M43.ToString("0.000000") + "\r\n";
            s += m.M14.ToString("0.000000") + " " + m.M24.ToString("0.000000") + " " + m.M34.ToString("0.000000") + " " + m.M44.ToString("0.000000") + "\r\n";
            return s;
        }
        public static string ToStringAccurate(Matrix4  m)
        {
            string s = "";
            s += ((Decimal)m.M11) + " " + ((Decimal)m.M21) + " " + ((Decimal)m.M31) + " " + ((Decimal)m.M41) + "\r\n";
            s += ((Decimal)m.M12) + " " + ((Decimal)m.M22) + " " + ((Decimal)m.M32) + " " + ((Decimal)m.M42) + "\r\n";
            s += ((Decimal)m.M13) + " " + ((Decimal)m.M23) + " " + ((Decimal)m.M33) + " " + ((Decimal)m.M43) + "\r\n";
            s += ((Decimal)m.M14) + " " + ((Decimal)m.M24) + " " + ((Decimal)m.M34) + " " + ((Decimal)m.M44) + "\r\n";
            return s;
        }

    }
}
