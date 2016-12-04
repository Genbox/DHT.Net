using System;
using DHTNet.Nodes;
using Xunit;

namespace DHTNet.Tests.Common
{
    public class InfoHashTests
    {
        private InfoHash Create()
        {
            return new InfoHash(new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
            });
        }

        [Fact]
        public void HexTest()
        {
            InfoHash hash = Create();
            string hex = hash.ToHex();
            Assert.Equal(40, hex.Length);
            InfoHash other = InfoHash.FromHex(hex);
            Assert.Equal(hash, other);
        }

        [Fact]
        public void InvalidHex()
        {
            Assert.Throws<ArgumentException>(() => InfoHash.FromHex("123123123123123123123"));
        }

        [Fact]
        public void NullHex()
        {
            Assert.Throws<ArgumentNullException>(() => InfoHash.FromHex(null));
        }
    }
}