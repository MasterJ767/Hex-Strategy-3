using System;
using UnityEngine;

namespace World {
    [Serializable]
    public struct AxialCoordinate
    {
        [SerializeField] private int q;
        [SerializeField] private int r;

        public int Q => q; // X (up to down)
        public int R => r; // Z (top right to bottom left diagonal)
        public int S => -q - r; // Y (top left to bottom right diagonal)

        public AxialCoordinate(int q, int r) {
            this.q = q;
            this.r = r;
        }

        public static AxialCoordinate Zero() => new AxialCoordinate(0, 0);
        public static AxialCoordinate NorthEast() => new AxialCoordinate(1, 0);
        public static AxialCoordinate SouthEast() => new AxialCoordinate(1, -1);
        public static AxialCoordinate South() => new AxialCoordinate(0, -1);
        public static AxialCoordinate SouthWest() => new AxialCoordinate(-1, 0);
        public static AxialCoordinate NorthWest() => new AxialCoordinate(-1, 1);
        public static AxialCoordinate North() => new AxialCoordinate(0, 1);

        public static AxialCoordinate FromOffsetCoordinates(int x, int z) {
            return new AxialCoordinate(x, z - (int)(x / 2));
        }

        public static Vector2Int ToOffsetCoordinates(AxialCoordinate coordinates) {
            return new Vector2Int(coordinates.q, coordinates.r + coordinates.q / 2);
        }

        public static AxialCoordinate FromPosition(Vector3 position) {
            float q = position.x / (Config.OuterRadius * 1.5f);
            float r = position.z / (Config.InnerRadius * 2f);
            r -= q / 2;
            
            int iQ = Mathf.RoundToInt(q);
            int iR = Mathf.RoundToInt(r);
            int iS = Mathf.RoundToInt(-q - r);

            if (iQ + iR + iS != 0)
            {
                float dQ = Mathf.Abs(q - iQ);
                float dR = Mathf.Abs(r - iR);
                float dS = Mathf.Abs(-q - r - iS);

                if (dQ > dR && dQ > dS) iQ = -iR - iS;
                else if (dR > dS) iR = -iQ - iS;
            }
            
            return new AxialCoordinate(iQ, iR);
        }
        
        public static AxialCoordinate operator +(AxialCoordinate a, AxialCoordinate b) => new AxialCoordinate(a.q + b.q, a.r + b.r);
        public static AxialCoordinate operator -(AxialCoordinate a, AxialCoordinate b) => new AxialCoordinate(a.q - b.q, a.r - b.r);
        public static AxialCoordinate operator *(AxialCoordinate a, int b) => new AxialCoordinate(a.q * b, a.r * b);
        public static AxialCoordinate operator *(int a, AxialCoordinate b) => new AxialCoordinate(a * b.q, a * b.r);
        
        public override string ToString() => "(" + Q + ", " + S + ", " + R + ")";
        public string ToStringOnSeparateLines() => Q + "\n" + S + "\n" + R;
    }
}