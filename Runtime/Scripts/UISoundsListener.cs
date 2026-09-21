using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UISoundsListener : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField] private UISoundsManagerAPI uiSoundsManager;
        [SerializeField] private GameObject soundDefinitionGo;
        [SerializeField] private AudioClip audioClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public GameObject SoundDefinitionGo => soundDefinitionGo;
        public const string UiSoundsManagerPropName = nameof(uiSoundsManager);
        public const string SoundDefinitionGoPropName = nameof(soundDefinitionGo);
        public const string AudioClipPropName = nameof(audioClip);
        public const string VolumePropName = nameof(volume);
#endif

        public void OnUIEvent() => uiSoundsManager.PlaySound(audioClip, volume);
    }
}
