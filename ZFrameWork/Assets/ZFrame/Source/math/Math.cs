
using System;

namespace zf.core
{
    #if FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST
    // Math for fixed
    public struct Math
    {
        public static readonly Fixed PI = Fixed.PI;
        public static readonly Fixed DEG2RAD = Fixed.DEG2RAD;
        public static readonly Fixed RAD2DEG = Fixed.RAD2DEG;
        public static readonly Fixed ZERO = Fixed.ZERO;
        public static readonly Fixed ONE = Fixed.ONE;
        public static readonly Fixed TWO = new Fixed(2,0);
        public static readonly Fixed Epsilon = new Fixed(0,1);

        public static Fixed Sin(Fixed f)
        {
            return Fixed.Sin (f);
        }

        public static Fixed Cos(Fixed f)
        {
            return Fixed.Cos (f);
        }

        public static Fixed Tan(Fixed f)
        {
            return Fixed.Tan(f);
        }

        public static Fixed Asin(Fixed f)
        {
            return Fixed.Asin(f);
        }

        public static Fixed Acos(Fixed f)
        {
            return Fixed.Acos(f);
        }

        public static Fixed Atan(Fixed f)
        {
            return Fixed.Atan(f);
        }

        public static Fixed Atan2(Fixed y, Fixed x)
        {
            return Fixed.Atan2(y, x);
        }

        public static Fixed Sqrt(Fixed f)
        {
            return Fixed.Sqrt(f);
        }

        public static Fixed Abs(Fixed value)
        {
            return Fixed.Abs(value);
        }

        public static Fixed Min(Fixed a, Fixed b)
        {
            return (a >= b) ? b : a;
        }

        public static int Min(int a, int b)
        {
            return (a >= b) ? b : a;
        }

        public static Fixed Max(Fixed a, Fixed b)
        {
            return (a <= b) ? b : a;
        }

        public static Fixed Exp(Fixed power)
        {
            return Fixed.Exp(power);
        }

        public static Fixed Ceil(Fixed f)
        {
            return Fixed.Ceil(f);
        }

        public static Fixed Floor(Fixed f)
        {
            return Fixed.Floor(f);
        }

        public static Fixed Round(Fixed f)
        {
            return Fixed.Round(f);
        }

        public static Fixed Sign(Fixed f)
        {
            return Fixed.Sign (f);
        }

        public static Fixed Clamp(Fixed value, Fixed min, Fixed max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static Fixed Clamp01(Fixed value)
        {
            if (value < Fixed.ZERO)
            {
                return Fixed.ZERO;
            }
            if (value > Fixed.ONE)
            {
                return Fixed.ONE;
            }
            return value;
        }

        public static Fixed Lerp(Fixed a, Fixed b, Fixed t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static Fixed LerpUnclamped(Fixed a, Fixed b, Fixed t)
        {
            return a + (b - a) * t;
        }
    }
    #else
    // Math for float
	public class Math
	{
        public const float PI = 3.14159274f;
        public const float DEG2RAD = 0.0174532924f;
        public const float RAD2DEG = 57.29578f;
        public const float ZERO = 0f;
        public const float ONE = 1f;
        public static readonly float TWO = 2f;
        public static readonly float Epsilon = 1E-05f;

        public static float Sin(float f)
        {
            return (float)System.Math.Sin((double)f);
        }

        public static float Cos(float f)
        {
            return (float)System.Math.Cos((double)f);
        }

        public static float Tan(float f)
        {
            return (float)System.Math.Tan((double)f);
        }

        public static float Asin(float f)
        {
            return (float)System.Math.Asin((double)f);
        }

        public static float Acos(float f)
        {
            return (float)System.Math.Acos((double)f);
        }

        public static float Atan(float f)
        {
            return (float)System.Math.Atan((double)f);
        }

        public static float Atan2(float y, float x)
        {
            return (float)System.Math.Atan2((double)y, (double)x);
        }

        public static float Sqrt(float f)
        {
            return (float)System.Math.Sqrt((double)f);
        }

        public static float Abs(float f)
        {
            return System.Math.Abs(f);
        }

        public static int Abs(int value)
        {
            return System.Math.Abs(value);
        }

        public static float Min(float a, float b)
        {
            return (a >= b) ? b : a;
        }

        public static float Max(float a, float b)
        {
            return (a <= b) ? b : a;
        }

        public static float Exp(float power)
        {
            return (float)System.Math.Exp((double)power);
        }

        public static float Ceil(float f)
        {
            return (float)System.Math.Ceiling((double)f);
        }

        public static float Floor(float f)
        {
            return (float)System.Math.Floor((double)f);
        }

        public static float Round(float f)
        {
            return (float)System.Math.Round((double)f);
        }

        public static float Sign(float f)
        {
            return (f < 0f) ? -1f : 1f;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }


    }
    #endif
}
