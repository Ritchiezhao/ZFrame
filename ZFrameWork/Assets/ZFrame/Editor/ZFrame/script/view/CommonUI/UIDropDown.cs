using UnityEngine;
using UnityEditor;



namespace ZFEditor
{
    public class UIDropDown : UINode
    {
        public delegate void OnChangeFunc(UIDropDown ui, int newVal);

        protected static UnityGUIStyleWrapper TmpDropDownStyle = new UnityGUIStyleWrapper(EditorStyles.popup);

        public FontInfo Font;
        public TextAnchor Alignment;

        public string[] Options;
        public int Selected = 0;

        public OnChangeFunc onChange;

        public UIDropDown()
        {
            Font.font = ResCache.GetFont("/Fonts/MicrosoftYaHei.ttf");
            Font.size = 12;
            Font.style = FontStyle.Normal;
            Font.color = Color.white;

            Alignment = TextAnchor.MiddleLeft;
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            // fill style //
            TmpDropDownStyle.SetFont(this.Font);
            TmpDropDownStyle.SetAlignment(this.Alignment);

            // draw //
            Rect relative = GetRelative(clipArea);
            Selected = EditorGUI.Popup(relative, Selected, Options, TmpDropDownStyle.UnityStyle);
            if (onChange != null)
                onChange(this, Selected);
        }
    }
}



