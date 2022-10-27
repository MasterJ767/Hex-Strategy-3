using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace World {
[CustomPropertyDrawer(typeof(AxialCoordinate))]
    public class AxialCoordinateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AxialCoordinate coordinates = new AxialCoordinate(property.FindPropertyRelative("q").intValue,property.FindPropertyRelative("r").intValue);
            
            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, coordinates.ToString());
        }
    }
}