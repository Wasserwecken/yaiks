using System;
using UnityEngine;

namespace YAIKS
{
    [Serializable]
    public struct IKJoint
    {
        public Transform Root;
        public Transform Tip;
        public Quaternion InitialLocalRotation;
        public Quaternion TargetLocalRotation;
        public IIKConstraint[] Constraints;

        public IKJoint(Transform root, Transform tip)
        {
            Root = root;
            Tip = tip;
            InitialLocalRotation = root.localRotation;
            TargetLocalRotation = root.localRotation;
            Constraints = GetConstraintList(Root);
        }

        public void SavePose()
        {
            InitialLocalRotation = Root.localRotation;
            foreach (var constraint in Constraints)
                constraint.SetJoint(this);
        }

        public void ResetPose()
        {
            Root.localRotation = InitialLocalRotation;
        }

        public void PullConstraints()
        {
            Constraints = GetConstraintList(Root);
            foreach (var constraint in Constraints)
                constraint.SetJoint(this);
        }

        private static IIKConstraint[] GetConstraintList(Transform transform)
        {
            var result = transform.GetComponents<IIKConstraint>();
            return result ?? (new IIKConstraint[0]);
        }
    }
}
