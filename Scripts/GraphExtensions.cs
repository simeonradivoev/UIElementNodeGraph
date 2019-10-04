using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEditor
{
    public static class GraphExtensions
    {
        public static bool TryGetPropertyId(this IGraph graph, string name, out Guid guid)
        {
            var prop = graph.GetProperties().FirstOrDefault(n => n.reference == name);
            if (prop != null)
            {
                guid = prop.guid;
                return true;
            }
            guid = Guid.Empty;
            return false;
        }

        public static IEnumerable<IEdge> GetEdges(this IGraph graph, SlotReference slotReference)
        {
            List<IEdge> list = new List<IEdge>();
            graph.GetEdges(slotReference, list);
            return list;
        }

        public static IEnumerable<T> GetNodes<T>(this IGraph graph) where T : INode
        {
            var e = graph.GetNodes().GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is T)
                    yield return (T)e.Current;
            }
            e.Dispose();
        }

        public static void GetEdges(this IGraph graph, SlotReference s, IList<IEdge> edges)
        {
            var node = graph.GetNodeFromGuid(s.nodeGuid);
            if (node == null)
            {
                Debug.LogWarning("Node does not exist");
                return;
            }
            ISlot slot = node.FindSlot<ISlot>(s.slotId);
            ReadOnlyList<IEdge> candidateEdges = graph.GetEdges(s.nodeGuid);

            for (int i = 0; i < candidateEdges.Count; i++)
            {
                var edge = candidateEdges[i];
                var cs = slot.isInputSlot ? edge.inputSlot : edge.outputSlot;
                if (cs.nodeGuid == s.nodeGuid && cs.slotId == s.slotId)
                    edges.Add(edge);
            }
        }

        public static IEdge GetEdge(this IGraph graph, SlotReference s)
        {
            var node = graph.GetNodeFromGuid(s.nodeGuid);
            if (node == null)
            {
                Debug.LogWarning("Node does not exist");
                return null;
            }
            ISlot slot = node.FindSlot<ISlot>(s.slotId);
            ReadOnlyList<IEdge> candidateEdges = graph.GetEdges(s.nodeGuid);

            for (int i = 0; i < candidateEdges.Count; i++)
            {
                var edge = candidateEdges[i];
                var cs = slot.isInputSlot ? edge.inputSlot : edge.outputSlot;
                if (cs.nodeGuid == s.nodeGuid && cs.slotId == s.slotId)
                    return edge;
            }

            return null;
        }
    }
}