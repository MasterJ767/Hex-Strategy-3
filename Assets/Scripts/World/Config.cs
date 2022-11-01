using UnityEngine;

namespace World {
    public static class Config
    {
        public static readonly float OuterRadius = 10f;
        public static readonly float InnerRadius = (Mathf.Sqrt(3f) / 2f) * OuterRadius;

        public static readonly int ChunkWidth = 5;
        public static readonly int ChunkHeight = 5;
        public static readonly int WorldWidthInChunks = 3;
        public static readonly int WorldHeightInChunks = 2;
        public static readonly int WorldWidthInCells = ChunkWidth * WorldWidthInChunks;
        public static readonly int WorldHeightInCells = ChunkHeight * WorldHeightInChunks;

        public static readonly float StartBlendFactor = 0.8f;
        public static readonly float StartJunctionFactor = StartBlendFactor / 2f;

        public static readonly float ElevationStep = 3f;
        public static readonly int TerracesPerSlope = 3;
        public static readonly int TerraceSteps = (TerracesPerSlope * 2) + 1;
        public static readonly float TerraceHorizontalSize = 1f / TerracesPerSlope;
        public static readonly float TerraceVerticalSize = 1f / (TerracesPerSlope + 1);
        
        public static readonly float RiverBedElevationOffset = -1.5f;
        public static readonly float RiverSurfaceElevationOffset = -0.25f;

        public static readonly Vector3 RoadElevationOffset = new Vector3(0f, 0.0005f, 0f);

        private static readonly Vector3[] Corners = {
            new(0.5f * OuterRadius, 0f, InnerRadius), //top right corner
            new(OuterRadius, 0f, 0f), //centre right corner
            new(0.5f * OuterRadius, 0f, -InnerRadius), //bottom right corner
            new(-0.5f * OuterRadius, 0f, -InnerRadius), // bottom left corner
            new(-OuterRadius, 0f, 0f), //centre left corner
            new(-0.5f * OuterRadius, 0f, InnerRadius) //top left corner
        };

        public static int Mod(int n, int m) {
            return n % m < 0 ? (n % m) + m : n % m;
        }

        public static Vector3 GetFirstCorner(HexDirection direction) {
            return Corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction) {
            return Corners[Mod((int)direction + 1, 6)];
        }

        public static Vector3 GetFirstBlendCorner(HexDirection direction) {
            return GetFirstCorner(direction) * StartBlendFactor;
        }

        public static Vector3 GetSecondBlendCorner(HexDirection direction) {
            return GetSecondCorner(direction) * StartBlendFactor;
        }

        public static Vector3 GetFirstJunctionCorner(HexDirection direction) {
            return GetFirstCorner(direction) * StartJunctionFactor;
        }

        public static Vector3 GetSecondJunctionCorner(HexDirection direction) {
            return GetSecondCorner(direction) * StartJunctionFactor;
        }
    }
}
