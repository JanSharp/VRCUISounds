using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UISoundsListener : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField] private UISoundsManagerAPI uiSoundsManager;
        [SerializeField] private GameObject soundDefinitionGo;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public GameObject SoundDefinitionGo => soundDefinitionGo;
#endif
        [SerializeField] private AudioClip audioClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume;

        public void OnUIEvent() => uiSoundsManager.PlaySound(audioClip, volume);
    }
}
