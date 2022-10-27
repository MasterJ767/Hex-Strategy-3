using UnityEngine;
using TMPro;

namespace World{
    public class Grid : MonoBehaviour
    {
        public Color defaultColour = Color.white;
        public Cell cellPrefab;
        public Chunk chunkPrefab;
        public TextMeshProUGUI cellLabelPrefab;

        private Cell[] cells;
        private Chunk[] chunks;

        private void Awake()
        {
            CreateChunks();
            CreateCells();
        }

        public void ShowUI (bool visible) {
            for (int i = 0; i < chunks.Length; i++) {
                chunks[i].ShowUI(visible);
            }
        }

        private void CreateChunks() {
            chunks = new Chunk[Config.WorldWidthInChunks * Config.WorldHeightInChunks];
            
            for (int x = 0, i = 0; x < Config.WorldWidthInChunks; ++x) 
            {
                for (int z = 0; z < Config.WorldHeightInChunks; ++z) 
                {
                    Chunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }

        private void CreateCells() {
            cells = new Cell[Config.WorldWidthInCells * Config.WorldHeightInCells];

            for (int x = 0, i = 0; x < Config.WorldWidthInCells; ++x)
            {
                for (int z = 0; z < Config.WorldHeightInCells; ++z)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i) {
            Vector3 position = new (x * Config.OuterRadius * 1.5f, 0f, (z + x * 0.5f - (int)(x / 2)) * (Config.InnerRadius * 2f));
            
            Cell cell = cells[i] = Instantiate(cellPrefab);
            cell.transform.localPosition = position;
            cell.coordinates = AxialCoordinate.FromOffsetCoordinates(x, z);
            cell.Colour = defaultColour;
            
            // Add North/South neighbour relations
            if (z > 0)
            {
                cell.SetNeighbour(HexDirection.S, cells[i - 1]);
            }
            // Add NE/SW and SE/NW neighbour relations
            if (x > 0)
            {
                if ((x & 1) == 1)
                {
                    cell.SetNeighbour(HexDirection.SW, cells[i - Config.WorldHeightInCells]);
                    if (z < Config.WorldHeightInCells - 1)
                    {
                        cell.SetNeighbour(HexDirection.NW, cells[i - Config.WorldHeightInCells + 1]);
                    }
                }
                else
                {
                    cell.SetNeighbour(HexDirection.NW, cells[i - Config.WorldHeightInCells]);
                    if (z > 0)
                    {
                        cell.SetNeighbour(HexDirection.SW, cells[i - Config.WorldHeightInCells - 1]);
                    }
                }
            }
            
            TextMeshProUGUI label = Instantiate(cellLabelPrefab);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
            
            cell.uiTransform = label.rectTransform;
            cell.Elevation = 0;
            
            AddCellToChunk(x, z, cell);
        }
        
        private void AddCellToChunk (int x, int z, Cell cell) {
            int chunkX = (int)(x / Config.ChunkWidth);
            int chunkZ = (int)(z / Config.ChunkHeight);
            Chunk chunk = chunks[chunkX * Config.WorldHeightInChunks + chunkZ];
            
            int localX = x - chunkX * Config.ChunkWidth;
            int localZ = z - chunkZ * Config.ChunkHeight;
            chunk.AddCell(localX * Config.ChunkHeight + localZ, cell);
        }
        
        public Cell GetCell (Vector3 position) {
            position = transform.InverseTransformPoint(position);
            AxialCoordinate coordinates = AxialCoordinate.FromPosition(position);
            Vector2Int rectangularCoordinates = AxialCoordinate.ToOffsetCoordinates(coordinates);
            int index = rectangularCoordinates.x * Config.WorldHeightInCells + rectangularCoordinates.y;
            return cells[index];
        }
        
        public Cell GetCell (AxialCoordinate coordinates) {
            Vector2Int rectangularCoordinates = AxialCoordinate.ToOffsetCoordinates(coordinates);
            if (rectangularCoordinates.x < 0 || rectangularCoordinates.x >= Config.WorldWidthInCells) return null;
            if (rectangularCoordinates.y < 0 || rectangularCoordinates.y >= Config.WorldHeightInCells) return null;
            int index = rectangularCoordinates.x * Config.WorldHeightInCells + rectangularCoordinates.y;
            return cells[index];
        }
    }
}