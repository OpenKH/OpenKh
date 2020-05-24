using OpenKh.Engine.Maths;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class MiddleMesh
    {
        public Vector4[] rawPositionList = null; // small
        public Vector2[] uvList = null; // large
        public int[] vertexIndexMappingList = null; // large2small
        public int[] vertexFlagList = null; // large
        public JointAssignment[][] assignedJointsList = null; // small
        public int textureIndex = -1;
    }
}
