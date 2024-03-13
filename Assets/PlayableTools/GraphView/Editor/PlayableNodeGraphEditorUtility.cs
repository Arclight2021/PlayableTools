using System;
using System.Collections.Generic;
using System.Linq;
using PlayableTools.Editor.Extension;
using PlayableTools.Nodes.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PlayableTools.GraphView.Editor
{
    public static class PlayableNodeGraphEditorUtility
    {
        public static void Export(string path,PlayableNodeGraphManager _manager,PlayableNodeGraphView graphView)
        {
            var nodeDataSo = _manager.Export();



            NodeEditorDataSO editorDataSo = null;
            if (graphView.userData != null && graphView.userData is NodeEditorDataSO)
            {
                editorDataSo = graphView.userData as NodeEditorDataSO;
            }else
                ScriptableObject.CreateInstance<NodeEditorDataSO>();
            
            editorDataSo.nodeDataso = nodeDataSo;

            var nodeEditorDatas = new List<PlayableNodeEditorData>();
            editorDataSo.NodeEditorDatas = nodeEditorDatas;

            foreach (var node in graphView.nodes)
            {
                // Debug.Log(node == null);
                var playableNode = (PlayableGraphNodeBase)node;

                var nodeEditorData = new PlayableNodeEditorData()
                {
                    name = playableNode.tar.name,
                    type = playableNode.GetType().ToString(),
                    position = playableNode.GetPosition(),
                    //todo:nickname nodeColor
                };
                nodeEditorDatas.Add(nodeEditorData);
            }

            if (!AssetDatabase.Contains(nodeDataSo))
            {
                AssetDatabase.CreateAsset(nodeDataSo, path);
            }
            AssetDatabase.SaveAssets();

            path = path.Replace(".asset", "_editor.asset");
            if (!AssetDatabase.Contains(editorDataSo))
            {
                AssetDatabase.CreateAsset(editorDataSo, path);
            }
            AssetDatabase.SaveAssets();
        }

        public static void Import(NodeEditorDataSO so,PlayableNodeGraphManager _manager,PlayableNodeGraphView graphView)
        {
            if (so == null)
                return;

            graphView.userData = so;

            var editorDataSo = so;
            _manager.Import(editorDataSo.nodeDataso);

            var editorNodeDataDic =
                editorDataSo.NodeEditorDatas.ToDictionary(
                    key => key.name, value => value
                );
            var nodeLookup = new Dictionary<string, PlayableGraphNodeBase>();
            //node
            foreach (var nodeKP in _manager.nodeDic)
            {
                // var editorNodeData =
                if (!editorNodeDataDic.TryGetValue(nodeKP.Key, out var editorNodeData))
                {
                    Debug.Log($"Cant find {nodeKP.Key}");
                    // var typeToCreate = Type.GetType(nodeKP.Key.Replace("Node", "GraphNode"));
                    // var nodeNewCreated = graphView.CreateGraphNode(typeToCreate, Vector2.zero, nodeKP.Value);
                    // nodeLookup.Add(nodeKP.Key,nodeNewCreated);
                    continue;
                }

                
                var type = Type.GetType(editorNodeData.type);
                var node = graphView.CreateGraphNode(type, editorNodeData.position, nodeKP.Value);
                // object[] paras = new object[]
                // {
                //     _manager,
                //     nodeKP.Value
                // };
                // var node = Activator.CreateInstance(type, paras) as PlayableGraphNodeBase;

                // node.SetPosition(editorNodeData.position);
                // graphView.AddElement(node);
                nodeLookup.Add(nodeKP.Key, node);
            }

            //edge
            foreach (var nodeKP in _manager.nodeDic)
            {
                foreach (var inputPort in nodeKP.Value.inputPorts)
                {
                    try
                    {
                        if (inputPort.IsConnected)
                        {
                            Edge edge = new Edge();
                            var inputNode = nodeLookup[inputPort.parentNode.name];
                            var outputNode = nodeLookup[inputPort.targetNodePort.parentNode.name];

                            edge.input = (Port)inputNode.inputContainer.Children()
                                .FirstOrDefault(x => (x as Port).portName == inputPort.name);
                            edge.output = (Port)outputNode.outputContainer.Children()
                                .FirstOrDefault(x => (x as Port).portName == inputPort.targetNodePort.name);
                            // edge.input = inputNode.portLookup[inputPort];
                            // edge.output = outputNode.portLookup[inputPort.targetNodePort];

                            edge.input.Connect(edge);
                            edge.output.Connect(edge);

                            graphView.Add(edge);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }
                }
            }

            if (nodeLookup.ContainsKey(_manager.outputNodeName))
            {
                var outputGraphNode = nodeLookup[_manager.outputNodeName];
                graphView.SetAsOutputNode(outputGraphNode);
            }
            
        }
    }
}