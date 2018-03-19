
using System;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core
{
	public struct Vector4
	{
        public tfloat x;
        public tfloat y;
        public tfloat z;
        public tfloat w;

        [NonSerialized]
        public static readonly Vector4 ZERO = new Vector4(Math.ZERO, Math.ZERO, Math.ZERO, Math.ZERO);

        [NonSerialized]
        public static readonly Vector4 ONE = new Vector4(Math.ONE, Math.ONE, Math.ONE, Math.ONE);

        public tfloat this[int index]
        {
            get
            {
                switch (index)
                {
                case 0:
                    return this.x;
                case 1:
                    return this.y;
                case 2:
                    return this.z;
                case 3:
                    return this.w;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
                }
            }
            set
            {
                switch (index)
                {
                case 0:
                    this.x = value;
                    break;
                case 1:
                    this.y = value;
                    break;
                case 2:
                    this.z = value;
                    break;
                case 3:
                    this.w = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
                }
            }
        }

        public Vector4 normalized
        {
            get
            {
                return Vector4.Normalize(this);
            }
        }

        public tfloat magnitude
        {
            get
            {
                return Math.Sqrt(Vector4.Dot(this, this));
            }
        }

        public tfloat sqrMagnitude
        {
            get
            {
                return Vector4.Dot(this, this);
            }
        }

        public Vector4(tfloat x, tfloat y, tfloat z, tfloat w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(tfloat x, tfloat y, tfloat z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = Math.ZERO;
        }

        public Vector4(tfloat x, tfloat y)
        {
            this.x = x;
            this.y = y;
            this.z = Math.ZERO;
            this.w = Math.ZERO;
        }

        public void Set(tfloat x, tfloat y, tfloat z, tfloat w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Vector4 Lerp(Vector4 a, Vector4 b, tfloat t)
        {
            t = Math.Clamp01(t);
            return new Vector4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
        }

        public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, tfloat t)
        {
            return new Vector4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
        }

        public static Vector4 MoveTowards(Vector4 current, Vector4 target, tfloat maxDistanceDelta)
        {
            Vector4 a = target - current;
            tfloat magnitude = a.magnitude;
            if (magnitude <= maxDistanceDelta || magnitude == Math.ZERO)
            {
                return target;
            }
            return current + a / magnitude * maxDistanceDelta;
        }

        public static Vector4 Scale(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        public void Scale(Vector4 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
            this.w *= scale.w;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector4))
            {
                return false;
            }
            Vector4 vector = (Vector4)other;
            return this.x.Equals(vector.x) && this.y.Equals(vector.y) && this.z.Equals(vector.z) && this.w.Equals(vector.w);
        }

        public static Vector4 Normalize(Vector4 a)
        {
            tfloat num = Vector4.Magnitude(a);
            if (num > Math.Epsilon)
            {
                return a / num;
            }
            return Vector4.ZERO;
        }

        public void Normalize()
        {
            tfloat num = Vector4.Magnitude(this);
            if (num > Math.Epsilon)
            {
                this /= num;
            }
            else
            {
                this = Vector4.ZERO;
            }
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
                {
                    this.x,
                    this.y,
                    this.z,
                    this.w
                });
        }

        public static tfloat Dot(Vector4 a, Vector4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static Vector4 Project(Vector4 a, Vector4 b)
        {
            return b * Vector4.Dot(a, b) / Vector4.Dot(b, b);
        }

        public static tfloat Distance(Vector4 a, Vector4 b)
        {
            return Vector4.Magnitude(a - b);
        }

        public static tfloat Magnitude(Vector4 a)
        {
            return Math.Sqrt(Vector4.Dot(a, a));
        }

        public static tfloat SqrMagnitude(Vector4 a)
        {
            return Vector4.Dot(a, a);
        }

        public tfloat SqrMagnitude()
        {
            return Vector4.Dot(this, this);
        }

        public static Vector4 Min(Vector4 lhs, Vector4 rhs)
        {
            return new Vector4(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z), Math.Min(lhs.w, rhs.w));
        }

        public static Vector4 Max(Vector4 lhs, Vector4 rhs)
        {
            return new Vector4(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z), Math.Max(lhs.w, rhs.w));
        }

        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Vector4 operator -(Vector4 a)
        {
            return new Vector4(-a.x, -a.y, -a.z, -a.w);
        }

        public static Vector4 operator *(Vector4 a, tfloat d)
        {
            return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
        }

        public static Vector4 operator *(tfloat d, Vector4 a)
        {
            return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
        }

        public static Vector4 operator /(Vector4 a, tfloat d)
        {
            return new Vector4(a.x / d, a.y / d, a.z / d, a.w / d);
        }

        public static bool operator ==(Vector4 lhs, Vector4 rhs)
        {
            return Vector4.SqrMagnitude(lhs - rhs) < Math.Epsilon;
        }

        public static bool operator !=(Vector4 lhs, Vector4 rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator Vector4(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, Math.ZERO);
        }

        public static implicit operator Vector3(Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4(Vector2 v)
        {
            return new Vector4(v.x, v.y, Math.ZERO, Math.ZERO);
        }

        public static implicit operator Vector2(Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }        
	}
}

