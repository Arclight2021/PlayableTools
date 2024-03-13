using System;
using System.Collections.Generic;
using System.Linq;
using PlayableTools.Nodes;
using PlayableTools.Nodes.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PlayableTools.GraphView.Editor
{
    public class PlayableNodeGraphSearchWindowProvider:ScriptableObject, ISearchWindowProvider
    {
        public Action<Type, Vector2> onCreateNode;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            
            var assembly = typeof(AnimationClipGraphNode).Assembly;
            var types = assembly.GetTypes().Where(type => type.Name.EndsWith("GraphNode"));

            foreach (var type in types)
            {
                entries.Add(new SearchTreeEntry(new GUIContent(type.Name.ToString()))
                {
                    level = 1,
                    userData = type
                });
            }
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            // Debug.Log(SearchTreeEntry.userData);
            // context.screenMousePosition
            onCreateNode?.Invoke(SearchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}