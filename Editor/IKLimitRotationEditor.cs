#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace YAIKS
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(IKLimitRotation))]
    public class IKLimitRotationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            IKLimitRotation ik = (IKLimitRotation)target;


            GUILayout.BeginHorizontal();

            GUILayout.Label("Set axis by local: ");
            if (GUILayout.Button("X"))
            {
                ik.RotationAxis = Vector3.right;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (GUILayout.Button("Y"))
            {
                ik.RotationAxis = Vector3.up;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (GUILayout.Button("Z"))
            {
                ik.RotationAxis = Vector3.forward;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            GUILayout.Label("Set axis by global: ");
            if (GUILayout.Button("X"))
            {
                ik.RotationAxis = ik.transform.right;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (GUILayout.Button("Y"))
            {
                ik.RotationAxis = ik.transform.up;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (GUILayout.Button("Z"))
            {
                ik.RotationAxis = ik.transform.forward;
                ik.OnValidate();
                EditorApplication.QueuePlayerLoopUpdate();
            }

            GUILayout.EndHorizontal();


            DrawDefaultInspector();
        }
    }
}

#endif