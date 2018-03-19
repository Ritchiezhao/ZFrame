
using System;

#if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)
    using tfloat = zf.core.Fixed;
#else
    using tfloat = System.Single;
#endif


namespace zf.core
{
    public enum RotationOrder
    {
        kOrderXYZ,
        kOrderXZY,
        kOrderYZX,
        kOrderYXZ,
        kOrderZXY,
        kOrderZYX,
        kRotationOrderLast = kOrderZYX,
        kOrderUnityDefault = kOrderZXY
    };

    public struct Quaternion
    {
        public const int kRotationOrderCount = (int)RotationOrder.kRotationOrderLast + 1;

        public tfloat w;
        public tfloat x;
        public tfloat y;
        public tfloat z;
        [NonSerialized]
        public static readonly Quaternion identity = new Quaternion(Math.ZERO, Math.ZERO, Math.ZERO, Math.ONE);
        public Quaternion(tfloat x, tfloat y, tfloat z, tfloat w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public Quaternion(Quaternion qua)
        {
            this.x = qua.x;
            this.y = qua.y;
            this.z = qua.z;
            this.w = qua.w;
        }
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
                    case 3:
                        return this.w;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
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
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaternion index!");
                }
            }
        }
        public static Quaternion operator +(Quaternion lhs, Quaternion rhs)
        {
            Quaternion q = new Quaternion(lhs);
            return q += rhs;
        }
        public static Quaternion operator -(Quaternion lhs, Quaternion rhs)
        {
            Quaternion q = new Quaternion(lhs);
            return q -= rhs;
        }
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) {
            tfloat tempx = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
            tfloat tempy = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
            tfloat tempz = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
            tfloat tempw = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
            Quaternion result = new Quaternion(tempx, tempy, tempz, tempw);
            return result;
        }
        public static Quaternion operator *(Quaternion q, tfloat s)
        {
            return new Quaternion(q.x * s, q.y * s, q.z * s, q.w * s);
        }
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            tfloat num = rotation.x * Math.TWO;
            tfloat num2 = rotation.y * Math.TWO;
            tfloat num3 = rotation.z * Math.TWO;
            tfloat num4 = rotation.x * num;
            tfloat num5 = rotation.y * num2;
            tfloat num6 = rotation.z * num3;
            tfloat num7 = rotation.x * num2;
            tfloat num8 = rotation.x * num3;
            tfloat num9 = rotation.y * num3;
            tfloat num10 = rotation.w * num;
            tfloat num11 = rotation.w * num2;
            tfloat num12 = rotation.w * num3;
            Vector3 result;
            result.x = (Math.ONE - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (Math.ONE - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (Math.ONE - (num4 + num5)) * point.z;
            return result;
        }
        public static Quaternion operator /(Quaternion q, tfloat s)
        {
            return new Quaternion(q.x / s, q.y / s, q.z / s, q.w / s);
        }
        public static Quaternion operator /(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }
        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return (Math.Abs(lhs.x - rhs.x) < Math.Epsilon
                && Math.Abs(lhs.y - rhs.y) < Math.Epsilon
                && Math.Abs(lhs.z - rhs.z) < Math.Epsilon
                && Math.Abs(lhs.w - rhs.w) < Math.Epsilon
            );
        }
        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object other)
        {
            if (!(other is Quaternion)) {
                return false;
            }
            Quaternion q = (Quaternion)other;
            return this.x.Equals(q.x) && this.y.Equals(q.y) && this.z.Equals(q.z) && this.w.Equals(q.w);
        }
        public override int GetHashCode()
        {
            return (((this.x.GetHashCode() + this.y.GetHashCode()) + this.z.GetHashCode()) + this.w.GetHashCode());
        }
        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            tfloat lhsMag = fromDirection.magnitude;
            tfloat rhsMag = toDirection.magnitude;
            if (lhsMag < Math.Epsilon || rhsMag < Math.Epsilon) {
                return identity;
            }

            //nomarl///
            Vector3 lhs = fromDirection / lhsMag;
            Vector3 rhs = toDirection / rhsMag;

            Matrix33 mtx = new Matrix33();
            mtx = mtx.SetFromToRotation(lhs, rhs);

            Quaternion q = new Quaternion();
            MatrixToQuaternion(mtx, ref q);
            return q;
        }
        public static void MatrixToQuaternion(Matrix33 mtx, ref Quaternion q)
        {
            tfloat fTrace = mtx[0, 0] + mtx[1, 1] + mtx[2, 2];
            tfloat fRoot;
            if (fTrace > Math.ZERO) {
                fRoot = Math.Sqrt(fTrace + Math.ONE);  // 2w
                q.w = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;  // 1/(4w)
                q.x = (mtx[2, 1] - mtx[1, 2]) * fRoot;
                q.y = (mtx[0, 2] - mtx[2, 0]) * fRoot;
                q.z = (mtx[1, 0] - mtx[0, 1]) * fRoot;
            }
            else {

                int[] next = new int[3] { 1, 2, 0 };

                int i = 0;
                if (mtx[1, 1] > mtx[0, 0]) {
                    i = 1;
                }

                if (mtx[2, 2] > mtx[i, i]) {
                    i = 2;
                }

                int j = next[i];
                int k = next[j];

                fRoot = Math.Sqrt(mtx[i, i] - mtx[j, j] - mtx[k, k] + Math.ONE);
                q[i] = 0.5f / fRoot;
                q.w = (mtx[k, j] - mtx[j, k]) * fRoot;
                q[j] = (mtx[j, i] + mtx[i, j]) * fRoot;
                q[k] = (mtx[k, i] + mtx[i, k]) * fRoot;
            }

            q = Normalize(q);
        }
        public static tfloat Dot(Quaternion q1, Quaternion q2)
        {
            return (q1.x * q2.x + q1.y * q2.y + q1.z * q2.z + q1.w * q2.w);
        }

        public static tfloat SqrMagnitude(Quaternion q) {
		    return Dot (q, q);
	    }

        public static tfloat Magnitude(ref Quaternion q)
        {
	        return Math.Sqrt(SqrMagnitude (q));
        }

        public static Quaternion Normalize(Quaternion q)
        {
            return q / Math.Sqrt(Dot(q, q));
        }

        public static Quaternion NormalizeSafe(ref Quaternion q) { 
            tfloat mag = Magnitude(ref q);
            if (mag < Math.Epsilon) { 
                return Quaternion.identity;
            } else { 
                return q / mag;
            }
        }

        public static Quaternion Conjugate(Quaternion q) { 
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        public static Quaternion Inverse(Quaternion q) { 
            return Conjugate(q);
        }

        private static Quaternion CreateQuaternionFromAxisQuaternions (
            Quaternion q1, Quaternion q2, Quaternion q3, out Quaternion result)
	    {
		    result = (q1 * q2) * q3;
		    //Assert (core::CompareApproximately (SqrMagnitude (result), 1.0F));
		    return result;
	    }

        private static tfloat qCos(tfloat a) { 
            //return Math.Cos(a);
            return (float)System.Math.Cos((float)a);
        }

        private static tfloat qSin(tfloat a) {
            //return Math.Sin(a);
            return (float)System.Math.Sin((float)a);
        }

        /// <summary>
        /// Input euler angle is rad
        /// </summary>
        /// <param name="someEulerAngles"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static Quaternion EulerToQuaternion (Vector3 someEulerAngles, 
            RotationOrder order = RotationOrder.kOrderUnityDefault)
	    {
		    tfloat cX = qCos (someEulerAngles.x / 2.0f);
		    tfloat sX = qSin (someEulerAngles.x / 2.0f);

		    tfloat cY = qCos (someEulerAngles.y / 2.0f);
		    tfloat sY = qSin (someEulerAngles.y / 2.0f);

		    tfloat cZ = qCos (someEulerAngles.z / 2.0f);
		    tfloat sZ = qSin (someEulerAngles.z / 2.0f);

		    Quaternion qX = new Quaternion(sX, 0.0F, 0.0F, cX);
		    Quaternion qY = new Quaternion(0.0F, sY, 0.0F, cY);
		    Quaternion qZ = new Quaternion(0.0F, 0.0F, sZ, cZ);
		    Quaternion ret = Quaternion.identity;

		    switch (order)
		    {
		    case RotationOrder.kOrderZYX: CreateQuaternionFromAxisQuaternions(qX, qY, qZ, out ret); break;
		    case RotationOrder.kOrderYZX: CreateQuaternionFromAxisQuaternions(qX, qZ, qY, out ret); break;
		    case RotationOrder.kOrderXZY: CreateQuaternionFromAxisQuaternions(qY, qZ, qX, out ret); break;
		    case RotationOrder.kOrderZXY: CreateQuaternionFromAxisQuaternions(qY, qX, qZ, out ret); break;
		    case RotationOrder.kOrderYXZ: CreateQuaternionFromAxisQuaternions(qZ, qX, qY, out ret); break;
            case RotationOrder.kOrderXYZ: CreateQuaternionFromAxisQuaternions(qZ, qY, qX, out ret); break;
		    }

		    return ret;
	    }

        //Indexes for values used to calculate euler angles
        enum Indexes
        {
            X1,
            X2,
            Y1,
            Y2,
            Z1,
            Z2,
            singularity_test,
            IndexesCount
        };

        //indexes for pre-multiplied quaternion values
        enum QuatIndexes
        {
            xx,
            xy,
            xz,
            xw,
            yy,
            yz,
            yw,
            zz,
            zw,
            ww,
            QuatIndexesCount
        };

        delegate tfloat qFunc(tfloat a, tfloat b);

        private static tfloat qAsin(tfloat a, tfloat b) { 
            //return a*Math.Asin(Math.Clamp(b, -1.0f, 1.0f));
            return a*(float)System.Math.Asin((float)Math.Clamp(b, -1.0f, 1.0f));
        }

        private static tfloat qACos(tfloat a) { 
            return (float)System.Math.Acos((float)a); 
        }

        private static tfloat qAtan2(tfloat a, tfloat b) {
            //return Math.Atan2(a, b);
            return (float)System.Math.Atan2((float)a, (float)b);
        }

        private static tfloat qNull(tfloat a, tfloat b) {
            return 0.0f;
        }

        private static qFunc[,] qFuncs = new qFunc[kRotationOrderCount, 3] {
            {qAtan2, qAsin, qAtan2}, //OrderXYZ
			{qAtan2, qAtan2, qAsin}, //OrderXZY
			{qAtan2, qAtan2, qAsin}, //OrderYZX,
			{qAsin, qAtan2, qAtan2}, //OrderYXZ,
			{qAsin, qAtan2, qAtan2}, //OrderZXY,
			{qAtan2, qAsin, qAtan2}, //OrderZYX,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <param name="order"></param>
        /// <returns>Return angle is degree</returns>
        private static Vector3 QuaternionToEuler(
            Quaternion q, RotationOrder order = RotationOrder.kOrderUnityDefault)
	    {
            if(Math.Abs(SqrMagnitude(q) - Math.ONE) > Math.Epsilon*10) {
                throw new Exception("QuaternionToEuler: Input quaternion was not normalized");
            }
		    //setup all needed values
		    tfloat[] d = new tfloat[(int)QuatIndexes.QuatIndexesCount]{
                q.x*q.x, q.x*q.y, q.x*q.z, q.x*q.w, q.y*q.y
                , q.y*q.z, q.y*q.w, q.z*q.z, q.z*q.w, q.w*q.w};

		    //Float array for values needed to calculate the angles
		    tfloat[] v = new tfloat[(int)Indexes.IndexesCount];
		    qFunc[] f = new qFunc[3] {
                qFuncs[(int)order, 0], 
                qFuncs[(int)order, 1], 
                qFuncs[(int)order, 2]
            }; //functions to be used to calculate angles

		    tfloat SINGULARITY_CUTOFF = 0.499999f;
		    Vector3 rot;
		    switch (order)
		    {
		    case RotationOrder.kOrderZYX:
			    v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];
                v[(int)Indexes.Z1] = 2.0f * (-d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw]);
                v[(int)Indexes.Z2] = d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] + d[(int)QuatIndexes.ww];
			    v[(int)Indexes.Y1] = 1.0f;
			    v[(int)Indexes.Y2] = 2.0f*v[(int)Indexes.singularity_test];
			    if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
			    {
                    v[(int)Indexes.X1] = 2.0f * (-d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw]);
                    v[(int)Indexes.X2] = d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
			    }
			    else //x == xzx z == 0
			    {
				    float a,b,c,e;
                    a = d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];
                    b = -d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];
                    c = d[(int)QuatIndexes.xz] - d[(int)QuatIndexes.yw];
                    e = d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];

				    v[(int)Indexes.X1] = a*e + b*c;
				    v[(int)Indexes.X2] = b*e - a*c;
				    f[2] = qNull;
			    }
			    break;
		    case RotationOrder.kOrderXZY:
                v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];
                v[(int)Indexes.X1] = 2.0f * (-d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw]);
                v[(int)Indexes.X2] = d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
			    v[(int)Indexes.Z1] = 1.0f;
			    v[(int)Indexes.Z2] = 2.0f*v[(int)Indexes.singularity_test];

			    if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
			    {
                    v[(int)Indexes.Y1] = 2.0f * (-d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw]);
                    v[(int)Indexes.Y2] = d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] + d[(int)QuatIndexes.ww];
			    }
			    else //y == yxy x == 0
			    {
				    float a,b,c,e;
                    a = d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];
                    b = -d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];
                    c = d[(int)QuatIndexes.xy] - d[(int)QuatIndexes.zw];
                    e = d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];

				    v[(int)Indexes.Y1] = a*e + b*c;
				    v[(int)Indexes.Y2] = b*e - a*c;
				    f[0] = qNull;
			    }
			    break;

		    case RotationOrder.kOrderYZX:
                v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.xy] - d[(int)QuatIndexes.zw];
                v[(int)Indexes.Y1] = 2.0f * (d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw]);
                v[(int)Indexes.Y2] = d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] + d[(int)QuatIndexes.ww];
			    v[(int)Indexes.Z1] = -1.0f;
			    v[(int)Indexes.Z2] = 2.0f*v[(int)Indexes.singularity_test];

			    if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
			    {
                    v[(int)Indexes.X1] = 2.0f * (d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw]);
                    v[(int)Indexes.X2] = d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.zz] + d[(int)QuatIndexes.ww];
			    }
			    else //x == xyx y == 0
			    {
				    float a,b,c,e;
                    a = d[(int)QuatIndexes.xy] - d[(int)QuatIndexes.zw];
                    b = d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];
                    c = d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];
                    e = -d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];

				    v[(int)Indexes.X1] = a*e + b*c;
				    v[(int)Indexes.X2] = b*e - a*c;
				    f[1] = qNull;
			    }
			    break;
		    case RotationOrder.kOrderZXY:
			    {
                    v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.yz] - d[(int)QuatIndexes.xw];
                    v[(int)Indexes.Z1] = 2.0f * (d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw]);
                    v[(int)Indexes.Z2] = d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
				    v[(int)Indexes.X1] = -1.0f;
				    v[(int)Indexes.X2] = 2.0f*v[(int)Indexes.singularity_test];

				    if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
				    {
                        v[(int)Indexes.Y1] = 2.0f * (d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw]);
                        v[(int)Indexes.Y2] = d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.yy] + d[(int)QuatIndexes.ww];
				    }
				    else //x == yzy z == 0
				    {
					    float a,b,c,e;
                        a = d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw];
                        b = -d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];
                        c = d[(int)QuatIndexes.xy] - d[(int)QuatIndexes.zw];
                        e = d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];

					    v[(int)Indexes.Y1] = a*e + b*c;
					    v[(int)Indexes.Y2] = b*e - a*c;
					    f[2] = qNull;
				    }
			    }
			    break;
		    case RotationOrder.kOrderYXZ:
                v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];
                v[(int)Indexes.Y1] = 2.0f * (-d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw]);
                v[(int)Indexes.Y2] = d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
			    v[(int)Indexes.X1] = 1.0f;
			    v[(int)Indexes.X2] = 2.0f*v[(int)Indexes.singularity_test];

			    if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
			    {
                    v[(int)Indexes.Z1] = 2.0f * (-d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw]);
                    v[(int)Indexes.Z2] = d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
			    }
			    else //x == zyz y == 0
			    {
				    float a,b,c,e;
                    a = d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];
                    b = -d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];
                    c = d[(int)QuatIndexes.yz] - d[(int)QuatIndexes.xw];
                    e = d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];

				    v[(int)Indexes.Z1] = a*e + b*c;
				    v[(int)Indexes.Z2] = b*e - a*c;
				    f[1] = qNull;
			    }
			    break;
		    case RotationOrder.kOrderXYZ:
                v[(int)Indexes.singularity_test] = d[(int)QuatIndexes.xz] - d[(int)QuatIndexes.yw];
                v[(int)Indexes.X1] = 2.0f * (d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw]);
                v[(int)Indexes.X2] = d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] - d[(int)QuatIndexes.xx] + d[(int)QuatIndexes.ww];
			    v[(int)Indexes.Y1] = -1.0f;
                v[(int)Indexes.Y2] = 2.0f * v[(int)Indexes.singularity_test];

                if (Math.Abs(v[(int)Indexes.singularity_test]) < SINGULARITY_CUTOFF)
			    {
                    v[(int)Indexes.Z1] = 2.0f * (d[(int)QuatIndexes.xy] + d[(int)QuatIndexes.zw]);
                    v[(int)Indexes.Z2] = d[(int)QuatIndexes.xx] - d[(int)QuatIndexes.zz] - d[(int)QuatIndexes.yy] + d[(int)QuatIndexes.ww];
			    }
			    else //x == zxz x == 0
			    {
				    float a,b,c,e;
                    a = d[(int)QuatIndexes.xz] - d[(int)QuatIndexes.yw];
                    b = d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];
                    c = d[(int)QuatIndexes.xz] + d[(int)QuatIndexes.yw];
                    e = -d[(int)QuatIndexes.yz] + d[(int)QuatIndexes.xw];

				    v[(int)Indexes.Z1] = a*e + b*c;
				    v[(int)Indexes.Z2] = b*e - a*c;
				    f[0] = qNull;
			    }
			    break;
		    }

		    rot = new Vector3(	f[0](v[(int)Indexes.X1], v[(int)Indexes.X2]),
			    f[1](v[(int)Indexes.Y1], v[(int)Indexes.Y2]),
			    f[2](v[(int)Indexes.Z1], v[(int)Indexes.Z2]));

		    //Assert(IsFinite(rot));

		    return rot;
	    }

        public static Quaternion FromEulerDegree(ref Vector3 degree) {
            return EulerToQuaternion(degree*Math.DEG2RAD);
        }

        public static Quaternion FromEulerRad(ref Vector3 rad) { 
            return EulerToQuaternion(rad);
        }

        public void Set (float inX, float inY, float inZ, float inW) {
	        x = inX;
	        y = inY;
	        z = inZ;
	        w = inW;
        }

        public static Quaternion Lerp( Quaternion q1, Quaternion q2, float t ) {
	        Quaternion tmpQuat = Quaternion.identity;
	        // if (dot < 0), q1 and q2 are more than 360 deg apart.
	        // The problem is that quaternions are 720deg of freedom.
	        // so we - all components when lerping
	        if (Dot (q1, q2) < 0.0F) {
		        tmpQuat.Set(q1.x + t * (-q2.x - q1.x),
		                    q1.y + t * (-q2.y - q1.y),
		                    q1.z + t * (-q2.z - q1.z),
		                    q1.w + t * (-q2.w - q1.w));
	        } else {
		        tmpQuat.Set(q1.x + t * (q2.x - q1.x),
		                    q1.y + t * (q2.y - q1.y),
		                    q1.z + t * (q2.z - q1.z),
		                    q1.w + t * (q2.w - q1.w));
	        }
	        return Normalize (tmpQuat);
        }

        public static Quaternion Slerp( Quaternion q1, Quaternion q2, float t ) {
	        float dot = Dot( q1, q2 );

	        // dot = cos(theta)
	        // if (dot < 0), q1 and q2 are more than 90 degrees apart,
	        // so we can invert one to reduce spinning
	        Quaternion tmpQuat = Quaternion.identity;
	        if (dot < 0.0f ) {
		        dot = -dot;
		        tmpQuat.Set( -q2.x,
					         -q2.y,
					         -q2.z,
					         -q2.w );
            } else {
		        tmpQuat = q2;
            }

	        if (dot < 0.95f ) {
                float angle = qACos(dot);
		        float sinadiv, sinat, sinaomt;
		        sinadiv = 1.0f/qSin(angle);
		        sinat   = qSin(angle*t);
		        sinaomt = qSin(angle*(1.0f-t));
		        tmpQuat.Set( (q1.x*sinaomt+tmpQuat.x*sinat)*sinadiv,
			             (q1.y*sinaomt+tmpQuat.y*sinat)*sinadiv,
			             (q1.z*sinaomt+tmpQuat.z*sinat)*sinadiv,
			             (q1.w*sinaomt+tmpQuat.w*sinat)*sinadiv  );
		        return tmpQuat;

	        }
	        // if the angle is small, use linear interpolation
	        else
	        {
		        return Lerp(q1,tmpQuat,t);
	        }
        }

        private static Vector3 MakePositive(Vector3 degrees) {
            tfloat negativeFlip = -0.0001f * Math.RAD2DEG;
            tfloat positiveFlip = 360.0f + negativeFlip;

            if (degrees.x < negativeFlip)
                degrees.x += 360.0f;
            else if (degrees.x > positiveFlip)
                degrees.x -= 360.0f;

            if (degrees.y < negativeFlip)
                degrees.y += 360.0f;
            else if (degrees.y > positiveFlip)
                degrees.y -= 360.0f;

            if (degrees.z < negativeFlip)
                degrees.z += 360.0f;
            else if (degrees.z > positiveFlip)
                degrees.z -= 360.0f;

            return degrees;
        }

        public static Vector3 ToEulerDegree(ref Quaternion q) { 
            Quaternion rot = NormalizeSafe(ref q);
            return MakePositive(Quaternion.QuaternionToEuler(rot) * Math.RAD2DEG);
        }

        public static Vector3 ToEulerRad(ref Quaternion q) {
            return ToEulerDegree(ref q) * Math.DEG2RAD;
        }

        public override string ToString()
        {
            return string.Format("x;{0}, y{1}, z:{2}, w:{3}", (float)x, (float)y, (float)z, (float)w);
        }
    }
}
