using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MG_Balancing<,>))]
public class MG_BalancingInspector<TBot, TData> : Editor
    where TBot : MiniGameBotData
    where TData : MiniGameBalancingData

{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        MG_Balancing<TBot, TData> myScript = (MG_Balancing <TBot, TData>)target;
        SerializedProperty datasList = serializedObject.FindProperty("m_datas");

        EditorList.Show(datasList, EditorListOption.All);
        EditorList.Show(serializedObject.FindProperty("m_botDatas"), EditorListOption.All);
        serializedObject.ApplyModifiedProperties();
    }
}