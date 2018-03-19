
#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
#define USE_FIXED
#endif

using System;
using System.Runtime.Serialization;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core
{
    public struct Vector2
	{        
		public tfloat x;
		public tfloat y;

        [NonSerialized]
        public static readonly Vector2 ZERO = new Vector2(Math.ZERO, Math.ZERO);

        [NonSerialized]
        public static readonly Vector2 ONE = new Vector2 (Math.ONE, Math.ONE);

        [NonSerialized]
        public static readonly Vector2 UP = new Vector2 (Math.ZERO, Math.ONE);

        [NonSerialized]
        public static readonly Vector2 DOWN = new Vector2 (Math.ZERO, -Math.ONE);

        [NonSerialized]
        public static readonly Vector2 LEFT = new Vector2 (-Math.ONE, Math.ZERO);

        [NonSerialized]
        public static readonly Vector2 RIGHT = new Vector2 (Math.ONE, Math.ZERO);

		public Vector2 (tfloat x, tfloat y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2 (Vector2 vec)
		{
			x = vec.x;
			y = vec.y;
		}

		public tfloat this [int index] {
			get {
				if (index == 0) {
					return this.x;
				}
				if (index == 1) {
					return this.y;
				}
				throw new IndexOutOfRangeException ("Vector2 index Invalid");
			}
			set {
				if (index == 0) {
					this.x = value;
				} else if (index == 1) {
					this.y = value;
				} else {
					throw new IndexOutOfRangeException ("Vector2 index Invalid"); 
				}
			}
		}

        public Vector2 normalized
        {
            get
            {
                Vector2 result = new Vector2(this.x, this.y);
                result.Normalize();
                return result;
            }
        }

        public tfloat magnitude
        {
            get
            {
                return Math.Sqrt(this.x * this.x + this.y * this.y);
            }
        }

        public tfloat sqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y;
            }
        }

        public void Set(tfloat x, tfloat y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, tfloat t)
        {
            t = Math.Clamp01(t);
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, tfloat t)
        {
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, tfloat maxDistanceDelta)
        {
            Vector2 a = target - current;
            tfloat magnitude = a.magnitude;
            if (magnitude <= maxDistanceDelta || magnitude == Math.ZERO)
            {
                return target;
            }
            return current + a / magnitude * maxDistanceDelta;
        }

        public static Vector2 Scale(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public void Scale(Vector2 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
        }

        public void Normalize()
        {
            tfloat magnitude = this.magnitude;
            if (magnitude > Math.Epsilon)
            {
                this /= magnitude;
            }
            else
            {
                this = Vector2.ZERO;
            }
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1})", new object[]
                {
                    this.x,
                    this.y
                });
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2))
            {
                return false;
            }
            Vector2 vector = (Vector2)other;
            return this.x.Equals(vector.x) && this.y.Equals(vector.y);
        }

        public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal)
        {
            return -Math.TWO * Vector2.Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static tfloat Dot(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public static tfloat Angle(Vector2 from, Vector2 to)
        {
            return Math.Acos(Math.Clamp(Vector2.Dot(from.normalized, to.normalized), -Math.ONE, Math.ONE)) * Math.RAD2DEG;
        }

        public static tfloat Distance(Vector2 a, Vector2 b)
        {
            return (a - b).magnitude;
        }

        public static Vector2 ClampMagnitude(Vector2 vector, tfloat maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength)
            {
                return vector.normalized * maxLength;
            }
            return vector;
        }

        public static tfloat SqrMagnitude(Vector2 a)
        {
            return a.x * a.x + a.y * a.y;
        }

        public tfloat SqrMagnitude()
        {
            return this.x * this.x + this.y * this.y;
        }

        public static Vector2 Min(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
        }

        public static Vector2 Max(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator *(Vector2 a, tfloat d)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator *(tfloat d, Vector2 a)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator /(Vector2 a, tfloat d)
        {
            return new Vector2(a.x / d, a.y / d);
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return (Math.Abs (lhs.x - rhs.x) <= Math.Epsilon && Math.Abs (lhs.y - rhs.y) < Math.Epsilon);
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, Math.ZERO);
        }
	}
}

