using System.IO;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{
    public class AnimatorControllerNode:PlayableNodeBase
    {
        // private int animatorControllerIndex = int.MaxValue;
        // public AnimatorController AnimatorController => NodeGraphManager.GetAnimatorController(animatorControllerIndex);
        public AnimatorControllerPlayable AnimatorControllerPlayable { get; private set; }
        public AnimatorController AnimatorController { get; private set; }

        public AnimatorControllerNode(PlayableNodeGraphManager nodeGraphManager, string name) : base(nodeGraphManager, name)
        {
            this.SetInputPortCount(0);
        }

        public void SetAnimatorController(AnimatorController animatorController)
        {
            // NodeGraphManager.RemoveAnimationClip(animatorControllerIndex);
            // animatorControllerIndex = NodeGraphManager.AddAnimatorController(animatorController);
            this.AnimatorController = animatorController;
        }
        
        public override void Export(BinaryWriter binaryWriter)
        {
            int index = NodeGraphManager.AddAnimatorController(this.AnimatorController);
            binaryWriter.Write(index);
        }

        public override void Import(BinaryReader binaryReader)
        {
            var index = binaryReader.ReadInt32();
            this.AnimatorController = NodeGraphManager.GetAnimatorController(index);
            // animatorControllerIndex = binaryReader.ReadInt32();
        }

        public override void OnRuntimeCreate(ref PlayableRuntimeCreateContext context)
        {
            AnimatorControllerPlayable = AnimatorControllerPlayable.Create(context.PlayableGraph, AnimatorController);
            _playable = AnimatorControllerPlayable;
            // pl.SetFloat("Name",1);
            // pl.GetParameter(0).nameHash;
            
            // AnimatorControllerPlayable.SetFloat();
        }
    }
}