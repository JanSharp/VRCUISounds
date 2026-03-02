using UdonSharp;
using UnityEngine;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [CustomRaisedEventsDispatcher(typeof(UISoundsEventAttribute), typeof(UISoundsEventType))]
    public class UISoundsManager : UISoundsManagerAPI
    {
        // [SerializeField] private UISoundsAudioSource[] allSources;
        // private UISoundsAudioSource[] inactiveSources = new UISoundsAudioSource[ArrList.MinCapacity];
        // private int inactiveSourcesCount = 0;
        // private UISoundsAudioSource[] activeSources = new UISoundsAudioSource[ArrList.MinCapacity];
        // private int activeSourcesCount = 0;

        // private bool isInitialized = false;

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

        // private void Start() => Initialize();

        // private void Initialize()
        // {
        //     if (isInitialized)
        //         return;
        //     isInitialized = true;
        //     ArrList.AddRange(ref inactiveSources, ref inactiveSourcesCount, allSources);
        //     for (int i = 0; i < inactiveSourcesCount; i++)
        //         inactiveSources[i].index = i;
        // }

        public override void PlaySound(AudioClip audioClip, float volume)
        {
            if (muted)
                return;
            audioSource.PlayOneShot(audioClip, volume);
            // Initialize();
            // UISoundsAudioSource source = GetSourceToPlaySoundWith();
            // source.PlaySound(audioClip, volume);
        }

        // private UISoundsAudioSource GetSourceToPlaySoundWith()
        // {
        //     if (inactiveSourcesCount != 0)
        //         return inactiveSources[inactiveSourcesCount - 1];
        //     float smallestRemainder = float.PositiveInfinity;
        //     UISoundsAudioSource bestSource = null;
        //     for (int i = activeSourcesCount - 1; i >= 0; i--)
        //     {
        //         UISoundsAudioSource source = activeSources[i];
        //         float remainder = source.GetRemainingActiveTime();
        //         if (remainder >= smallestRemainder)
        //             continue;
        //         smallestRemainder = remainder;
        //         bestSource = source;
        //     }
        //     return bestSource;
        // }

        // public void BecomeActive(UISoundsAudioSource source)
        // {
        //     int index = source.index;
        //     if (index != (--inactiveSourcesCount))
        //     {
        //         UISoundsAudioSource top = inactiveSources[inactiveSourcesCount];
        //         inactiveSources[index] = top;
        //         top.index = index;
        //     }
        //     source.index = activeSourcesCount;
        //     ArrList.Add(ref activeSources, ref activeSourcesCount, source);
        // }

        // public void BecomeInactive(UISoundsAudioSource source)
        // {
        //     int index = source.index;
        //     if (index != (--activeSourcesCount))
        //     {
        //         UISoundsAudioSource top = activeSources[activeSourcesCount];
        //         activeSources[index] = top;
        //         top.index = index;
        //     }
        //     source.index = inactiveSourcesCount;
        //     ArrList.Add(ref inactiveSources, ref inactiveSourcesCount, source);
        // }

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
