using UnityEditor;
using UnityEngine;

namespace CatsVsPillows.Editor.Art
{
    /// <summary>
    /// Generates a cartoon cushion mesh: two puffy halves meeting at a pinched seam, sides that bow inward,
    /// pointy corners and a tufted button dimple. The mesh fits a 1x1x1 box and is scaled by the Pillow prefab.
    /// </summary>
    internal static class PillowMeshGenerator
    {
        private const string MeshPath = "Assets/GAME ASSETS/Meshes/PuffyPillow.asset";

        private const int Segments = 28;
        private const float Puffiness = 0.28f;    // lower = fuller, boxier cushion; higher = rounder, thinner edges
        private const float SidePinch = 0.12f;    // how far the middle of each side bows inward
        private const float TuftDepth = 0.12f;    // depth of the center button dimple
        private const float TuftRadius = 0.16f;   // radius of the dimple in normalized units

        [MenuItem("Cats vs Pillows/Art/Generate Puffy Pillow Mesh")]
        private static void GenerateMenu()
        {
            Mesh mesh = Generate();
            EditorGUIUtility.PingObject(mesh);
        }

        public static Mesh Generate()
        {
            Mesh generated = Build();
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (existing != null)
            {
                // Copy into the existing asset so prefab references (GUIDs) stay intact.
                EditorUtility.CopySerialized(generated, existing);
                Object.DestroyImmediate(generated);
                AssetDatabase.SaveAssets();
                return existing;
            }

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MeshPath));
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(generated, MeshPath);
            AssetDatabase.SaveAssets();
            return generated;
        }

        private static Mesh Build()
        {
            int side = Segments + 1;
            int halfVertexCount = side * side;
            var vertices = new Vector3[halfVertexCount * 2];
            var uvs = new Vector2[halfVertexCount * 2];
            var triangles = new int[Segments * Segments * 6 * 2];

            int triangle = 0;
            for (int half = 0; half < 2; half++)
            {
                float sign = half == 0 ? 1f : -1f;
                int offset = half * halfVertexCount;

                for (int z = 0; z < side; z++)
                {
                    for (int x = 0; x < side; x++)
                    {
                        float u = x / (float)Segments * 2f - 1f;
                        float v = z / (float)Segments * 2f - 1f;
                        int index = offset + z * side + x;
                        vertices[index] = Surface(u, v, sign);
                        uvs[index] = new Vector2(u * 0.5f + 0.5f, v * 0.5f + 0.5f);
                    }
                }

                for (int z = 0; z < Segments; z++)
                {
                    for (int x = 0; x < Segments; x++)
                    {
                        int a = offset + z * side + x;
                        int b = a + 1;
                        int c = a + side;
                        int d = c + 1;

                        if (half == 0)
                        {
                            triangles[triangle++] = a; triangles[triangle++] = c; triangles[triangle++] = b;
                            triangles[triangle++] = b; triangles[triangle++] = c; triangles[triangle++] = d;
                        }
                        else
                        {
                            triangles[triangle++] = a; triangles[triangle++] = b; triangles[triangle++] = c;
                            triangles[triangle++] = b; triangles[triangle++] = d; triangles[triangle++] = c;
                        }
                    }
                }
            }

            var mesh = new Mesh { name = "PuffyPillow" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Point on the top (sign = 1) or bottom (sign = -1) half for normalized coordinates u, v in [-1, 1].</summary>
        private static Vector3 Surface(float u, float v, float sign)
        {
            float x = 0.5f * u * (1f - SidePinch * (1f - v * v));
            float z = 0.5f * v * (1f - SidePinch * (1f - u * u));

            float puff = Mathf.Pow(Mathf.Max(0f, (1f - u * u) * (1f - v * v)), Puffiness);
            float tuft = TuftDepth * Mathf.Exp(-(u * u + v * v) / (TuftRadius * TuftRadius));
            float y = sign * 0.5f * Mathf.Max(0f, puff - tuft);

            return new Vector3(x, y, z);
        }
    }
}
