using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Rigidbody))]
public class linearVelocityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Rigidbody linear_Velocity = (Rigidbody)target;
        if (linear_Velocity != null) EditorGUILayout.Vector3Field("LinearVelocity", linear_Velocity.linearVelocity);

        Rigidbody Freeze_rotation = (Rigidbody)target;

        EditorGUILayout.LabelField("Freeze Rotation", EditorStyles.boldLabel);
        bool freezeX = (Freeze_rotation.constraints & RigidbodyConstraints.FreezeRotationX) != 0;
        bool freezeY = (Freeze_rotation.constraints & RigidbodyConstraints.FreezeRotationY) != 0;
        bool freezeZ = (Freeze_rotation.constraints & RigidbodyConstraints.FreezeRotationZ) != 0;

        freezeX = EditorGUILayout.Toggle("Freeze X", freezeX);
        freezeY = EditorGUILayout.Toggle("Freeze Y", freezeY);
        freezeZ = EditorGUILayout.Toggle("Freeze Z", freezeZ);

        // 변경 사항이 있으면 적용
        if (GUI.changed)
        {
            Freeze_rotation.constraints = RigidbodyConstraints.None;
            if (freezeX) Freeze_rotation.constraints |= RigidbodyConstraints.FreezeRotationX;
            if (freezeY) Freeze_rotation.constraints |= RigidbodyConstraints.FreezeRotationY;
            if (freezeZ) Freeze_rotation.constraints |= RigidbodyConstraints.FreezeRotationZ;

            EditorUtility.SetDirty(Freeze_rotation);
        }
    }
}
