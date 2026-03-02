using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    public enum UISoundsEventType
    {
        OnUISoundsMutedChanged,
        OnUISoundsVolumeChanged,
    }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class UISoundsEventAttribute : CustomRaisedEventBaseAttribute
    {
        /// <summary>
        /// <para>The method this attribute gets applied to must be public.</para>
        /// <para>The name of the function this attribute is applied to must have the exact same name as the
        /// name of the <paramref name="eventType"/>.</para>
        /// <para>Event registration is performed at OnBuild, which is to say that scripts with these kinds of
        /// event handlers must exist in the scene at build time, any runtime instantiated objects with these
        /// scripts on them will not receive these events.</para>
        /// <para>Disabled scripts still receive events.</para>
        /// </summary>
        /// <param name="eventType">The event to register this function as a listener to.</param>
        public UISoundsEventAttribute(UISoundsEventType eventType)
            : base((int)eventType)
        { }
    }

    [SingletonScript("a0ed4bb18158293d5bf9863e7cafc577")] // Runtime/Prefabs/UISoundsManager.prefab
    public abstract class UISoundsManagerAPI : UdonSharpBehaviour
    {
        public abstract bool Muted { get; set; }
        public abstract float Volume { get; set; }

        public abstract void PlaySound(AudioClip audioClip, float volume);
    }
}
