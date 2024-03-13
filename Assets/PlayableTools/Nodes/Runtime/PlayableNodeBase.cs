using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{
    public struct PlayableRuntimeCreateContext
    {
        public PlayableGraph PlayableGraph;
        public Animator Animator;
    }
    public abstract class PlayableNodeBase
    {
        public class Port
        {
            public enum PortType
            {
                Input,OutPut
            }
            public PortType Type;
            public bool IsConnected => targetNodePort != null;
            public bool IsRemoved => parentNode == null;
            public int index;
            public PlayableNodeBase parentNode;
            public PlayableNodeBase.Port targetNodePort;

            public float portWeight { get;internal set; }

            public string name { get; private set; }

#if UNITY_EDITOR
            //use in PlayableGraphNodeBase to check if the port is generated
            public bool isGenerated = false;
#endif

            public Port(PortType type,PlayableNodeBase parentNode,int index)
            {
                this.Type = type;
                this.parentNode = parentNode;
                
                // string mark = type == PortType.Input ? "I" : "O";
                // name = mark + index;
                name = index.ToString();
                this.index = index;
            }

            public void Connect(Port port)
            {
                this.targetNodePort = port;
                port.targetNodePort = this;
            }

            public void DisConnect()
            {
                if (!IsConnected)
                    return;
                this.targetNodePort.targetNodePort = null;
                this.targetNodePort = null;
            }

            public void Remove()
            {
                DisConnect();
                parentNode = null;
                index = -1;
            }
        }
        
        
        public PlayableNodeGraphManager NodeGraphManager;
        public PlayableGraph PlayableGraph => NodeGraphManager.PlayableGraph;
        
        public string name;
        public int inputCount;
        public List<Port> inputPorts;
        public int outputCount;
        public List<Port> outputPorts;
        
        public bool isRunning => NodeGraphManager.isRunning;
        
        public struct PlayableBase:IPlayable
        {
            private IPlayable _playable;
            public PlayableBase(IPlayable playable)
            {
                _playable = playable;
            }
            public PlayableHandle GetHandle()
            {
                return _playable.GetHandle();
            }
        }

        private PlayableBase _playableBase;
        protected IPlayable _playable
        {
            get => _playableBase;
            set { _playableBase = new PlayableBase(value); }
        }
        public PlayableBase Playable => _playableBase;
        public PlayableNodeBase(PlayableNodeGraphManager nodeGraphManager, string name)
        {
            NodeGraphManager = nodeGraphManager;
            this.name = name;

            inputCount = 0;
            inputPorts = new();
            outputPorts = new();
            
            SetOutputPortCount(1);
            
            Debug.Log($"Create {this.GetType().Name}");
        }

        public virtual void Export(BinaryWriter binaryWriter)
        {
            // return new byte[]{};
            int count = inputPorts.Count;
            binaryWriter.Write(count);
            foreach (var port in inputPorts)
            {
                binaryWriter.Write(port.portWeight);
            }
            
        }

        public virtual void Import(BinaryReader binaryReader)
        {
            int count = binaryReader.ReadInt32();
            SetInputPortCount(count);
            for (int i = 0; i < count; i++)
            {
                var port = inputPorts[i];
                port.portWeight = binaryReader.ReadSingle();
            }
        }
        
        public void SetInputPortWeight(int index,float weight)
        {
            if (index < inputPorts.Count)
            {
                inputPorts[index].portWeight = weight;
            }
        }

        public void SetInputPortCount(int count)
        {
            inputCount = count;
            int diffValue = inputCount - inputPorts.Count;
            int inputPortsCount = inputPorts.Count;
            
            if (inputCount > inputPortsCount)
            {
                for (int i = inputPortsCount; i < inputPortsCount + diffValue; i++)
                {
                    //add port
                    var port =GenPort(Port.PortType.Input,i);
                    inputPorts.Add(port); 
                }
            }
            else if (inputCount == inputPortsCount)
            {
                
            }
            else
            {
                for (int i = inputPortsCount-1; i > inputPortsCount + diffValue-1; i--)
                {
                    //remove port
                    // var port = inputPorts[i];
                    // port.Remove();
                    // inputPorts.RemoveAt(i);
                    RemovePort(i);
                }
            }
        }

        private void SetOutputPortCount(int count)
        {
            outputCount = count;
            int diffValue = outputCount - outputPorts.Count;
            int outputPortsCount = outputPorts.Count;
            
            if (outputCount > outputPortsCount)
            {
                for (int i = outputPortsCount; i < outputPortsCount+ diffValue; i++)
                {
                    //add port
                    var port =GenPort(Port.PortType.OutPut,i);
                    outputPorts.Add(port); 
                }
            }else if (outputCount == outputPortsCount)
            {
                
            }
            else
            {
                for (int i = outputPortsCount-1; i > outputPortsCount + diffValue-1; i--)
                {
                    //remove port
                    var port = outputPorts[i];
                    port.Remove();
                    outputPorts.RemoveAt(i);
                }
            }
        }

        private Port GenPort(Port.PortType portType,int index)
        {
            var port = new Port(portType, this,index);
            OnGenPort(port);
            return port;
        }

        private void RemovePort(int index)
        {
            var port = inputPorts[index];
            port.Remove();
            OnRemovePort(port);
            inputPorts.RemoveAt(index);
        }

        protected virtual void OnGenPort(Port port)
        {
            
        }

        protected virtual void OnRemovePort(Port port)
        {
            
        }
        
        public abstract void OnRuntimeCreate(ref PlayableRuntimeCreateContext context);
        public virtual void OnRuntimeUpdate()
        {
            PlayableBase playableBase = new PlayableBase(Playable);
            foreach (var port in inputPorts)
            {
                playableBase.SetInputWeight(port.index,port.portWeight);
            }
            // foreach (var port in outputPorts)
            // {
            //     playableBase.(port.index,port.portWeight);
            // }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.ToString());
            stringBuilder.AppendLine();
            stringBuilder.Append(name);
            stringBuilder.Append($"\n-----inputPort:{inputCount}-----\n");
            foreach (var port in inputPorts)
            {
                stringBuilder.Append($"InputPort{port.index}:");
                stringBuilder.Append(port?.targetNodePort?.parentNode.name);
                stringBuilder.AppendLine();
            }
            stringBuilder.Append($"-----outputPort:{outputCount}-----\n");
            foreach (var port in outputPorts)
            {
                stringBuilder.Append($"OutputPort{port.index}:");
                stringBuilder.Append(port?.targetNodePort?.parentNode.name);
                // stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}