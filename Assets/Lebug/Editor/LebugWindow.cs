using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LebugWindow : EditorWindow
{
    // Styles
    GUIStyle windowStyle;
    GUIStyle categoryStyle;
    GUIStyle keyStyle;
    GUIStyle valueStyle;

    // Colors
    Color defaultColor = Color.black;
    Color categoryColor = new Color(0.2f, 0.8f, 0.4f, 1); // Greenish
    //Color categoryColor = new Color(0.0f, 0.2f, 0.8f, 1); // Blueish
    //Color categoryColor = new Color(0.6f, 0.4f, 0.0f, 1); // Brownish :)
    Color[] contentColor;

    // Scroll positions
    Vector2 scrollViewPosition = Vector2.zero;


    private void OnEnable()
    {
        contentColor = new Color[] { Color.black, new Color(0.2f, 0.2f, 0.2f, 1) };

        // Min window size
        minSize = new Vector2(200, 100);

        windowStyle = new GUIStyle();
        windowStyle.normal.background = Texture2D.whiteTexture;

        categoryStyle = new GUIStyle();
        categoryStyle.alignment = TextAnchor.MiddleLeft;
        categoryStyle.normal.background = Texture2D.whiteTexture;
        categoryStyle.normal.textColor = Color.white;
        categoryStyle.padding = new RectOffset(1, 1, 1, 1);
        categoryStyle.fontStyle = FontStyle.Bold;

        keyStyle = new GUIStyle();
        keyStyle.wordWrap = true;
        keyStyle.normal.background = Texture2D.whiteTexture;
        keyStyle.normal.textColor = Color.white;
        keyStyle.fontStyle = FontStyle.Bold;

        valueStyle = new GUIStyle();
        valueStyle.wordWrap = true;
        valueStyle.normal.background = Texture2D.whiteTexture;
        valueStyle.normal.textColor = Color.white;
        valueStyle.fontStyle = FontStyle.Bold;
        valueStyle.alignment = TextAnchor.MiddleRight;
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }


    void OnGUI()
    {
        DrawCategories();
    }

    void DrawCategories()
    {
        GUI.backgroundColor = defaultColor;
        scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition, EditorStyles.textField);

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandHeight(true));

        foreach (KeyValuePair<string, Dictionary<string, object>> categoryDict in Lebug.lebugDict)
        {
            GUI.backgroundColor = categoryColor;
            if (GUILayout.Button(
                (Lebug.categoriesExpanded[categoryDict.Key] ? ((char)8212).ToString() : "+") + " " + categoryDict.Key, categoryStyle))
            {
                Lebug.categoriesExpanded[categoryDict.Key] = !Lebug.categoriesExpanded[categoryDict.Key];
            }

            if (!Lebug.categoriesExpanded[categoryDict.Key])
            {
                // This category is collapsed. Don't show the contents...
                continue;
            }

            DrawCategoryContents(categoryDict.Value);
        }


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void DrawCategoryContents(Dictionary<string, object> categoryContents)
    {
        EditorGUILayout.BeginVertical();
        int i = 0;
        foreach (KeyValuePair<string, object> row in categoryContents)
        {
            // Write content line
            GUI.backgroundColor = contentColor[i++ % 2];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label(row.Key, keyStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Label(row.Value.ToString(), valueStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.Space(8);
        EditorGUILayout.EndVertical();
    }

    [MenuItem("Window/Lebug Window")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(LebugWindow));
    }
}