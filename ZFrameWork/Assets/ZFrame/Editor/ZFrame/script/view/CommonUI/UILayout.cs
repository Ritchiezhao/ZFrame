
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZFEditor
{
    /*
    public class Rect
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rect()
        { }

        public Rect(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public bool Contains(float x, float y)
        {
            return (x >= X && x < (X + Width) && y >= Y && y < (Y + Height));
        }

        public void Copy(Rect b)
        {
            this.X = b.X;
            this.Y = b.Y;
            this.Width = b.Width;
            this.Height = b.Height;
        }

        public UnityEngine.Rect ToUnityRect()
        {
            return new UnityEngine.Rect(X, Y, Width, Height);
        }
    }*/

    public class ExpandInfo
    {
        public float min = 8.0f;
        public float max = 100000.0f;
        //public float factor = 0.0f;
        public void SetFix(float fixValue)
        {
            this.min = fixValue;
            this.max = fixValue;
        }
    }
    
    public class LayoutInfo
    {
        public Vector2 LayoutPos = Vector2.zero;
        public Vector2 LayoutSize = new Vector2(10, 10);
        public Vector2 TryChangeSize = new Vector2(0, 0);

        public Rect WorldArea = new Rect();

        Vector2 _SelfMinSize = new Vector2(10, 10);
        public void SetMinSize(Vector2 size)
        {
            _SelfMinSize = size;
            UpdateMinSize();
        }

        protected Vector2 _ContentMinSize = Vector2.zero;
        protected void SetContentMinWidth(float width)
        {
            _ContentMinSize.x = width;
            UpdateMinSize();
        }
        protected void SetContentMinHeight(float height)
        {
            _ContentMinSize.y = height;
            UpdateMinSize();
        }

        void UpdateMinSize()
        {
            _FinalMinSize.x = Math.Max(_SelfMinSize.x, _ContentMinSize.x);
            _FinalMinSize.y = Math.Max(_SelfMinSize.y, _ContentMinSize.y);
        }

        Vector2 _FinalMinSize = new Vector2(10f, 10f);
        public Vector2 MinSize
        {
            get { return _FinalMinSize; }
        }

        public Vector2 MaxSize = new Vector2(100000.0f, 100000.0f);

        public bool Active = true;
        public bool MouseResizing = true;
        
        public void FixWidth(float val)
        {
            SetMinSize(new Vector2(val, this._SelfMinSize.y));
            this.MaxSize.x = val;
        }

        public void FixHeight(float val)
        {
            SetMinSize(new Vector2(this._SelfMinSize.x, val));
            this.MaxSize.y = val;
        }

        public void CorrectSize()
        {
            this.LayoutSize.x = Math.Min(this.LayoutSize.x, this.MaxSize.x);
            this.LayoutSize.y = Math.Min(this.LayoutSize.y, this.MaxSize.y);

            this.LayoutSize.x = Math.Max(this.LayoutSize.x, this.MinSize.x);
            this.LayoutSize.y = Math.Max(this.LayoutSize.y, this.MinSize.y);
        }

        public virtual void UpdateWorldArea(Rect worldArea)
        {
            this.WorldArea = worldArea;
        }
    }


    public enum LayoutType
    {
        Horizontal,
        Vertical,
        Free
    }

    public abstract class UILayout
    {
        // Factory Create //
        public static UILayout Create(LayoutType type)
        {
            switch (type)
            {
                case LayoutType.Horizontal:
                    return new UILayout_Orien(false);
                case LayoutType.Vertical:
                    return new UILayout_Orien(true);
                case LayoutType.Free:
                    return new UILayout_Free();
                default:
                    return null;
            }
        }

        protected Vector2 mWorldPos;
        protected Vector2 mSize;

        protected List<LayoutInfo> mItems = new List<LayoutInfo>();

        public bool EnableMouseResizing = false;

        
        public WindowBase Window
        {
            get; set;
        }
        
        public virtual void SetWorldPos(Vector2 pos)
        {
            mWorldPos = pos;
        }

        public virtual void SetSize(Vector2 size)
        {
            mSize = size;
            if (mSize.x <= 0.0f)
            {
                mSize.x = 0.0f;
                GLog.LogError("Size.x <= 0.0f");
            }
            if (mSize.y <= 0.0f)
            {
                mSize.y = 0.0f;
                GLog.LogError("Size.y <= 0.0f");
            }
        }
        
        public virtual void AddItem(LayoutInfo item)
        {
            mItems.Add(item);
        }

        public virtual void RemoveItem(LayoutInfo item)
        {
            mItems.Remove(item);
        }

        public virtual void Clear()
        {
            mItems.Clear();
        }

        public virtual void Update() { }

        public virtual void HandleEvents(InputEvent evt) { }

        public virtual Vector2 ContentMinSize()
        {
            return Vector2.zero;
        }
    }
}
