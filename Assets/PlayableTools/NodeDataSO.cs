using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace PlayableTools
{
    [System.Serializable]
    public class PlayableNodeData
    {
        public string name;
        
        public string type;
        public int inputCount;
        //todo: ReadNodeName as node name and port name
        public int outputCount;
        // public List<string> outputNodeNamesAndPorts;
        public string nodeData;
    }
    
    [System.Serializable]
    public class PlayableNodeLinkData
    {
        public string source;//e.g. Node#2 output
        public string target;//input 
    }
    
    // [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class NodeDataSO : ScriptableObject
    {
        public string graphName;
        public List<PlayableNodeData> nodeDatas;
        public List<PlayableNodeLinkData> linkDatas;
        public string outputNodeName;
        //todo: 字段
        public List<AnimationClip> clips;
        public List<AvatarMask> masks;
        public List<AnimatorController> controllers;

        public void Clear()
        {
            graphName = null;
            nodeDatas = null;
            linkDatas = null;
            outputNodeName = null;
            clips = null;
            masks = null;
            controllers = null;
        }
    }
}