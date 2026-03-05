using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    public static class UISoundsEditorUtil
    {
        public static bool ContextMenuAddSoundsValidation<T>(MenuCommand menuCommand)
            where T : Component
        {
            Component target = (Component)menuCommand.context;
            return !target.TryGetComponent<T>(out _);
        }

        public static void ContextMenuAddSounds<T>(MenuCommand menuCommand)
            where T : Component
        {
            // Copy paste from the UI Styling package.

            Component target = (Component)menuCommand.context;
            T component = target.gameObject.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(component, $"Add {typeof(T).Name}");

            Component[] components = target.gameObject.GetComponents<Component>();
            int targetIndex = System.Array.IndexOf(components, target);
            int componentIndex = System.Array.IndexOf(components, component);
            if (targetIndex < 0 || componentIndex < 0)
                throw new System.Exception("[UISounds] Impossible.");

            for (int i = componentIndex - 1; i > targetIndex; i--)
                if (components[i] == null // A missing script, very most likely if not guaranteed to be shown in inspector.
                    || (components[i].hideFlags & HideFlags.HideInInspector) == 0)
                {
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
                }
        }

        public static bool ContextMenuRemoveSoundsValidation<T>(MenuCommand menuCommand)
            where T : Component
        {
            Component target = (Component)menuCommand.context;
            return target.TryGetComponent<T>(out _);
        }

        public static bool TryGetRoot(IEnumerable<Component> targets, out UISoundsRoot root, out string errorMsg)
        {
            root = null;
            UISoundsRoot[] roots = targets
                .Select(p => p.GetComponentInParent<UISoundsRoot>(includeInactive: true))
                .ToArray();
            if (roots.All(c => c == null))
            {
                errorMsg = "Missing UI Sounds Root in parents.";
                return false;
            }
            if (roots.Any(c => c == null))
            {
                errorMsg = "Some selected objects are not a child of any UI Sounds Root.";
                return false;
            }
            if (roots.Distinct().Count() != 1)
            {
                errorMsg = "Selected objects are children of different UI Sounds Roots.";
                return false;
            }
            root = roots[0];
            errorMsg = null;
            return true;
        }

        public static bool TryGetDefGroup(UISoundsRoot root, out UISoundDefinitionGroup defGroup, out string errorMsg)
        {
            defGroup = root.definitionGroup;
            if (defGroup == null)
            {
                errorMsg = "The UI Sounds Root is missing the reference to a UI Sound Definition Group.";
                return false;
            }
            errorMsg = null;
            return true;
        }

        public static bool TryGetContainerFromRoots(IEnumerable<Component> targets, out UISoundDefinitionGroup defGroup, out string errorMsg)
        {
            defGroup = null;
            UISoundDefinitionGroup[] defGroups = targets
                .Select(p => p.GetComponentInParent<UISoundsRoot>(includeInactive: true)?.definitionGroup)
                .ToArray();
            if (defGroups.All(c => c == null))
            {
                errorMsg = "Missing UI Sounds Root in parents.";
                return false;
            }
            if (defGroups.Any(c => c == null))
            {
                errorMsg = "Some selected objects are not a child of any UI Sounds Root.";
                return false;
            }
            if (defGroups.Distinct().Count() != 1)
            {
                errorMsg = "Selected objects are children of different UI Sounds Roots referencing different "
                    + "UI Sound Definition Groups.";
                return false;
            }
            defGroup = defGroups[0];
            errorMsg = null;
            return true;
        }

        public static void GetDefinitionNames(UISoundDefinitionGroup defGroup, out string[] defNames)
        {
            defNames = defGroup.GetComponentsInChildren<UISoundDefinition>(includeInactive: true)
                .Where(def => UISoundDefinitionGroupOnBuild.IsDefinitionActive(def))
                .Select(def => def.name)
                .Distinct()
                .ToArray();
        }

        // Copy paste from the UI Styling package.
        public static void DrawSelectorFieldLayout(SerializedProperty prop, bool disabled, string[] names)
            => DrawSelectorFieldLayout(prop, disabled, names, p => EditorGUILayout.PropertyField(p));

        // Copy paste from the UI Styling package.
        public static void DrawSelectorFieldLayout(SerializedProperty prop, GUIContent label, bool disabled, string[] names)
            => DrawSelectorFieldLayout(prop, disabled, names, p => EditorGUILayout.PropertyField(p, label));

        // Copy paste from the UI Styling package.
        private static void DrawSelectorFieldLayout(SerializedProperty prop, bool disabled, string[] names, System.Action<SerializedProperty> drawProp)
        {
            using (new GUILayout.HorizontalScope())
            {
                drawProp(prop);
                using (new EditorGUI.DisabledScope(disabled))
                {
                    int index = System.Array.IndexOf(names, prop.stringValue);
                    int newIndex = EditorGUILayout.Popup(index, names, GUILayout.Width(20f));
                    if (newIndex != index)
                        prop.stringValue = names[newIndex];
                }
            }
        }

        // Copy paste from the UI Styling package.
        public static void DrawSelectorField(Rect rect, SerializedProperty prop, string errorMsg, string[] names)
            => DrawSelectorField(rect, prop, errorMsg, names, (r, p) => EditorGUI.PropertyField(r, p));

        // Copy paste from the UI Styling package.
        public static void DrawSelectorField(Rect rect, SerializedProperty prop, GUIContent label, string errorMsg, string[] names)
            => DrawSelectorField(rect, prop, errorMsg, names, (r, p) => EditorGUI.PropertyField(r, p, label));

        // Copy paste from the UI Styling package, and edited to remove the apply button and have a tooltip on the popup button.
        private static void DrawSelectorField(Rect rect, SerializedProperty prop, string errorMsg, string[] names, System.Action<Rect, SerializedProperty> drawProp)
        {
            float width = rect.width;
            rect.width -= 2f + 20f;
            drawProp(rect, prop);
            rect.width = 20f;
            rect.x += width - rect.width;
            if (errorMsg != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Button(rect, new GUIContent("", errorMsg), EditorStyles.popup); // A button to be able to have a tooltip.
                EditorGUI.EndDisabledGroup();
                return;
            }
            int index = System.Array.IndexOf(names, prop.stringValue);
            int newIndex = EditorGUI.Popup(rect, index, names);
            if (newIndex != index)
                prop.stringValue = names[newIndex];
        }
    }

    public abstract class UISoundsEditor<T> : Editor
        where T : Component
    {
        private UISoundsRoot root;
        private UISoundDefinitionGroup defGroup;
        private string[] defNames = new string[] { };
        private string errorMsg;
        protected bool IsValid => errorMsg == null;

        protected virtual void OnEnable()
        {
            if (!UISoundsEditorUtil.TryGetRoot(targets.Cast<T>(), out root, out errorMsg))
                return;
            if (!UISoundsEditorUtil.TryGetDefGroup(root, out defGroup, out errorMsg))
                return;
            UISoundsEditorUtil.GetDefinitionNames(defGroup, out defNames);
        }

        protected void DrawInvalidHeader()
        {
            if (IsValid)
                return;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                GUILayout.Label(errorMsg, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
        }

        protected void DrawDefSelectorField(SerializedProperty prop)
            => UISoundsEditorUtil.DrawSelectorFieldLayout(prop, !IsValid, defNames);
        protected void DrawDefSelectorField(SerializedProperty prop, GUIContent label)
            => UISoundsEditorUtil.DrawSelectorFieldLayout(prop, label, !IsValid, defNames);
    }
}
