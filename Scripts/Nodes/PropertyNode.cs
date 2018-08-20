using System;
using System.Linq;
using UnityEngine;

namespace NodeEditor.Nodes
{
    public class PropertyNode : AbstractNode, IOnAssetEnabled
    {
        private Guid m_PropertyGuid;

        [SerializeField]
        private string m_PropertyGuidSerialized;

        public const int OutputSlotId = 0;

        public PropertyNode()
        {
            name = "Property";
            UpdateNodeAfterDeserialization();
        }

        public override string documentationURL
        {
            get { return "https://github.com/Unity-Technologies/ShaderGraph/wiki/Property-Node"; }
        }

        private void UpdateNode()
        {
            var graph = owner as AbstractNodeGraph;
            var property = graph.properties.FirstOrDefault(x => x.guid == propertyGuid);
            if (property == null)
                return;

	        var valueSlot = (NodeSlot)Activator.CreateInstance(typeof(ValueSlot<>).MakeGenericType(property.propertyType), OutputSlotId, property.displayName, SlotType.Output,false);
	        AddSlot(valueSlot);
			RemoveSlotsNameNotMatching(new[] { OutputSlotId });
		}

        public Guid propertyGuid
        {
            get { return m_PropertyGuid; }
            set
            {
                if (m_PropertyGuid == value)
                    return;

                var graph = owner as AbstractNodeGraph;
                var property = graph.properties.FirstOrDefault(x => x.guid == value);
                if (property == null)
                    return;
                m_PropertyGuid = value;

                UpdateNode();

                Dirty(ModificationScope.Topological);
            }
        }

        protected override bool CalculateNodeHasError()
        {
            var graph = owner as AbstractNodeGraph;

            if (!graph.properties.Any(x => x.guid == propertyGuid))
                return true;

            return false;
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            m_PropertyGuidSerialized = m_PropertyGuid.ToString();
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            if (!string.IsNullOrEmpty(m_PropertyGuidSerialized))
                m_PropertyGuid = new Guid(m_PropertyGuidSerialized);
        }

        public void OnEnable()
        {
            UpdateNode();
        }
    }
}
