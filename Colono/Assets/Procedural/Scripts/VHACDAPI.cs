using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace CMR
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VHACDSession
    {
        // Maximum number of convex hulls to produce (default = 64, range = 1 - 1024)
        public int maxConvexHulls;
        // Maximum number of voxels generated during the voxelization stage
        // (default = 100,000, range = 10,000 - 64,000,000)
        public int resolution;
        // Maximum allowed concavity (default = 0.0025, range = 0.0 - 1.0)
        public double concavity;
        // Controls the granularity of the search for the "best" clipping plane
        // (default = 4, range = 1 - 16)
        public int planeDownsampling;
        // Controls the precision of the convex - hull generation process during
        // the clipping plane selection stage(default = 4, range = 1 - 16)
        public int convexhullDownsampling;
        // Controls the bias toward clipping along symmetry planes
        // (default = 0.05, range = 0.0 - 1.0)
        public double alpha;
        // Controls the bias toward clipping along revolution axes
        // (default = 0.05, range = 0.0 - 1.0)
        public double beta;
        // Enable / disable normalizing the mesh before applying the convex
        // decomposition (default = false)
        public int pca;
        // 0: voxel - based approximate convex decomposition, 1 : tetrahedron - based
        // approximate convex decomposition (default = 0, range = { 0,1 })
        public int mode;
        // Controls the maximum number of triangles per convex hull
        // (default = 64, range = 4 - 1024)
        public int maxNumVerticesPerCH;
        // Controls the adaptive sampling of the generated convex hulls
        // (default = 0.0001, range = 0.0 - 0.01)
        public double minVolumePerCH;
        // Enable / disable approximation when computing convex hulls
        // (default = true)
        public int convexHullApproximation;
        // Project the output convex hull vertices onto the original source mesh to
        // increase the floating point accuracy of the results (default = true)
        public int projectHullVertices;
        // Enable / disable OpenCL acceleration (default = false)
        public int oclAcceleration;
        // OpenCL platform id (default = 0, range = 0 - # OCL platforms)
        public int oclPlatformID;
        // OpenCL device id (default = 0, range = 0 - # OCL devices)
        public int oclDeviceID;

        public IntPtr outPoints;
        public IntPtr outTriangles;
        public IntPtr outPointOffsets;
        public IntPtr outTriangleOffsets;
        public int outNumPoints;
        public int outNumTriangles;
        public int outNumMeshes;

        public static VHACDSession Create()
        {
            VHACDSession session = new VHACDSession();
            session.maxConvexHulls = 64;
            session.resolution = 100000;
            session.concavity = 0.0025;
            session.planeDownsampling = 4;
            session.convexhullDownsampling = 4;
            session.alpha = 0.05;
            session.beta = 0.05;
            session.pca = 0;
            session.mode = 0;
            session.maxNumVerticesPerCH = 64;
            session.minVolumePerCH = 0.0001;
            session.convexHullApproximation = 1;
            session.projectHullVertices = 1;
            session.oclAcceleration = 0;
            session.oclPlatformID = 0;
            session.oclDeviceID = 0;
            return session;
        }
    }

    public class VHCDAPI
    {
        [DllImport("vhacdapi.dll")]
        static extern bool VHACDGetOpenCLPlatforms(IntPtr outPlatforms);

        [DllImport("vhacdapi.dll")]
        static extern bool VHACDGetOpenCLDevices(uint platformIndex, IntPtr outDevices);

        [DllImport("vhacdapi.dll")]
        static extern bool VHACDConvexDecomposition(
            [In] double[] inPoints, int inNumPoints,
            [In] int[] inTriangles, int inNumTriangles,
            [In][Out] ref VHACDSession session);

        [DllImport("vhacdapi.dll")]
        static extern void VHACDShutdown([In][Out] ref VHACDSession session);

        public static List<string> GetPlatforms()
        {
            List<string> names = new List<string>();

            IntPtr namesPtr = Marshal.AllocHGlobal(8 * 64);
            if (!VHACDGetOpenCLPlatforms(namesPtr))
            {
                Marshal.FreeHGlobal(namesPtr);
                return names;
            }

            byte[] namesData = new byte[8 * 64];
            Marshal.Copy(namesPtr, namesData, 0, 8 * 64);
            Marshal.FreeHGlobal(namesPtr);

            for (int i = 0; i < 8; i++)
            {
                string name = Encoding.UTF8.GetString(namesData, i * 64, 64).TrimEnd('\0');
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public static List<string> GetDevices(uint platformIndex)
        {
            List<string> names = new List<string>();

            IntPtr namesPtr = Marshal.AllocHGlobal(8 * 64);
            if (!VHACDGetOpenCLDevices(platformIndex, namesPtr))
            {
                Marshal.FreeHGlobal(namesPtr);
                return names;
            }

            byte[] namesData = new byte[8 * 64];
            Marshal.Copy(namesPtr, namesData, 0, 8 * 64);
            Marshal.FreeHGlobal(namesPtr);

            for (int i = 0; i < 8; i++)
            {
                string name = Encoding.UTF8.GetString(namesData, i * 64, 64).TrimEnd('\0');
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public static List<Mesh> ConvexDecomposition(Vector3[] vertices, int[] indices, VHACDSession session)
        {
            double[] points = new double[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                int offset = i * 3;
                points[offset + 0] = vertex.x;
                points[offset + 1] = vertex.y;
                points[offset + 2] = vertex.z;
            }

            bool res = VHACDConvexDecomposition(points, vertices.Length, indices, indices.Length / 3, ref session);
            if (!res)
            {
                return new List<Mesh>();
            }

            double[] outPoints = new double[session.outNumPoints * 3];
            int[] outTriangles = new int[session.outNumTriangles * 3];
            int[] outPointOffsets = new int[session.outNumMeshes];
            int[] outTriangleOffsets = new int[session.outNumMeshes];

            Marshal.Copy(session.outPoints, outPoints, 0, outPoints.Length);
            Marshal.Copy(session.outTriangles, outTriangles, 0, outTriangles.Length);
            Marshal.Copy(session.outPointOffsets, outPointOffsets, 0, outPointOffsets.Length);
            Marshal.Copy(session.outTriangleOffsets, outTriangleOffsets, 0, outTriangleOffsets.Length);

            List<Mesh> meshes = new List<Mesh>(session.outNumMeshes);
            for (int i = 0; i < session.outNumMeshes; i++)
            {
                int pointOffset = outPointOffsets[i];
                int triangleOffset = outTriangleOffsets[i];
                int vertexDataCount;
                int indexCount;
                if (i < session.outNumMeshes - 1)
                {
                    vertexDataCount = outPointOffsets[i + 1] - pointOffset;
                    indexCount = outTriangleOffsets[i + 1] - triangleOffset;
                }
                else
                {
                    vertexDataCount = outPoints.Length - pointOffset;
                    indexCount = outTriangles.Length - triangleOffset;
                }

                Vector3[] curVertices = new Vector3[vertexDataCount / 3];
                for (int j = 0; j < curVertices.Length; j++)
                {
                    curVertices[j].x = (float)outPoints[pointOffset + 0];
                    curVertices[j].y = (float)outPoints[pointOffset + 1];
                    curVertices[j].z = (float)outPoints[pointOffset + 2];
                    pointOffset += 3;
                }

                int[] curIndices = new int[indexCount];
                Array.Copy(outTriangles, triangleOffset, curIndices, 0, indexCount);

                Mesh mesh = new Mesh();
                mesh.Clear();
                mesh.vertices = curVertices;
                mesh.triangles = curIndices;
                mesh.Optimize();
                mesh.RecalculateNormals();

                meshes.Add(mesh);
            }

            VHACDShutdown(ref session);
            return meshes;
        }

        private static uint IntToUInt(int value)
        {
            return (uint)value;
        }
    }
}
