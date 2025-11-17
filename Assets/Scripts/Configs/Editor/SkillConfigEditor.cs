using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Components;

namespace Configs.Editor {
	[CustomPropertyDrawer(typeof(SkillConfig))]
	public sealed class SkillConfigEditor : PropertyDrawer {
		private static readonly Dictionary<string, Type> _skillTypes = new();
		private static bool _typesInitialized;

		private static void InitializeTypes() {
			if (_typesInitialized) {
				return;
			}

			_skillTypes.Clear();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsValueType && type.GetCustomAttribute<SkillAttribute>() != null) {
							var typeName = type.Name;
							_skillTypes[typeName] = type;
						}
					}
				} catch (Exception e) {
					Debug.LogException(e);
				}
			}

			_typesInitialized = true;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			InitializeTypes();

			EditorGUI.BeginProperty(position, label, property);

			var idProperty = property.FindPropertyRelative("_id");
			var nameProperty = property.FindPropertyRelative("_name");
			var iconProperty = property.FindPropertyRelative("_icon");
			var baseEffectProperty = property.FindPropertyRelative("_baseEffect");
			var baseUseCountProperty = property.FindPropertyRelative("_baseUseCount");
			var levelEffectIncreaseProperty = property.FindPropertyRelative("_levelEffectIncrease");
			var levelUseCountIncreaseProperty = property.FindPropertyRelative("_levelUseCountIncrease");

			var currentId = idProperty.stringValue;
			var skillNames = _skillTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(skillNames, currentId);

			EditorGUI.BeginChangeCheck();

			var lineHeight = EditorGUIUtility.singleLineHeight;
			var spacing = EditorGUIUtility.standardVerticalSpacing;
			var currentY = position.y;

			var dropdownRect = new Rect(position.x, currentY, position.width, lineHeight);
			var newIndex = EditorGUI.Popup(dropdownRect, "Skill Type", currentIndex, skillNames);

			if (EditorGUI.EndChangeCheck() || newIndex != currentIndex) {
				idProperty.stringValue = skillNames[newIndex];
				nameProperty.stringValue = skillNames[newIndex];
				property.serializedObject.ApplyModifiedProperties();
			}

			currentY += lineHeight + spacing;

			var nameRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(nameRect, nameProperty, new GUIContent("Name"));
			currentY += lineHeight + spacing;

			var iconRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(iconRect, iconProperty, new GUIContent("Icon"));
			currentY += lineHeight + spacing;

			var baseEffectRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(baseEffectRect, baseEffectProperty, new GUIContent("Base Effect"));
			currentY += lineHeight + spacing;

			var baseUseCountRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(baseUseCountRect, baseUseCountProperty, new GUIContent("Base Use Count"));
			currentY += lineHeight + spacing;

			var levelEffectIncreaseRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(levelEffectIncreaseRect, levelEffectIncreaseProperty, new GUIContent("Level Effect Increase"));
			currentY += lineHeight + spacing;

			var levelUseCountIncreaseRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(levelUseCountIncreaseRect, levelUseCountIncreaseProperty, new GUIContent("Level Use Count Increase"));

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 7 + EditorGUIUtility.standardVerticalSpacing * 6;
		}
	}
}

