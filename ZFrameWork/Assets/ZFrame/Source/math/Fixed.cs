//
//the compiler pre define :
// FIXED16 is 16.16 format fixed float
// FIXED32 is 32.32 format fixed float
// FIXED4816 is 48.16 format fixed float
// otherwise use float (Single) for the math base type
//
// FIXED16，[0x80000000, 0x7fffffff]区间
// 世界的坐标不要超过［-128.0000，127.0000］的范围，精确到小数点后4位
//
// FIXED32, [0x8000000000000000, 0x7fffffffffffffff]区间
// 世界坐标应在[-32768.000000000, 32767.000000000]范围内，精确到小数点后9位
//
// FIXED4816, [0x8000000000000000, 0x7fffffffffffffff]区间
// 普通模式：世界坐标应在[-8388608.0000, 8388607.0000]范围内，精确到小数点后4位
// FAST_FIXED模式：世界坐标应在[-32768.0000, 32767.0000]范围内，精确到小数点后4位，乘除运算所有定点数里最快
//

// [ precompiler defines ]
//
// #define FIXED16              // 16.16
// #define FIXED32              // 32.32
// #define FIXED4816            // 48.16  并且关掉了异常和饱和处理，要求表示范围不能超过65536，这样不用管溢出
// #define FIXED4816_FAST
//

#if FIXED4816_FAST
    #define FAST_FIXED
    #define FIXED4816
#endif
// FAST_FIXED, 不产生溢出异常，也不会做饱和处理
// 注意此模式下，48.16定点数的乘除法会用更简单直接，要控制乘法结果在32.16范围内，高16位用于乘法运算结束后移位
// 控制除法运算的被除数在32.16范围内，高16位用于除法运算前的移位
// #define FAST_FIXED         


#if !FAST_FIXED

    //如果不定义下述两个宏，则不处理溢出
    #define FIXED_EXCEPTION  //溢出直接报告异常
    #define FIXED_SATURATING //溢出饱和保护

    #if FIXED_EXCEPTION || FIXED_SATURATING
    #define CHECK_FIXED
    #endif

#endif

using System;
using System.Text;
using System.Runtime.Serialization;

#if FIXED32 || FIXED4816
using tint = System.Int64;
using tuint = System.UInt64;
#elif FIXED16
using tint = System.Int32;
using tuint = System.UInt32;
#endif


namespace zf.core
{
    #if FIXED16 || FIXED32 || FIXED4816 || FIXED4816_FAST
    public partial struct Fixed : IEquatable<Fixed>, IComparable<Fixed>, ISerializable
    {
        #if (FIXED32)
		public const int WHOLE_BITS 	= 64;
		public const int INTPART_BITS 	= 32;
		public const int FRACPART_BITS 	= WHOLE_BITS - INTPART_BITS;
        public static readonly Fixed PI = new Fixed (13493037705);
        public static readonly Fixed PI_DIV_2 = new Fixed (6746518852);
        public static readonly Fixed PI_DIV_4 = new Fixed (3373259426);
        public static readonly Fixed E = new Fixed (11674931555);
        public static readonly Fixed PI_MUL_2 = new Fixed (26986075409);
        public static readonly Fixed DEG2RAD = new Fixed (74961321);
        public static readonly Fixed RAD2DEG = new Fixed (246083499208);
        #elif (FIXED4816)
        public const int WHOLE_BITS     = 64;
        public const int INTPART_BITS   = 48;
        public const int FRACPART_BITS  = WHOLE_BITS - INTPART_BITS;
        public static readonly Fixed PI = new Fixed (205887);
        public static readonly Fixed PI_DIV_2 = new Fixed (102944);
        public static readonly Fixed PI_DIV_4 = new Fixed (51472);
        public static readonly Fixed E = new Fixed (178145);
        public static readonly Fixed PI_MUL_2 = new Fixed (411775);
        public static readonly Fixed DEG2RAD = new Fixed (1144);
        public static readonly Fixed RAD2DEG = new Fixed (3754936);
        #elif (FIXED16)
        public const int WHOLE_BITS = 32;
        public const int INTPART_BITS = 16;
        public const int FRACPART_BITS = WHOLE_BITS - INTPART_BITS;
        public static readonly Fixed PI = new Fixed (205887);
        public static readonly Fixed PI_DIV_2 = new Fixed (102944);
        public static readonly Fixed PI_DIV_4 = new Fixed (51472);
        public static readonly Fixed E = new Fixed (178145);
        public static readonly Fixed PI_MUL_2 = new Fixed (411775);
        public static readonly Fixed DEG2RAD = new Fixed (1144);
        public static readonly Fixed RAD2DEG = new Fixed (3754936);
        #endif

        public const tint SIGN_MASK = (tint)1 << (WHOLE_BITS - 1);
        public const tint FRAC_0DOT5 = (tint)1 << (FRACPART_BITS - 1);
        public const tint FRACPART_MASK = ((tint)1 << FRACPART_BITS) - 1;
        public const tint INTPART_MASK = (((tint)1 << INTPART_BITS) - 1) << FRACPART_BITS;
        public const tint INTPART_MAX = ((tint)1 << (INTPART_BITS - 1)) ^ (((tint)1 << INTPART_BITS) - 1);
        public const tint INTPART_MIN = INTPART_MAX ^ (INTPART_MASK | FRACPART_MASK);
        public const tint WHOLE_MASK = -1;

        public static readonly Fixed ONE = new Fixed (1, 0);
        public static readonly Fixed ZERO = new Fixed ();
        public static readonly Fixed MAX = new Fixed (tint.MaxValue);
        public static readonly Fixed MIN = new Fixed (tint.MinValue);

        //三角函数速查表大小
        public const tint SIN_LUT_SIZE = 90 * 10; // 90*10，即精确到0.1度
        //public const int TAN_LUT_SIZE = 90 * 100;// 精确到0.01度

        public tint rawValue;       

        public Fixed (tint intVal, tint fracVal)
        {
            rawValue = intVal << FRACPART_BITS;
			rawValue += (fracVal & FRACPART_MASK);
        }

        public Fixed (Fixed fixedVal)
        {
            rawValue = fixedVal.rawValue;
        }

        public Fixed (tint rawVal)
        {
            rawValue = rawVal;
        }

        public Fixed (double value)
        {
            double val = value * ONE.rawValue;

            if (val >= 0) {
                val += 0.5;
            } else {
                val += -0.5;
            }

            rawValue = (tint)val;
        }

        public Fixed(float value)
        {
            float val = value * ONE.rawValue;

            if (val >= 0) {
                val += 0.5f;
            } else {
                val += -0.5f;
            }

            rawValue = (tint)val;
        }

        public Fixed (decimal value)
        {
            decimal val = value * ONE.rawValue;
            if (val >= 0) {
                val += 0.5m;
            } else {
                val += -0.5m;
            }
            rawValue = (tint)val;
        }

        public Fixed (string valStr)
        {
			this.rawValue = 0;

			valStr.Trim ();
			int Len = valStr.Length;
			if (Len <= 0) {
				return;
			}

            tint intpart = 0;
            tint fracpart = 0;
            tint fracnum = 1;
            bool negative = false;

			int i = 0;
			char ch = valStr [0];
			if (ch == '-') {
				negative = true;
				++i;
			} else if (ch == '+') {
				++i;
			}

			for (; i < valStr.Length; ++i) {
                ch = valStr [i];                
                if (ch >= '0' && ch <= '9') {
                    intpart = intpart * 10 + (ch - '0');
                } else if (ch == '.') {
					++i;
					break;
                } else {
                    throw new ArgumentOutOfRangeException ("Fixed string constructure param error");
                }
            }

			for (; i < valStr.Length; ++i) {
				ch = valStr [i];
				if (ch >= '0' && ch <= '9') {
					fracpart += (ch - '0') * ONE.rawValue / fracnum;
					fracnum *= 10;
				} else {
					throw new ArgumentOutOfRangeException ("Fixed string constructure param error");
				}
			}

            fracpart = ((fracpart + 5) / 10)&FRACPART_MASK;  //for round

            if (negative) {
            	#if CHECK_FIXED
                if (-intpart-((fracpart&INTPART_MASK)>>FRACPART_BITS) < INTPART_MIN) {
					#if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed string constructure param overflow");
                    #elif FIXED_SATURATING
					rawValue = MIN;
                    return ;
					#endif
                }
                #endif
                rawValue = - (intpart << FRACPART_BITS) - fracpart;
            } else {
            	#if CHECK_FIXED
                if (intpart+((fracpart&INTPART_MASK)>>FRACPART_BITS) > INTPART_MAX) {
                	#if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed string constructure param overflow");
                    #elif FIXED_SATURATING
					rawValue = MAX;
                    return ;
					#endif
                }
                #endif
                rawValue = (intpart << FRACPART_BITS) + fracpart;
            }
            //*/
        }

        public static Fixed Make(tint rawVal) {
            Fixed f = ZERO;
            f.rawValue = rawVal;
            return f;
        }

        public static implicit operator float(Fixed value)
        {
            return (float)((double)value.rawValue / ONE.rawValue);
        }

        public static implicit operator Fixed(float value)
        {
            return new Fixed(value);
        }

        public string ToRawString ()
        {
            return rawValue.ToString ();
        }

        public override string ToString ()
        {
            bool isNegative = false;
            if (rawValue < 0) {
                isNegative = true;
            }
			tint intpart = rawValue/ONE.rawValue;
			tuint fracpart = (tuint)(rawValue&FRACPART_MASK);
            if (isNegative) {
                intpart = -intpart;
                fracpart = ((fracpart ^ FRACPART_MASK) + 1)&FRACPART_MASK;
            }

            StringBuilder str = new StringBuilder ();
            if (isNegative) {
                str.Append ("-");
            }
            str.Append (intpart.ToString ());

            if (fracpart != 0) {
				str.Append ('.');
				tuint bpart = fracpart >> 1;
				tuint cpart = fracpart - bpart;
				#if FIXED32
				const tuint mulby = 10000000000;
				#else //FIXED16 || FIXED4816
				const tuint mulby = 100000;
				#endif
				bpart = (bpart * mulby) >> (FRACPART_BITS-1);
				cpart = (cpart * mulby) >> (FRACPART_BITS-1);
				tuint combin = ((bpart + cpart) + 1) >> 1;
                #if FIXED32
                str.AppendFormat ("{0:0000000000}", combin);
                #else //FIXED16 || FIXED4816
                str.AppendFormat ("{0:00000}", combin);
                #endif
            }
            return str.ToString();
        }

        public static explicit operator Fixed(int value)
        {
            return new Fixed((tint)value*ONE.rawValue);
        }

        public static explicit operator int(Fixed value)
        {
            return (int)(value.rawValue/ONE.rawValue);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue ("Fixed", rawValue);
        }

        public Fixed(SerializationInfo info, StreamingContext context)
        {
            #if FIXED16
                rawValue = info.GetInt32 ("Fixed");
            #elif FIXED4816 || FIXED4816_FAST || FIXED32
                rawValue = info.GetInt64 ("Fixed");
            #endif
        }

        public bool Equals (Fixed other)
        {
            return this.rawValue == other.rawValue;
        }

        public int CompareTo (Fixed other)
        {
            return this.rawValue.CompareTo (other.rawValue);
        }

        public override bool Equals(object obj) 
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (obj is Fixed) {
                return Equals ((Fixed)obj);
            }
            return false;
        }

        public override int GetHashCode() 
        {
            return rawValue.GetHashCode();
        }

        public static Fixed Sign (Fixed val)
        {
            if (val.rawValue < 0)
                return -ONE;
            else if (val.rawValue > 0)
                return ONE;
            return ZERO;
        }

        public static Fixed Abs (Fixed val)
        {
            if (val.rawValue < 0) {
                #if CHECK_FIXED
				if (val.rawValue == MIN.rawValue) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException("Value is too small");   
                    #elif FIXED_SATURATING
                    return MAX;
                    #endif
				}
                #endif
                return -val;
            }
            return val;
        }

        public static Fixed Ceil (Fixed val)
        {
            Fixed result = val;
            if (result.rawValue < 0) {
                result.rawValue = -result.rawValue;
            }
            result.rawValue = result.rawValue & INTPART_MASK;
            if (val.rawValue > 0 && (val.rawValue & FRACPART_MASK) != 0) {
                #if CHECK_FIXED
                if (result.rawValue <= (INTPART_MAX << FRACPART_BITS) - ONE.rawValue) {
                    result.rawValue += ONE.rawValue;
                } else {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed ceil overflow");
                    #elif FIXED_SATURATING
					result.rawValue = MAX.rawValue;
                    #endif
                }
                #else
				result.rawValue += ONE.rawValue;
                #endif
            } else if (val.rawValue < 0) {                
                result.rawValue = -result.rawValue;
            }
            return result;
        }

        public static Fixed Floor (Fixed val)
        {
            Fixed result = val;
            if (result.rawValue < 0) {
                result.rawValue = -result.rawValue;
            }
            result.rawValue = result.rawValue & INTPART_MASK;
            if (val.rawValue < 0 && ((val.rawValue & FRACPART_MASK) != 0)) {
                result.rawValue = -result.rawValue;
                #if CHECK_FIXED
                if (result.rawValue >= (INTPART_MIN << FRACPART_BITS) + ONE.rawValue) {
                    result.rawValue -= ONE.rawValue;
                } else {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed floor overflow");
                    #elif FIXED_SATURATING
					result.rawValue = MIN.rawValue;
                    #endif
                }
                #else
				result.rawValue -= ONE.rawValue;
                #endif
            }
            return result;
        }

        //4 out 5 in, NOT BANKER's ROUNDING
        public static Fixed Round (Fixed val)
        {
            Fixed result = val;
            if (result.rawValue < 0) {
                result.rawValue = -result.rawValue;
            }
            if ((result.rawValue & FRACPART_MASK) >= FRAC_0DOT5) {
                result.rawValue = result.rawValue & INTPART_MASK;
                if (val.rawValue >= 0) {
                    #if CHECK_FIXED
                    if (result.rawValue <= (INTPART_MAX << FRACPART_BITS) - ONE.rawValue) {
                        result.rawValue += ONE.rawValue;
                    } else {
                        #if FIXED_EXCEPTION
                        throw new OverflowException ("Fixed round overflow");
                        #elif FIXED_SATURATING
						result.rawValue = MAX.rawValue;
                        #endif
                    }
                    #else
					result.rawValue += ONE.rawValue;
                    #endif
                    return result;
                }
                //<0
                #if CHECK_FIXED
                result.rawValue = -result.rawValue;
                if (result.rawValue >= (INTPART_MIN << FRACPART_BITS) + ONE.rawValue) {
                    result.rawValue -= ONE.rawValue;
                } else {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed round overflow");
                    #elif FIXED_SATURATING
					result.rawValue = MIN.rawValue;
                    #endif
                }
                #else
				result.rawValue -= ONE.rawValue;
                #endif
                return result;
            } else {
                result.rawValue = result.rawValue & INTPART_MASK;
            }
            return result;
        }

		#if FAST_FIXED
		public static Fixed operator + (Fixed lhs, Fixed rhs)
		{
			return new Fixed (lhs.rawValue + rhs.rawValue);
		}
		#else
        public static Fixed operator + (Fixed lhs, Fixed rhs)
        {
            tint lval = lhs.rawValue;
            tint rval = rhs.rawValue;
            Fixed result;
            result.rawValue = lval + rval;
            #if CHECK_FIXED
            // lhs & rhs are same sign && lhs & result are different sign, then the result was overflow
            if ((((~(lval ^ rval)) & (lval ^ result.rawValue)) & SIGN_MASK) == 0) {
                return result;
            } else {
                #if FIXED_EXCEPTION
                throw new OverflowException ("Fixed add overflow");
                #elif FIXED_SATURATING
				// overflow
				if (lval > 0) {
					return MAX;
				}
				return MIN;
                #endif
            }
            #else
			return result;
            #endif
        }
		#endif


        public static Fixed operator - (Fixed val)
        {
			return new Fixed(-val.rawValue);
        }

		#if FAST_FIXED
		public static Fixed operator - (Fixed lhs, Fixed rhs)
		{
			return new Fixed (lhs.rawValue - rhs.rawValue);
		}
		#else
        public static Fixed operator - (Fixed lhs, Fixed rhs)
        {
            tint lval = lhs.rawValue;
            tint rval = rhs.rawValue;
            Fixed result;
            result.rawValue = lval - rval;
            // lhs & rhs are different sign && lhs & result are different sign,  then the result was overflow
            if ((((lval ^ rval) & (lval ^ result.rawValue)) & SIGN_MASK) == 0) {
                return result;
            }
            #if (FIXED_EXCEPTION)
            throw new OverflowException ("Fixed sub overflow");
            #else
			// overflow
			if (lval > 0) {
				return MAX;
			}
			return MIN;
            #endif
        }
		#endif


        #if FIXED16
        //1000w times 325ms
        public static Fixed Mul (Fixed lhs, Fixed rhs)
        {
            Int64 val64 = (Int64)lhs.rawValue * rhs.rawValue;
            Int32 high17bits;
            if (val64 < 0) {
                val64 += unchecked((Int64)(-0x8000)); // -1/2
                #if CHECK_FIXED
                high17bits = (Int32)(val64 >> 47);
                if ((~high17bits) != 0) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed mul overflow");
                    #elif FIXED_SATURATING
                    return MIN; 
                    #endif
                }
                #endif
            } else {
                val64 += (tint)(0x8000); //1/2/
                #if CHECK_FIXED
                high17bits = (Int32)(val64 >> 47);
                if (high17bits != 0) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed mul overflow");
                    #elif FIXED_SATURATING
                    return MAX;
                    #endif
                }
                #endif
            }
            return new Fixed((tint)(val64 / 65536));
        }
        #endif

		#if FAST_FIXED && FIXED4816 
		//只有48.16和FAST_FIXED同时开启才用这个最简单的加速乘法
		public static Fixed operator * (Fixed lhs, Fixed rhs)
		{
			return new Fixed ((lhs.rawValue * rhs.rawValue) >> FRACPART_BITS);
		}
		#else
        //1000w time 97ms
        public static Fixed operator * (Fixed lhs, Fixed rhs)
        {
            bool resultIsNeg = false;
            if (((lhs.rawValue ^ rhs.rawValue) & SIGN_MASK) != 0) {
                resultIsNeg = true;
            }

            tuint left = (tuint)(lhs.rawValue < 0 ? -lhs.rawValue : lhs.rawValue);
            tuint right = (tuint)(rhs.rawValue < 0 ? -rhs.rawValue : rhs.rawValue);

			tuint lint = (tuint)((left & unchecked((tuint)INTPART_MASK)) >> FRACPART_BITS);
			tuint rint = (tuint)((right & unchecked((tuint)INTPART_MASK)) >> FRACPART_BITS);

            tuint lfrac = left & FRACPART_MASK;
            tuint rfrac = right & FRACPART_MASK;

            tuint lfracMulRfrac = (lfrac * rfrac + ((tuint)1 << (FRACPART_BITS - 1))) >> FRACPART_BITS;

            tuint lintMulRfrac = (lint * rfrac);
            tuint RintMulLFrac = (rint * lfrac);

            tuint RintMulLint = rint * lint;
            //check int part mul overflow
            Fixed result;
            result.rawValue = (((tint)RintMulLint << FRACPART_BITS) | ((tint)lfracMulRfrac & FRACPART_MASK));
            if (resultIsNeg) {                
                #if CHECK_FIXED
		        if (result.rawValue<0 || (-result.rawValue < (INTPART_MIN << FRACPART_BITS) + (tint)lintMulRfrac + (tint)RintMulLFrac)) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed mul overflow");
                    #elif FIXED_SATURATING
					return MIN; //down overflow 
                    #endif
                }
                #endif
                result.rawValue += (tint)(lintMulRfrac + RintMulLFrac);
                result.rawValue = -result.rawValue;
            } else {
                #if CHECK_FIXED
		        if (result.rawValue<0 || (result.rawValue > (INTPART_MAX << FRACPART_BITS) - (tint)lintMulRfrac - (tint)RintMulLFrac)) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed mul overflow");
                    #elif FIXED_SATURATING
					return MAX; //up overflow
                    #endif
                }
                #endif
                result.rawValue += (tint)(lintMulRfrac + RintMulLFrac);
            }
            return result;
        }
		#endif

        private static int CountLeadingZeros (tuint value)
        {
            if (value == 0) {
                return WHOLE_BITS;
            }
            int zeroCount = 0;
            tuint mask = (tuint)0xF << (WHOLE_BITS - 4);
            while ((value & mask) == 0) {
                value <<= 4;
                zeroCount += 4;
            }
            mask = (tuint)0x1 << (WHOLE_BITS - 1);
            while ((value & mask) == 0) {
                value <<= 1;
                zeroCount += 1;
            }
            return zeroCount;
        }

		#if FAST_FIXED && FIXED4816
		public static Fixed operator / (Fixed lhs, Fixed rhs)
		{
			return new Fixed (lhs.rawValue * ONE.rawValue / rhs.rawValue);
		}
		#else
        // 1000w times 305ms
        public static Fixed operator / (Fixed lhs, Fixed rhs)
        {
            bool resultIsNeg = false;
            if (((lhs.rawValue ^ rhs.rawValue) & SIGN_MASK) != 0) {
                resultIsNeg = true;
            }

            if (rhs.rawValue == 0) {
                #if FIXED_EXCEPTION
                throw new DivideByZeroException ("Fixed divide by zero");
                #else
                return MAX;
                #endif
            }

            Fixed result;
            if (lhs.rawValue == 0) {
                result.rawValue = 0;
                return result;
            }

            tuint r = (tuint)(lhs.rawValue < 0 ? -lhs.rawValue : lhs.rawValue);
            tuint d = (tuint)(rhs.rawValue < 0 ? -rhs.rawValue : rhs.rawValue);
            tuint q = 0;

            #if FIXED16
            if ((d & 0xFFF00000) != 0) {
                tuint td = (d >> 17) + 1;
                q = r / td;
                r -= (tuint)(((UInt64)q * d) >> 17);
            }
            #endif

            // 把D右边的0移出去
            #if FIXED16 || FIXED4816
            int bitPos = 17;
            #else
            int bitPos = 33;
            #endif
            while (bitPos >= 4 && (d & 0xF) == 0) {
                d >>= 4;
                bitPos -= 4;
            }

            while (bitPos >= 1 && (d & 0x1) == 0) {
                d >>= 1;
                --bitPos;
            }


            while (r != 0 && bitPos >= 0) {
                // 计算R的最大左移位数
                int shiftBits = CountLeadingZeros (r);
                if (shiftBits > bitPos) {
                    shiftBits = bitPos;
                }
                r <<= shiftBits;
                bitPos -= shiftBits;

                tuint div = r / d;
                r = r % d;
                q += div << bitPos;

                #if CHECK_FIXED
                if ((div & ~ ((unchecked((tuint)(-1))) >> bitPos)) != 0) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed div overflow");
                    #elif FIXED_SATURATING
                    return MAX;
                    #endif
                }
                #endif

                r <<= 1;
                --bitPos;
            }

            ++q;
            result.rawValue = (tint)(q >> 1);

            if (resultIsNeg) {
                //#if CHECK_FIXED
                //if (result.rawValue == MIN.rawValue) {
                //    #if FIXED_EXCEPTION
                //    throw new OverflowException ("Fixed div overflow");
                //    #elif FIXED_SATURATING
                //    return MIN;
                //    #endif
                //}
                //#endif
                result.rawValue = -result.rawValue;
            }
            return result;
        }
		#endif

        // 1000w times, 351ms
        public static Fixed Div (Fixed lhs, Fixed rhs)
        {
            bool resultIsNeg = false;

            if (((lhs.rawValue ^ rhs.rawValue) & SIGN_MASK) != 0) {
                resultIsNeg = true;
            }

            if (rhs.rawValue == 0) {
                #if FIXED_EXCEPTION
                throw new DivideByZeroException ("Fixed divide by zero");
                #else
                return MAX;
                #endif
            }

            if (lhs.rawValue == 0) {
                return new Fixed ();
            }

            tuint r = (tuint)(lhs.rawValue < 0 ? -lhs.rawValue : lhs.rawValue);
            tuint d = (tuint)(rhs.rawValue < 0 ? -rhs.rawValue : rhs.rawValue);
            tuint q = 0;
            tuint bit = (tuint)1<<FRACPART_BITS;

            while (d < r) {
                d <<= 1;
                bit <<= 1;
            }
            #if CHECK_FIXED
            if (bit == 0) {
                #if FIXED_EXCEPTION
                throw new OverflowException ("Fixed div overflow");
                #elif FIXED_SATURATING
                return MAX;
                #endif
            }
            #endif
    
            if ((d & ((tuint)1 << (WHOLE_BITS - 1))) != 0) {
                if (r >= d) {
                    q |= bit;
                    r -= d;
                }
                d >>= 1;
                bit >>= 1;
            }

            while (bit != 0 && r != 0) {
                if (r >= d) {
                    q |= bit;
                    r -= d;
                }
                r <<= 1;
                bit >>= 1;
            }

            //for round
            if (r >= d) {
                ++q;
            }

            Fixed result;
            result.rawValue = (tint)q;
            if (resultIsNeg) {
                #if CHECK_FIXED
                if (result.rawValue == MIN.rawValue) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException ("Fixed div overflow");
                    #elif FIXED_SATURATING
                    return MIN;
                    #endif
                }
                #endif
                result.rawValue = -result.rawValue;
            }

            return result;
        }

        public static bool operator > (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue > rhs.rawValue;
        }

        public static bool operator >= (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue >= rhs.rawValue;
        }

        public static bool operator < (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue < rhs.rawValue;   
        }

        public static bool operator <= (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue <= rhs.rawValue;
        }

        public static bool operator == (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue == rhs.rawValue;
        }

        public static bool operator != (Fixed lhs, Fixed rhs)
        {
            return lhs.rawValue != rhs.rawValue;
        }

        public static Fixed operator ++ (Fixed val)
        {
            return val + ONE;
        }

        public static Fixed operator -- (Fixed val)
        {    
            return val - ONE;
        }

        public static Fixed operator % (Fixed lhs, Fixed rhs)
        {
            return new Fixed (lhs.rawValue % rhs.rawValue);
        }

        public static Fixed operator >> (Fixed val, int shift)
        {
            return new Fixed (val.rawValue >> shift);
        }

        public static Fixed operator << (Fixed val, int shift)
        {            
            return new Fixed (val.rawValue << shift);
        }

		public static Fixed Clamp(Fixed value, Fixed min, Fixed max)
		{
			return (value < min) ? min : ((value > max) ? max : value);
		}


		#if FIXED16 || FIXED4816
		public static readonly Fixed FOUR_DIV_PI = new Fixed (83443);
		public static readonly Fixed FOUR_DIV_PI_DIV_PI = new Fixed (26561);
		public static readonly Fixed DOT_225 = new Fixed (14746);
		#elif FIXED32
		public static readonly Fixed FOUR_DIV_PI = new Fixed (5468522205);
		public static readonly Fixed FOUR_DIV_PI_DIV_PI = new Fixed (1740684681);
		public static readonly Fixed DOT_225 = new Fixed (966367642);
		#endif

		// http://lab.polygonal.de/wp-content/assets/070718/fastTrig.as
		public static Fixed Sin(Fixed x)
		{
			while (x < -PI) { x += PI_MUL_2; }
			while (x > PI) { x -= PI_MUL_2; }

			Fixed result;

			if (x < ZERO) {
				result = x * (FOUR_DIV_PI + FOUR_DIV_PI_DIV_PI * x);
			} else {
				result = x * (FOUR_DIV_PI - FOUR_DIV_PI_DIV_PI * x);
			}

			if (result < ZERO) {
				result *= (DOT_225 * (result + ONE) + ONE);
			} else {
				result *= (DOT_225 * (result - ONE) + ONE);
			}

			return Clamp (result, -ONE, ONE);
		}
	/*
		// sin : use lookup table
        public static Fixed SinLut(Fixed radian)
        {
            tint realRad = radian.rawValue % PI_MUL_2.rawValue; // (-2PI, 2PI)

            if (realRad < 0) {
                realRad += PI_MUL_2.rawValue;
            }
            if (realRad >= PI.rawValue) {
                realRad -= PI.rawValue;
                if (realRad >= PI_DIV_2.rawValue) {
                    realRad = PI.rawValue - realRad;
                }
                tint idx = realRad * SIN_LUT_SIZE / PI_DIV_2.rawValue;
                if (idx >= SIN_LUT_SIZE) {
                    return new Fixed (-ONE.rawValue);
                }
                return new Fixed (-sinLut [idx]);
            } else {
                if (realRad >= PI_DIV_2.rawValue) {
                    realRad = PI.rawValue - realRad;
                }
                tint idx = realRad * SIN_LUT_SIZE / PI_DIV_2.rawValue;
                if (idx >= SIN_LUT_SIZE)
                    return ONE;
                return new Fixed (sinLut [idx]);
            }
        }

		public static Fixed CosLut(Fixed radian)
		{
			return SinLut (radian + PI_DIV_2);
		}

		public static Fixed TanLut(Fixed radian)
		{
			Fixed divby = CosLut (radian);
			if (divby.rawValue != 0) {
				return new Fixed(SinLut (radian) / divby);
			} else {
				tint realRad = radian.rawValue % PI.rawValue;
				if (realRad < -PI_DIV_2.rawValue) {
					realRad += PI.rawValue;
				} else if (realRad > PI_DIV_2.rawValue) {
					realRad -= PI.rawValue;
				}
				if (realRad < 0) {
					return MIN;
				}
				return MAX;
			}
		}
		*/

        public static Fixed Cos(Fixed radian)
        {
            return Sin (radian + PI_DIV_2);
        }


        /*
		public static Fixed Tan(Fixed radian)
        {
            tint realRad = radian.rawValue % PI.rawValue; // (-PI, PI)

            //(-PI/2, PI/2)
            if (realRad < -PI_DIV_2.rawValue) {
                realRad += PI.rawValue;
            } else if (realRad > PI_DIV_2.rawValue) {
                realRad -= PI.rawValue;
            }

            Fixed result;

            if (realRad < 0) {
                realRad = -realRad;
                int idx = realRad * TAN_LUT_SIZE / PI_DIV_2.rawValue;
                if (idx >= TAN_LUT_SIZE) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException("Fixed::Tan radian out of range");
                    #else
                    return MIN;
                    #endif
                }
                result.rawValue = -tanLut [idx];
            } else {
                int idx = realRad * TAN_LUT_SIZE / PI_DIV_2.rawValue;
                if (idx >= TAN_LUT_SIZE) {
                    #if FIXED_EXCEPTION
                    throw new OverflowException("Fixed::Tan radian out of range");
                    #else
                    return MAX;
                    #endif
                }
                result.rawValue = tanLut [idx];
            }
            return result;
        }
        */

        public static Fixed Tan(Fixed radian)
        {
            Fixed divby = Cos (radian);
            if (divby.rawValue != 0) {
                return new Fixed(Sin (radian) / divby);
            } else {
                tint realRad = radian.rawValue % PI.rawValue;
                if (realRad < -PI_DIV_2.rawValue) {
                    realRad += PI.rawValue;
                } else if (realRad > PI_DIV_2.rawValue) {
                    realRad -= PI.rawValue;
                }
                if (realRad < 0) {
                    return MIN;
                }
                return MAX;
            }
        }


        /*
        //https://codingforspeed.com/using-faster-exponential-approximation/
        public static Fixed Exp1(Fixed x)
        {
            Fixed val;
            Fixed divby = new Fixed (256, 0);
            val = ONE + x / divby;
            for (int i = 0; i < 8; ++i) {
                val *= val;
            }
            return val;
        }

        //https://codingforspeed.com/using-faster-exponential-approximation/
        public static Fixed Exp2(Fixed x)
        {
            Fixed val;
            Fixed divby = new Fixed (1024, 0);
            val = ONE + x / divby;
            for (int i = 0; i < 10; ++i) {
                val *= val;
            }
            return val;
        }
        */

        
		#if FIXED16
		//expValMax -> (M.N) : log((1<<(M-1))-1)*(1<<N)
		private const tint expValMax = 681389; 
		//expValMin -> -log(ONE)*ONE
		private const tint expValMin = -726817; 
        private const int expIter1Nums = 5;
        private const int expIter2Nums = 15;
		#elif FIXED4816
		private const tint expValMax = 2135026; 
		private const tint expValMin = -726817; 
        private const int expIter1Nums = 5;
        private const int expIter2Nums = 15;
		#elif FIXED32
		private const tint expValMax = 92288378624; 
		private const tint expValMin = -95265423098; 
        private const int expIter1Nums = 6;
        private const int expIter2Nums = 32;
		#endif

        #if FIXED16 || FIXED4816
        // 16*LN2, 8*LN2, 4*LN2, 2*LN2, LN2
        //private static tint[] c1 = new tint[] {726817, 363408, 181704, 90852, 45426}; //no round
        private static tint[] c1 = new tint[] {726817,363409,181704,90852,45426};
        // log(1+1/2), log(1+1/4), log(1+1/8)... , log(1+1/32768)
        //private static tint[] c2 = new tint[] {26572, 14623, 7719, 3973, 2016, 1016,510, 256, 128, 64, 32, 16, 8, 4, 2};//no round
        private static tint[] c2 = new tint[] {26573,14624,7719,3973,2017,1016,510,256,128,64,32,16,8,4,2};
        #elif FIXED32
        //private static tint[] c1 = new tint[] {47632711549, 23816355774 , 11908177887, 5954088943 , 2977044471 };
        private static tint[] c1 = new tint[] {95265423098,47632711549,23816355775,11908177887,5954088944,2977044472};
        private static tint[] c2 = new tint[32] {1741459379,958394255,505874286,260380768,132163268,66589974,33424039
        ,16744533,8380427,4192257,2096640,1048448,524256,262136,131070
        ,65536,32768,16384,8192,4096,2048,1024,512,256,128,64,32,16,8,4,2,1};
        #endif

        public static Fixed Exp(Fixed x)
        {
            if (x.rawValue == 0) {
                return ONE;
            }
            if (x.rawValue == ONE.rawValue) {
                return E;
            }
			if (x.rawValue > expValMax) {
				return MAX;
			}
			if (x.rawValue < expValMin) {
				return new Fixed (0);
			}           
            tuint v = (tuint)ONE.rawValue;
            tint s = x.rawValue;
            if (s < 0) {
                s = -s;
            }

            int bitPos = 1 << (expIter1Nums-1);
            for (int i = 0; i < expIter1Nums; ++i) {
                if (s >= c1 [i]) {
                    s -= c1 [i];
                    v <<= bitPos;
                }
                bitPos >>= 1;
            }

            for (int i = 0; i < expIter2Nums; ++i) {
                if (s >= c2 [i]) {
                    s -= c2 [i];
                    v += (v >> (i + 1));
                }
            }

            if (x.rawValue >= 0) {
                return new Fixed ((tint)v);
            } else {                
                Fixed result = ONE / new Fixed ((tint)v);
                return result;
            }
        }
 
		/**
		 *  Fixed point square root function
	     *  Computing the square root of an integer or a fixed point into a fixed point integer. A fixed point is a 32 bit value with the comma between the bits 15 and 16, where bit 0 is the less significant bit of the value.
    	 *  The algorithms can be easily extended to 64bit integers, or different fixed point comma positions.
  	     *  Algorithm and code Author: Christophe Meessen 1993. Initially published in: usenet comp.lang.c, Thu, 28 Jan 1993 08:35:23 GMT, Subject: Fixed point sqrt by: Meessen Christophe 
  	     *  https://github.com/chmike/fpsqrt/blob/master/README.md
		**/
        #if FIXED16
		private const tuint sqrtB = 0x40000000;
		private const tuint sqrtBE = 0x40;
		private const int qShift = 8;
		#elif FIXED32 
		private const tuint sqrtB = 0x4000000000000000;
		private const tuint sqrtBE = 0x4000;
		private const int qShift = 16;
		#elif FIXED4816
		private const tuint sqrtB = 0x4000000000000000;
		private const tuint sqrtBE = 0x400000;
		private const int qShift = 24;
		#endif
        
		public static Fixed Sqrt(Fixed val)
        {
            if (val.rawValue < 0) {
                throw new ArithmeticException ("sqrt(x), x was negative!");
            }           
            tuint r = (tuint)val.rawValue;
            tuint b = sqrtB;
            tuint q = 0;
			while (b > sqrtBE) {
				tuint t = q + b;
				if (r >= t) {
					r -= t;
					q = t + b; // equivalent to q += 2*b
				}
				r <<= 1;
				b >>= 1;
			}
			q >>= qShift;
            return new Fixed((tint)q);
        }


        public static Fixed Asin(Fixed x)
        {
            if (x > ONE || x < -ONE) {
                return ZERO;
            } else if (x == ONE) {
                return PI_DIV_2;
            } else if (x == -ONE) {
                return -PI_DIV_2;
            }
            
            Fixed temp;
            temp = (ONE - x * x);
            temp = x / temp;
            temp = Atan (temp);
            return temp;
        }

        public static Fixed Acos(Fixed x)
        {
            return ((PI_DIV_2) - Asin (x));
        }

		#if FIXED16 || FIXED4816
		// theta[i] = floor((atan(1 / pow(2, i)) * 2 ^ BITS);
		private static readonly tint[] theta = new tint[16] {51471, 30385, 16054, 8149, 4090, 2047, 1023, 511, 255, 127, 63, 31, 15, 7, 3, 1};
		#elif FIXED32
		private static readonly tint[] theta = new tint[16] {3373259426, 1991351317, 1052175346, 534100634, 268086747, 134174062, 67103403, 33553749
		, 16777130, 8388597, 4194302, 2097151, 1048575, 524287, 262143, 131071};
		#endif
		private static tint[] absX = new tint[16];
		private static tint[] absY = new tint[16];

		//cordic algorithm for atan2
		public static Fixed Atan2(Fixed y, Fixed x)
		{
			Fixed result = new Fixed ();
			if (x == ZERO || y == ZERO) {
				if (x == ZERO) {
					if (y > ZERO) {
						return PI_DIV_2;
					} else if (y < ZERO) {
						return -PI_DIV_2;
					}
					return result;
				}
				if (y == ZERO) {
					if (x >= ZERO) {
						return result;
					} else {
						return PI;
					}
				}
			} else {
                absX [0] = x.rawValue < 0 ? -x.rawValue : x.rawValue;
                absY [0] = y.rawValue < 0 ? -y.rawValue : y.rawValue;
				const int iterNums = 16;
				for (int i = 0; i < iterNums-1; ++i) {
                    if (absY [i] > 0) {
                        result.rawValue = result.rawValue + theta [i];
                        absX [i + 1] = absX [i] + (absY [i] >> i);
                        absY [i + 1] = absY [i] - (absX [i] >> i);
                    } else if (absY [i] < 0) {
                        result.rawValue = result.rawValue - theta [i];
                        absX [i + 1] = absX [i] - (absY [i] >> i);
                        absY [i + 1] = absY [i] + (absX [i] >> i);
                    } else {
                        break;
                    }
				}
			}
			if (x > ZERO && y < ZERO) {
				result.rawValue = -result.rawValue;
			} else if (x < ZERO) {
				if (y > ZERO) {
					result.rawValue = PI.rawValue - result.rawValue;
				} else if (y < ZERO) {
					result.rawValue = result.rawValue - PI.rawValue;
				}
			}
			return result;
		}

        public static Fixed Atan(Fixed x)
        {
            return Atan2 (x, ONE);
        }


        #if FIXED32 || FIXED4816 || FIXED4816_FAST
        private const tuint FIXED_RAND_A = 6364136223846793005;
        private const tuint FIXED_RAND_C = 1442695040888963407;
        #elif FIXED16
        private const tuint FIXED_RAND_A = 1664525;
        private const tuint FIXED_RAND_C = 1013904223;
        #endif

        private static tuint randVal = 0;

        private static tuint GenRandom()
        {
            tuint cur = randVal * FIXED_RAND_A + FIXED_RAND_C;
            randVal = cur;
            return cur;
        }

        private static Fixed GenRandomFixed()
        {            
            return new Fixed ((tint)GenRandom ());
        }

        public static void SRand(Fixed seed)
        {
            randVal = (tuint)seed.rawValue;
        }

        public static Fixed RandomRange(Fixed min, Fixed max)
        {
            tuint val = GenRandom ();
            tint rand = (tint)((tuint)min.rawValue + (val % (tuint)(max.rawValue - min.rawValue)));
            if (rand < min.rawValue) {
                rand = min.rawValue;
            }
            if (rand > max.rawValue) {
                rand = max.rawValue;
            }
            return new Fixed (rand);
        }

        public static Fixed RandomZeroOne()
        {
            return RandomRange (ZERO, ONE);
        }

        //生成sin函数速查表（本函数应在编辑器里调用）
        public static tint [] GenSinLut ()
        {
            tint[] sinLutData = new tint [SIN_LUT_SIZE];
            for (int i = 0; i < SIN_LUT_SIZE; ++i) {
                decimal rad = (decimal)i * 3.14159265358979323846m / (2 * (SIN_LUT_SIZE - 1));//[0,pi/2]
                Fixed val = new Fixed(System.Math.Sin ((double)rad));
                sinLutData [i] = val.rawValue;
            }
            return sinLutData;
        }
        /*
        //生成tan函数速查表（本函数应在编辑器里调用）
        //tan函数值域较大，后面和sin分开table大小，tan表的table给大一些
        public static tint[] GenTanLut()
        {
            tint[] tanLutData = new tint [TAN_LUT_SIZE];
            for (int i = 0; i < TAN_LUT_SIZE; ++i) {
                decimal rad = (decimal)i * 3.14159265358979323846m / (2 * (TAN_LUT_SIZE));//[0,pi/2]
                Fixed val = new Fixed(System.Math.Tan ((double)rad));
                tanLutData [i] = val.rawValue;
            }
            return tanLutData;
        }
        */

        //生成到文件
        public static void GenTrigLutToFile (string filePath, string tableName, tint[] array, int size)
        {
            System.IO.FileStream fs = new System.IO.FileStream (filePath, System.IO.FileMode.Create);
            //获得字节数组
            string fileHeadLine =
                                #if FIXED32
                                "using tint = System.Int64;\n"+
                                #else //default it's Fixed32
                                "using tint = System.Int32;\n" +
                                #endif
                                "namespace zf.core {\n" +
                                #if FIXED16
                                "\tpublic partial struct Fixed {\n" +
                                "\t\t#if FIXED16\n"+
                                #else
                                "public partial struct Fixed {\n" +
                                "\t\t#if FIXED32\n"+
                                #endif
                                "\t\tpublic static readonly tint[] " +
                                tableName +
                                " = new tint[] {\n\t\t\t";

            string fileTailLine = "\n\t\t}; //end of lookup table\n" +
                                  "\t\t#endif //end of FIXED32 or FIXED16\n"+
                                  "\t} //end of struct Fixed\n" +
                                  "} //end of namespace\n";

            Encoding encoding = Encoding.UTF8;
            //write head
            byte[] fileData = encoding.GetBytes (fileHeadLine);
            fs.Write (fileData, 0, fileData.Length);

            string lineEnding = "\n\t\t\t";
            byte[] lineEndingData = encoding.GetBytes (lineEnding);
            for (int i = 0; i < size;) {
				#if FIXED16 || FIXED4816
                string lineData = String.Format ("0x{0,-8:X}, ", array [i]);
                #elif FIXED32 
                string lineData = String.Format ("0x{0,-16:X}, ", array [i]);
                #endif
                fileData = Encoding.UTF8.GetBytes (lineData);
                fs.Write (fileData, 0, fileData.Length);
                ++i;
                if (i % 8 == 0) { //每8个数一行
                    fs.Write (lineEndingData, 0, lineEndingData.Length);
                }
            }

            //write tail
            fileData = Encoding.UTF8.GetBytes (fileTailLine);
            fs.Write (fileData, 0, fileData.Length);

            fs.Flush ();
            fs.Close ();
        }
    }
    #endif
}

