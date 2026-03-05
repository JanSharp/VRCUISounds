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
            int applierIndex = System.Array.IndexOf(components, component);
            if (targetIndex < 0 || applierIndex < 0)
                throw new System.Exception("[UISounds] Impossible.");

            for (int i = 0; i < applierIndex - targetIndex - 1; i++)
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
        }

        public static bool ContextMenuRemoveSoundsValidation<T>(MenuCommand menuCommand)
            where T : Component
        {
            Component target = (Component)menuCommand.context;
            return target.TryGetComponent<T>(out _);
        }
    }
}
