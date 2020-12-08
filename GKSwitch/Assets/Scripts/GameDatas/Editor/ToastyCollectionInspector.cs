using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ToastyCollection))]
public class ToastyCollectionInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ToastyCollection myScript = (ToastyCollection)target;
        SerializedProperty datasList = serializedObject.FindProperty("toastyDatas");
        EditorList.Show(datasList, EditorListOption.All);
        serializedObject.ApplyModifiedProperties();
    }
}