using System;
using System.Collections.Generic;
using System.IO;

namespace zf.core
{
    public sealed partial class Entity : EnvObject
    {
        public Transform transform;

        private string name;
        private long tag;

        private bool activeSelf;
        private bool activeHierarchy;

        public List<EntityComponent> components;
        public List<Entity> children;
        public Entity parent;

        public bool isRootEntity = false;

        public bool isTopEntity {
            get {
                return parent == null || parent.isRootEntity;
            }
        }

        //private bool isAwaked;
        //private bool isStarted;
        public bool IsAwaked {
            get;
            set;
        }

        public bool IsStarted {
            get;
            set;
        }

        public bool ActiveDefault {
            get;
            set;
        }

        private Queue<Entity> waitforStart;
        private Queue<EntityComponent> waitforStartComponents;

        private bool isActiveComponentChanged;
        public HashSet<EntityComponent> activeComponents;
        private List<EntityComponent> runActiveComponents;

        private bool isChildActiveChanged;
        public HashSet<Entity> activeChild;
        private List<Entity> runActiveChilds;

        public int awakeTime = 0;

        int sharedAttrLastSendTime = 0;
        const int shareAttrSendDelta = 500;

        public void AddActivedChild(Entity child)
        {
            this.activeChild.Add(child);
            isChildActiveChanged = true;
        }

        public void RemoveActivedChild(Entity child)
        {
            this.activeChild.Remove(child);
            isChildActiveChanged = true;
        }

        public void AddActiveComponent(EntityComponent component)
        {
            activeComponents.Add(component);
            isActiveComponentChanged = true;
        }

        public void RemoveActiveComponent(EntityComponent component)
        {
            activeComponents.Remove(component);
            isActiveComponentChanged = true;
        }

        public Entity()
        {
            components = new List<EntityComponent>();

            activeComponents = new HashSet<EntityComponent>();
            runActiveComponents = new List<EntityComponent>();
            isActiveComponentChanged = false;

            activeChild = new HashSet<Entity>();
            runActiveChilds = new List<Entity>();
            isChildActiveChanged = false;

            waitforStart = new Queue<Entity>();
            waitforStartComponents = new Queue<EntityComponent>();

            activeSelf = false;
            activeHierarchy = false;

            children = null;
            parent = null;

            IsAwaked = false;
            IsStarted = false;

        }

        public override void OnCreated()
        {
            //this.tag = template.tag;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public long Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        //public long CateTag
        //{
        //    get { return (long)template.cateTag; }
        //}

        public int PlayerIdx
        {
            get; set;
        }

        public bool ActiveSelf
        {
            get { return activeSelf; }
        }

        public bool ActiveHierarchy
        {
            get { return activeHierarchy; }
        }

        public override int GetHashCode()
        {
            return Uid.GetHashCode();
        }

        public bool HasParent()
        {
            return parent != null;
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="child">Child.</param>
        public void AddChild(Entity child)
        {
            if (child != null) {
                if (this.children == null) {
                    this.children = new List<Entity>();
                }
                this.children.Add(child);
                this.transform.AddChild(child.transform);
                child.parent = this;
                child.transform.parent = transform;
                child.RefreshActiveHierarchyRecursively();
                if (child.ActiveHierarchy) {
                    AddActivedChild(child);
                }
            }
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <returns><c>true</c>, if child was removed, <c>false</c> otherwise.</returns>
        /// <param name="child">Child.</param>
        public bool RemoveChild(Entity child)
        {
            if (this.children != null && child != null) {
                RemoveActivedChild(child);
                this.children.Remove(child);
                this.transform.RemoveChild(child.transform);
                child.parent = null;
                child.transform.parent = null;
            }
            return false;
        }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parentEntity">Parent entity.</param>
        public void SetParent(Entity parentEntity)
        {
            if (this.parent != parentEntity) {
                if (parent != null) {
                    parent.RemoveChild(this);
                }
                if (parentEntity != null) {
                    parentEntity.AddChild(this);
                }
                else {
                    parent = null;
                    transform.parent = null;
                }
                this.RefreshActiveHierarchyRecursively();
            }
        }

        /// <summary>
        /// Sets the active recursively.
        /// </summary>
        /// <param name="active">If set to <c>true</c> active.</param>
        public void SetActiveRecursively(bool active)
        {
            if (children != null) {
                List<Entity> tempChilds = new List<Entity>(this.children);
                int count = tempChilds.Count;
                for (int i = 0; i < count; ++i) {
                    Entity child = tempChilds[i];
                    if (child != null) {
                        child.SetActiveRecursively(active);
                    }
                }
            }
            this.activeSelf = active;
            RefreshActiveHierarchy();
        }

        private bool RefreshActiveHierarchy()
        {
            bool localActiveHierarchy = activeHierarchy;
            if (activeSelf) {
                if (HasParent()) {
                    if (parent.activeHierarchy == true) {
                        localActiveHierarchy = true;
                    }
                    else {
                        localActiveHierarchy = false;
                    }
                }
                else {
                    localActiveHierarchy = true;
                }
            }
            else {
                localActiveHierarchy = false;
            }

            if (localActiveHierarchy != activeHierarchy) {

                activeHierarchy = localActiveHierarchy;

                if (this.parent != null) {
                    if (localActiveHierarchy) {
                        this.parent.AddActivedChild(this);
                        if (!IsStarted) {
                            this.parent.AddWaitStart(this);
                        }
                    }
                    else {
                        this.parent.RemoveActivedChild(this);
                    }
                }

                List<EntityComponent> subComponents = new List<EntityComponent>(this.components);
                for (int i = 0; i < subComponents.Count; ++i) {
                    EntityComponent component = subComponents[i];
                    if (component != null) {
                        //递归调用可能的OnEnable
                        component.RefreshActiveHierarchy();
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Refreshs the active hierarchy recursively.
        /// </summary>
        public void RefreshActiveHierarchyRecursively()
        {
            if (RefreshActiveHierarchy()) {
                if (children != null) {
                    for (int i = 0; i < children.Count; ++i) {
                        Entity child = children[i];
                        if (child != null) {
                            child.RefreshActiveHierarchyRecursively();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the active.
        /// </summary>
        /// <param name="active">If set to <c>true</c> active.</param>
        public void SetActive(bool active)
        {
            if (this.IsAwaked) {
                if (this.activeSelf != active) {
                    this.activeSelf = active;
                    RefreshActiveHierarchyRecursively();
                }
            } else {
                this.ActiveDefault = active;
            }
        }

        /// <summary>
        /// Adds the wait start.
        /// </summary>
        /// <param name="child">Child.</param>
        public void AddWaitStart(Entity child)
        {
            //如果entity已经start了，则子节点不能走通常的entity start路径
            if (IsStarted) {
                waitforStart.Enqueue(child);
            }
        }

        /// <summary>
        /// Adds the wait start.
        /// </summary>
        /// <param name="sub">Sub.</param>
        public void AddWaitStart(EntityComponent sub)
        {
            //如果entity已经start了，则组件不能走通常的entity start路径
            if (IsStarted) {
                waitforStartComponents.Enqueue(sub);
            }
        }

        /// <summary>
        /// Adds the component.
        /// </summary>
        /// <param name="component">Component.</param>
        public void AddComponent(EntityComponent component)
        {
            if (component != null) {
                components.Add(component);
                component.entity = this;

                if (IsAwaked) {
                    AwakeComponent(component);
                    if (IsStarted) {
                        StartComponent(component);
                    }
                }
            }
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(EntityComponent component)
        {
            if (this.components == null
                || component == null) {
                return;
            }

            if (this.components.Contains(component)) {
                this.components.Remove(component);
            }
        }

        //public T AddComponent<T>(TID tid) where T : EntityComponent, new()
        //{
        //    T component = new T();
        //    AddComponent(component);
        //    return component;
        //}

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <returns>The component.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetComponent<T>() where T : EntityComponent
        {
            Type tp = typeof(T);
            tp.GetHashCode();
            for (int i = 0; i < components.Count; ++i) {
                EntityComponent component = components[i];
                if (component is T) {
                    return component as T;
                }
            }
            return null;
        }

        public T GetComponentInChildren<T>() where T : EntityComponent
        {
            if (children == null)
                return null;

            for (int i = 0; i < children.Count; i++) {
                Entity child = children[i];
                EntityComponent component = child.GetComponent<T>();
                if (component != null)
                    return component as T;
            }

            return null;
        }

        private void AwakeComponent(EntityComponent component)
        {
            if (component != null && !component.IsAwaked) {
                component.Awake();
                component.IsAwaked = true;
                component.SetActive(component.ActiveDefault);
            }
        }

        private void StartComponent(EntityComponent component)
        {
            if (component != null && component.IsActive() && !component.IsStarted) {
                component.Start();
                component.IsStarted = true;
            }
        }

        private void StartChild(Entity child)
        {
            if (child != null && child.ActiveHierarchy && !child.IsStarted) {
                child.Start();
                child.IsStarted = true;
            }
        }

        public void Awake()
        {
            for (int i = 0; i < this.components.Count; ++i) {
                EntityComponent component = components[i];
                if (component != null) {
                    AwakeComponent(component);
                }
            }

            awakeTime = Env.GetFrameTime();
        }

        public void Start()
        {
            if (this.ActiveHierarchy && !IsStarted) {
                if (isActiveComponentChanged) {
                    this.runActiveComponents.Clear();
                    this.runActiveComponents.AddRange(this.activeComponents);
                    isActiveComponentChanged = false;
                }

                for (int i = 0; i < this.runActiveComponents.Count; ++i) {
                    EntityComponent component = this.runActiveComponents[i];
                    if (component != null) {
                        StartComponent(component);
                    }
                }

                if (isChildActiveChanged) {
                    this.runActiveChilds.Clear();
                    this.runActiveChilds.AddRange(this.activeChild);
                    isChildActiveChanged = false;
                }

                for (int i = 0; i < this.runActiveChilds.Count; ++i) {
                    Entity child = runActiveChilds[i];
                    StartChild(child);
                }
                IsStarted = true;
            }
        }

        public void Reset()
        {
            for (int i = 0; i < components.Count; ++i) {
                EntityComponent component = components[i];
                if (component != null) {
                    component.Reset();
                }
            }

            if (this.children != null) {
                for (int i = 0; i < children.Count; ++i) {
                    Entity child = children[i];
                    if (child != null) {
                        child.Reset();
                    }
                }
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (activeHierarchy) {

                while (waitforStartComponents.Count > 0) {
                    EntityComponent component = waitforStartComponents.Dequeue();
                    if (component != null) {
                        StartComponent(component);
                    }
                }

                while (waitforStart.Count > 0) {
                    Entity child = waitforStart.Dequeue();
                    if (child != null && !child.isDestroyed) {
                        StartChild(child);
                    }
                }

                if (isActiveComponentChanged) {
                    this.runActiveComponents.Clear();
                    this.runActiveComponents.AddRange(this.activeComponents);
                }


                for (int i = 0, len = runActiveComponents.Count; i < len; ++i) {
                    EntityComponent component = runActiveComponents[i];
                    if (component != null) {
                        component.Update();
                    }
                }

                if (isChildActiveChanged) {
                    this.runActiveChilds.Clear();
                    this.runActiveChilds.AddRange(this.activeChild);
                }

                for (int i = 0, len = this.runActiveChilds.Count; i < len; ++i) {
                    Entity entity = this.runActiveChilds[i];
                    if (entity != null) {
                        entity.Update();
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (activeHierarchy) {
                for (int i = 0, len = runActiveComponents.Count; i < len; ++i) {
                    EntityComponent component = runActiveComponents[i];
                    if (component != null) {
                        component.FixedUpdate();
                    }
                }

                for (int i = 0, len = runActiveChilds.Count; i < len; ++i) {
                    Entity entity = this.runActiveChilds[i];
                    if (entity != null) {
                        entity.FixedUpdate();
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (activeHierarchy) {
                for (int i = 0, len = runActiveComponents.Count; i < len; ++i) {
                    EntityComponent component = runActiveComponents[i];
                    if (component != null) {
                        component.LateUpdate();
                    }
                }

                for (int i = 0, len = runActiveChilds.Count; i < len; ++i) {
                    Entity entity = this.runActiveChilds[i];
                    if (entity != null) {
                        entity.LateUpdate();
                    }
                }
            }
        }


        public bool isDestroyed = false;
        public void OnDestroy()
        {
            if (parent != null) {
                //在DestroyEntity里也会调用，这里重复无影响
                parent.RemoveChild(this);
            }

            for (int i = components.Count - 1; i >= 0; --i) {
                EntityComponent component = components[i];
                if (component != null) {
                    component.OnDestroy();
                }
            }

            //if (this.children != null) {
            //    for (int i = children.Count - 1; i >= 0; --i) {
            //        Entity child = children[i];
            //        if (child != null) {
            //            service.DestroyEntity(child);
            //        }
            //    }
            //}

            isDestroyed = true;
        }

        public override string ToString()
        {
            return string.Format("[Entity: {0}({1})]", Uid, Tid);
        }
    }
}