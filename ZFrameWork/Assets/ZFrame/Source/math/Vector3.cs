
using System;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core
{
    public partial struct Vector3
    {
        public tfloat x;
        public tfloat y;
        public tfloat z;

        [NonSerialized]
        public static readonly Vector3 ZERO = new Vector3(Math.ZERO, Math.ZERO, Math.ZERO);

        [NonSerialized]
        public static readonly Vector3 ONE = new Vector3(Math.ONE, Math.ONE, Math.ONE);

        [NonSerialized]
        public static readonly Vector3 FORWARD = new Vector3(Math.ZERO, Math.ZERO, Math.ONE);

        [NonSerialized]
        public static readonly Vector3 BACK = new Vector3(Math.ZERO, Math.ZERO, -Math.ONE);

        [NonSerialized]
        public static readonly Vector3 UP = new Vector3(Math.ZERO, Math.ONE, Math.ZERO);

        [NonSerialized]
        public static readonly Vector3 DOWN = new Vector3(Math.ZERO, -Math.ONE, Math.ZERO);

        [NonSerialized]
        public static readonly Vector3 LEFT = new Vector3(-Math.ONE, Math.ZERO, Math.ZERO);

        [NonSerialized]
        public static readonly Vector3 RIGHT = new Vector3(Math.ONE, Math.ZERO, Math.ZERO);

        public tfloat this[int index]
        {
            get
            {
                switch (index) {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
            set
            {
                switch (index) {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public Vector3 normalized
        {
            get
            {
                return Vector3.Normalize(this);
            }
        }

        public tfloat magnitude
        {
            get
            {
                return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
        }

        public tfloat sqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }


        public Vector3(tfloat x, tfloat y, tfloat z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(tfloat x, tfloat y)
        {
            this.x = x;
            this.y = y;
            this.z = Math.ZERO;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, tfloat t)
        {
            t = Math.Clamp01(t);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, tfloat t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, tfloat maxDistanceDelta)
        {
            Vector3 a = target - current;
            tfloat magnitude = a.magnitude;
            if (magnitude <= maxDistanceDelta || magnitude == Math.ZERO) {
                return target;
            }
            return current + a / magnitude * maxDistanceDelta;
        }

        public void Set(tfloat x, tfloat y, tfloat z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 Scale(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Scale(Vector3 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
        }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3)) {
                return false;
            }
            Vector3 vector = (Vector3)other;
            return this.x.Equals(vector.x) && this.y.Equals(vector.y) && this.z.Equals(vector.z);
        }

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            return Math.TWO * Vector3.Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector3 Normalize(Vector3 value)
        {
            tfloat num = Vector3.Magnitude(value);
            if (num > Math.Epsilon) {
                return value / num;
            }
            return Vector3.ZERO;
        }

        public void Normalize()
        {
            tfloat num = Vector3.Magnitude(this);
            if (num > Math.Epsilon) {
                this /= num;
            }
            else {
                this = Vector3.ZERO;
            }
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", new object[]
                {
                    this.x,
                    this.y,
                    this.z
                });
        }

        public static tfloat Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            tfloat num = Vector3.Dot(onNormal, onNormal);
            if (num < Math.Epsilon) {
                return Vector3.ZERO;
            }
            return onNormal * Vector3.Dot(vector, onNormal) / num;
        }

        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            return vector - Vector3.Project(vector, planeNormal);
        }

        public static tfloat Angle(Vector3 from, Vector3 to)
        {
            return Math.Acos(Math.Clamp(Vector3.Dot(from.normalized, to.normalized), -Math.ONE, Math.ONE)) * Math.RAD2DEG;
        }

        public static tfloat Distance(Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static Vector3 ClampMagnitude(Vector3 vector, tfloat maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength) {
                return vector.normalized * maxLength;
            }
            return vector;
        }

        public static tfloat Magnitude(Vector3 a)
        {
            return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        }

        public static tfloat SqrMagnitude(Vector3 a)
        {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }

        public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        }

        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        public static Vector3 operator *(Vector3 a, tfloat d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        public static Vector3 operator *(tfloat d, Vector3 a)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        public static Vector3 operator /(Vector3 a, tfloat d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return (Math.Abs(lhs.x - rhs.x) < Math.Epsilon
                && Math.Abs(lhs.y - rhs.y) < Math.Epsilon
                && Math.Abs(lhs.z - rhs.z) < Math.Epsilon
            );
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
    }
}

