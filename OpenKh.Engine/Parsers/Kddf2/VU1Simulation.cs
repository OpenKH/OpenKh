using OpenKh.Engine.Maths;
using System;
using System.IO;
using System.Numerics;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class VU1Simulation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrixIndexList">The limited count of matrices are transferred to VU1 due to memory limitation.</param>
        /// <returns></returns>
        public static ImmutableMesh Run(byte[] vu1mem, int tops, int top2, int textureIndex, int[] matrixIndexList)
        {
            MemoryStream si = new MemoryStream(vu1mem, true);
            BinaryReader br = new BinaryReader(si);

            si.Position = 16 * (tops);

            var vpu = VpuPacket.Header(si);

            if (vpu.Type != 1 && vpu.Type != 2) throw new ProtInvalidTypeException();

            // Type1 example:

            // 0004   unpack V4-32 c 4 a 000 usn 0 flg 1 m 0
            //     00000001 00000000 00000000 00000000 
            //     0000004f 00000004 0000009e 000000a1 
            //     00000000 00000000 00000000 00000000 // this 16 bytes: seen in only for Type1
            //     0000004b 00000053 00000000 0000000b 

            //  int type = 1;
            //  int unk1 = 0;
            //  int unk2 = 0;
            //  int unk3 = 0;

            //  int cntBox2 = 0x4f;
            //  int offBox2 = 0x4;
            //  int offVertexCountPerMatrix = 0x9e;
            //  int offMatrices = 0xa1;

            //  int cntVertsColor = 0x0; // only for Type1
            //  int offVertsColor = 0x0; // only for Type1
            //  int cntSpec = 0x0; // only for Type1
            //  int offSpec = 0x0; // only for Type1

            //  int cntVerts = 0x4b;
            //  int offVerts = 0x53;
            //  int unka = 0;
            //  int cntVertexCountPerMatrix = 0xb;

            // box1 → VertexCountPerMatrix

            int v04 = vpu.Unknown04;
            int v08 = vpu.Unknown08;
            int v0c = vpu.Unknown1cLocation;
            int offMatrices = vpu.Unknown1cLocation; // off matrices

            // Shady logic here???? It MIGHT cause problems:
            // If vpu.Type != 1, those 0x10 bytes will be skipped. Therefore,
            // stuff that it's at 0x40, will be at 0x30 instead.
            int cntvertscolor = (vpu.Type == 1) ? vpu.ColorCount : 0; // cntvertscolor
            int offvertscolor = (vpu.Type == 1) ? vpu.ColorLocation : 0; // offvertscolor
            int cntVertexMixer = (vpu.Type == 1) ? vpu.Unknown28 : 0; // cnt spec
            int offVertexMixer = (vpu.Type == 1) ? vpu.Unknown2c : 0; // off spec

            int v38 = vpu.Unknown38; // 

            si.Position = 16 * (tops + vpu.UnkBoxLocation);
            int[] vertexCountPerMatrix = new int[vpu.UnkBoxCount];
            for (int x = 0; x < vertexCountPerMatrix.Length; x++)
            {
                vertexCountPerMatrix[x] = br.ReadInt32();
            }

            ImmutableMesh mesh = new ImmutableMesh();
            mesh.textureIndex = textureIndex;
            mesh.vertexAssignmentsList = new VertexAssignment[vpu.VertexCount][];

            VertexAssignment[] vertexAssignmentList = new VertexAssignment[vpu.VertexCount];

            int vertexIndex = 0;
            si.Position = 16 * (tops + vpu.VertexLocation);
            for (int indexToMatrixIndex = 0; indexToMatrixIndex < vertexCountPerMatrix.Length; indexToMatrixIndex++)
            {
                int cntVertices = vertexCountPerMatrix[indexToMatrixIndex];
                for (int t = 0; t < cntVertices; t++)
                {
                    float xPos = br.ReadSingle();
                    float yPos = br.ReadSingle();
                    float zPos = br.ReadSingle();
                    float weight = br.ReadSingle();
                    Vector4 v4 = new Vector4(xPos, yPos, zPos, weight);
                    mesh.vertexAssignmentsList[vertexIndex] = new VertexAssignment[] {
                        vertexAssignmentList[vertexIndex] = new VertexAssignment{
                            matrixIndex = matrixIndexList[indexToMatrixIndex],
                            weight = weight,
                            rawPos = v4,
                        }
                    };

                    vertexIndex++;
                }
            }

            mesh.indexAssignmentList = new IndexAssignment[vpu.IndexCount];

            si.Position = 16 * (tops + vpu.IndexLocation);
            for (int x = 0; x < vpu.IndexCount; x++)
            {
                int texU = br.ReadUInt16() / 16;
                br.ReadUInt16(); // skip
                int texV = br.ReadUInt16() / 16;
                br.ReadUInt16(); // skip

                var localVertexIndex = br.ReadUInt16();
                br.ReadUInt16(); // skip
                var vertexFlag = br.ReadUInt16();
                br.ReadUInt16(); // skip

                mesh.indexAssignmentList[x] = new IndexAssignment(
                    new Vector2(texU / 256.0f, texV / 256.0f),
                    localVertexIndex,
                    vertexFlag
                );
            }

            if (cntVertexMixer != 0)
            {
                si.Position = 16 * (tops + offVertexMixer);
                int cntSkip = br.ReadInt32();
                int cntVerticesMix2ToOne = br.ReadInt32();
                int cntVerticesMix3ToOne = br.ReadInt32();
                int cntVerticesMix4ToOne = br.ReadInt32();
                int cntVerticesMix5ToOne = 0;
                int cntVerticesMix6ToOne = 0;
                int cntVerticesMix7ToOne = 0;
                int cntVerticesMix8ToOne = 0;

                if (cntVertexMixer >= 5)
                {
                    cntVerticesMix5ToOne = br.ReadInt32();
                    cntVerticesMix6ToOne = br.ReadInt32();
                    cntVerticesMix7ToOne = br.ReadInt32();
                    cntVerticesMix8ToOne = br.ReadInt32(); // unused in asset models.
                }

                var vertexAssignmentCount = cntSkip
                    + cntVerticesMix2ToOne
                    + cntVerticesMix3ToOne
                    + cntVerticesMix4ToOne
                    + cntVerticesMix5ToOne
                    + cntVerticesMix6ToOne
                    + cntVerticesMix7ToOne
                    + cntVerticesMix8ToOne
                    ;

                VertexAssignment[][] newVertexAssignList = new VertexAssignment[vertexAssignmentCount][];
                int inputVertexIndex = 0;
                for (; inputVertexIndex < cntSkip; inputVertexIndex++)
                {
                    int index = br.ReadInt32();
                    newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                        vertexAssignmentList[index]
                    });
                }
                if (cntVertexMixer >= 2)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15); // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix2ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2]
                        });
                    }
                }
                if (cntVertexMixer >= 3)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15); // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix3ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3]
                        });
                    }
                }
                if (cntVertexMixer >= 4)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15); // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix4ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        int vertex4 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3],
                            vertexAssignmentList[vertex4]
                        });
                    }
                }
                if (cntVertexMixer >= 5)
                {
                    si.Position = (si.Position + 15) & (~15);  // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix5ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        int vertex4 = br.ReadInt32();
                        int vertex5 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3],
                            vertexAssignmentList[vertex4],
                            vertexAssignmentList[vertex5]
                        });
                    }
                }
                if (cntVertexMixer >= 6)
                {
                    si.Position = (si.Position + 15) & (~15);  // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix6ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        int vertex4 = br.ReadInt32();
                        int vertex5 = br.ReadInt32();
                        int vertex6 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3],
                            vertexAssignmentList[vertex4],
                            vertexAssignmentList[vertex5],
                            vertexAssignmentList[vertex6]
                        });
                    }
                }
                if (cntVertexMixer >= 7)
                {
                    si.Position = (si.Position + 15) & (~15);  // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix7ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        int vertex4 = br.ReadInt32();
                        int vertex5 = br.ReadInt32();
                        int vertex6 = br.ReadInt32();
                        int vertex7 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3],
                            vertexAssignmentList[vertex4],
                            vertexAssignmentList[vertex5],
                            vertexAssignmentList[vertex6],
                            vertexAssignmentList[vertex7]
                        });
                    }
                }
                if (cntVertexMixer >= 8)
                {
                    si.Position = (si.Position + 15) & (~15);  // 16 bytes alignment

                    for (int x = 0; x < cntVerticesMix8ToOne; x++, inputVertexIndex++)
                    {
                        int vertex1 = br.ReadInt32();
                        int vertex2 = br.ReadInt32();
                        int vertex3 = br.ReadInt32();
                        int vertex4 = br.ReadInt32();
                        int vertex5 = br.ReadInt32();
                        int vertex6 = br.ReadInt32();
                        int vertex7 = br.ReadInt32();
                        int vertex8 = br.ReadInt32();
                        newVertexAssignList[inputVertexIndex] = (new VertexAssignment[] {
                            vertexAssignmentList[vertex1],
                            vertexAssignmentList[vertex2],
                            vertexAssignmentList[vertex3],
                            vertexAssignmentList[vertex4],
                            vertexAssignmentList[vertex5],
                            vertexAssignmentList[vertex6],
                            vertexAssignmentList[vertex7],
                            vertexAssignmentList[vertex8]
                        });
                    }
                }
                // if cntVertexMixer >= 1, replace current vertexAssignmentList with regrouped list.
                mesh.vertexAssignmentsList = newVertexAssignList;
            }

            return mesh;
        }
    }
}
