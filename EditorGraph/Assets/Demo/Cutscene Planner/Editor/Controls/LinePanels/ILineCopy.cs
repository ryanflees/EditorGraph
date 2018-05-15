namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Line panel copy interface</summary>
    public interface ILineCopy
    {
        
        /// <summary> Data to be copied.  </summary>
        TimedData CopyData { get; set; }
        /// <summary> True if data to be copied are available. False otherwise.  </summary>
        bool CopyDataAvailable { get; }

        /// <summary> Method that copy selected panel.</summary>
        void CopySelectedPanel();
        /// <summary> Method that copy and remove selected panel.</summary>
        void CutSelecedPanel();
        /// <summary> Method that assign copied panel data to existing one.</summary>
        void PasteToSelectedPanel();
        /// <summary> Method that assign copied panel data to new one.</summary>
        void PasteToNewPanel();
    }
}