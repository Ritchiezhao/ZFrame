using UnityEngine;
using UnityEditor;



namespace ZFEditor
{
    public class UIToggle : UINode
    {
        public delegate void OnClick(UIToggle toggle);

        static Texture2D falseIcon;
        static Texture2D trueIcon;

        public OnClick onClick;

        public bool Toggle
        {
            get; set;
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            Rect relative = GetRelative(clipArea);
            if (falseIcon == null)
            {
                falseIcon = ResCache.GetTexture("/Textures/Toggle_False.jpg");
                trueIcon = ResCache.GetTexture("/Textures/Toggle_True.jpg");
            }

            if (Toggle)
                GUI.Box(relative, trueIcon, null);
            else
                GUI.Box(relative, falseIcon, null);
        }

        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseUp)
            {
                if (!evt.handled && this.WorldArea.Contains(evt.MousePos))
                {
                    this.Toggle = !this.Toggle;
                    if (onClick != null)
                        onClick(this);
                }
            }
        }
    }
}