using CutscenePlanner.Editor.Utils;
using UnityEngine;

namespace CutscenePlanner.Editor.Controls.LinePanels
{

    /// <summary> Represents the timeline bookmark label.</summary>
    public class TimeLabel : Label
    {
        /// <summary> Dimensions of the label on the texture. </summary>
        public override Rect TextureRect { get {  return new Rect(_timeLinePanel.TimeToWorldX(SourceData.Start), Owner.Rect.height - _timeLinePanel.SmallStroke + 5, _timeLinePanel.TimeLineEngine.TimeToX(SourceData.Duration), _timeLinePanel.SmallStroke - 5); } }
        /// <summary> Create new time  bookmark label. </summary>
        /// <param name="owner"> Owning timeline.></param>
        /// <param name="sourceData">Source data.</param>
        public TimeLabel(TimeLinePanel owner, TimeLabelData sourceData) : base(owner, owner, sourceData) { }

        /// <summary> Draws the label on the texture, blending with already existing pixels. </summary>
        /// <param name="toDraw">Texture on which label should be printed.</param>
        /// <param name="oryginal">Oryginal source of the pixel colors.</param>
        /// <remarks>Invoke Refresh first to allow component to be redrawn.  </remarks>
        public override void Draw(Texture2D oryginal, Texture2D toDraw)
        {
            _redrawOrder = false;
            int maxWidth = (int)Owner.Rect.width;
            int maxHeight = (int)Owner.Rect.height;


            Rect rect = TextureRect;

            int x = (int)rect.x;
            int y = Mathf.Clamp((int)rect.y - 1, 0, maxHeight);

            int width = (int)rect.width;
            if (x < 0)
            {
                width += x;
                x = 0;
            }

            if (x + width > maxWidth)
                width = maxWidth - x;

            if (width < 0)
                return;
            int height = Mathf.Clamp((int)rect.height, 0, maxHeight);

            if (width - _borderThickness * 2 > 1)
            {
                Color[] colors = oryginal.GetPixels(x + _borderThickness, y + _borderThickness, width - _borderThickness * 2, height - _borderThickness * 2);
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].r = colors[i].r * 0.2f + Color.r * 0.8f;
                    colors[i].g = colors[i].g * 0.2f + Color.g * 0.8f;
                    colors[i].b = colors[i].b * 0.2f + Color.b * 0.8f;
                    colors[i].a = 1;
                }
                toDraw.SetPixels(x + _borderThickness, y + _borderThickness, width - _borderThickness * 2, height - _borderThickness * 2, colors);
            }


            int leftRightBorderPixelCount = _borderThickness * height;
            int upDownBorderPixelCount = width * _borderThickness - _borderThickness * _borderThickness * 2;
            int borderPixelCount = leftRightBorderPixelCount * 2 - upDownBorderPixelCount * 2;
            if (width * height < borderPixelCount)
                return;

            Color[] leftRightBorderColor = new Color[leftRightBorderPixelCount];
            Color[] upDownBorderColor = new Color[upDownBorderPixelCount];

            Color borderColor = ChangeColorBrightness(Color, _isSelected ? -0.4f : -0.2f);

            leftRightBorderColor.Fill(borderColor);
            upDownBorderColor.Fill(borderColor);

            //left board
            if (x + _borderThickness < maxWidth)
                toDraw.SetPixels(x, y, _borderThickness, height, leftRightBorderColor);
            //right board
            if (x + width - _borderThickness > 0)
                toDraw.SetPixels(x + width - _borderThickness, y, _borderThickness, height, leftRightBorderColor);
            // bottom board
            toDraw.SetPixels(x + _borderThickness, y, width - _borderThickness * 2, _borderThickness, upDownBorderColor);
            // top board
            toDraw.SetPixels(x + _borderThickness, y + (height - _borderThickness), width - _borderThickness * 2, _borderThickness, upDownBorderColor);

        }
    }
}
