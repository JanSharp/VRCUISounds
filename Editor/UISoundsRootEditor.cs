using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UISoundsRootOnBuild
    {
        static UISoundsRootOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<UISoundsRoot>(OnBuild, order: -1000);
        }

        private static bool OnBuild(IEnumerable<UISoundsRoot> roots)
        {
            bool result = true;
            foreach (UISoundsRoot root in roots)
                result &= OnBuild(root);
            return result;
        }

        private static bool OnBuild(UISoundsRoot root)
        {
            if (root.definitionGroup == null)
            {
                Debug.LogError($"[UISounds] UI Sounds Root '{root.name}' must have a reference to a UI Sound Definition Group.", root);
                return false;
            }
            return true;
        }
    }

    public static class UISoundsRootUtil
    {
        public static bool TryGetRootOrError(Component requestingComponent, out UISoundsRoot root)
        {
            root = requestingComponent.GetComponentInParent<UISoundsRoot>(includeInactive: true);
            if (root != null)
                return true;
            Debug.LogError($"[UISounds] {requestingComponent.GetType().Name} '{requestingComponent.name}' "
                    + $"must have a UI Sounds Root in its parents.", requestingComponent);
            return false;
        }
    }
}
