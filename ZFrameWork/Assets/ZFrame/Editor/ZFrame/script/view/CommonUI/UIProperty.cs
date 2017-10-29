using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class UIPropertyItem : UIContainer
    {
        public static UIPropertyItem CreateByType(UIContainer container, string type, UIProperty view)
        {
            UIPropertyItem ret = null;
            if (type.Equals(GType.Bool)
                || type.Equals(GType.Int)
                || type.Equals(GType.Float)
                || type.Equals(GType.String)
                || type.Equals(GType.TID))
            {
                ret = container.CreateNode<UIPropertyItem_Simple>();
            }
            else if (type.Equals(GType.Array))
            {
                ret = container.CreateNode<UIPropertyItem_Array>();
            }
            else
            {
                GType tp = GTypeManager.Instance.GetType(type);
                if (tp.IsEnum() || tp.IsMacro())
                {
                    ret = container.CreateNode<UIPropertyItem_Simple>();
                }
                else if (tp.IsStruct())
                {
                    ret = container.CreateNode<UIPropertyItem_Object>();
                }
            }

            if (ret != null)
                ret.mView = view;
            else
                GLog.LogError("ret is null. type name: " + type);

            return ret;
        }
        
        protected float mOffsetX = 0.0f;
        protected GData mData;
        protected string mName;
        protected UIProperty mView;

        public float PropHeight { get; protected set; }

        public virtual void SetData(string name, GData data, float offsetX)
        {
            mOffsetX = offsetX;
            mName = name;
            mData = data;
        }

        bool mouseDownOnThis = false;
        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseDown
                && !evt.handled
                && WorldArea.Contains(evt.MousePos))
            {
                mouseDownOnThis = true;
                evt.handled = true;
            }
            else if (evt.type == EventType.MouseUp
                && !evt.handled)
            {
                if (WorldArea.Contains(evt.MousePos)
                    && mouseDownOnThis)
                {
                    Window.FocusMgr.TakeFocus(this, !evt.Ctrl);
                    evt.handled = true;
                }
                mouseDownOnThis = false;
            }
        }

        public override void UpdateFoucs(FocusState state)
        {
            base.UpdateFoucs(state);
            if (state == FocusState.Current)
            {
                this.Background = ResCache.GetTexture("/Textures/Default_Background_Focus.png");
            }
            else if (state == FocusState.InSelection)
            {
                this.Background = ResCache.GetTexture("/Textures/Default_Background_Focus.png");
            }
            else if (state == FocusState.None)
            {
                this.Background = null;
            }
        }
    }

    public class UIPropertyItem_Simple : UIPropertyItem
    {
        UIContainer mLeftPart;
        UIBlank mBlank;
        UILabel mUIName;

        UIContainer mRightPart;
        UITextField mUIField;
        UIToggle mToggle;
        UIDropDown mUIEnum;
        UILabel mUILabel;


        public override void Init()
        {
            base.Init();
            Init_UIContainer(LayoutType.Horizontal);
            
            this.Background = ResCache.GetTexture("/Textures/Pure_Gray.psd");

            // left part //
            mLeftPart = CreateNode<UIContainer>(LayerIndex.Normal);
            mLeftPart.Init_UIContainer(LayoutType.Horizontal);
            mLeftPart.FixHeight(20.0f);

            mBlank = mLeftPart.CreateNode<UIBlank>();

            mUIName = mLeftPart.CreateNode<UILabel>();
            mUIName.FixHeight(20);

            // right part //
            mRightPart = CreateNode<UIContainer>(LayerIndex.Normal);
            mRightPart.Init_UIContainer(LayoutType.Horizontal);
            mRightPart.FixHeight(20.0f);
        }


        public override void SetData(string name, GData data, float offsetX)
        {
            base.SetData(name, data, offsetX);
            // clear //
            this.mRightPart.Clear();

            mBlank.FixWidth(offsetX);
            mUIName.Text = name;

            if (mData.Type.Equals(GType.Bool))
            {
                mToggle = mRightPart.CreateNode<UIToggle>();
                mToggle.onClick = OnToggle;
            }
            else if (mData.Type.Equals(GType.Int)
                    || mData.Type.Equals(GType.Float)
                    || mData.Type.Equals(GType.String))
            {
                mUIField = mRightPart.CreateNode<UITextField>();
                mUIField.onTextChange = OnEdit;
                mUIField.Delayed = true;

                if (mData.Type.Equals(GType.Int))
                    mUIField.Text = mData.Int.ToString();
                else if (mData.Type.Equals(GType.Float))
                    mUIField.Text = mData.Float.ToString();
                else if (mData.Type.Equals(GType.String))
                    mUIField.Text = mData.String;
            }
            else if (mData.Type.Equals(GType.TID))
            {
                mUILabel = mRightPart.CreateNode<UILabel>();
                mUILabel.Text = mData.TID;
            }
            else
            {
                GType tp = GTypeManager.Instance.GetType(data.Type);
                if (tp != null && tp.IsEnum())
                {
                    GTypeEnum enumTp = tp as GTypeEnum;

                    mUIEnum = mRightPart.CreateNode<UIDropDown>();
                    mUIEnum.Options = enumTp.Fields;
                    mUIEnum.Selected = (int)enumTp.GetVal(data.Enum);
                    mUIEnum.onChange = OnChangeEnum;
                }
                else
                {
                    GLog.LogError("Unknown type: " + mData.Type);
                }
            }

            // height //
            this.FixHeight(20.0f);
            PropHeight = 20.0f;
        }

        void OnToggle(UIToggle tog)
        {
            mData.Bool = tog.Toggle;
        }

        void OnEdit(UITextField label, string oldText)
        {
            try
            {
                if (mData.Type.Equals(GType.Int))
                    mData.Int = int.Parse(label.Text);
                else if (mData.Type.Equals(GType.Float))
                    mData.Float = float.Parse(label.Text);
                else if (mData.Type.Equals(GType.String))
                    mData.String = label.Text;
                // todo validate //
            }
            catch (System.Exception e)
            {
                // revert text //
                label.Text = oldText;
            }
        }

        void OnChangeEnum(UIDropDown ui, int selected)
        {
			mData.Enum = mData.GetType<GTypeEnum>().Fields[selected];
        }

    }
    
    public class UIPropertyItem_Array : UIPropertyItem
    {
        UIContainer mTitleLine;
        UIBlank mBlank;
        UIFoldout mUIFoldout;
        UILabel mUITitle;

        List<UIPropertyItem> subItems = new List<UIPropertyItem>();

        public override void Init()
        {
            base.Init();
            Init_UIContainer(LayoutType.Vertical);

            mTitleLine = this.CreateNode<UIContainer>(LayerIndex.Normal);
            mTitleLine.Init_UIContainer(LayoutType.Horizontal);
            mTitleLine.FixHeight(20.0f);

            mBlank = mTitleLine.CreateNode<UIBlank>();

            mUIFoldout = mTitleLine.CreateNode<UIFoldout>();
            mUIFoldout.FixWidth(20.0f);
            mUIFoldout.onClick = OnFoldoutClick;

            mUITitle = mTitleLine.CreateNode<UILabel>();
        }

        public override void SetData(string name, GData data, float offsetX)
        {
            base.SetData(name, data, offsetX);
            // clear //
            this.Clear();
            subItems.Clear();
            this.AddNode(mTitleLine);
            PropHeight = 0.0f;

            // title //
            mBlank.FixWidth(offsetX);
            mUIFoldout.isFoldOut = data.display_foldout;
            mUITitle.Text = name + "  " + "Len: " + data.Count;
            PropHeight += 20.0f;

            // sub items //
            if (mData.display_foldout)
            {
                int count = data.Count;
                for (int i = 0; i < count; ++i)
                {
                    GData subData = data.GetArrayIndex(i);
                    UIPropertyItem subItem = UIPropertyItem.CreateByType(this, subData.Type, mView);
                    subItems.Add(subItem);
                    subItem.SetData(subData.Type, subData, this.mOffsetX + 10.0f);
                    PropHeight += subItem.PropHeight;
                }
            }

            // height //
            this.FixHeight(PropHeight);
        }

        public override void Update()
        {
            base.Update();
        }

        void OnFoldoutClick(UIFoldout btn)
        {
            mData.display_foldout = !mData.display_foldout;
            mView.Refresh();
        }
    }



    public class UIPropertyItem_Object : UIPropertyItem
    {
        UIContainer mTitleLine;
        UIBlank mBlank;
        UIFoldout mUIFoldout;
        UILabel mUITitle;

        List<UIPropertyItem> subItems = new List<UIPropertyItem>();

        public override void Init()
        {
            base.Init();
            Init_UIContainer(LayoutType.Vertical);

            mTitleLine = this.CreateNode<UIContainer>(LayerIndex.Normal);
            mTitleLine.Init_UIContainer(LayoutType.Horizontal);
            mTitleLine.FixHeight(20.0f);

            mBlank = mTitleLine.CreateNode<UIBlank>();

            mUIFoldout = mTitleLine.CreateNode<UIFoldout>();
            mUIFoldout.FixWidth(20.0f);
            mUIFoldout.onClick = OnFoldoutClick;

            mUITitle = mTitleLine.CreateNode<UILabel>();
        }

        public override void SetData(string name, GData data, float offsetX)
        {
            base.SetData(name, data, offsetX);
            // clear //
            this.Clear();
            subItems.Clear();
            this.AddNode(mTitleLine);
            PropHeight = 0.0f;

            // title //
            mBlank.FixWidth(offsetX);
            mUIFoldout.isFoldOut = data.display_foldout;
            mUITitle.Text = name;
            PropHeight += 20.0f;

            // sub items //
            if (mData.display_foldout)
            {
                GType tp = GTypeManager.Instance.GetType(data.Type);
                if (tp != null && tp.IsStruct())
                {
                    GTypeStruct structTp = tp as GTypeStruct;
                    int count = structTp.FieldCount;
                    for (int i = 0; i < count; ++i)
                    {
                        GStructField field = structTp.GetField(i);
                        GData subData = data.GetFieldWithInheritance(field.Name);
                        if (subData != null)
                        {
                            UIPropertyItem subItem = UIPropertyItem.CreateByType(this, subData.Type, mView);
                            subItems.Add(subItem);
                            subItem.SetData(field.Name, subData, this.mOffsetX + 10.0f);
                            PropHeight += subItem.PropHeight;
                        }
                    }
                }
            }

            // height //
            this.FixHeight(PropHeight);
        }

        public override void Update()
        {
            base.Update();
        }

        void OnFoldoutClick(UIFoldout btn)
        {
            mData.display_foldout = !mData.display_foldout;
            mView.Refresh();
        }
    }

    public class UIProperty : UIContainer
    {
        GData mRootData;

        public override void Init()
        {
            base.Init();

            this.Init_UIContainer(LayoutType.Vertical);

            this.Background = ResCache.GetTexture("/Textures/Default_Border.psd");
        }

        public void SetData(GData data)
        {
            this.Clear();

            mRootData = data;

            GType tp = GTypeManager.Instance.GetType(data.Type);
            if (tp != null && tp.IsStruct())
            {
                GTypeStruct structTp = tp as GTypeStruct;
                int count = structTp.FieldCount;
                for (int i = 0; i < count; ++i)
                {
                    GStructField field = structTp.GetField(i);
                    GData subData = data.GetFieldWithInheritance(field.Name);
                    if (subData != null)
                    {
                        UIPropertyItem subItem = UIPropertyItem.CreateByType(this, subData.Type, this);
                        subItem.SetData(field.Name, subData, 10.0f);
                    }
                }
            }
        }

        public void Refresh()
        {
            SetData(mRootData);
        }
    }
}









