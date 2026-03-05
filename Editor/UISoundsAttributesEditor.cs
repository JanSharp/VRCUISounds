using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UISoundsAttributesOnBuild
    {
        /// <summary>
        /// <para>Contains only entires where <see cref="TypeCache.referenceFieldPairs"/> contains at least
        /// one value.</para>
        /// </summary>
        private static List<TypeCache> ubTypeCache = new();
        private static Dictionary<System.Type, TypeCache> ubTypeCacheByType = new();
        private static List<System.Type> invalidUbTypes = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private class TypeCache
        {
            public System.Type ubType;
            public List<FieldPair> referenceFieldPairs = new();
            public Component[] instancesToApplyTo;

            public class FieldPair
            {
                public string nameFieldName;
                public string associatedFieldName;
                public string nameFieldValue;

                public FieldPair(string nameFieldName, string associatedFieldName)
                {
                    this.nameFieldName = nameFieldName;
                    this.associatedFieldName = associatedFieldName;
                }
            }

            public TypeCache(System.Type ubType)
            {
                this.ubType = ubType;
            }
        }

        static UISoundsAttributesOnBuild()
        {
            ubTypeCache.Clear();
            invalidUbTypes.Clear();
            foreach (System.Type ubType in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes)
                TryGenerateTypeCache(ubType);
            if (invalidUbTypes.Count != 0)
            {
                OnBuildUtil.RegisterAction(InvalidAttributes, order: -1000000);
                return;
            }

            foreach (TypeCache cached in ubTypeCache)
            {
                OnBuildUtil.RegisterTypeCumulative(
                    cached.ubType,
                    ubs => OnBuild(ubs, cached),
                    order: -961); // "Random" order, but after manager on build.
            }
        }

        private static bool InvalidAttributes()
        {
            foreach (System.Type ubType in invalidUbTypes)
                TryGenerateTypeCache(ubType, validateOnly: true);
            return false;
        }

        private static void TryGenerateTypeCache(System.Type ubType, bool validateOnly = false)
        {
            TypeCache cached = validateOnly ? null : new(ubType);

            bool isValid = true;

            foreach (FieldInfo field in EditorUtil.GetFieldsIncludingBase(ubType, PrivateAndPublicFlags, stopAtType: typeof(UdonSharpBehaviour)))
                isValid &= CheckForAttribute<UISoundReferenceAttribute, UISoundsListener>(ubType, field, cached?.referenceFieldPairs, a => a.ReferenceFieldName, validateOnly);

            if (validateOnly)
                return;

            if (!isValid)
            {
                invalidUbTypes.Add(ubType);
                return;
            }

            if (cached.referenceFieldPairs.Count != 0)
            {
                ubTypeCache.Add(cached);
                ubTypeCacheByType.Add(ubType, cached);
            }
        }

        private static bool CheckForAttribute<TAttribute, TAssociated>(
            System.Type ubType,
            FieldInfo field,
            List<TypeCache.FieldPair> cachedFieldPairs,
            System.Func<TAttribute, string> getAssociatedFieldName,
            bool validateOnly)
            where TAttribute : System.Attribute
        {
            TAttribute attr = field.GetCustomAttribute<TAttribute>(inherit: true);
            if (attr == null)
                return true;

            bool isValid = true;

            if (field.FieldType != typeof(string))
            {
                Debug.LogError($"[UISounds] The {ubType.Name}.{field.Name} field has the {typeof(TAttribute).Name} "
                    + $"however its type is {field.FieldType.Name}. It must be a string.");
                isValid = false;
            }
            if (!EditorUtil.IsSerializedField(field))
            {
                Debug.LogError($"[UISounds] The {ubType.Name}.{field.Name} field has the {typeof(TAttribute).Name} "
                    + $"however it is not a serialized field. It must either be public or have the {nameof(SerializeField)} attribute.");
                isValid = false;
            }

            string associatedFieldName = getAssociatedFieldName(attr) ?? "";
            FieldInfo defField = EditorUtil.GetFieldIncludingBase(ubType, associatedFieldName, PrivateAndPublicFlags);
            if (defField == null)
            {
                Debug.LogError($"[UISounds] The {ubType.Name}.{field.Name} field has the {typeof(TAttribute).Name} "
                    + $"pointing to a {typeof(TAssociated).Name} field by the name '{associatedFieldName}' however no such field exists.");
                isValid = false;
            }
            if (defField != null && defField.FieldType != typeof(TAssociated))
            {
                Debug.LogError($"[UISounds] The {ubType.Name}.{field.Name} field has the {typeof(TAttribute).Name} "
                    + $"pointing to the field by the name '{associatedFieldName}' which has the type {defField.FieldType.Name}, "
                    + $"however it must be a {typeof(TAssociated).Name}.");
                isValid = false;
            }
            if (defField != null && !EditorUtil.IsSerializedField(defField))
            {
                Debug.LogError($"[UISounds] The {ubType.Name}.{field.Name} field has the {typeof(TAttribute).Name} "
                    + $"pointing to the field by the name '{associatedFieldName}' which is not a serialized field. "
                    + $"It must either be public or have the {nameof(SerializeField)} attribute.");
                isValid = false;
            }

            if (isValid && !validateOnly)
                cachedFieldPairs.Add(new(field.Name, associatedFieldName));
            return isValid;
        }

        // Using IEnumerable<> here causes Unity to fail compilation complaining about missing an assembly reference
        // to the odin serializer. ReadOnlyCollection<> it is, that's what the OnBuildUtil gives us anyway.
        // Yes this makes no sense.
        private static bool OnBuild(ReadOnlyCollection<Component> ubs, TypeCache cached)
        {
            bool result = true;
            foreach (Component ub in ubs)
                result &= OnBuild((UdonSharpBehaviour)ub, cached);
            return result;
        }

        private static bool OnBuild(UdonSharpBehaviour ub, TypeCache cached)
        {
            SerializedObject so = new(ub);
            if (!UISoundsRootUtil.TryGetRootOrError(ub, out UISoundsRoot root))
                return false;
            bool result = true;
            foreach (TypeCache.FieldPair pair in cached.referenceFieldPairs)
            {
                string defName = (string)EditorUtil.GetFieldIncludingBase(cached.ubType, pair.nameFieldName, PrivateAndPublicFlags)
                    .GetValue(ub) ?? "";
                if (!UISoundDefinitionGroupOnBuild.TryGetDefByNameOrError(root.definitionGroup, defName, ub, out UISoundDefinition def))
                {
                    result = false;
                    continue;
                }
                UISoundsListener listener = UISoundsManagerOnBuild.GetListenerForDef(def);
                so.FindProperty(pair.associatedFieldName).objectReferenceValue = listener;
            }
            so.ApplyModifiedProperties();
            return result;
        }
    }
}
