using ArcaneRealms.Scripts.Effects;
using ArcaneRealms.Scripts.SO;
using Assets.Scripts.SO;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace ArcaneRealms.Editor {

	[CustomEditor(typeof(CardInfoSO))]
	public class CardInfoSOEditor : UnityEditor.Editor {
		private SerializedProperty effectInfosProperty;
		private bool showEffectInfos = true;

		private void OnEnable() {
			effectInfosProperty = serializedObject.FindProperty("Effects");
		}

		public override void OnInspectorGUI() {
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			DrawDefaultInspector();

			EditorGUILayout.Space(20);

			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			style.fontSize = 14;
			style.alignment = TextAnchor.LowerLeft;
			showEffectInfos = EditorGUILayout.Foldout(showEffectInfos, "Card Effects", true, style);

			// display the list of card effects
			for(int i = 0; i < effectInfosProperty.arraySize; i++) {
				SerializedProperty effectInfoProperty = effectInfosProperty.GetArrayElementAtIndex(i);
				SerializedProperty parametersProperty = effectInfoProperty.FindPropertyRelative("Parameters");

				// display the effect foldout and name field
				EditorGUILayout.BeginHorizontal();

				SerializedProperty effectProperty = effectInfoProperty.FindPropertyRelative("EffectSO");

				GUIContent guiContent = effectProperty.objectReferenceValue != null ? new GUIContent(((CardEffectSO) effectProperty.objectReferenceValue).GetType().Name) : new GUIContent("Card Effect");
				EditorGUILayout.PropertyField(effectProperty, guiContent);

				EditorGUILayout.EndHorizontal();
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);

				if(effectProperty != null && effectProperty.objectReferenceValue != null) {
					List<Parameter> defaultParameters = ((CardInfoSO) target).Effects[i].effectSO.GetDefaultValueDictionary();
					// display the list of parameters for this effect


					for(int j = 0; j < defaultParameters.Count; j++) {
						if(parametersProperty.arraySize <= j) {
							parametersProperty.InsertArrayElementAtIndex(parametersProperty.arraySize);
						}
						SerializedProperty parameterProperty = parametersProperty.GetArrayElementAtIndex(j);

						// get the parameter key and value fields
						SerializedProperty valueProperty = parameterProperty.FindPropertyRelative("Value");
						SerializedProperty keyProperty = parameterProperty.FindPropertyRelative("Key");

						EditorGUILayout.BeginHorizontal();

						EditorGUILayout.LabelField(defaultParameters[j].Key);
						bool useDefaultValue = valueProperty == null || valueProperty.propertyType == SerializedPropertyType.Generic || valueProperty.stringValue == null || valueProperty.stringValue.Length == 0;
						if(useDefaultValue) {
							keyProperty.stringValue = defaultParameters[j].Key;
							valueProperty.stringValue = defaultParameters[j].Value;
							serializedObject.ApplyModifiedProperties();
							EditorUtility.SetDirty(target);
						}
						Type type = Type.GetType(defaultParameters[j].Type);

						switch(type) {
							case Type t when t == typeof(int):
								valueProperty.stringValue = EditorGUILayout.IntField(int.Parse(useDefaultValue ? defaultParameters[j].Value : valueProperty.stringValue)).ToString();
								break;
							case Type t when t == typeof(float):
								valueProperty.stringValue = EditorGUILayout.FloatField(float.Parse(valueProperty.stringValue)).ToString();
								break;
							case Type t when t == typeof(string):
								valueProperty.stringValue = EditorGUILayout.TextField(valueProperty.stringValue);
								break;
							case Type t when t == typeof(bool):
								valueProperty.stringValue = EditorGUILayout.Toggle(bool.Parse(valueProperty.stringValue)).ToString();
								break;
							/*case Type t when t == typeof(CardEffectTarget):
								CardEffectTarget targetCardEffect = (CardEffectTarget) Enum.Parse(typeof(CardEffectTarget), valueProperty.stringValue);
								targetCardEffect = (CardEffectTarget) EditorGUILayout.EnumPopup(targetCardEffect, GUILayout.MinWidth(500));
								valueProperty.stringValue = targetCardEffect.ToString();
								break;*/
							default:
								EditorGUILayout.HelpBox("Unsupported property type: " + valueProperty.propertyType, MessageType.Warning);
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

				EditorGUILayout.Space(20);

				if(deleteIndex >= 0) {
					effectInfosProperty.DeleteArrayElementAtIndex(deleteIndex);
				}
			}
			// Add card effect button
			if(GUILayout.Button("Add Card Effect")) {
				CardEffect newCardEffect = new CardEffect();
				((CardInfoSO) target).Effects.Add(newCardEffect);
			}
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}