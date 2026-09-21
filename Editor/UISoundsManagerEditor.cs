using System.Collections.Generic;
using JanSharp.Internal;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    public static class UISoundsManagerOnBuild
    {
        private static Dictionary<UISoundDefinition, UISoundsListener> listenersByDef = new();
        private static HashSet<UISoundsListener> managedListenersLut = new();
        private static HashSet<UISoundDefinition> usedDefsLut = new();
        private static UISoundsManager manager;
        private static bool didCreateNewListeners;

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterAction(OnPreBuild, order: -1001);
            OnBuildUtil.RegisterType<UISoundsManager>(OnBuild, order: -1000, includeEditorOnly: true);
            OnBuildUtil.RegisterAction(OnPostBuild, order: 1001);
        }

        private static bool OnPreBuild()
        {
            listenersByDef.Clear();
            managedListenersLut.Clear();
            usedDefsLut.Clear();
            manager = null;
            didCreateNewListeners = false;
            return true;
        }

        private static bool OnBuild(UISoundsManager manager)
        {
            UISoundsManagerOnBuild.manager = manager;

            int count = manager.ListenersContainer.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Transform child = manager.ListenersContainer.GetChild(i);
                UISoundsListener listener = child.GetComponent<UISoundsListener>();
                if (listener == null || listener.SoundDefinitionGo == null)
                {
                    if (listener != null)
                        OnBuildUtil.MarkForRerunDueToDestruction();
                    Undo.DestroyObjectImmediate(child.gameObject);
                    continue;
                }
                UISoundDefinition def = listener.SoundDefinitionGo.GetComponent<UISoundDefinition>();
                if (def == null || def.silenced)
                {
                    OnBuildUtil.UndoDestroyObjectImmediate(child.gameObject);
                    continue;
                }
                listenersByDef.Add(def, listener);
                managedListenersLut.Add(listener);
            }

            return true;
        }

        public static UISoundsListener GetListenerForDef(UISoundDefinition def)
        {
            if (def == null || def.silenced)
                return null;
            if (manager == null)
                return null; // Wait for the singleton editor scripting to create the manager and trigger a rerun.

            usedDefsLut.Add(def);
            if (listenersByDef.TryGetValue(def, out UISoundsListener listener))
                return listener;
            GameObject go = new(def.name);
            go.transform.SetParent(manager.ListenersContainer, worldPositionStays: false);
            Undo.RegisterCreatedObjectUndo(go, "Generate UI Sounds Listener");
            listener = UdonSharpUndo.AddComponent<UISoundsListener>(go);
            listenersByDef.Add(def, listener);
            managedListenersLut.Add(listener);
            didCreateNewListeners = true;
            return listener;
        }

        public static bool IsManagedListenerInst(UISoundsListener listener) => managedListenersLut.Contains(listener);

        private static bool OnPostBuild()
        {
            if (didCreateNewListeners) // Deduplicate the mark call to prevent running into the rerun limit.
                OnBuildUtil.MarkForRerunDueToScriptInstantiation();
            foreach (var kvp in listenersByDef)
                if (usedDefsLut.Contains(kvp.Key))
                    MakeListenerMatchDef(kvp.Value, kvp.Key);
                else
                    OnBuildUtil.UndoDestroyObjectImmediate(kvp.Value.gameObject);
            return true;
        }

        private static void MakeListenerMatchDef(UISoundsListener listener, UISoundDefinition def)
        {
            SerializedObject listenerGo = new(listener);
            listenerGo.FindProperty(UISoundsListener.UiSoundsManagerPropName).objectReferenceValue = manager;
            listenerGo.FindProperty(UISoundsListener.SoundDefinitionGoPropName).objectReferenceValue = def.gameObject;
            listenerGo.FindProperty(UISoundsListener.AudioClipPropName).objectReferenceValue = def.audioClip;
            listenerGo.FindProperty(UISoundsListener.VolumePropName).floatValue = def.volume;
            listenerGo.ApplyModifiedProperties();

            SerializedObject goSo = new(listener.gameObject);
            goSo.FindProperty("m_Name").stringValue = def.name;
            goSo.ApplyModifiedProperties();
        }
    }

    // No multi edit support because there is only ever one. It is a singleton after all.
    [CustomEditor(typeof(UISoundsManager))]
    public class UISoundsManagerEditor : Editor
    {
        private SerializedObject so;
        private SerializedProperty audioSourceProp;
        private SerializedProperty listenersContainerProp;
        private SerializedProperty mutedProp;
        private SerializedObject audioSourceSo;
        private SerializedProperty volumeProp;

        private void OnEnable()
        {
            so = serializedObject;
            audioSourceProp = so.FindProperty(UISoundsManager.AudioSourcePropName);
            listenersContainerProp = so.FindProperty(UISoundsManager.ListenersContainerPropName);
            mutedProp = so.FindProperty(UISoundsManager.MutedPropName);
            CheckAudioSourceProp();
        }

        private void CheckAudioSourceProp()
        {
            AudioSource audioSource = (AudioSource)audioSourceProp.objectReferenceValue;
            if (audioSource == null)
            {
                audioSourceSo = null;
                volumeProp = null;
                return;
            }
            audioSourceSo = new(audioSource);
            volumeProp = audioSourceSo.FindProperty("m_Volume");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) // There is only one.
                return;

            so.Update();
            using (new EditorGUI.DisabledGroupScope(audioSourceSo != null)) // Only allow setting it, not unsetting.
                if (EditorGUILayout.PropertyField(audioSourceProp))
                    CheckAudioSourceProp();
            using (new EditorGUI.DisabledGroupScope(listenersContainerProp.objectReferenceValue != null)) // Only allow setting it, not unsetting.
                EditorGUILayout.PropertyField(listenersContainerProp);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mutedProp);
            so.ApplyModifiedProperties();

            if (audioSourceSo != null)
            {
                audioSourceSo.Update();
                DrawVolumeProp();
                audioSourceSo.ApplyModifiedProperties();
            }
        }

        private void DrawVolumeProp()
        {
            Rect rect = EditorGUILayout.GetControlRect(hasLabel: true);
            using (new EditorGUI.PropertyScope(rect, label: null, volumeProp))
            {
                EditorGUI.PrefixLabel(rect, new GUIContent(volumeProp.displayName));
                rect.x += EditorGUIUtility.labelWidth;
                rect.width -= EditorGUIUtility.labelWidth;
                volumeProp.floatValue = EditorGUI.Slider(rect, volumeProp.floatValue, 0f, 1f);
            }
        }
    }
}
