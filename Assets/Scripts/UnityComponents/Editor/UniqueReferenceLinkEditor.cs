using UnityEditor;
using UnityComponents;
using Editor;

namespace Editor {
	[CustomEditor(typeof(UniqueReferenceLink))]
	public sealed class UniqueReferenceLinkEditor : UnityEditor.Editor {
		SerializedProperty? _useGameObjectNameAsIdProperty;
		SerializedProperty? _idProperty;
		SerializedProperty? _optionsProperty;

		void OnEnable() {
			_useGameObjectNameAsIdProperty = serializedObject.FindProperty("useGameObjectNameAsId");
			_idProperty = serializedObject.FindProperty("id");
			_optionsProperty = serializedObject.FindProperty("options");
		}

		public override void OnInspectorGUI() {
			if (!this.Validate(_useGameObjectNameAsIdProperty) || !this.Validate(_idProperty) || !this.Validate(_optionsProperty)) {
				return;
			}

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
