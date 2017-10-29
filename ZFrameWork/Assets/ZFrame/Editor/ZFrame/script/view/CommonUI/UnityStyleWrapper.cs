
using UnityEngine;
using UnityEditor;



namespace ZFEditor
{
    public struct FontInfo
    {
        public Font font;
        public int size;
        public FontStyle style;
        public Color color;
    }

    public class UnityGUIStyleWrapper
    {
        string fromStyle = null;
        GUIStyle _UnityStyle;
        public GUIStyle UnityStyle
        {
            get
            {
                if (_UnityStyle == null)
                    _UnityStyle = new GUIStyle(ResCache.GetSkin().GetStyle(fromStyle));
                return _UnityStyle;
            }
        }


        public UnityGUIStyleWrapper(string predefinedUnityStyle)
        {
            fromStyle = predefinedUnityStyle;
        }

        public UnityGUIStyleWrapper(GUIStyle predefinedUnityStyle)
        {
            _UnityStyle = new GUIStyle(predefinedUnityStyle);
        }

        public void SetBackground(Texture2D tex)
        {
            UnityStyle.normal.background = tex;
            UnityStyle.focused.background = tex;
            UnityStyle.hover.background = tex;
            UnityStyle.active.background = tex;

            UnityStyle.onNormal.background = null;
            UnityStyle.onFocused.background = null;
            UnityStyle.onHover.background = null;
            UnityStyle.onActive.background = null;
        }

        public void SetBorder(RectOffset offset)
        {
            UnityStyle.border = offset;
        }

        void SetTextColor(Color color)
        {
            UnityStyle.normal.textColor = color;
            UnityStyle.focused.textColor = color;
            UnityStyle.hover.textColor = color;
            UnityStyle.active.textColor = color;

            UnityStyle.onNormal.textColor = color;
            UnityStyle.onFocused.textColor = color;
            UnityStyle.onHover.textColor = color;
            UnityStyle.onActive.textColor = color;
        }

        public void SetAlignment(TextAnchor alignment)
        {
            UnityStyle.alignment = alignment;
        }

        public void SetFont(FontInfo font)
        {
            UnityStyle.font = font.font;
            UnityStyle.fontStyle = font.style;
            UnityStyle.fontSize = font.size;
            SetTextColor(font.color);
        }

        public void SetTextOffset(float offset)
        {
            UnityStyle.contentOffset = new Vector2(offset, 0);
        }
    }
}