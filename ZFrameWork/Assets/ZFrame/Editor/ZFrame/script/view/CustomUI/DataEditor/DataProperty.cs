using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class DataProperty
    {
        GObject mCurObject;

        UIContainer mContainer;

        UILabel mUITitle;
        UILabel mUIParent;
        UIProperty mUIProperty;
        UIPreview mUIPreview;


        public void Init(UIContainer container)
        {
            mContainer = container;
            mContainer.SetContentOffset(new Vector2(1, 5));

            mUITitle = mContainer.CreateNode<UILabel>();
            mUITitle.FixHeight(20);
            mUITitle.MouseResizing = false;
            mUITitle.Alignment = TextAnchor.MiddleCenter;
            mUITitle.Font.size = 14;
            mUITitle.Font.style = FontStyle.Bold;
            mUITitle.Font.color = new Color(1f, 0.8f, 0.3f);

            mUIParent = mContainer.CreateNode<UILabel>();
            mUIParent.FixHeight(20);
            mUIParent.Alignment = TextAnchor.MiddleCenter;
            mUIParent.MouseResizing = false;
            mUIParent.Font.size = 14;
            //mUIParent.Font.style = FontStyle.Bold;
            mUIParent.Font.color = new Color(0.2f, 0.8f, 0.7f);

            mUIProperty = mContainer.CreateNode<UIProperty>();
            mUIProperty.SetMinSize(new Vector2(10, 400));
            //mUIProperty.Init();

            mUIPreview = mContainer.CreateNode<UIPreview>();
            mUIPreview.SetMinSize(new Vector2(10, 200));
            //mUIPreview.Init();
        }

        public void SetObject(GObject obj)
        {
            mCurObject = obj;

            if (mCurObject != null)
            {
                mUITitle.Text = mCurObject.ID;
                if (!string.IsNullOrEmpty(mCurObject.Parent))
                {
                    mUIParent.Active = true;
                    mUIParent.Text = mCurObject.Parent;
                }
                else
                {
                    mUIParent.Active = false;
                }
            }
            mUIProperty.SetData(mCurObject);
        }
    }
}








