using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NodeEditor
{
	[Serializable]
	public abstract class NodeSlot : ISlot
	{
		const string k_NotInit = "Not Initilaized";

		[SerializeField] int m_Id;

		[SerializeField] string m_DisplayName = k_NotInit;

		[SerializeField] SlotType m_SlotType = SlotType.Input;

		[SerializeField] int m_Priority = int.MaxValue;

		[SerializeField] bool m_Hidden;

		[SerializeField] SerializedType m_ValueType;

		bool m_HasError;

		protected NodeSlot()
		{
		}

		protected NodeSlot(int slotId, string displayName, Type valueType, SlotType slotType, bool hidden = false)
		{
			m_Id = slotId;
			m_DisplayName = displayName;
			m_SlotType = slotType;
			m_Hidden = hidden;
			m_ValueType = new SerializedType(valueType);
		}

		protected NodeSlot(int slotId, string displayName, Type valueType, SlotType slotType, int priority, bool hidden = false)
		{
			m_Id = slotId;
			m_DisplayName = displayName;
			m_SlotType = slotType;
			m_Priority = priority;
			m_Hidden = hidden;
			m_ValueType = new SerializedType(valueType);
		}

		public virtual VisualElement InstantiateControl()
		{
			return null;
		}

		public virtual string displayName
		{
			get { return m_DisplayName + string.Format(" ({0})", m_ValueType.Type.Name); }
			set { m_DisplayName = value; }
		}

		public string RawDisplayName()
		{
			return m_DisplayName;
		}

		public SlotReference slotReference
		{
			get { return new SlotReference(owner.guid, m_Id); }
		}

		public INode owner { get; set; }

		public bool hidden
		{
			get { return m_Hidden; }
			set { m_Hidden = value; }
		}

		public int id
		{
			get { return m_Id; }
		}

		public int priority
		{
			get { return m_Priority; }
			set { m_Priority = value; }
		}

		public bool isInputSlot
		{
			get { return m_SlotType == SlotType.Input; }
		}

		public bool isOutputSlot
		{
			get { return m_SlotType == SlotType.Output; }
		}

		public SlotType slotType
		{
			get { return m_SlotType; }
		}

		public bool isConnected
		{
			get
			{
				// node and graph respectivly
				if (owner == null || owner.owner == null)
					return false;

				var graph = owner.owner;
				var edges = graph.GetEdges(slotReference);
				return edges.Any();
			}
		}

		public bool hasError
		{
			get { return m_HasError; }
			set { m_HasError = value; }
		}

		public SerializedType valueType
		{
			get { return m_ValueType; }
			set { m_ValueType = value; }
		}

		public bool IsCompatibleWith(NodeSlot otherSlot)
		{
			return otherSlot != null
			       && otherSlot.owner != owner
			       && otherSlot.isInputSlot != isInputSlot
			       && otherSlot.isOutputSlot != isOutputSlot
			       && otherSlot.valueType == valueType;
		}

		public virtual void GetPreviewProperties(List<PreviewProperty> properties)
		{
			properties.Add(default(PreviewProperty));
		}

		public abstract void CopyValuesFrom(NodeSlot foundSlot);

		public abstract T GetValue<T>();

		bool Equals(NodeSlot other)
		{
			return m_Id == other.m_Id && owner.guid.Equals(other.owner.guid);
		}

		public bool Equals(ISlot other)
		{
			return Equals(other as object);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((NodeSlot) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (m_Id * 397) ^ (owner != null ? owner.GetHashCode() : 0);
			}
		}
	}
}