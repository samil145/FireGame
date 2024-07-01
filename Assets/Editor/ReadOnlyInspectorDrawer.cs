using UnityEditor;
using UnityEngine;

namespace CustomAttributes
{
    //"Should be put into "Editor" folder. If this folder is not created, create it"
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            //if (property.isArray)
            //{
            //    for (int i = 0; i < property.arraySize; i++)
            //    {
            //        SerializedProperty serializeProperty = property.GetArrayElementAtIndex(i);
            //        label.text = $"Element {i}";
            //        EditorGUI.PropertyField(position, serializeProperty, label);
            //    }
                
            //}
            //else
            //{
            //    EditorGUI.PropertyField(position, property, label);
            //}
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}

