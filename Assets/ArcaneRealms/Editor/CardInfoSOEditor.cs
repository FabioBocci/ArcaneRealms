using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Utils;
using System;
using ArcaneRealms.Scripts.Cards.Effects;
using ArcaneRealms.Scripts.Cards.Effects.ScriptableEffects;
using ArcaneRealms.Scripts.Cards.ScriptableCards;
using UnityEditor;
using UnityEngine;


namespace ArcaneRealms.Editor {

	[CustomEditor(typeof(CardInfoSO), true)]
	public class CardInfoSOEditor : UnityEditor.Editor {
		private SerializedProperty effectInfosProperty;
		private SerializedProperty databaseProperty;
		private bool showEffectInfos = true;

		private void OnEnable() {
			effectInfosProperty = serializedObject.FindProperty("Effects");
			databaseProperty = serializedObject.FindProperty("database");
		}

		public override void OnInspectorGUI() {
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			if(databaseProperty.objectReferenceValue == null) {
				EditorGUILayout.Space(30);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(databaseProperty);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space(30);
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);
			}


			DrawDefaultInspector();
			EditorGUILayout.Space(20);

			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			style.fontSize = 14;
			style.alignment = TextAnchor.LowerLeft;
			showEffectInfos = EditorGUILayout.Foldout(showEffectInfos, "Card Effects", true, style);

			if(!showEffectInfos) {
				return;
			}

			// display the list of card effects
			for(int i = 0; i < effectInfosProperty.arraySize; i++) {
				SerializedProperty effectInfoProperty = effectInfosProperty.GetArrayElementAtIndex(i);

				// display the effect foldout and name field
				EditorGUILayout.BeginHorizontal();

				SerializedProperty effectProperty = effectInfoProperty.FindPropertyRelative("effectSO");
				SerializedProperty implementedParamProperty = effectInfoProperty.FindPropertyRelative("effectParameters");

				GUIContent guiContent = effectProperty.objectReferenceValue != null ? new GUIContent(((CardEffectSO) effectProperty.objectReferenceValue).GetType().Name) : new GUIContent("Card Effect");
				EditorGUILayout.PropertyField(effectProperty, guiContent);

				EditorGUILayout.EndHorizontal();
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);

				if(effectProperty != null && effectProperty.objectReferenceValue != null) {
					EffectParameters defaultParameters = ((CardInfoSO) target).Effects[i].effectSO.GetDefaultValueDictionary();
					EffectParameters implementedParameters = ((CardInfoSO) target).Effects[i].effectParameters;
					TargetsEnum targetType = defaultParameters.GetValueOrDefault(CardEffectSO.TARGET_PARAM_NAME, TargetsEnum.NONE);
					defaultParameters = defaultParameters.AddAll(targetType.parameters);
					for(int j = 0; j < defaultParameters.GetSize(); j++) {

						if(implementedParameters.GetSize() <= j) {
							implementedParameters.Add(new() { Key = defaultParameters[j].Key, Value = defaultParameters[j].Value, Type = defaultParameters[j].Type });
							serializedObject.ApplyModifiedProperties();
							EditorUtility.SetDirty(target);
							Debug.Log("Created new parameter. For parameter: " + defaultParameters[j].Key + " with value: " + implementedParameters[j].Value);
						}
						EditorGUILayout.BeginHorizontal();

						EditorGUILayout.LabelField(defaultParameters[j].Key);

						Type type = Type.GetType(defaultParameters[j].Type);
						switch(type) {
							case Type t when t == typeof(int):
								implementedParameters[j].Value = EditorGUILayout.IntField(int.Parse(implementedParameters[j].Value)).ToString();
								break;
							case Type t when t == typeof(float):
								implementedParameters[j].Value = EditorGUILayout.FloatField(float.Parse(implementedParameters[j].Value)).ToString();
								break;
							case Type t when t == typeof(string):
								implementedParameters[j].Value = EditorGUILayout.TextField(implementedParameters[j].Value);
								break;
							case Type t when t == typeof(bool):
								implementedParameters[j].Value = EditorGUILayout.Toggle(bool.Parse(implementedParameters[j].Value)).ToString();
								break;
							case Type t when t == typeof(TargetsEnum):
								EditorGUILayout.Space(5);
								TargetsEnum target = TargetsEnum.GetTargetType(implementedParameters[j].Value);
								if(target == null) {
									Debug.Log("TargetsEnum == null -> resetting default value: " + targetType.name);
									target = targetType;
								}
								int index = TargetsEnum.GetIndexOf(target);
								index = EditorGUILayout.Popup(index, TargetsEnum.GetTargetTypeNames().ToArray(), GUILayout.MinWidth(500));
								implementedParameters[j].Value = TargetsEnum.GetTargetTypes()[index].name;
								EditorGUILayout.Space(5);
								break;
							default:
								EditorGUILayout.HelpBox("Unsupported property type: " + defaultParameters[j].Type, MessageType.Warning);
								break;
						}


						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.Space();
				}

				int deleteIndex = -1;
				if(GUILayout.Button("Delete")) {
					deleteIndex = i;
				}
				DrawUILine(Color.gray, 2, 20);

				if(deleteIndex >= 0) {
					effectInfosProperty.DeleteArrayElementAtIndex(deleteIndex);
				}
				implementedParamProperty.serializedObject.ApplyModifiedProperties();
			}
			// Add card effect button
			if(GUILayout.Button("Add Card Effect")) {
				CardEffect newCardEffect = new CardEffect();
				((CardInfoSO) target).Effects.Add(newCardEffect);
			}
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
			//AssetDatabase.SaveAssets();
		}



		public static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}
	}
}