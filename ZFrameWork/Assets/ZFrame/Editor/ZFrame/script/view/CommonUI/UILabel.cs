using UnityEngine;
using UnityEditor;



namespace ZFEditor
{
    public class UILabel : UINode
    {
        protected static UnityGUIStyleWrapper TmpLabelStyle = new UnityGUIStyleWrapper("Default_Label");

        public FontInfo Font;
        public TextAnchor Alignment;
        public Texture2D Background;
        public float TextOffset = 0;

        public string Text;

        public UILabel()
        {
            Font.font = ResCache.GetFont("/Fonts/MicrosoftYaHei.ttf");
            Font.size = 12;
            Font.style = FontStyle.Normal;
            Font.color = Color.white;

            Alignment = TextAnchor.MiddleLeft;

            Background = null;
        }
        
        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            // fill style //
            TmpLabelStyle.SetFont(this.Font);
            TmpLabelStyle.SetAlignment(this.Alignment);
            TmpLabelStyle.SetBackground(this.Background);
            TmpLabelStyle.SetTextOffset(TextOffset);

            // draw //
            Rect relative = GetRelative(clipArea);
            GUI.Label(relative, Text, TmpLabelStyle.UnityStyle);
        }
    }
}



