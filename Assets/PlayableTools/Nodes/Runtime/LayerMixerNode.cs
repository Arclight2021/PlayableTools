using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{
    public class LayerMixerNode:PlayableNodeBase
    {
        public class LayerInfo
        {
            public bool isAdditive = false;
            public AvatarMask AvatarMask;
        }
        private Dictionary<Port, LayerInfo> _layerInfos = new ();
        private AnimationLayerMixerPlayable _animationLayerMixerPlayable;
        public LayerMixerNode(PlayableNodeGraphManager nodeGraphManager, string name) : base(nodeGraphManager, name)
        {
            this.SetInputPortCount(2);
        }
        
        public void SetAvatarMask(Port port,AvatarMask mask)
        {
            _layerInfos[port].AvatarMask = mask;
            if (isRunning)
            {
                _animationLayerMixerPlayable.SetLayerMaskFromAvatarMask((uint)port.index,mask);
            }
        }

        public AvatarMask GetAvatarMask(Port port)
        {
            return _layerInfos[port].AvatarMask;
        }

        public void SetLayerAdditive(Port port,bool isAdditive)
        {
            _layerInfos[port].isAdditive = isAdditive;
            if (isRunning)
            {
                _animationLayerMixerPlayable.SetLayerAdditive((uint)port.index,isAdditive);
            }
        }
        
        public bool GetIsAdditive(Port port)
        {
            return _layerInfos[port].isAdditive;
        }


        protected override void OnGenPort(Port port)
        {
            base.OnGenPort(port);
            _layerInfos.Add(port,new LayerInfo());
        }

        protected override void OnRemovePort(Port port)
        {
            base.OnRemovePort(port);
            _layerInfos.Remove(port);
        }

        public override void Export(BinaryWriter binaryWriter)
        {
            base.Export(binaryWriter);

            int count = _layerInfos.Count;
            binaryWriter.Write(count);
            foreach (var kp in _layerInfos)
            {
                binaryWriter.Write(kp.Key.index);
                var index = NodeGraphManager.AddAvatarMask(kp.Value.AvatarMask);
                binaryWriter.Write(index);
                binaryWriter.Write(kp.Value.isAdditive);
            }
        }

        public override void Import(BinaryReader binaryReader)
        {
            base.Import(binaryReader);
            // index = binaryReader.ReadInt32();
            int count = binaryReader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int index = binaryReader.ReadInt32();
                var port = inputPorts[index];
                int maskIndex = binaryReader.ReadInt32();
                var mask = NodeGraphManager.GetAvatarMask(maskIndex);
                _layerInfos[port] = new LayerInfo()
                {
                    AvatarMask = mask,
                    isAdditive = binaryReader.ReadBoolean()
                };
            }
        }

        public override void OnRuntimeCreate(ref PlayableRuntimeCreateContext context)
        {
            _animationLayerMixerPlayable =AnimationLayerMixerPlayable.Create(context.PlayableGraph);
            _playable = _animationLayerMixerPlayable;
            _animationLayerMixerPlayable.SetInputCount(this.inputCount);
            foreach (var layerInfoKP in _layerInfos)
            {
                if (layerInfoKP.Value.AvatarMask is not null)
                {
                    _animationLayerMixerPlayable.SetLayerMaskFromAvatarMask((uint)layerInfoKP.Key.index,layerInfoKP.Value.AvatarMask);
                }
                _animationLayerMixerPlayable.SetLayerAdditive((uint)layerInfoKP.Key.index,layerInfoKP.Value.isAdditive);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            foreach (var kp in _layerInfos)
            {
                sb.Append(kp.Key.index);
                sb.Append(":");
                sb.Append(kp.Value.AvatarMask?.name);
                sb.Append("   ");
                sb.Append(kp.Value.isAdditive);
            }
            return sb.ToString();
        }
    }
}