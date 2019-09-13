using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeEditor
{
	[Serializable]
	public abstract class NodeSlot : ISlot
	{
		const string k_NotInit = "Not Initilaized";

		[SerializeField] int m_Id;

		[SerializeField] string m_DisplayName = k_NotInit;

		[SerializeField] SlotType m_SlotType = SlotType.Input;

		[SerializeField] bool m_AllowMultipleConnections = false;

		[SerializeField] int m_Priority = int.MaxValue;

		[SerializeField] bool m_Hidden;

		[NonSerialized] private List<ISlot> m_SlotConnectionCache = new List<ISlot>();

		bool m_HasError;

		public virtual VisualElement InstantiateControl()
		{
			return null;
		}

		public NodeSlot SetDisplayName(string name)
		{
			m_DisplayName = name;
			return this;
		}

		public ReadOnlyList<ISlot> GetSlotConnectionCache()
		{
			return new ReadOnlyList<ISlot>(m_SlotConnectionCache);
		}

		internal void ClearConnectionCache()
		{
			m_SlotConnectionCache.Clear();
		}

		internal void AddConnectionToCache(ISlot slot)
		{
			if(Equals(slot)) throw new Exception("Cannot add itself to cache");
			if(m_SlotConnectionCache.Contains(slot)) throw new Exception("Slot is already present");
			m_SlotConnectionCache.Add(slot);
		}

		public virtual string displayName
		{
			get => m_DisplayName + $" ({valueType.Type.Name})";
            set => m_DisplayName = value;
        }

		public string RawDisplayName()
		{
			return m_DisplayName;
		}

		public SlotReference slotReference => new SlotReference(owner.guid, m_Id);

		public INode owner { get; set; }

		public NodeSlot SetAllowMultipleConnections(bool allow)
		{
			m_AllowMultipleConnections = allow;
			return this;
		}

		public bool allowMultipleConnections
		{
			get => m_AllowMultipleConnections;
            set => m_AllowMultipleConnections = value;
        }

		public bool hidden
		{
			get => m_Hidden;
            set => m_Hidden = value;
        }

		public int id
		{
			get => m_Id;
            protected internal set => m_Id = value;
        }

		public NodeSlot SetPriority(int priority)
		{
			m_Priority = priority;
			return this;
		}

		public int priority
		{
			get => m_Priority;
            set => m_Priority = value;
        }

		public bool isInputSlot => m_SlotType == SlotType.Input;

		public bool isOutputSlot => m_SlotType == SlotType.Output;

		public SlotType slotType
		{
			get => m_SlotType;
            protected internal set => m_SlotType = value;
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
			get => m_HasError;
            set => m_HasError = value;
        }


		public abstract SerializedType valueType
		{
			get;
		}

		public bool IsCompatibleWith(NodeSlot otherSlot)
		{
			return otherSlot != null
			       && otherSlot.owner != owner
			       && otherSlot.isInputSlot != isInputSlot
			       && otherSlot.isOutputSlot != isOutputSlot
			       && (otherSlot.valueType == valueType || otherSlot.valueType.Type.IsAssignableFrom(valueType));
		}

		public virtual void GetPreviewProperties(List<PreviewProperty> properties)
		{
			properties.Add(default);
		}

		public abstract void CopyValuesFrom(NodeSlot foundSlot);

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