// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JointConstrainer)), CanEditMultipleObjects]
public class JointConstrainerEditor : Editor
{
    public SerializedProperty
        constrainerType_Prop,
        active_Prop,
        drawProjection_Prop,
        minAngle_Prop,
        maxAngle_Prop,
        transform_Prop,
        parent_Prop,
        child_Prop,
        plane_Prop,
        rotAxis_Prop,
        threshold_Prop,
        mag_Prop,
        localForward_Prop;

    void OnEnable()
    {
        // Setup the SerializedProperties
        constrainerType_Prop = serializedObject.FindProperty("constrainerType");
        active_Prop = serializedObject.FindProperty("active");
        drawProjection_Prop = serializedObject.FindProperty("drawProjection");
        minAngle_Prop = serializedObject.FindProperty("minAngle");
        maxAngle_Prop = serializedObject.FindProperty("maxAngle");
        transform_Prop = serializedObject.FindProperty("transform");
        transform_Prop = serializedObject.FindProperty("transform");
        parent_Prop = serializedObject.FindProperty("parent");
        child_Prop = serializedObject.FindProperty("child");
        plane_Prop = serializedObject.FindProperty("plane");
        rotAxis_Prop = serializedObject.FindProperty("rotAxis");
        threshold_Prop = serializedObject.FindProperty("threshold");
        mag_Prop = serializedObject.FindProperty("mag");
        localForward_Prop = serializedObject.FindProperty("localForward");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(constrainerType_Prop);
        EditorGUILayout.PropertyField(active_Prop, new GUIContent("active"));
        EditorGUILayout.ObjectField(transform_Prop, new GUIContent("transform"));

        JointConstrainer.ConstrainerType type = (JointConstrainer.ConstrainerType)constrainerType_Prop.enumValueIndex;

        switch (type)
        {
            case JointConstrainer.ConstrainerType.Constrainer_angle:
                EditorGUILayout.PropertyField(maxAngle_Prop, new GUIContent("maxAngle"));
                EditorGUILayout.ObjectField(parent_Prop, new GUIContent("parent"));
                EditorGUILayout.ObjectField(child_Prop, new GUIContent("child"));
                break;

            case JointConstrainer.ConstrainerType.Constrainer_minmaxangle:
                EditorGUILayout.PropertyField(minAngle_Prop, new GUIContent("minAngle"));
                EditorGUILayout.PropertyField(maxAngle_Prop, new GUIContent("maxAngle"));
                EditorGUILayout.ObjectField(parent_Prop, new GUIContent("parent"));
                EditorGUILayout.ObjectField(child_Prop, new GUIContent("child"));
                break;

            case JointConstrainer.ConstrainerType.Constrainer_minmaxangle_plane:
                EditorGUILayout.PropertyField(drawProjection_Prop, new GUIContent("drawProjection"));
                EditorGUILayout.PropertyField(minAngle_Prop, new GUIContent("minAngle"));
                EditorGUILayout.PropertyField(maxAngle_Prop, new GUIContent("maxAngle"));
                EditorGUILayout.ObjectField(parent_Prop, new GUIContent("parent"));
                EditorGUILayout.ObjectField(child_Prop, new GUIContent("child"));
                EditorGUILayout.ObjectField(plane_Prop, new GUIContent("plane"));
                EditorGUILayout.PropertyField(threshold_Prop, new GUIContent("threshold"));
                EditorGUILayout.PropertyField(mag_Prop, new GUIContent("mag"));
                break;           

            case JointConstrainer.ConstrainerType.Constrainer_plane:
                EditorGUILayout.PropertyField(drawProjection_Prop, new GUIContent("drawProjection"));
                EditorGUILayout.ObjectField(plane_Prop, new GUIContent("plane"));
                EditorGUILayout.ObjectField(parent_Prop, new GUIContent("parent"));
                EditorGUILayout.ObjectField(child_Prop, new GUIContent("child"));
                EditorGUILayout.PropertyField(threshold_Prop, new GUIContent("threshold"));
                EditorGUILayout.PropertyField(mag_Prop, new GUIContent("mag"));
                break;

            case JointConstrainer.ConstrainerType.Constrainer_twist:
                EditorGUILayout.PropertyField(minAngle_Prop, new GUIContent("minAngle"));
                EditorGUILayout.PropertyField(maxAngle_Prop, new GUIContent("maxAngle"));
                EditorGUILayout.PropertyField(localForward_Prop);
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }
}