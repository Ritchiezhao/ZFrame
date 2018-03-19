using System;
using System.Collections.Generic;
using System.IO;



[AttributeUsage(AttributeTargets.All)]
public class HelpAttribute : System.Attribute 
{
   public readonly string Url;

   public string Topic               // Topic is a named parameter
   {
      get 
      { 
         return topic; 
      }
      set 
      { 

        topic = value; 
      }
   }

   public HelpAttribute(string url)  // url is a positional parameter
   {
      this.Url = url;
   }

   private string topic;
}


namespace zf.core
{
    public partial class EntityComponent : EnvObject
    {
        public Entity entity;

        public bool ActiveDefault { 
            get;
            set;
        }

        protected bool activeHierarchy;
        protected bool activeSelf;

        public bool IsAwaked { 
            get;
            set;
        }

        public bool IsStarted {
            get;
            set;
        }

        public EntityComponent()
        {
            activeSelf = false;
            activeHierarchy = false;
            entity = null;
            IsAwaked = false;
            IsStarted = false;
        }

        public EntityComponent(Entity entity):this()
        {
            this.entity = entity;
        }

        public override int GetHashCode()
        {
            return Uid.GetHashCode();
        }

        public void RefreshActiveHierarchy()
        {
            if (activeHierarchy == false) {
                if (activeSelf == true && entity.ActiveHierarchy == true) {
                    activeHierarchy = true;
                    entity.AddActiveComponent(this);
                    if (!IsStarted) {
                        entity.AddWaitStart(this);
                    }
                    OnEnable();
                }
            } else {
                if (activeSelf == false || entity.ActiveHierarchy == false) {
                    activeHierarchy = false;
                    entity.RemoveActiveComponent(this);
                    OnDisable();
                }
            }
        }

        public void SetActive(bool active)
        {
            if (IsAwaked) {
                if (active != activeSelf) {
                    activeSelf = active;
                    RefreshActiveHierarchy();
                }
            } else {
                ActiveDefault = active;
            }
        }

        public bool IsActive()
        {
            return this.activeSelf;
        }
       

        void RegisterMessages()
        {
        }

        public virtual void Awake()
        {
            RegisterMessages();
        }

        public virtual void OnEnable()
        {            
        }

        public virtual void OnDisable()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void OnDestroy()
        {
            if(this.entity!=null) {
                this.entity.RemoveComponent(this);
            }
        }

        public T GetComponent<T>() where T : EntityComponent
        {
            return entity.GetComponent<T>();
        }

        public virtual void OnSharedAttrRefresh()
        {
        }
    }

}
