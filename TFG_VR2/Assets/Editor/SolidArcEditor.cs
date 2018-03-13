// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityJoint)), CanEditMultipleObjects]
public class SolidArcEditor : Editor
{
    void OnSceneGUI()
    {
        UnityJoint joint = target as UnityJoint;

        if (joint.transform.childCount == 0)
            return;
        Transform child = joint.transform.GetChild(0);
        Vector3 forward = (child.position - joint.transform.position).normalized;
        float radius = Vector3.Distance(child.position, joint.transform.position);
        Quaternion rot = Quaternion.Euler(0, -Mathf.Abs(joint.MaxAngleCCW), 0);

        SolidArc t = new SolidArc();
        t.from = rot * forward;
        t.angle = joint.MaxAngleCW + joint.MaxAngleCCW;

        if (joint.YAxisRotation == UnityJoint.AxisRotState_Enum.Constrained)
        {
            Handles.color = new Color(0, 1, 0, 0.2f);
            Vector3 from = Quaternion.Euler(0, -joint.YMinAngle, 0) * forward;
            float angle = joint.YMinAngle + joint.YMaxAngle;
            Handles.DrawSolidArc(joint.transform.position,
                Vector3.up, from, angle, radius);
        }

        if (joint.XAxisRotation == UnityJoint.AxisRotState_Enum.Constrained)
        {
            Handles.color = new Color(1, 0, 0, 0.2f);
            Vector3 from = Quaternion.Euler(-joint.XMinAngle, 0, 0) * Vector3.up;
            float angle = joint.XMinAngle + joint.XMaxAngle;
            Handles.DrawSolidArc(joint.transform.position,
                Vector3.right, from, angle, radius);
        }

        if (joint.ZAxisRotation == UnityJoint.AxisRotState_Enum.Constrained)
        { 
            Handles.color = new Color(0, 0, 1, 0.2f);
            Vector3 from = Quaternion.Euler(0, 0, -joint.ZMinAngle) * forward;
            float angle = joint.ZMinAngle + joint.ZMaxAngle;
            Handles.DrawSolidArc(joint.transform.position,
                Vector3.forward, from, angle, radius);
        }

        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(5, new Vector3[] { joint.transform.position, child.position });

        Handles.color = Color.white;
    }

    public SerializedProperty
        XAxisRotation_Prop,
        YAxisRotation_Prop,
        ZAxisRotation_Prop,
        XMinAngle_Prop,
        XMaxAngle_Prop,
        YMinAngle_Prop,
        YMaxAngle_Prop,
        ZMinAngle_Prop,
        ZMaxAngle_Prop;

    void OnEnable()
    {
        XAxisRotation_Prop = serializedObject.FindProperty("XAxisRotation");
        YAxisRotation_Prop = serializedObject.FindProperty("YAxisRotation");
        ZAxisRotation_Prop = serializedObject.FindProperty("ZAxisRotation");
        XMinAngle_Prop = serializedObject.FindProperty("XMinAngle");
        XMaxAngle_Prop = serializedObject.FindProperty("XMaxAngle");
        YMinAngle_Prop = serializedObject.FindProperty("YMinAngle");
        YMaxAngle_Prop = serializedObject.FindProperty("YMaxAngle");
        ZMinAngle_Prop = serializedObject.FindProperty("ZMinAngle");
        ZMaxAngle_Prop = serializedObject.FindProperty("ZMaxAngle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(XAxisRotation_Prop);
        if(XAxisRotation_Prop.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(XMinAngle_Prop, new GUIContent("XMinAngle"));
            EditorGUILayout.PropertyField(XMaxAngle_Prop, new GUIContent("XMaxAngle"));
        }

        EditorGUILayout.PropertyField(YAxisRotation_Prop);
        if (YAxisRotation_Prop.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(YMinAngle_Prop, new GUIContent("YMinAngle"));
            EditorGUILayout.PropertyField(YMaxAngle_Prop, new GUIContent("YMaxAngle"));
        }

        EditorGUILayout.PropertyField(ZAxisRotation_Prop);
        if (ZAxisRotation_Prop.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(ZMinAngle_Prop, new GUIContent("ZMinAngle"));
            EditorGUILayout.PropertyField(ZMaxAngle_Prop, new GUIContent("ZMaxAngle"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
