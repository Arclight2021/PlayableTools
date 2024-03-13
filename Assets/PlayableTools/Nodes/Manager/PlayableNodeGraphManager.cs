using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlayableTools.Nodes;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace PlayableTools
{
    public partial class PlayableNodeGraphManager
    {
        // public static Dictionary<string, PlayableNodeGraphManager> NodeManagers = new Dictionary<string, PlayableNodeGraphManager>();
        
        public Dictionary<string, PlayableNodeBase> nodeDic;
        public string graphName;
        public string outputNodeName;

        private List<AnimationClip> _clips;
        private List<AnimatorController> _animatorControllers;
        private List<AvatarMask> _avatarMasks;

        private NodeDataSO so;
        
        public PlayableNodeGraphManager(string graphName)
        {
            this.graphName = graphName;

            nodeDic = new Dictionary<string, PlayableNodeBase>();
            _clips = new();
            _animatorControllers = new();
            _avatarMasks = new();
        }

        #region NodeCreateAndRemove
        public PlayableNodeBase CreateNode(string nodeType)
        {
            Type type = Type.GetType(nodeType);
            if (type == null)
            {
                Debug.Log("Type not exist");
                return null;
            }

            string nodeName = Guid.NewGuid().ToString();

            object[] constructerEle = new[] { (object)this, nodeName };
            var nodeBase = Activator.CreateInstance(type,constructerEle) as PlayableNodeBase;
            
            nodeDic.Add(nodeName,nodeBase);
            return nodeBase;
        }

        public T CreateNode<T>() where T : PlayableNodeBase
        {
            return (T)CreateNode(typeof(T).ToString());
        }

        public void RemoveNode(PlayableNodeBase nodeBase)
        {
            if (nodeDic.ContainsKey(nodeBase.name))
            {
                nodeDic.Remove(nodeBase.name);
            }
            else
            {
                Debug.Log($"{this.GetType()} doesnt in dic");
            }
        }
        #endregion

        #region NodeConnect
        /// <summary>
        /// 从Source的output连接到target的input
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="sourcePortIndex"></param>
        /// <param name="target"></param>
        /// <param name="targetPortIndex"></param>
        public void Connect(PlayableNodeBase sourceNode, int sourcePortIndex,
            PlayableNodeBase target, int targetPortIndex)
        {
            if (sourceNode.outputPorts.Count<=sourcePortIndex)
            {
                Debug.Log("Source Port output index out of range");
                return;
            }
            if (target.inputPorts.Count<=targetPortIndex)
            {
                Debug.Log("Target Port iutput index out of range");
                return;
            }

            var outputPort = sourceNode.outputPorts[sourcePortIndex];
            var inputPort = target.inputPorts[targetPortIndex];
            
            outputPort.Connect(inputPort);
        }

        public void DisConnect(PlayableNodeBase target, int targetPortIndex)
        {
            if (target.inputPorts.Count<=targetPortIndex)
            {
                Debug.Log("Target Port iutput index out of range");
                return;
            }

            var port = target.inputPorts[targetPortIndex];
            
            port.DisConnect();
        }
        #endregion
        
        public void SetOutputNode(PlayableNodeBase nodeBase)
        {
            this.outputNodeName = nodeBase.name;
            Debug.Log($"Set output node to {nodeBase.name}");
        }

        #region Get

        public int AddAnimationClip(AnimationClip clip)
        {
            _clips.Add(clip);
            return _clips.IndexOf(clip);
        }

        public AnimationClip GetAnimationClip(int index)
        {
            if (_clips.Count <= index)
            {
                // Debug.Log($"Cant find clip where index {index}");
                return null;
            }
            return _clips[index];
        }

        public void RemoveAnimationClip(int index)
        {
            if (_clips.Count > index)
            {
                _clips.RemoveAt(index);
            }else
                Debug.Log("Clip Out of Range");
        }

        public int AddAnimatorController(AnimatorController animatorController)
        {
            _animatorControllers.Add(animatorController);
            return _animatorControllers.IndexOf(animatorController);
        }

        public AnimatorController GetAnimatorController(int index)
        {
            if (_animatorControllers.Count <= index)
            {
                // Debug.Log($"Cant find clip where index {index}");
                return null;
            }

            return _animatorControllers[index];
        }

        public void RemoveAnimatorController(int index)
        {
            if (_animatorControllers.Count > index)
            {
                _animatorControllers.RemoveAt(index);
            }else
                Debug.Log("Index Out of Range");
        }

        public int AddAvatarMask(AvatarMask mask)
        {
            if (mask is null)
            {
                return int.MaxValue;
            }
            _avatarMasks.Add(mask);
            return _avatarMasks.IndexOf(mask);
        }

        public AvatarMask GetAvatarMask(int index)
        {
            if (_avatarMasks.Count <= index)
            {
                // Debug.Log($"Cant find clip where index {index}");
                return null;
            }
            return _avatarMasks[index];
        }
        public void RemoveAvatarMask(int index)
        {
            if (_avatarMasks.Count > index)
            {
                _avatarMasks.RemoveAt(index);
            }else
                Debug.Log("Index Out of Range");
        }
        

        #endregion

        
        private void Clear()
        {
            nodeDic = null;
            graphName = null;
            outputNodeName = null;
            so = null;
            _clips = null;
            _animatorControllers = null;
            _avatarMasks = null;
        }
        
        #region Runtime

        public PlayableGraph PlayableGraph { get;private set;}
        private bool _isRunning;
        public bool isRunning => _isRunning;
        public void GeneratePlayableGraph(Animator animator)
        {
            PlayableGraph = PlayableGraph.Create();
            PlayableRuntimeCreateContext context = new PlayableRuntimeCreateContext()
            {
                PlayableGraph = PlayableGraph,
                Animator = animator
            };
            //generate nodes/playables
            foreach (var nodeBase in nodeDic.Values)
            {
                // nodeBase.OnRuntimeCreate(_playableGraph);
                nodeBase.OnRuntimeCreate(ref context);
            }
            
            //set output source
            var outputNode = nodeDic[outputNodeName];
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(PlayableGraph,"Out",animator);

            PlayableNodeBase.PlayableBase ps = new PlayableNodeBase.PlayableBase(outputNode.Playable);
            output.SetSourcePlayable(ps,0);
            //connect them
            foreach (var nodeBase in nodeDic.Values)
            {
                foreach (var port in nodeBase.inputPorts)
                {
                    if (port.IsConnected)
                    {
                        PlayableNodeBase.PlayableBase p1 =
                            new PlayableNodeBase.PlayableBase(port.targetNodePort.parentNode.Playable);
                        int p1Index = port.targetNodePort.index;
                        PlayableNodeBase.PlayableBase p2 =
                            new PlayableNodeBase.PlayableBase(nodeBase.Playable);
                        int p2Index = port.index;
                        PlayableGraph.Connect(p1, p1Index, p2, p2Index);
                    }
                }
            }
            
            PlayableGraph.Play();
            _isRunning = true;

            CollectParams();
        }

        public void StopPlay()
        {
            PlayableGraph.Destroy();
            _isRunning = false;
        }

        public void RuntimeUpdate()
        {
            foreach (var playableNodeBase in nodeDic.Values)
            {
                playableNodeBase.OnRuntimeUpdate();
            }
        }
        
        public T GetNode<T>(string nodeName) where T : PlayableNodeBase
        {
            if (nodeDic.TryGetValue(nodeName, out var value))
            {
                return (T)value;
            }
            return null;
        }

        #endregion
    }
}