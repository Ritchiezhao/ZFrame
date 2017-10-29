
using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace ZFEditor
{
    public class UILayout_Free : UILayout
    {
        float mNewWidth = 0.0f;
        const float ResizeBarRadius = 6.0f;
        
        public override void Update()
        {
            for (int i = 0; i < mItems.Count; ++i)
            {
                mItems[i].CorrectSize();
                mItems[i].UpdateWorldArea(new Rect(mItems[i].LayoutPos + mWorldPos, mItems[i].LayoutSize));
            }
        }
    }
}