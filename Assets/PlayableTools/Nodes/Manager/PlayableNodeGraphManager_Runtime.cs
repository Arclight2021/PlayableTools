using System.Collections.Generic;
using System.Linq;
using PlayableTools.Nodes;
using UnityEngine;

namespace PlayableTools
{
    public partial class PlayableNodeGraphManager
    {
        public class AniInfo
        {
            private AnimatorControllerParameter _parameter;
            public AnimatorControllerParameter Parameter => _parameter;
            private List<AnimatorControllerNode> _controllerNodes = new();

            public AniInfo(AnimatorControllerParameter parameter)
            {
                _parameter = parameter;
            }


            internal void AddAnimatorControllerNode(AnimatorControllerNode node)
            {
                _controllerNodes.Add(node);
            }

            public AnimatorControllerParameterType GetParamType()
            {
                return _parameter.type;
            }

            public int GetNodeCount()
            {
                return _controllerNodes.Count;
            }

            public void SetFloat(float value)
            {
                foreach (var animatorControllerNode in _controllerNodes)
                {
                    animatorControllerNode.AnimatorControllerPlayable.SetFloat(_parameter.nameHash,value);
                }
            }

            public void SetInteger(int value)
            {
                foreach (var animatorControllerNode in _controllerNodes)
                {
                    animatorControllerNode.AnimatorControllerPlayable.SetInteger(_parameter.nameHash,value);
                }
            }
            
            public void SetBool(bool value)
            {
                foreach (var animatorControllerNode in _controllerNodes)
                {
                    animatorControllerNode.AnimatorControllerPlayable.SetBool(_parameter.nameHash,value);
                }
            }

            public void SetTrigger()
            {
                foreach (var animatorControllerNode in _controllerNodes)
                {
                    animatorControllerNode.AnimatorControllerPlayable.SetTrigger(_parameter.nameHash);
                }
            }
        }
        private Dictionary<string, AniInfo> _parameters = new ();
        public void SetFloat(string name, float value)
        {
            _parameters[name].SetFloat(value);
        }

        public void SetInt(string name, int value)
        {
            _parameters[name].SetInteger(value);
        }

        public void SetBool(string name, bool value)
        {
            _parameters[name].SetBool(value);
        }

        public void SetTrigger(string name)
        {
            _parameters[name].SetTrigger();
        }

        public void CollectParams()
        {
            _parameters.Clear();
            var aniControllerNodes = nodeDic.Where(node => node.Value.GetType() == typeof(AnimatorControllerNode));
            Debug.Log($"Collect {aniControllerNodes.Count()}");
            foreach (var aniControllerNode in aniControllerNodes)
            {
                var node = (AnimatorControllerNode)aniControllerNode.Value; 
                var paramsArr = node.AnimatorController.parameters;
                foreach (var parameter in paramsArr)
                {
                    var name = parameter.name;
                    if (_parameters.TryGetValue(name,out var list))
                    {
                        // list.Add(node);
                        list.AddAnimatorControllerNode(node);
                    }
                    else
                    {
                        var aniInfo = new AniInfo(parameter);
                        // var newList = new List<AnimatorControllerNode>();
                        // newList.Add(node);
                        aniInfo.AddAnimatorControllerNode(node);
                        _parameters.Add(name,aniInfo);
                    }
                }
            }
        }

        public Dictionary<string,AniInfo> GetParams() => _parameters;
    }
}