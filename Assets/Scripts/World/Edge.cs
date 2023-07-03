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

        public static Edge3 PerturbInRelation(Edge3 e1, Edge5 e2)
        {
            Edge3 result;
            result.v1 = Config.PerturbInRelation(e1.v1, e2.v1);
            result.v2 = Config.PerturbInRelation(e1.v2, e2.v3);
            result.v3 = Config.PerturbInRelation(e1.v3, e2.v5);
            return result;
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

        public static Edge5 Reverse(Edge5 e) {
            Edge5 result;
            result.v1 = e.v5;
            result.v2 = e.v4;
            result.v3 = e.v3;
            result.v4 = e.v2;
            result.v5 = e.v1;
            return result;
        }

        public static Edge5 Perturb(Edge5 e)
        {
            Edge5 result;
            result.v1 = Config.Perturb(e.v1);
            result.v2 = Config.Perturb(e.v2);
            result.v3 = Config.Perturb(e.v3);
            result.v4 = Config.Perturb(e.v4);
            result.v5 = Config.Perturb(e.v5);
            return result;
        }
    }

    public struct Edge7 {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
        public Vector3 v4;
        public Vector3 v5;
        public Vector3 v6;
        public Vector3 v7;

        public Edge7(Vector3 start, Vector3 end){
            v1 = start;
            v2 = Vector3.Lerp(start, end, 1f / 6);
            v3 = Vector3.Lerp(start, end, 2f / 6);
            v4 = Vector3.Lerp(start, end, 0.5f);
            v5 = Vector3.Lerp(start, end, 4f / 6);
            v6 = Vector3.Lerp(start, end, 5f / 6);
            v7 = end;
        }

        public static Edge7 SetY(Edge7 e, float y){
            e.v1.y = y;
            e.v2.y = y;
            e.v3.y = y;
            e.v4.y = y;
            e.v5.y = y;
            e.v6.y = y;
            e.v7.y = y;
            return e;
        }
    }
}
