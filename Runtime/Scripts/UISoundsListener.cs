using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UISoundsListener : UdonSharpBehaviour
    {
        [SerializeField] private UISoundsManagerAPI uiSoundsManager;
        [SerializeField] private AudioClip audioClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume;

        public void OnUIEvent() => uiSoundsManager.PlaySound(audioClip, volume);
    }
}
