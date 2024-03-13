using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayableTools.Nodes.Editor
{
    public class LookAtGraphNode:PlayableGraphNodeBase<LookAtNode>
    {
        public LookAtGraphNode(PlayableNodeGraphManager manager, LookAtNode targetNode = null) : base(manager, targetNode)
        {
        }

        protected override void OnInit()
        {
            
        }

        public override void OnCreateExtensionContainer(VisualElement container)
        {
            EnumField enumField = new EnumField();
            enumField.Init(target.axis);
            enumField.label = "Axis";
            enumField.RegisterValueChangedCallback(evt =>
            {
                target.SetAxis(evt.newValue as LookAtNode.Axis? ?? LookAtNode.Axis.Up);
            });

            FloatField minAngleField = new FloatField();
            minAngleField.label = "MinAngle";
            minAngleField.value = target.minAngle;
            minAngleField.RegisterValueChangedCallback(evt =>
            {
                target.SetMinAngle(evt.newValue);
            });
            
            FloatField maxAngleField = new FloatField();
            maxAngleField.label = "MaxAngle";
            maxAngleField.value = target.maxAngle;
            maxAngleField.RegisterValueChangedCallback(evt =>
            {
                target.SetMaxAngle(evt.newValue);
            });
            
            container.Add(enumField);
            container.Add(minAngleField);
            container.Add(maxAngleField);
        }

        public override void OnRuntimeCreateContainer(VisualElement container)
        {
            ObjectField jointField = new ObjectField();
            jointField.label = "Joint";
            jointField.allowSceneObjects = true;
            jointField.value = target.Joint;
            jointField.RegisterValueChangedCallback(evt =>
            {
                target.SetJoint(((GameObject)evt.newValue).transform);
            });

            ObjectField targetField = new ObjectField();
            targetField.label = "Target";
            targetField.allowSceneObjects = true;
            targetField.value = target.Target;
            targetField.RegisterValueChangedCallback(evt =>
            {
                target.SetTarget(((GameObject)evt.newValue).transform);
            });
            
            Button btn = new Button();
            btn.text = "Set Job Data";
            btn.clicked += () =>
            {
                target.lookAtJob.isReady = true;
                target.SetJobData();
            };
            

            container.Add(jointField);
            container.Add(targetField);
            container.Add(btn);
        }


    }
}