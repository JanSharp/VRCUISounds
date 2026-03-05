using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIButtonSoundsOnBuild
    {
        static UIButtonSoundsOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<UIButtonSounds>(OnBuild, order: -990);
            OnBuildUtil.RegisterTypeCumulative<Button>(OnButtonBuild, order: -980);
        }

        private static bool OnBuild(IEnumerable<UIButtonSounds> buttons)
        {
            bool result = true;
            foreach (UIButtonSounds buttonSounds in buttons)
                result &= OnBuild(buttonSounds);
            return result;
        }

        private static bool OnBuild(UIButtonSounds buttonSounds)
        {
            if (!UISoundsRootUtil.TryGetRootOrError(buttonSounds, out UISoundsRoot root))
                return false;
            if (!UISoundDefinitionGroupOnBuild.TryGetDefByNameOrError(root.definitionGroup, buttonSounds.definitionName, buttonSounds, out UISoundDefinition def))
                return false;

            UISoundsListener listener = UISoundsManagerOnBuild.GetListenerForDef(def);
            UISoundsListenerUtil.SetPersistentListener(buttonSounds.GetComponent<Button>(), "m_OnClick", listener);

            return true;
        }

        private static bool OnButtonBuild(IEnumerable<Button> buttons)
        {
            foreach (Button button in buttons)
                if (!button.TryGetComponent<UIButtonSounds>(out _))
                    UISoundsListenerUtil.SetPersistentListener(button, "m_OnClick", listener: null);
            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIButtonSounds))]
    public class UIButtonSoundsEditor : Editor
    {
        [MenuItem("CONTEXT/" + nameof(Button) + "/Add UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool AddSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuAddSoundsValidation<UIButtonSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Button) + "/Add UI Sounds", secondaryPriority = 10)]
        public static void AddSounds(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuAddSounds<UIButtonSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Button) + "/Remove UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool RemoveSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuRemoveSoundsValidation<UIButtonSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Button) + "/Remove UI Sounds", secondaryPriority = 10)]
        public static void RemoveSounds(MenuCommand menuCommand)
        {
            Button target = (Button)menuCommand.context;
            RemoveSounds(target, target.GetComponent<UIButtonSounds>());
        }

        private static void RemoveSounds(Button button, UIButtonSounds buttonSounds)
        {
            UISoundsListenerUtil.SetPersistentListener(button, "m_OnClick", listener: null);
            Undo.DestroyObjectImmediate(buttonSounds);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove UI Sounds"))
                foreach (UIButtonSounds buttonSounds in targets.Cast<UIButtonSounds>())
                    RemoveSounds(buttonSounds.GetComponent<Button>(), buttonSounds);
        }
    }
}
