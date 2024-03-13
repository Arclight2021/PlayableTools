using UnityEditor.Experimental.GraphView;

namespace PlayableTools.Editor.Extension
{
    public static class GraphViewExtensions
    {
        public static void ClearNodesAndEdges(this UnityEditor.Experimental.GraphView.GraphView graphView)
        {
            var nodes = graphView.nodes;
            var edges = graphView.edges;
            graphView.DeleteElements(nodes);
            graphView.DeleteElements(edges);
        }
    }
}