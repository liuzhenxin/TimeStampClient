﻿/*
*  Copyright 2016 Disig a.s.
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

/*
*  Written by:
*  Marek KLEIN <kleinmrk@gmail.com>
*/

using Org.BouncyCastle.Tsp;
using System;

namespace Disig.TimeStampClient
{
    /*
       TimeStampReq ::= SEQUENCE  {
            version               INTEGER  { v1(1) },
            messageImprint        MessageImprint,
            reqPolicy             TSAPolicyId              OPTIONAL,
            nonce                 INTEGER                  OPTIONAL,
            certReq               BOOLEAN                  DEFAULT FALSE,
            extensions            [0] IMPLICIT Extensions  OPTIONAL
       }

       The version field (currently v1) describes the version of the Time-
       Stamp request.
    */

    /// <summary>
    /// Time stamp request
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="hashedMessage">This field contains the hash of the data to be time-stamped.</param>
        /// <param name="hashAlgOid">The hash algorithm identifier.</param>
        /// <param name="nonce">Cryptographic nonce preventing replay attack.</param>
        /// <param name="reqPolicy">Requested policy.</param>
        /// <param name="certReq">Flag indicating that a TSA should include its certificate in a response.</param>
        /// <param name="version">The version of the time stamp request.</param>
        public Request(byte[] hashedMessage, string hashAlgOid, byte[] nonce = null, string reqPolicy = null, bool certReq = false, int version = 1)
        {
            this.Setup(hashedMessage, hashAlgOid, nonce, reqPolicy, certReq, version);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="hashedMessage">This field contains the hash of the data to be time-stamped.</param>
        /// <param name="hashAlg">The hash algorithm identifier.</param>
        /// <param name="nonce">Cryptographic nonce preventing replay attack.</param>
        /// <param name="reqPolicy">Requested policy.</param>
        /// <param name="certReq">Flag indicating that TSA should include its certificate in a response.</param>
        /// <param name="version">The version of the time stamp request.</param>
        public Request(byte[] hashedMessage, Oid hashAlg, byte[] nonce = null, string reqPolicy = null, bool certReq = false, int version = 1)
        {
            if (null != hashAlg)
            {
                this.Setup(hashedMessage, hashAlg.OID, nonce, reqPolicy, certReq, version);
            }
            else
            {
                throw new ArgumentNullException("hashAlg");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="tsrBytes">DER encoded time stamp request</param>
        public Request(byte[] tsrBytes)
        {
            TimeStampRequest req;
            req = new TimeStampRequest(tsrBytes);
            byte[] nonce = null;
            if (null != req.Nonce)
            {
                nonce = req.Nonce.ToByteArray();
            }

            this.Setup(req.GetMessageImprintDigest(), req.MessageImprintAlgOid, nonce, req.ReqPolicy, req.CertReq, req.Version);
        }

        /// <summary>
        /// Gets the version of a time stamp request.
        /// </summary>
        public int Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the requested policy (OID).
        /// </summary>
        public string ReqPolicy
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Nonce, a large random number with a high probability that it is generated by the client only once.
        /// Prevents replay attack.
        /// </summary>
        public byte[] Nonce
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether a TSA should include its certificate in a response.
        /// </summary>
        public bool CertReq
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the hash algorithm identifier.
        /// </summary>
        public string HashAlgorithm
        {
            get
            {
                return this.MessageImprint.HashAlgorithm;
            }
        }

        /// <summary>
        /// Gets the hashed message.
        /// </summary>
        public byte[] HashedMessage
        {
            get
            {
                return this.MessageImprint.HashedMessage;
            }
        }

        /// <summary>
        /// Gets the structure containing the hash of the data to be time-stamped and the algorithm identifier used to compute the hash. 
        /// </summary>
        internal MsgImprint MessageImprint
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns DER encoded time-stamp request.
        /// </summary>
        /// <returns>Byte array containing DER encoded request.</returns>
        public byte[] ToByteArray()
        {
            TimeStampRequestGenerator tsqGenerator = new TimeStampRequestGenerator();
            TimeStampRequest tsreq;

            tsqGenerator.SetCertReq(this.CertReq);
            if (!string.IsNullOrEmpty(this.ReqPolicy))
            {
                tsqGenerator.SetReqPolicy(this.ReqPolicy);
            }

            if (null == this.Nonce)
            {
                tsreq = tsqGenerator.Generate(this.MessageImprint.HashAlgorithm, this.MessageImprint.HashedMessage);
            }
            else
            {
                tsreq = tsqGenerator.Generate(this.MessageImprint.HashAlgorithm, this.MessageImprint.HashedMessage, new Org.BouncyCastle.Math.BigInteger(this.Nonce));
            }

            return tsreq.GetEncoded();
        }

        /// <summary>
        /// Sets properties of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="hashedMessage">This field contains the hash of the data to be time-stamped.</param>
        /// <param name="hashAlgOid">The hash algorithm identifier.</param>
        /// <param name="nonce">Cryptographic nonce preventing replay attack.</param>
        /// <param name="reqPolicy">Requested policy.</param>
        /// <param name="certReq">The flag indicating that TSA should include its certificate in a response.</param>
        /// <param name="version">The version of the time stamp request.</param>
        private void Setup(byte[] hashedMessage, string hashAlgOid, byte[] nonce = null, string reqPolicy = null, bool certReq = false, int version = 1)
        {
            this.Version = version;
            this.MessageImprint = new MsgImprint(hashedMessage, hashAlgOid);
            this.ReqPolicy = reqPolicy;
            this.Nonce = nonce;
            this.CertReq = certReq;
        }
    }
}