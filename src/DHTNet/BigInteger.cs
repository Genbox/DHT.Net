//
// BigInteger.cs - Big Integer implementation
//
// Authors:
//	Ben Maurer
//	Chew Keong TAN
//	Sebastien Pouliot <sebastien@ximian.com>
//	Pieter Philippaerts <Pieter@mentalis.org>
//
// Copyright (c) 2003 Ben Maurer
// All rights reserved
//
// Copyright (c) 2002 Chew Keong TAN
// All rights reserved.
//
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace DHTNet
{
    internal class BigInteger
    {
        public enum Sign
        {
            Negative = -1,
            Zero = 0,
            Positive = 1
        }

        /// <summary>
        /// Default length of a BigInteger in bytes
        /// </summary>
        private const uint DefaultLen = 20;

        private const string WouldReturnNegVal = "Operation would return a negative value";

        /// <summary>
        /// The data for this BigInteger
        /// </summary>
        private readonly uint[] _data;

        /// <summary>
        /// The Length of this BigInteger
        /// </summary>
        private uint _length = 1;

        public BigInteger()
        {
            _data = new uint[DefaultLen];
            _length = DefaultLen;
        }

        public BigInteger(uint ui)
        {
            _data = new[] {ui};
        }

        public BigInteger(uint[] ui)
        {
            _data = ui;
            _length = (uint) _data.Length;
            Normalize();
        }

        public BigInteger(Sign sign, uint len)
        {
            _data = new uint[len];
            _length = len;
        }

        public BigInteger(BigInteger bi)
        {
            _data = (uint[]) bi._data.Clone();
            _length = bi._length;
        }

        public BigInteger(BigInteger bi, uint len)
        {
            _data = new uint[len];

            for (uint i = 0; i < bi._length; i++)
                _data[i] = bi._data[i];

            _length = bi._length;
        }

        public BigInteger(byte[] inData)
        {
            if (inData.Length == 0)
                inData = new byte[1];
            _length = (uint) inData.Length >> 2;
            int leftOver = inData.Length & 0x3;

            // length not multiples of 4
            if (leftOver != 0) _length++;

            _data = new uint[_length];

            for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++)
            {
                _data[j] = (uint) (
                    (inData[i - 3] << (3 * 8)) |
                    (inData[i - 2] << (2 * 8)) |
                    (inData[i - 1] << (1 * 8)) |
                    inData[i]
                );
            }

            switch (leftOver)
            {
                case 1:
                    _data[_length - 1] = inData[0];
                    break;
                case 2:
                    _data[_length - 1] = (uint) ((inData[0] << 8) | inData[1]);
                    break;
                case 3:
                    _data[_length - 1] = (uint) ((inData[0] << 16) | (inData[1] << 8) | inData[2]);
                    break;
            }

            Normalize();
        }

        /// <summary>
        ///     Normalizes this by setting the length to the actual number of
        ///     uints used in data and by setting the sign to Sign.Zero if the
        ///     value of this is 0.
        /// </summary>
        private void Normalize()
        {
            // Normalize length
            while ((_length > 0) && (_data[_length - 1] == 0)) _length--;

            // Check for zero
            if (_length == 0)
                _length++;
        }

        internal BigInteger Xor(BigInteger other)
        {
            int len = Math.Min(_data.Length, other._data.Length);
            uint[] result = new uint[len];

            for (int i = 0; i < len; i++)
                result[i] = _data[i] ^ other._data[i];

            return new BigInteger(result);
        }

        public static implicit operator BigInteger(uint value)
        {
            return new BigInteger(value);
        }

        public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
        {
            if (bi1 == 0)
                return new BigInteger(bi2);
            if (bi2 == 0)
                return new BigInteger(bi1);
            return Kernel.AddSameSign(bi1, bi2);
        }

        public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
        {
            if (bi2 == 0)
                return new BigInteger(bi1);

            if (bi1 == 0)
                throw new ArithmeticException(WouldReturnNegVal);

            switch (Kernel.Compare(bi1, bi2))
            {
                case Sign.Zero:
                    return 0;

                case Sign.Positive:
                    return Kernel.Subtract(bi1, bi2);

                case Sign.Negative:
                    throw new ArithmeticException(WouldReturnNegVal);
                default:
                    throw new Exception();
            }
        }

        public static int operator %(BigInteger bi, int i)
        {
            if (i > 0)
                return (int) Kernel.DwordMod(bi, (uint) i);
            return -(int) Kernel.DwordMod(bi, (uint) -i);
        }

        public static uint operator %(BigInteger bi, uint ui)
        {
            return Kernel.DwordMod(bi, ui);
        }

        public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.MultiByteDivide(bi1, bi2)[1];
        }

        public static BigInteger operator /(BigInteger bi, int i)
        {
            if (i > 0)
                return Kernel.DwordDiv(bi, (uint) i);

            throw new ArithmeticException(WouldReturnNegVal);
        }

        public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.MultiByteDivide(bi1, bi2)[0];
        }

        public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
        {
            if ((bi1 == 0) || (bi2 == 0)) return 0;

            //
            // Validate pointers
            //
            if (bi1._data.Length < bi1._length) throw new IndexOutOfRangeException("bi1 out of range");
            if (bi2._data.Length < bi2._length) throw new IndexOutOfRangeException("bi2 out of range");

            BigInteger ret = new BigInteger(Sign.Positive, bi1._length + bi2._length);

            Kernel.Multiply(bi1._data, 0, bi1._length, bi2._data, 0, bi2._length, ret._data, 0);

            ret.Normalize();
            return ret;
        }

        public static BigInteger operator *(BigInteger bi, int i)
        {
            if (i < 0) throw new ArithmeticException(WouldReturnNegVal);
            if (i == 0) return 0;
            if (i == 1) return new BigInteger(bi);

            return Kernel.MultiplyByDword(bi, (uint) i);
        }

        public static BigInteger operator <<(BigInteger bi1, int shiftVal)
        {
            return Kernel.LeftShift(bi1, shiftVal);
        }

        public static BigInteger operator >>(BigInteger bi1, int shiftVal)
        {
            return Kernel.RightShift(bi1, shiftVal);
        }

        public int BitCount()
        {
            Normalize();

            uint value = _data[_length - 1];
            uint mask = 0x80000000;
            uint bits = 32;

            while ((bits > 0) && ((value & mask) == 0))
            {
                bits--;
                mask >>= 1;
            }
            bits += (_length - 1) << 5;

            return (int) bits;
        }

        public byte[] GetBytes(int arraySize)
        {
            if (this == 0)
                return new byte[arraySize];

            int numBits = BitCount();
            int numBytes = numBits >> 3;
            if ((numBits & 0x7) != 0)
                numBytes++;

            if (arraySize < numBytes)
                throw new ArgumentOutOfRangeException(nameof(arraySize));

            byte[] result = new byte[arraySize];

            int numBytesInWord = numBytes & 0x3;
            if (numBytesInWord == 0) numBytesInWord = 4;

            int pos = 0;
            for (int i = (int) _length - 1; i >= 0; i--)
            {
                uint val = _data[i];
                for (int j = numBytesInWord - 1; j >= 0; j--)
                {
                    result[pos + j] = (byte) (val & 0xFF);
                    val >>= 8;
                }
                pos += numBytesInWord;
                numBytesInWord = 4;
            }
            return result;
        }

        public static bool operator ==(BigInteger bi1, uint ui)
        {
            if (bi1._length != 1) bi1.Normalize();
            return (bi1._length == 1) && (bi1._data[0] == ui);
        }

        public static bool operator !=(BigInteger bi1, uint ui)
        {
            if (bi1._length != 1) bi1.Normalize();
            return !((bi1._length == 1) && (bi1._data[0] == ui));
        }

        public static bool operator ==(BigInteger bi1, BigInteger bi2)
        {
            // we need to compare with null
            if (bi1 == bi2 as object)
                return true;
            if ((null == bi1) || (null == bi2))
                return false;
            return Kernel.Compare(bi1, bi2) == 0;
        }

        public static bool operator !=(BigInteger bi1, BigInteger bi2)
        {
            // we need to compare with null
            if (bi1 == bi2 as object)
                return false;
            if ((null == bi1) || (null == bi2))
                return true;
            return Kernel.Compare(bi1, bi2) != 0;
        }

        public static bool operator >(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.Compare(bi1, bi2) > 0;
        }

        public static bool operator <(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.Compare(bi1, bi2) < 0;
        }

        public static bool operator >=(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.Compare(bi1, bi2) >= 0;
        }

        public static bool operator <=(BigInteger bi1, BigInteger bi2)
        {
            return Kernel.Compare(bi1, bi2) <= 0;
        }

        public Sign Compare(BigInteger bi)
        {
            return Kernel.Compare(this, bi);
        }

        public string ToString(uint radix, string characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            if (characterSet.Length < radix)
                throw new ArgumentException("charSet length less than radix", nameof(characterSet));
            if (radix == 1)
                throw new ArgumentException("There is no such thing as radix one notation", nameof(radix));

            if (this == 0) return "0";
            if (this == 1) return "1";

            string result = "";

            BigInteger a = new BigInteger(this);

            while (a != 0)
            {
                uint rem = Kernel.SingleByteDivideInPlace(a, radix);
                result = characterSet[(int) rem] + result;
            }

            return result;
        }

        public override int GetHashCode()
        {
            uint val = 0;

            for (uint i = 0; i < _length; i++)
                val ^= _data[i];

            return (int) val;
        }

        public override string ToString()
        {
            return ToString(10);
        }

        public override bool Equals(object o)
        {
            if (o == null) return false;
            if (o is int) return ((int) o >= 0) && (this == (uint) o);

            return Kernel.Compare(this, (BigInteger) o) == 0;
        }

        private sealed class Kernel
        {
            /// <summary>
            /// Compares two BigInteger
            /// </summary>
            /// <param name="bi1">A BigInteger</param>
            /// <param name="bi2">A BigInteger</param>
            /// <returns>The sign of bi1 - bi2</returns>
            public static Sign Compare(BigInteger bi1, BigInteger bi2)
            {
                //
                // Step 1. Compare the lengths
                //
                uint l1 = bi1._length, l2 = bi2._length;

                while ((l1 > 0) && (bi1._data[l1 - 1] == 0)) l1--;
                while ((l2 > 0) && (bi2._data[l2 - 1] == 0)) l2--;

                if ((l1 == 0) && (l2 == 0)) return Sign.Zero;

                // bi1 len < bi2 len
                if (l1 < l2) return Sign.Negative;
                // bi1 len > bi2 len
                if (l1 > l2) return Sign.Positive;

                //
                // Step 2. Compare the bits
                //

                uint pos = l1 - 1;

                while ((pos != 0) && (bi1._data[pos] == bi2._data[pos])) pos--;

                if (bi1._data[pos] < bi2._data[pos])
                    return Sign.Negative;
                if (bi1._data[pos] > bi2._data[pos])
                    return Sign.Positive;
                return Sign.Zero;
            }

            /// <summary>
            /// Adds two numbers with the same sign.
            /// </summary>
            /// <param name="bi1">A BigInteger</param>
            /// <param name="bi2">A BigInteger</param>
            /// <returns>bi1 + bi2</returns>
            public static BigInteger AddSameSign(BigInteger bi1, BigInteger bi2)
            {
                uint[] x, y;
                uint yMax, xMax, i = 0;

                // x should be bigger
                if (bi1._length < bi2._length)
                {
                    x = bi2._data;
                    xMax = bi2._length;
                    y = bi1._data;
                    yMax = bi1._length;
                }
                else
                {
                    x = bi1._data;
                    xMax = bi1._length;
                    y = bi2._data;
                    yMax = bi2._length;
                }

                BigInteger result = new BigInteger(Sign.Positive, xMax + 1);

                uint[] r = result._data;

                ulong sum = 0;

                // Add common parts of both numbers
                do
                {
                    sum = x[i] + (ulong) y[i] + sum;
                    r[i] = (uint) sum;
                    sum >>= 32;
                } while (++i < yMax);

                // Copy remainder of longer number while carry propagation is required
                bool carry = sum != 0;

                if (carry)
                {
                    if (i < xMax)
                        do
                        {
                            carry = (r[i] = x[i] + 1) == 0;
                        } while ((++i < xMax) && carry);

                    if (carry)
                    {
                        r[i] = 1;
                        result._length = ++i;
                        return result;
                    }
                }

                // Copy the rest
                if (i < xMax)
                    do
                    {
                        r[i] = x[i];
                    } while (++i < xMax);

                result.Normalize();
                return result;
            }

            public static BigInteger Subtract(BigInteger big, BigInteger small)
            {
                BigInteger result = new BigInteger(Sign.Positive, big._length);

                uint[] r = result._data, b = big._data, s = small._data;
                uint i = 0, c = 0;

                do
                {
                    uint x = s[i];
                    if (((x += c) < c) | ((r[i] = b[i] - x) > ~x))
                        c = 1;
                    else
                        c = 0;
                } while (++i < small._length);

                if (i == big._length) goto fixup;

                if (c == 1)
                {
                    do
                    {
                        r[i] = b[i] - 1;
                    } while ((b[i++] == 0) && (i < big._length));

                    if (i == big._length) goto fixup;
                }

                do
                {
                    r[i] = b[i];
                } while (++i < big._length);

                fixup:

                result.Normalize();
                return result;
            }

            /// <summary>
            /// Performs n / d and n % d in one operation.
            /// </summary>
            /// <param name="n">A BigInteger, upon exit this will hold n / d</param>
            /// <param name="d">The divisor</param>
            /// <returns>n % d</returns>
            public static uint SingleByteDivideInPlace(BigInteger n, uint d)
            {
                ulong r = 0;
                uint i = n._length;

                while (i-- > 0)
                {
                    r <<= 32;
                    r |= n._data[i];
                    n._data[i] = (uint) (r / d);
                    r %= d;
                }
                n.Normalize();

                return (uint) r;
            }

            public static uint DwordMod(BigInteger n, uint d)
            {
                ulong r = 0;
                uint i = n._length;

                while (i-- > 0)
                {
                    r <<= 32;
                    r |= n._data[i];
                    r %= d;
                }

                return (uint) r;
            }

            public static BigInteger DwordDiv(BigInteger n, uint d)
            {
                BigInteger ret = new BigInteger(Sign.Positive, n._length);

                ulong r = 0;
                uint i = n._length;

                while (i-- > 0)
                {
                    r <<= 32;
                    r |= n._data[i];
                    ret._data[i] = (uint) (r / d);
                    r %= d;
                }
                ret.Normalize();

                return ret;
            }

            public static BigInteger[] DwordDivMod(BigInteger n, uint d)
            {
                BigInteger ret = new BigInteger(Sign.Positive, n._length);

                ulong r = 0;
                uint i = n._length;

                while (i-- > 0)
                {
                    r <<= 32;
                    r |= n._data[i];
                    ret._data[i] = (uint) (r / d);
                    r %= d;
                }
                ret.Normalize();

                BigInteger rem = (uint) r;

                return new[] {ret, rem};
            }

            public static BigInteger[] MultiByteDivide(BigInteger bi1, BigInteger bi2)
            {
                if (Compare(bi1, bi2) == Sign.Negative)
                    return new BigInteger[2] {0, new BigInteger(bi1)};

                bi1.Normalize();
                bi2.Normalize();

                if (bi2._length == 1)
                    return DwordDivMod(bi1, bi2._data[0]);

                uint remainderLen = bi1._length + 1;
                int divisorLen = (int) bi2._length + 1;

                uint mask = 0x80000000;
                uint val = bi2._data[bi2._length - 1];
                int shift = 0;
                int resultPos = (int) bi1._length - (int) bi2._length;

                while ((mask != 0) && ((val & mask) == 0))
                {
                    shift++;
                    mask >>= 1;
                }

                BigInteger quot = new BigInteger(Sign.Positive, bi1._length - bi2._length + 1);
                BigInteger rem = bi1 << shift;

                uint[] remainder = rem._data;

                bi2 = bi2 << shift;

                int j = (int) (remainderLen - bi2._length);
                int pos = (int) remainderLen - 1;

                uint firstDivisorByte = bi2._data[bi2._length - 1];
                ulong secondDivisorByte = bi2._data[bi2._length - 2];

                while (j > 0)
                {
                    ulong dividend = ((ulong) remainder[pos] << 32) + remainder[pos - 1];

                    ulong qHat = dividend / firstDivisorByte;
                    ulong rHat = dividend % firstDivisorByte;

                    do
                    {
                        if ((qHat == 0x100000000) ||
                            (qHat * secondDivisorByte > (rHat << 32) + remainder[pos - 2]))
                        {
                            qHat--;
                            rHat += firstDivisorByte;

                            if (rHat < 0x100000000)
                                continue;
                        }
                        break;
                    } while (true);

                    //
                    // At this point, q_hat is either exact, or one too large
                    // (more likely to be exact) so, we attempt to multiply the
                    // divisor by q_hat, if we get a borrow, we just subtract
                    // one from q_hat and add the divisor back.
                    //

                    uint t;
                    uint dPos = 0;
                    int nPos = pos - divisorLen + 1;
                    ulong mc = 0;
                    uint uintQHat = (uint) qHat;
                    do
                    {
                        mc += bi2._data[dPos] * (ulong) uintQHat;
                        t = remainder[nPos];
                        remainder[nPos] -= (uint) mc;
                        mc >>= 32;
                        if (remainder[nPos] > t) mc++;
                        dPos++;
                        nPos++;
                    } while (dPos < divisorLen);

                    nPos = pos - divisorLen + 1;
                    dPos = 0;

                    // Overestimate
                    if (mc != 0)
                    {
                        uintQHat--;
                        ulong sum = 0;

                        do
                        {
                            sum = remainder[nPos] + (ulong) bi2._data[dPos] + sum;
                            remainder[nPos] = (uint) sum;
                            sum >>= 32;
                            dPos++;
                            nPos++;
                        } while (dPos < divisorLen);
                    }

                    quot._data[resultPos--] = uintQHat;

                    pos--;
                    j--;
                }

                quot.Normalize();
                rem.Normalize();
                BigInteger[] ret = new BigInteger[2] {quot, rem};

                if (shift != 0)
                    ret[1] >>= shift;

                return ret;
            }

            public static BigInteger LeftShift(BigInteger bi, int n)
            {
                if (n == 0) return new BigInteger(bi, bi._length + 1);

                int w = n >> 5;
                n &= (1 << 5) - 1;

                BigInteger ret = new BigInteger(Sign.Positive, bi._length + 1 + (uint) w);

                uint i = 0, l = bi._length;
                if (n != 0)
                {
                    uint x, carry = 0;
                    while (i < l)
                    {
                        x = bi._data[i];
                        ret._data[i + w] = (x << n) | carry;
                        carry = x >> (32 - n);
                        i++;
                    }
                    ret._data[i + w] = carry;
                }
                else
                {
                    while (i < l)
                    {
                        ret._data[i + w] = bi._data[i];
                        i++;
                    }
                }

                ret.Normalize();
                return ret;
            }

            public static BigInteger RightShift(BigInteger bi, int n)
            {
                if (n == 0) return new BigInteger(bi);

                int w = n >> 5;
                int s = n & ((1 << 5) - 1);

                BigInteger ret = new BigInteger(Sign.Positive, bi._length - (uint) w + 1);
                uint l = (uint) ret._data.Length - 1;

                if (s != 0)
                {
                    uint x, carry = 0;

                    while (l-- > 0)
                    {
                        x = bi._data[l + w];
                        ret._data[l] = (x >> n) | carry;
                        carry = x << (32 - n);
                    }
                }
                else
                {
                    while (l-- > 0)
                        ret._data[l] = bi._data[l + w];
                }
                ret.Normalize();
                return ret;
            }

            public static BigInteger MultiplyByDword(BigInteger n, uint f)
            {
                BigInteger ret = new BigInteger(Sign.Positive, n._length + 1);

                uint i = 0;
                ulong c = 0;

                do
                {
                    c += n._data[i] * (ulong) f;
                    ret._data[i] = (uint) c;
                    c >>= 32;
                } while (++i < n._length);
                ret._data[i] = (uint) c;
                ret.Normalize();
                return ret;
            }

            /// <summary>
            /// Multiplies the data in x [xOffset:xOffset+xLen] by
            /// y [yOffset:yOffset+yLen] and puts it into
            /// d [dOffset:dOffset+xLen+yLen].
            /// </summary>
            /// <remarks>
            /// This code is unsafe! It is the caller's responsibility to make
            /// sure that it is safe to access x [xOffset:xOffset+xLen],
            /// y [yOffset:yOffset+yLen], and d [dOffset:dOffset+xLen+yLen].
            /// </remarks>
            public static unsafe void Multiply(uint[] x, uint xOffset, uint xLen, uint[] y, uint yOffset, uint yLen, uint[] d, uint dOffset)
            {
                fixed (uint* xx = x, yy = y, dd = d)
                {
                    uint* xP = xx + xOffset,
                        xE = xP + xLen,
                        yB = yy + yOffset,
                        yE = yB + yLen,
                        dB = dd + dOffset;

                    for (; xP < xE; xP++, dB++)
                    {
                        if (*xP == 0) continue;

                        ulong mcarry = 0;

                        uint* dP = dB;
                        for (uint* yP = yB; yP < yE; yP++, dP++)
                        {
                            mcarry += *xP * (ulong) *yP + *dP;

                            *dP = (uint) mcarry;
                            mcarry >>= 32;
                        }

                        if (mcarry != 0)
                            *dP = (uint) mcarry;
                    }
                }
            }
        }
    }
}