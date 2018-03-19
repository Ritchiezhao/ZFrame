
using System;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core
{
	public struct Matrix44
	{
        public tfloat m00;
        public tfloat m10;
        public tfloat m20;
        public tfloat m30;
        public tfloat m01;
        public tfloat m11;
        public tfloat m21;
        public tfloat m31;
        public tfloat m02;
        public tfloat m12;
        public tfloat m22;
        public tfloat m32;
        public tfloat m03;
        public tfloat m13;
        public tfloat m23;
        public tfloat m33;

        public tfloat this[int row, int column]
        {
            get
            {
                return this[row + column * 4];
            }
            set
            {
                this[row + column * 4] = value;
            }
        }

        public tfloat this[int index]
        {
            get
            {
                switch (index)
                {
                case 0:
                    return this.m00;
                case 1:
                    return this.m10;
                case 2:
                    return this.m20;
                case 3:
                    return this.m30;
                case 4:
                    return this.m01;
                case 5:
                    return this.m11;
                case 6:
                    return this.m21;
                case 7:
                    return this.m31;
                case 8:
                    return this.m02;
                case 9:
                    return this.m12;
                case 10:
                    return this.m22;
                case 11:
                    return this.m32;
                case 12:
                    return this.m03;
                case 13:
                    return this.m13;
                case 14:
                    return this.m23;
                case 15:
                    return this.m33;
                default:
                    throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
            set
            {
                switch (index)
                {
                case 0:
                    this.m00 = value;
                    break;
                case 1:
                    this.m10 = value;
                    break;
                case 2:
                    this.m20 = value;
                    break;
                case 3:
                    this.m30 = value;
                    break;
                case 4:
                    this.m01 = value;
                    break;
                case 5:
                    this.m11 = value;
                    break;
                case 6:
                    this.m21 = value;
                    break;
                case 7:
                    this.m31 = value;
                    break;
                case 8:
                    this.m02 = value;
                    break;
                case 9:
                    this.m12 = value;
                    break;
                case 10:
                    this.m22 = value;
                    break;
                case 11:
                    this.m32 = value;
                    break;
                case 12:
                    this.m03 = value;
                    break;
                case 13:
                    this.m13 = value;
                    break;
                case 14:
                    this.m23 = value;
                    break;
                case 15:
                    this.m33 = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }

        public Matrix44 transpose
        {
            get
            {
                return Matrix44.Transpose(this);
            }
        }

        public tfloat determinant
        {
            get
            {
                return Matrix44.Determinant(this);
            }
        }

        public static Matrix44 zero
        {
            get
            {
                return new Matrix44
                {
                    m00 = Math.ZERO,
                    m01 = Math.ZERO,
                    m02 = Math.ZERO,
                    m03 = Math.ZERO,
                    m10 = Math.ZERO,
                    m11 = Math.ZERO,
                    m12 = Math.ZERO,
                    m13 = Math.ZERO,
                    m20 = Math.ZERO,
                    m21 = Math.ZERO,
                    m22 = Math.ZERO,
                    m23 = Math.ZERO,
                    m30 = Math.ZERO,
                    m31 = Math.ZERO,
                    m32 = Math.ZERO,
                    m33 = Math.ZERO
                };
            }
        }

        public static Matrix44 identity
        {
            get
            {
                return new Matrix44
                {
                    m00 = Math.ONE,
                    m01 = Math.ZERO,
                    m02 = Math.ZERO,
                    m03 = Math.ZERO,
                    m10 = Math.ZERO,
                    m11 = Math.ONE,
                    m12 = Math.ZERO,
                    m13 = Math.ZERO,
                    m20 = Math.ZERO,
                    m21 = Math.ZERO,
                    m22 = Math.ONE,
                    m23 = Math.ZERO,
                    m30 = Math.ZERO,
                    m31 = Math.ZERO,
                    m32 = Math.ZERO,
                    m33 = Math.ONE
                };
            }
        }

        public override int GetHashCode()
        {
            return this.GetColumn(0).GetHashCode() ^ this.GetColumn(1).GetHashCode() << 2 ^ this.GetColumn(2).GetHashCode() >> 2 ^ this.GetColumn(3).GetHashCode() >> 1;
        }

        public override bool Equals(object other)
        {
            if (!(other is Matrix44))
            {
                return false;
            }
            Matrix44 matrix4x = (Matrix44)other;
            return this.GetColumn(0).Equals(matrix4x.GetColumn(0)) && this.GetColumn(1).Equals(matrix4x.GetColumn(1)) && this.GetColumn(2).Equals(matrix4x.GetColumn(2)) && this.GetColumn(3).Equals(matrix4x.GetColumn(3));
        }   

        public static Matrix44 Transpose(Matrix44 m)
        {
            Matrix44 result = m;
            result.m00 = m.m00; result.m11 = m.m11; result.m22 = m.m22; result.m33 = m.m33;
            result.m01 = m.m10; result.m02 = m.m20; result.m03 = m.m30;
            result.m10 = m.m01; result.m12 = m.m21; result.m13 = m.m31;
            result.m20 = m.m02; result.m21 = m.m12; result.m23 = m.m32;
            result.m30 = m.m03; result.m31 = m.m13; result.m32 = m.m23;
            return result;
        }

        public static bool Inverse(Matrix44 inMatrix, out Matrix44 dest)
        {            
            Matrix44 tempMatrix = inMatrix;

            dest = identity;

            int r0 = 0, r1 = 1, r2 = 2, r3 = 3;

            if (Math.Abs (tempMatrix [r3, 0]) > Math.Abs (tempMatrix [r2, 0])) {
                int t = r3;
                r3 = r2;
                r2 = t;
            }

            if (Math.Abs (tempMatrix [r2, 0]) > Math.Abs (tempMatrix [r1, 0])) {
                int t = r1;
                r1 = r2;
                r2 = t;
            }

            if (Math.Abs (tempMatrix [r1, 0]) > Math.Abs (tempMatrix [r0, 0])) {
                int t = r0;
                r0 = r1;
                r1 = t;
            }

            if (Math.ZERO == tempMatrix [0, 0]) {
                return false;
            }

            tfloat m1 = tempMatrix[r1,0] / tempMatrix[r0,0];
            tfloat m2 = tempMatrix[r2,0] / tempMatrix[r0,0];
            tfloat m3 = tempMatrix[r3,0] / tempMatrix[r0,0];

            tfloat s = tempMatrix[r0,1];
            tempMatrix [r1, 1] -= m1 * s; tempMatrix [r2, 1] -= m2 * s; tempMatrix [r3, 1] -= m3 * s;

            s = tempMatrix [r0, 2];
            tempMatrix [r1, 2] -= m1 * s; tempMatrix [r2, 2] -= m2 * s; tempMatrix [r3, 2] -= m3 * s;

            s = tempMatrix [r0, 3];
            tempMatrix [r1, 3] -= m1 * s; tempMatrix [r2, 3] -= m2 * s; tempMatrix [r3, 3] -= m3 * s;

            s = dest [r0, 0];
            if (s != Math.ZERO) {
                dest [r1, 0] -= m1 * s; dest [r2, 0] -= m2 * s; dest [r3, 0] -= m3 * s;
            }
            s = dest [r0, 1];
            if (s != Math.ZERO) {
                dest [r1, 1] -= m1 * s; dest [r2, 1] -= m2 * s; dest [r3, 1] -= m3 * s;
            }
            s = dest [r0, 2];
            if (s != Math.ZERO) {
                dest [r1, 2] -= m1 * s; dest [r2, 2] -= m2 * s; dest [r3, 2] -= m3 * s;
            }
            s = dest [r0, 3];
            if (s != Math.ZERO) {
                dest [r1, 3] -= m1 * s; dest [r2, 3] -= m2 * s; dest [r3, 3] -= m3 * s;
            }
            

            if (Math.Abs (tempMatrix [r3, 1]) > Math.Abs (tempMatrix [r2, 1])) {
                int t = r2; r2 = r3; r3 = t;
            }
            if (Math.Abs (tempMatrix [r2, 1]) > Math.Abs (tempMatrix [r1, 1])) {
                int t = r2; r2 = r1; r1 = t;
            }
            if (tempMatrix [r1, 1] == Math.ZERO) {
                return false;
            }

            m2 = tempMatrix [r2, 1] / tempMatrix [r1, 1]; 
            m3 = tempMatrix [r3, 1] / tempMatrix [r1, 1];
            tempMatrix [r2, 2] -= m2 * tempMatrix [r1, 2];
            tempMatrix [r3, 2] -= m3 * tempMatrix [r1, 2];
            tempMatrix [r2, 3] -= m2 * tempMatrix [r1, 3];
            tempMatrix [r3, 3] -= m3 * tempMatrix [r1, 3];
            s = dest [r1, 0];
            if (s != Math.ZERO) {
                dest [r2, 0] -= m2 * s; dest [r3, 0] -= m3 * s;
            }
            s = dest [r1, 1];
            if (s != Math.ZERO) {
                dest [r2, 1] -= m2 * s; dest [r3, 1] -= m3 * s;
            }
            s = dest [r1, 2];
            if (s != Math.ZERO) {
                dest [r2, 2] -= m2 * s; dest [r3, 2] -= m3 * s;
            }
            s = dest [r1, 3];
            if (s != Math.ZERO) {
                dest [r2, 3] -= m2 * s; dest [r3, 3] -= m3 * s;
            }

            if (Math.Abs (tempMatrix [r3, 2]) > Math.Abs (tempMatrix [r2, 2])) {
                int t = r3;
                r3 = r2;
                r2 = t;
            }
            if (tempMatrix [r2, 2] == Math.ZERO) {
                return false;
            }

            m3 = tempMatrix [r3, 2] / tempMatrix [r2, 2];
            tempMatrix [r3, 3] -= m3 * tempMatrix [r2, 3]; dest [r3, 0] -= m3 * dest [r2, 0];
            dest [r3, 1] -= m3 * dest [r2, 1]; dest [r3, 2] -= m3 * dest [r2, 2];
            dest [r3, 3] -= m3 * dest [r2, 3];

            if (tempMatrix [r3, 3] == Math.ZERO) {
                return false;
            }

            s = Math.ONE / tempMatrix [r3, 3];
            dest [r3, 0] *= s;  dest [r3, 1] *= s; dest [r3, 2] *= s; dest [r3, 3] *= s;
            m2 = tempMatrix [r2, 3];
            s = Math.ONE / tempMatrix [r2, 2];
            dest [r2, 0] = s * (dest [r2, 0] - dest [r3, 0] * m2);
            dest [r2, 1] = s * (dest [r2, 1] - dest [r3, 1] * m2);
            dest [r2, 2] = s * (dest [r2, 2] - dest [r3, 2] * m2);
            dest [r2, 3] = s * (dest [r2, 3] - dest [r3, 3] * m2);
            m1 = tempMatrix [r1, 3];
            dest [r1, 0] -= dest [r3, 0] * m1;
            dest [r1, 1] -= dest [r3, 1] * m1;
            dest [r1, 2] -= dest [r3, 2] * m1;
            dest [r1, 3] -= dest [r3, 3] * m1;
            tfloat m0 = tempMatrix [r0, 3];
            dest [r0, 0] -= dest [r3, 0] * m0; 
            dest [r0, 1] -= dest [r3, 1] * m0;
            dest [r0, 2] -= dest [r3, 2] * m0;
            dest [r0, 3] -= dest [r3, 3] * m0;

            m1 = tempMatrix [r1, 2];
            s = Math.ONE / tempMatrix [r1, 1];
            dest [r1, 0] = s * (dest [r1, 0] - dest [r2, 0] * m1);
            dest [r1, 1] = s * (dest [r1, 1] - dest [r2, 1] * m1);
            dest [r1, 2] = s * (dest [r1, 2] - dest [r2, 2] * m1);
            dest [r1, 3] = s * (dest [r1, 3] - dest [r2, 3] * m1);

            m0 = tempMatrix [r0, 2];
            dest [r0, 0] -= dest [r2, 0] * m0;
            dest [r0, 1] -= dest [r2, 1] * m0;
            dest [r0, 2] -= dest [r2, 2] * m0;
            dest [r0, 3] -= dest [r2, 3] * m0;

            m0 = tempMatrix [r0, 1];
            s = Math.ONE / tempMatrix [r0, 0];
            dest [r0, 0] = s * (dest [r0, 0] - dest [r1, 0] * m0);
            dest [r0, 1] = s * (dest [r0, 1] - dest [r1, 1] * m0);
            dest [r0, 2] = s * (dest [r0, 2] - dest [r1, 2] * m0);
            dest [r0, 3] = s * (dest [r0, 3] - dest [r1, 3] * m0);

            return true;
        }

        public static tfloat Determinant(Matrix44 m)
        {
            tfloat v1 = m.m22 * m.m33 - m.m23 * m.m32;
            tfloat v2 = m.m23 * m.m31 - m.m21 * m.m33;
            tfloat v3 = m.m21 * m.m32 - m.m22 * m.m31;

            tfloat t1 = m.m11 * v1;
            tfloat t2 = m.m12 * v2;
            tfloat t3 = m.m13 * v3;
            tfloat x1 = m.m00 * (t1 + t2 + t3);

            tfloat v4 = m.m23 * m.m30 - m.m20 * m.m33;
            tfloat v5 = m.m20 * m.m32 - m.m22 * m.m30;
            t1 = m.m10 * v1;
            t2 = m.m12 * v4;
            t3 = m.m13 * v5;
            tfloat x2 = -m.m01 * (t1 + t2 + t3);

            tfloat v6 = m.m20 * m.m31 - m.m21 * m.m30;
            t1 = m.m10 * (-v2);
            t2 = m.m11 * v4;
            t3 = m.m13 * v6;
            tfloat x3 = m.m02 * (t1 + t2 + t3);

            t1 = m.m10 * v3;
            t2 = m.m11 * (-v5);
            t3 = m.m12 * v6;
            tfloat x4 = -m.m03 * (t1 + t2 + t3);

            return x1 + x2 + x3 + x4;
        }

        public Vector4 GetColumn(int i)
        {
            return new Vector4 (this [0, i], this [1, i], this [2, i], this [3, i]);
        }

        public Vector4 GetRow(int i)
        {
            return new Vector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
        }

        public void SetColumn(int i, Vector4 v)
        {
            this[0, i] = v.x;
            this[1, i] = v.y;
            this[2, i] = v.z;
            this[3, i] = v.w;
        }

        public void SetRow(int i, Vector4 v)
        {
            this[i, 0] = v.x;
            this[i, 1] = v.y;
            this[i, 2] = v.z;
            this[i, 3] = v.w;
        }

        public Vector3 MultiplyPoint(Vector3 v)
        {
            Vector3 result;
            result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
            result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
            result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
            tfloat num = this.m30 * v.x + this.m31 * v.y + this.m32 * v.z + this.m33;
            num = Math.ONE / num;
            result.x *= num;
            result.y *= num;
            result.z *= num;
            return result;
        }

        public Vector3 MultiplyPoint3x4(Vector3 v)
        {
            Vector3 result;
            result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03;
            result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13;
            result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23;
            return result;
        }

        public Vector3 MultiplyVector(Vector3 v)
        {
            Vector3 result;
            result.x = this.m00 * v.x + this.m01 * v.y + this.m02 * v.z;
            result.y = this.m10 * v.x + this.m11 * v.y + this.m12 * v.z;
            result.z = this.m20 * v.x + this.m21 * v.y + this.m22 * v.z;
            return result;
        }

        public static Matrix44 Scale(Vector3 v)
        {
            return new Matrix44
            {
                m00 = v.x,
                m01 = Math.ZERO,
                m02 = Math.ZERO,
                m03 = Math.ZERO,
                m10 = Math.ZERO,
                m11 = v.y,
                m12 = Math.ZERO,
                m13 = Math.ZERO,
                m20 = Math.ZERO,
                m21 = Math.ZERO,
                m22 = v.z,
                m23 = Math.ZERO,
                m30 = Math.ZERO,
                m31 = Math.ZERO,
                m32 = Math.ZERO,
                m33 = Math.ONE
            };
        }
 
        public override string ToString()
        {
            return string.Format("{0:F5}\t{1:F5}\t{2:F5}\t{3:F5}\n{4:F5}\t{5:F5}\t{6:F5}\t{7:F5}\n{8:F5}\t{9:F5}\t{10:F5}\t{11:F5}\n{12:F5}\t{13:F5}\t{14:F5}\t{15:F5}\n", new object[]
                {
                    this.m00,
                    this.m01,
                    this.m02,
                    this.m03,
                    this.m10,
                    this.m11,
                    this.m12,
                    this.m13,
                    this.m20,
                    this.m21,
                    this.m22,
                    this.m23,
                    this.m30,
                    this.m31,
                    this.m32,
                    this.m33
                });
        }

        public static Matrix44 Ortho(tfloat left, tfloat right, tfloat bottom, tfloat top, tfloat zNear, tfloat zFar)
        {
            Matrix44 result = identity;

            tfloat deltax = right - left;
            tfloat deltay = top - bottom;
            tfloat deltaz = zFar - zNear;

            result[0,0] = Math.TWO / deltax;
            result[0,3] = -(right + left) / deltax;
            result[1,1] = Math.TWO / deltay;
            result[1,3] = -(top + bottom) / deltay;
            result[2,2] = -Math.TWO / deltaz;
            result[2,3] = -(zFar + zNear) / deltaz;

            return result;
        }

        public static Matrix44 Perspective(tfloat fov, tfloat aspect, tfloat zNear, tfloat zFar)
        {
            Matrix44 result = new Matrix44 ();
            tfloat radians = Math.DEG2RAD * (fov / Math.TWO);
            tfloat cotangent = Math.Cos (radians) / Math.Sin (radians);
            tfloat deltaZ = zNear - zFar;
            result [0,0] = cotangent / aspect;      result [0,1] = Math.ZERO;      result [0,2] = Math.ZERO;                    result [0,3] = Math.ZERO;
            result [1,0] = Math.ZERO;               result [1,1] = cotangent;      result [1,2] = Math.ZERO;                    result [1,3] = Math.ZERO;
            result [2,0] = Math.ZERO;               result [2,1] = Math.ZERO;      result [2,2] = (zFar + zNear) / deltaZ;      result [2,3] = Math.TWO * zNear * zFar / deltaZ;
            result [3,0] = Math.ZERO;               result [3,1] = Math.ZERO;      result [3,2] = -Math.ONE;                    result [3,3] = Math.ZERO;
            return result;
        }

        public static Matrix44 operator *(Matrix44 lhs, Matrix44 rhs)
        {
            return new Matrix44
            {
                m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30,
                m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31,
                m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32,
                m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33,
                m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30,
                m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31,
                m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32,
                m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33,
                m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30,
                m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31,
                m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32,
                m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33,
                m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30,
                m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31,
                m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32,
                m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33
            };
        }

        public static Vector4 operator *(Matrix44 lhs, Vector4 v)
        {
            Vector4 result;
            result.x = lhs.m00 * v.x + lhs.m01 * v.y + lhs.m02 * v.z + lhs.m03 * v.w;
            result.y = lhs.m10 * v.x + lhs.m11 * v.y + lhs.m12 * v.z + lhs.m13 * v.w;
            result.z = lhs.m20 * v.x + lhs.m21 * v.y + lhs.m22 * v.z + lhs.m23 * v.w;
            result.w = lhs.m30 * v.x + lhs.m31 * v.y + lhs.m32 * v.z + lhs.m33 * v.w;
            return result;
        }

        public static bool operator ==(Matrix44 lhs, Matrix44 rhs)
        {
            return lhs.GetColumn(0) == rhs.GetColumn(0) && lhs.GetColumn(1) == rhs.GetColumn(1) && lhs.GetColumn(2) == rhs.GetColumn(2) && lhs.GetColumn(3) == rhs.GetColumn(3);
        }

        public static bool operator !=(Matrix44 lhs, Matrix44 rhs)
        {
            return !(lhs == rhs);
        }
	}

}

