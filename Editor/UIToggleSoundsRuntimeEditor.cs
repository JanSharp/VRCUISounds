using UdonSharpEditor;
using UnityEditor;

namespace JanSharp
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleSoundsRuntime))]
    public class UIToggleSoundsRuntimeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets, skipLine: true))
                return;
        }
    }
}
