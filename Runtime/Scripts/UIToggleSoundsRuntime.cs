using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(UIToggleSounds))]
    [DisallowMultipleComponent]
    public class UIToggleSoundsRuntime : UdonSharpBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private UISoundsListener onTurnOnListener;
        [SerializeField] private UISoundsListener onTurnOffListener;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public const string TogglePropName = nameof(toggle);
        public const string OnTurnOnListenerPropName = nameof(onTurnOnListener);
        public const string OnTurnOffListenerPropName = nameof(onTurnOffListener);
#endif

        public void OnValueChanged()
        {
            if (toggle.isOn)
            {
                if (onTurnOnListener != null)
                    onTurnOnListener.OnUIEvent();
            }
            else
            {
                if (onTurnOffListener != null)
                    onTurnOffListener.OnUIEvent();
            }
        }
    }
}
