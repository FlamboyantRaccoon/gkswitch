using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(SA_Grid))]
public class CustomSA_Grid : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);

        Rect newPosition = position;
        newPosition.y += 18f;
        newPosition.height = 20;
        newPosition.width = 250;

        SerializedProperty nRow = property.FindPropertyRelative("nRows");
        SerializedProperty nCols = property.FindPropertyRelative("nCols");

        EditorGUI.LabelField(newPosition, "rows");
        newPosition.x += 70;
        EditorGUI.PropertyField(newPosition, nRow, GUIContent.none );
        newPosition.y += 20;
        newPosition.x = position.x;
        EditorGUI.LabelField(newPosition, "cols");
        newPosition.x += 70;
        EditorGUI.PropertyField(newPosition, nCols, GUIContent.none);
        newPosition.x = position.x;
        newPosition.y += 20;
        newPosition.width = 750;
        EditorGUI.LabelField(newPosition, "0 for empty cells, -1 for dead cells, 1 for not moving obstacle, each number after is a path for a moving obstacle");
        newPosition.y += 20;

        int nRowCount = nRow.intValue;
        int nColCount = nCols.intValue;

        SerializedProperty rows = property.FindPropertyRelative("rows");

        if( rows.arraySize != nRowCount )
        {
            rows.arraySize = nRowCount;
        }

        newPosition.width = 50;
        for ( int i=0; i<rows.arraySize; i++ )
        {
            SerializedProperty row = rows.GetArrayElementAtIndex(i).FindPropertyRelative("row");
            

            if( row.arraySize != nColCount)
            {
                row.arraySize = nColCount;
            }

            for( int j=0; j<row.arraySize; j++ )
            {
                EditorGUI.PropertyField(newPosition, row.GetArrayElementAtIndex(j), GUIContent.none); //, GUIContent.none);
                newPosition.x += newPosition.width / 2f;
            }
            newPosition.x = position.x;
            newPosition.y += 20;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty rows = property.FindPropertyRelative("rows");
        return (rows.arraySize + 5f) * 20f;
    }
}