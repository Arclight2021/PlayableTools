using System.Collections.Generic;
using System.Linq;
using PlayableTools.Editor.Extension;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class PlayableGraphNodeBase:Node
    {
        public virtual PlayableNodeBase tar { get; }
        public Dictionary<PlayableNodeBase.Port, Port> portLookup = new Dictionary<PlayableNodeBase.Port, Port>();
        public virtual Color DefaultColor { get=>Color.gray; }
        public void Refresh()
        {
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
        
        public void SetPosition(Vector2 newpos)
        {
            var rect = this.GetPosition();
            rect.position = newpos;
            this.SetPosition(rect);
        }

        public void SetColor(Color color)
        {
            this.titleContainer.style.backgroundColor = color;
        }

        public void SetDefaultColor()
        {
            this.SetColor(DefaultColor);
        }

        public virtual void OnCreateExtensionContainer(VisualElement container)
        {
            
        }

        public virtual void OnRuntimeCreateContainer(VisualElement container)
        {
            
        }

        public virtual void OnRuntimeUpdate()
        {
            
        }
    }
    public abstract class PlayableGraphNodeBase<T>:PlayableGraphNodeBase where T:PlayableNodeBase
    {
        public T target;

        public override PlayableNodeBase tar { get=>target; }

        public PlayableGraphNodeBase(PlayableNodeGraphManager manager,T targetNode = null)
        {
            // Debug.Log(targetNode is null);
            if (targetNode == null)
            {
                target = (T)CreateTargetNode(manager);
            }
            else
            {
                target = targetNode;
            }
            
            this.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                Debug.Log("Attach to panel");    
            });

            this.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                Debug.Log("Detach from panel");    
                manager.RemoveNode(target);
            });
            
            this.title = this.GetType().Name;
            
            OnInit();
            
            var container = this.extensionContainer;
            OnCreateExtensionContainer(container);
            
            SetPortsByTarget();
            SetDefaultColor();
        }

        protected abstract void OnInit();

        protected virtual PlayableNodeBase CreateTargetNode(PlayableNodeGraphManager manager)
        {
            return manager.CreateNode<T>();
        }

        protected Port AddPort(PlayableNodeBase.Port port)
        {
            var portDir = (port.Type == PlayableNodeBase.Port.PortType.Input) ? Direction.Input : Direction.Output;
            
            var instantiatePort = this.InstantiatePort(Orientation.Horizontal, portDir, Port.Capacity.Single,typeof(object));
            instantiatePort.portName = port.name;
            if (portDir == Direction.Input)
            {
                this.inputContainer.Add(instantiatePort);
            }
            else
            {
                this.outputContainer.Add(instantiatePort);
            }

            instantiatePort.userData = port;
            // Debug.Log(port.name);
            // portLookup.Add(port,instantiatePort);
            
            
            instantiatePort.AddConnectLinstener(evt =>
            {
                var edge = evt.connections.FirstOrDefault();
                int inputIndex = int.Parse(edge.input.portName);
                int outputIndex = int.Parse(edge.output.portName);
                var inputNode = ((PlayableGraphNodeBase)edge.input.node).tar;
                var outputNode = ((PlayableGraphNodeBase)edge.output.node).tar;
                
                
                port.parentNode.NodeGraphManager.Connect(outputNode,outputIndex,inputNode,inputIndex);
                // Debug.Log(inputNode.name);
                // Debug.Log(outputNode.name);
                
                Debug.Log($"OnConnect {evt}");
            });
            instantiatePort.AddDisConnectLinstener(evt =>
            {
                //input port index
                // var edge = evt.connections.FirstOrDefault();
                // int inputIndex = int.Parse(edge.input.portName);
                // int outputIndex = int.Parse(edge.output.portName);
                // var inputNode = ((PlayableGraphNodeBase)edge.input.node).tar;
                // var outputNode = ((PlayableGraphNodeBase)edge.output.node).tar;
                
                Debug.Log((evt.userData as PlayableNodeBase.Port).parentNode.name);
                if ((evt.userData as PlayableNodeBase.Port).Type == PlayableNodeBase.Port.PortType.OutPut)
                {
                    return;
                }

                var inputNode = ((PlayableGraphNodeBase)evt.node).tar;
                var inputIndex = int.Parse(evt.portName);
                
                port.parentNode.NodeGraphManager.DisConnect(inputNode,inputIndex);
                Debug.Log($"OnDisConnect{evt.userData}");
            });
            
            OnGenPort(port,instantiatePort);
            
            Refresh();
            return instantiatePort;
        }

        protected virtual void OnGenPort(PlayableNodeBase.Port port,Port instantiatePort)
        {
            if (port.Type == PlayableNodeBase.Port.PortType.OutPut)
            {
                return;
            }
            var weight = new FloatField("weight");
            weight.labelElement.style.minWidth = 50;
            weight.labelElement.style.width = 50;
            weight.style.width = 100;
            instantiatePort.contentContainer.Add(weight);

            weight.value = port.portWeight;
            weight.RegisterValueChangedCallback(evt =>
            {
                // port.portWeight = evt.newValue;
                target.SetInputPortWeight(port.index,evt.newValue);
            });
        }

        protected void RemovePort(Direction portDir,int index)
        {
            if (portDir == Direction.Input)
            {
                this.inputContainer.RemoveAt(index);
            }
            else
            {
                this.outputContainer.RemoveAt(index);
            }
        }

        public void SetPortsByTarget()
        {
            //todo:两层port间的绑定还没写好
            // AddPort(Direction.Input, 0);
            // AddPort(Direction.Output, 0);
            //port数量不会随便变化，必须使用set进行修改，因此当删除port时不会删除port，只会断开,等待连接
            //自己的那一层行为应该尽量接近playable的逻辑
            //视图层删除掉所有节点然后重新连接就行
            
            // Debug.Log($"input{target.inputPorts.Count} output{target.outputCount}");
            
            // Debug.Log(target is null);
            // Debug.Log(target.inputPorts is null);

            
            // this.inputContainer.Clear();
            // this.outputContainer.Clear();

            List<Edge> edgesToRemove = new();
            List<Port> portsToRemove = new();
            
            foreach (var p in inputContainer.Children())
            {
                var port = p as Port;
                if (port == null)
                {
                    continue;
                }

                var targetPort = (PlayableNodeBase.Port)p.userData;
                if (targetPort.IsRemoved)
                {
                    portsToRemove.Add(port);
                    edgesToRemove.AddRange(port.connections);
                }
            }
            
            foreach (var p in outputContainer.Children())
            {
                var port = p as Port;
                if (port == null)
                {
                    continue;
                }

                var targetPort = (PlayableNodeBase.Port)p.userData;
                if (targetPort.IsRemoved)
                {
                    portsToRemove.Add(port);
                    edgesToRemove.AddRange(port.connections);
                }
            }

            foreach (var edge in edgesToRemove)
            {
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                edge.input = null;
                edge.output = null;
                edge.RemoveFromHierarchy();
            }

            foreach (var port in portsToRemove)
            {
                port.RemoveFromHierarchy();
            }
            
            foreach (var port in target.inputPorts)
            {
                if (!port.isGenerated)
                {
                    AddPort(port);
                    port.isGenerated = true;
                }
            }
            
            foreach (var port in target.outputPorts)
            {
                if (!port.isGenerated)
                {
                    AddPort(port);
                    port.isGenerated = true;
                }
            }
            
            Refresh();
        }
        

    }
}