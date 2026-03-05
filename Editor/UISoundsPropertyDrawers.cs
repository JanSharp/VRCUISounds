using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [CustomPropertyDrawer(typeof(UISoundReferenceAttribute))]
    public class UISoundReferenceDrawer : PropertyDrawer
    {
        private bool isInitialized = false;
        private string errorMsg = null;
        private UISoundDefinitionGroup defGroup = null;
        private string[] defNames = new string[] { };

        private void Initialize(SerializedProperty property)
        {
            if (isInitialized)
                return;
            isInitialized = true;

            if (!UISoundsEditorUtil.TryGetContainerFromRoots(property.serializedObject.targetObjects.Cast<Component>(), out defGroup, out errorMsg))
                return;
            UISoundsEditorUtil.GetDefinitionNames(defGroup, out defNames);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            UISoundsEditorUtil.DrawSelectorField(
                position,
                property,
                defGroup != null ? null : (errorMsg ?? "UI Sound Definition Group got destroyed."),
                defNames);
        }
    }
}
