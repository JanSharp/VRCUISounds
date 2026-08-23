using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    public static class UISoundDefinitionGroupOnBuild
    {
        private static Dictionary<UISoundDefinitionGroup, Dictionary<string, UISoundDefinition>> defsByNameByGroup = new();

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterAction(OnPreBuild, order: -1001);
            OnBuildUtil.RegisterTypeCumulative<UISoundDefinitionGroup>(OnBuild, order: -1000, includeEditorOnly: true);
        }

        private static bool OnPreBuild()
        {
            defsByNameByGroup.Clear();
            return true;
        }

        private static bool OnBuild(IEnumerable<UISoundDefinitionGroup> defGroups)
        {
            bool result = true;
            foreach (UISoundDefinitionGroup defGroup in defGroups)
                result &= OnBuild(defGroup);
            return result;
        }

        public static bool IsDefinitionActive(UISoundDefinition def) => def.gameObject.activeSelf;

        private static bool OnBuild(UISoundDefinitionGroup defGroup)
        {
            Dictionary<string, UISoundDefinition> defsByName = new();
            bool result = true;

            foreach (UISoundDefinition def in defGroup.GetComponentsInChildren<UISoundDefinition>(includeInactive: true))
            {
                if (!IsDefinitionActive(def))
                    continue;
                if (defsByName.ContainsKey(def.name))
                {
                    Debug.LogError($"[UISounds] Multiple UI Sound Definition with the name '{def.name}' in one group.", def);
                    result = false;
                    continue;
                }
                if (def.audioClip == null && !def.silenced)
                {
                    Debug.LogError($"[UISounds] Audio Clip must not be null on non Silenced UI Sound Definitions, see '{def.name}'.", def);
                    result = false;
                    continue;
                }
                defsByName.Add(def.name, def);
            }

            defsByNameByGroup.Add(defGroup, defsByName);
            return result;
        }

        public static bool TryGetDefByName(UISoundDefinitionGroup group, string definitionName, out UISoundDefinition def)
        {
            if (string.IsNullOrEmpty(definitionName))
            {
                def = null;
                return true;
            }
            return defsByNameByGroup[group].TryGetValue(definitionName, out def);
        }

        public static bool TryGetDefByNameOrError(UISoundDefinitionGroup group, string definitionName, Component requestingComponent, out UISoundDefinition def)
        {
            if (TryGetDefByName(group, definitionName, out def))
                return true;
            Debug.LogError($"[UISounds] {requestingComponent.GetType().Name} '{requestingComponent.name}' is trying to reference a "
                + $"non existent UI Sound Definition by the name '{definitionName}'.", requestingComponent);
            return false;
        }
    }
}
