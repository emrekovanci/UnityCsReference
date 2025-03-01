// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditorInternal;
using Debug = UnityEngine.Debug;

namespace UnityEditor.UIElements
{
    /// <summary>
    /// A <see cref="TagField"/> editor. For more information, refer to [[wiki:UIE-uxml-element-TagField|UXML element TagField]].
    /// </summary>
    public class TagField : PopupField<string>
    {
        [UnityEngine.Internal.ExcludeFromDocs, Serializable]
        public new class UxmlSerializedData : PopupField<string>.UxmlSerializedData
        {
            [Conditional("UNITY_EDITOR")]
            public new static void Register()
            {
                PopupField<string>.UxmlSerializedData.Register();
                UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[]
                {
                    new (nameof(overrideValue), "value")
                });
            }

            #pragma warning disable 649
            [TagFieldValueDecorator]
            [UxmlAttribute("value")]
            [SerializeField] string overrideValue;
            [SerializeField, UxmlIgnore, HideInInspector] UxmlAttributeFlags overrideValue_UxmlAttributeFlags;
            #pragma warning restore 649

            public override object CreateInstance() => new TagField();

            public override void Deserialize(object obj)
            {
                base.Deserialize(obj);

                if (ShouldWriteAttributeValue(overrideValue_UxmlAttributeFlags))
                {
                    var e = (TagField)obj;
                    e.overrideValue = overrideValue;
                }
            }
        }

        /// <summary>
        /// Instantiates a <see cref="TagField"/> using the data read from a UXML file.
        /// </summary>
        [Obsolete("UxmlFactory is deprecated and will be removed. Use UxmlElementAttribute instead.", false)]
        public new class UxmlFactory : UxmlFactory<TagField, UxmlTraits> {}
        /// <summary>
        /// Defines <see cref="UxmlTraits"/> for the <see cref="TagField"/>.
        /// </summary>
        [Obsolete("UxmlTraits is deprecated and will be removed. Use UxmlElementAttribute instead.", false)]
        public new class UxmlTraits : PopupField<string>.UxmlTraits
        {
            UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription { name = "value" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var tagField = (TagField)ve;
                tagField.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
            }
        }

        internal override string GetValueToDisplay()
        {
            return rawValue;
        }

        internal string overrideValue
        {
            get => rawValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                    rawValue = value; // bypass the check on m_Choices
                else
                    this.value = value;
            }
        }

        public override string value
        {
            get { return base.value; }
            set
            {
                // Allow the setting of value outside of Tags, but do nothing with them...
                if (m_Choices.Contains(value))
                {
                    base.value = value;
                }
            }
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            // Allow the setting of value outside of Tags, but do nothing with them...
            if (m_Choices.Contains(newValue))
            {
                base.SetValueWithoutNotify(newValue);
            }
        }

        /// <summary>
        /// Unsupported.
        /// </summary>
        public override Func<string, string> formatSelectedValueCallback
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    Debug.LogWarning(L10n.Tr("TagField doesn't support the formatting of the selected value."));
                }

                m_FormatSelectedValueCallback = null;
            }
        }

        /// <summary>
        /// Unsupported.
        /// </summary>
        public override Func<string, string> formatListItemCallback
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    Debug.LogWarning(L10n.Tr("TagField doesn't support the formatting of the list items."));
                }

                m_FormatListItemCallback = null;
            }
        }

        static List<string> InitializeTags()
        {
            return new List<string>(InternalEditorUtility.tags);
        }

        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "unity-tag-field";
        /// <summary>
        /// USS class name of labels in elements of this type.
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";
        /// <summary>
        /// USS class name of input elements in elements of this type.
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// Initializes and returns an instance of TagField.
        /// </summary>
        public TagField()
            : this(null) {}

        /// <summary>
        /// Initializes and returns an instance of TagField.
        /// </summary>
        /// <param name="label">The text to use as a label for the field.</param>
        /// <param name="defaultValue">The initial tag value this field uses.</param>
        public TagField(string label, string defaultValue = null)
            : base(label)
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            visualInput.AddToClassList(inputUssClassName);

            choices = InitializeTags();
            if (defaultValue != null)
            {
                SetValueWithoutNotify(defaultValue);
            }
        }

        internal override void AddMenuItems(IGenericMenu menu)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            choices = InitializeTags();
            foreach (var menuItem in choices)
            {
                var isSelected = (menuItem == value) && !showMixedValue;
                menu.AddItem(menuItem, isSelected, () => ChangeValueFromMenu(menuItem));
            }
            menu.AddSeparator(String.Empty);
            menu.AddItem(L10n.Tr("Add Tag..."), false, OpenTagInspector);
        }

        void ChangeValueFromMenu(string menuItem)
        {
            value = menuItem;
        }

        static void OpenTagInspector()
        {
            TagManagerInspector.ShowWithInitialExpansion(TagManagerInspector.InitialExpansionState.Tags);
        }
    }
}
