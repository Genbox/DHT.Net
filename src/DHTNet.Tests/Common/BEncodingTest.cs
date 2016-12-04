//
// BEncodingTest.cs
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


using System.IO;
using System.Text;
using DHTNet.BEncode;
using Xunit;
using Toolbox = DHTNet.Utils.Toolbox;

namespace DHTNet.Tests.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class BEncodeTest
    {
        [Fact]
        public void BenDictionaryDecoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("d4:spaml1:a1:bee");
            using (Stream stream = new MemoryStream(data))
            {
                BEncodedValue result = BEncodedValue.Decode(stream);
                Assert.Equal(result.ToString(), "d4:spaml1:a1:bee");
                Assert.Equal(result is BEncodedDictionary, true);

                BEncodedDictionary dict = (BEncodedDictionary) result;
                Assert.Equal(dict.Count, 1);
                Assert.True(dict["spam"] is BEncodedList);

                BEncodedList list = (BEncodedList) dict["spam"];
                Assert.Equal(((BEncodedString) list[0]).Text, "a");
                Assert.Equal(((BEncodedString) list[1]).Text, "b");
            }
        }

        [Fact]
        public void BenDictionaryEncoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("d4:spaml1:a1:bee");

            BEncodedDictionary dict = new BEncodedDictionary();
            BEncodedList list = new BEncodedList();
            list.Add(new BEncodedString("a"));
            list.Add(new BEncodedString("b"));
            dict.Add("spam", list);
            Assert.Equal(Encoding.UTF8.GetString(data), Encoding.UTF8.GetString(dict.Encode()));
            Assert.True(Toolbox.ByteMatch(data, dict.Encode()));
        }

        [Fact]
        public void BenDictionaryEncodingBuffered()
        {
            byte[] data = Encoding.UTF8.GetBytes("d4:spaml1:a1:bee");
            BEncodedDictionary dict = new BEncodedDictionary();
            BEncodedList list = new BEncodedList();
            list.Add(new BEncodedString("a"));
            list.Add(new BEncodedString("b"));
            dict.Add("spam", list);
            byte[] result = new byte[dict.LengthInBytes()];
            dict.Encode(result, 0);
            Assert.True(Toolbox.ByteMatch(data, result));
        }

        [Fact]
        public void BenDictionaryLengthInBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("d4:spaml1:a1:bee");
            BEncodedDictionary dict = (BEncodedDictionary) BEncodedValue.Decode(data);

            Assert.Equal(data.Length, dict.LengthInBytes());
        }

        [Fact]
        public void BenDictionaryStackedTest()
        {
            string benString = "d4:testd5:testsli12345ei12345ee2:tod3:tomi12345eeee";
            byte[] data = Encoding.UTF8.GetBytes(benString);
            BEncodedDictionary dict = (BEncodedDictionary) BEncodedValue.Decode(data);
            string decoded = Encoding.UTF8.GetString(dict.Encode());
            Assert.Equal(benString, decoded);
        }

        [Fact]
        public void BenListDecoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("l4:test5:tests6:testede");
            using (Stream stream = new MemoryStream(data))
            {
                BEncodedValue result = BEncodedValue.Decode(stream);
                Assert.Equal(result.ToString(), "l4:test5:tests6:testede");
                Assert.Equal(result is BEncodedList, true);
                BEncodedList list = (BEncodedList) result;

                Assert.Equal(list.Count, 3);
                Assert.Equal(list[0] is BEncodedString, true);
                Assert.Equal(((BEncodedString) list[0]).Text, "test");
                Assert.Equal(((BEncodedString) list[1]).Text, "tests");
                Assert.Equal(((BEncodedString) list[2]).Text, "tested");
            }
        }

        [Fact]
        public void BenListEncoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("l4:test5:tests6:testede");
            BEncodedList list = new BEncodedList();
            list.Add(new BEncodedString("test"));
            list.Add(new BEncodedString("tests"));
            list.Add(new BEncodedString("tested"));

            Assert.True(Toolbox.ByteMatch(data, list.Encode()));
        }

        [Fact]
        public void BenListEncodingBuffered()
        {
            byte[] data = Encoding.UTF8.GetBytes("l4:test5:tests6:testede");
            BEncodedList list = new BEncodedList();
            list.Add(new BEncodedString("test"));
            list.Add(new BEncodedString("tests"));
            list.Add(new BEncodedString("tested"));
            byte[] result = new byte[list.LengthInBytes()];
            list.Encode(result, 0);
            Assert.True(Toolbox.ByteMatch(data, result));
        }

        [Fact]
        public void BenListLengthInBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("l4:test5:tests6:testede");
            BEncodedList list = (BEncodedList) BEncodedValue.Decode(data);

            Assert.Equal(data.Length, list.LengthInBytes());
        }

        [Fact]
        public void BenListStackedTest()
        {
            string benString = "l6:stringl7:stringsl8:stringedei23456eei12345ee";
            byte[] data = Encoding.UTF8.GetBytes(benString);
            BEncodedList list = (BEncodedList) BEncodedValue.Decode(data);
            string decoded = Encoding.UTF8.GetString(list.Encode());
            Assert.Equal(benString, decoded);
        }

        [Fact]
        public void BenNumberDecoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("i12412e");
            using (Stream stream = new MemoryStream(data))
            {
                BEncodedValue result = BEncodedValue.Decode(stream);
                Assert.Equal(result is BEncodedNumber, true);
                Assert.Equal(result.ToString(), "12412");
                Assert.Equal(((BEncodedNumber) result).Number, 12412);
            }
        }

        [Fact]
        public void BenNumberEncoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("i12345e");
            BEncodedNumber number = 12345;
            Assert.True(Toolbox.ByteMatch(data, number.Encode()));
        }

        [Fact]
        public void BenNumberEncoding2()
        {
            byte[] data = Encoding.UTF8.GetBytes("i0e");
            BEncodedNumber number = 0;
            Assert.Equal(3, number.LengthInBytes());
            Assert.True(Toolbox.ByteMatch(data, number.Encode()));
        }

        [Fact]
        public void BenNumberEncoding3()
        {
            byte[] data = Encoding.UTF8.GetBytes("i1230e");
            BEncodedNumber number = 1230;
            Assert.Equal(6, number.LengthInBytes());
            Assert.True(Toolbox.ByteMatch(data, number.Encode()));
        }

        [Fact]
        public void BenNumberEncoding4()
        {
            byte[] data = Encoding.UTF8.GetBytes("i-1230e");
            BEncodedNumber number = -1230;
            Assert.Equal(7, number.LengthInBytes());
            Assert.True(Toolbox.ByteMatch(data, number.Encode()));
        }

        [Fact]
        public void BenNumberEncoding5()
        {
            byte[] data = Encoding.UTF8.GetBytes("i-123e");
            BEncodedNumber number = -123;
            Assert.Equal(6, number.LengthInBytes());
            Assert.True(Toolbox.ByteMatch(data, number.Encode()));
        }

        [Fact]
        public void BenNumberEncoding6()
        {
            BEncodedNumber a = -123;
            BEncodedNumber b = BEncodedValue.Decode<BEncodedNumber>(a.Encode());
            Assert.Equal(a.Number, b.Number);
        }

        [Fact]
        public void BenNumberEncodingBuffered()
        {
            byte[] data = Encoding.UTF8.GetBytes("i12345e");
            BEncodedNumber number = 12345;
            byte[] result = new byte[number.LengthInBytes()];
            number.Encode(result, 0);
            Assert.True(Toolbox.ByteMatch(data, result));
        }

        [Fact]
        public void BenNumberLengthInBytes()
        {
            int number = 1635;
            BEncodedNumber num = number;
            Assert.Equal(number.ToString().Length + 2, num.LengthInBytes());
        }

        [Fact]
        public void BenStringDecoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("21:this is a test string");
            using (MemoryStream stream = new MemoryStream(data))
            {
                BEncodedValue result = BEncodedValue.Decode(stream);
                Assert.Equal("this is a test string", result.ToString());
                Assert.Equal(result is BEncodedString, true);
                Assert.Equal(((BEncodedString) result).Text, "this is a test string");
            }
        }

        [Fact]
        public void BenStringEncoding()
        {
            byte[] data = Encoding.UTF8.GetBytes("22:this is my test string");

            BEncodedString benString = new BEncodedString("this is my test string");
            Assert.True(Toolbox.ByteMatch(data, benString.Encode()));
        }

        [Fact]
        public void BenStringEncoding2()
        {
            byte[] data = Encoding.UTF8.GetBytes("0:");

            BEncodedString benString = new BEncodedString("");
            Assert.True(Toolbox.ByteMatch(data, benString.Encode()));
        }

        [Fact]
        public void BenStringEncodingBuffered()
        {
            byte[] data = Encoding.UTF8.GetBytes("22:this is my test string");

            BEncodedString benString = new BEncodedString("this is my test string");
            byte[] result = new byte[benString.LengthInBytes()];
            benString.Encode(result, 0);
            Assert.True(Toolbox.ByteMatch(data, result));
        }

        [Fact]
        public void BenStringLengthInBytes()
        {
            string text = "thisisateststring";

            BEncodedString str = text;
            int length = text.Length;
            length += text.Length.ToString().Length;
            length++;

            Assert.Equal(length, str.LengthInBytes());
        }

        [Fact]
        public void CorruptBenDataDecode()
        {
            string testString = "corruption!";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.UTF8.GetBytes(testString)));
        }


        [Fact]
        public void CorruptBenDictionaryDecode()
        {
            string testString = "d3:3521:a3:aedddd";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.UTF8.GetBytes(testString)));
        }

        [Fact]
        public void CorruptBenListDecode()
        {
            string testString = "l3:3521:a3:ae";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.UTF8.GetBytes(testString)));
        }

        [Fact]
        public void CorruptBenNumberDecode()
        {
            string testString = "i35212";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.UTF8.GetBytes(testString)));
        }

        [Fact]
        public void CorruptBenStringDecode()
        {
            string testString = "50:i'm too short";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.UTF8.GetBytes(testString)));
        }

        [Fact]
        public void CorruptBenStringDecode2()
        {
            string s = "d8:completei2671e10:incompletei669e8:intervali1836e12min intervali918e5:peers0:e";

            Assert.Throws<BEncodingException>(() => BEncodedValue.Decode(Encoding.ASCII.GetBytes(s)));
        }

        [Fact]
        public void Utf8Test()
        {
            string s = "ã";
            BEncodedString str = s;
            Assert.Equal(s, str.Text);
        }

        //[Fact]
        //public void EncodingUTF32()
        //{
        //    UTF8Encoding enc8 = new UTF8Encoding();
        //    UTF32Encoding enc32 = new UTF32Encoding();
        //    BEncodedDictionary val = new BEncodedDictionary();

        //    val.Add("Test", (BEncodedNumber)1532);
        //    val.Add("yeah", (BEncodedString)"whoop");
        //    val.Add("mylist", new BEncodedList());
        //    val.Add("mydict", new BEncodedDictionary());

        //    byte[] utf8Result = val.Encode();
        //    byte[] utf32Result = val.Encode(enc32);

        //    Assert.Equal(enc8.GetString(utf8Result), enc32.GetString(utf32Result));
        //}
    }
}