using System;
using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{
    public class AnimationClipNode:PlayableNodeBase
    {
        private AnimationClip _animationClip;
        public AnimationClip AnimationClip => _animationClip;
        private AnimationClipPlayable _animationClipPlayable;
        public AnimationClipNode(PlayableNodeGraphManager nodeGraphManager, string name) : base(nodeGraphManager, name)
        {
            this.SetInputPortCount(0);
        }

        public void SetAnimationClip(AnimationClip clip)
        {
            this._animationClip = clip;
            // if (isRunning)
            // {
            //     
            //     _animationClipPlayable.Destroy();
            //     _animationClipPlayable = AnimationClipPlayable.Create(PlayableGraph, AnimationClip);
            //     _playable = _animationClipPlayable;
            // }
        }

        public override void Export(BinaryWriter binaryWriter)
        {
            var clipIndex = NodeGraphManager.AddAnimationClip(_animationClip);
            binaryWriter.Write(clipIndex);
        }

        public override void Import(BinaryReader binaryReader)
        {
            var clipIndex = binaryReader.ReadInt32();
            _animationClip = NodeGraphManager.GetAnimationClip(clipIndex);
        }

        public override void OnRuntimeCreate(ref PlayableRuntimeCreateContext context)
        {
            _animationClipPlayable = AnimationClipPlayable.Create(context.PlayableGraph, AnimationClip);
            _playable = _animationClipPlayable;
        }
    }
}