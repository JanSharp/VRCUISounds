using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [RequireComponent(typeof(Slider))]
    [DisallowMultipleComponent]
    public class UISliderSounds : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public string definitionName;
    }
}
