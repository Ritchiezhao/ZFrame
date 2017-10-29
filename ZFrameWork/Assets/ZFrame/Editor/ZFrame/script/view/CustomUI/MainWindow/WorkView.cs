using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class WorkView
    {
        UITabPanel mPages;

        List<GObject> mOpenedObjs = new List<GObject>();

        public void Init(UITabPanel panel)
        {
            mPages = panel;
        }

        public void OpenEditor(GObject obj)
        {
            if (obj == null)
                return;

            int index = -1;
            for (int i = 0; i < mOpenedObjs.Count; ++i)
            {
                if (mOpenedObjs[i].ID.Equals(obj.ID))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                mPages.ActivePage(index);
            }
            else
            {
                // todo: switch obj.catagory
                DataEditor editor = mPages.AddPage<DataEditor>(obj.ID);
                editor.SetObj(obj);
                mOpenedObjs.Add(obj);
            }
        }
    }
}