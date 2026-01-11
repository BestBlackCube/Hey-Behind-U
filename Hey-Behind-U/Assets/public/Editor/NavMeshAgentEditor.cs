using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(NavMeshAgent))]
public class NavMeshAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NavMeshAgent navAgnet = (NavMeshAgent)target;
        if(navAgnet != null) EditorGUILayout.Vector3Field("NavVelocity", navAgnet.velocity);
    }
}
