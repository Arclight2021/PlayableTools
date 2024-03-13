using System.Collections.Generic;
using UnityEngine;

namespace PlayableTools
{
    [System.Serializable]
    public class PlayableNodeEditorData
    {
        public string name;
        //Editor
        public string type;
        public Rect position;
        public string nodeNickName;
    }
    public class NodeEditorDataSO : ScriptableObject
    {
        public NodeDataSO nodeDataso;
        public List<PlayableNodeEditorData> NodeEditorDatas;
    }
}