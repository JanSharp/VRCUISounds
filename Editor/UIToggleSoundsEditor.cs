using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
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
}
