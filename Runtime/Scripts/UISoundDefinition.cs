using UnityEngine;

namespace JanSharp
{
    [DisallowMultipleComponent]
    public class UISoundDefinition : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public bool silenced = false;
        public AudioClip audioClip = null;
        [Range(0f, 1f)]
        public float volume = 0.75f;
    }
}
