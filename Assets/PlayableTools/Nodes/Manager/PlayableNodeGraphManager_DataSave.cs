using System;
using System.Collections.Generic;
using System.IO;
using PlayableTools.Nodes;
using UnityEngine;

namespace PlayableTools
{
    public partial class PlayableNodeGraphManager
    {
        public NodeDataSO Export()
        {
            if (this.so == null)
            {
                so = ScriptableObject.CreateInstance<NodeDataSO>();
            }

            so.Clear();
            
            this._clips = new();
            this._avatarMasks = new();
            this._animatorControllers = new();

            so.graphName = this.graphName;
            so.outputNodeName = this.outputNodeName;
            so.clips = this._clips;
            so.masks = this._avatarMasks;
            so.controllers = this._animatorControllers;

            var nodeDatas = new List<PlayableNodeData>();
            so.nodeDatas = nodeDatas;
            
            var nodeLinkDatas = new List<PlayableNodeLinkData>();
            so.linkDatas = nodeLinkDatas;

            using MemoryStream stream = new MemoryStream(1024);
            using BinaryWriter binaryWriter = new BinaryWriter(stream);
            
            foreach (var keyValuePair in nodeDic)
            {
                binaryWriter.BaseStream.Position = 0; //reset stream
                
                var nodeBase = keyValuePair.Value;
                var name = keyValuePair.Value.name;
                nodeBase.Export(binaryWriter);
                var dataArr = stream.ToArray();

                PlayableNodeData nodeData = new PlayableNodeData()
                {
                    name = name,
                    type = nodeBase.GetType().Name,
                    inputCount = nodeBase.inputCount,
                    outputCount = nodeBase.outputCount,
                    nodeData = Convert.ToBase64String(dataArr)
                };
                nodeDatas.Add(nodeData);

                foreach (var inputPort in nodeBase.inputPorts)
                {
                    if (inputPort.targetNodePort == null)
                    {
                        continue;
                    }
                    var linkData = new PlayableNodeLinkData()
                    {
                        source = inputPort.parentNode.name + ":"+inputPort.name,
                        target = inputPort.targetNodePort.parentNode.name + ":"+inputPort.targetNodePort.name,
                    };
                    nodeLinkDatas.Add(linkData);
                }
            }
            
            return so;
        }

        public void Import(NodeDataSO so)
        {
            Clear();
            this.so = so;
            
            this.graphName = so.graphName;
            this.outputNodeName = so.outputNodeName;
            this._clips = so.clips;
            this._avatarMasks = so.masks;
            this._animatorControllers = so.controllers;
            
            nodeDic = new Dictionary<string, PlayableNodeBase>();
            
            //generate nodes

            using MemoryStream stream = new MemoryStream(1024);
            using BinaryReader reader = new BinaryReader(stream);
            
            var nameSpace = typeof(PlayableNodeBase).Namespace;
            foreach (var nodeData in so.nodeDatas)
            {
                var typeStr = $"{nameSpace}.{nodeData.type}";
                var nodeType = Type.GetType(typeStr);
                if (nodeType == null)
                {
                    Debug.Log($"Cant find type {typeStr}");
                    continue;
                }

                object[] paras = new object[]
                {
                    this,
                    nodeData.name
                };
                var node = Activator.CreateInstance(nodeType,paras) as PlayableNodeBase;
                if (node==null)
                {
                    Debug.Log($"Cant create instance of {typeStr}");
                    continue;
                }
                
                stream.SetLength(0);
                stream.Write(Convert.FromBase64String(nodeData.nodeData));
                stream.Position = 0;
                
                // node.name = nodeData.name;
                node.SetInputPortCount(nodeData.inputCount);
                // node.SetOutputPortCount(nodeData.outputCount);
#if UNITY_EDITOR
                try
                {
                    node.Import(reader);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
#else
                    node.Import(reader);
#endif
                nodeDic.Add(node.name,node);
            }
            
            //connect nodes
            foreach (var linkData in so.linkDatas)
            {
                var sourcestr = linkData.source.Split(':');
                var sourceName = sourcestr[0];
                int sourceIndex = int.Parse(sourcestr[1]);
                
                var targetstr = linkData.target.Split(':');
                var targetName = targetstr[0];
                int targetIndex = int.Parse(targetstr[1]);

                var sourceNode = nodeDic[sourceName];
                var targetNode = nodeDic[targetName];
                this.Connect(targetNode,targetIndex,sourceNode,sourceIndex);
            }
        }
    }
}