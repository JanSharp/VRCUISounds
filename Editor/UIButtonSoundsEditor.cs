using System.Collections.Generic;
using UnityEditor;
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
}
