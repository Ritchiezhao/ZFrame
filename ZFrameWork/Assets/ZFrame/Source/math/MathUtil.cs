using System.Collections;

#if FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST
using tfloat = zf.core.Fixed;
#else
using tfloat = System.Single;
#endif

namespace zf.core {
    public static class MathUtil {

        public static tfloat PI = 3.1415926f;

        public static tfloat Epsilon = 1.401298E-45F;

        public static tfloat Rad2Deg = 180 / PI;

        public static tfloat Deg2Rad = PI / 180;

        public static int FloorToInt(tfloat f)
        {
            return (int)f;
        }

        public static tfloat Clamp01(tfloat a)
        {
            if(a > 1.0f) return 1.0f;
            else if(a < 0.0f) return 0.0f;
            else return a;
        }

        public static int Clamp01(int a) {
            return a > 1 ? 1 : a < 0 ? 0 : a;
        }

        public static tfloat Lerp(tfloat a, tfloat b, tfloat t)
        {
            return a + (b - a) * (t > 1 ? 1 : t < 0 ? 0 : t);
        }

        public static int RoundToInt(tfloat f)
        {
            return (int)f;
        }

        // Return angle in degree [0 - 360]
        public static tfloat ToDegAngle(tfloat y, tfloat x)
        {
            tfloat angle = Atan2(y, x) * Rad2Deg;
            if (angle < 0) angle += 360;
            return angle;
        }

        public static sbyte ClampByte( int i ) {
	        if ( i < sbyte.MinValue ) {
                return sbyte.MinValue;
	        }
            if (i > sbyte.MaxValue) {
                return sbyte.MaxValue;
	        }
            return (sbyte)i;
        }

        public static byte ClampUByte(int i) { 
            if(i < byte.MinValue) return byte.MinValue;
            if(i > byte.MaxValue) return byte.MaxValue;
            return (byte)i;
        }

        public static short ClampShort( int i ) {
            if (i < short.MinValue) {
                return short.MinValue;
            }
            if (i > short.MaxValue) {
                return short.MaxValue;
            }
            return (short)i;
        }

        public static int ClampInt(int min, int max, int value) {
            return value < min? min : (value > max? max : value);
        }

        public static int Abs(int i) {
            return System.Math.Abs(i);
        }

        public static float Abs(float f) {
            return System.Math.Abs(f);
        }

        public static Fixed Abs(Fixed f) {
            return Math.Abs(f);
        }

        public static int Ftoi(float f) {
            return (int)f;
        }

        public static float Sqrt(float f) {
            return (float)System.Math.Sqrt(f);
        }

        public static Fixed Sqrt(Fixed f) {
            return Math.Sqrt(f);
        }

        public static float Atan2(float y, float x) {
            return (float)System.Math.Atan2(y, x);
        }

        public static Fixed Atan2(Fixed y, Fixed x) {
            return Math.Atan2(y, x);
        }

        public static int Sign(float f) { 
            return f<0? -1 : (f>0? 1 : 0);
        }

        public static int SignBit(float f) { 
            return f<0? 1 : 0;
        }

        public static ushort[] Convert(int[] ints, int offset, int count) {
            if(count <=0 || (ints.Length-offset) < count) {
                return null;
            }

            ushort[] shorts = new ushort[count];
            for(int i=0; i<count; ++i) {
                shorts[i] = (ushort)ints[offset+i];
            }
            return shorts;
        }
    }
}
