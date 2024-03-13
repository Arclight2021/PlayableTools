using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class LayerMixerGraphNode : PlayableGraphNodeBase<LayerMixerNode>
    {
        // private ObjectField _objectField;
        private IntegerField _integerField;
    
        public override Color DefaultColor
        {
            get => Color.magenta;
        }
    
        public LayerMixerGraphNode(PlayableNodeGraphManager manager, LayerMixerNode targetNode = null) : base(manager,
            targetNode)
        {
        }
    
        protected override void OnInit()
        {
        }

        protected override void OnGenPort(PlayableNodeBase.Port port, Port instantiatePort)
        {
            base.OnGenPort(port, instantiatePort);
            if (port.Type == PlayableNodeBase.Port.PortType.OutPut)
            {
                return;
            }

            var boolField = new Toggle();
            boolField.label = "Is additive?";
            boolField.value = target.GetIsAdditive(port);
            boolField.style.width = 100;
            boolField.labelElement.style.minWidth = 80;
            boolField.RegisterValueChangedCallback(evt =>
            {
                target.SetLayerAdditive(port,evt.newValue);
            });
            instantiatePort.contentContainer.Add(boolField);
            
            
            //avatar mask
           var objectField = new ObjectField()
            {
                objectType = typeof(AvatarMask)
            };
            objectField.value = target.GetAvatarMask(port);
            objectField.RegisterValueChangedCallback(evt =>
            {
                target.SetAvatarMask(port,evt.newValue as AvatarMask);
                // objectField.value = target.AvatarMask;
            });
            objectField.style.width = 100;
            instantiatePort.contentContainer.Add(objectField);
        }

        public override void OnCreateExtensionContainer(VisualElement container)
        {
            _integerField = new IntegerField();
            _integerField.label = "input count";
            _integerField.value = target.inputCount;
            _integerField.RegisterValueChangedCallback(evt =>
            {
                target.SetInputPortCount(evt.newValue);
                SetPortsByTarget();
            });
            
            container.Add(_integerField);
        }
    }
    }