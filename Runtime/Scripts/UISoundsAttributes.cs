using UnityEngine;

namespace JanSharp
{
    /// <summary>
    /// <inheritdoc cref="UISoundReferenceAttribute(string)"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class UISoundReferenceAttribute : PropertyAttribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        readonly string referenceFieldName;
        public string ReferenceFieldName => referenceFieldName;

        /// <summary>
        /// <para>This attribute must be applied to a <see cref="string"/> field which is serialized by unity
        /// (so public or using <see cref="SerializeField"/>).</para>
        /// <para>Even when the name is set in the inspector and it is referencing a
        /// <see cref="UISoundDefinition"/>, the associated field will still be populated as
        /// <see langword="null"/> when <see cref="UISoundDefinition.silenced"/> is
        /// <see langword="true"/>.</para>
        /// </summary>
        /// <param name="referenceFieldName">The <c>nameof()</c> a <see cref="UISoundsListener"/> field. Said
        /// field must also be serialized by unity. Can also use the <see cref="HideInInspector"/> attribute
        /// since that field gets auto populated through applying UI style, though for easy viewing of what
        /// the color got resolved to it can be useful to keep it visible in the inspector.</param>
        public UISoundReferenceAttribute(string referenceFieldName)
        {
            this.referenceFieldName = referenceFieldName;
        }
    }
}
