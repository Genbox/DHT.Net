//
// BEncodedString.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
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
using System.Linq;
using System.Text;

namespace DHTNet.BEncode
{
    /// <summary>
    /// Class representing a BEncoded string
    /// </summary>
    public class BEncodedString : BEncodedValue, IComparable<BEncodedString>, IEquatable<BEncodedString>
    {
        /// <summary>
        /// Create a new BEncodedString using UTF8 encoding
        /// </summary>
        public BEncodedString()
            : this(new byte[0])
        {
        }

        /// <summary>
        /// Create a new BEncodedString using UTF8 encoding
        /// </summary>
        /// <param name="value"></param>
        public BEncodedString(char[] value)
            : this(Encoding.UTF8.GetBytes(value))
        {
        }

        /// <summary>
        /// Create a new BEncodedString using UTF8 encoding
        /// </summary>
        /// <param name="value">Initial value for the string</param>
        public BEncodedString(string value)
            : this(Encoding.UTF8.GetBytes(value))
        {
        }


        /// <summary>
        /// Create a new BEncodedString using UTF8 encoding
        /// </summary>
        /// <param name="value"></param>
        public BEncodedString(byte[] value)
        {
            TextBytes = value;
        }

        /// <summary>
        /// The value of the BEncodedString
        /// </summary>
        public string Text
        {
            get { return Encoding.UTF8.GetString(TextBytes); }
            set { TextBytes = Encoding.UTF8.GetBytes(value); }
        }

        /// <summary>
        /// The underlying byte[] associated with this BEncodedString
        /// </summary>
        public byte[] TextBytes { get; private set; }

        public string Hex => BitConverter.ToString(TextBytes).Replace("-", string.Empty);

        public int CompareTo(BEncodedString other)
        {
            if (other == null)
                return 1;

            int difference;
            int length = TextBytes.Length > other.TextBytes.Length ? other.TextBytes.Length : TextBytes.Length;

            for (int i = 0; i < length; i++)
                if ((difference = TextBytes[i].CompareTo(other.TextBytes[i])) != 0)
                    return difference;

            if (TextBytes.Length == other.TextBytes.Length)
                return 0;

            return TextBytes.Length > other.TextBytes.Length ? 1 : -1;
        }

        public bool Equals(BEncodedString other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            return TextBytes.SequenceEqual(other.TextBytes);
        }


        public static implicit operator BEncodedString(string value)
        {
            return new BEncodedString(value);
        }

        public static implicit operator BEncodedString(char[] value)
        {
            return new BEncodedString(value);
        }

        public static implicit operator BEncodedString(byte[] value)
        {
            return new BEncodedString(value);
        }

        /// <summary>
        /// Encodes the BEncodedString to a byte[] using the supplied Encoding
        /// </summary>
        /// <param name="buffer">The buffer to encode the string to</param>
        /// <param name="offset">The offset at which to save the data to</param>
        /// <returns>The number of bytes encoded</returns>
        public override int Encode(byte[] buffer, int offset)
        {
            int written = offset;
            written += WriteAscii(buffer, written, TextBytes.Length.ToString());
            written += WriteAscii(buffer, written, ":");
            written += Write(buffer, written, TextBytes);
            return written - offset;
        }

        private static int Write(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest, destOffset, count);
            return count;
        }

        private static int Write(byte[] buffer, int offset, byte[] value)
        {
            return Write(buffer, offset, value, 0, value.Length);
        }

        private static int WriteAscii(byte[] buffer, int offset, string text)
        {
            for (int i = 0; i < text.Length; i++)
                buffer[offset + i] = (byte)text[i];
            return text.Length;
        }

        /// <summary>
        /// Decodes a BEncodedString from the supplied StreamReader
        /// </summary>
        /// <param name="reader">The StreamReader containing the BEncodedString</param>
        protected override void DecodeInternal(RawReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            int letterCount;
            string length = string.Empty;

            while ((reader.PeekByte() != -1) && (reader.PeekByte() != ':')) // read in how many characters
                length += (char)reader.ReadByte(); // the string is

            if (reader.ReadByte() != ':') // remove the ':'
                throw new BEncodingException("Invalid data found. Aborting");

            if (!int.TryParse(length, out letterCount))
                throw new BEncodingException(
                    string.Format("Invalid BEncodedString. Length was '{0}' instead of a number", length));

            TextBytes = new byte[letterCount];
            if (reader.Read(TextBytes, 0, letterCount) != letterCount)
                throw new BEncodingException("Couldn't decode string");
        }

        public override int LengthInBytes()
        {
            // The length is equal to the length-prefix + ':' + length of data
            int prefix = 1; // Account for ':'

            // Count the number of characters needed for the length prefix
            for (int i = TextBytes.Length; i != 0; i = i / 10)
                prefix += 1;

            if (TextBytes.Length == 0)
                prefix++;

            return prefix + TextBytes.Length;
        }

        public int CompareTo(object other)
        {
            return CompareTo(other as BEncodedString);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            BEncodedString other;
            string s = obj as string;
            if (s != null)
                other = new BEncodedString(s);
            else if (obj is BEncodedString)
                other = (BEncodedString)obj;
            else
                return false;

            return Equals(this, other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 0;
                foreach (byte b in TextBytes)
                    result = (result * 31) ^ b;
                return result;
            }
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(TextBytes);
        }
    }
}