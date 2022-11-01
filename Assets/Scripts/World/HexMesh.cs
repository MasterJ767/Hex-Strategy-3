using System;
using System.Collections.Generic;
using UnityEngine;

namespace World {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        public bool useCollider;
        public bool useColours;
        public bool useUVCoordinates;
        
        private Mesh mesh;
        private MeshCollider meshCollider;
        
        [NonSerialized] private List<Vector3> vertices;
        [NonSerialized] private List<Color> colours;
        [NonSerialized] private List<Vector2> uvs;
        [NonSerialized] private List<int> triangles;
        
        private void Awake() {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            if (useCollider) meshCollider = gameObject.AddComponent<MeshCollider>();
            mesh.name = "Hex Mesh";
        }

        public void Clear () {
            mesh.Clear();
            vertices = ListPool<Vector3>.Get();
            if (useColours) colours = ListPool<Color>.Get();
            if (useUVCoordinates) uvs = ListPool<Vector2>.Get();
            triangles = ListPool<int>.Get();
        }

        public void Apply () {
            mesh.SetVertices(vertices);
            ListPool<Vector3>.Add(vertices);
            if (useColours)
            {
                mesh.SetColors(colours);
                ListPool<Color>.Add(colours);
            }
            if (useUVCoordinates) {
                mesh.SetUVs(0, uvs);
                ListPool<Vector2>.Add(uvs);
            }
            mesh.SetTriangles(triangles, 0);
            ListPool<int>.Add(triangles);
            mesh.RecalculateNormals();
            if (useCollider) {
                meshCollider.sharedMesh = mesh;
            }
        }

        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        public void AddTriangleColour(Color c1) {
            colours.Add(c1);
            colours.Add(c1);
            colours.Add(c1);
        }

        public void AddTriangleColour(Color c1, Color c2, Color c3) {
            colours.Add(c1);
            colours.Add(c2);
            colours.Add(c3);
        }

        public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
        }

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        public void AddQuadColour (Color c1) {
            colours.Add(c1);
            colours.Add(c1);
            colours.Add(c1);
            colours.Add(c1);
        }

        public void AddQuadColour(Color c1, Color c2) {
            colours.Add(c1);
            colours.Add(c1);
            colours.Add(c2);
            colours.Add(c2);
        }

        public void AddQuadColour(Color c1, Color c2, Color c3, Color c4) {
            colours.Add(c1);
            colours.Add(c2);
            colours.Add(c3);
            colours.Add(c4);
        }

        public void AddQuadUV (Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
            uvs.Add(uv4);
        }


    }
}
