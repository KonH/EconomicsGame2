using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Components;

namespace Configs.Editor {
	[CustomPropertyDrawer(typeof(ItemStatConfig))]
	public sealed class ItemStatConfigEditor : PropertyDrawer {
		private static readonly Dictionary<string, Type> _itemStatTypes = new();
		private static readonly Dictionary<string, FieldInfo[]> _floatFieldsCache = new();
		private static bool _typesInitialized;

		private static void InitializeTypes() {
			if (_typesInitialized) {
				return;
			}

			_itemStatTypes.Clear();
			_floatFieldsCache.Clear();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsValueType && type.GetCustomAttribute<ItemStatAttribute>() != null) {
							var typeName = type.Name;
							_itemStatTypes[typeName] = type;

							var floatFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
								.Where(field => field.FieldType == typeof(float))
								.ToArray();
							_floatFieldsCache[typeName] = floatFields;
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
			var floatArgumentsProperty = property.FindPropertyRelative("_floatArguments");

			var currentTypeName = typeNameProperty.stringValue;
			var typeNames = _itemStatTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(typeNames, currentTypeName);

			EditorGUI.BeginChangeCheck();

			var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			var newIndex = EditorGUI.Popup(dropdownRect, "Type", currentIndex, typeNames);

			if (EditorGUI.EndChangeCheck() || newIndex != currentIndex) {
				typeNameProperty.stringValue = typeNames[newIndex];
				floatArgumentsProperty.ClearArray();
				property.serializedObject.ApplyModifiedProperties();
			}

			if (newIndex >= 0 && newIndex < typeNames.Length) {
				var selectedTypeName = typeNames[newIndex];
				if (_floatFieldsCache.TryGetValue(selectedTypeName, out var floatFields) && floatFields.Length > 0) {
					var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

					EditorGUI.BeginChangeCheck();

					for (var i = 0; i < floatFields.Length; i++) {
						var field = floatFields[i];
						var fieldRect = new Rect(position.x, position.y + yOffset + i * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width, EditorGUIUtility.singleLineHeight);

						var currentValue = i < floatArgumentsProperty.arraySize ? floatArgumentsProperty.GetArrayElementAtIndex(i).floatValue : 0f;
						var newValue = EditorGUI.FloatField(fieldRect, field.Name, currentValue);

						if (Math.Abs(newValue - currentValue) > float.Epsilon) {
							if (i >= floatArgumentsProperty.arraySize) {
								floatArgumentsProperty.arraySize = i + 1;
							}
							floatArgumentsProperty.GetArrayElementAtIndex(i).floatValue = newValue;
						}
					}

					if (EditorGUI.EndChangeCheck()) {
						property.serializedObject.ApplyModifiedProperties();
					}
				}
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			InitializeTypes();

			var typeNameProperty = property.FindPropertyRelative("_typeName");
			var currentTypeName = typeNameProperty.stringValue;
			var typeNames = _itemStatTypes.Keys.ToArray();
			var currentIndex = Array.IndexOf(typeNames, currentTypeName);

			if (currentIndex >= 0 && currentIndex < typeNames.Length) {
				var selectedTypeName = typeNames[currentIndex];
				if (_floatFieldsCache.TryGetValue(selectedTypeName, out var floatFields)) {
					return EditorGUIUtility.singleLineHeight + (floatFields.Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
				}
			}

			return EditorGUIUtility.singleLineHeight;
		}
	}
}
