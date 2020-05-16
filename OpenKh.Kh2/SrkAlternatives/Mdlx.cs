#define read_mdlx_from_ram_feature
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenKh.Common;
using OpenTK.Graphics.ES11;
using OpenKh.Imaging;
using System.Drawing.Drawing2D;
using OpenKh.Kh2.Extensions;

namespace OpenKh.Kh2.SrkAlternatives
{
    public class Mdlx : Bar
    {
        byte[] ikData;

#if read_mdlx_from_ram_feature

        public const long RAM_PARTY_POINTER_PLAYER = 0x00341708;
        public const long RAM_PARTY_POINTER_PARTNER1 = 0x0034170C;
        public const long RAM_PARTY_POINTER_PARTNER2 = 0x00341710;
        public const long RAM_PARTY_POINTER_LOCK_ON_TARGET = 0x01C5FFF4;
        public const long RAM_MAP_POINTER = 0x00348D00;

#endif
        public Mdlx(Stream stream) : base(stream, 0, "", 0)
        {
            this.models = new List<Model>(0);
            this.GetData();
        }

        public void ExportDAE(string filename)
        {
            if (Path.GetExtension(filename) == ".dae")
                filename = filename.Remove(filename.Length - 4, 4);
            for (int h = 0; h < this.models.Count; h++)
            {
                XmlDocument output = new XmlDocument();
                output.PreserveWhitespace = false;
                output.Load("sample.dae");
                XmlNode image = output.SelectSingleNode("COLLADA/library_images/image"); image.ParentNode.RemoveChild(image);

                XmlNode effect = output.SelectSingleNode("//effect"); effect.ParentNode.RemoveChild(effect);
                XmlNode material = output.SelectSingleNode("//material"); material.ParentNode.RemoveChild(material);


                XmlNode geometry = output.SelectSingleNode("COLLADA/library_geometries/geometry"); geometry.ParentNode.RemoveChild(geometry);
                XmlNode controller = output.SelectSingleNode("COLLADA/library_controllers/controller"); controller.ParentNode.RemoveChild(controller);
                XmlNode polySurface_controller = output.SelectSingleNode("//visual_scene/node[instance_controller]"); polySurface_controller.ParentNode.RemoveChild(polySurface_controller);
                XmlNode polySurface_geometry = output.SelectSingleNode("//visual_scene/node[instance_geometry]"); polySurface_geometry.ParentNode.RemoveChild(polySurface_geometry);

                XmlNode scene = output.SelectSingleNode("//visual_scene");
                XmlNode instance_scene = output.SelectSingleNode("//scene/instance_visual_scene"); instance_scene.ParentNode.RemoveChild(instance_scene);

                XmlNode joint = output.SelectSingleNode("//node[@name='joint0']");
                scene.RemoveChild(scene.ChildNodes[0]);
                scene.RemoveChild(scene.ChildNodes[0]);
                scene.RemoveChild(scene.ChildNodes[0]);
                scene.ParentNode.RemoveChild(scene);
                int total_image_files = 0;

                XmlNode scene_new = scene.Clone();
                scene_new.Attributes["id"].Value = this.models[h].Name;
                scene_new.Attributes["name"].Value = this.models[h].Name;

                string inside_name = this.models[h].Name;
                if (h > 1)
                    inside_name += (h-2).ToString();

                if (!Directory.Exists(Path.GetDirectoryName(filename) + @"\" + inside_name + @"_textures"))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename) + @"\" + inside_name + @"_textures");

                bool mapv_type = this.models[h].TexturekhiiMapv != null;
                int textureCount = mapv_type ? this.models[h].TexturekhiiMapv.tex.pics.Length : this.models[h].Texture.Images.Count;
                for (int j = 0; j < textureCount; j++)
                {
                    XmlNode image_new = image.Clone();
                    image_new.Attributes["id"].Value = "file" + (total_image_files+j) + "-image";
                    image_new.Attributes["name"].Value = "file" + (total_image_files + j);
                    image_new.FirstChild.InnerText = inside_name + "_textures/texture" + j.ToString("d3")+".png";
                    output.SelectSingleNode("//library_images").AppendChild(image_new);

                    XmlNode material_new = material.Clone();
                    material_new.Attributes["id"].Value = "lambert" + (total_image_files + j);
                    material_new.Attributes["name"].Value = "lambert" + (total_image_files + j);
                    material_new.ChildNodes[0].Attributes["url"].Value = "#lambert" + (total_image_files + j)+"-fx";
                    output.SelectSingleNode("//library_materials").AppendChild(material_new);


                    XmlNode effect_new = effect.Clone();
                    effect_new.Attributes["id"].Value = "lambert" + (total_image_files + j)+"-fx";
                    effect_new.Attributes["name"].Value = "lambert" + (total_image_files + j);
                    var texture_node = effect_new.SelectSingleNode("descendant::texture");
                    texture_node.Attributes["texture"].Value = "file" + (total_image_files + j) + "-image";
                    output.SelectSingleNode("//library_effects").AppendChild(effect_new);
                    if (mapv_type)
                    this.models[h].TexturekhiiMapv.tex.pics[j].Clone(new Rectangle(0, 0, this.models[h].TexturekhiiMapv.tex.pics[j].Width, this.models[h].TexturekhiiMapv.tex.pics[j].Height), this.models[h].TexturekhiiMapv.tex.pics[j].PixelFormat).Save(Path.GetDirectoryName(filename) + @"\" + inside_name + @"_textures\texture" + j.ToString("d3") + ".png");
                    else
                        this.models[h].Texture.Images[j].SaveImage(Path.GetDirectoryName(filename)+@"\"+ inside_name + @"_textures\texture" + j.ToString("d3") + ".png");
                }
                if (this.models[h].Skeleton != null)
                for (int k = 0; k < this.models[h].Skeleton.Bones.Length; k++)
                {
                    XmlNode joint_new = joint.Clone();
                    joint_new.Attributes["name"].Value = "joint" + k;
                    joint_new.Attributes["id"].Value = "joint" + k;
                    joint_new.Attributes["sid"].Value = "joint" + k;
                    XmlNode matrix_node = joint_new.ChildNodes[0];
                    matrix_node.InnerText = "";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M11.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M21.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M31.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M41.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M12.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M22.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M32.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M42.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M13.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M23.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M33.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M43.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M14.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M24.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M34.ToString("0.000000") + " ";
                    matrix_node.InnerText += this.models[h].Skeleton.Bones[k].TransformLocal.M44.ToString("0.000000");
                    if (this.models[h].Skeleton.Bones[k].Parent < 0)
                    {
                        scene_new.AppendChild(joint_new);
                    }
                    else
                    {
                        scene_new.SelectSingleNode("//node[@type='JOINT' and @name='joint" + this.models[h].Skeleton.Bones[k].Parent + "']").AppendChild(joint_new);
                    }
                }
                for (int j = 0; j < this.models[h].Meshes.Count + this.models[h].ShadowMeshes.Count; j++)
                {

                    string polySurfaceName = this.models[h].Name+"_";
                    bool shadow = j > this.models[h].Meshes.Count - 1;
                    Mesh mesh = shadow ? this.models[h].ShadowMeshes[j- this.models[h].Meshes.Count] : this.models[h].Meshes[j];
                    if (shadow)
                    {
                        polySurfaceName += "shadow_";
                        output.SelectSingleNode("//library_visual_scenes").AppendChild(scene_new);

                        XmlNode instance_scene_new_ = instance_scene.Clone();
                        instance_scene_new_.Attributes["url"].Value = "#" + scene_new.Attributes["id"].Value;
                        output.SelectSingleNode("//scene").AppendChild(instance_scene_new_);

                        scene_new = scene.Clone();
                        scene_new.Attributes["id"].Value = this.models[h].Name+"_shadow";
                        scene_new.Attributes["name"].Value = this.models[h].Name + "_shadow";
                    }
                    polySurfaceName += "polySurface";

                    XmlNode geometry_new = geometry.Clone();
                    geometry_new.SelectSingleNode("//triangles").Attributes["material"].Value = "lambert"+(total_image_files + mesh.TextureIndex).ToString();
                    var nodes = geometry_new.SelectNodes("//*");

                    XmlNode polysurface_new = (mesh.influences.Count > 0 ? polySurface_controller: polySurface_geometry).Clone();
                    var instance_material = polysurface_new.SelectSingleNode("descendant::instance_material");
                    instance_material.Attributes["symbol"].Value = "lambert" + (total_image_files + mesh.TextureIndex).ToString();
                    instance_material.Attributes["target"].Value = "#lambert" + (total_image_files + mesh.TextureIndex).ToString();
                    for (int k = -3; k < nodes.Count; k++)
                    {
                        XmlNode node = null;
                        if (k < -2)
                            node = polysurface_new;
                        else if (k < -1)
                            node = polysurface_new.ChildNodes[0];
                        else if (k <0)
                            node = geometry_new;
                        else
                            node = nodes[k];

                        if (node.InnerText.Contains("polySurface0"))
                            node.InnerText = node.InnerText.Replace("polySurface0", polySurfaceName + j);

                        for (int l = 0; l < node.Attributes.Count; l++)
                            if (node.Attributes[l].Value.Contains("polySurface0"))
                                node.Attributes[l].Value = node.Attributes[l].Value.Replace("polySurface0", polySurfaceName + j);
                    }
                    scene_new.AppendChild(polysurface_new);

                    XmlNode array_node = geometry_new.SelectSingleNode("//float_array[contains(@id,'POSITION')]");
                    array_node.Attributes["count"].Value = (mesh.reverseVertices.Count*3).ToString();
                    array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = (mesh.reverseVertices.Count).ToString();
                    array_node.InnerText = "\n";
                    for (int k = 0; k < mesh.reverseVertices.Count; k++)
                    {
                        Vector3 v = Vector3.Zero;
                        for (int l = 0; l < mesh.reverseVertices[k].Length; l++)
                        {
                            Vector4 v4 = mesh.reverseVertices[k][l];
                            if (this.models[h].Skeleton != null)
                            {
                                v4 = Vector4.Transform(v4, this.models[h].Skeleton.Bones[mesh.influences[k][l]].TransformModel);
                            }
                            v.X += v4.X;
                            v.Y += v4.Y;
                            v.Z += v4.Z;
                        }
                        array_node.InnerText += v.X + " " + v.Y + " " + v.Z + "\n";
                    }
                    bool has_uv = mesh.textureCoordinates.Count > 0;
                    if (has_uv)
                    {
                        array_node = geometry_new.SelectSingleNode("//float_array[contains(@id,'UV')]");
                        array_node.Attributes["count"].Value = (mesh.textureCoordinates.Count * 2).ToString();
                        array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = (mesh.textureCoordinates.Count).ToString();
                        array_node.InnerText = "\n";
                        for (int k = 0; k < mesh.textureCoordinates.Count; k++)
                        {
                            array_node.InnerText += mesh.textureCoordinates[k].X + " " + mesh.textureCoordinates[k].Y + "\n";
                        }
                    }
                    else
                    {
                        var removeTexcoordinatesNode = geometry_new.SelectSingleNode("//source[contains(@id,'UV')]");
                        removeTexcoordinatesNode.ParentNode.RemoveChild(removeTexcoordinatesNode);
                        removeTexcoordinatesNode = geometry_new.SelectSingleNode("//triangles/input[contains(@source,'UV')]");
                        removeTexcoordinatesNode.ParentNode.RemoveChild(removeTexcoordinatesNode);
                        geometry_new.SelectSingleNode("//triangles/input[contains(@source,'COLOR')]").Attributes["offset"].Value = "1";
                    }
                    bool has_color = false;
                    for (int k = 0; k < mesh.colors.Count; k++)
                    {
                        if (mesh.colors[k].R < 1 ||
                            mesh.colors[k].G < 1 ||
                            mesh.colors[k].B < 1 ||
                            mesh.colors[k].A < 1)
                        {
                            has_color = true;
                        }
                    }
                    if (has_color)
                    {
                        array_node = geometry_new.SelectSingleNode("//float_array[contains(@id,'COLOR')]");
                        array_node.Attributes["count"].Value = (mesh.colors.Count * 4).ToString();
                        array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = (mesh.colors.Count).ToString();
                        array_node.InnerText = "\n";
                        for (int k = 0; k < mesh.colors.Count; k++)
                        {
                            array_node.InnerText += mesh.colors[k].R + " " + mesh.colors[k].G + " "+ mesh.colors[k].B+" "+ mesh.colors[k].A + "\n";
                        }
                    }
                    else
                    {
                        var removeColorNode = geometry_new.SelectSingleNode("//source[contains(@id,'COLOR')]");
                        removeColorNode.ParentNode.RemoveChild(removeColorNode);
                        removeColorNode = geometry_new.SelectSingleNode("//triangles/input[contains(@source,'COLOR')]");
                        removeColorNode.ParentNode.RemoveChild(removeColorNode);
                    }

                    int triangle_count = 0;

                    array_node = geometry_new.SelectSingleNode("//triangles/p");
                    array_node.InnerText = "\n";
                    List<int> stock_vertex_index = new List<int>(0);
                    List<int> stock_texture_index = new List<int>(0);
                    List<int> stock_color_index = new List<int>(0);

                    for (int k = 0; k < mesh.triangleFlags.Count; k++)
                    {
                        int current_vertex_index = mesh.vertexIndices[k];
                        int texture_index = k;
                        int current_color_index = has_color ? mesh.colorIndices[k] : -1;

                        if (stock_vertex_index.Count == 4)
                            stock_vertex_index.RemoveAt(0);

                        if (stock_texture_index.Count == 4)
                            stock_texture_index.RemoveAt(0);

                        if (stock_color_index.Count == 4)
                            stock_color_index.RemoveAt(0);

                        stock_vertex_index.Add(current_vertex_index);
                        stock_texture_index.Add(texture_index);
                        stock_color_index.Add(current_color_index);

                        if (mesh.triangleFlags[k] == 0x10)
                            continue;

                        int[] flags = mesh.triangleFlags[k] == 0 ? new int[] {0x20,0x30} : new int[] { mesh.triangleFlags[k] };

                        for (int l = 0; l < flags.Length; l++)
                        {
                            int ind0 = stock_vertex_index.Count - 3;
                            int ind1 = stock_vertex_index.Count - 2;
                            int ind2 = stock_vertex_index.Count - 1;
                            if (flags[l] == 0x30)
                            {
                                ind0 = stock_vertex_index.Count - 1;
                                ind1 = stock_vertex_index.Count - 2;
                                ind2 = stock_vertex_index.Count - 3;
                            }
                            array_node.InnerText += stock_vertex_index[ind0] + " ";
                            if (has_uv)
                                array_node.InnerText += stock_texture_index[ind0] + " ";
                            if (has_color) array_node.InnerText += stock_color_index[ind0] + " ";

                            array_node.InnerText += stock_vertex_index[ind1] + " ";
                            if (has_uv)
                            array_node.InnerText += stock_texture_index[ind1] + " ";
                            if (has_color) array_node.InnerText += stock_color_index[ind1] + " ";

                            array_node.InnerText += stock_vertex_index[ind2] + " ";
                            if (has_uv)
                                array_node.InnerText += stock_texture_index[ind2] + " ";
                            if (has_color) array_node.InnerText += stock_color_index[ind2] + " ";
                            triangle_count++;
                        }

                    }
                    var triangles_node = geometry_new.SelectSingleNode("//triangles");
                    triangles_node.Attributes["count"].Value = triangle_count.ToString();
                    output.SelectSingleNode("//library_geometries").AppendChild(geometry_new);


                    if (mesh.influences.Count>0)
                    {
                        XmlNode controller_new = controller.Clone();
                        nodes = controller_new.SelectNodes("//*");

                        for (int k = -1; k < nodes.Count; k++)
                        {
                            XmlNode node = null;
                            if (k < 0)
                                node = controller_new;
                            else
                                node = nodes[k];

                            if (node.InnerText.Contains("polySurface0"))
                                node.InnerText = node.InnerText.Replace("polySurface0", polySurfaceName + j);

                            for (int l = 0; l < node.Attributes.Count; l++)
                                if (node.Attributes[l].Value.Contains("polySurface0"))
                                    node.Attributes[l].Value = node.Attributes[l].Value.Replace("polySurface0", polySurfaceName + j);
                        }

                        array_node = controller_new.SelectSingleNode("//Name_array");
                        array_node.Attributes["count"].Value = this.models[h].Skeleton.Bones.Length.ToString();
                        array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = this.models[h].Skeleton.Bones.Length.ToString();
                        array_node.InnerText = "\n";
                        for (int k = 0; k < this.models[h].Skeleton.Bones.Length; k++)
                        {
                            array_node.InnerText += "joint" + k + " ";
                        }
                        array_node = controller_new.SelectSingleNode("//float_array[contains(@id,'Matrices')]");
                        array_node.Attributes["count"].Value = (this.models[h].Skeleton.Bones.Length * 16).ToString();
                        array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = this.models[h].Skeleton.Bones.Length.ToString();
                        array_node.InnerText = "\n";
                        for (int k = 0; k < this.models[h].Skeleton.Bones.Length; k++)
                        {
                            Matrix4 m = Matrix4.Invert(this.models[h].Skeleton.Bones[k].TransformModel);
                            array_node.InnerText += m.M11.ToString("0.000000") + " ";
                            array_node.InnerText += m.M21.ToString("0.000000") + " ";
                            array_node.InnerText += m.M31.ToString("0.000000") + " ";
                            array_node.InnerText += m.M41.ToString("0.000000") + " ";
                            array_node.InnerText += m.M12.ToString("0.000000") + " ";
                            array_node.InnerText += m.M22.ToString("0.000000") + " ";
                            array_node.InnerText += m.M32.ToString("0.000000") + " ";
                            array_node.InnerText += m.M42.ToString("0.000000") + " ";
                            array_node.InnerText += m.M13.ToString("0.000000") + " ";
                            array_node.InnerText += m.M23.ToString("0.000000") + " ";
                            array_node.InnerText += m.M33.ToString("0.000000") + " ";
                            array_node.InnerText += m.M43.ToString("0.000000") + " ";
                            array_node.InnerText += m.M14.ToString("0.000000") + " ";
                            array_node.InnerText += m.M24.ToString("0.000000") + " ";
                            array_node.InnerText += m.M34.ToString("0.000000") + " ";
                            array_node.InnerText += m.M44.ToString("0.000000") + "\n";
                        }


                        array_node = controller_new.SelectSingleNode("//float_array[contains(@id,'Weights')]");
                        int reverseCount = 0;
                        array_node.InnerText = "\n";

                        for (int k=0;k<mesh.reverseVertices.Count;k++)
                        {
                            for (int l = 0; l < mesh.reverseVertices[k].Length; l++)
                            {
                                array_node.InnerText += mesh.reverseVertices[k][l].W.ToString("0.000000")+" ";
                                reverseCount++;
                            }
                        }
                        array_node.Attributes["count"].Value = reverseCount.ToString();
                        array_node.ParentNode.SelectSingleNode("descendant::accessor").Attributes["count"].Value = reverseCount.ToString();

                        var vertex_weights = controller_new.SelectSingleNode("//vertex_weights");
                        vertex_weights.Attributes["count"].Value = mesh.reverseVertices.Count.ToString();

                        var array_node_2 = vertex_weights.SelectSingleNode("vcount");
                        array_node = vertex_weights.SelectSingleNode("v");

                        array_node.InnerText = "\n";
                        array_node_2.InnerText = "\n";
                        reverseCount = 0;
                        for (int k = 0; k < mesh.influences.Count; k++)
                        {
                            array_node_2.InnerText += mesh.influences[k].Length+" ";
                            for (int l=0;l< mesh.influences[k].Length;l++)
                            {
                                array_node.InnerText += mesh.influences[k][l] + " "+ reverseCount + " ";
                                reverseCount++;
                            }
                        }
                        output.SelectSingleNode("//library_controllers").AppendChild(controller_new);
                    }
                }


                output.SelectSingleNode("//library_visual_scenes").AppendChild(scene_new);

                XmlNode instance_scene_new = instance_scene.Clone();
                instance_scene_new.Attributes["url"].Value = "#"+ scene_new.Attributes["id"].Value;
                output.SelectSingleNode("//scene").AppendChild(instance_scene_new);

                total_image_files += textureCount;


                var shadow_geometries = output.SelectNodes("//library_geometries/geometry[contains(@id,'shadow')]");
                var shadow_controllers = output.SelectNodes("//library_controllers/controller[contains(@id,'shadow')]");
                var shadow_scenes = output.SelectNodes("//library_visual_scenes/visual_scene[contains(@id,'shadow')]");
                var shadow_scenes_instance = output.SelectNodes("//scene/instance_visual_scene[contains(@url,'shadow')]");
                for (int i = 0; i < shadow_geometries.Count; i++) shadow_geometries[i].ParentNode.RemoveChild(shadow_geometries[i]);
                for (int i = 0; i < shadow_controllers.Count; i++) shadow_controllers[i].ParentNode.RemoveChild(shadow_controllers[i]);
                for (int i = 0; i < shadow_scenes.Count; i++) shadow_scenes[i].ParentNode.RemoveChild(shadow_scenes[i]);
                for (int i = 0; i < shadow_scenes_instance.Count; i++) shadow_scenes_instance[i].ParentNode.RemoveChild(shadow_scenes_instance[i]);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.GetEncoding("ISO-8859-1");
                settings.Indent = true;
                settings.IndentChars = "	";
                XmlWriter writer = XmlWriter.Create(filename+"["+ inside_name + "].dae", settings);

                output.Save(writer);
                if (shadow_scenes_instance.Count > 0)
                {
                    var library_geometries_ = output.SelectSingleNode("//library_geometries");
                    var library_controllers_ = output.SelectSingleNode("//library_controllers");
                    var library_visual_scenes_ = output.SelectSingleNode("//library_visual_scenes");
                    XmlNodeList joints = output.SelectNodes("//library_visual_scenes/visual_scene/node[@type='JOINT']");
                    if (joints != null)
                        for (int i = 0; i < joints.Count; i++) shadow_scenes[0].AppendChild(joints[i]);

                    var scene_ = output.SelectSingleNode("//scene");
                    while (library_geometries_.ChildNodes.Count > 0) library_geometries_.RemoveChild(library_geometries_.ChildNodes[0]);
                    while (library_controllers_.ChildNodes.Count > 0) library_controllers_.RemoveChild(library_controllers_.ChildNodes[0]);
                    while (library_visual_scenes_.ChildNodes.Count > 0) library_visual_scenes_.RemoveChild(library_visual_scenes_.ChildNodes[0]);
                    while (scene_.ChildNodes.Count > 0) scene_.RemoveChild(scene_.ChildNodes[0]);

                    for (int i = 0; i < shadow_geometries.Count; i++) library_geometries_.AppendChild(shadow_geometries[i]);
                    for (int i = 0; i < shadow_controllers.Count; i++) library_controllers_.AppendChild(shadow_controllers[i]);
                    for (int i = 0; i < shadow_scenes.Count; i++) library_visual_scenes_.AppendChild(shadow_scenes[i]);
                    for (int i = 0; i < shadow_scenes_instance.Count; i++) scene_.AppendChild(shadow_scenes_instance[i]);

                    writer.Close();
                    writer = XmlWriter.Create(filename + "[" + inside_name + "]-shadow.dae", settings);

                    output.Save(writer);
                }
            }

        }

        List<Model> models;
        void GetData()
        {
            for (int i = 0; i < this.Files.Count; i++)
            {
                if (this.Files[i].Length > 0 &&
                    this.Files[i].Type == 4)
                {
                    Model model = new Model(ref this.Files[i].Data);
                    model.Name = this.Files[i].Name;

                    if (i + 1 < this.Files.Count && this.Files[i + 1].Type == 7)
                    {
                        /*if (this.Files[i].Name.Contains("MAP") ||
                        this.Files[i].Name.Contains("SK"))*/
                            model.TexturekhiiMapv = new KenunoTim(this.Files[i + 1].Data);
                        /*else
                            model.Texture = new ModelTexture(new MemoryStream(this.Files[i + 1].Data));*/
                    }
                    
                    this.models.Add(model);
                }
            }
        }


        public class Mesh
        {
            public int TextureIndex;
            public List<Color4> colors;
            public List<Vector2> textureCoordinates;
            public List<Vector4[]> reverseVertices;
            public List<int[]> influences;
            public List<int> triangleFlags;
            public List<int> vertexIndices;
            public List<int> colorIndices;

            public Mesh()
            {
                this.colors = new List<Color4>(0);
                this.colors.Add(Color.White);
                this.textureCoordinates = new List<Vector2>(0);
                this.reverseVertices = new List<Vector4[]>(0);
                this.influences = new List<int[]>(0);
                this.triangleFlags = new List<int>(0);
                this.vertexIndices = new List<int>(0);
                this.colorIndices = new List<int>(0);
            }
        }

        public class Model : BitConverter
        {
            static byte[] vu_memory = new byte[0x1030];
            public Skeleton Skeleton;
            public KenunoTim TexturekhiiMapv;
            public ModelTexture Texture;
            public string Name;

            public List<Mesh> Meshes;
            public List<Mesh> ShadowMeshes;

            public Model(ref byte[] data)
            {
                this.Data = data;
                this.Meshes = new List<Mesh>(0);
                this.ShadowMeshes = new List<Mesh>(0);
                int s = 0x90;

                while (true)
                {
                    int model_type = this.Int16(s + 0x00);
                    int shadow_offset = this.Int32(s + 0x0C);
                    short bonesCount = this.Int16(s + 0x10);
                    short texturesCount = this.Int16(s + 0x12);

                    if (model_type == 3)
                    {
                        this.Skeleton = new Skeleton(bonesCount);
                        int bonesOffset = s + this.Int16(s + 0x14);

                        for (int i = 0; i < bonesCount; i++)
                        {
                            int pos = bonesOffset + i * 0x40;

                            int index = this.Int16(pos + 0x00);
                            int parent_index = this.Int16(pos + 0x04);

                            Bone bone = new Bone();
                            bone.Parent = parent_index;
                            bone.ScaleX = this.Single(pos + 0x10);
                            bone.ScaleY = this.Single(pos + 0x14);
                            bone.ScaleZ = this.Single(pos + 0x18);

                            bone.RotateX = this.Single(pos + 0x20);
                            bone.RotateY = this.Single(pos + 0x24);
                            bone.RotateZ = this.Single(pos + 0x28);

                            bone.TranslateX = this.Single(pos + 0x30);
                            bone.TranslateY = this.Single(pos + 0x34);
                            bone.TranslateZ = this.Single(pos + 0x38);

                            this.Skeleton.Bones[index] = bone;
                        }
                        if (this.Skeleton.Bones != null && this.Skeleton.Bones.Length > 0)
                            this.Skeleton.ComputeMatrices(Matrix4.CreateScale(1f));

                        int ikOffset = s + this.Int16(s + 0x18);
                        //this.ikData = new byte[bonesOffset - ikOffset];
                        //Array.Copy(this.Data, ikOffset, this.ikData, 0, this.ikData.Length);

                    }

                    if (model_type == 2)
                    {
                        int polyCount = this.Int16(s + 0x10);
                        int dma_header = s + this.Int16(s + 0x18);

                        Mesh mesh = null;
                        for (int poly_index = 0; poly_index < polyCount; poly_index++)
                        {
                            int pos = s + 0x20 + poly_index * 0x10;
                            int newTextureIndex = this.Int16(pos + 0x04);
                            mesh = null;
                            for (int m=0;m<this.Meshes.Count;m++)
                            {
                                if (this.Meshes[m].TextureIndex == newTextureIndex)
                                    mesh = this.Meshes[m];
                            }
                            bool new_mesh = false;
                            if (mesh==null)
                            {
                                new_mesh = true;
                                mesh = new Mesh();
                                mesh.TextureIndex = newTextureIndex;
                            }

                            int vif_offset = s + this.Int32(pos);
                            while (true)
                            {
                                int vif_size = this.Int16(vif_offset) * 0x10;
                                byte[] vif_packet = new byte[vif_size + 0x10];
                                Array.Copy(this.Data, vif_offset+8, vif_packet, 0, vif_packet.Length);
                                unpack_vif(vif_packet);
                                int mati_cursor_void = 0;
                                decode_unpacked(ref mesh, ref mati_cursor_void);
                                vif_offset += vif_size;

                                while (this.Int32(vif_offset-4)!=0x17000000)
                                    vif_offset += 4;
                                while (vif_offset%16>0)
                                    vif_offset += 4;

                                if (this.Int32(vif_offset + 8) != 0x01000101)
                                    break;
                            }

                            if (new_mesh)
                                this.Meshes.Add(mesh);
                        }

                    }
                    else
                    {
                        int meshCount = this.Int16(s + 0x1C);
                        for (int mesh_index = 0; mesh_index < meshCount; mesh_index++)
                        {
                            int pos = s + 0x20 + mesh_index * 0x20;

                            Mesh mesh = new Mesh();
                            mesh.TextureIndex = this.Int16(pos + 0x04);

                            //int vu_memory_size = this.Int32(pos + 0x08) * 0x10;
                            int dma_offset = s + this.Int32(pos + 0x10);
                            int mat_indices_offset = s + this.Int32(pos + 0x14);
                            int dma_size = this.Int32(pos + 0x18) * 0x10;
                            int mati_cursor = mat_indices_offset + 4;

                            while (dma_size > 0) /* We stock the dma tags size to know when all of the tags have been read */
                            {

                                int vif_offset = s + this.Int32(dma_offset + 0x04);
                                int vif_size = this.Int16(dma_offset + 0x00) * 0x10;

                                unpack_vif(vif_offset, vif_size);

                                /* reading DMA tag unpacks to make sure everything works */
                                vif_offset = dma_offset + 0x18;
                                dma_size -= 0x18;
                                int vif_cursor = 0;
                                while (this.Int32(vif_offset + vif_cursor) == 0x01000101)
                                {
                                    /* cycle command */
                                    vif_cursor += 4;

                                    /* unpack command */
                                    int unpack_offset = this.Data[vif_offset + vif_cursor + 0x00] * 0x10;
                                    byte usn_command = this.Data[vif_offset + vif_cursor + 0x01];
                                    bool signed = (usn_command & 0x40) != 0x40; /* 0x40 = 01000000 'second bit equals 0' */
                                    int unpack_size = this.Data[vif_offset + vif_cursor + 0x02] * 0x10;
                                    byte unpack_command = this.Data[vif_offset + vif_cursor + 0x03];
                                    bool i = (unpack_command & 0x80) == 0x80;/* 0x80 = 10000000 'first bit equals 1' */
                                    bool masked = (unpack_command & 0x10) == 0x10;/* 0x10 = 00010000 'fourth bit equals 1' */
                                    byte vn_vl = (byte)(unpack_command % 0x10); /* xxxx0000 */
                                    int data_len_in_bits = 16; int vn_vl_index = vn_vl % 4;
                                    if (vn_vl_index < 15) data_len_in_bits = (int)Math.Pow(2, 5 - vn_vl_index);
                                    int data_dimension = 1 + vn_vl / 4;
                                    int data_len_in_bytes = data_len_in_bits / 8;

                                    for (int cursor = 0; cursor < unpack_size; cursor += 0x10)
                                    {
                                        int abs_pos_in_unpckd_mem = unpack_offset + cursor;
                                        while (global::System.BitConverter.ToInt32(vu_memory, abs_pos_in_unpckd_mem) != -1)
                                            abs_pos_in_unpckd_mem += 4;

                                        /* This data correspond to the 4x4 entries (M11,M12,...M43,M44) of the corresponding bone 
                                         * (Located either 4 bytes before the current cycle command, or in the memory region at mat_indices_offset,
                                         * right after the DMA tags. Direct Memory Access is used here to get computed matrices from the memory.)*/
                                        for (int d = 0; d < data_dimension; d++)
                                        {

                                        }
                                    }
                                    vif_cursor += 0x0C;
                                    dma_size -= 0x10;
                                }
                                dma_offset = vif_offset + vif_cursor + 0x08;
                                dma_size -= 0x08;

                                decode_unpacked(ref mesh, ref mati_cursor);
                            }
                            if (model_type == 4)
                                this.ShadowMeshes.Add(mesh);
                            else
                                this.Meshes.Add(mesh);
                        }
                    }
                    
                    
                    if (shadow_offset > 0)
                        s += shadow_offset;
                    else
                        break;
                }
                Array.Clear(vu_memory, 0, vu_memory.Length);
            }
            bool unpack_vif(int vif_offset, int vif_size)
            {
                for (int f = 0; f < vu_memory.Length; f++)
                    vu_memory[f] = 255; /* 255 will mean "the cursor didn't pass to write here" */

                int vif_cursor = 0;

                while (vif_cursor < vif_size)
                {
                    while (vif_offset + vif_cursor < this.Data.Length && 
                        this.Int32(vif_offset + vif_cursor) != 0x01000101)
                        vif_cursor += 4;
                    if (vif_offset + vif_cursor >= this.Data.Length)
                        return true;
                    if (vif_cursor >= vif_size)
                        break;
                    /* cycle command */
                    vif_cursor += 4;

                    /* unpack command */
                    int unpack_offset = this.Data[vif_offset + vif_cursor + 0x00] * 0x10;
                    byte usn_command = this.Data[vif_offset + vif_cursor + 0x01];
                    bool signed = (usn_command & 0x40) != 0x40; /* 0x40 = 01000000 'second bit equals 0' */
                    int unpack_size = this.Data[vif_offset + vif_cursor + 0x02] * 0x10;
                    byte unpack_command = this.Data[vif_offset + vif_cursor + 0x03];
                    bool i = (unpack_command & 0x80) == 0x80;/* 0x80 = 10000000 'first bit equals 1' */
                    bool masked = (unpack_command & 0x10) == 0x10;/* 0x10 = 00010000 'fourth bit equals 1' */
                    byte vn_vl = (byte)(unpack_command % 0x10); /* xxxx0000 */
                    int data_len_in_bits = 16; int vn_vl_index = vn_vl % 4;
                    if (vn_vl_index < 15) data_len_in_bits = (int)Math.Pow(2, 5 - vn_vl_index);
                    int data_dimension = 1 + vn_vl / 4;
                    int data_len_in_bytes = data_len_in_bits / 8;

                    vif_cursor += 4;
                    /* we're positionned at the data to unpack */

                    for (int cursor = 0; cursor < unpack_size; cursor += 0x10)
                    {
                        int abs_pos_in_unpckd_mem = unpack_offset + cursor;
                        while (global::System.BitConverter.ToInt32(vu_memory, abs_pos_in_unpckd_mem) != -1)
                            abs_pos_in_unpckd_mem += 4;


                        for (int d = 0; d < data_dimension; d++)
                        {
                            Array.Copy(new byte[4], 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                            Array.Copy(this.Data, vif_offset + vif_cursor, vu_memory, abs_pos_in_unpckd_mem + d * 4, data_len_in_bytes);

                            if (signed && data_dimension * data_len_in_bytes < 4)
                            {
                                if (data_len_in_bytes == 2)
                                {
                                    short val = global::System.BitConverter.ToInt16(vu_memory, abs_pos_in_unpckd_mem + d * 4);
                                    Array.Copy(global::System.BitConverter.GetBytes((int)val), 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                                }
                                else
                                if (data_len_in_bytes == 1)
                                {
                                    sbyte val = (sbyte)vu_memory[abs_pos_in_unpckd_mem + d * 4];
                                    Array.Copy(global::System.BitConverter.GetBytes((int)val), 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                                }
                            }
                            vif_cursor += data_len_in_bytes;
                        }
                    }
                    while (vif_cursor % 4 > 0)
                        vif_cursor++;
                }
                return false;
            }
            void unpack_vif(byte[] vif_packet)
            {
                for (int f = 0; f < vu_memory.Length; f++)
                    vu_memory[f] = 255; /* 255 will mean "the cursor didn't pass to write here" */


                for (int cursor=0;cursor< vif_packet.Length;cursor+=4)
                {
                    while (cursor < vif_packet.Length && global::System.BitConverter.ToInt32(vif_packet, cursor) != 0x01000101)
                        cursor += 4;
                    if (cursor + 8  >= vif_packet.Length)
                        break;
                    /* cycle command */
                    cursor += 4;

                    /* unpack command */
                    int unpack_offset = vif_packet[cursor + 0x00] * 0x10;
                    byte usn_command = vif_packet[cursor + 0x01];
                    bool signed = (usn_command & 0x40) != 0x40; /* 0x40 = 01000000 'second bit equals 0' */
                    int unpack_size = vif_packet[cursor + 0x02] * 0x10;
                    byte unpack_command = vif_packet[cursor + 0x03];
                    bool i = (unpack_command & 0x80) == 0x80;/* 0x80 = 10000000 'first bit equals 1' */
                    bool masked = (unpack_command & 0x10) == 0x10;/* 0x10 = 00010000 'fourth bit equals 1' */
                    byte vn_vl = (byte)(unpack_command % 0x10); /* xxxx0000 */
                    int data_len_in_bits = 16; int vn_vl_index = vn_vl % 4;
                    if (vn_vl_index < 15) data_len_in_bits = (int)Math.Pow(2, 5 - vn_vl_index);
                    int data_dimension = 1 + vn_vl / 4;
                    int data_len_in_bytes = data_len_in_bits / 8;

                    cursor += 4;
                    /* we're positionned at the data to unpack */


                    for (int unpack_cursor = 0; unpack_cursor < unpack_size; unpack_cursor += 0x10)
                    {
                        int abs_pos_in_unpckd_mem = unpack_offset + unpack_cursor;
                        while (global::System.BitConverter.ToInt32(vu_memory, abs_pos_in_unpckd_mem) != -1)
                            abs_pos_in_unpckd_mem += 4;


                        for (int d = 0; d < data_dimension; d++)
                        {
                            Array.Copy(new byte[4], 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                            Array.Copy(vif_packet, cursor, vu_memory, abs_pos_in_unpckd_mem + d * 4, data_len_in_bytes);

                            if (signed && data_dimension * data_len_in_bytes < 4)
                            {
                                if (data_len_in_bytes == 2)
                                {
                                    short val = global::System.BitConverter.ToInt16(vu_memory, abs_pos_in_unpckd_mem + d * 4);
                                    Array.Copy(global::System.BitConverter.GetBytes((int)val), 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                                }
                                else
                                if (data_len_in_bytes == 1)
                                {
                                    sbyte val = (sbyte)vu_memory[abs_pos_in_unpckd_mem + d * 4];
                                    Array.Copy(global::System.BitConverter.GetBytes((int)val), 0, vu_memory, abs_pos_in_unpckd_mem + d * 4, 4);
                                }
                            }
                            cursor += data_len_in_bytes;
                        }
                    }
                    while (cursor % 4 > 0)
                        cursor++;
                    cursor -= 4;
                }
            }

            void decode_unpacked(ref Mesh mesh, ref int mati_cursor)
            {
                List<int> all_influences = new List<int>(0);

                int vif_type = global::System.BitConverter.ToInt32(vu_memory, 0x00);

                int count_texcoo_ind_strips = global::System.BitConverter.ToInt32(vu_memory, 0x10);
                int offset_texcoo_ind_strips = global::System.BitConverter.ToInt32(vu_memory, 0x14) * 0x10;
                int offset_per_mat_verts = global::System.BitConverter.ToInt32(vu_memory, 0x18) * 0x10;
                int offset_matrices = global::System.BitConverter.ToInt32(vu_memory, 0x1C) * 0x10;

                int count_color = 0;
                int offset_color = 0;
                int count_influences = 0;
                int offset_influences = 0;

                int count_vertices = 0;
                int offset_vertices = 0;
                int count_per_mat_verts = 0;

                if (vif_type == 2)
                {
                    count_vertices = global::System.BitConverter.ToInt32(vu_memory, 0x20);
                    offset_vertices = global::System.BitConverter.ToInt32(vu_memory, 0x24) * 0x10;
                    count_per_mat_verts = global::System.BitConverter.ToInt32(vu_memory, 0x2C);
                }
                else
                {
                    count_color = global::System.BitConverter.ToInt32(vu_memory, 0x20);
                    offset_color = global::System.BitConverter.ToInt32(vu_memory, 0x24) * 0x10;
                    count_influences = global::System.BitConverter.ToInt32(vu_memory, 0x28);
                    offset_influences = global::System.BitConverter.ToInt32(vu_memory, 0x2C) * 0x10;

                    count_vertices = global::System.BitConverter.ToInt32(vu_memory, 0x30);
                    offset_vertices = global::System.BitConverter.ToInt32(vu_memory, 0x34) * 0x10;
                    count_per_mat_verts = global::System.BitConverter.ToInt32(vu_memory, 0x3C);
                }


                for (int i = 0; i < count_per_mat_verts; i++)
                {
                    int cnt = global::System.BitConverter.ToInt32(vu_memory, offset_per_mat_verts + i * 4);
                    for (int j = 0; j < cnt; j++)
                        all_influences.Add(this.Int32(mati_cursor));
                    mati_cursor += 4;
                }
                mati_cursor += 4;

                //influences.Count must equal vertices_count ALWAYS

                int relative_output_vert_index = mesh.reverseVertices.Count;
                if (count_influences == 0)
                {
                    for (int i = 0; i < count_vertices; i++)
                    {
                        float x = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + i * 0x10);
                        float y = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + i * 0x10 + 0x04);
                        float z = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + i * 0x10 + 0x08);

                        mesh.reverseVertices.Add(new Vector4[] { new Vector4(x, y, z, 1f) });
                        if (count_per_mat_verts > 0)
                        mesh.influences.Add(new int[] { all_influences[i] });
                    }
                }
                else
                {
                    int cursor = offset_influences;
                    List<int> counts = new List<int>(0);
                    for (int i = 0; i < count_influences; i++)
                    {
                        counts.Add(global::System.BitConverter.ToInt32(vu_memory, cursor));
                        cursor += 4;
                    }
                    while (cursor % 16 > 0) cursor += 4;


                    for (int i = 0; i < counts.Count; i++)
                    {
                        for (int j = 0; j < counts[i]; j++)
                        {
                            byte inf_count = (byte)(i + 1);
                            Vector4[] vec = new Vector4[inf_count];
                            int[] inf = new int[inf_count];

                            for (int k = 0; k < inf_count; k++)
                            {
                                int vert_index = global::System.BitConverter.ToInt32(vu_memory, cursor);
                                float x = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + vert_index * 0x10);
                                float y = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + vert_index * 0x10 + 0x04);
                                float z = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + vert_index * 0x10 + 0x08);
                                float w = global::System.BitConverter.ToSingle(vu_memory, offset_vertices + vert_index * 0x10 + 0x0C);

                                vec[k] = new Vector4(x, y, z, w);
                                inf[k] = all_influences[vert_index];
                                cursor += 4;
                            }
                            mesh.reverseVertices.Add(vec);
                            mesh.influences.Add(inf);
                        }
                        while (cursor % 16 > 0) cursor += 4;
                    }
                }

                int relative_output_color_index = mesh.colors.Count;

                for (int i = 0; i < count_color; i++)
                {
                    float r = global::System.BitConverter.ToInt32(vu_memory, offset_color + i * 0x10 + 0x00) / 128f;
                    float g = global::System.BitConverter.ToInt32(vu_memory, offset_color + i * 0x10 + 0x04) / 128f;
                    float b = global::System.BitConverter.ToInt32(vu_memory, offset_color + i * 0x10 + 0x08) / 128f;
                    float a = global::System.BitConverter.ToInt32(vu_memory, offset_color + i * 0x10 + 0x0C) / 128f;
                    mesh.colors.Add(new Color4(r, g, b, a));
                }
                for (int i = 0; i < count_texcoo_ind_strips; i++)
                {
                    int vert_flag_off_in_mem = 0x00;
                    if (vif_type < 2)
                    {
                        float u = global::System.BitConverter.ToInt32(vu_memory, offset_texcoo_ind_strips + i * 0x10) / 4096f;
                        float v = global::System.BitConverter.ToInt32(vu_memory, offset_texcoo_ind_strips + i * 0x10 + 0x04) / 4096f;
                        if (v>1f) v -= 16f;
                        mesh.textureCoordinates.Add(new Vector2(u, v));
                        vert_flag_off_in_mem += 0x08;
                    }

                    int vertexIndex = relative_output_vert_index + global::System.BitConverter.ToInt32(vu_memory, offset_texcoo_ind_strips + i * 0x10 + vert_flag_off_in_mem);
                    int triangle_strip = global::System.BitConverter.ToInt32(vu_memory, offset_texcoo_ind_strips + i * 0x10 + vert_flag_off_in_mem + 0x04);
                    
                    int colorIndex = 1;
                    if (count_color > 0)
                        colorIndex = relative_output_color_index + i;

                    mesh.vertexIndices.Add(vertexIndex);
                    if (vif_type < 2)
                        mesh.colorIndices.Add(colorIndex);
                    mesh.triangleFlags.Add(triangle_strip);
                }
            }
        }
    }
}
