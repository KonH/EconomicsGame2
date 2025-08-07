using UnityEditor;
using UnityComponents;
using Editor;

namespace UnityComponents.Editor {
	[CustomEditor(typeof(UniqueReferenceLink))]
	public sealed class UniqueReferenceLinkEditor : UnityEditor.Editor {
		SerializedProperty? _useGameObjectNameAsIdProperty;
		SerializedProperty? _idProperty;

		void OnEnable() {
			_useGameObjectNameAsIdProperty = serializedObject.FindProperty("_useGameObjectNameAsId");
			_idProperty = serializedObject.FindProperty("_id");
		}

		public override void OnInspectorGUI() {
			if (!this.Validate(_useGameObjectNameAsIdProperty) || !this.Validate(_idProperty)) {
				return;
			}

			serializedObject.Update();

			EditorGUILayout.PropertyField(_useGameObjectNameAsIdProperty);

			if (!_useGameObjectNameAsIdProperty.boolValue) {
				EditorGUILayout.PropertyField(_idProperty);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
