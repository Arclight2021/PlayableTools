using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{
    public class SlotNode:PlayableNodeBase
    {
        public string slotName { get; private set; }
        private PlayableGraph _playableGraph;

        public int MaxCacheClipAmount = 8;
        public class AnimationClipInfo
        {
            public int portIndex;
            public AnimationClipPlayable Playable;
            public float fadeTime;  //渐入时长
            public float currentFadeAnimTime;  //当前渐入动画时长
        }
        private Dictionary<AnimationClip, AnimationClipInfo> _playables = new();
        private AnimationClipInfo _currentPlayClip;
        private AnimationClipInfo _clipToRemove = null;
        public SlotNode(PlayableNodeGraphManager nodeGraphManager, string name) : base(nodeGraphManager, name)
        {
            slotName = "Default Slot Name";
            this.SetInputPortCount(1);
        }

        public void SetSlotName(string slotName)
        {
            this.slotName = slotName;
        }

        public void SetMaxCachedAmount(int maxCachedAmount)
        {
            this.MaxCacheClipAmount = maxCachedAmount;
        }

        public void PlayAnimationClip(AnimationClip clip,float fadeTime = 0.07f)
        {
            if (_currentPlayClip != null)
            {
                _currentPlayClip.currentFadeAnimTime = 0;
                Playable.SetInputWeight(_currentPlayClip.portIndex,0);
            }
            if (_playables.TryGetValue(clip,out var inDicInfo))
            {
                inDicInfo.Playable.SetTime(0);
                inDicInfo.fadeTime = fadeTime;
                inDicInfo.currentFadeAnimTime = 0;
                _currentPlayClip = inDicInfo;
            }
            else
            {
                int index = -1;
                AnimationClipInfo info;
                if (_playables.Count < MaxCacheClipAmount)  //缓存未满
                {
                    index = _playables.Count+1;
                }
                else
                {
                    var removedIndex = RandomRemoveInCached(clip);
                    index = removedIndex;
                }
                info = new AnimationClipInfo()
                {
                    portIndex = index,
                    Playable = AnimationClipPlayable.Create(_playableGraph, clip),
                    fadeTime = fadeTime,
                };
                
                _playables.Add(clip,info);
                _currentPlayClip = info;
                _playableGraph.Connect(info.Playable, 0, Playable, info.portIndex);
            }
        }

        private int RandomRemoveInCached(AnimationClip newClip)
        {
            AnimationClipInfo info = null;
            int index = -1;
            foreach (var animationClipInfo in _playables)
            {
                if (animationClipInfo.Key == newClip)
                {
                    continue;
                }
                info = animationClipInfo.Value;
                _playables.Remove(animationClipInfo.Key);
                // _clipInfosToRemove.Add(info);
                if (_clipToRemove != null)
                {
                    _clipToRemove.Playable.Destroy();
                }
                _clipToRemove = info;
                // info.IsWeightIncrease = false;
                index = info.portIndex;
                Playable.DisconnectInput(info.portIndex);
                info.portIndex = MaxCacheClipAmount+1;
                Playable.ConnectInput(info.portIndex,info.Playable,0);
                break;
            }
            // return info.portIndex;
            return index;
        }

        public override void Export(BinaryWriter binaryWriter)
        {
            base.Export(binaryWriter);
            binaryWriter.Write(slotName);
            binaryWriter.Write(MaxCacheClipAmount);
        }

        public override void Import(BinaryReader binaryReader)
        {
            base.Import(binaryReader);
            slotName = binaryReader.ReadString();
            MaxCacheClipAmount = binaryReader.ReadInt32();
        }

        public override void OnRuntimeCreate(ref PlayableRuntimeCreateContext context)
        {
            _playableGraph = context.PlayableGraph;
            _playable = AnimationLayerMixerPlayable.Create(_playableGraph, MaxCacheClipAmount+2);//一个为了其他链接的节点，一个为了淡出动画
            NodeGraphManager.AddSlot(this.slotName,this);
            this.inputPorts[0].portWeight = 1;
            Playable.SetInputWeight(0,1);
        }

        public override void OnRuntimeUpdate()
        {
            // base.OnRuntimeUpdate();
            // foreach (var animationClipInfoKp in _playables)
            // {
            //     var animationClipInfo = animationClipInfoKp.Value;
            //     if (animationClipInfo.IsWeightIncrease)
            //     {
            //         animationClipInfo.currentFadeAnimTime += Time.deltaTime;
            //         var weight = animationClipInfo.currentFadeAnimTime / animationClipInfo.fadeTime;
            //         if (weight >=1)
            //         {
            //             weight = 1;
            //         }
            //         Playable.SetInputWeight(animationClipInfo.portIndex,weight);
            //     }
            //     else
            //     {
            //         animationClipInfo.currentFadeAnimTime -= Time.deltaTime;
            //         var weight = animationClipInfo.currentFadeAnimTime / animationClipInfo.fadeTime;
            //         if (weight <=0)
            //         {
            //             weight = 0;
            //         }
            //         Playable.SetInputWeight(animationClipInfo.portIndex,weight);
            //     }
            // }

            if (_currentPlayClip != null)
            {
                var animationClipInfo = _currentPlayClip;
                var time = animationClipInfo.Playable.GetTime();
                var totalTime = animationClipInfo.Playable.GetAnimationClip().length;
                if (time > totalTime-animationClipInfo.fadeTime)
                {
                    animationClipInfo.currentFadeAnimTime -= Time.deltaTime;
                    var weight = animationClipInfo.currentFadeAnimTime / animationClipInfo.fadeTime;
                    if (weight <=0)
                    {
                        weight = 0;
                        _currentPlayClip = null;
                    }
                    Playable.SetInputWeight(animationClipInfo.portIndex,weight);
                }
                else if (time < animationClipInfo.fadeTime)
                {
                    animationClipInfo.currentFadeAnimTime += Time.deltaTime;
                    var weight = animationClipInfo.currentFadeAnimTime / animationClipInfo.fadeTime;
                    if (weight >=1)
                    {
                        weight = 1;
                    }
                    Playable.SetInputWeight(animationClipInfo.portIndex,weight);
                }else
                {
                    Playable.SetInputWeight(animationClipInfo.portIndex,1);
                }
            }

            if (_clipToRemove != null)
            {
                var animationClipInfo = _clipToRemove;
                animationClipInfo.currentFadeAnimTime -= Time.deltaTime;
                var weight = animationClipInfo.currentFadeAnimTime / animationClipInfo.fadeTime;
                if (weight <=0)
                {
                    Playable.DisconnectInput(animationClipInfo.portIndex);
                    _playableGraph.DestroyPlayable(animationClipInfo.Playable);
                    _clipToRemove = null;
                    return;
                }
                Playable.SetInputWeight(animationClipInfo.portIndex,weight);
            }
        }
    }
}