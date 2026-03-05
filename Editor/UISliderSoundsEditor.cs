using System.Collections.Generic;
using UnityEditor;
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
}
