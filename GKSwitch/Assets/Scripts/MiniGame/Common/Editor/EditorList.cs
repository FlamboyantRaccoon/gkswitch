using UnityEditor;
using UnityEngine;
using System;

[Flags]
public enum EditorListOption
{
    None = 0,
    ListSize = 1,
    ListLabel = 2,
    ElementLabels = 4,
    Buttons = 8,
    Default = ListSize | ListLabel | ElementLabels,
    NoElementLabels = ListSize | ListLabel,
    All = Default | Buttons
}

public static class EditorList
{
    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);
    private static GUIContent
        moveButtonContent = new GUIContent("\u21b4", "move down"),
        duplicateButtonContent = new GUIContent("+", "duplicate"),
        deleteButtonContent = new GUIContent("-", "delete"),
        addButtonContent = new GUIContent("+", "add element");

    public static void Show(SerializedProperty list, EditorListOption options = EditorListOption.Default)
    {
        bool showListLabel = (options & EditorListOption.ListLabel) != 0,
             showListSize = (options & EditorListOption.ListSize) != 0;
        if (showListLabel)
        {
            //EditorGUILayout.PropertyField(list);
            EditorGUILayout.LabelField(list.name);
            EditorGUI.indentLevel += 1;
        }
        if (!showListLabel || list.isExpanded)
        {
            if (showListSize)
            {
                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
            }
            ShowElements(list, options);
        }
        if (showListLabel)
        {
            EditorGUI.indentLevel -= 1;
        }
    }

    private static void ShowElements(SerializedProperty list, EditorListOption options)
    {

        bool showElementLabels = (options & EditorListOption.ElementLabels) != 0,
             showButtons = (options & EditorListOption.Buttons) != 0;

        for (int i = 0; i < list.arraySize; i++)
        {
            BattleContext.MiniGameDifficulty difficulty = GetDifficulty(list.GetArrayElementAtIndex(i));
            Color backGroundColor = Color.white;
            switch (difficulty)
            {
                case BattleContext.MiniGameDifficulty.easy:
                    backGroundColor = new Color( 155f/255f, 253f/255f, 108f/255f );
                    break;
                case BattleContext.MiniGameDifficulty.medium:
                    backGroundColor = new Color(243 / 255f, 239f / 255f, 84f / 255f);
                    break;
                case BattleContext.MiniGameDifficulty.hard:
                    backGroundColor = new Color(249f / 255f, 167f / 255f, 113f / 255f);
                    break;
                case BattleContext.MiniGameDifficulty.champion:
                    backGroundColor = new Color(208f / 255f, 205f / 255f, 205f / 255f);
                    break;
                case BattleContext.MiniGameDifficulty.fiesta:
                    backGroundColor = new Color(143f / 255f, 201f / 255f, 248f / 255f);
                    break;
            }



            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = backGroundColor;

            if (showButtons)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
            }


            if (showElementLabels)
            {
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), true);
            }
            else
            {
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none, true);
            }
            if (showButtons)
            {
                ShowButtons(list, i);
                EditorGUILayout.EndHorizontal();
            }

            GUI.backgroundColor = oldColor;

        }
        if (showButtons && list.arraySize == 0 && GUILayout.Button(addButtonContent, EditorStyles.miniButton))
        {
            list.arraySize += 1;
        }


    }

    private static void ShowButtons(SerializedProperty list, int index)
    {
        if (GUILayout.Button(moveButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth))
        {
            list.MoveArrayElement(index, index + 1);
        }
        if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth))
        {
            list.InsertArrayElementAtIndex(index);
        }
        if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
        {
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex(index);
            if (list.arraySize == oldSize)
            {
                list.DeleteArrayElementAtIndex(index);
            }
        }
    }

    private static BattleContext.MiniGameDifficulty GetDifficulty(SerializedProperty element)
    {
        int nEnumSize = Enum.GetNames(typeof(BattleContext.MiniGameDifficulty)).Length;
        if ( element.hasChildren)
        {
            while( element.Next(true) )
            {
                if( element.propertyType != SerializedPropertyType.Enum )
                {
                    continue;
                }
                int elementEnumSize = element.enumNames.Length;
                if(elementEnumSize == nEnumSize)
                {
                    bool bSame = true;
                    for( int i=0; i<element.enumNames.Length; i++ )
                    {
                        if( element.enumNames[i] != ((BattleContext.MiniGameDifficulty)i).ToString() )
                        {
                            bSame = false;
                        }
                    }
                    if ( bSame )
                    {
                        return (BattleContext.MiniGameDifficulty)element.enumValueIndex;
                    }
                }
                
            }
        }
        
        return BattleContext.MiniGameDifficulty.easy;
    }
}