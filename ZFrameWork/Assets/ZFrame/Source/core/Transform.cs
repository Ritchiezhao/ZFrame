
using System;
using System.Collections.Generic;

namespace zf.core
{

    public partial class Transform : EntityComponent
    {
        public Transform parent;
        public List<Transform> children;

        Vector3 posForTile = new Vector3(-99999, -99999, -99999);

        public Vector3 selfPosition = Vector3.ZERO;

        // Euler angle in degree
        public Vector3 selfRotation = Vector3.ZERO;
        public Vector3 selfScale = Vector3.ONE;

        public Vector3 Forward {
            get {
                Quaternion q = Quaternion.FromEulerDegree(ref selfRotation);
                return q * Vector3.FORWARD;
            }
        }

        public Vector3 Right {
            get {
                Quaternion q = Quaternion.FromEulerDegree(ref selfRotation);
                return q * Vector3.RIGHT;
            }
        }

        public Vector3 Up {
            get {
                Quaternion q = Quaternion.FromEulerDegree(ref selfRotation);
                return q * Vector3.UP;
            }
        }

        public Transform()
        {
            parent = null;
            children = null;
        }

        public override void Awake()
        {
            base.Awake();
        }

        public override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        public bool RemoveChild(Transform child)
        {
            if (children != null && child != null) {
                return children.Remove(child);
            }
            return false;
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="child">Child.</param>
        public void AddChild(Transform child)
        {
            if (child != null) {
                if (children == null) {
                    children = new List<Transform>();
                }
                children.Add(child);
            }
        }

        public void RotateDegree(Vector3 eulerAngles)
        {
            Quaternion eulerRot = Quaternion.FromEulerDegree(ref eulerAngles);
            Quaternion rotation = Quaternion.FromEulerDegree(ref selfRotation);
            //rotation = rotation * (Quaternion.Inverse(ref rotation)*eulerRot*rotation);
            rotation = rotation * eulerRot;
            selfRotation = Quaternion.ToEulerDegree(ref rotation);
        }

        public void RotateRad(Vector3 eulerAngles)
        {
            RotateDegree(eulerAngles * Math.RAD2DEG);
        }
    }

}