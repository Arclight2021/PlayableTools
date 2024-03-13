using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PlayableTools.Nodes
{

    public struct LookAtJob : IAnimationJob
    {
        public TransformStreamHandle joint;
        public TransformSceneHandle target;
        public Vector3 axis;
        public float minAngle;
        public float maxAngle;

        public bool isReady;

        public void ProcessRootMotion(AnimationStream stream)
        {
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            if (!isReady)
            {
                return;
            }
            Solve(stream, joint, target, axis, minAngle, maxAngle);
        }

        private static void Solve(
            AnimationStream stream,
            TransformStreamHandle joint,
            TransformSceneHandle target,
            Vector3 jointAxis,
            float minAngle,
            float maxAngle)
        {
            var jointPosition = joint.GetPosition(stream);
            var jointRotation = joint.GetRotation(stream);
            var targetPosition = target.GetPosition(stream);
            
            var fromDir = jointRotation * jointAxis;
            var toDir = targetPosition - jointPosition;
            
            var axis = Vector3.Cross(fromDir, toDir).normalized;
            var angle = Vector3.Angle(fromDir, toDir);
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            var jointToTargetRotation = Quaternion.AngleAxis(angle, axis);
            
            jointRotation = jointToTargetRotation * jointRotation;
            
            joint.SetRotation(stream, jointRotation);
            
            // var jointRotation = joint.GetRotation(stream);
            // jointRotation.eulerAngles = jointRotation.eulerAngles + jointAxis;
            // joint.SetRotation(stream, jointRotation);
        }
    }
    public class LookAtNode:PlayableNodeBase
    {
        public LookAtJob lookAtJob = new LookAtJob()
        {
            isReady = false
        };

        private Animator _animator;
        private AnimationScriptPlayable animationScriptPlayable;
        public Transform Target { get; private set; }

        public Transform Joint { get; private set; }
        public Axis axis { get;private set; }
        public float minAngle { get;private set; }
        public float maxAngle { get;private set; }
        public enum Axis
        {
            Forward,
            Back,
            Up,
            Down,
            Left,
            Right
        }
        Vector3 GetAxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.Forward:
                    return Vector3.forward;
                case Axis.Back:
                    return Vector3.back;
                case Axis.Up:
                    return Vector3.up;
                case Axis.Down:
                    return Vector3.down;
                case Axis.Left:
                    return Vector3.left;
                case Axis.Right:
                    return Vector3.right;
            }

            return Vector3.forward;
        }
        public LookAtNode(PlayableNodeGraphManager nodeGraphManager, string name) : base(nodeGraphManager, name)
        {
        }
        public override void OnRuntimeCreate(ref PlayableRuntimeCreateContext context)
        {
            _animator = context.Animator;
            
            // lookAtJob = new LookAtJob()
            // {
            //     // joint = animator.BindStreamTransform(joint),
            //     // target = animator.BindSceneTransform(m_Target.transform),
            //     // axis = GetAxisVector(axis),
            //     // minAngle = Mathf.Min(minAngle, maxAngle),
            //     // maxAngle = Mathf.Max(minAngle, maxAngle)
            //     isReady = false
            // };

            animationScriptPlayable = AnimationScriptPlayable.Create(context.PlayableGraph,lookAtJob);
            _playable = animationScriptPlayable;
        }

        public void SetTarget(Transform transform)
        {
            this.Target = transform;
            this.lookAtJob.target = _animator.BindSceneTransform(transform);
            SetJobData();
        }

        public void SetJoint(Transform transform)
        {
            this.Joint = transform;
            this.lookAtJob.joint = _animator.BindStreamTransform(transform);
            SetJobData();
        }

        public void SetAxis(Axis axis)
        {
            this.axis = axis;
            this.lookAtJob.axis = GetAxisVector(axis);
            SetJobData();
        }

        public void SetMinAngle(float minAngle)
        {
            this.minAngle = minAngle;
            this.lookAtJob.minAngle = minAngle;
            SetJobData();   
        }

        public void SetMaxAngle(float maxAngle)
        {
            this.maxAngle = maxAngle;
            this.lookAtJob.maxAngle = maxAngle;
            SetJobData();
        }

        public void SetJobData()
        {
            if (!isRunning)
            {
                return;
            }
            animationScriptPlayable.SetJobData(lookAtJob);
        }

        public override void Export(BinaryWriter binaryWriter)
        {
            base.Export(binaryWriter);
            binaryWriter.Write((int)axis);
            binaryWriter.Write(minAngle);
            binaryWriter.Write(maxAngle);
        }

        public override void Import(BinaryReader binaryReader)
        {
            base.Import(binaryReader);
            axis = (Axis)binaryReader.ReadInt32();
            minAngle = binaryReader.ReadSingle();
            maxAngle = binaryReader.ReadSingle();
        }
    }
}