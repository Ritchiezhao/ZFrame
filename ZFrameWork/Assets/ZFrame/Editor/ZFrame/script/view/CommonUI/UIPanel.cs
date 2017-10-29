
using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace ZFEditor
{
    public class UIPanel : UIContainer
    {
        const string TitleStyle = "PanelTitle";
        const int TitleHeight = 27;

        UILabel mUITitle;
        string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                if (string.IsNullOrEmpty(_Title) && mUITitle != null)
                    mUITitle.Active = false;
                else
                    mUITitle.Text = _Title;
            }
        }

        public UIContainer Content;

        public void Init(string title, LayoutType innerLayout)
        {
            this.Init_UIContainer(LayoutType.Vertical);

            this.Background = ResCache.GetTexture("/Textures/Panel_Background.psd");
            this.BackgroundBorder.top = 30;

            mUITitle = CreateNode<UILabel>();
            mUITitle.Font.size = 14;
            //mUITitle.Font.color = new Color(1f, 0.8f, 0.3f);
            mUITitle.TextOffset = 5;
            mUITitle.FixHeight(TitleHeight);
            mUITitle.MouseResizing = false;

            Title = title;

            Content = CreateNode<UIContainer>();
            Content.Init_UIContainer(innerLayout);
        }
    }
}