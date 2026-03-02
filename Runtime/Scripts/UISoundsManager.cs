using UdonSharp;
using UnityEngine;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [CustomRaisedEventsDispatcher(typeof(UISoundsEventAttribute), typeof(UISoundsEventType))]
    public class UISoundsManager : UISoundsManagerAPI
    {
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private bool muted = false;
        public override bool Muted
        {
            get => muted;
            set
            {
                if (muted == value)
                    return;
                muted = value;
                RaiseOnUISoundsMutedChanged();
            }
        }

        public override float Volume
        {
            get => audioSource.volume;
            set
            {
                if (audioSource.volume == value)
                    return;
                audioSource.volume = value;
                RaiseOnUISoundsVolumeChanged();
            }
        }

        public override void PlaySound(AudioClip audioClip, float volume)
        {
            if (muted)
                return;
            audioSource.PlayOneShot(audioClip, volume);
        }

        #region EventDispatcher

        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onUISoundsMutedChangedListeners;
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onUISoundsVolumeChangedListeners;

        private void RaiseOnUISoundsMutedChanged()
        {
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onUISoundsMutedChangedListeners, nameof(UISoundsEventType.OnUISoundsMutedChanged));
        }

        private void RaiseOnUISoundsVolumeChanged()
        {
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onUISoundsVolumeChangedListeners, nameof(UISoundsEventType.OnUISoundsVolumeChanged));
        }

        #endregion
    }
}
