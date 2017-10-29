using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class DefaultDragger : EventHandler
    {
        public int mMouseKey;
        public UINode mMoveNode = null;

        public DefaultDragger(UINode node, int mouseKey, UINode moveNode = null)
            :base(node)
        {
            mMouseKey = mouseKey;
            mMoveNode = moveNode;
        }

        bool dragging = false;
        public override void HandleEvents(InputEvent evt)
        {
            base.HandleEvents(evt);
            if (evt.type == EventType.MouseDown && evt.MouseKey == mMouseKey)
            {
                if (mNode.WorldArea.Contains(evt.MousePos))
                    dragging = true;
            }
            else if (evt.type == EventType.MouseUp && evt.MouseKey == mMouseKey)
            {
                dragging = false;
            }
            else if (evt.type == EventType.MouseMove || evt.type == EventType.MouseDrag && evt.MouseKey == mMouseKey)
            {
                if (dragging)
                {
                    if (mMoveNode != null)
                        mMoveNode.LayoutPos += evt.MousePosDelta;
                    else
                        mNode.LayoutPos += evt.MousePosDelta;
                }
            }
        }

    }
}