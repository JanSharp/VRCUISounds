using UnityEditor;

namespace JanSharp
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UISoundDefinition))]
    public class UISoundDefinitionEditor : Editor
    {
        private SerializedObject so;
        private SerializedProperty silencedProp;
        private SerializedProperty audioClipProp;
        private SerializedProperty volumeProp;

        private void OnEnable()
        {
            so = serializedObject;
            silencedProp = so.FindProperty(nameof(UISoundDefinition.silenced));
            audioClipProp = so.FindProperty(nameof(UISoundDefinition.audioClip));
            volumeProp = so.FindProperty(nameof(UISoundDefinition.volume));
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            EditorGUILayout.PropertyField(silencedProp);
            using (new EditorGUI.DisabledGroupScope(silencedProp.boolValue))
                EditorGUILayout.PropertyField(audioClipProp);
            EditorGUILayout.PropertyField(volumeProp);
            so.ApplyModifiedProperties();
        }
    }
}
