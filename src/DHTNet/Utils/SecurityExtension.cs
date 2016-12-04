using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DHTNet.Utils
{
    //See http://www.bittorrent.org/beps/bep_0042.html
    public static class SecurityExtension
    {
        private static readonly Random _random = new Random();
        private static readonly byte[] _ipv4Mask = { 0x03, 0x0f, 0x3f, 0xff };
        private static readonly byte[] _ipv6Mask = { 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

        public static uint CalculatePrefix(IPAddress ip, int rand)
        {
            byte[] ipBytes = ip.GetAddressBytes(); //Already big endian

            if (ip.AddressFamily == AddressFamily.InterNetwork)
                InPlaceAnd(ipBytes, _ipv4Mask);
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                InPlaceAnd(ipBytes, _ipv6Mask);
            else
                throw new ArgumentException("Unsupported network type");

            byte[] mask = ipBytes.Length == 4 ? _ipv4Mask : _ipv6Mask;

            for (int i = 0; i < ipBytes.Length; ++i)
                ipBytes[i] &= mask[i];

            byte r = (byte)(rand & 0x7);
            ipBytes[0] |= (byte)(r << 5);

            return Crc32C.Calculate(0, ipBytes, 0, ipBytes.Length);
        }

        public static void CalculateHardenedId(IPAddress addr, byte[] nodeId)
        {
            int seed = _random.Next(0, int.MaxValue) & 0xff;
            uint crc32Hash = CalculatePrefix(addr, seed);
            nodeId[0] = (byte)((crc32Hash >> 24) & 0xff);
            nodeId[1] = (byte)((crc32Hash >> 16) & 0xff);
            nodeId[2] = (byte)((crc32Hash >> 8) & 0xf8);
            nodeId[2] ^= (byte)(_random.Next() & 0x7);

            //need to change all bits except the first 5, xor randomizes the rest of the bits
            for (int i = 3; i < 19; i++)
                nodeId[i] = (byte)(_random.Next() & 0xff);

            nodeId[19] = (byte)seed;
        }

        public static bool VerifyHardenedId(IPAddress addr, byte[] nodeId)
        {
            if (GetLocalIPs().Contains(addr))
                return true;

            int seed = nodeId[19];
            uint crc32Hash = CalculatePrefix(addr, seed);

            //Compare the first 21 bits only
            byte fromHash = (byte)((crc32Hash >> 8) & 0xff);
            byte fromNode = nodeId[2];
            bool firstValid = nodeId[0] == (byte)((crc32Hash >> 24) & 0xff);
            bool secondValid = nodeId[1] == (byte)((crc32Hash >> 16) & 0xff);
            bool thirdValid = (fromHash & 0xf8) == (fromNode & 0xf8);

            return firstValid && secondValid && thirdValid;
        }

        private static void InPlaceAnd(byte[] source, byte[] mask)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                source[i] &= mask[i];
            }
        }

        private static IEnumerable<IPAddress> GetLocalIPs()
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType != NetworkInterfaceType.Ethernet || item.OperationalStatus != OperationalStatus.Up)
                    continue;

                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork || ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        yield return ip.Address;
                    }
                }
            }
        }
    }
}
