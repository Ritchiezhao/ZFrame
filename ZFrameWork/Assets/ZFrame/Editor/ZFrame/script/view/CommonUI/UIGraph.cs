using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class UIGraphNode : UIContainer
    {
        public delegate void OnSelectedFunc(UIGraphNode node);

        UILabel mTitle;
        UIProperty mProperty;
        OnSelectedFunc OnSelected;

        public override void Init()
        {
            base.Init();

            Init_UIContainer(LayoutType.Vertical);

            
            this.AutoFixContentMinWidth = true;
            this.AutoFixContentMinHeight = true;

            Background = ResCache.GetTexture("/Textures/GraphNode_Background.psd");

            mTitle = this.CreateNode<UILabel>(LayerIndex.Normal);
            mTitle.Text = "None";
            mTitle.Alignment = TextAnchor.MiddleCenter;
            mTitle.Background = ResCache.GetTexture("/Textures/Default_Border.psd");
            mTitle.Font.size = 12;
            mTitle.Font.style = FontStyle.Bold;
            mTitle.Font.color = new Color(1f, 0.8f, 0.3f);
            mTitle.FixHeight(20);
            mTitle.FixWidth(180);
            mTitle.SetEventHandler(new DefaultDragger(mTitle, 0, this));

            mProperty = this.CreateNode<UIProperty>();
            mProperty.AutoFixContentMinHeight = true;
        }

        public string Title
        {
            get
            {
                return mTitle.Text;
            }
            set
            {
                mTitle.Text = value;
            }
        }

        public void SetData(GObject obj)
        {
            Title = obj.ID;
            mProperty.SetData(obj);

        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

        }
    }

    public class UIGraph : UIContainer
    {
        Vector2 mCurOffset = Vector2.zero;

        public override void Init()
        {
            base.Init();
            Init_UIContainer(LayoutType.Free);
            EnalbeClip(true);
        }


        // 将pos位置移动到视野中间，pos坐标和Graph的Node坐标同坐标系 //
        bool waitFocus = false;
        Vector2 focusPos;
        public void FocusPos(Vector2 pos)
        {
            waitFocus = true;
            focusPos = pos;
        }

        bool dragging = false;
        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseDown && evt.MouseMiddle)
            {
                if (this.WorldArea.Contains(evt.MousePos))
                    dragging = true;
            }
            else if (evt.type == EventType.MouseUp && evt.MouseMiddle)
            {
                dragging = false;
            }
            else if (evt.type == EventType.MouseMove || evt.type == EventType.MouseDrag && evt.MouseMiddle)
            {
                if (dragging)
                {
                    mCurOffset += evt.MousePosDelta;
                    this.SetContentOffset(mCurOffset);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (waitFocus == true)
            {
                waitFocus = false;
                mCurOffset = WorldArea.size / 2 - focusPos;
                SetContentOffset(mCurOffset);
            }
        }
    }
}

