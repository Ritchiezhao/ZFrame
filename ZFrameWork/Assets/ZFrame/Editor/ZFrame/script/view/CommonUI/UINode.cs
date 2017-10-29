using UnityEngine;


namespace ZFEditor
{
    public class UINode : LayoutInfo
    {
        public static UINode Create()
        {
            return new UINode();
        }

        public WindowBase Window
        {
            get; set;
        }

        public int InstID
        {
            get; set;
        }

        public UINode Parent
        {
            get; set;
        }
        
        public bool Visible = true;
        
        public string Style
        {
            get; set;
        }

        protected EventHandler mEventHandler;
        protected FocusState mFocusState = FocusState.None;

        public virtual void Init() { }
        public virtual void Update() { }
        public virtual void Draw(Rect clipArea) { }

        public virtual void SetEventHandler(EventHandler handler)
        {
            this.mEventHandler = handler;
        }

        public virtual void HandleEvents(InputEvent evt)
        {
            if (mEventHandler != null)
                mEventHandler.HandleEvents(evt);
        }

        public Rect GetRelative(Rect clipWorldArea)
        {
            return new Rect(WorldArea.x - clipWorldArea.x, WorldArea.y - clipWorldArea.y, WorldArea.width, WorldArea.height);
        }
        
        public virtual void UpdateFoucs(FocusState state)
        {
            mFocusState = state;
        }

        public virtual void Serialize() { }
        public virtual void Deserialize() { }
    }

}