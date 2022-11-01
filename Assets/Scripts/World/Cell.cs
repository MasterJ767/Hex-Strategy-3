using System;
using System.Linq;
using UnityEngine;

namespace World {
    public class Cell : MonoBehaviour
    {
        public AxialCoordinate coordinates;
        public RectTransform uiTransform;
        public Chunk chunk;
        [SerializeField] public Cell[] neighbours;
        [SerializeField] public bool[] incomingRivers;
        [SerializeField] public bool[] outgoingRivers;
        [SerializeField] public bool[] roads;
        
        public Vector3 Position => transform.localPosition;

        public int IncomingRiverAmount => incomingRivers.Count(x => x);
        
        public int OutgoingRiverAmount => outgoingRivers.Count(x => x);
        
        public bool HasIncomingRiver => incomingRivers.Count(x => x) > 0;
        public bool HasOutgoingRiver => outgoingRivers.Count(x => x) > 0;

        public HexDirection GetOutGoingRiver => (HexDirection)Array.IndexOf(outgoingRivers, true);

        public bool HasRiver => HasIncomingRiver || HasOutgoingRiver;

        public bool HasRiverStart => !HasIncomingRiver && HasOutgoingRiver;

        public bool HasRiverEnd => HasIncomingRiver && !HasOutgoingRiver;
        
        public bool HasRiverStartOrEnd => HasRiverStart || HasRiverEnd;

        public bool HasRiverThroughEdge(HexDirection direction) => incomingRivers[(int)direction] || outgoingRivers[(int)direction];

        public bool HasRoad => roads.Count(x => x) > 0;

        public bool HasRoadStartOrEnd => roads.Count(x => x) == 1;
        
        public bool HasRoadThroughEdge(HexDirection direction) => roads[(int)direction];

        public float RiverBedY => (elevation + Config.RiverBedElevationOffset) * Config.ElevationStep;

        public float RiverSurfaceY => (elevation + Config.RiverSurfaceElevationOffset) * Config.ElevationStep;
        
        private Color colour;
        public Color Colour
        {
            get => colour;
            set
            {
                if (colour == value) return;
                colour = value;
                RefreshNeighbours();
            }
        }

        private int elevation;
        public int Elevation
        {
            get => elevation;
            set
            {
                if (elevation == value) return;
                elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * Config.ElevationStep;
                transform.localPosition = position;
                
                Vector3 uiPosition = uiTransform.localPosition;
                uiPosition.z = -position.y;
                uiTransform.localPosition = uiPosition;

                if (HasOutgoingRiver)
                {
                    for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
                    {
                        if (outgoingRivers[(int)direction] && elevation < GetNeighbour(direction).elevation)
                        {
                            RemoveOutgoingRiver(direction);
                            break;
                        }
                    }
                }

                if (HasIncomingRiver)
                {
                    for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
                    {
                        if (incomingRivers[(int)direction] && elevation > GetNeighbour(direction).elevation)
                        {
                            RemoveIncomingRiver(direction);
                        }
                    }
                }

                if (HasRoad)
                {
                    for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
                    {
                        if (GetElevationDifference(direction) > 1)
                        {
                            SetRoad(direction, false);
                        }
                    }
                }
                
                RefreshNeighbours();
            }
        }

        private void Refresh() {
            chunk.Refresh();
        }

        private void RefreshNeighbours() {
            if (chunk) {
                chunk.Refresh();
                for (int i = 0; i < neighbours.Length; ++i) {
                    Cell neighbour = neighbours[i];
                    if (neighbour != null && neighbour.chunk != chunk) neighbour.chunk.Refresh();
                }
            }
        }
        
        public Cell GetNeighbour(HexDirection direction) {
            return neighbours[(int)direction];
        }
        

        public void SetNeighbour(HexDirection direction, Cell cell) {
            neighbours[(int)direction] = cell;
            cell.neighbours[(int)direction.Opposite()] = this;
        }


        public int GetElevationDifference(HexDirection direction) {
            return Mathf.Abs(elevation - GetNeighbour(direction).elevation);
        }
        

        public void RemoveRiver() {
            RemoveOutgoingRivers();
            RemoveIncomingRivers();
        }


        public void RemoveOutgoingRivers() {
            if (!HasOutgoingRiver) return;

            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
            {
                if (outgoingRivers[(int)direction])
                {
                    RemoveOutgoingRiver(direction);
                    break;
                }
            }
        }
        

        public void RemoveOutgoingRiver (HexDirection direction) {
            outgoingRivers[(int)direction] = false;

            Cell neighbour = GetNeighbour(direction);
            if (!neighbour) return;
            
            neighbour.incomingRivers[(int)direction.Opposite()] = false;
            neighbour.Refresh();
            Refresh();
        }

        public void RemoveIncomingRivers() {
            if (!HasIncomingRiver) return;

            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
            {
                RemoveIncomingRiver(direction);
            }
        }
        
        public void RemoveIncomingRiver (HexDirection direction) {
            incomingRivers[(int)direction] = false;

            Cell neighbour = GetNeighbour(direction);
            if (!neighbour) return;
            
            neighbour.outgoingRivers[(int)direction.Opposite()] = false;
            neighbour.Refresh();
            Refresh();
        }

        public void SetOutgoingRiver(HexDirection direction) {
            if (HasOutgoingRiver && outgoingRivers[(int)direction]) return;
            
            Cell neighbour = GetNeighbour(direction);
            if (!neighbour || elevation < neighbour.elevation) return;
            
            RemoveOutgoingRivers();
            if (incomingRivers[(int)direction]) RemoveIncomingRiver(direction);

            outgoingRivers[(int)direction] = true;

            neighbour.incomingRivers[(int)direction.Opposite()] = true;
            
            RemoveRoad();
        }

        public void RemoveRoad() {
            if (!HasRoad) 
            {
                RefreshNeighbours();
                return;
            }
            
            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
            {
                if (roads[(int)direction])
                {
                    SetRoad(direction, false);
                }
            }
        }

        public void AddRoad(HexDirection direction) {
            Cell neighbour = GetNeighbour(direction);
            if (GetElevationDifference(direction) > 1 || roads[(int)direction] || HasRiverThroughEdge(direction)) return;
            if (!neighbour.HasRiver && !HasRiver) {
                SetRoad(direction, true);
            }
            else if (neighbour.HasRiver && !neighbour.HasRoad && !GetNeighbour(direction).HasRiverThroughEdge(direction)) {
                SetRoad(direction, true);
                GetNeighbour(direction).SetRoad(direction, true);
            }
            
        }

        public void SetRoad(HexDirection direction, bool state) {
            roads[(int)direction] = state;
            
            Cell neighbour = GetNeighbour(direction);
            neighbour.roads[(int)direction.Opposite()] = state;
            neighbour.Refresh();
            Refresh();
        }
    }
}