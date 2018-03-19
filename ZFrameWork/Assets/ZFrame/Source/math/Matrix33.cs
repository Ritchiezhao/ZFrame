using System;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core
{
    public struct Matrix33
    {
        // The Get function accesses the matrix in std math convention
        // m0,0 m0,1 m0,2
        // m1,0 m1,1 m1,2
        // m2,0 m2,1 m2,2
        public tfloat m00;
        public tfloat m10;
        public tfloat m20;

        public tfloat m01;
        public tfloat m11;
        public tfloat m21;

        public tfloat m02;
        public tfloat m12;
        public tfloat m22;
        

        public tfloat this[int row, int column]
        {
            get
            {
                return this[row + column * 3];
            }
            set
            {
                this[row + column * 3] = value;
            }
        }

        public tfloat this[int index]
        {
            get
            {
                switch (index) {
                    case 0:
                        return this.m00;
                    case 1:
                        return this.m10;
                    case 2:
                        return this.m20;
                    case 3:
                        return this.m01;
                    case 4:
                        return this.m11;
                    case 5:
                        return this.m21;
                    case 6:
                        return this.m02;
                    case 7:
                        return this.m12;
                    case 8:
                        return this.m22;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
            set
            {
                switch (index) {
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
                        this.m01 = value;
                        break;
                    case 4:
                        this.m11 = value;
                        break;
                    case 5:
                        this.m21 = value;
                        break;
                    case 6:
                        this.m02 = value;
                        break;
                    case 7:
                        this.m12 = value;
                        break;
                    case 8:
                        this.m22 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }

        public Matrix33 SetFromToRotation(Vector3 from, Vector3 to)
        {
            Matrix33 mtx = new Matrix33();
            this.FromToRotation(from, to, ref mtx);
            this[0, 0] = mtx[0, 0]; this[0, 1] = mtx[0, 1]; this[0, 2] = mtx[0, 2];
            this[1, 0] = mtx[1, 0]; this[1, 1] = mtx[1, 1]; this[1, 2] = mtx[1, 2];
            this[2, 0] = mtx[2, 0]; this[2, 1] = mtx[2, 1]; this[2, 2] = mtx[2, 2];
            return mtx;
        }

        public void FromToRotation(Vector3 from, Vector3 to, ref Matrix33 mtx)
        {
            Vector3 v = Vector3.Cross(from, to);
            tfloat e = Vector3.Dot(from, to);

            if (e > (Math.ONE - Math.Epsilon)) {
                mtx[0, 0] = Math.ONE; mtx[0, 1] = Math.ZERO; mtx[0, 2] = Math.ZERO;
                mtx[0, 0] = Math.ZERO; mtx[0, 1] = Math.ONE; mtx[0, 2] = Math.ZERO;
                mtx[0, 0] = Math.ZERO; mtx[0, 1] = Math.ZERO; mtx[0, 2] = Math.ONE;
            }
            else if (e < (new tfloat(-1) * Math.ONE + Math.Epsilon)) {
                Vector3 up, left = Vector3.ZERO;
                tfloat invlen;
                tfloat fxx, fyy, fzz, fxy, fxz, fyz;
                tfloat uxx, uyy, uzz, uxy, uxz, uyz;
                tfloat lxx, lyy, lzz, lxy, lxz, lyz;

                left[0] = Math.ZERO; left[1] = from[2]; left[2] = -from[1];
                if (Vector3.Dot(left, left) < Math.Epsilon) 
                {

                    left[0] = -from[2]; left[1] = Math.ZERO; left[2] = from[0];
                }
                
                invlen = Math.ONE / Math.Sqrt(Vector3.Dot(left, left));
                left[0] *= invlen;
                left[1] *= invlen;
                left[2] *= invlen;
                up = Vector3.Cross(left, from);

                fxx = -from[0] * from[0]; fyy = -from[1] * from[1]; fzz = -from[2] * from[2];
                fxy = -from[0] * from[1]; fxz = -from[0] * from[2]; fyz = -from[1] * from[2];

                uxx = up[0] * up[0]; uyy = up[1] * up[1]; uzz = up[2] * up[2];
                uxy = up[0] * up[1]; uxz = up[0] * up[2]; uyz = up[1] * up[2];

                lxx = -left[0] * left[0]; lyy = -left[1] * left[1]; lzz = -left[2] * left[2];
                lxy = -left[0] * left[1]; lxz = -left[0] * left[2]; lyz = -left[1] * left[2];
                /* symmetric matrix */
                mtx[0, 0] = fxx + uxx + lxx; mtx[0, 1] = fxy + uxy + lxy; mtx[0, 2] = fxz + uxz + lxz;
                mtx[1, 0] = mtx[0, 1]; mtx[1, 1] = fyy + uyy + lyy; mtx[1, 2] = fyz + uyz + lyz;
                mtx[2, 0] = mtx[0, 2]; mtx[2, 1] = mtx[1, 2]; mtx[2, 2] = fzz + uzz + lzz;
            }
            else {
                tfloat hvx, hvz, hvxy, hvxz, hvyz;
                tfloat h = (Math.ONE - e) / Vector3.Dot(v, v);
                hvx = h * v[0];
                hvz = h * v[2];
                hvxy = hvx * v[1];
                hvxz = hvx * v[2];
                hvyz = hvz * v[1];
                mtx[0, 0] = e + hvx * v[0]; mtx[0, 1] = hvxy - v[2]; mtx[0, 2] = hvxz + v[1];
                mtx[1, 0] = hvxy + v[2]; mtx[1, 1] = e + h * v[1] * v[1]; mtx[1, 2] = hvyz - v[0];
                mtx[2, 0] = hvxz - v[1]; mtx[2, 1] = hvyz + v[0]; mtx[2, 2] = e + hvz * v[2];
            }
        }

    }
}