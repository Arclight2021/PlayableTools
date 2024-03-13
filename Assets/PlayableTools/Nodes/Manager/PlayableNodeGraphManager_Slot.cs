
using System.Collections.Generic;
using PlayableTools.Nodes;
using UnityEngine;

namespace PlayableTools
{
    public partial class PlayableNodeGraphManager
    {
        private Dictionary<string, SlotNode> _slotPlayables = new ();
        internal void AddSlot(string slotName, SlotNode node)
        {
            _slotPlayables[slotName] = node;
        }
        
        public void PlayAnimationClip(string slotName, AnimationClip clip, float fadeInTime = 0.07F)
        {
            if (_slotPlayables.TryGetValue(slotName,out var slotNode))
            {
                slotNode.PlayAnimationClip(clip,fadeInTime);
            }
        }
    }
}
