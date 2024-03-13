using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class SlotGraphNode:PlayableGraphNodeBase<SlotNode>
    {
        public override Color DefaultColor { get=>Color.red; }
        private AnimationClip _clip;

        public SlotGraphNode(PlayableNodeGraphManager manager, SlotNode targetNode = null) : base(manager, targetNode)
        {
        }

        protected override void OnInit()
        {

        }

        public override void OnCreateExtensionContainer(VisualElement container)
        {
            var textField = new TextField();
            textField.label = "SlotName";
            textField.value = target.slotName;
            textField.labelElement.style.minWidth = 60;
            textField.RegisterValueChangedCallback(evt => { target.SetSlotName(evt.newValue); });
            container.Add(textField);

            var integerField = new IntegerField();
            integerField.label = "MaxCacheClipAmount";
            integerField.value = target.MaxCacheClipAmount;
            integerField.RegisterValueChangedCallback(evt => { target.SetMaxCachedAmount(evt.newValue); });
            container.Add(integerField);
        }

        public override void OnRuntimeCreateContainer(VisualElement container)
        {
            container.Add(new Label("Runtime Test"));

            var animationClipField = new ObjectField();
            animationClipField.objectType = typeof(AnimationClip);
            animationClipField.RegisterValueChangedCallback(evt =>
            {
                _clip = evt.newValue as AnimationClip;
            });
            container.Add(animationClipField);

            var playButton = new Button();
            playButton.text = "Test Slot";
            playButton.clicked += () =>
            {
                if (!target.Playable.IsNull() && _clip is not null)
                {
                    target.PlayAnimationClip(_clip);
                }
            };
            container.Add(playButton);
        }


        protected override void OnGenPort(PlayableNodeBase.Port port, Port instantiatePort)
        {
        }
    }
}