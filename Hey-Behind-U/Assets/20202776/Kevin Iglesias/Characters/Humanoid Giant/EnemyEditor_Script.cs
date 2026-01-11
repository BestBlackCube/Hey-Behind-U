using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Enemy_Script))]
public class EnemyEditor_Script : Editor
{
    SerializedProperty PlayerGameObjectProp;
    SerializedProperty SelectLoopProp;
    private void OnEnable()
    {
        PlayerGameObjectProp = serializedObject.FindProperty("PlayerChar");
        SelectLoopProp = serializedObject.FindProperty("SelectLoop");
    }
    bool EnemyTypeSet = false;
    bool Enemy01_10 = false;
    bool Enemy15_16 = false;
    bool Enemy20 = false;
    bool Enemy30 = false;
    bool Enemy50 = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Enemy_Script enemyEdit = (Enemy_Script)target;

        EditorGUILayout.LabelField("EnemyCheckLength");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DirectRay", GUILayout.Width(60));
        enemyEdit.DirectPlayerRay = EditorGUILayout.FloatField(enemyEdit.DirectPlayerRay, GUILayout.Width(60));
        GUILayout.Space(60);
        EditorGUILayout.LabelField("Angle", GUILayout.Width(40));
        enemyEdit.viewAngle = EditorGUILayout.FloatField(enemyEdit.viewAngle, GUILayout.Width(40));
        EditorGUILayout.LabelField("Distance", GUILayout.Width(60));
        enemyEdit.viewDistance = EditorGUILayout.FloatField(enemyEdit.viewDistance);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("PlayerAttack", GUILayout.Width(80));
        enemyEdit.PlayerAttack = EditorGUILayout.Toggle(enemyEdit.PlayerAttack);
        EditorGUILayout.LabelField("Attack Length 2.5f");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Field Select name & Type");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("EnemyType", GUILayout.Width(70));
        enemyEdit.walkType = EditorGUILayout.IntField(enemyEdit.walkType, GUILayout.Width(40));
        GUILayout.Space(20);
        EditorGUILayout.LabelField("walkType", GUILayout.Width(60));
        enemyEdit.walkLoop = EditorGUILayout.IntField(enemyEdit.walkLoop, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(PlayerGameObjectProp);
        EditorGUILayout.Space(10);

        EnemyTypeSet = EditorGUILayout.Foldout(EnemyTypeSet, "EnemyTypeSetting", true);
        if (EnemyTypeSet)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MoveChange");
            enemyEdit.SetmoveChange = EditorGUILayout.Toggle(enemyEdit.SetmoveChange);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(SelectLoopProp);
            EditorGUILayout.Space(5);
            Enemy01_10 = EditorGUILayout.Foldout(Enemy01_10, "EnemyType 01~10", true);
            if (Enemy01_10)
            {
                EditorGUILayout.Space(5);
                enemyEdit.LoopName = EditorGUILayout.TextField("Enemy LoopName", enemyEdit.LoopName);
            }
            EditorGUILayout.Space(5);
            Enemy15_16 = EditorGUILayout.Foldout(Enemy15_16, "EnemyType 15-16", true);
            if (Enemy15_16)
            {
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.Space(5);
            Enemy20 = EditorGUILayout.Foldout(Enemy20, "EnemyType 20", true);
            if (Enemy20)
            {
                EditorGUILayout.Space(5);
                enemyEdit.SoundCount = EditorGUILayout.IntField("SoundCount", enemyEdit.SoundCount);
            }
            EditorGUILayout.Space(5);
            Enemy30 = EditorGUILayout.Foldout(Enemy30, "EnemyType 30", true);
            if (Enemy30)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("RushRay", GUILayout.Width(50));
                enemyEdit.RushRayLength = EditorGUILayout.FloatField(enemyEdit.RushRayLength, GUILayout.Width(40));
                GUILayout.Space(20);
                EditorGUILayout.LabelField("StopMaxCount", GUILayout.Width(90));
                EditorGUILayout.IntField(enemyEdit.RayMaxCount, GUILayout.Width(40));
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Move", GUILayout.Width(40));
                enemyEdit.SetmovePoint = EditorGUILayout.Toggle(enemyEdit.SetmovePoint);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
                enemyEdit.lastPoint = EditorGUILayout.Vector3Field("LastPoint", enemyEdit.lastPoint);
            }
            EditorGUILayout.Space(5);
            Enemy50 = EditorGUILayout.Foldout(Enemy50, "EnemyType 50", true);
            if (Enemy50)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("FollowCount", GUILayout.Width(80));
                enemyEdit.FollowCount = EditorGUILayout.IntField(enemyEdit.FollowCount);
                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
