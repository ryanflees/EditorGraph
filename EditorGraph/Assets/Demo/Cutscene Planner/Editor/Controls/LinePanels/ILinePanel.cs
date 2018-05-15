using UnityEngine;

namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Line panel interface. </summary>
    public interface ILinePanel
    {
        /// <summary> Do component rect recalcing, basing on the given one [have to be invoked in OnGUI]. </summary>
        /// <param name="rect">Window, in which should be shown timeline.</param>
        /// <returns>Real size of the component.</returns>
        Rect GUICalcRect(Rect rect);
        /// <summary> Draws the component [have to be invoked in OnGUI].</summary>
        /// <param name="rect">X position for the timeline.</param>
        /// <param name="scrollDelta">Scroll delta.</param>
        /// <returns>Real component size.</returns>
        Rect GUIDraw(float xPos, float scrollDelta);
        /// <summary> Method that is invoked in Update signal. </summary>
        /// <param name="allowDrag">True if drag is allowed. False otherwise.</param>
        void Update(bool allowDrag);
    }
}