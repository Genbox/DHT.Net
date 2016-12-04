using System;
using System.Linq;
using System.Net;
using DHTNet.Utils;
using Xunit;

namespace DHTNet.Tests.Dht
{
    public class SecurityExtensionTests
    {
        [Fact]
        public void CheckHardenedId()
        {
            byte[] nodeId = new byte[20];
            SecurityExtension.CalculateHardenedId(IPAddress.Parse("21.75.31.124"), nodeId);
            bool valid = SecurityExtension.VerifyHardenedId(IPAddress.Parse("21.75.31.124"), nodeId);

            Assert.True(valid);
        }

        [Fact]
        public void CheckHardenedIdCases()
        {
            Assert.Equal("5FBFB", ConvertToUsuable(SecurityExtension.CalculatePrefix(IPAddress.Parse("124.31.75.21"), 1)));
            Assert.Equal("5A3CE", ConvertToUsuable(SecurityExtension.CalculatePrefix(IPAddress.Parse("21.75.31.124"), 86)));
            Assert.Equal("A5D43", ConvertToUsuable(SecurityExtension.CalculatePrefix(IPAddress.Parse("65.23.51.170"), 22)));
            Assert.Equal("1B032", ConvertToUsuable(SecurityExtension.CalculatePrefix(IPAddress.Parse("84.124.73.14"), 65)));
            Assert.Equal("E56F6", ConvertToUsuable(SecurityExtension.CalculatePrefix(IPAddress.Parse("43.213.53.83"), 90)));
        }

        private string ConvertToUsuable(uint crc)
        {
            //Only 21 bytes of the output are deterministic, so in order to tests it we only use 5 characters (20 bytes) of the output
            return BitConverter.ToString(BitConverter.GetBytes(crc).Reverse().ToArray()).Replace("-", string.Empty).Substring(0, 5);
        }
    }
}