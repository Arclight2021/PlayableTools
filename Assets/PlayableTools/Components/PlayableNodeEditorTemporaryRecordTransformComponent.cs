using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableTools.Components
{
    public class PlayableNodeEditorTemporaryRecordTransformComponent : MonoBehaviour
    {
        [Serializable]
        public class TransformDatas
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public Transform Transform;
        }

        public List<TransformDatas> TransformDatasList = new List<TransformDatas>();

        public void Record()
        {
            var trans = transform;
            RecordTransform(trans);
            RecursionChilds(trans);
        }

        private void RecursionChilds(Transform trans)
        {
            if (trans.childCount > 0)
            {
                int childCount = trans.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var childTrans = trans.GetChild(i);
                    RecursionChilds(childTrans);
                    RecordTransform(childTrans);
                }
            }
        }

        private void RecordTransform(Transform trans)
        {
            TransformDatasList.Add(new TransformDatas()
            {
                position = trans.localPosition,
                rotation = trans.localRotation,
                scale = trans.localScale,
                Transform = trans
            });
        }

        public void Revert()
        {
            foreach (var transformData in TransformDatasList)
            {
                var trans = transformData.Transform;
                trans.localPosition = transformData.position;
                trans.localRotation = transformData.rotation;
                trans.localScale = transformData.scale;
            }
            
            DestroyImmediate(this);
        }
    }
}