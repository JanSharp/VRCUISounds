using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public class UIButtonSounds : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public string definitionName;
    }
}
