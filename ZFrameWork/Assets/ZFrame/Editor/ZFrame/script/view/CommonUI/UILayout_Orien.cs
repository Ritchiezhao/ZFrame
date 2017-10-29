
using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace ZFEditor
{
    public class UILayout_Orien : UILayout
    {
        const float ResizeBarRadius = 6.0f;

        bool mDirty = true;
        bool mDraging = false;
        int mDragingItemIndex = -1;

        bool vertical = false;


        // SetDirty
        public void SetDirty()
        {
            mDirty = true;
        }

        public UILayout_Orien(bool vertical)
        {
            this.vertical = vertical;
        }

        #region UILayout Interface
        // SetSize
        public override void SetSize(Vector2 size)
        {
            if (mSize.x != size.x
                || mSize.y != size.y)
            {
                SetDirty();
            }
            base.SetSize(size);
        }

        // AddItem
        public override void AddItem(LayoutInfo item)
        {
            base.AddItem(item);
            SetDirty();
        }

        // RemoveItem
        public override void RemoveItem(LayoutInfo item)
        {
            base.RemoveItem(item);
            SetDirty();
        }

        float Orien(Vector2 v)
        {
            if (vertical)
                return v.y;
            else
                return v.x;
        }

        float NotOrienVal(ref Vector2 v)
        {
            if (!vertical)
                return v.y;
            else
                return v.x;
        }

        void OrienSet(ref Vector2 v, float val)
        {
            if (vertical)
                v.y = val;
            else
                v.x = val;
        }

        void SetNotOrienVal(ref Vector2 v, float val)
        {
            if (!vertical)
                v.y = val;
            else
                v.x = val;
        }

        // Update
        public override void Update()
        {
            if (mDirty)
            {
                float contentLen = 0.0f;
                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                    {
                        mItems[i].CorrectSize();
                        SetNotOrienVal(ref mItems[i].LayoutSize, Math.Min(NotOrienVal(ref mItems[i].MaxSize), NotOrienVal(ref mSize)));
                        contentLen += Orien(mItems[i].LayoutSize);
                    }
                }

                if (contentLen != 0)
                    UpdateContentLen(Orien(mSize) - contentLen);
            }
            
            // change specified items size //
            for (int i = 0; i < mItems.Count; ++i)
            {
                if (TryChangeItemLen(i))
                    mDirty = true;
            }

            // x, y, another border //
            if (mDirty)
            {
                float curpos = 0.0f;
                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                    {
                        OrienSet(ref mItems[i].LayoutPos, curpos);
                        SetNotOrienVal(ref mItems[i].LayoutPos, 0.0f);
                        curpos += Orien(mItems[i].LayoutSize) + 1;
                    }
                }
            }

            // finish //
            for (int i = 0; i < mItems.Count; ++i)
            {
                mItems[i].UpdateWorldArea(new Rect(mItems[i].LayoutPos + mWorldPos, mItems[i].LayoutSize));
            }

            // set mouse cursor //
            if (!mDraging && this.EnableMouseResizing)
            {
                for (int i = 1; i < mItems.Count; ++i)
                {
                    if (mItems[i - 1].Active
                        && mItems[i - 1].MouseResizing)
                    {
                        if (vertical)
                        {
                            EditorGUIUtility.AddCursorRect(
                                new UnityEngine.Rect(mItems[i].WorldArea.x, mItems[i].WorldArea.y - ResizeBarRadius,
                                                    mItems[i].WorldArea.width, 2 * ResizeBarRadius),
                                MouseCursor.ResizeVertical);
                        }
                        else
                        {
                            EditorGUIUtility.AddCursorRect(
                                new UnityEngine.Rect(mItems[i].WorldArea.x - ResizeBarRadius, mItems[i].WorldArea.y,
                                                    2 * ResizeBarRadius, mItems[i].WorldArea.height),
                                MouseCursor.ResizeHorizontal);
                        }
                    }
                }
            }

            mDirty = false;
        }


        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);

            Vector2 localMouse = evt.MousePos - mWorldPos;

            if (evt.type == EventType.MouseDown && evt.MouseLeft)
            {
                if (this.EnableMouseResizing
                    && localMouse.x > 0 && localMouse.x < mSize.x
                    && localMouse.y > 0 && localMouse.y < mSize.y)
                {
                    for (int i = 1; i < mItems.Count; ++i)
                    {
                        if (mItems[i - 1].Active
                            && mItems[i - 1].MouseResizing
                            && Math.Abs(Orien(localMouse) - Orien(mItems[i].LayoutPos)) <= ResizeBarRadius)
                        {
                            mDraging = true;
                            mDragingItemIndex = i - 1;
                            if (this.vertical)
                                Window.SetCursor(MouseCursor.ResizeVertical);
                            else
                                Window.SetCursor(MouseCursor.ResizeHorizontal);
                            break;
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (mDraging == true)
                {
                    Window.SetCursor(MouseCursor.Arrow);
                    mDraging = false;
                }
            }
            else if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag && evt.MouseLeft)
            {
                if (mDraging == true && mDragingItemIndex >= 0 && mDragingItemIndex < mItems.Count)
                {
                    OrienSet(ref mItems[mDragingItemIndex].TryChangeSize, Orien(evt.MousePosDelta));
                }
            }
        }

        public override Vector2 ContentMinSize()
        {
            Vector2 ret = Vector2.zero;
            for (int i = 0; i < mItems.Count; ++i)
            {
                if (mItems[i].Active)
                {
                    if (vertical)
                    {
                        ret.y += mItems[i].MinSize.y;
                        ret.x = Math.Max(ret.x, mItems[i].MinSize.x);
                    }
                    else
                    {
                        ret.x += mItems[i].MinSize.x;
                        ret.y = Math.Max(ret.y, mItems[i].MinSize.y);
                    }
                }
            }
            return ret;
        }

        // TryChangeItemLen
        bool TryChangeItemLen(int itemIndex)
        {
            LayoutInfo item = mItems[itemIndex];
            float delta = Orien(item.TryChangeSize);
            OrienSet(ref item.TryChangeSize, 0);

            if (delta > 0.1f)
            {
                // item self limit //
                delta = GetCanExpandSize(item, delta);
                if (delta <= 0)
                    return false;

                // items after limit //
                float realDelta = 0.0f;
                for (int i = itemIndex + 1; i < mItems.Count; ++i)
                {
                    if (!mItems[i].Active)
                        continue;
                    float curChange = GetCanShrinkSize(mItems[i], delta);
                    if (curChange > 0.0f)
                    {
                        realDelta += curChange;
                        OrienSet(ref mItems[i].LayoutPos, Orien(mItems[i].LayoutPos) + curChange);
                        OrienSet(ref mItems[i].LayoutSize, Orien(mItems[i].LayoutSize) - curChange);
                    }

                    if (realDelta >= (delta - 0.1f))
                        break;
                }

                OrienSet(ref item.LayoutSize, Orien(item.LayoutSize) + realDelta);
                return true;
            }
            else if (delta < 0.1f)
            {
                delta = GetCanShrinkSize(item, -delta);
                if (delta <= 0)
                    return false;

                // items after limit //
                float realDelta = 0.0f;
                for (int i = itemIndex + 1; i < mItems.Count; ++i)
                {
                    if (!mItems[i].Active)
                        continue;
                    float curChange = GetCanExpandSize(mItems[i], delta);
                    if (curChange > 0.0f)
                    {
                        realDelta += curChange;
                        OrienSet(ref mItems[i].LayoutPos, Orien(mItems[i].LayoutPos) - curChange);
                        OrienSet(ref mItems[i].LayoutSize, Orien(mItems[i].LayoutSize) + curChange);
                    }

                    if (realDelta >= (delta - 0.1f))
                        break;
                }

                OrienSet(ref item.LayoutSize, Orien(item.LayoutSize) - realDelta);
                return true;
            }

            return false;
        }
        #endregion

        float GetCanExpandSize(LayoutInfo item, float wantedSize)
        {
            if (Orien(item.MaxSize) <= Orien(item.MinSize))
                return 0.0f;
            return Math.Min(Orien(item.MaxSize) - Orien(item.LayoutSize), wantedSize);
        }

        float GetCanShrinkSize(LayoutInfo item, float wantedSize)
        {
            if (Orien(item.MaxSize) <= Orien(item.MinSize))
                return 0.0f;
            return Math.Min(Orien(item.LayoutSize) - Orien(item.MinSize), wantedSize);
        }

        void UpdateContentLen(float delta)
        {
            if (delta > 0.5f)
            {
                float totalCanExpand = 0.0f;
                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                        totalCanExpand += GetCanExpandSize(mItems[i], delta);
                }

                float realExpand = Math.Min(totalCanExpand, delta);
                if (realExpand < 0.01f)
                    return;

                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                        OrienSet(ref mItems[i].LayoutSize, Orien(mItems[i].LayoutSize) + (GetCanExpandSize(mItems[i], delta) / totalCanExpand * realExpand));
                }
            }
            else if (delta < -0.5f)
            {
                delta = -delta;
                float totalCanShrink = 0.0f;
                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                        totalCanShrink += GetCanShrinkSize(mItems[i], delta);
                }

                float realShrink = Math.Min(totalCanShrink, delta);
                if (realShrink < 0.01f)
                    return;

                for (int i = 0; i < mItems.Count; ++i)
                {
                    if (mItems[i].Active)
                        OrienSet(ref mItems[i].LayoutSize, Orien(mItems[i].LayoutSize) - (GetCanShrinkSize(mItems[i], delta) / totalCanShrink * realShrink));
                }
            }
        }
    }
}