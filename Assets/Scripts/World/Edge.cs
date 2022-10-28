using UnityEngine;

namespace World {
    public static class Edge 
    {
        public static Vector3 TerraceLerp(Vector3 p1, Vector3 p2, int step) {
            if (step <= 0) return p1;
            if (step >= Config.TerraceSteps) return p2;

            float h = (int)(step / 2) * Config.TerraceHorizontalSize;
            p1.x += (p2.x - p1.x) * h;
            p1.z += (p2.z - p1.z) * h;
            float v = (int)((step + 1) / 2) * Config.TerraceVerticalSize;
            p1.y += (p2.y - p1.y) * v;
            return p1;
        }

        public static Color TerraceColourLerp(Color c1, Color c2, int step)
        {
            if (step <= 0) return c1;
            if (step >= Config.TerraceSteps) return c2;
            
            float h = (int)(step / 2) * Config.TerraceHorizontalSize;
            return Color.Lerp(c1, c2, h);
        }
    }

    public struct Edge3 {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;

        public Edge3(Vector3 start, Vector3 end){
            v1 = start;
            v2 = Vector3.Lerp(start, end, 0.5f);
            v3 = end;
        }

        public Edge3(Edge5 e1) {
            v1 = e1.v2;
            v2 = e1.v3;
            v3 = e1.v4;
        }

        public static Edge3 SetY(Edge3 e, float y){
            e.v1.y = y;
            e.v2.y = y;
            e.v3.y = y;
            return e;
        }

        public static Edge3 TerraceLerp(Edge3 e1, Edge3 e2, int step){
            Edge3 e3;
            e3.v1 = Edge.TerraceLerp(e1.v1, e2.v1, step);
            e3.v2 = Edge.TerraceLerp(e1.v2, e2.v2, step);
            e3.v3 = Edge.TerraceLerp(e1.v3, e2.v3, step);
            return e3;
        }
    }

    public struct Edge5 {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
        public Vector3 v4;
        public Vector3 v5;

        public Edge5(Vector3 start, Vector3 end){
            v1 = start;
            v2 = Vector3.Lerp(start, end, 0.25f);
            v3 = Vector3.Lerp(start, end, 0.5f);
            v4 = Vector3.Lerp(start, end, 0.75f);
            v5 = end;
        }

        public Edge5(Edge3 e1) {
            v1 = e1.v1;
            v2 = Vector3.Lerp(e1.v1, e1.v2, 0.5f);
            v3 = e1.v2;
            v4 = Vector3.Lerp(e1.v2, e1.v3, 0.5f);
            v5 = e1.v3;
        }

        public static Edge5 SetY(Edge5 e, float y){
            e.v1.y = y;
            e.v2.y = y;
            e.v3.y = y;
            e.v4.y = y;
            e.v5.y = y;
            return e;
        }

        public static Edge5 TerraceLerp(Edge5 e1, Edge5 e2, int step){
            Edge5 e3;
            e3.v1 = Edge.TerraceLerp(e1.v1, e2.v1, step);
            e3.v2 = Edge.TerraceLerp(e1.v2, e2.v2, step);
            e3.v3 = Edge.TerraceLerp(e1.v3, e2.v3, step);
            e3.v4 = Edge.TerraceLerp(e1.v4, e2.v4, step);
            e3.v5 = Edge.TerraceLerp(e1.v5, e2.v5, step);
            return e3;
        }
    }
}
