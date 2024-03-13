using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class AnimationClipGraphNode:PlayableGraphNodeBase<AnimationClipNode>
    {
        private ObjectField _objectField;
        private ProgressBar _progressBar;
        public override Color DefaultColor { get=>Color.blue; }

        public AnimationClipGraphNode(PlayableNodeGraphManager manager, AnimationClipNode targetNode = null) : base(manager, targetNode)
        {
        }
        protected override void OnInit()
        {
        }

        protected override PlayableNodeBase CreateTargetNode(PlayableNodeGraphManager manager)
        {
            return manager.CreateNode<AnimationClipNode>();
        }

        public override void OnCreateExtensionContainer(VisualElement container)
        {
            _objectField = new ObjectField()
            {
                objectType = typeof(AnimationClip)
            };
            _objectField.value = target.AnimationClip;
            _objectField.RegisterValueChangedCallback(evt =>
            {
                target.SetAnimationClip(evt.newValue as AnimationClip);
                _objectField.value = target.AnimationClip;
            });
            container.Add(_objectField);
        }

        public override void OnRuntimeCreateContainer(VisualElement container)
        {
            var replayBtn = new Button();
            replayBtn.text = "Reply";
            replayBtn.clicked +=()=>target.Playable.SetTime(0);
            container.Add(replayBtn);

            if (target.AnimationClip == null)
                return;
            _progressBar = new ProgressBar();
            _progressBar.lowValue = 0;
            _progressBar.highValue = target.AnimationClip.length;
            container.Add(_progressBar);
        }

        public override void OnRuntimeUpdate()
        {
            _progressBar.value = (float)target.Playable.GetTime() % target.AnimationClip.length;
        }
    }
}