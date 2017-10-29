using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{

    // multi //
    public class DataGraph
    {
        class Item
        {
            public GObject obj;
            public UIGraphNode node;
            public UILine line;
        }

        UIGraph mUIGraph;

        Item mCoreItem;
        Item mParentItem;
        List<Item> mRefItems;

        List<Item> mItems = new List<Item>();


        public void Init(UIContainer parent)
        {
            mUIGraph = parent.CreateNode<UIGraph>(LayerIndex.Normal);
        }

        public void SetCoreObj(GObject obj)
        {
            mUIGraph.Clear();

            mCoreItem = new Item();
            mCoreItem.obj = obj;
            mCoreItem.node = mUIGraph.CreateNode<UIGraphNode>();
            mCoreItem.node.SetData(mCoreItem.obj);
            mCoreItem.node.LayoutPos = new Vector2(0, 0);

			GObject parent = GDataManager.Instance.GetObj(obj.Parent);
            if (parent != null)
            {
                mParentItem = new Item();
                mParentItem.obj = parent;
                
                // node
                mParentItem.node = mUIGraph.CreateNode<UIGraphNode>();
                mParentItem.node.SetData(mParentItem.obj);
                mParentItem.node.LayoutPos = new Vector2(0, -150);

                // line 
                mParentItem.line = mUIGraph.CreateNode<UILine>(LayerIndex.Upper);
                mParentItem.line.SetBeginPoint(new UILine.Point(mCoreItem.node, 0.5f, 0.0f));
                mParentItem.line.SetEndPoint(new UILine.Point(mParentItem.node, 0.5f, 1.0f));
                mParentItem.line.SetBezier(Vector2.down, Vector2.up);
                mParentItem.line.SetColor(Color.yellow);
            }

            List<GObject> refs = new List<GObject>();
            AddRefObj(obj, ref refs);

            mRefItems = new List<Item>();
            for (int i = 0; i < refs.Count; ++i)
            {
                Item item = new Item();
                item.obj = refs[i];

                // node
                item.node = mUIGraph.CreateNode<UIGraphNode>();
                item.node.SetData(item.obj);
                item.node.Title = refs[i].ID;
                item.node.LayoutPos = new Vector2(300 * (i%2 == 1 ? -1 : 1), i * 50 -125); // TODO: 坐标计算 //

                // line 
                item.line = mUIGraph.CreateNode<UILine>(LayerIndex.Upper);
                item.line.SetColor(Color.green);
                item.line.SetBeginPoint(new UILine.Point(mCoreItem.node, 1.0f, 0.5f));
                item.line.SetEndPoint(new UILine.Point(item.node, 0.0f, 0.5f));
                if (i%2 == 1)
                {
                    item.line.SetBeginPoint(new UILine.Point(mCoreItem.node, 0.0f, 0.5f));
                    item.line.SetEndPoint(new UILine.Point(item.node, 1.0f, 0.5f));
                    item.line.SetBezier(Vector2.left, Vector2.right);
                }
                else
                {
                    item.line.SetBeginPoint(new UILine.Point(mCoreItem.node, 1.0f, 0.5f));
                    item.line.SetEndPoint(new UILine.Point(item.node, 0.0f, 0.5f));
                    item.line.SetBezier(Vector2.right, Vector2.left);
                }

                mRefItems.Add(item);
            }

            mUIGraph.Update();

            FocusCoreItem();
        }

        void AddRefObj(GData data, ref List<GObject> objList)
        {
            if (data == null)
                return;

            if (data.Type.Equals(GType.TID))
            {
				GObject obj = GDataManager.Instance.GetObj(data.TID);
                if (obj != null)
                {
                    objList.Add(obj);
                }
            }
            else if (data.Type.Equals(GType.Array))
            {
                for (int i = 0; i < data.Count; ++i)
                    AddRefObj(data.GetArrayIndex(i), ref objList);
            }
            else
            {
                GType tp = GTypeManager.Instance.GetType(data.Type);

                if (tp.IsStruct())
                {
                    GTypeStruct structTp = tp as GTypeStruct;

                    for (int i = 0; i < structTp.FieldCount; ++i)
                        AddRefObj(data.GetField(structTp.GetField(i).Name), ref objList);
                }
            }
        }
        
        void FocusCoreItem()
        {
            mUIGraph.FocusPos(mCoreItem.node.LayoutPos + mCoreItem.node.LayoutSize/2 + new Vector2(0, 80));
        }

        public void AddObj(GObject obj, Vector2 worldPos)
        {

        }
          
    }
}











