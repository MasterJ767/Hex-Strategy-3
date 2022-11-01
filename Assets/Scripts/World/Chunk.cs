using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World {
    public class Chunk : MonoBehaviour
    {
        public HexMesh terrain;
        public HexMesh rivers;
        public HexMesh roads;
        
        private Cell[] cells;
        private Canvas gridCanvas;
        
        private void Awake () {
            gridCanvas = GetComponentInChildren<Canvas>();

            roads.transform.position = Config.RoadElevationOffset;

            cells = new Cell[Config.ChunkWidth * Config.ChunkHeight];
            ShowUI(false);
        }
        
        private void LateUpdate() {
            TriangulateCells();
            enabled = false;
        }

        public void Refresh () {
            enabled = true;
        }
        
        public void ShowUI (bool visible) {
            gridCanvas.gameObject.SetActive(visible);
        }
        
        public void AddCell (int index, Cell cell) {
            cells[index] = cell;
            cell.chunk = this;
            cell.transform.SetParent(transform, false);
            cell.uiTransform.SetParent(gridCanvas.transform, false);
        }
        
        private void TriangulateCells() {
            terrain.Clear();
            rivers.Clear();
            roads.Clear();
            for (int i = 0; i < cells.Length; ++i)
            {
                TriangulateCell(cells[i]);
            }
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
        }
        
        private void TriangulateCell(Cell cell) {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.N; ++d)
            {
                Triangulate(d, cell);
            }
        }

        private void Triangulate(HexDirection direction, Cell cell) {
            Vector3 centre = cell.Position;
            Vector3 v1 = Config.GetFirstBlendCorner(centre, direction);
            Vector3 v2 = Config.GetSecondBlendCorner(centre, direction);
            Edge5 rawBlendEdge = new Edge5(v1, v2);
            Edge5 blendEdge = Edge5.Perturb(rawBlendEdge);

            if ((int)direction < 3) TriangulateBlend(direction, cell, blendEdge);

            Vector3 v3 = Config.GetFirstJunctionCorner(centre, direction);
            Vector3 v4 = Config.GetSecondJunctionCorner(centre, direction);
            Edge3 junctionEdge = new Edge3(v3, v4);
            junctionEdge = Edge3.PerturbInRelation(junctionEdge, rawBlendEdge);

            if (cell.HasRiver) TriangulateRiverCell(direction, cell, centre, junctionEdge, blendEdge);
            else TriangulateSolidCell(direction, cell, centre, junctionEdge, blendEdge);

            if (cell.HasRoad) TriangulateRoadCell(direction, cell, centre, junctionEdge, blendEdge);
        }

        private void TriangulateBlend(HexDirection direction, Cell cell, Edge5 blendEdge) {
            Cell neighbour = cell.GetNeighbour(direction);
            if (neighbour == null) return;

            Vector3 v1 = Config.GetFirstBlendCorner(neighbour.Position, direction.Opposite());
            Vector3 v2 = Config.GetSecondBlendCorner(neighbour.Position, direction.Opposite());
            Edge5 neighbourBlendEdge = new Edge5(v1, v2);
            neighbourBlendEdge = Edge5.Reverse(Edge5.Perturb(neighbourBlendEdge));
            TriangulateBridge(direction, cell, neighbour, blendEdge, neighbourBlendEdge);

            Cell neighbour2 = cell.GetNeighbour(direction.Next());
            if ((int)direction >= 2 || neighbour2 == null) return;

            Vector3 v3 = Config.Perturb(Config.GetSecondBlendCorner(neighbour2.Position, direction.Next().Opposite()));
            TriangulateCorner(cell, neighbour, neighbour2, blendEdge.v5, neighbourBlendEdge.v5, v3);
        }

        private void TriangulateBridge(HexDirection direction, Cell cell, Cell neighbour, Edge5 cellEdge, Edge5 neighbourEdge){
            if (cell.HasRiverThroughEdge(direction)) {
                cellEdge.v3.y = cell.RiverBedY;
                neighbourEdge.v3.y = neighbour.RiverBedY;

                if (cell.outgoingRivers[(int)direction]) TriangulateStrip(Edge3.SetY(new Edge3(cellEdge), cell.RiverSurfaceY), Edge3.SetY(new Edge3(neighbourEdge), neighbour.RiverSurfaceY), 0f, 1f, 0.8f, 1f);
                else TriangulateStrip(Edge3.SetY(new Edge3(cellEdge), cell.RiverSurfaceY), Edge3.SetY(new Edge3(neighbourEdge), neighbour.RiverSurfaceY), 1f, 0f, 1f, 0.8f);
            }

            if (cell.GetElevationDifference(direction) == 1) {
                if (cell.HasRoadThroughEdge(direction)) TriangulateBridgeTerraceRoad(cell, neighbour, cellEdge, neighbourEdge);
                else TriangulateBridgeTerrace(cell, neighbour, cellEdge, neighbourEdge);
            }
            else {
                TriangulateStrip(cellEdge, neighbourEdge, cell.Colour, neighbour.Colour);
                if (cell.HasRoadThroughEdge(direction)) TriangulateStrip(new Edge3(cellEdge), new Edge3(neighbourEdge));
            }
        }

        private void TriangulateBridgeTerrace(Cell cell, Cell neighbour, Edge5 cellEdge, Edge5 neighbourEdge){
            for (int i = 0; i < Config.TerraceSteps; ++i) {
                Edge5 e1 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i);
                Edge5 e2 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i + 1);
                Color c1 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i);
                Color c2 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i + 1);

                TriangulateStrip(e1, e2, c1, c2);
            }
        }

        private void TriangulateBridgeTerraceRoad(Cell cell, Cell neighbour, Edge5 cellEdge, Edge5 neighbourEdge) {
            float ratioXZ = 1 / (Config.TerraceSteps / 2);
            float ratioY = 1 / ((Config.TerraceSteps / 2) + 1);

            for (int i = 0; i < Config.TerraceSteps; ++i) {
                Edge5 e1 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i);
                Edge5 e2 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i + 1);
                Color c1 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i);
                Color c2 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i + 1);

                terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                terrain.AddQuadColour(c1, c2);
                terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                terrain.AddQuadColour(c1, c2);

                if (i < Config.TerraceSteps - 2) {
                    Edge5 e3 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i + 2);
                    Edge5 e4 = Edge5.TerraceLerp(cellEdge, neighbourEdge, i + 3);
                    Color c3 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i + 2);
                    Color c4 = Edge.TerraceColourLerp(cell.Colour, neighbour.Colour, i + 3);

                    terrain.AddQuad(e1.v2, e2.v2, e3.v2, e4.v2);
                    terrain.AddQuadColour(c1, c2, c3, c4);
                    terrain.AddQuad(e1.v4, e2.v4, e3.v4, e4.v4);
                    terrain.AddQuadColour(c1, c2, c3, c4);
                }
            }

            TriangulateStrip(new Edge3(cellEdge), new Edge3(neighbourEdge));
        }

        private void TriangulateCorner(Cell cell, Cell neighbour, Cell neighbour2, Vector3 cellCorner, Vector3 neighbourCorner, Vector3 neighbour2Corner) {
            int[] inclines = new int[3]{
                Mathf.Abs(cell.Elevation - neighbour.Elevation),
                Mathf.Abs(neighbour.Elevation - neighbour2.Elevation),
                Mathf.Abs(neighbour2.Elevation - cell.Elevation)
            };

            int slopes = inclines.Count(x => Mathf.Abs(x) == 1);
            int flat = inclines.Count(x => Mathf.Abs(x) == 0);

            if (slopes == 2) {
                // Triangle Terrace
                if (flat == 1) {
                    if (inclines[0] == 0) TriangulateCornerTerrace(neighbour2Corner, cellCorner, neighbourCorner, neighbour2.Colour, cell.Colour, neighbour.Colour);
                    else if (inclines[1] == 0) TriangulateCornerTerrace(cellCorner, neighbourCorner, neighbour2Corner, cell.Colour, neighbour.Colour, neighbour2.Colour);
                    else TriangulateCornerTerrace(neighbourCorner, neighbour2Corner, cellCorner, neighbour.Colour, neighbour2.Colour, cell.Colour);
                }
                // Double Triangle Terrace
                else {
                    if (inclines[0] != 1) TriangulateDoubleCornerTerrace(neighbourCorner, neighbour2Corner, cellCorner, neighbour.Colour, neighbour2.Colour, cell.Colour);
                    else if (inclines[1] != 1) TriangulateDoubleCornerTerrace(neighbour2Corner, cellCorner, neighbourCorner, neighbour2.Colour, cell.Colour, neighbour.Colour);
                    else TriangulateDoubleCornerTerrace(cellCorner, neighbourCorner, neighbour2Corner, cell.Colour, neighbour.Colour, neighbour2.Colour);
                }
            }
            else if (slopes == 1) {
                // Half Triangle Terrace
                if (inclines[0] == 1 && inclines[2] > inclines[1]) TriangulateHalfCornerTerrace(cellCorner, neighbourCorner, neighbour2Corner, cell.Colour, neighbour.Colour, neighbour2.Colour, true);
                else if (inclines[1] == 1 && inclines[2] > inclines[0]) TriangulateHalfCornerTerrace(cellCorner, neighbourCorner, neighbour2Corner, cell.Colour, neighbour.Colour, neighbour2.Colour, false);
                else if (inclines[1] == 1 && inclines[0] > inclines[2]) TriangulateHalfCornerTerrace(neighbourCorner, neighbour2Corner, cellCorner, neighbour.Colour, neighbour2.Colour, cell.Colour, true);
                else if (inclines[2] == 1 && inclines[0] > inclines[1]) TriangulateHalfCornerTerrace(neighbourCorner, neighbour2Corner, cellCorner, neighbour.Colour, neighbour2.Colour, cell.Colour, false);
                else if (inclines[2] == 1 && inclines[1] > inclines[0]) TriangulateHalfCornerTerrace(neighbour2Corner, cellCorner, neighbourCorner, neighbour2.Colour, cell.Colour, neighbour.Colour, true);
                else TriangulateHalfCornerTerrace(neighbour2Corner, cellCorner, neighbourCorner, neighbour2.Colour, cell.Colour, neighbour.Colour, false);
            }
            else{
                // Flat Triangle
                terrain.AddTriangle(cellCorner, neighbourCorner, neighbour2Corner);
                terrain.AddTriangleColour(cell.Colour, neighbour.Colour, neighbour2.Colour);
            }
        }

        private void TriangulateCornerTerrace(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3) {
            Vector3 v4 = Edge.TerraceLerp(v1, v2, 1);
            Vector3 v5 = Edge.TerraceLerp(v1, v3, 1);

            Color c4 = Edge.TerraceColourLerp(c1, c2, 1);
            Color c5 = Edge.TerraceColourLerp(c1, c3, 1);

            terrain.AddTriangle(v1, v4, v5);
            terrain.AddTriangleColour(c1, c4, c5);

            for (int i = 1; i < Config.TerraceSteps; ++i)
            {
                Vector3 tv1 = Edge.TerraceLerp(v1, v2, i);
                Vector3 tv2 = Edge.TerraceLerp(v1, v3, i);
                Vector3 tv3 = Edge.TerraceLerp(v1, v2, i + 1);
                Vector3 tv4 = Edge.TerraceLerp(v1, v3, i + 1);

                Color tc1 = Edge.TerraceColourLerp(c1, c2, i);
                Color tc2 = Edge.TerraceColourLerp(c1, c3, i);
                Color tc3 = Edge.TerraceColourLerp(c1, c2, i + 1);
                Color tc4 = Edge.TerraceColourLerp(c1, c3, i + 1);
                
                terrain.AddQuad(tv1, tv2, tv3, tv4);
                terrain.AddQuadColour(tc1, tc2, tc3, tc4);
            }
        }

        private void TriangulateDoubleCornerTerrace(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3) {
            Vector3 m = Vector3.Lerp(v1, v3, 0.5f);
            Color mc = Color.Lerp(c1, c3, 0.5f);
            TriangulateTerraceFan(v1, v2, m, c1, c2, mc);
            TriangulateTerraceFan(v2, v3, m, c2, c3, mc);
        }

        private void TriangulateTerraceFan(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3) {
            for (int i = 0; i < Config.TerraceSteps; ++i) {
                Vector3 tv1 = Edge.TerraceLerp(v1, v2, i);
                Vector3 tv2 = Edge.TerraceLerp(v1, v2, i + 1);

                Color tc1 = Edge.TerraceColourLerp(c1, c2, i);
                Color tc2 = Edge.TerraceColourLerp(c1, c2, i + 1);

                terrain.AddTriangle(tv1, tv2, v3);
                terrain.AddTriangleColour(tc1, tc2, c3);
            }
        }

        private void TriangulateHalfCornerTerrace(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3, bool bottomTerrace) {
            float t = (v2.y - v1.y) / (v3.y - v1.y);
            Vector3 m = Vector3.Lerp(v1, v3, t);
            Color mc = Color.Lerp(c1, c3, t);
            if (bottomTerrace) {
                TriangulateTerraceFan(v1, v2, m, c1, c2, mc);
                terrain.AddTriangle(v2, v3, m);
                terrain.AddTriangleColour(c2, c3, mc);
            }
            else {
                TriangulateTerraceFan(v2, v3, m, c2, c3, mc);
                terrain.AddTriangle(v1, v2, m);
                terrain.AddTriangleColour(c1, c2, mc);
            }
        }

        private void TriangulateRiverCell(HexDirection direction, Cell cell, Vector3 centre, Edge3 junctionEdge, Edge5 blendEdge) {
            if (cell.HasRiverThroughEdge(direction)) {
                centre.y = cell.RiverBedY;
                blendEdge.v3.y = cell.RiverBedY;
                junctionEdge.v2.y = cell.RiverBedY;
                TriangulateStrip(junctionEdge, blendEdge, cell.Colour, cell.Colour);
                TriangulateFan(centre, junctionEdge, cell.Colour);

                centre.y = cell.RiverSurfaceY;
                if (cell.outgoingRivers[(int)direction]) TriangulateStrip(Edge3.SetY(junctionEdge, cell.RiverSurfaceY), Edge3.SetY(new Edge3(blendEdge), cell.RiverSurfaceY), 0f, 1f, 0.6f, 0.8f);
                else TriangulateStrip(Edge3.SetY(junctionEdge, cell.RiverSurfaceY), Edge3.SetY(new Edge3(blendEdge), cell.RiverSurfaceY), 1f, 0f, 0.2f, 0f);
                if (!cell.HasRiverEnd) TriangulateRiverTriangleLarge(direction, cell, centre, Edge3.SetY(junctionEdge, cell.RiverSurfaceY));
            }
            else if (cell.HasRiverEnd){
                centre.y = cell.RiverBedY;
                TriangulateStrip(junctionEdge, blendEdge, cell.Colour, cell.Colour);
                TriangulateFan(centre, junctionEdge, cell.Colour);

                // Triangulate river mesh here
            }
            else {
                TriangulateStrip(junctionEdge, blendEdge, cell.Colour, cell.Colour);

                int previousRiver = -1;
                int nextRiver = -1;

                for (int pDir = Config.Mod((int)direction - 1, 6); pDir != (int)direction; pDir = Config.Mod(pDir - 1, 6))
                {
                    if (cell.HasRiverThroughEdge((HexDirection)pDir))
                    {
                        previousRiver = pDir;
                        break;
                    }
                }

                for (int nDir = Config.Mod((int)direction + 1, 6); nDir != (int)direction; nDir = Config.Mod(nDir + 1, 6))
                {
                    if (cell.HasRiverThroughEdge((HexDirection)nDir))
                    {
                        nextRiver = nDir;
                        break;
                    }
                }

                Vector3 m1 = Vector3.Lerp(centre, junctionEdge.v1, 0.5f);
                Vector3 m2 = Vector3.Lerp(centre, junctionEdge.v3, 0.5f);
                centre.y = cell.RiverBedY;

                if ((int)direction.Previous() == previousRiver && (int)direction.Next() == nextRiver) {
                    TriangulateFan(centre, junctionEdge, cell.Colour);

                    centre.y = cell.RiverSurfaceY;
                    TriangulateRiverTriangleLarge(direction, cell, centre, Edge3.SetY(junctionEdge, cell.RiverSurfaceY));
                }
                else if ((int)direction.Previous() == previousRiver) {
                    TriangulateFan(m2, junctionEdge, cell.Colour);
                    terrain.AddTriangle(centre, junctionEdge.v1, m2);
                    terrain.AddTriangleColour(cell.Colour);

                    centre.y = cell.RiverSurfaceY;
                    m2.y = cell.RiverSurfaceY;
                    junctionEdge.v1.y = cell.RiverSurfaceY;
                    TriangulateRiverTriangleSmall(direction, cell, centre, junctionEdge.v1, m2);
                }
                else if ((int)direction.Next() == nextRiver) {
                    TriangulateFan(m1, junctionEdge, cell.Colour);
                    terrain.AddTriangle(centre, m1, junctionEdge.v3);
                    terrain.AddTriangleColour(cell.Colour);

                    centre.y = cell.RiverSurfaceY;
                    m1.y = cell.RiverSurfaceY;
                    junctionEdge.v3.y = cell.RiverSurfaceY;
                    TriangulateRiverTriangleSmall(direction, cell, centre, m1, junctionEdge.v3);
                }
                else {
                    terrain.AddTriangle(m1, junctionEdge.v1, junctionEdge.v2);
                    terrain.AddTriangleColour(cell.Colour);
                    terrain.AddTriangle(m1, junctionEdge.v2, m2);
                    terrain.AddTriangleColour(cell.Colour);
                    terrain.AddTriangle(m2, junctionEdge.v2, junctionEdge.v3);
                    terrain.AddTriangleColour(cell.Colour);
                    terrain.AddTriangle(centre, m1, m2);
                    terrain.AddTriangleColour(cell.Colour);

                    centre.y = cell.RiverSurfaceY;
                    m1.y = cell.RiverSurfaceY;
                    m2.y = cell.RiverSurfaceY;
                    TriangulateRiverTriangleSmall(direction, cell, centre, m1, m2);
                }
            }
        }

        private void TriangulateRiverTriangleLarge(HexDirection direction, Cell cell, Vector3 centre, Edge3 junctionEdge) {    
            HexDirection outgoingDirection = cell.GetOutGoingRiver;
            if (outgoingDirection == direction) TriangulateFan(centre, junctionEdge, 0.5f, 0f, 1f, 0.4f, 0.6f, 0.6f);
            else if (outgoingDirection == direction.Previous()) TriangulateFan(centre, junctionEdge, 0.5f, 1f, 1f, 0.4f, 0.6f, 0.4f);
            else if (outgoingDirection == direction.Previous2()) TriangulateFan(centre, junctionEdge, 0.5f, 1f, 1f, 0.4f, 0.4f, 0.2f);
            else if (outgoingDirection == direction.Opposite()) TriangulateFan(centre, junctionEdge, 0.5f, 1f, 0f, 0.4f, 0.2f, 0.2f);
            else if (outgoingDirection == direction.Next2()) TriangulateFan(centre, junctionEdge, 0.5f, 0f, 0f, 0.4f, 0.2f, 0.4f);
            else TriangulateFan(centre, junctionEdge, 0.5f, 0f, 0f, 0.4f, 0.4f, 0.6f);
        }

        private void TriangulateRiverTriangleSmall(HexDirection direction, Cell cell, Vector3 centre, Vector3 v1, Vector3 v2) {
            HexDirection outgoingDirection = cell.GetOutGoingRiver;
            if (outgoingDirection == direction.Previous()) {
                rivers.AddTriangle(centre, v1, v2);
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.6f), new Vector2(1f, 0.4f));
            }
            else if (outgoingDirection == direction.Next()) {
                rivers.AddTriangle(centre, v1, v2);
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.4f), new Vector2(0f, 0.6f));
            }
            else if (outgoingDirection == direction.Opposite()) {
                rivers.AddTriangle(centre, v1, v2);
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f));
            }
            else if (outgoingDirection == direction.Previous2()) {
                rivers.AddTriangle(centre, v1, v2);
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.4f), new Vector2(1f, 0.2f));
            }
            else if (outgoingDirection == direction.Next2()) {
                rivers.AddTriangle(centre, v1, v2);
                rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.2f), new Vector2(0f, 0.4f));
            }
        }

        private void TriangulateSolidCell(HexDirection direction, Cell cell, Vector3 centre, Edge3 junctionEdge, Edge5 blendEdge) {
            TriangulateStrip(junctionEdge, blendEdge, cell.Colour, cell.Colour);
            TriangulateFan(centre, junctionEdge, cell.Colour);
        }

        private void TriangulateRoadCell(HexDirection direction, Cell cell, Vector3 centre, Edge3 junctionEdge, Edge5 blendEdge) {
            if (cell.HasRoadThroughEdge(direction)) {
                    TriangulateStrip(junctionEdge, new Edge3(blendEdge));
                    TriangulateFan(centre, junctionEdge, 1f);
            }
            else {
                int previousRoad = -1;
                int nextRoad = -1;

                for (int pDir = Config.Mod((int)direction - 1, 6); pDir != (int)direction; pDir = Config.Mod(pDir - 1, 6))
                {
                    if (cell.HasRoadThroughEdge((HexDirection)pDir))
                    {
                        previousRoad = pDir;
                        break;
                    }
                }

                for (int nDir = Config.Mod((int)direction + 1, 6); nDir != (int)direction; nDir = Config.Mod(nDir + 1, 6))
                {
                    if (cell.HasRoadThroughEdge((HexDirection)nDir))
                    {
                        nextRoad = nDir;
                        break;
                    }
                }

                Vector3 m1 = Vector3.Lerp(centre, junctionEdge.v1, 0.5f);
                Vector3 m2 = Vector3.Lerp(centre, junctionEdge.v3, 0.5f);

                if ((int)direction.Previous() == previousRoad && (int)direction.Next() == nextRoad) TriangulateRoadWedge(centre, junctionEdge);
                else if ((int)direction.Previous() == previousRoad) TriangulateRoadTriangle(centre, junctionEdge.v1, m2);
                else if ((int)direction.Next() == nextRoad) TriangulateRoadTriangle(centre, m1, junctionEdge.v3);
                else TriangulateRoadTriangle(centre, m1, m2);
            }
        }

        private void TriangulateRoadTriangle(Vector3 centre, Vector3 v1, Vector3 v2) {
            roads.AddTriangle(centre, v1, v2);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        }

        private void TriangulateRoadWedge(Vector3 centre, Edge3 junctionEdge) {
            float mx = centre.x + junctionEdge.v1.x + junctionEdge.v3.x;
            mx /= 3;
            float mz = centre.z + junctionEdge.v1.z + junctionEdge.v3.z;
            mz /= 3;
            Vector3 m = new Vector3(mx, centre.y, mz);

            roads.AddTriangle(centre, junctionEdge.v1, m);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            roads.AddTriangle(centre, m, junctionEdge.v3);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        }

        private void TriangulateFan(Vector3 p1, Edge3 e1, Color c1) {
            terrain.AddTriangle(p1, e1.v1, e1.v2);
            terrain.AddTriangleColour(c1);
            terrain.AddTriangle(p1, e1.v2, e1.v3);
            terrain.AddTriangleColour(c1);
        }

        private void TriangulateFan(Vector3 p1, Edge3 e1, float u1, float u2, float u3, float v1, float v2, float v3) {
            float um = (v1 + v2) / 2; 
            float vm = (v2 + v3) / 2;
            rivers.AddTriangle(p1, e1.v1, e1.v2);
            rivers.AddTriangleUV(new Vector2(u1, v1), new Vector2(u2, v2), new Vector2(um, vm));
            rivers.AddTriangle(p1, e1.v2, e1.v3);
            rivers.AddTriangleUV(new Vector2(u1, v1), new Vector2(um, vm), new Vector2(u3, v3));
        }

        private void TriangulateFan(Vector3 p1, Edge3 e1, float u) {
            roads.AddTriangle(p1 + Config.RoadElevationOffset, e1.v1 + Config.RoadElevationOffset, e1.v2 + Config.RoadElevationOffset);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(u, 0f));
            roads.AddTriangle(p1 + Config.RoadElevationOffset, e1.v2 + Config.RoadElevationOffset, e1.v3 + Config.RoadElevationOffset);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(u, 0f), new Vector2(0f, 0f));
        }

        private void TriangulateFan(Vector3 p1, Edge5 e1, Color c1) {
            terrain.AddTriangle(p1, e1.v1, e1.v2);
            terrain.AddTriangleColour(c1);
            terrain.AddTriangle(p1, e1.v2, e1.v3);
            terrain.AddTriangleColour(c1);
            terrain.AddTriangle(p1, e1.v3, e1.v4);
            terrain.AddTriangleColour(c1);
            terrain.AddTriangle(p1, e1.v4, e1.v5);
            terrain.AddTriangleColour(c1);
        }

        private void TriangulateStrip(Edge3 e1, Edge3 e2, Color c1, Color c2) {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuadColour(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuadColour(c1, c2);
        }

        private void TriangulateStrip(Edge3 e1, Edge3 e2, float u1, float u2, float v1, float v2) {
            float um = (u1 + u2) / 2;
            rivers.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            rivers.AddQuadUV(new Vector2(u1, v1), new Vector2(um, v1), new Vector2(u1, v2), new Vector2(um, v2));
            rivers.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            rivers.AddQuadUV(new Vector2(um, v1), new Vector2(u2, v1), new Vector2(um, v2), new Vector2(u2, v2));
        }

        private void TriangulateStrip(Edge3 e1, Edge3 e2) {
            roads.AddQuad(e1.v1 + Config.RoadElevationOffset, e1.v2 + Config.RoadElevationOffset, e2.v1 + Config.RoadElevationOffset, e2.v2 + Config.RoadElevationOffset);
            roads.AddQuadUV(new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
            roads.AddQuad(e1.v2 + Config.RoadElevationOffset, e1.v3 + Config.RoadElevationOffset, e2.v2 + Config.RoadElevationOffset, e2.v3 + Config.RoadElevationOffset);
            roads.AddQuadUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
        }

        private void TriangulateStrip(Edge5 e1, Edge5 e2, Color c1, Color c2) {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuadColour(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuadColour(c1, c2);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuadColour(c1, c2);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            terrain.AddQuadColour(c1, c2);
        }

        private void TriangulateStrip(Edge3 e1, Edge5 e2, Color c1, Color c2) {
            terrain.AddTriangle(e1.v1, e2.v1, e2.v2);
            terrain.AddTriangleColour(c1, c2, c2);
            terrain.AddQuad(e1.v1, e1.v2, e2.v2, e2.v3);
            terrain.AddQuadColour(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v3, e2.v4);
            terrain.AddQuadColour(c1, c2);
            terrain.AddTriangle(e1.v3, e2.v4, e2.v5);
            terrain.AddTriangleColour(c1, c2, c2);
        }
    }
}
