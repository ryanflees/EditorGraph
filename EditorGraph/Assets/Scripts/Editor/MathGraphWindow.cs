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
        private float mCellHeight = 120f;
        private float mCellMenuWidth = 250f;

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
            DrawMenu();
            mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < mExpressionList.Count; i++)
            {
                DrawExpression(i);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DrawExpression(int index)
        {
            float cellWidth = position.width * 0.97f;
            float cellMenuWidth = mCellMenuWidth;
            EditorGUILayout.BeginVertical("box", GUILayout.Width(cellWidth), GUILayout.Height(mCellHeight));

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(cellMenuWidth));
            mExpressionList[index] = EditorGUILayout.ObjectField(mExpressionList[index], typeof(MathExpression), true) as MathExpression;
            if (mExpressionList[index] != null)
            {
                mExpressionList[index].ScrollPosition = EditorGUILayout.BeginScrollView(mExpressionList[index].ScrollPosition);
           
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

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ver Alignment", GUILayout.Width(100));
                mExpressionList[index].VerAlignType = (MathExpression.YAxisAlignType)EditorGUILayout.EnumPopup(mExpressionList[index].VerAlignType);
                EditorGUILayout.EndHorizontal();

                if (mExpressionList[index].VerAlignType == MathExpression.YAxisAlignType.Custom)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Y Offset", GUILayout.Width(50));
                    mExpressionList[index].YAxisOffset = EditorGUILayout.FloatField(mExpressionList[index].YAxisOffset);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Hor Alignment", GUILayout.Width(100));
                mExpressionList[index].HorAlignType = (MathExpression.XAxisAlignType)EditorGUILayout.EnumPopup(mExpressionList[index].HorAlignType);
                EditorGUILayout.EndHorizontal();

                if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.Custom)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("X Offset", GUILayout.Width(50));
                    mExpressionList[index].XAxisOffset = EditorGUILayout.FloatField(mExpressionList[index].XAxisOffset);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Y Scale", GUILayout.Width(50));
                mExpressionList[index].YScale = EditorGUILayout.Slider(mExpressionList[index].YScale, 0.001f, 100);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("X Scale", GUILayout.Width(50));
                mExpressionList[index].XScale = EditorGUILayout.Slider(mExpressionList[index].XScale, 0.001f, 100);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
           
            EditorGUILayout.BeginVertical("box");

            DrawExpressionCurve(cellMenuWidth, cellWidth, index);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawExpressionCurve(float cellMenuWidth, float cellWidth, int index)
        {
            if (mExpressionList[index] == null)
                return;
            float yCellOffset = 4f;
            Rect graphRect = new Rect(
                   cellMenuWidth + 30f,
                   index * (mCellHeight + yCellOffset) + 15f,
                   cellWidth - cellMenuWidth - 30f,
                   mCellHeight * 0.9f
                   );

            float zeroAtXAxis = graphRect.x + graphRect.width * 0.5f; // cellMenuWidth + 200f;
            float zeroAtYAxis = graphRect.y + graphRect.height * 0.5f;

            if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.Left)
            {
                zeroAtXAxis = graphRect.x;
            }
            else if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.OneThird)
            {
                zeroAtXAxis = graphRect.x + graphRect.width * 0.3f;
            }
            else if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.Middle)
            {
                zeroAtXAxis = graphRect.x + graphRect.width * 0.5f;
            }
            else if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.Right)
            {
                zeroAtXAxis = graphRect.x + graphRect.width;
            }
            else if (mExpressionList[index].HorAlignType == MathExpression.XAxisAlignType.Custom)
            {
                zeroAtXAxis = graphRect.x + graphRect.width * 0.5f + mExpressionList[index].XAxisOffset;
            }

            if (mExpressionList[index].VerAlignType == MathExpression.YAxisAlignType.Top)
            {
                zeroAtYAxis = graphRect.y;
            }
            else if (mExpressionList[index].VerAlignType == MathExpression.YAxisAlignType.Middle)
            {
                zeroAtYAxis = graphRect.y + graphRect.height * 0.5f;
            }
            else if (mExpressionList[index].VerAlignType == MathExpression.YAxisAlignType.Bottom)
            {
                zeroAtYAxis = graphRect.y + graphRect.height;
            }
            else if (mExpressionList[index].VerAlignType == MathExpression.YAxisAlignType.Custom)
            {
                zeroAtYAxis = graphRect.y + graphRect.height * 0.5f + mExpressionList[index].YAxisOffset;
            }

            Handles.color = Color.black;
            Vector3 xaxisStart = new Vector3(graphRect.x, zeroAtYAxis, 0);
            Vector3 xaxisEnd = new Vector3(graphRect.x + graphRect.width, zeroAtYAxis, 0);
            Handles.DrawLine(xaxisStart, xaxisEnd);
            Vector3 yaxisStart = new Vector3(zeroAtXAxis, graphRect.y, 0);
            Vector3 yaxisEnd = new Vector3(zeroAtXAxis, graphRect.y + graphRect.height, 0);
            Handles.DrawLine(yaxisStart, yaxisEnd);

            Handles.color = Color.red;
            for (float i = zeroAtXAxis; i <= graphRect.x + graphRect.width; i ++)
            {
                float xPos = i - zeroAtXAxis;
                float x = xPos / mExpressionList[index].XScale;
                float y = -mExpressionList[index].CalculateFunction(x);
                float yPos = y * mExpressionList[index].YScale + zeroAtYAxis;
                yPos = Mathf.Clamp(yPos, graphRect.y, graphRect.y + graphRect.height);
                Handles.DrawLine(new Vector3(i, zeroAtYAxis, 0), new Vector3(i, yPos, 0));
            }

            for(float i = zeroAtXAxis; i >= graphRect.x; i --)
            {
                float xPos = i - zeroAtXAxis;
                float x = xPos / mExpressionList[index].XScale;
                float y = -mExpressionList[index].CalculateFunction(x);
                float yPos = y * mExpressionList[index].YScale + zeroAtYAxis;
                yPos = Mathf.Clamp(yPos, graphRect.y, graphRect.y + graphRect.height);
                Handles.DrawLine(new Vector3(i, zeroAtYAxis, 0), new Vector3(i, yPos, 0));
            }

            float precisionX = GetScalePrecisionX(mExpressionList[index].XScale);
            //for (int i = 0; i )
            int scaleCountNeededX = (int)(graphRect.width / (precisionX * mExpressionList[index].XScale));
            scaleCountNeededX = Mathf.Min(scaleCountNeededX, 100);
            for (int i = -scaleCountNeededX; i < scaleCountNeededX; i++)
            {
                float x = precisionX * i;
                float xPos = x * mExpressionList[index].XScale;
                float finalXPos = zeroAtXAxis + xPos;
                if (finalXPos >= graphRect.x && finalXPos <= graphRect.x + graphRect.width)
                {
                    Handles.Label(new Vector3(finalXPos, zeroAtYAxis + 10f, 0), x.ToString());
                    Handles.color = Color.black;
                    Handles.DrawLine(new Vector3(finalXPos, zeroAtYAxis, 0), new Vector3(finalXPos, zeroAtYAxis + 3, 0));
                }
            }

            float precisionY = GetScalePrecisionY(mExpressionList[index].YScale);
            int scaleCountNeededY = (int)(graphRect.height / (precisionY * mExpressionList[index].YScale));
            scaleCountNeededY = Mathf.Min(scaleCountNeededY, 100);
            for (int i = - scaleCountNeededY; i < scaleCountNeededY; i ++)
            {
                float y = precisionY * i;
                float yPos = - y * mExpressionList[index].YScale;
                float finalYPos = zeroAtYAxis + yPos;
                if (finalYPos >= graphRect.y && finalYPos <= graphRect.y + graphRect.height)
                {
                    Handles.Label(new Vector3(zeroAtXAxis + 10, finalYPos, 0), y.ToString());
                    Handles.color = Color.black;
                    Handles.DrawLine(new Vector3(zeroAtXAxis + 3, finalYPos, 0), new Vector3(zeroAtXAxis, finalYPos, 0));
                }
            }
        }

        private float GetScalePrecisionX(float scale)
        {
            if (scale > 50)
            {
                return 1;
            }
            if (scale > 20)
            {
                return 5;
            }
            if (scale > 10)
            {
                return 10;
            }
            if (scale > 1)
            {
                return 20;
            }
            if (scale > 0.5f)
            {
                return 100f;
            }
            if (scale > 0.1)
            {
                return 200f;
            }
            if (scale > 0.01)
            {
                return 500f;
            }
            if (scale > 0.001)
            {
                return 5000f;
            }
            return 50000f;
        }

        private float GetScalePrecisionY(float scale)
        {
            if (scale > 50)
            {
                return 0.5f;
            }
            if (scale > 20)
            {
                return 2;
            }
            if (scale > 10)
            {
                return 5;
            }
            if (scale > 1)
            {
                return 20;
            }
            if (scale > 0.5f)
            {
                return 100f;
            }
            if (scale > 0.1)
            {
                return 200f;
            }
            if (scale > 0.01)
            {
                return 500f;
            }
            if (scale > 0.001)
            {
                return 10000f;
            }
            return 100000f;
        }

        private void DrawMenu()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.95f));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cell Height", GUILayout.Width(70f));
            mCellHeight = EditorGUILayout.FloatField(mCellHeight, GUILayout.Width(70));

            EditorGUILayout.LabelField("Cell Menu Width", GUILayout.Width(100f));
            mCellMenuWidth = EditorGUILayout.FloatField(mCellMenuWidth, GUILayout.Width(70));
            //EditorGUILayout.Space();
            if (GUILayout.Button("Load Scene Expressions", GUILayout.Width(180)))
            {
                AddExpressionsInScene();
            }
            if (GUILayout.Button("Load Selected Expressions", GUILayout.Width(180)))
            {
                AddSelectedExpressions();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AddSelectedExpressions()
        {
            Object[] objs = Selection.GetFiltered(typeof(MathExpression), SelectionMode.Unfiltered);
            for (int i = 0; i < objs.Length; i++)
            {
                MathExpression expression = (MathExpression)objs[i];
                if (!mExpressionList.Contains(expression))
                {
                    mExpressionList.Add(expression);
                }
            }
        }

        private void AddExpressionsInScene()
        {
            MathExpression[] expressions = GameObject.FindObjectsOfType<MathExpression>();
            for (int i = 0; i< expressions.Length; i ++)
            {
                if (!mExpressionList.Contains(expressions[i]))
                {
                    mExpressionList.Add(expressions[i]);
                }
            }
        }
    }
}
