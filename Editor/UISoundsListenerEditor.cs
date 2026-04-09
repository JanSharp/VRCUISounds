using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UISoundsListenerOnBuild
    {
        static UISoundsListenerOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<UISoundsListener>(OnBuild, order: -900);
        }

        private static bool OnBuild(IEnumerable<UISoundsListener> listeners)
        {
            bool result = true;
            List<UISoundsListener> listenersList = listeners.ToList();
            for (int i = listenersList.Count - 1; i >= 0; i--)
            {
                UISoundsListener listener = listenersList[i];
                if (!UISoundsManagerOnBuild.IsManagedListenerInst(listener))
                {
                    Debug.LogError($"[UISounds] UI Sounds Listener components are purely used internally by "
                        + $"the UI Sounds Manager and must not exist anywhere else.", listener);
                    result = false;
                }
            }
            return result;
        }
    }

    public static class UISoundsListenerUtil
    {
        public static void SetPersistentListener(Component component, string eventPropertyName, UISoundsListener listener)
        {
            SetPersistentListener(component, eventPropertyName, listener, nameof(UISoundsListener.OnUIEvent));
        }

        public static void SetPersistentListener<T>(Component component, string eventPropertyName, T target, string customEventName)
            where T : UdonSharpBehaviour
        {
            SerializedObject so = new(component);
            SerializedProperty eventProp = so.FindProperty(eventPropertyName);
            if (target != null)
                EditorUtil.EnsureHasPersistentSendCustomEventListener(
                    eventProp,
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(target),
                    customEventName);
            else
            {
                int i = 0;
                List<int> indexesToRemove = null;
                foreach (var listener in EditorUtil.EnumeratePersistentEventListeners(eventProp))
                {
                    if ((listener.Target == null
                            || (listener.Target is UdonBehaviour ub && UdonSharpEditorUtility.GetProxyBehaviour(ub)?.GetType() == typeof(T)))
                        && listener.MethodName == nameof(UdonBehaviour.SendCustomEvent)
                        && listener.StringArgument == customEventName)
                    {
                        indexesToRemove ??= new();
                        indexesToRemove.Add(i);
                    }
                    i++;
                }
                if (indexesToRemove != null)
                    for (int j = indexesToRemove.Count - 1; j >= 0; j--)
                        EditorUtil.DeletePersistentEventListenerAtIndex(eventProp, indexesToRemove[j]);
            }
            so.ApplyModifiedProperties();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UISoundsListener))]
    public class UISoundsListenerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            using (new EditorGUI.DisabledGroupScope(true))
            {
                serializedObject.Update();
                DrawPropertiesExcluding(serializedObject, "m_Script");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
