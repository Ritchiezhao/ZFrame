using UnityEngine;
using UnityEditor;




namespace ZFEditor
{
    public class UIPreview : UINode
    {
        Editor mPreviewEditor;
        Object mUnityObj;

        public void Init()
        {
            Texture2D tex = ResCache.GetTexture("/Textures/Icon_Category.png");
            SetObject(tex);
        }

        public void SetObject(UnityEngine.Object obj)
        {
            mUnityObj = obj;
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            Rect relative = GetRelative(clipArea);

            if (mUnityObj != null)
            {
                if (mPreviewEditor == null || mPreviewEditor.target != mUnityObj)
                    mPreviewEditor = Editor.CreateEditor(mUnityObj);

                //EditorStyles.
                GUISkin tmp = GUI.skin;
                GUI.skin = null;
                mPreviewEditor.DrawPreview(relative);
                GUI.skin = tmp;
            }
        }
    }
}