
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace ZFEditor
{
    public class UITreeView : UINode
    {
        public class Item
        {
            public delegate void OnClickFunc(UITreeView view, Item item);

            public static void DefaultOnClick(UITreeView view, Item item)
            {
                if (!Event.current.control)
                    view.ClearSelection();
                item.IsSelected = true;
            }

            public Texture2D Icon;
            public string Label;
            public bool Visible = true;

            public List<Item> SubItems = new List<Item>();
            public OnClickFunc onClick = DefaultOnClick;
            
            public void AddItem(Item item)
            {
                SubItems.Add(item);
            }

            public void Clear()
            {
                this.SubItems = new List<Item>();
            }

            public bool HasSubItems()
            {
                return SubItems != null && SubItems.Count > 0;
            }

            // states //
            public bool IsClosed = false;
            public bool IsSelected = false;

            // pos relative to TreeView Area //
            public Vector2 Pos = new Vector2(0, 0);
        }

        // ---------------------------------------
        List<Item> topItems;

        // config //
        float OpenCloseBtnWidth = 25;
        float IconWidth = 20;
        float itemHeight = 20;
        float indentSize = 17;
        string BackgroundUIStyle = "TreeView";
        string CloseBtnUIStyle = "TreeViewClose";
        string OpenBtnUIStyle = "TreeViewOpen";
        string IconUIStyle = "TreeViewIcon";
        string NodeUIStyle = "TreeViewNode";
        string LeafUIStyle = "TreeViewLeaf";
        string SelectedBkUIStyle = "TreeViewSelectedBk";
        string NotSelectedBkUIStyle = "TreeViewNotSelectedBk";

        public void SetTree(List<Item> topItems)
        {
            this.topItems = topItems;
        }

        public override void Update()
        {
            base.Update();
            float curY = 2.0f;
            for (int i = 0; i < topItems.Count; ++i)
            {
                curY = UpdateItemPos(new Vector2(0, curY), topItems[i]);
            }
        }
        
        // 递归计算坐标 //
        float UpdateItemPos(Vector2 pos, Item item)
        {
            item.Pos = pos;
            float curY = item.Pos.y + itemHeight;
            if (item.HasSubItems() && !item.IsClosed)
            {
                for (int i = 0; i < item.SubItems.Count; ++i)
                    curY = UpdateItemPos(new Vector2(pos.x + indentSize, curY), item.SubItems[i]);
            }
            return curY;
        }

        // ClearSelections //
        void ClearSelection_Helper(Item item)
        {
            item.IsSelected = false;
            for (int i = 0; i < item.SubItems.Count; ++i)
                ClearSelection_Helper(item.SubItems[i]);
        }

        void ClearSelection()
        {
            for (int i = 0; i < topItems.Count; ++i)
                ClearSelection_Helper(topItems[i]);
        }

        void DrawItem(Item item)
        {
            // draw background //
            if (item.IsSelected)
                GUI.Box(new Rect(0, item.Pos.y, this.WorldArea.width, itemHeight), "", (GUIStyle)SelectedBkUIStyle);

            float curX = item.Pos.x;

            // draw open/close btn //
            if (item.HasSubItems())
            {
                if (item.IsClosed)
                {
                    if (GUI.Button(new Rect(curX, item.Pos.y, OpenCloseBtnWidth, itemHeight), "▷", (GUIStyle)OpenBtnUIStyle))
                        item.IsClosed = false;
                }
                else
                {
                    if (GUI.Button(new Rect(curX, item.Pos.y, OpenCloseBtnWidth, itemHeight), "◢", (GUIStyle)CloseBtnUIStyle))
                        item.IsClosed = true;
                }
            }
            curX += OpenCloseBtnWidth;

            // draw item icon //
            if (item.Icon != null)
            {
                GUI.Box(new Rect(curX, item.Pos.y, IconWidth, item.Icon.height), item.Icon, (GUIStyle)IconUIStyle);
                curX += IconWidth;
            }

            // draw item label //
            string style = NodeUIStyle;
            if (!item.HasSubItems())
                style = LeafUIStyle;
            if (GUI.Button(new Rect(curX, item.Pos.y, WorldArea.width, itemHeight), item.Label, (GUIStyle)style))
            {
                if (item.onClick != null)
                    item.onClick(this, item);
            }

            // draw sub items //
            if (item.HasSubItems() && !item.IsClosed)
            {
                for (int i = 0; i < item.SubItems.Count; ++i)
                    DrawItem(item.SubItems[i]);
            }
        }

        public override void Draw(Rect clipArea)
        {
            base.Draw(clipArea);

            Rect relative = GetRelative(clipArea);
            if (topItems == null || topItems.Count == 0)
                return;

            GUI.BeginGroup(relative, (GUIStyle)BackgroundUIStyle);
            for (int i = 0; i < topItems.Count; ++i)
            {
                DrawItem(topItems[i]);
            }
            GUI.EndGroup();
        }
    }
}




