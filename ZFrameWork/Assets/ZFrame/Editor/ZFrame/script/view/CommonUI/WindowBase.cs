using UnityEngine;
using UnityEditor;


namespace ZFEditor
{
    public class WindowBase : EditorWindow
    {
        protected GUISkin skin;
        protected UIContainer mainPanel;
        protected MouseCursor mCursor = MouseCursor.Arrow;

        double lastRepaintTime = 0.0f;
        bool isInited = false;

        public InputManager InputMgr = new InputManager();
        public FocusManager FocusMgr = new FocusManager();

        void Init()
        {
            isInited = true;
            curUINodeInstID = 1000;
            DoInit();
        }

        protected virtual void DoInit()
        {}

        void OnGUI()
        {
            // check compiling //
            if (EditorApplication.isCompiling)
            {
                ShowNotification(new GUIContent("Compiling Please Wait..."));
                isInited = false;
                return;
            }

            // init //
            if (!isInited)
                Init();

            // 防止显示不刷新 //
            if (EditorApplication.timeSinceStartup - lastRepaintTime > 0.02)
            {
                Repaint();
                lastRepaintTime = EditorApplication.timeSinceStartup;
            }

            // gui skin //
            GUI.skin = ResCache.GetSkin();

            // cursor //
            if (mCursor != MouseCursor.Arrow)
                EditorGUIUtility.AddCursorRect(new UnityEngine.Rect(0, 0, this.position.width, this.position.height), mCursor);

            // input events //
            InputMgr.Update();

            // main panel updates //
            if (mainPanel != null)
            {
                InputEvent evt;
                while ((evt = this.InputMgr.PopEvent()) != null)
                {
                    mainPanel.HandleEvents(evt);
                }

                mainPanel.UpdateWorldArea(new Rect(0, 0, this.position.width, this.position.height));
                mainPanel.Update();
                mainPanel.Draw(mainPanel.WorldArea);
            }
        }


        #region public methods
        int curUINodeInstID = 1000;
        public T CreateUI<T>() where T : UINode, new()
        {
            T ret = new T();
            ret.InstID = curUINodeInstID++;
            ret.Window = this;
            ret.Init();
            return ret;
        }

        public void SetSkin(GUISkin guiskin)
        {
            skin = guiskin;
        }

        public void SetMainPanel(UIPanel main)
        {
            this.mainPanel = main;
        }

        public void SetCursor(MouseCursor cursor)
        {
            mCursor = cursor;
        }

        #endregion
    }
}