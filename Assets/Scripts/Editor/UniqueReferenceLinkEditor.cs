using UnityEngine;
using UnityEditor;
using UnityComponents;

namespace Editor {
	[CustomEditor(typeof(UniqueReferenceLink))]
	public sealed class UniqueReferenceLinkEditor : UnityEditor.Editor {
		SerializedProperty _useGameObjectNameAsIdProperty = null!;
		SerializedProperty _idProperty = null!;
		SerializedProperty _optionsProperty = null!;

		void OnEnable() {
			_useGameObjectNameAsIdProperty = serializedObject.FindProperty("useGameObjectNameAsId");
			_idProperty = serializedObject.FindProperty("id");
			_optionsProperty = serializedObject.FindProperty("options");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(_useGameObjectNameAsIdProperty);

			if (!_useGameObjectNameAsIdProperty.boolValue) {
				EditorGUILayout.PropertyField(_idProperty);
			}

			EditorGUILayout.PropertyField(_optionsProperty);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
