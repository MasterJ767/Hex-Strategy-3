using UnityEngine;
using UnityEngine.EventSystems;
using World;
using Grid = World.Grid;

namespace Developer
{
    public class MapEditor : MonoBehaviour
    {
        public Color[] colours;
        public Grid grid;
        private Color activeColour;
        private int activeElevation;
        private bool applyColour;
        private bool applyElevation = true;
        private int brushSize;
        private OptionalToggle riverMode;
        private OptionalToggle roadMode;
        private bool isDrag;
        private HexDirection dragDirection;
        private Cell previousCell;

        private void Awake() {
            SelectColour(0);
        }

        private void Update() {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
            else previousCell = null;
        }

        private void HandleInput() {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                Cell currentCell = grid.GetCell(hit.point);
                if (previousCell && previousCell != currentCell) ValidateDrag(currentCell);
                else isDrag = false;
                EditCells(currentCell);
                previousCell = currentCell;
            }
            else {
                previousCell = null;
            }
        }
        
        private void ValidateDrag (Cell currentCell) {
            for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.N; ++dragDirection) 
            {
                if (previousCell.GetNeighbour(dragDirection) == currentCell) {
                    isDrag = true;
                    return;
                }
            }
            isDrag = false;
        }
        
        private void EditCells(Cell centre) {
            int centreQ = centre.coordinates.Q;
            int centreR = centre.coordinates.R;
            int centreS = centre.coordinates.S;

            for (int q = centreQ - brushSize; q <= centreQ + brushSize; ++q)
            {
                for (int r = centreR - brushSize; r <= centreR + brushSize; ++r)
                {
                    int s = -q - r;
                    if (s >= centreS - brushSize && s <= centreS + brushSize)
                    {
                        EditCell(grid.GetCell(new AxialCoordinate(q, r)));
                    }
                }
            }
        }

        private void EditCell(Cell cell) {
            if (cell)
            {
                if (applyColour) cell.Colour = activeColour;
                if (applyElevation) cell.Elevation = activeElevation;
                if (riverMode == OptionalToggle.No) cell.RemoveRiver();
                if (roadMode == OptionalToggle.No) cell.RemoveRoad();
                if (isDrag) {
                    Cell otherCell = cell.GetNeighbour(dragDirection.Opposite());
                    if (otherCell) {
                        if (riverMode == OptionalToggle.Yes)
                        {
                            otherCell.SetOutgoingRiver(dragDirection);
                        }
                        
                        if (roadMode == OptionalToggle.Yes)
                        {
                            otherCell.AddRoad(dragDirection);
                        }
                    }
                }
            }
        }
        
        public void SelectColour(int index) {
            applyColour = index >= 0;
            if (applyColour) activeColour = colours[index];
        }

        public void SetElevation(float elevation) {
            activeElevation = (int)elevation;
        }

        public void SetApplyElevation(bool toggle) {
            applyElevation = toggle;
        }
        
        public void SetBrushSize (float size) {
            brushSize = (int)size;
        }
        
        public void SetRiverMode (int mode) {
            riverMode = (OptionalToggle)mode;
        }
        
        public void SetRoadMode (int mode) {
            roadMode = (OptionalToggle)mode;
        }
        
        public void ShowUI (bool visible) {
            grid.ShowUI(visible);
        }
    }

    enum OptionalToggle {
        Ignore, 
        Yes, 
        No
    }    
}
