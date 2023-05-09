#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace YAIKS
{
    [CustomEditor(typeof(IKTarget))]
    public class IKTargetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            IKTarget ik = (IKTarget)target;

            if (ik.RootJoint == null)
                EditorGUILayout.HelpBox("Root joint is missing", MessageType.Error);

            if (ik.TipJoint == null)
                EditorGUILayout.HelpBox("Tip joint is missing", MessageType.Error);

            else if (ik.TipJoint.parent == null)
                EditorGUILayout.HelpBox("Tip joint has no parent transform, therefore it cannot be a tip", MessageType.Error);

            if (ik.IsValid && ik.RootJoint == ik.TipJoint)
                EditorGUILayout.HelpBox("Root and tip joint are equal!", MessageType.Warning);

            if (ik.IsValid)
                foreach (var joint in ik.Joints)
                    if (joint.Root == ik.transform || joint.Tip == ik.transform)
                        EditorGUILayout.HelpBox("The IK Target is part of the IK chain!", MessageType.Error);

            if (ik.ToleranceDistance == 0)
                EditorGUILayout.HelpBox("Tolerance is set to 0! Every iteration is executed and every bone will be updated every frame!", MessageType.Warning);

            ik.ExecuteInEditor = GUILayout.Toggle(ik.ExecuteInEditor, "Pose in Editor", new GUIStyle(GUI.skin.button));

            GUILayout.BeginHorizontal();
            GUI.enabled = !ik.ExecuteInEditor;

            if (GUILayout.Button("Reset pose"))
            {
                ik.ResetPose();
                ik.ExecuteInEditor = false;
            }

            if (GUILayout.Button("Save pose"))
                ik.SavePose();

            if (GUILayout.Button("To Tip"))
            {
                ik.transform.position = ik.TipJoint.transform.position;
                ik.transform.rotation = ik.TipJoint.transform.rotation;
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;


            if (ik.IsValid)
            {
                var infoString = $"Used iterations:\t\t\t {ik.SolverResult.UsedIterations}\n";
                infoString += $"Distance difference:\t\t {ik.SolverResult.DistanceToTarget}\n";
                infoString += $"Rotation difference:\t\t {ik.SolverResult.RotationToTarget}";
                EditorGUILayout.HelpBox(infoString, MessageType.None);
            }


            DrawDefaultInspector();
        }
    }
}

#endif