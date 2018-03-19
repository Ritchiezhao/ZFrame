using System;

namespace zf.core
{
    public struct Random
    {
        private const uint INT_RAND_A = 1103515245;
        private const uint INT_RAND_C = 12345;
        private const uint INT_M = 2147483647;
        private static uint randVal = 0;

        #if (FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST)

        public static void SRand(Fixed seed)
        {
            Fixed.SRand(seed);
        }

        public static Fixed RandomRange(Fixed min, Fixed max)
        {
            return Fixed.RandomRange(min, max);
        }

        public static Fixed RandomZeroOne()
        {
            return Fixed.RandomZeroOne();
        }

        #else

        public static float RandomRange(float min, float max)
        {
            uint val = GenRandom ();
            //IEEE754 float 1符号 8指数位 23尾数位, 2^23=8388608
            float t = (float)(val & 0x007FFFFF) * (1.0f / 8388607.0f);
            float rand = min * t + max * (1.0f - t);
            return rand;
        }

        public static float RandomZeroOne()
        {
            return RandomRange(0,1.0f);
        }

        #endif

        public static void SRand(int seed)
        {
            randVal = (uint)seed;
        }

        private static uint GenRandom()
        {
            uint cur = (randVal * INT_RAND_A + INT_RAND_C)%INT_M;
            randVal = cur;
            return cur;
        }

        public static int RandomInt()
        {
            return (int)(GenRandom()&0x7fffffff);
        }

        public static int RandomRange(int min, int max)
        {
            uint val = GenRandom ();
            int rand = (int)((uint)min + (val % (uint)(max - min)));
            return rand;
        }        
    }
}

