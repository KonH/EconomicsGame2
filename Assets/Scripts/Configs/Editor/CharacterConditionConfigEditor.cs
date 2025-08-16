using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Components;

namespace Configs.Editor {
	[CustomPropertyDrawer(typeof(CharacterConditionConfig))]
	public sealed class CharacterConditionConfigEditor : PropertyDrawer {
		private static readonly Dictionary<string, Type> _conditionTypes = new();
		private static bool _typesInitialized;

		private static void InitializeTypes() {
			if (_typesInitialized) {
				return;
			}

			_conditionTypes.Clear();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsValueType && type.GetCustomAttribute<ConditionAttribute>() != null) {
							var typeName = type.Name;
							_conditionTypes[typeName] = type;
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

			var currentId = idProperty.stringValue;
			var conditionNames = _conditionTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(conditionNames, currentId);

			EditorGUI.BeginChangeCheck();

			var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			var newIndex = EditorGUI.Popup(dropdownRect, "Condition Type", currentIndex, conditionNames);

			if (EditorGUI.EndChangeCheck() || newIndex != currentIndex) {
				idProperty.stringValue = conditionNames[newIndex];
				nameProperty.stringValue = conditionNames[newIndex];
				property.serializedObject.ApplyModifiedProperties();
			}

			var nameRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(nameRect, nameProperty, new GUIContent("Name"));

			var iconRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(iconRect, iconProperty, new GUIContent("Icon"));

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
		}
	}
}
