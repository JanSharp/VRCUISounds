using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIToggleSounds : UdonSharpBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private UISoundsListener onTurnOnListener;
        [SerializeField] private UISoundsListener onTurnOffListener;

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
