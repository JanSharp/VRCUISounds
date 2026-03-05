using UnityEngine;

namespace JanSharp
{
    [DisallowMultipleComponent]
    public class UISoundsRoot : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public UISoundDefinitionGroup definitionGroup;
    }
}
