using System;
using System.Text;

//using System.Security.Cryptography;
namespace RockBall.Crypto
{
	public class DH64
	{
		private const ulong p = 0xffffffffffffffc5;
		private const ulong g = 5;

		private static ulong mul_mod_p(ulong a, ulong b) {
#if false
            var tdes = new DESCryptoServiceProvider
            {
                Key = key,
                IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            var cTransform = tdes.CreateEncryptor();
#endif
            ulong m = 0;
			while (b > 0) {
				if ((b&1) > 0) {
					var t = p - a;
					if (m >= t) {
						m -= t;
					} else {
						m += a;
					}
				}
				if (a >= p-a) {
					a = a*2 - p;
				} else {
					a = a * 2;
				}
				b >>= 1;
			}
			return m;
		}

		private static ulong pow_mod_p(ulong a, ulong b) {
			if (b == 1) {
				return a;
			}
			var t = pow_mod_p(a, b>>1);
			t = mul_mod_p(t, t);
			if ((b%2) > 0) {
				t = mul_mod_p(t, a);
			}
			return t;
		}

		private static ulong powmodp(ulong a , ulong b) {
			if (a == 0) {
				throw new Exception("DH64 zero public key");
			}
			if (b == 0) {
				throw new Exception("DH64 zero private key");
			}
			if (a > p) {
				a %= p;
			}
			return pow_mod_p(a, b);
		}

		private Random rand;

		public DH64() {
			rand = new Random();
		}

		public void KeyPair(out ulong privateKey, out ulong publicKey) {
			var a = (ulong)rand.Next();
			var b = (ulong)rand.Next() + 1;
			privateKey = (a<<32) | b;
			publicKey = PublicKey(privateKey);
		}

		public ulong PublicKey(ulong privateKey) {
			return powmodp(g, privateKey);
		}

		public static ulong Secret(ulong privateKey, ulong anotherPublicKey) {
			return powmodp(anotherPublicKey, privateKey);
		}
	}

    public class HMac64
    {

        // Constants are the integer part of the sines of integers (in radians) * 2^32.
        private static readonly UInt32[] k = {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee ,
            0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501 ,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be ,
            0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821 ,
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa ,
            0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8 ,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed ,
            0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a ,
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c ,
            0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70 ,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05 ,
            0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665 ,
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039 ,
            0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1 ,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1 ,
            0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
        };

        // r specifies the per-round shift amounts
        private static readonly int[] r = {
            7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
            5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
            4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
            6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21
        };

        /*
         // Note: if you're in a checked context by default, you'll want to make this
        // explicitly unchecked
            uint u1 = (uint) int1;
            uint u2 = (uint) int2;

            ulong unsignedKey = (((ulong) u1) << 32) | u2;
            long key = (long) unsignedKey;
        And to reverse:

            ulong unsignedKey = (long) key;
            uint lowBits = (uint) (unsignedKey & 0xffffffffUL);
            uint highBits = (uint) (unsignedKey >> 32);
            int i1 = (int) highBits;
            int i2 = (int) lowBits;
        */

        // leftrotate function definition
        private static UInt32 LEFTROTATE(UInt32 x, int c)
        {
            return (x << c) | (x >> (32 - c));
        }
//#define LEFTROTATE(x, c) (((x) << (c)) | ((x) >> (32 - (c))))

        public static UInt64
        hmac(UInt64 text, UInt64 secret)
         //   hmac(UInt32 x[2], UInt32 y[2], UInt32 result[2])
        {
            UInt32[] w = new UInt32[16];
            UInt32 a, b, c, d, f, g, temp;
            //int i;

            a = 0x67452301u;
            b = 0xefcdab89u;
            c = 0x98badcfeu;
            d = 0x10325476u;

            for (uint i = 0; i < 16; i += 4)
            {
                w[i] = (UInt32)(text >> 32);
                w[i + 1] = (UInt32)(text & 0xffffffffUL);
                w[i + 2] = (UInt32)(secret >> 32);
                w[i + 3] = (UInt32)(secret & 0xffffffffUL);
                //                w[i] = x[1];
                //                w[i + 1] = x[0];
                //                w[i + 2] = y[1];
                //                w[i + 3] = y[0];
            }

            for (uint i = 0; i < 64; i++)
            {
                if (i < 16)
                {
                    f = (b & c) | ((~b) & d);
                    g = i;
                }
                else if (i < 32)
                {
                    f = (d & b) | ((~d) & c);
                    g = (5 * i + 1) % 16;
                }
                else if (i < 48)
                {
                    f = b ^ c ^ d;
                    g = (3 * i + 5) % 16;
                }
                else
                {
                    f = c ^ (b | (~d));
                    g = (7 * i) % 16;
                }

                temp = d;
                d = c;
                c = b;
                b = b + LEFTROTATE((a + f + k[i] + w[g]), r[i]);
                a = temp;

            }
            UInt64 resultText;
      
            resultText = ((UInt64)(a ^ b) << 32) | ((UInt64)(c ^ d));
            return resultText;
            //result[0] = c ^ d;
            //result[1] = a ^ b;
        }
    }

    public class DesCrypt
    {
        /* the eight DES S-boxes */

        private static UInt32[] SB1 = new UInt32[]
        {
            0x01010400, 0x00000000, 0x00010000, 0x01010404,
            0x01010004, 0x00010404, 0x00000004, 0x00010000,
            0x00000400, 0x01010400, 0x01010404, 0x00000400,
            0x01000404, 0x01010004, 0x01000000, 0x00000004,
            0x00000404, 0x01000400, 0x01000400, 0x00010400,
            0x00010400, 0x01010000, 0x01010000, 0x01000404,
            0x00010004, 0x01000004, 0x01000004, 0x00010004,
            0x00000000, 0x00000404, 0x00010404, 0x01000000,
            0x00010000, 0x01010404, 0x00000004, 0x01010000,
            0x01010400, 0x01000000, 0x01000000, 0x00000400,
            0x01010004, 0x00010000, 0x00010400, 0x01000004,
            0x00000400, 0x00000004, 0x01000404, 0x00010404,
            0x01010404, 0x00010004, 0x01010000, 0x01000404,
            0x01000004, 0x00000404, 0x00010404, 0x01010400,
            0x00000404, 0x01000400, 0x01000400, 0x00000000,
            0x00010004, 0x00010400, 0x00000000, 0x01010004
        };

        private static UInt32[] SB2 = new UInt32[]
        {
            0x80108020, 0x80008000, 0x00008000, 0x00108020,
            0x00100000, 0x00000020, 0x80100020, 0x80008020,
            0x80000020, 0x80108020, 0x80108000, 0x80000000,
            0x80008000, 0x00100000, 0x00000020, 0x80100020,
            0x00108000, 0x00100020, 0x80008020, 0x00000000,
            0x80000000, 0x00008000, 0x00108020, 0x80100000,
            0x00100020, 0x80000020, 0x00000000, 0x00108000,
            0x00008020, 0x80108000, 0x80100000, 0x00008020,
            0x00000000, 0x00108020, 0x80100020, 0x00100000,
            0x80008020, 0x80100000, 0x80108000, 0x00008000,
            0x80100000, 0x80008000, 0x00000020, 0x80108020,
            0x00108020, 0x00000020, 0x00008000, 0x80000000,
            0x00008020, 0x80108000, 0x00100000, 0x80000020,
            0x00100020, 0x80008020, 0x80000020, 0x00100020,
            0x00108000, 0x00000000, 0x80008000, 0x00008020,
            0x80000000, 0x80100020, 0x80108020, 0x00108000
        };

        private static UInt32[] SB3 = new UInt32[]
        {
            0x00000208, 0x08020200, 0x00000000, 0x08020008,
            0x08000200, 0x00000000, 0x00020208, 0x08000200,
            0x00020008, 0x08000008, 0x08000008, 0x00020000,
            0x08020208, 0x00020008, 0x08020000, 0x00000208,
            0x08000000, 0x00000008, 0x08020200, 0x00000200,
            0x00020200, 0x08020000, 0x08020008, 0x00020208,
            0x08000208, 0x00020200, 0x00020000, 0x08000208,
            0x00000008, 0x08020208, 0x00000200, 0x08000000,
            0x08020200, 0x08000000, 0x00020008, 0x00000208,
            0x00020000, 0x08020200, 0x08000200, 0x00000000,
            0x00000200, 0x00020008, 0x08020208, 0x08000200,
            0x08000008, 0x00000200, 0x00000000, 0x08020008,
            0x08000208, 0x00020000, 0x08000000, 0x08020208,
            0x00000008, 0x00020208, 0x00020200, 0x08000008,
            0x08020000, 0x08000208, 0x00000208, 0x08020000,
            0x00020208, 0x00000008, 0x08020008, 0x00020200
        };

        private static UInt32[] SB4 = new UInt32[]
        {
            0x00802001, 0x00002081, 0x00002081, 0x00000080,
            0x00802080, 0x00800081, 0x00800001, 0x00002001,
            0x00000000, 0x00802000, 0x00802000, 0x00802081,
            0x00000081, 0x00000000, 0x00800080, 0x00800001,
            0x00000001, 0x00002000, 0x00800000, 0x00802001,
            0x00000080, 0x00800000, 0x00002001, 0x00002080,
            0x00800081, 0x00000001, 0x00002080, 0x00800080,
            0x00002000, 0x00802080, 0x00802081, 0x00000081,
            0x00800080, 0x00800001, 0x00802000, 0x00802081,
            0x00000081, 0x00000000, 0x00000000, 0x00802000,
            0x00002080, 0x00800080, 0x00800081, 0x00000001,
            0x00802001, 0x00002081, 0x00002081, 0x00000080,
            0x00802081, 0x00000081, 0x00000001, 0x00002000,
            0x00800001, 0x00002001, 0x00802080, 0x00800081,
            0x00002001, 0x00002080, 0x00800000, 0x00802001,
            0x00000080, 0x00800000, 0x00002000, 0x00802080
        };

        private static UInt32[] SB5 = new UInt32[]
        {
            0x00000100, 0x02080100, 0x02080000, 0x42000100,
            0x00080000, 0x00000100, 0x40000000, 0x02080000,
            0x40080100, 0x00080000, 0x02000100, 0x40080100,
            0x42000100, 0x42080000, 0x00080100, 0x40000000,
            0x02000000, 0x40080000, 0x40080000, 0x00000000,
            0x40000100, 0x42080100, 0x42080100, 0x02000100,
            0x42080000, 0x40000100, 0x00000000, 0x42000000,
            0x02080100, 0x02000000, 0x42000000, 0x00080100,
            0x00080000, 0x42000100, 0x00000100, 0x02000000,
            0x40000000, 0x02080000, 0x42000100, 0x40080100,
            0x02000100, 0x40000000, 0x42080000, 0x02080100,
            0x40080100, 0x00000100, 0x02000000, 0x42080000,
            0x42080100, 0x00080100, 0x42000000, 0x42080100,
            0x02080000, 0x00000000, 0x40080000, 0x42000000,
            0x00080100, 0x02000100, 0x40000100, 0x00080000,
            0x00000000, 0x40080000, 0x02080100, 0x40000100
        };

        private static UInt32[] SB6 = new UInt32[]
        {
            0x20000010, 0x20400000, 0x00004000, 0x20404010,
            0x20400000, 0x00000010, 0x20404010, 0x00400000,
            0x20004000, 0x00404010, 0x00400000, 0x20000010,
            0x00400010, 0x20004000, 0x20000000, 0x00004010,
            0x00000000, 0x00400010, 0x20004010, 0x00004000,
            0x00404000, 0x20004010, 0x00000010, 0x20400010,
            0x20400010, 0x00000000, 0x00404010, 0x20404000,
            0x00004010, 0x00404000, 0x20404000, 0x20000000,
            0x20004000, 0x00000010, 0x20400010, 0x00404000,
            0x20404010, 0x00400000, 0x00004010, 0x20000010,
            0x00400000, 0x20004000, 0x20000000, 0x00004010,
            0x20000010, 0x20404010, 0x00404000, 0x20400000,
            0x00404010, 0x20404000, 0x00000000, 0x20400010,
            0x00000010, 0x00004000, 0x20400000, 0x00404010,
            0x00004000, 0x00400010, 0x20004010, 0x00000000,
            0x20404000, 0x20000000, 0x00400010, 0x20004010
        };

        private static UInt32[] SB7 = new UInt32[]
        {
            0x00200000, 0x04200002, 0x04000802, 0x00000000,
            0x00000800, 0x04000802, 0x00200802, 0x04200800,
            0x04200802, 0x00200000, 0x00000000, 0x04000002,
            0x00000002, 0x04000000, 0x04200002, 0x00000802,
            0x04000800, 0x00200802, 0x00200002, 0x04000800,
            0x04000002, 0x04200000, 0x04200800, 0x00200002,
            0x04200000, 0x00000800, 0x00000802, 0x04200802,
            0x00200800, 0x00000002, 0x04000000, 0x00200800,
            0x04000000, 0x00200800, 0x00200000, 0x04000802,
            0x04000802, 0x04200002, 0x04200002, 0x00000002,
            0x00200002, 0x04000000, 0x04000800, 0x00200000,
            0x04200800, 0x00000802, 0x00200802, 0x04200800,
            0x00000802, 0x04000002, 0x04200802, 0x04200000,
            0x00200800, 0x00000000, 0x00000002, 0x04200802,
            0x00000000, 0x00200802, 0x04200000, 0x00000800,
            0x04000002, 0x04000800, 0x00000800, 0x00200002
        };

        private static UInt32[] SB8 = new UInt32[]
        {
            0x10001040, 0x00001000, 0x00040000, 0x10041040,
            0x10000000, 0x10001040, 0x00000040, 0x10000000,
            0x00040040, 0x10040000, 0x10041040, 0x00041000,
            0x10041000, 0x00041040, 0x00001000, 0x00000040,
            0x10040000, 0x10000040, 0x10001000, 0x00001040,
            0x00041000, 0x00040040, 0x10040040, 0x10041000,
            0x00001040, 0x00000000, 0x00000000, 0x10040040,
            0x10000040, 0x10001000, 0x00041040, 0x00040000,
            0x00041040, 0x00040000, 0x10041000, 0x00001000,
            0x00000040, 0x10040040, 0x00001000, 0x00041040,
            0x10001000, 0x00000040, 0x10000040, 0x10040000,
            0x10040040, 0x10000000, 0x00040000, 0x10001040,
            0x00000000, 0x10041040, 0x00040040, 0x10000040,
            0x10040000, 0x10001000, 0x10001040, 0x00000000,
            0x10041040, 0x00041000, 0x00041000, 0x00001040,
            0x00001040, 0x00040040, 0x10000000, 0x10041000
        };

        /* PC1: left and right halves bit-swap */

        private static UInt32[] LHs = new UInt32[]
        {
            0x00000000, 0x00000001, 0x00000100, 0x00000101,
            0x00010000, 0x00010001, 0x00010100, 0x00010101,
            0x01000000, 0x01000001, 0x01000100, 0x01000101,
            0x01010000, 0x01010001, 0x01010100, 0x01010101
        };

        private static UInt32[] RHs = new UInt32[]
        {
            0x00000000, 0x01000000, 0x00010000, 0x01010000,
            0x00000100, 0x01000100, 0x00010100, 0x01010100,
            0x00000001, 0x01000001, 0x00010001, 0x01010001,
            0x00000101, 0x01000101, 0x00010101, 0x01010101,
        };

        /* platform-independant 32-bit integer manipulation macros */

        private static UInt32 Get_UInt32(Byte[] key, int iStartIndex)
        {
            return ((UInt32) key[iStartIndex] << 24) | ((UInt32) key[iStartIndex + 1] << 16) |
                   ((UInt32) key[iStartIndex + 2] << 8) | ((UInt32) key[(iStartIndex) + 3]);
        }

        private static void Put_UInt32(UInt32 value, ref Byte[] key, int iStartIndex)
        {
            key[iStartIndex] = (Byte) (value >> 24);
            key[iStartIndex + 1] = (Byte) (value >> 16);
            key[iStartIndex + 2] = (Byte) (value >> 8);
            key[iStartIndex + 3] = (Byte) value;
        }

    /*
#define PUT_UINT32(n,b,i)					   \
{											   \
 (b)[(i)] = (uint8_t) ( (n) >> 24 );	   \
 (b)[(i) + 1] = (uint8_t) ( (n) >> 16 );	   \
 (b)[(i) + 2] = (uint8_t) ( (n) >>  8 );	   \
 (b)[(i) + 3] = (uint8_t) ( (n)	   );	   \
}
*/

    private static UInt32[] des_main_ks(Byte[] key)
        {
            int i;
            UInt32 X, Y, T;
            X = Get_UInt32(key, 0);
            Y = Get_UInt32(key, 4);

            /* Permuted Choice 1 */

            T = ((Y >> 4) ^ X) & 0x0F0F0F0F;
            X ^= T;
            Y ^= (T << 4);
            T = ((Y) ^ X) & 0x10101010;
            X ^= T;
            Y ^= (T);

            X = (LHs[(X) & 0xF] << 3) | (LHs[(X >> 8) & 0xF] << 2)
                | (LHs[(X >> 16) & 0xF] << 1) | (LHs[(X >> 24) & 0xF])
                | (LHs[(X >> 5) & 0xF] << 7) | (LHs[(X >> 13) & 0xF] << 6)
                | (LHs[(X >> 21) & 0xF] << 5) | (LHs[(X >> 29) & 0xF] << 4);

            Y = (RHs[(Y >> 1) & 0xF] << 3) | (RHs[(Y >> 9) & 0xF] << 2)
                | (RHs[(Y >> 17) & 0xF] << 1) | (RHs[(Y >> 25) & 0xF])
                | (RHs[(Y >> 4) & 0xF] << 7) | (RHs[(Y >> 12) & 0xF] << 6)
                | (RHs[(Y >> 20) & 0xF] << 5) | (RHs[(Y >> 28) & 0xF] << 4);

            X &= 0x0FFFFFFF;
            Y &= 0x0FFFFFFF;

            /* calculate subkeys */
            UInt32[] SK = new UInt32[32];
            
            for (i = 0; i < 16; i++)
            {
                if (i < 2 || i == 8 || i == 15)
                {
                    X = ((X << 1) | (X >> 27)) & 0x0FFFFFFF;
                    Y = ((Y << 1) | (Y >> 27)) & 0x0FFFFFFF;
                }
                else
                {
                    X = ((X << 2) | (X >> 26)) & 0x0FFFFFFF;
                    Y = ((Y << 2) | (Y >> 26)) & 0x0FFFFFFF;
                }


                SK[i * 2] = ((X << 4) & 0x24000000) | ((X << 28) & 0x10000000)
                          | ((X << 14) & 0x08000000) | ((X << 18) & 0x02080000)
                          | ((X << 6) & 0x01000000) | ((X << 9) & 0x00200000)
                          | ((X >> 1) & 0x00100000) | ((X << 10) & 0x00040000)
                          | ((X << 2) & 0x00020000) | ((X >> 10) & 0x00010000)
                          | ((Y >> 13) & 0x00002000) | ((Y >> 4) & 0x00001000)
                          | ((Y << 6) & 0x00000800) | ((Y >> 1) & 0x00000400)
                          | ((Y >> 14) & 0x00000200) | ((Y) & 0x00000100)
                          | ((Y >> 5) & 0x00000020) | ((Y >> 10) & 0x00000010)
                          | ((Y >> 3) & 0x00000008) | ((Y >> 18) & 0x00000004)
                          | ((Y >> 26) & 0x00000002) | ((Y >> 24) & 0x00000001);


                SK[i * 2 + 1] = ((X << 15) & 0x20000000) | ((X << 17) & 0x10000000)
                          | ((X << 10) & 0x08000000) | ((X << 22) & 0x04000000)
                          | ((X >> 2) & 0x02000000) | ((X << 1) & 0x01000000)
                          | ((X << 16) & 0x00200000) | ((X << 11) & 0x00100000)
                          | ((X << 3) & 0x00080000) | ((X >> 6) & 0x00040000)
                          | ((X << 15) & 0x00020000) | ((X >> 4) & 0x00010000)
                          | ((Y >> 2) & 0x00002000) | ((Y << 8) & 0x00001000)
                          | ((Y >> 14) & 0x00000808) | ((Y >> 9) & 0x00000400)
                          | ((Y) & 0x00000200) | ((Y << 7) & 0x00000100)
                          | ((Y >> 7) & 0x00000020) | ((Y >> 3) & 0x00000011)
                          | ((Y << 2) & 0x00000004) | ((Y >> 21) & 0x00000002);
            }
            return SK;
        }

        /* Initial Permutation macro */

        private static void Des_Ip(ref UInt32 X,ref UInt32 Y)
        {
            UInt32 T = ((X >> 4) ^ Y) & 0x0F0F0F0F;
            Y ^= T;
            X ^= (T << 4);
            T = ((X >> 16) ^ Y) & 0x0000FFFF;
            Y ^= T;
            X ^= (T << 16);
            T = ((Y >> 2) ^ X) & 0x33333333;
            X ^= T;
            Y ^= (T << 2);
            T = ((Y >> 8) ^ X) & 0x00FF00FF;
            X ^= T;
            Y ^= (T << 8);
            Y = ((Y << 1) | (Y >> 31)) & 0xFFFFFFFF;
            T = (X ^ Y) & 0xAAAAAAAA;
            Y ^= T;
            X ^= T;
            X = ((X << 1) | (X >> 31)) & 0xFFFFFFFF;
        }

    /* Final Permutation macro */

        private static void Des_Fp(ref UInt32 X, ref UInt32 Y)
        {
            X = ((X << 31) | (X >> 1)) & 0xFFFFFFFF;
            UInt32 T = (X ^ Y) & 0xAAAAAAAA;
            X ^= T;
            Y ^= T;
            Y = ((Y << 31) | (Y >> 1)) & 0xFFFFFFFF;
            T = ((Y >> 8) ^ X) & 0x00FF00FF;
            X ^= T;
            Y ^= (T << 8);
            T = ((Y >> 2) ^ X) & 0x33333333;
            X ^= T;
            Y ^= (T << 2);
            T = ((X >> 16) ^ Y) & 0x0000FFFF;
            Y ^= T;
            X ^= (T << 16);
            T = ((X >> 4) ^ Y) & 0x0F0F0F0F;
            Y ^= T;
            X ^= (T << 4);
        }

        /* DES round macro */

        private static void Des_Round(UInt32[] SK, ref int iStartIndex, ref UInt32 X, ref UInt32 Y)
        {
            UInt32 T = SK[iStartIndex++] ^ X;
            Y ^= SB8[(T) & 0x3F] ^
                 SB6[(T >> 8) & 0x3F] ^
                 SB4[(T >> 16) & 0x3F] ^
                 SB2[(T >> 24) & 0x3F];

            T = SK[iStartIndex++] ^ ((X << 28) | (X >> 4));
            Y ^= SB7[(T) & 0x3F] ^
                 SB5[(T >> 8) & 0x3F] ^
                 SB3[(T >> 16) & 0x3F] ^
                 SB1[(T >> 24) & 0x3F];
        }

        /* DES 64-bit block encryption/decryption */

        private static void des_crypt(UInt32[] SK, Byte[] input, ref Byte[] output, int iInStartIndex, int iOutStartIndex)
        {
            UInt32 X, Y;

            X = Get_UInt32(input, iInStartIndex);

            Y = Get_UInt32(input, iInStartIndex + 4);


            Des_Ip(ref X, ref Y);

            int iIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                Des_Round(SK, ref iIndex, ref Y, ref X);
                Des_Round(SK, ref iIndex, ref X, ref Y);
            }

            Des_Fp(ref Y, ref X);

            
            Put_UInt32(Y, ref output, iOutStartIndex);

            Put_UInt32(X, ref output, iOutStartIndex + 4);

        }

        /// <summary>
        /// encode text with des key
        /// </summary>
        /// <param name="key">des key</param>
        /// <param name="text">text to encode (normal string)</param>
        /// <returns>encoded des text (base64 string)</returns>
        public static string desencode(UInt64 key, string text)
        {
            UInt32[] SK = des_main_ks(BitConverter.GetBytes(key));

            //size_t textsz = 0;
            //const uint8_t* text = (const uint8_t*)luaL_checklstring(L, 2, &textsz);
            byte[] textArray = Encoding.UTF8.GetBytes(text);
            int textsz = textArray.Length;
            int chunksz = (textsz + 8) & ~7;
            Byte[] buffer = new byte[chunksz];
           
            int i;
            for (i = 0; i < textsz - 7; i += 8)
            {
                des_crypt(SK, textArray, ref buffer, i, i);
            }
            int bytes = textsz - i;
            Byte[] tail = new byte[8];
            int j;
            for (j = 0; j < 8; j++)
            {
                if (j < bytes)
                {
                    tail[j] = textArray[i + j];
                }
                else if (j == bytes)
                {
                    tail[j] = 0x80;
                }
                else
                {
                    tail[j] = 0;
                }
            }
            des_crypt(SK, tail,ref buffer, 0, i);

            return Convert.ToBase64String(buffer);
        }

        
        /// <summary>
        /// decode Des Text
        /// </summary>
        /// <param name="key">des key</param> 
        /// <param name="encodedText">text to decode(base64 string)</param>
        /// <returns>decoded byte array</returns>
        public static Byte[] desdecode(UInt64 key, string encodedText)
        {
            UInt32[] ESK = des_main_ks(BitConverter.GetBytes(key));
            
            UInt32[] SK = new UInt32[32];
            int i;
            for (i = 0; i < 32; i += 2)
            {
                SK[i] = ESK[30 - i];
                SK[i + 1] = ESK[31 - i];
            }
            byte[] textArray = Convert.FromBase64String(encodedText);
            int textsz = textArray.Length;
            //size_t textsz = 0;
            if (((textsz & 7) != 0) || (textsz == 0))
            {
                throw new Exception("Invalid des crypt text length " + textsz);
            }
          
            Byte[] buffer = new Byte[textsz];
     
            for (i = 0; i < textsz; i += 8)
            {
                des_crypt(SK, textArray, ref buffer, i, i);
            }
            int padding = 1;
            for (i = textsz - 1; i >= textsz - 8; i--)
            {
                if (buffer[i] == 0)
                {
                    padding++;
                }
                else if (buffer[i] == 0x80)
                {
                    break;
                }
                else
                {
                    throw new Exception("Invalid des crypt text");
                }
            }
            if (padding > 8)
            {
                throw new Exception("Invalid des crypt text");
            }
            Byte[] resultText = new Byte[textsz - padding];
            for (int j = 0; j < resultText.Length; j++)
            {
                resultText[j] = buffer[j];
            }     
            return resultText;
        }

    }
}