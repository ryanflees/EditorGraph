using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;
using CutscenePlanner;
namespace CutscenePlanner.Editor.Utils
{
    public class CutsceneIconUtility
    {
        [DidReloadScripts]
        static CutsceneIconUtility()
        {
            EditorApplication.projectWindowItemOnGUI = ItemOnGUI;
            //EditorApplication.
        }

        static void ItemOnGUI(string guid, Rect rect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (type == typeof(CutsceneAsset))
            {
                Rect oryginal = rect;
                if (oryginal.height > 16)
                {
                    oryginal.height -= 18; 
                    oryginal.width -= 14;
                    rect.width = Mathf.Clamp(Resources.EditorResources.CutsceneIcon.TextureWidth, 0, oryginal.width);
                    rect.height = Mathf.Clamp(Resources.EditorResources.CutsceneIcon.TextureHeight, 0, oryginal.height);
                    rect.y += oryginal.height / 2 - rect.height / 2 + 2; //-7;
                    rect.x += oryginal.width / 2 - rect.width / 2 + 7;
                }
                else
                {
                    rect.height -= 3;
                    rect.width = rect.height;
                    rect.width -= 1;
                    rect.y += 2;
                    rect.x += 5;
                }
                GUI.DrawTexture(rect, Resources.EditorResources.CutsceneIcon.Texture);
            }
        }
    }
}