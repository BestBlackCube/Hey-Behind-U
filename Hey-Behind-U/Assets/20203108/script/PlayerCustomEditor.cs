using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEditor.Rendering;

[CustomEditor(typeof(Player_test))]
public class PlayerCustomEditor : Editor
{
    SerializedProperty rigidProp;
    SerializedProperty animatorProp;
    SerializedProperty playerCameraProp;

    SerializedProperty playerDotProp;

    SerializedProperty TextMeshProProp;
    SerializedProperty LbottomPointProp;
    SerializedProperty LtopPointProp;

    SerializedProperty GroundLayerProp;
    SerializedProperty StairLayerProp;

    SerializedProperty LadderProp;
    SerializedProperty GroundCheckProp;
    SerializedProperty SliderProp;
    SerializedProperty keyCountTextProp;
    SerializedProperty gameClearTMPText;
    private void OnEnable()
    {
        rigidProp = serializedObject.FindProperty("rigid");
        animatorProp = serializedObject.FindProperty("animator");
        playerCameraProp = serializedObject.FindProperty("playerCamera");
        playerDotProp = serializedObject.FindProperty("PlayerDot");
        GroundLayerProp = serializedObject.FindProperty("GroundlayerMask");
        StairLayerProp = serializedObject.FindProperty("StairlayerMask");
        GroundCheckProp = serializedObject.FindProperty("groundcheck");
        SliderProp = serializedObject.FindProperty("staminaSlider");
        keyCountTextProp = serializedObject.FindProperty("KeyCountText");
        gameClearTMPText = serializedObject.FindProperty("gameClearTMPText");
    }
    bool ShowPlayerStatus = false;
    bool ShowComponent_variable = false;
    bool ShowStamina = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Player_test playerEdit = (Player_test)target;

        ShowPlayerStatus = EditorGUILayout.Foldout(ShowPlayerStatus, "player inGame status", true);
        if (ShowPlayerStatus)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("HP", GUILayout.Width(60));
            playerEdit.PlayerHp = EditorGUILayout.IntField(playerEdit.PlayerHp);
            EditorGUILayout.LabelField("runPow", GUILayout.Width(60));
            playerEdit.runPow = EditorGUILayout.IntField(playerEdit.runPow);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        ShowStamina = EditorGUILayout.Foldout(ShowStamina, "Stamina variable", true);
        if (ShowStamina)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stamina", GUILayout.Width(90));
            playerEdit.NowStamina = EditorGUILayout.FloatField(playerEdit.NowStamina);
            EditorGUILayout.LabelField("MaxStamina", GUILayout.Width(90));
            playerEdit.maxStamina = EditorGUILayout.FloatField(playerEdit.maxStamina);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Drain");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("drainStamina", GUILayout.Width(90));
            playerEdit.NowStamina = EditorGUILayout.FloatField(playerEdit.NowStamina);
            EditorGUILayout.LabelField("drainLadder", GUILayout.Width(90));
            playerEdit.maxStamina = EditorGUILayout.FloatField(playerEdit.maxStamina);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Setting");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DelayTimer", GUILayout.Width(90));
            playerEdit.delayStamina = EditorGUILayout.FloatField(playerEdit.delayStamina);
            EditorGUILayout.LabelField("recoveryIndex", GUILayout.Width(90));
            playerEdit.recoveryStamina = EditorGUILayout.FloatField(playerEdit.recoveryStamina);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("MaxTimer : 2f");
            EditorGUILayout.PropertyField(SliderProp);
            EditorGUILayout.Space(10);
        }

        ShowComponent_variable = EditorGUILayout.Foldout(ShowComponent_variable, "player Component & variable", true);
        if (ShowComponent_variable)
        {
            EditorGUILayout.LabelField("Component");
            EditorGUILayout.PropertyField(rigidProp);
            EditorGUILayout.PropertyField(animatorProp);
            EditorGUILayout.PropertyField(playerCameraProp);
            EditorGUILayout.PropertyField(keyCountTextProp);
            EditorGUILayout.PropertyField(gameClearTMPText);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("variable");
            EditorGUILayout.PropertyField(playerDotProp);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DotTimer", GUILayout.Width(60));
            playerEdit.DotTimer = EditorGUILayout.FloatField(playerEdit.DotTimer);
            EditorGUILayout.LabelField("DotCount", GUILayout.Width(60));
            playerEdit.DotCount = EditorGUILayout.IntField(playerEdit.DotCount);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }
        

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.EndHorizontal();

        if (GUI.changed) EditorUtility.SetDirty(playerEdit);

        serializedObject.ApplyModifiedProperties();
    }
    void ComponentField() // X
    {
        if (rigidProp.objectReferenceValue != null)
        {
            Rigidbody rigidRef = (Rigidbody)rigidProp.objectReferenceValue;
            //EditorGUILayout.LabelField("Rigidbody mass : " + rigidRef.mass.ToString("F2"));
        }
        if (animatorProp.objectReferenceValue != null)
        {
            Animator animatorRef = (Animator)animatorProp.objectReferenceValue;
            //EditorGUILayout.LabelField("Animator enabled : " + animatorRef.enabled);
        }
    }

    // manual

    // EditorField Ÿ�� GUI

    // 1. typeof_Name.TargetScript_variableName = EditorGUILayout.IntField(typeof_Name.TargetScript_variableName);

    // LabelField("Text")         : Inspector GUI�� TextŸ���� ���ڸ� �߰��Ѵ�
    // Int, float, Vector3Field() : Inspector GUI�� ������ ������ public Ÿ���� ������ �߰��Ѵ� 

    // SerializedObject Ÿ�� GUI

    // 1. SerializedObject Property_Name;
    // 2. OnEnabled �Լ� ���ο� Property_Name = serializedObject.FindProperty("variableName"); 
    // 3. OnInspectorGUI �Լ� ���ο� EditorGUILayout.PropertyField("PropertyName");

    // SerializedObject Ÿ�� ������ ����
    // ����� ���� : ������ int, float, ���� Vector3, string ��� typeof���� �ҷ��ü� ������
    //               �׿� �迭, ������Ʈ, Transform ��� ������ �������� �ش��ϴ� Field�δ� �ҷ����� �����
    //               ������Ƽ�� ����



    // EditorGUILayout.Space() : ���ڸ�ŭ ����ĭ ���� (GUILayout.Space()�� ������)
    // BeginHorizontal         : Inspector GUI�� ���� �迭�� ������ �Ǿ����� �˸���, ���� 1�ٷ� �߰��Ѵ� (�ٹٲ� ����)
    // EndHorizontal           : Inspector GUI�� ���� �迭�� �����ٰ� �˸���
    // GUILayout.Width()       : �ش� Field�� ���̸� �����Ѵ� (���� ����)

    // EditorGUILayout.Foldout() : Inspector GUI���� ��ġ��, ���� ������ �����
    //                             Foldout(booltype, "StringName", true) true�� ���ٸ� �̸��� ������
    //                             �������°� �ƴ� �̸� �� ȭ��ǥ�� �������� ��ġ�ų� ������ �ִ�
}
