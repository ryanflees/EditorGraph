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
        private float mCellHeight = 100f;
        private float mCellMenuWidth = 200f;

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
                DrawExpression(i);
            }

            DrawMenu();
            EditorGUILayout.EndScrollView();
        }

        private void DrawExpression(int index)
        {
            float cellWidth = position.width * 0.97f;
            float cellMenuWidth = 200f;
            EditorGUILayout.BeginVertical("box", GUILayout.Width(cellWidth), GUILayout.Height(mCellHeight));

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(cellMenuWidth));
            mExpressionList[index] = EditorGUILayout.ObjectField(mExpressionList[index], typeof(MathExpression), true) as MathExpression;

            if (GUILayout.Button("Reset Value"))
            {
                mExpressionList[index].ResetValue();
                Repaint();
            }
            if (GUILayout.Button("Remove"))
            {
                mExpressionList.RemoveAt(index);
                return;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMenu()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.95f));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cell Height", GUILayout.Width(70f));
            mCellHeight = EditorGUILayout.FloatField(mCellHeight);

            EditorGUILayout.LabelField("Cell Menu Width", GUILayout.Width(100f));
            mCellMenuWidth = EditorGUILayout.FloatField(mCellMenuWidth);
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
