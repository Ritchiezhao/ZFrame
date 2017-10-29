
using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace ZFEditor
{
    public class UIGroupToggle : UINode
    {
        public delegate void OnSelectFunc(UIGroupToggle ui, int index);
        class ItemInfo
        {
            public Texture2D Icon;
            public string Text;
        }

        protected static UnityGUIStyleWrapper TmpGroupToggleStyle = new UnityGUIStyleWrapper("Default_Button");

        public OnSelectFunc onSelect;

        public float ItemMaxWidth = 100;
        public Texture2D ActiveBack;
        public Texture2D InactiveBack;
        public FontInfo Font;

        public int CurIndex = 0;

        List<ItemInfo> mItems = new List<ItemInfo>();

        public UIGroupToggle()
        {
            Font.font = ResCache.GetFont("/Fonts/MicrosoftYaHei.ttf");
            Font.size = 12;
            Font.style = FontStyle.Normal;
            Font.color = Color.white;
        }

        public override void Init()
        {
            base.Init();

            ActiveBack = ResCache.GetTexture("/Textures/Button_Background_Down.psd");
            InactiveBack = ResCache.GetTexture("/Textures/Button_Background_Up.psd");
        }

        public void Add(string text)
        {
            ItemInfo info = new ItemInfo();
            info.Text = text;
            mItems.Add(info);
        }

        public void Add(Texture2D icon, string text)
        {
            ItemInfo info = new ItemInfo();
            info.Icon = icon;
            info.Text = text;
            mItems.Add(info);
        }

        public void Remove(int index)
        {
            mItems.RemoveAt(index);
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            TmpGroupToggleStyle.SetFont(this.Font);

            if (mItems.Count > 0)
            {
                Rect relative = GetRelative(clipArea);

                float itemWidth = Math.Min(ItemMaxWidth, this.WorldArea.width / mItems.Count);
                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (i != CurIndex)
                    {
                        Rect itemRect = new Rect(relative.x + i * itemWidth, relative.y, itemWidth, relative.height);
                        DrawItem(mItems[i], itemRect, false);
                    }
                }
                DrawItem(mItems[CurIndex], new Rect(relative.x + CurIndex * itemWidth, relative.y, itemWidth, relative.height), true);
            }
        }

        void DrawItem(ItemInfo info, Rect rect, bool active)
        {
            if (active)
                TmpGroupToggleStyle.SetBackground(ActiveBack);
            else
                TmpGroupToggleStyle.SetBackground(InactiveBack);
            GUI.Box(rect, "", TmpGroupToggleStyle.UnityStyle);

            TmpGroupToggleStyle.SetBackground(null);
            if (info.Icon != null)
            {
                GUI.Box(new Rect(rect.x + 10, rect.y, 20, rect.height), info.Icon, "None");
                GUI.Label(new Rect(rect.x + 30, rect.y, rect.width - 30, rect.height), info.Text, TmpGroupToggleStyle.UnityStyle);
            }
            else
            {
                GUI.Label(new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height), info.Text, TmpGroupToggleStyle.UnityStyle);
            }
        }

        public void ActiveIndex(int i)
        {
            if (i >= 0 && i < mItems.Count)
                CurIndex = i;
        }

        public void RemoveIndex(int i)
        {
            if (i >= 0 && i < mItems.Count)
                mItems.RemoveAt(i);
        }

        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);

            if (!evt.handled
                && WorldArea.Contains(evt.MousePos))
            {
                Vector2 mousePos = evt.MousePos - WorldArea.position;
                float itemWidth = Math.Min(ItemMaxWidth, this.WorldArea.width / mItems.Count);
                int clickIndex = (int)(mousePos.x / itemWidth);
                if (clickIndex >=0 && clickIndex < mItems.Count)
                {
                    if (CurIndex != clickIndex)
                    {
                        if (onSelect != null)
                        {
                            onSelect(this, clickIndex);
                            CurIndex = clickIndex;
                        }
                    }
                }
            }
        }
    }



    public class UITabPanel : UIContainer
    {
        UIGroupToggle mTops;
        List<UINode> mPages;
        int mCurPage = -1;

        public override void Init()
        {
            Init_UIContainer(LayoutType.Vertical);
            mTops = this.CreateNode<UIGroupToggle>(LayerIndex.Normal);
            mTops.FixHeight(25.0f);
            mTops.onSelect = OnSelectIndex;
            mPages = new List<UINode>();
        }

        public void AddPage(string title, UINode page, bool active = true)
        {
            mTops.Add(title);
            mPages.Add(page);
            if (mCurPage == -1 || active)
                ActivePage(mPages.Count - 1);
        }

        public T AddPage<T>(string title, bool active = true) where T : UINode, new ()
        {
            T ret = CreateNode<T>();
            AddPage(title, ret, active);
            return ret;
        }

        public void ActivePage(int index, bool forceRefresh = false)
        {
            if (index >= 0 && index < mPages.Count)
            {
                mTops.ActiveIndex(index);
                if (forceRefresh || index != mCurPage)
                {
                    if (mCurPage != -1)
                        mPages[mCurPage].Active = false;
                    mPages[index].Active = true;
                    UILayout_Orien layout = mLayers[(int)LayerIndex.Normal].GetLayout() as UILayout_Orien;
                    layout.SetDirty();
                }

                mCurPage = index;
            }
        }

        public void RemovePage(int i)
        {
            if (i >= 0 && i < mPages.Count)
            {
                mTops.RemoveIndex(i);
                mPages.RemoveAt(i);

                if (i == mCurPage)
                {
                    if (mCurPage == mPages.Count - 1)
                        ActivePage(mCurPage - 1, true);
                    else
                        ActivePage(mCurPage, true);
                }
            }
        }

        void OnSelectIndex(UIGroupToggle ui, int index)
        {
            ActivePage(index);
        }
    }
}