namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class MathGraphWindow : EditorWindow
    {
        private Vector2 mScrollPosition;
        private List<MathExpression> mExpressionList = new List<MathExpression>();

        [MenuItem("AceSea/MathGraph/Open Panel")]
        private static void OpenPanel()
        {
            MathGraphWindow window = (MathGraphWindow)GetWindow(typeof(MathGraphWindow));
            window.minSize = new Vector2(600, 300);
            window.titleContent = new GUIContent("Math Graph");
            window.Show();
        }

        private void OnEnable()
        {
            
        }

        private void OnGUI()
        {
            mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < mExpressionList.Count; i++)
            {
                DrawExpression(mExpressionList[i]);
            }

            DrawMenu();
            EditorGUILayout.EndScrollView();
        }

        private void DrawExpression(MathExpression expression)
        {

        }

        private void DrawMenu()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.95f));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Scene Expressions"))
            {

            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Load Selected Expressions"))
            {

            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
