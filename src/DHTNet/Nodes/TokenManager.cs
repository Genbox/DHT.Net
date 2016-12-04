// Authors:
//   Olivier Dufour <olivier.duff@gmail.com>
//
// Copyright (C) 2008 Olivier Dufour
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

using System;
using System.Security.Cryptography;
using DHTNet.BEncode;

namespace DHTNet.Nodes
{
    internal class TokenManager
    {
        private readonly byte[] _previousSecret;
        private readonly RandomNumberGenerator _random;
        private readonly byte[] _secret;
        private readonly IncrementalHash _sha1;
        private DateTime _lastSecretGeneration;

        public TokenManager()
        {
            _sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            _random = RandomNumberGenerator.Create();
            _lastSecretGeneration = DateTime.MinValue; //in order to force the update
            _secret = new byte[10];
            _previousSecret = new byte[10];

            //PORT NOTE: Used GetNonZeroBytes() here before
            _random.GetBytes(_secret);
            _random.GetBytes(_previousSecret);
        }

        internal TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

        public BEncodedString GenerateToken(Node node)
        {
            return GetToken(node, _secret);
        }

        public bool VerifyToken(Node node, BEncodedString token)
        {
            return token.Equals(GetToken(node, _secret)) || token.Equals(GetToken(node, _previousSecret));
        }

        private BEncodedString GetToken(Node node, byte[] secret)
        {
            //refresh secret if needed
            if (_lastSecretGeneration.Add(Timeout) < DateTime.UtcNow)
            {
                _lastSecretGeneration = DateTime.UtcNow;
                _secret.CopyTo(_previousSecret, 0);

                //PORT NOTE: Used GetNonZeroBytes() here before
                _random.GetBytes(_secret);
            }

            byte[] compactNode = node.CompactAddressPort().TextBytes;

            _sha1.AppendData(compactNode);
            _sha1.AppendData(secret);

            return _sha1.GetHashAndReset();
        }
    }
}