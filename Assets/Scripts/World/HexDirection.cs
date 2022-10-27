using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World {
    public enum HexDirection
    {
        NE,
        SE,
        S,
        SW,
        NW,
        N
    }

    public static class HexDirectionExtensions {
        public static HexDirection Opposite (this HexDirection direction) {
            return (HexDirection)Config.Mod((int)direction + 3, 6);
        }

        public static HexDirection Previous (this HexDirection direction) {
            return (HexDirection)Config.Mod((int)direction - 1, 6);
        }

        public static HexDirection Next (this HexDirection direction) {
            return (HexDirection)Config.Mod((int)direction + 1, 6);
        }

        public static HexDirection Previous2 (this HexDirection direction) {
            return (HexDirection)Config.Mod((int)direction - 2, 6);
        }

        public static HexDirection Next2 (this HexDirection direction) {
            return (HexDirection)Config.Mod((int)direction + 2, 6);
        }
    }
}