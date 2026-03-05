using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleSoundsOnBuild
    {
        static UIToggleSoundsOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<UIToggleSounds>(OnBuild, order: -990);
        }

        private static bool OnBuild(IEnumerable<UIToggleSounds> toggles)
        {
            bool result = true;
            foreach (UIToggleSounds toggleSounds in toggles)
                result &= OnBuild(toggleSounds);
            return result;
        }

        private static bool OnBuild(UIToggleSounds toggleSounds)
        {
            if (!UISoundsRootUtil.TryGetRootOrError(toggleSounds, out UISoundsRoot root))
                return false;
            if (!UISoundDefinitionGroupOnBuild.TryGetDefByNameOrError(root.definitionGroup, toggleSounds.turnOnDefinitionName, toggleSounds, out UISoundDefinition turnOnDef))
                return false;
            if (!UISoundDefinitionGroupOnBuild.TryGetDefByNameOrError(root.definitionGroup, toggleSounds.turnOffDefinitionName, toggleSounds, out UISoundDefinition turnOffDef))
                return false;

            UISoundsListener turnOnListener = UISoundsManagerOnBuild.GetListenerForDef(turnOnDef);
            UISoundsListener turnOffListener = UISoundsManagerOnBuild.GetListenerForDef(turnOffDef);
            UIToggleSoundsRuntime soundsRuntime = GetRuntimeComponent(toggleSounds);
            Toggle toggle = toggleSounds.GetComponent<Toggle>();

            SerializedObject soundsRuntimeSo = new(soundsRuntime);
            soundsRuntimeSo.FindProperty("toggle").objectReferenceValue = toggle;
            soundsRuntimeSo.FindProperty("onTurnOnListener").objectReferenceValue = turnOnListener;
            soundsRuntimeSo.FindProperty("onTurnOffListener").objectReferenceValue = turnOffListener;
            soundsRuntimeSo.ApplyModifiedProperties();

            UISoundsListenerUtil.SetPersistentListener(
                toggleSounds.GetComponent<Toggle>(),
                "onValueChanged",
                soundsRuntime,
                nameof(UIToggleSoundsRuntime.OnValueChanged));

            return true;
        }

        private static UIToggleSoundsRuntime GetRuntimeComponent(UIToggleSounds toggleSounds)
        {
            if (toggleSounds.TryGetComponent(out UIToggleSoundsRuntime soundsRuntime))
                return soundsRuntime;
            soundsRuntime = UdonSharpUndo.AddComponent<UIToggleSoundsRuntime>(toggleSounds.gameObject);
            OnBuildUtil.MarkForRerunDueToScriptInstantiation();
            return soundsRuntime;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleSounds))]
    public class UIToggleSoundsEditor : UISoundsEditor<UIToggleSounds>
    {
        [MenuItem("CONTEXT/" + nameof(Toggle) + "/Add UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool AddSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuAddSoundsValidation<UIToggleSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Toggle) + "/Add UI Sounds", secondaryPriority = 10)]
        public static void AddSounds(MenuCommand menuCommand)
        {
            UISoundsEditorUtil.ContextMenuAddSounds<UIToggleSounds>(menuCommand);
            Toggle target = (Toggle)menuCommand.context;
            // Just for transparency, it would get added on build anyway.
            UdonSharpUndo.AddComponent<UIToggleSoundsRuntime>(target.gameObject);
        }

        [MenuItem("CONTEXT/" + nameof(Toggle) + "/Remove UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool RemoveSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuRemoveSoundsValidation<UIToggleSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Toggle) + "/Remove UI Sounds", secondaryPriority = 10)]
        public static void RemoveSounds(MenuCommand menuCommand)
        {
            Toggle target = (Toggle)menuCommand.context;
            RemoveSounds(target, target.GetComponent<UIToggleSounds>());
        }

        private static void RemoveSounds(Toggle toggle, UIToggleSounds toggleSounds)
        {
            UISoundsListenerUtil.SetPersistentListener<UIToggleSoundsRuntime>(
                toggle,
                "onValueChanged",
                target: null,
                nameof(UIToggleSoundsRuntime.OnValueChanged));

            UIToggleSoundsRuntime soundsRuntime = toggleSounds.GetComponent<UIToggleSoundsRuntime>();
            if (soundsRuntime != null)
                UdonSharpUndo.DestroyImmediate(soundsRuntime);
            Undo.DestroyObjectImmediate(toggleSounds);
        }

        private SerializedObject so;
        private SerializedProperty turnOnDefinitionNameProp;
        private SerializedProperty turnOffDefinitionNameProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            so = serializedObject;
            turnOnDefinitionNameProp = so.FindProperty("turnOnDefinitionName");
            turnOffDefinitionNameProp = so.FindProperty("turnOffDefinitionName");
        }

        public override void OnInspectorGUI()
        {
            DrawInvalidHeader();

            so.Update();
            DrawDefSelectorField(turnOnDefinitionNameProp);
            DrawDefSelectorField(turnOffDefinitionNameProp);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove UI Sounds"))
                foreach (UIToggleSounds toggleSounds in targets.Cast<UIToggleSounds>())
                    RemoveSounds(toggleSounds.GetComponent<Toggle>(), toggleSounds);
        }
    }
}
