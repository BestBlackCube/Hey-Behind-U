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

    // EditorField 타입 GUI

    // 1. typeof_Name.TargetScript_variableName = EditorGUILayout.IntField(typeof_Name.TargetScript_variableName);

    // LabelField("Text")         : Inspector GUI에 Text타입의 문자를 추가한다
    // Int, float, Vector3Field() : Inspector GUI에 수정이 가능한 public 타입의 변수를 추가한다 

    // SerializedObject 타입 GUI

    // 1. SerializedObject Property_Name;
    // 2. OnEnabled 함수 내부에 Property_Name = serializedObject.FindProperty("variableName"); 
    // 3. OnInspectorGUI 함수 내부에 EditorGUILayout.PropertyField("PropertyName");

    // SerializedObject 타입 변수를 만듦
    // 만드는 이유 : 간단한 int, float, 단일 Vector3, string 등등 typeof으로 불러올수 있지만
    //               그외 배열, 컴포넌트, Transform 등등 복잡한 변수들은 해당하는 Field로는 불러오기 힘들어
    //               프로퍼티를 만듦



    // EditorGUILayout.Space() : 숫자만큼 공백칸 생성 (GUILayout.Space()도 존재함)
    // BeginHorizontal         : Inspector GUI에 가로 배열의 시작이 되었음을 알리고, 가로 1줄로 추가한다 (줄바꿈 없음)
    // EndHorizontal           : Inspector GUI에 가로 배열이 끝났다고 알린다
    // GUILayout.Width()       : 해당 Field의 길이를 수정한다 (왼쪽 정렬)

    // EditorGUILayout.Foldout() : Inspector GUI에서 펼치고, 접는 블럭을 만든다
    //                             Foldout(booltype, "StringName", true) true가 없다면 이름을 눌러서
    //                             펼쳐지는게 아닌 이름 옆 화살표만 눌러야지 펼치거나 접을수 있다
}
