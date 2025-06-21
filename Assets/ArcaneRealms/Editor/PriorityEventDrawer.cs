using ArcaneRealms.Scripts.Utils.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArcaneRealms.Editor
{
    [CustomPropertyDrawer(typeof(PriorityEvent<>))]
    public class PriorityEventDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            
            return base.CreatePropertyGUI(property);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty listProperty = property.FindPropertyRelative("list");

            EditorGUI.PropertyField(position, listProperty, true);

            if (GUILayout.Button("Add Priority Callback"))
            {
                property.serializedObject.Update();
                int index = listProperty.arraySize;
                listProperty.InsertArrayElementAtIndex(index);
                SerializedProperty newElement = listProperty.GetArrayElementAtIndex(index);
                newElement.FindPropertyRelative("priority").intValue = 0;
                newElement.FindPropertyRelative("callBack").objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}