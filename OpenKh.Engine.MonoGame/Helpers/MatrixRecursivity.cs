using OpenKh.Engine.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenKh;
using System.Numerics;

namespace OpenKh.Engine.Monogame.Helpers
{
    public class MatrixRecursivity
    {
        public static void ReverseMatrices(ref List<Microsoft.Xna.Framework.Matrix> matrices, MdlxParser mParser)
        {
            bool[] treatedMatrices = new bool[mParser.Bones.Count];

            int awaitingReverseCount;
            do
            {
                awaitingReverseCount = mParser.Bones.Count;
                for (int b = 0; b < mParser.Bones.Count; b++)
                {
                    if (treatedMatrices[b] == false)
                    {
                        int awaitingReverseChildrenCount = 0;
                        for (int j = 0; j < mParser.Bones.Count; j++)
                            if (mParser.Bones[j].Parent == b && treatedMatrices[j] == false)
                                awaitingReverseChildrenCount++;
                        if (awaitingReverseChildrenCount == 0)
                        {
                            if (mParser.Bones[b].Parent > -1)
                                matrices[b] *= Microsoft.Xna.Framework.Matrix.Invert(matrices[mParser.Bones[b].Parent]);
                            treatedMatrices[b] = true;
                            awaitingReverseCount--;
                        }
                    }
                    else
                        awaitingReverseCount--;
                }
            }
            while (awaitingReverseCount > 0);
        }

        public static void ComputeMatrices(ref List<Microsoft.Xna.Framework.Matrix> matrices, MdlxParser mParser)
        {
            for (int b = 0; b < mParser.Bones.Count; b++)
                if (mParser.Bones[b].Parent > -1)
                    matrices[b] *= matrices[mParser.Bones[b].Parent];
        }

        public static int LocateMostSimilarPose(List<Microsoft.Xna.Framework.Matrix> matrices, Matrix4x4[] Fk, MdlxParser mParser, float similarity, bool skinnedMatricesOnly)
        {
            if (Fk.Length != mParser.Bones.Count)
                throw new Exception("Incorrect input pose matrices.");
            Microsoft.Xna.Framework.Vector3[] scales = new Microsoft.Xna.Framework.Vector3[mParser.Bones.Count];
            Microsoft.Xna.Framework.Quaternion[] rotates = new Microsoft.Xna.Framework.Quaternion[mParser.Bones.Count];
            Microsoft.Xna.Framework.Vector3[] translates = new Microsoft.Xna.Framework.Vector3[mParser.Bones.Count];

            for (int j = 0; j < Fk.Length; j++)
            {
                Fk[j].ToXnaMatrix().Decompose(out scales[j], out rotates[j], out translates[j]);
                rotates[j].Normalize();
            }
            float smallestDist = Single.MaxValue;
            int position = (matrices.Count/ mParser.Bones.Count)-1;
            for (int i=0; i< matrices.Count; i+= mParser.Bones.Count)
            {
                float currentDist = 0;
                for (int j = 0; j < mParser.Bones.Count; j++)
                {
                    if (skinnedMatricesOnly && mParser.skinnedBones[j] == false) continue;
                    Microsoft.Xna.Framework.Vector3 scale;
                    Microsoft.Xna.Framework.Quaternion rotate;
                    Microsoft.Xna.Framework.Vector3 translate;
                    matrices[i + j].Decompose(out scale, out rotate, out translate);
                    rotate.Normalize();
                    currentDist += Microsoft.Xna.Framework.Vector3.Distance(scale, scales[j]);
                    currentDist += (float)(Math.Abs(rotate.X - rotates[j].X) + Math.Abs(rotate.Y - rotates[j].Y) + Math.Abs(rotate.Z - rotates[j].Z) + Math.Abs(rotate.W - rotates[j].W));
                    currentDist += Microsoft.Xna.Framework.Vector3.Distance(translate, translates[j]);
                }
                if (currentDist < similarity && currentDist < smallestDist)
                {
                    smallestDist = currentDist;
                    position = i / mParser.Bones.Count;
                }
            }
            Console.WriteLine(smallestDist);
            return position;
        }
    }
}
