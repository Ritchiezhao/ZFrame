using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

namespace ZFEditor
{
    public class ResCache
    {
        static string ResDir = "Assets/Editor/sgaf/res";

        static Dictionary<string, Font> FontDic = new Dictionary<string, Font>();
        public static Font GetFont(string path)
        {
            Font ret;
            if (!FontDic.TryGetValue(path, out ret))
            {
                ret = AssetDatabase.LoadAssetAtPath<Font>(ResDir + path);
                if (ret != null)
                {
                    FontDic.Add(path, ret);
                }
            }
            return ret;
        }

        static Dictionary<string, Texture2D> TexDic = new Dictionary<string, Texture2D>();
        public static Texture2D GetTexture(string path)
        {
            Texture2D ret;
            if (!TexDic.TryGetValue(path, out ret))
            {
                ret = AssetDatabase.LoadAssetAtPath<Texture2D>(ResDir + path);
                if (ret != null)
                {
                    TexDic.Add(path, ret);
                }
            }
            return ret;
        }

        static GUISkin skin;
        public static GUISkin GetSkin()
        {
            if (skin == null)
                skin = AssetDatabase.LoadAssetAtPath<GUISkin>(ResDir + "/sgafEditorSkin.guiskin");
            return skin;
        }
    }
}