using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UISliderSoundsOnBuild
    {
        static UISliderSoundsOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<UISliderSounds>(OnBuild, order: -990);
            OnBuildUtil.RegisterTypeCumulative<Slider>(OnSliderBuild, order: -980);
        }

        private static bool OnBuild(IEnumerable<UISliderSounds> sliders)
        {
            bool result = true;
            foreach (UISliderSounds sliderSounds in sliders)
                result &= OnBuild(sliderSounds);
            return result;
        }

        private static bool OnBuild(UISliderSounds sliderSounds)
        {
            if (!UISoundsRootUtil.TryGetRootOrError(sliderSounds, out UISoundsRoot root))
                return false;
            if (!UISoundDefinitionGroupOnBuild.TryGetDefByNameOrError(root.definitionGroup, sliderSounds.definitionName, sliderSounds, out UISoundDefinition def))
                return false;

            UISoundsListener listener = UISoundsManagerOnBuild.GetListenerForDef(def);
            UISoundsListenerUtil.SetPersistentListener(sliderSounds.GetComponent<Slider>(), "m_OnValueChanged", listener);

            return true;
        }

        private static bool OnSliderBuild(IEnumerable<Slider> sliders)
        {
            foreach (Slider slider in sliders)
                if (!slider.TryGetComponent<UISliderSounds>(out _))
                    UISoundsListenerUtil.SetPersistentListener(slider, "m_OnValueChanged", listener: null);
            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UISliderSounds))]
    public class UISliderSoundsEditor : Editor
    {
        [MenuItem("CONTEXT/" + nameof(Slider) + "/Add UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool AddSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuAddSoundsValidation<UISliderSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Slider) + "/Add UI Sounds", secondaryPriority = 10)]
        public static void AddSounds(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuAddSounds<UISliderSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Slider) + "/Remove UI Sounds", isValidateFunction: true, secondaryPriority = 10)]
        public static bool RemoveSoundsValidation(MenuCommand menuCommand)
            => UISoundsEditorUtil.ContextMenuRemoveSoundsValidation<UISliderSounds>(menuCommand);

        [MenuItem("CONTEXT/" + nameof(Slider) + "/Remove UI Sounds", secondaryPriority = 10)]
        public static void RemoveSounds(MenuCommand menuCommand)
        {
            Slider target = (Slider)menuCommand.context;
            RemoveSounds(target, target.GetComponent<UISliderSounds>());
        }

        private static void RemoveSounds(Slider slider, UISliderSounds sliderSounds)
        {
            UISoundsListenerUtil.SetPersistentListener(slider, "m_OnValueChanged", listener: null);
            Undo.DestroyObjectImmediate(sliderSounds);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove UI Sounds"))
                foreach (UISliderSounds sliderSounds in targets.Cast<UISliderSounds>())
                    RemoveSounds(sliderSounds.GetComponent<Slider>(), sliderSounds);
        }
    }
}
