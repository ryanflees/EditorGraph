using UnityEngine;
using System;
using CutscenePlanner.Editor.Utils;
using System.Collections.Generic;
using CutscenePlanner.Editor.Controls.LinePanels;

namespace CutscenePlanner.Editor.Controls
{
    /// <summary> Represents a panel </summary>
    public abstract class Panel
    {
        /// <summary> Unique ID of this Panel </summary>
        public readonly long ID;
        /// <summary> True if panel is selected. False otherwise. </summary>
        public bool IsSelected { get { return _isSelected; } }
        /// <summary> Color of the panel in normal state. </summary>
        public Color NormalColor {
            get { return _normalColor; }
            set { _normalColor = value; MakeBackgroundTexture(); }
        }
        /// <summary> Color of the panel in selected state. </summary>
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; MakeBackgroundTexture(); }
        }
        /// <summary>Thickness of the border. </summary>
        public int BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; MakeBackgroundTexture(); }
        }
        /// <summary>Color of the border.  </summary>
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _normalColor = value; MakeBackgroundTexture(); }
        }
        /// <summary> Rect of the component used on the last Draw. </summary>
        public Rect Rect { get { return _rect; } }
        /// <summary> Labels collection on this time panel.</summary>
        public List<Label> Labels { get { return _labels; } }

        protected List<Label> _labels;
        protected Texture2D _backgroundTexture;
        protected GUIStyle _boxContoursStyle;
        protected Rect _rect;

        private Color _normalColor = Color.gray;
        private Color _selectedColor = Color.white;
        private Color _borderColor = Color.black;
        private int _borderThickness = 1;
        private bool _isSelected;

        /// <summary> Creates a new Panel</summary>
        public Panel()
        {
            ID = DateTime.Now.ToBinary();
            _labels = new List<Label>();

            _rect = new Rect();
            _boxContoursStyle = new GUIStyle();
        }
        /// <summary> Selects this panel. </summary>
        /// <param name="select">True, if pannel is selected. False otherwise.</param>
        public virtual void Select(bool select)
        {
            _isSelected = select;
            MakeBackgroundTexture();
        }

        /// <summary> Draws panel.</summary>
        /// <remarks> Have to be invoked in OnGUI() signal. </remarks>
        /// <param name="rect">Size of the panel.</param>
        /// <param name="forceDraw">Force the texture draw, even if rects are the same.</param>
        /// <returns>Real component size.</returns>
        public virtual Rect Draw(Rect rect, bool forceDraw = false)
        {
            if (_backgroundTexture == null || !AreRectsEquals(rect, _rect) || forceDraw)
            {
                _rect = rect;
                MakeBackgroundTexture();
                _boxContoursStyle.normal.background = _backgroundTexture;
            }
            GUI.Box(rect, GUIContent.none, _boxContoursStyle);

            return rect;
        }
        /// <summary> Validate if given time range is emoty for new timelabel..</summary>
        /// <param name="start">Strat time range.</param>
        /// <param name="end">End time range.</param>
        /// <param name="withoutIndex">Index of the element that supposte to be ignored during test.</param>
        /// <param name="errorLabel">Label on which was time colision.</param>
        /// <param name="errorType">Error type. It can be: 0 - no error, 1 - error on start, 2 - error on end, 3 - other error.</param>
        /// <returns>True id Timelabel is valid. False otherwise.</returns>
        public bool IsTimelabelValid(TimeSpan start, TimeSpan end, int withoutIndex, out int errorType, out Label errorLabel)
        {
            errorLabel = null;
            errorType = 0;
            for (int i = 0; i < _labels.Count; i++)
            {
                if (i == withoutIndex)
                    continue;
                if (start <= _labels[i].SourceData.Start && end >= _labels[i].SourceData.End)
                {
                    errorLabel = _labels[i];
                    errorType = 3;
                    return false;
                }
                if (_labels[i].ContainsTime(start))
                {
                    errorLabel = _labels[i];
                    errorType = 1;
                    return false;
                }
                if (_labels[i].ContainsTime(end))
                {
                    errorLabel = _labels[i];
                    errorType = 2;
                    return false;
                }
                
            }
            return true;
        }

        /// <summary> Remove label with given sourceData.</summary>
        /// <param name="sourceData">Source data that is used by label to remove.</param>
        public void RemoveLabel(TimedData sourceData)
        {
            sourceData.MarkToDelete();
        }


        protected void MakeBackgroundTexture()
        {
            int textureWidth = (int)_rect.width;
            int textureHeight = (int)_rect.height;

            _backgroundTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.Repeat
            };
            int leftRightBorderPixelCount = _borderThickness * textureHeight;
            int upDownBorderPixelCount = _borderThickness * (textureWidth - _borderThickness* _borderThickness * 2);
            int borderPixelCount = leftRightBorderPixelCount * 2 - upDownBorderPixelCount * 2;
            if (textureWidth * textureHeight < borderPixelCount)
                return;
            int centerBorderPixelCount = textureWidth * textureHeight - borderPixelCount;

            Color[] leftRightBorderColor = new Color[leftRightBorderPixelCount];
            Color[] upDownBorderColor =new Color[_borderThickness * textureWidth];
            Color[] centerBorderColor = new Color[centerBorderPixelCount];

            leftRightBorderColor.Fill(BorderColor);
            upDownBorderColor.Fill(BorderColor);
            centerBorderColor.Fill(_isSelected ? SelectedColor : NormalColor);

            //left board
            _backgroundTexture.SetPixels(0, 0, _borderThickness, textureHeight, leftRightBorderColor);
            //right board
            _backgroundTexture.SetPixels(textureWidth - _borderThickness, 0, _borderThickness, textureHeight, leftRightBorderColor);
            // bottom board
            _backgroundTexture.SetPixels(_borderThickness, textureHeight - _borderThickness, textureWidth - _borderThickness * 2, _borderThickness, upDownBorderColor);
            // top board
            _backgroundTexture.SetPixels(_borderThickness, 0, textureWidth - _borderThickness * 2, _borderThickness, upDownBorderColor);          
            // center
            _backgroundTexture.SetPixels(_borderThickness, _borderThickness, textureWidth - _borderThickness * 2, textureHeight- _borderThickness * 2, centerBorderColor);

            _backgroundTexture.Apply();
        }
        
        protected static bool AreRectsEquals(Rect r1, Rect r2)
        {
            return r1.x == r2.x && r1.y == r2.y && r1.width == r2.width && r1.height == r2.height;
        }
        
    }
}