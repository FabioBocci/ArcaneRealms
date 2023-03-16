using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Assets.Editors {

	[CanEditMultipleObjects]
	[CustomEditor(typeof(ArchLayoutGroup))]
	public class ArchLayoutGroupEditor : HorizontalOrVerticalLayoutGroupEditor {
		private SerializedProperty archWidthProperty;
		private SerializedProperty archHeightProperty;
		private SerializedProperty archCurveProperty;
		private SerializedProperty angleOutProperty;

		protected override void OnEnable() {
			base.OnEnable();
			archWidthProperty = serializedObject.FindProperty("archWidth");
			archHeightProperty = serializedObject.FindProperty("archHeight");
			archCurveProperty = serializedObject.FindProperty("curveRadius");
			angleOutProperty = serializedObject.FindProperty("angleOut");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorGUILayout.PropertyField(archWidthProperty);
			EditorGUILayout.PropertyField(archHeightProperty);
			EditorGUILayout.PropertyField(archCurveProperty);
			EditorGUILayout.PropertyField(angleOutProperty);

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);

			if(EditorGUI.EndChangeCheck()) {
				ArchLayoutGroup arch = target as ArchLayoutGroup;
				arch.SetLayout();
			}
		}
	}
}