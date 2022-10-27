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
            for (int i = 0; i < cells.Length; i++)
            {
                TriangulateCell(cells[i]);
            }
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
        }
        
        private void TriangulateCell(Cell cell) {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.N; d++)
            {
                Triangulate(d, cell);
            }
        }

        private void Triangulate(HexDirection direction, Cell cell)
        {
            Vector3 centre = cell.Position;
            Vector3 v2 = centre + Config.GetFirstBlendCorner(direction);
            Vector3 v3 = centre + Config.GetSecondBlendCorner(direction);
            terrain.AddTriangle(centre, v2, v3);
        }
    }
}
