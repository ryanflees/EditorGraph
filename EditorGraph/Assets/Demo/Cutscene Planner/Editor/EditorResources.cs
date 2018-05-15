using UnityEngine;

namespace CutscenePlanner.Editor.Resources
{
    /// <summary> Collection of all cutscene planner images </summary>
    public static class EditorResources
    {
        /// <summary> Resource image of the "Play"</summary>
        public static ImageResource AVNext      { get { if (_AVNext == null)    _AVNext     = new ImageResource("Img/next",     "❚▶");  return _AVNext; } }
        /// <summary> Resource image of the "Skip next"</summary>
        public static ImageResource AVPrevious  { get { if (_AVPrevious == null)_AVPrevious = new ImageResource("Img/previous", "❚◀");  return _AVPrevious; } }
        /// <summary> Resource image of the "Skip previous"</summary>
        public static ImageResource AVPlay      { get { if (_AVPlay == null)    _AVPlay     = new ImageResource("Img/play",     "▶");   return _AVPlay; } }
        /// <summary> Resource image of the "Pause"</summary>
        public static ImageResource AVPause     { get { if (_AVPause == null)   _AVPause    = new ImageResource("Img/pause",    "❚❚");   return _AVPause; } }
        /// <summary> Resource image of the "Stop"</summary>
        public static ImageResource AVStop      { get { if (_AVStop == null)    _AVStop     = new ImageResource("Img/stop",     "◼");    return _AVStop; } }
        /// <summary> Resource image of the "Arrow up"</summary>
        public static ImageResource ArrowUp      { get { if (_ArrowUp == null) _ArrowUp = new ImageResource("Img/arrow_up", "^"); return _ArrowUp; } }
        /// <summary> Resource image of the "Arrow down"</summary>
        public static ImageResource ArrowDown { get { if (_ArrowDown == null) _ArrowDown = new ImageResource("Img/arrow_down", "v"); return _ArrowDown; } }
        /// <summary> Resource image of the "Arrow left"</summary>
        public static ImageResource ArrowLeft { get { if (_ArrowLeft == null) _ArrowLeft = new ImageResource("Img/arrow_left", "<"); return _ArrowLeft; } }
        /// <summary> Resource image of the "Arrow right"</summary>
        public static ImageResource ArrowRight { get { if (_ArrowRight == null) _ArrowRight = new ImageResource("Img/arrow_right", ">"); return _ArrowRight; } }
        /// <summary> Resource image of the "Cutsceme Icon"</summary>
        public static ImageResource CutsceneIcon { get { if (_CutsceneIcon == null) _CutsceneIcon = new ImageResource("Img/cutscene_icon", "H"); return _CutsceneIcon; } }
        /// <summary> Resource image of the "Cutsceme Icon small"</summary>
        public static ImageResource CutsceneIconSmall { get { if (_CutsceneIconSmall == null) _CutsceneIconSmall = new ImageResource("Img/cutscene_icon_small", "H"); return _CutsceneIconSmall; } }


        private static ImageResource _AVNext;
        private static ImageResource _AVPrevious;
        private static ImageResource _AVPlay;
        private static ImageResource _AVPause;
        private static ImageResource _AVStop;
        private static ImageResource _ArrowUp;
        private static ImageResource _ArrowDown;
        private static ImageResource _ArrowLeft;
        private static ImageResource _ArrowRight;
        private static ImageResource _CutsceneIcon;
        private static ImageResource _CutsceneIconSmall;

        /// <summary> Represents the image resource</summary>
        public class ImageResource
        {
            /// <summary> Size of the texture or alternative text if texture not available (mesured in label skin).</summary>
            public readonly Vector2 Size;
            /// <summary> GUIContent based on loaded texture or alternative text if texture not available. </summary>
            public readonly GUIContent GUIContent;
            /// <summary> Loaded texture or null, if texture not available. </summary>
            public readonly Texture2D Texture; 
            /// <summary> Alternative text for this resource, if texture not available. </summary>
            public readonly string Alternative;
            /// <summary> True if only alternative string is available. False otherwise. </summary>
            public bool OnlyString { get { return Texture == null; } }

            public readonly int TextureWidth;

            public readonly int TextureHeight;
            

            /// <summary>Creates new image resource.</summary>
            /// <param name="path">  Path for the texture in Resource folder.  </param>
            /// <param name="alternative"> Alternative text that will be used if texture is not available.</param>
            public ImageResource(string path, string alternative)
            {
                Texture = UnityEngine.Resources.Load<Texture2D>(path);
                if (Texture != null)
                {
                    TextureWidth = Texture.width;
                    TextureHeight = Texture.height;
                }

                Alternative = alternative;
                if (OnlyString)
                    GUIContent = new GUIContent(Alternative);
                else
                    GUIContent = new GUIContent(Texture);
                Size = new GUIStyle(GUI.skin.label).CalcSize(GUIContent);
            }
        }
    }
    
}