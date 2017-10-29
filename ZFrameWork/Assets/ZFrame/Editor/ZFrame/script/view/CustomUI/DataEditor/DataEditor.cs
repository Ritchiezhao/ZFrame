using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class DataEditor : UIContainer
    {
        DataProperty Property;
        DataGraph Graph;
        GObject mObj;

        public override void Init()
        {
            base.Init();

            Init_UIContainer(LayoutType.Horizontal);
            this.EnableMouseResize(true);

            this.Background = ResCache.GetTexture("/Textures/Editor_Inner_Background.psd");

            UIContainer middle = CreateNode<UIContainer>();
            middle.Init_UIContainer(LayoutType.Horizontal);
            middle.SetMinSize(new Vector2(400, 500));
            middle.EnableMouseResize(true);

            UIContainer right = CreateNode<UIContainer>();
            right.Init_UIContainer(LayoutType.Vertical);
            right.SetMinSize(new Vector2(150, 10));
            right.MaxSize.x = 300;
            right.EnableMouseResize(true);
            right.Background = ResCache.GetTexture("/Textures/Editor_Inner_Background.psd");

            Property = new DataProperty();
            Property.Init(right);

            Graph = new DataGraph();
            Graph.Init(middle);
        }

        public void SetObj(GObject obj)
        {
            mObj = obj;
            Property.SetObject(obj);
            Graph.SetCoreObj(obj);
        }
    }
}









