using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Components;

namespace Configs.Editor {
	[CustomPropertyDrawer(typeof(CommonItemStatConfig))]
	public sealed class CommonItemStatConfigEditor : PropertyDrawer {
		private static readonly Dictionary<string, Type> _itemStatTypes = new();
		private static bool _typesInitialized;

		private static void InitializeTypes() {
			if (_typesInitialized) {
				return;
			}

			_itemStatTypes.Clear();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsValueType && type.GetCustomAttribute<ItemStatAttribute>() != null) {
							var typeName = type.Name;
							_itemStatTypes[typeName] = type;
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

			var typeNameProperty = property.FindPropertyRelative("_typeName");
			var iconProperty = property.FindPropertyRelative("_icon");

			var currentTypeName = typeNameProperty.stringValue;
			var typeNames = _itemStatTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(typeNames, currentTypeName);

			EditorGUI.BeginChangeCheck();

			var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			var newIndex = EditorGUI.Popup(dropdownRect, "Type", currentIndex, typeNames);

			if (EditorGUI.EndChangeCheck() || newIndex != currentIndex) {
				typeNameProperty.stringValue = typeNames[newIndex];
				property.serializedObject.ApplyModifiedProperties();
			}

			var iconRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(iconRect, iconProperty, new GUIContent("Icon"));

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}
