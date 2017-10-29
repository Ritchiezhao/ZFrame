using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class ProjectView
    {
        class TVItem : UITreeView.Item
        {
            public TVItem(GType tp, Texture2D icon)
            {
                this.Mark = tp.Name;
                this.Icon = icon;
                this.Label = tp.Name;
            }

            public TVItem(GObject obj, Texture2D icon)
            {
                this.Mark = obj.ID;
                this.Icon = icon;
                this.Label = obj.ID;
            }

            public string Mark;
        }

        UIPanel mPanel;
        UITreeView mTreeView;
        List<UITreeView.Item> mTreeViewData = new List<UITreeView.Item>();

        Texture2D typeIcon;
        Texture2D dataIcon;

        public void Init(UIPanel panel)
        {
            mPanel = panel;

            mTreeView = mPanel.Content.CreateNode<UITreeView>();
            mTreeView.SetMinSize(new Vector2(80, 80));

            typeIcon = ResCache.GetTexture("/Textures/Icon_Type.png");
            dataIcon = ResCache.GetTexture("/Textures/Icon_Data.png");

            RefreshData();
        }

        public void RefreshData()
        {
            // refresh tree view //
            mTreeViewData.Clear();
            List<GType> types = GTypeManager.Instance.GetTypeList();
            for (int i = 0; i < types.Count; ++i)
            {
                if (types[i].IsClass())
                {
                    UITreeView.Item item = new TVItem(types[i], typeIcon);
                    item.onClick = OnTypeClick;
                    mTreeViewData.Add(item);

                    // data //
                    List<GObject> objs = GDataManager.Instance.GetObjsByType(types[i].Name);
                    if (objs != null)
                    {
                        for (int j = 0; j < objs.Count; ++j)
                        {
                            UITreeView.Item subItem = new TVItem(objs[j], dataIcon);
                            subItem.onClick = OnObjectClick;
                            item.AddItem(subItem);
                        }
                    }
                }
            }
            mTreeView.SetTree(mTreeViewData);
        }

        void OnTypeClick(UITreeView view, UITreeView.Item item)
        {
            UITreeView.Item.DefaultOnClick(view, item);

        }

        void OnObjectClick(UITreeView view, UITreeView.Item item)
        {
            UITreeView.Item.DefaultOnClick(view, item);
            // notify //
            TVItem tvItem = (TVItem)item;
            GObject selectedObj = GDataManager.Instance.GetObj(tvItem.Mark);
            MainWindow.Workspace.OpenEditor(selectedObj);
        }
    }
}