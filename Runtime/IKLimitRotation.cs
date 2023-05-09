using System;
using UnityEditor;
using UnityEngine;

namespace YAIKS
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class IKLimitRotation : MonoBehaviour, IIKConstraint
    {
        public Vector3 RotationAxis = Vector3.up;
        [Range(0, 180)] public float RotationRange = 155f;
        [Range(0, 360)] public float RotationRangeOffset = 0f;

        private IKJoint Joint;
        private Vector3 RotationRangeCenter;
        private Vector3 RotationRangeStart;
        private Vector3 RotationRangeEnd;


        private void Awake()
        {
            if (Joint.Root == null)
                Joint = new IKJoint(transform, null);

            RotationRangeCenter = Vector3.Dot(RotationAxis, Vector3.right) > 0.5f ? Vector3.forward : Vector3.right;
            RotationRangeCenter = Vector3.Cross(RotationRangeCenter, RotationAxis);
            RotationRangeCenter = Quaternion.AngleAxis(RotationRangeOffset, RotationAxis) * RotationRangeCenter;

            RotationRangeStart = Quaternion.AngleAxis(-RotationRange, RotationAxis) * RotationRangeCenter;
            RotationRangeEnd = Quaternion.AngleAxis(RotationRange, RotationAxis) * RotationRangeCenter;
        }

        public void OnValidate()
        {
            Awake();
        }

        public void SetJoint(IKJoint joint)
        {
            Joint = joint;
        }

        public void Apply()
        {
            var currentDirection = transform.localRotation * RotationAxis;
            var originalDirection = Joint.InitialLocalRotation * RotationAxis;
            var rotationCorrection = Quaternion.FromToRotation(currentDirection, originalDirection);
            transform.localRotation = rotationCorrection * transform.localRotation;

            var currentCenter = transform.localRotation * RotationRangeCenter;
            var originalCenter = Joint.InitialLocalRotation * RotationRangeCenter;
            var rotation = Vector3.SignedAngle(originalCenter, currentCenter, RotationAxis);
            if (Mathf.Abs(rotation) > RotationRange)
                transform.localRotation = Quaternion.AngleAxis(Mathf.Sign(rotation) * RotationRange - rotation, RotationAxis) * transform.localRotation;
        }

        #region GIZMOS
#if UNITY_EDITOR
        [Serializable]
        private class GizmoSettings
        {
            public bool DrawIfSelected = true;
            public bool ShowRange = true;
            public bool ShowRangeInidactors = true;
            public float ArcSize = 0.07f;
            public float ArcTransparency = 0.05f;
            public float LineLength = 0.08f;
            public Color LineColor = Color.cyan * 5f;
        }
        [SerializeField] private GizmoSettings _gizmoSettings;

        private void OnDrawGizmos()
        {
            if (!_gizmoSettings.DrawIfSelected)
                DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (_gizmoSettings.DrawIfSelected && UnityEditor.Selection.activeGameObject == gameObject)
                DrawGizmos();
        }

        public void DrawGizmos()
        {
            DrawGizmos(Joint, RotationRange, RotationAxis, RotationRangeCenter, RotationRangeStart, RotationRangeEnd, _gizmoSettings);
        }

        private static void DrawGizmos(IKJoint joint, float rotationRange, Vector3 rotationAxis, Vector3 rangeCenter, Vector3 rangeStart, Vector3 rangeEnd, GizmoSettings settings)
        {
            var axisColor = new Color(Mathf.Abs(rotationAxis.x), Mathf.Abs(rotationAxis.y), Mathf.Abs(rotationAxis.z));

            rotationAxis = joint.Root.parent.TransformDirection(joint.InitialLocalRotation * rotationAxis);
            rangeStart = joint.Root.parent.TransformDirection(joint.InitialLocalRotation * rangeStart);
            rangeCenter = joint.Root.TransformDirection(rangeCenter);
            rangeEnd = joint.Root.parent.TransformDirection(joint.InitialLocalRotation * rangeEnd);

            if (settings.ShowRange)
            {
                UnityEditor.Handles.color = new Color(axisColor.r, axisColor.g, axisColor.b, settings.ArcTransparency);
                UnityEditor.Handles.DrawSolidArc(joint.Root.position, rotationAxis, rangeStart, rotationRange * 2, settings.ArcSize);
            }

            if (settings.ShowRangeInidactors)
            {
                UnityEditor.Handles.color = axisColor;
                UnityEditor.Handles.DrawLine(joint.Root.position, joint.Root.position + rotationAxis * settings.LineLength);
                UnityEditor.Handles.color = settings.LineColor;
                UnityEditor.Handles.DrawLine(joint.Root.position, joint.Root.position + rangeCenter * settings.LineLength);
                
                if (rotationRange < 180f)
                {
                    UnityEditor.Handles.color = settings.LineColor;
                    UnityEditor.Handles.DrawDottedLine(joint.Root.position, joint.Root.position + rangeStart * settings.LineLength, 2f);
                    UnityEditor.Handles.DrawDottedLine(joint.Root.position, joint.Root.position + rangeEnd * settings.LineLength, 2f);
                }
            }
        }
#endif
    #endregion
    }
}