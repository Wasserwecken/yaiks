using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YAIKS
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class IKTarget : MonoBehaviour
    {
        public Transform RootJoint;
        public Transform TipJoint;
        public int IterationsPerFrame = 3;
        public int OrientationMatchingJoints = 3;
        public float ToleranceDistance = 0.001f;
        public IKJoint[] Joints;

        public bool ExecuteInEditor { get; set; }
        public bool IsValid { get; private set; }
        public IKSolverResult SolverResult { get; private set; }


        private void Awake()
        {
            OnValidate();
            ResetPose();
        }

        private void OnValidate()
        {
            IsValid = RootJoint != null && TipJoint != null && TipJoint.parent != null;
            var joints = new IKJoint[0];

            if (IsValid)
                joints = ProvideIKChain(RootJoint, TipJoint);

            if (IsValid && (Joints == null || Joints.Length != joints.Length))
                Joints = joints;

            if (IsValid)
                for (int i = 0; i < joints.Length; i++)
                    if (Joints[i].Root != joints[i].Root || Joints[i].Tip != joints[i].Tip)
                        Joints[i] = joints[i];

            if (IsValid)
                for (int i = 0; i < joints.Length; i++)
                    Joints[i].PullConstraints();

            if (!IsValid)
            {
                Joints = new IKJoint[0];
                ExecuteInEditor = false;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying && !ExecuteInEditor)
                return;

            if (IsValid)
                SolverResult = CCD();
        }

        public void SavePose()
        {
            for (int i = 0; i < Joints.Length; i++)
                Joints[i].SavePose();
        }

        public void ResetPose()
        {
            for (int i = 0; i < Joints.Length; i++)
                Joints[i].ResetPose();
        }

        private static IKJoint[] ProvideIKChain(Transform rootJoint, Transform tipJoint)
        {
            var candidates = new List<IKJoint> { new IKJoint(tipJoint.parent, tipJoint) };

            if (tipJoint.parent != null)
                while (candidates.First().Root != rootJoint && candidates.First().Root.parent != null)
                    candidates.Insert(0, new IKJoint(candidates.First().Root.parent, candidates.First().Root));

            return candidates.ToArray();
        }

        private IKSolverResult CCD()
        {
            var tip = Joints.Last().Tip;
            var result = new IKSolverResult();

            for (int i = 0; i < IterationsPerFrame; i++)
            {
                for (int j = Joints.Length - 1, count = 0; j >= 0; j--, count++)
                {
                    // update solver info
                    result.DistanceToTarget = Vector3.Distance(tip.position, transform.position);
                    result.RotationToTarget = Quaternion.Angle(tip.rotation, transform.rotation);

                    // validate if the solver already matched the target
                    if (result.DistanceToTarget < ToleranceDistance)
                        return result;

                    // align the bone
                    if (count < OrientationMatchingJoints)
                    {
                        Joints[j].Root.rotation = Quaternion.FromToRotation(tip.right, transform.right) * Joints[j].Root.rotation;
                        Joints[j].Root.rotation = Quaternion.FromToRotation(tip.up, transform.up) * Joints[j].Root.rotation;
                    }
                    else
                    {
                        var toTip = tip.position - Joints[j].Root.position;
                        var toTarget = transform.position - Joints[j].Root.position;
                        Joints[j].Root.rotation = Quaternion.FromToRotation(toTip, toTarget) * Joints[j].Root.rotation;
                    }

                    // apply constaints to the rotation
                    for (int c = 0; c < Joints[j].Constraints.Length; c++)
                        Joints[j].Constraints[c].Apply();

                    // update solver info
                    result.UsedIterations = i + ((count + 1f) / Joints.Length);
                }
            }

            return result;
        }

        #region GIZMOS
#if UNITY_EDITOR
        [Serializable]
        private class GizmoSettings
        {
            [Header("Joints")]
            public bool DrawJointsIfSelected = true;
            public bool DrawJointLocalSpace = true;
            public bool DrawJointConnection = true;
            public bool DrawJointRootTip = true;
            public float JointAxisLength = 0.05f;
            public float JointSize = 0.01f;
            public Color JointColor = Color.yellow;
            public Color JointConnectionColor = Color.yellow;

            [Header("Constraints")]
            public bool DrawJointConstraints = true;

            [Header("Target")]
            public bool DrawTargetIfSelected = true;
            public bool DrawTargetPoint = true;
            public bool DrawTargetTipDifference = true;
            public bool DrawTargetAxes = true;
            public float TargetSize = 0.01f;
            public float TargetAxesLength = 0.1f;
            public Color TargetColor = Color.magenta;
            public Color TargetDifferenceColor = Color.magenta;
        }
        [SerializeField] private GizmoSettings _gizmoSettings;

        private void OnDrawGizmos()
        {
            if (!IsValid) return;

            if (!_gizmoSettings.DrawTargetIfSelected)
                DrawTargetGizmo(transform, Joints.Last().Tip, ToleranceDistance, _gizmoSettings);

            if (!_gizmoSettings.DrawJointsIfSelected)
                foreach (var joint in Joints)
                    DrawJointGizmo(joint, _gizmoSettings);
        }

        private void OnDrawGizmosSelected()
        {
            if (!IsValid) return;

            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                if (_gizmoSettings.DrawTargetIfSelected)
                    DrawTargetGizmo(transform, Joints.Last().Tip, ToleranceDistance, _gizmoSettings);


                if (_gizmoSettings.DrawJointsIfSelected)
                    foreach (var joint in Joints)
                        DrawJointGizmo(joint, _gizmoSettings);

                if (_gizmoSettings.DrawJointConstraints)
                    foreach (var joint in Joints)
                        foreach (var constraint in joint.Constraints)
                            constraint.DrawGizmos();
            }
        }

        private static void DrawTargetGizmo(Transform target, Transform tip, float tolerance, GizmoSettings settings)
        {
            if (settings.DrawTargetPoint)
            {
                Gizmos.color = settings.TargetColor;
                Gizmos.DrawWireSphere(target.position, settings.TargetSize + tolerance);
            }

            if (settings.DrawTargetAxes)
            {
                var axesOrigin = target.position;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * target.forward);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * target.right);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * target.up);
            }

            if (settings.DrawTargetTipDifference)
            {
                Gizmos.color = settings.TargetDifferenceColor;
                Gizmos.DrawLine(target.position, tip.position);
            }
        }

        private static void DrawJointGizmo(IKJoint joint, GizmoSettings settings)
        {
            if (settings.DrawJointRootTip)
            {
                Gizmos.color = settings.JointColor;
                Gizmos.DrawSphere(joint.Root.position, settings.JointSize);
                Gizmos.DrawWireSphere(joint.Tip.position, settings.JointSize * 1.1f);
            }

            if (settings.DrawJointConnection)
            {
                Gizmos.color = settings.JointConnectionColor;
                Gizmos.DrawLine(joint.Root.position, joint.Tip.position);
            }

            if (settings.DrawJointLocalSpace)
            {
                var axesOrigin = joint.Root.position;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * joint.Root.forward);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * joint.Root.right);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(axesOrigin, axesOrigin + settings.JointAxisLength * joint.Root.up);
            }
        }
#endif
        #endregion
    }
}
