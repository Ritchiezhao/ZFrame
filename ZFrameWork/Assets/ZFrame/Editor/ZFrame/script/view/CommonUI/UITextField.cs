using UnityEngine;
using UnityEditor;



namespace ZFEditor
{
    public class UITextField : UINode
    {
        public delegate void OnTextChange(UITextField lab, string oldText);

        protected static UnityGUIStyleWrapper TmpTextFieldStyle = new UnityGUIStyleWrapper("Default_TextField");

        public FontInfo Font;
        public TextAnchor Alignment;
        public Texture2D Background;

        public string Text;

        public bool Delayed = false;

        public OnTextChange onTextChange = null;

        public UITextField()
        {
            Font.font = ResCache.GetFont("/Fonts/MicrosoftYaHei.ttf");
            Font.size = 12;
            Font.style = FontStyle.Normal;
            Font.color = Color.white;

            Alignment = TextAnchor.MiddleLeft;

            Background = ResCache.GetTexture("/Textures/TextField_Background.psd");
        }

        void FillStyle()
        {
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);
            // fill style //
            TmpTextFieldStyle.SetFont(this.Font);
            TmpTextFieldStyle.SetAlignment(this.Alignment);
            TmpTextFieldStyle.SetBackground(this.Background);

            // draw //
            Rect relative = GetRelative(clipArea);
            string ret;
            if (Delayed)
                ret = EditorGUI.DelayedTextField(relative, Text, TmpTextFieldStyle.UnityStyle);
            else
                ret = GUI.TextField(relative, Text, TmpTextFieldStyle.UnityStyle);

            if (!ret.Equals(Text))
            {
                string oldText = Text;
                Text = ret;
                if (onTextChange != null)
                    onTextChange(this, oldText);
            }
        }
    }
}



