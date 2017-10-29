
using UnityEngine;
using UnityEditor;


namespace ZFEditor
{
    public class UIButton : UINode
    {
        public delegate void OnClick(UIButton btn);

        protected static UnityGUIStyleWrapper TmpButtonStyle = new UnityGUIStyleWrapper("Default_Button");

        public FontInfo Font;
        public TextAnchor Alignment;
        public Texture2D Background_Down;
        public Texture2D Background_Up;

        public OnClick onClick = null;

        public bool HoldUpEvent = true;


        public string Text;

        public Texture Icon;

        bool isDown = false;

        public UIButton()
        {
            // default style //
            Font.font = ResCache.GetFont("/Fonts/MicrosoftYaHei.ttf");
            Font.size = 12;
            Font.style = FontStyle.Normal;
            Font.color = Color.white;

            Alignment = TextAnchor.MiddleCenter;

            Background_Up = ResCache.GetTexture("/Textures/Button_Background_Up.psd");
            Background_Down = ResCache.GetTexture("/Textures/Button_Background_Down.psd");
        }
        
        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            // fill style //
            TmpButtonStyle.SetFont(this.Font);
            TmpButtonStyle.SetAlignment(this.Alignment);
            if (isDown)
                TmpButtonStyle.SetBackground(this.Background_Down);
            else
                TmpButtonStyle.SetBackground(this.Background_Up);

            // draw //
            Rect relative = GetRelative(clipArea);
            if (Icon != null)
                GUI.Box(relative, Icon, TmpButtonStyle.UnityStyle);
            else
                GUI.Box(relative, Text, TmpButtonStyle.UnityStyle);
        }

        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseDown)
            {
                if (!evt.handled && this.WorldArea.Contains(evt.MousePos))
                {
                    isDown = true;
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                if (isDown && !evt.handled && this.WorldArea.Contains(evt.MousePos))
                {
                    if (onClick != null)
                        onClick(this);
                    if (HoldUpEvent)
                        evt.handled = true;
                }
                isDown = false;
            }
        }

    }
}