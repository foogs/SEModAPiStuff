using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.Voxels;
using VRageMath;

namespace FoogsVoxelHelper
{
    /// <summary>
    /// https://github.com/foogs/SEModAPiStuff
    /// </summary>
    public static class VoxelUtils
    {
        /// <summary>
        /// Gets all materials from voxel maps inside given box
        /// </summary>
        /// <param name="WorldlBoxWrongRotation"><see langword="abstract"/>box</param>
        /// <param name="m">rotation matrix (World matrix from block) </param>
        /// <param name="materialsBuf">Dictionary for materials</param>
        /// <param name="foundVoxelBuf">List for voxel maps</param>
        /// <returns></returns>
        public static int GetMaterialsFromVoxelsWhoseInBox(ref BoundingBoxD WorldlBoxWrongRotation, ref MatrixD m, ref Dictionary<byte, int> materialsBuf, ref List<MyVoxelBase> foundVoxelBuf, int lod = 0)
        {
            var WorldBoxCorrectRotation = VoxelUtils.FoogsDontKnowMathKek(ref m, ref WorldlBoxWrongRotation);
            GetVoxelMapsWhoseBoundingBoxesIntersectBox(ref WorldBoxCorrectRotation, null, ref foundVoxelBuf);
            if (foundVoxelBuf != null)
            {
                foreach (var entity in foundVoxelBuf)
                {
                    GetMaterialsFromVoxelMapIntersectBox(entity, WorldBoxCorrectRotation, ref materialsBuf, lod);
                }
            }
            return foundVoxelBuf.Count;
        }

        /// <summary>
        /// Retrive materials in a given area
        /// </summary>
        /// <param name="voxel"></param>
        /// <param name="areaWorldRealBox"></param>
        /// <param name="materialsBuf"></param>
        public static void GetMaterialsFromVoxelMapIntersectBox(MyVoxelBase voxel, BoundingBoxD areaWorldRealBox, ref Dictionary<byte, int> materialsBuf, int lod)
        {
            try
            {
                bool flag = voxel == null || voxel.MarkedForClose;
                if (!flag)
                {
                    // Debug.GPS("groundingArea3", area.Center); //vectorMin);
                    // Debug.GPS("amin", area.Min);
                    // Debug.GPS("amax", area.Max);
                    Vector3I locmin;
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxel.PositionLeftBottomCorner, ref areaWorldRealBox.Max, out locmin);
                    Vector3I locmax;
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxel.PositionLeftBottomCorner, ref areaWorldRealBox.Min, out locmax);
                    BoundingBoxD boundingBoxD = areaWorldRealBox.TransformFast(voxel.PositionComp.WorldMatrixNormalizedInv);
                    boundingBoxD.Translate(voxel.StorageMin + voxel.SizeInMetresHalf);
                    locmin = Vector3I.Floor(boundingBoxD.Min);
                    locmax = Vector3I.Ceiling(boundingBoxD.Max);
                    Vector3I vector3I = voxel.Storage.Size;
                    vector3I -= 1;
                    Vector3I.Clamp(ref locmin, ref Vector3I.Zero, ref vector3I, out locmin);
                    Vector3I.Clamp(ref locmax, ref Vector3I.Zero, ref vector3I, out locmax);
                    Vector3I llocmin = locmin - 0;
                    Vector3I llocmax = locmax + 0;
                    MyStorageData myStorageData = new MyStorageData(MyStorageDataTypeFlags.ContentAndMaterial);
                    ClampVoxelCoord(voxel.Storage, ref llocmin, 1);
                    ClampVoxelCoord(voxel.Storage, ref llocmax, 1);
                    llocmin >>= lod;
                    llocmin -= 0;
                    llocmax >>= lod;
                    llocmax += 0;
                    bool flag2 = myStorageData == null;
                    if (flag2)
                    {
                        myStorageData = new MyStorageData();
                    }



                    myStorageData.Resize(llocmin, llocmax);

                    //Vector3D test = new Vector3D();
                    //Vector3D test2 = new Vector3D(); ;
                    // MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxel.PositionLeftBottomCorner, ref locmin, out test);
                    // MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxel.PositionLeftBottomCorner, ref locmax, out test2);
                    //Debug.GPS("min", test);
                    // Debug.GPS("max", test2);

                    using (voxel.Pin())
                    {
                        voxel.Storage.ReadRange(myStorageData, MyStorageDataTypeFlags.Material, lod, llocmin, llocmax);
                    }
                    HashSet<byte> hashSet = new HashSet<byte>();
                    Vector3I vector3I5;
                    vector3I5.X = llocmin.X;
                    int num3 = 0;
                    int num2 = 0;
                    while (vector3I5.X <= llocmax.X)
                    {
                        vector3I5.Y = llocmin.Y;
                        while (vector3I5.Y <= llocmax.Y)
                        {
                            vector3I5.Z = llocmin.Z;
                            while (vector3I5.Z <= llocmax.Z)
                            {
                                Vector3I vector3I6 = vector3I5 - llocmin;
                                num2 = myStorageData.ComputeLinear(ref vector3I6);
                                byte b = myStorageData.Material(num2);
                                bool flag3 = b != byte.MaxValue;
                                if (flag3)
                                {
                                    //MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(b);
                                    try
                                    {
                                        materialsBuf[b]++;
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        materialsBuf[b] = 1;
                                    }
                                }
                                vector3I5.Z++;
                            }
                            vector3I5.Y++;
                        }
                        vector3I5.X++;
                    }
                }
            }
            catch
            {
                // Debug.Chat("catch");
            }
        }

        /// <summary>
        ///Copy of Sandbox.Game.Entities.MyVoxelMaps::GetVoxelMapsWhoseBoundingBoxesIntersectBox        
        /// </summary>
        /// <param name="worldAABB"></param>
        /// <param name="ignoreVoxelMap"></param>
        /// <param name="voxelList"></param>
        /// <returns></returns>
        public static bool GetVoxelMapsWhoseBoundingBoxesIntersectBox(ref BoundingBoxD worldAABB, MyVoxelBase ignoreVoxelMap, ref List<MyVoxelBase> voxelList)
        {
            // Debug.GPS("voxel check", worldAABB.Center);
            // Debug.GPS("voxel check max", worldAABB.Max);
            // Debug.GPS("voxel check min", worldAABB.Min);
            int num = 0;
            List<IMyVoxelBase> buf = new List<IMyVoxelBase>();
            MyAPIGateway.Session.VoxelMaps.GetInstances(buf, (x) => x != ignoreVoxelMap && (x is MyPlanet || x is MyVoxelMap));
            foreach (MyVoxelBase myVoxelBase in buf)
            {
                if (!myVoxelBase.MarkedForClose && !myVoxelBase.Closed && myVoxelBase.IsBoxIntersectingBoundingBoxOfThisVoxelMap(ref worldAABB))
                {
                    voxelList.Add(myVoxelBase);
                    num++;
                }
            }
            buf.Clear();
            return num > 0;
        }

        public static void ClampVoxelCoord(VRage.ModAPI.IMyStorage self, ref Vector3I voxelCoord, int distance = 100)
        {
            if (self == null)
            {
                return;
            }
            Vector3I vector3I = self.Size - distance;
            Vector3I.Clamp(ref voxelCoord, ref Vector3I.Zero, ref vector3I, out voxelCoord);
        }

        /// <summary>
        /// Copy math from DrawWireFramedBox.
        /// Apply correct rotation and position from matrix
        /// </summary>
        /// <param name="worldMatrix"></param>
        /// <param name="localbox"></param>
        /// <returns></returns>
        private static BoundingBoxD FoogsDontKnowMathKek(ref MatrixD worldMatrix, ref BoundingBoxD localbox)
        {
            MatrixD orientation = MatrixD.Identity;
            orientation.Forward = worldMatrix.Forward;
            orientation.Up = worldMatrix.Up;
            orientation.Right = worldMatrix.Right;
            var forwardNormal = orientation.Forward;
            var rightNormal = orientation.Right;
            var upNormal = orientation.Up;
            float width = (float)localbox.Size.X;
            float height = (float)localbox.Size.Y;
            float deep = (float)localbox.Size.Z;
            Vector3D globalBoxCenter = Vector3D.Transform(localbox.Center, worldMatrix);
            Vector3D faceCenter = globalBoxCenter + forwardNormal * (deep * 0.5f); // Front side
            Vector3D faceCenter2 = globalBoxCenter - forwardNormal * (deep * 0.5f); // Back side
            var wireDivideRatio = new Vector3I();
            //@ FrontSide
            Vector3D vctStart = localbox.Min;
            Vector3D vctEnd = vctStart + Vector3.Up * height;
            Vector3D vctSideStep = Vector3.Right * (width / wireDivideRatio.X);
            var color = new Vector4(0, 155, 0, 123);
            vctStart = Vector3D.Transform(vctStart, worldMatrix);
            vctEnd = Vector3D.Transform(vctEnd, worldMatrix);
            //MySimpleObjectDraw.DrawLine(vctEnd, vctEnd + 0.5, MyStringId.GetOrCompute("Square"), ref color, 0.5f);
            //  GenerateLines(vctStart, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            var max = vctEnd;
            // BackSide
            vctStart += Vector3.Backward * deep;
            vctEnd = vctStart + Vector3.Up * height;
            //  GenerateLines(vctStart, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            //@ FrontSide
            vctStart = localbox.Min;
            vctEnd = vctStart + Vector3.Right * width;
            vctSideStep = Vector3.Up * (height / wireDivideRatio.Y);
            //  GenerateLines(vctStart, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            //@ BackSide
            vctStart += Vector3.Backward * deep;
            vctEnd += Vector3.Backward * deep;
            //GenerateLines(vctStart, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            vctStart = Vector3D.Transform(vctStart, worldMatrix);
            vctEnd = Vector3D.Transform(vctEnd, worldMatrix);
            //MySimpleObjectDraw.DrawLine(vctEnd, vctEnd + 0.5, MyStringId.GetOrCompute("Square"), ref color, 0.5f);
            var min = vctEnd;
            return new BoundingBoxD(min, max);

        }
    }
}
