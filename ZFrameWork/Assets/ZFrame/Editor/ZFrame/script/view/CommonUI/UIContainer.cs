
using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace ZFEditor
{
    public class UILayer : UINode
    {
        protected UILayout mLayout = null;
        protected List<UINode> mNodes = new List<UINode>();

        Vector2 mContentOffset = Vector2.zero;
        public bool EnableClip = false;

        public UILayout GetLayout()
        {
            return mLayout;
        }

        public void SetLayout(LayoutType layout)
        {
            mLayout = UILayout.Create(layout);
            if (mLayout != null)
                mLayout.Window = this.Window;
        }

        public void EnableMouseResize(bool enalbe)
        {
            if (mLayout != null)
                mLayout.EnableMouseResizing = enalbe;
        }

        // 设置子节点布局区域，坐标相对于UIContainer //
        public void SetContentOffset(Vector2 offset)
        {
            this.mContentOffset = offset;
        }
        
        public virtual void AddNode(UINode node)
        {
            node.Parent = this;
            mNodes.Add(node);
            mLayout.AddItem(node);
        }

        public virtual void Clear()
        {
            mNodes.Clear();
            mLayout.Clear();
        }

        public sealed override void Update()
        {
            base.Update();

            mLayout.SetWorldPos(WorldArea.min + mContentOffset);
            mLayout.SetSize(WorldArea.size - mContentOffset);
            mLayout.Update();

            for (int i = 0; i < mNodes.Count; ++i)
            {
                mNodes[i].Update();
            }
        }

        public override void Draw(Rect clipArea)
        {
            if (EnableClip)
            {
                Rect relative = GetRelative(clipArea);
                if (clipArea.Overlaps(relative))
                {
                    GUI.BeginGroup(relative);
                    for (int i = 0; i < mNodes.Count; ++i)
                    {
                        if (mNodes[i].Active && mNodes[i].Visible)
                            mNodes[i].Draw(WorldArea);
                    }
                    GUI.EndGroup();
                }
            }
            else
            {
                for (int i = 0; i < mNodes.Count; ++i)
                {
                    if (mNodes[i].Active && mNodes[i].Visible)
                        mNodes[i].Draw(clipArea);
                }
            }
        }
        
        public override void HandleEvents(InputEvent evt)
        {
            for (int i = mNodes.Count - 1; i >= 0; --i)
            {
                if (mNodes[i].Active && mNodes[i].Visible)
                    mNodes[i].HandleEvents(evt);
            }

            mLayout.HandleEvents(evt);
        }

        public Vector2 GetContentMinSize()
        {
            return mLayout.ContentMinSize();
        }
    }


    // UI层级定义，每个Container包含若干Layer，枚举值需要从0开始递增 //
    public enum LayerIndex
    {
        Lower = 0,
        Normal = 1,
        Upper = 2
    }


    public class UIContainer : UINode
    {
        protected static UnityGUIStyleWrapper TmpContainerStyle = new UnityGUIStyleWrapper("Default_Container");

        protected UILayer[] mLayers;
        protected LayoutType mLayoutType;

        public Texture2D Background;
        public RectOffset BackgroundBorder = new RectOffset(8, 8, 8, 8);

        // 让container自动扩展到 内容的最小size //
        public bool AutoFixContentMinWidth = false;
        public bool AutoFixContentMinHeight = false;

        bool mEnableClip = false;

        protected void EnsureLayer(LayerIndex index)
        {
            int i = (int)index;
            if (mLayers[i] == null)
            {
                mLayers[i] = Window.CreateUI<UILayer>();
                mLayers[i].SetLayout(mLayoutType);
                mLayers[i].Parent = this;
                mLayers[i].EnableClip = mEnableClip;
            }
        }

        public void Init_UIContainer(LayoutType layout)
        {
            mLayoutType = layout;
            mLayers = new UILayer[Enum.GetNames(typeof(LayerIndex)).Length];
            EnsureLayer(LayerIndex.Normal);
        }

        public void EnableMouseResize(bool enable)
        {
            for (int i = 0; i < mLayers.Length; ++i)
                if (mLayers[i] != null)
                    mLayers[i].EnableMouseResize(enable);
        }

        public void EnableMouseResize(bool enable, LayerIndex layerIndex)
        {
            if ((int)layerIndex < mLayers.Length && mLayers[(int)layerIndex] != null)
                mLayers[(int)layerIndex].EnableMouseResize(enable);
        }

        public void EnalbeClip(bool enable)
        {
            mEnableClip = enable;
            for (int i = 0; i < mLayers.Length; ++i)
                if (mLayers[i] != null)
                    mLayers[i].EnableClip = enable;
        }

        public void EnalbeClip(bool enable, LayerIndex layerIndex)
        {
            if ((int)layerIndex < mLayers.Length && mLayers[(int)layerIndex] != null)
                mLayers[(int)layerIndex].EnableClip = enable;
        }

        // 设置子节点布局区域，坐标相对于UIContainer //
        public void SetContentOffset(Vector2 offset)
        {
            for (int i = 0; i < mLayers.Length; ++i)
                if (mLayers[i] != null)
                    mLayers[i].SetContentOffset(offset);
        }

        // CreateNode
        public T CreateNode<T>(LayerIndex layer = LayerIndex.Normal) where T : UINode, new()
        {
            T ret = this.Window.CreateUI<T>();
            AddNode(ret, layer);
            return ret;
        }

        public virtual void AddNode(UINode node, LayerIndex layer = LayerIndex.Normal)
        {
            if ((int)layer < mLayers.Length)
            {
                EnsureLayer(layer);
                mLayers[(int)layer].AddNode(node);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < mLayers.Length; ++i)
                if (mLayers[i] != null)
                    mLayers[i].Clear();
        }

        public void Clear(LayerIndex layer)
        {
            if ((int)layer < mLayers.Length)
                if (mLayers[(int)layer] != null)
                    mLayers[(int)layer].Clear();
        }
        
        public override void Update()
        {
            if (AutoFixContentMinWidth || AutoFixContentMinHeight)
            {
                Vector2 minSize = Vector2.zero;
                for (int i = 0; i < mLayers.Length; ++i)
                {
                    if (mLayers[i] != null)
                    {
                        Vector2 vec = mLayers[i].GetContentMinSize();
                        minSize.x = Math.Max(minSize.x, vec.x);
                        minSize.y = Math.Max(minSize.y, vec.y);
                    }
                }

                if (AutoFixContentMinWidth)
                {
                    this.SetContentMinWidth(minSize.x);
                    this.LayoutSize.x = this.MinSize.x;
                }

                if (AutoFixContentMinHeight)
                {
                    this.SetContentMinHeight(minSize.y);
                    this.LayoutSize.y = this.MinSize.y;
                }
            }

            base.Update();
            if (mLayers == null)
            {
                Debug.LogError(this.GetType().ToString() + "  didn't initialize");
                return;
            }

            for (int i = 0; i < mLayers.Length; ++i)
            {
                if (mLayers[i] != null)
                {
                    mLayers[i].UpdateWorldArea(WorldArea);
                    mLayers[i].Update();
                }
            }
        }
        
        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            Rect relative = GetRelative(clipArea);

            if (Background != null)
            {
                TmpContainerStyle.SetBackground(Background);
                TmpContainerStyle.SetBorder(BackgroundBorder);

                GUI.Box(relative, "", TmpContainerStyle.UnityStyle);
            }

            //GUI.BeginGroup(relative);
            for (int i = 0; i < mLayers.Length; ++i)
            {
                if (mLayers[i] != null)
                {
                    mLayers[i].Draw(clipArea);
                }
            }
            //GUI.EndGroup();
        }


        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);

            for (int i = 0; i < mLayers.Length; ++i)
                if (mLayers[i] != null)
                    mLayers[i].HandleEvents(evt);
        }
        
    }
}