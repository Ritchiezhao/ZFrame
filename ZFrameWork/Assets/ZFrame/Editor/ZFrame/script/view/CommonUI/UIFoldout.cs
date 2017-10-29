
using UnityEngine;
using UnityEditor;


namespace ZFEditor
{
    public class UIFoldout : UINode
    {
        public delegate void OnClickFunc(UIFoldout btn);

        protected static UnityGUIStyleWrapper TmpFoldoutStyle = new UnityGUIStyleWrapper("Default_Foldout");

        public Texture2D Background_Open;
        public Texture2D Background_Close;

        public bool isFoldOut = false;

        public OnClickFunc onClick;

        public UIFoldout()
        {
            Background_Open = ResCache.GetTexture("/Textures/Foldout_Open.png");
            Background_Close = ResCache.GetTexture("/Textures/Foldout_Close.png");
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            // fill style //
            if (isFoldOut)
                TmpFoldoutStyle.SetBackground(this.Background_Open);
            else
                TmpFoldoutStyle.SetBackground(this.Background_Close);

            // draw //
            Rect relative = GetRelative(clipArea);
            GUI.Box(relative, "", TmpFoldoutStyle.UnityStyle);
        }

        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseDown)
            {
                if (!evt.handled && this.WorldArea.Contains(evt.MousePos))
                {
                    isFoldOut = !isFoldOut;
                    if (onClick != null)
                        onClick(this);
                    evt.handled = true;
                }
            }
        }
    }
}