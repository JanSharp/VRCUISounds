using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [RequireComponent(typeof(Toggle))]
    [DisallowMultipleComponent]
    public class UIToggleSounds : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public string turnOnDefinitionName;
        public string turnOffDefinitionName;
    }
}
