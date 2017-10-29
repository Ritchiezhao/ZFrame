using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace ZFEditor
{
    public enum FocusState
    {
        None,
        InSelection,
        Current
    }

    public class FocusManager
    {
        // 当前选择对象 //
        public UINode CurFocus
        {
            get; private set;
        }

        // 多选时历史选择对象，"不包含" 当前选择对象 //
        List<UINode> mFocusList = new List<UINode>();

        public void TakeFocus(UINode node, bool clear = true)
        {
            if (clear)
            {
                for (int i = 0; i < mFocusList.Count; ++i)
                    mFocusList[i].UpdateFoucs(FocusState.None);
            }

            if (CurFocus != node)
            {
                if (CurFocus != null)
                {
                    if (clear)
                    {
                        CurFocus.UpdateFoucs(FocusState.None);
                    }
                    else
                    {
                        mFocusList.Add(CurFocus);
                        CurFocus.UpdateFoucs(FocusState.InSelection);
                    }
                }

                CurFocus = node;
                if (node != null)
                {
                    node.UpdateFoucs(FocusState.Current);

                    if (mFocusList.Contains(node))
                    {
                        mFocusList.Remove(node);
                    }
                }
            }
            else
            {
                if (CurFocus != null)
                    CurFocus.UpdateFoucs(FocusState.Current);
            }
        }

        public bool isInFocusList(UINode node)
        {
            return mFocusList.Contains(node);
        }

        public List<UINode> GetFocusList()
        {
            return mFocusList;
        }
    }
}