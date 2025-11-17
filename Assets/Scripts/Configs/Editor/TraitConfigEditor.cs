using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Components;

namespace Configs.Editor {
	[CustomPropertyDrawer(typeof(TraitConfig))]
	public sealed class TraitConfigEditor : PropertyDrawer {
		private static readonly Dictionary<string, Type> _traitTypes = new();
		private static bool _typesInitialized;

		private static void InitializeTypes() {
			if (_typesInitialized) {
				return;
			}

			_traitTypes.Clear();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsValueType && type.GetCustomAttribute<TraitAttribute>() != null) {
							var typeName = type.Name;
							_traitTypes[typeName] = type;
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
			var effectProperty = property.FindPropertyRelative("_effect");

			var currentId = idProperty.stringValue;
			var traitNames = _traitTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(traitNames, currentId);

			EditorGUI.BeginChangeCheck();

			var lineHeight = EditorGUIUtility.singleLineHeight;
			var spacing = EditorGUIUtility.standardVerticalSpacing;
			var currentY = position.y;

			var dropdownRect = new Rect(position.x, currentY, position.width, lineHeight);
			var newIndex = EditorGUI.Popup(dropdownRect, "Trait Type", currentIndex, traitNames);

			if (EditorGUI.EndChangeCheck() || newIndex != currentIndex) {
				idProperty.stringValue = traitNames[newIndex];
				nameProperty.stringValue = traitNames[newIndex];
				property.serializedObject.ApplyModifiedProperties();
			}

			currentY += lineHeight + spacing;

			var nameRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(nameRect, nameProperty, new GUIContent("Name"));
			currentY += lineHeight + spacing;

			var iconRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(iconRect, iconProperty, new GUIContent("Icon"));
			currentY += lineHeight + spacing;

			var effectRect = new Rect(position.x, currentY, position.width, lineHeight);
			EditorGUI.PropertyField(effectRect, effectProperty, new GUIContent("Effect"));

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3;
		}
	}
}

