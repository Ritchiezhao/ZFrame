using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


namespace ZFEditor
{
    public class InputEvent
    {
        public EventType type;

        public Vector2 MousePos;
        public Vector2 MousePosDelta;
        public KeyCode Key;

        public int MouseKey;
        public bool MouseLeft;
        public bool MouseMiddle;
        public bool MouseRight;

        public bool Ctrl;
        public bool Shift;
        public bool Alt;

        public bool handled = false;

        static Vector2 lastMousePos;
        public InputEvent(Event unityEvt)
        {
            this.type = unityEvt.type;

            MousePos = unityEvt.mousePosition;
            if (unityEvt.isMouse)
            {
                MousePosDelta = MousePos - lastMousePos;
                lastMousePos = unityEvt.mousePosition;
            }

            Key = unityEvt.keyCode;

            MouseKey = unityEvt.button;
            MouseLeft = (unityEvt.button == 0);
            MouseRight = (unityEvt.button == 1);
            MouseMiddle = (unityEvt.button == 2);

            Ctrl = unityEvt.control;
            Shift = unityEvt.shift;
            Alt = unityEvt.alt;
        }
    }

    public class InputManager
    {
        Queue<InputEvent> mEvents = new Queue<InputEvent>();

        public void Update()
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                case EventType.KeyUp:
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    mEvents.Enqueue(new InputEvent(Event.current));
                    break;
            }
        }

        public InputEvent PopEvent()
        {
            if (mEvents.Count > 0)
            {
                return mEvents.Dequeue();
            }
            return null;
        }
        
    }


}