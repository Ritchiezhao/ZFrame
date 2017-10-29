using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{

    public class EventHandler
    {
        protected UINode mNode;

        public EventHandler(UINode node)
        {
            mNode = node;
        }

        public virtual void HandleEvents(InputEvent evt)  { }
    }
}
