using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class AnimatorControllerGraphNode:PlayableGraphNodeBase<AnimatorControllerNode>
    {
        private ObjectField _objectField;
        private List<Action> _updateValueList;
        public override Color DefaultColor { get=>Color.cyan; }
        public AnimatorControllerGraphNode(PlayableNodeGraphManager manager, AnimatorControllerNode targetNode = null) : base(manager, targetNode)
        {
        }

        protected override void OnInit()
        {

        }

        public override void OnCreateExtensionContainer(VisualElement container)
        {
            _objectField = new ObjectField()
            {
                objectType = typeof(AnimatorController)
            };
            _objectField.value = target.AnimatorController;
            _objectField.RegisterValueChangedCallback(evt =>
            {
                target.SetAnimatorController(evt.newValue as AnimatorController);
                _objectField.value = target.AnimatorController;
            });
            container.Add(_objectField);
        }

        public override void OnRuntimeCreateContainer(VisualElement container)
        {
            _updateValueList = new();
            if (target.AnimatorController is null)
                return;
            container.Add(new Label($"Parameters Total:{target.AnimatorController.parameters.Length}"));
            foreach (var parameter in target.AnimatorController.parameters)
            {
                container.Add(CreateParamVE(parameter));
            }
        }
        
        private VisualElement CreateParamVE(AnimatorControllerParameter parameter)
        {
            var container = new VisualElement();
            container.name = "PlayableTools-AnimatorControllerParameter-Container";
            // container.style.borderTopColor = new Color(0.48f,0.48f,0.48f);
            // container.style.borderTopWidth = 1;
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    var floatField = new FloatField();
                    floatField.label = parameter.name;
                    floatField.labelElement.style.minWidth = 80;
                    floatField.style.flexGrow = 1;
                    container.Add(floatField);

                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        target.AnimatorControllerPlayable.SetFloat(parameter.nameHash,evt.newValue);
                    });
                    _updateValueList.Add(() =>
                    {
                        floatField.value = target.AnimatorControllerPlayable.GetFloat(parameter.nameHash);
                    });
                    break;
                case AnimatorControllerParameterType.Int:
                    var intField = new IntegerField();
                    intField.label = parameter.name;
                    intField.labelElement.style.minWidth = 80;
                    intField.style.flexGrow = 1;
                    container.Add(intField);

                    intField.RegisterValueChangedCallback(evt =>
                    {
                        target.AnimatorControllerPlayable.SetInteger(parameter.nameHash, evt.newValue);
                    });
                    _updateValueList.Add(() =>
                    {
                        intField.value = target.AnimatorControllerPlayable.GetInteger(parameter.nameHash);
                    });
                    break;
                case AnimatorControllerParameterType.Bool:
                    var toggle = new Toggle();
                    toggle.label = parameter.name;
                    toggle.labelElement.style.minWidth = 80;
                    toggle.style.flexGrow = 1;
                    container.Add(toggle);

                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        target.AnimatorControllerPlayable.SetBool(parameter.nameHash,evt.newValue);
                    });
                    _updateValueList.Add(() =>
                    {
                        toggle.value = target.AnimatorControllerPlayable.GetBool(parameter.nameHash);
                    });
                    break;
                case AnimatorControllerParameterType.Trigger:
                    var label = new Label();
                    label.text = parameter.name;
                    var btn = new Button();
                    btn.text = "Trigger";
                    btn.style.flexGrow = 1;
                    container.Add(label);
                    container.Add(btn);
                    btn.clicked += () =>
                    {
                        target.AnimatorControllerPlayable.SetTrigger(parameter.nameHash);
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return container;
        }

        public override void OnRuntimeUpdate()
        {
            foreach (var action in _updateValueList)
            {
                action?.Invoke();
            }
        }
    }
}