//
//  IdentityTests.cs
//  DiME - Digital Identity Message Envelope
//  A secure and compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ShiftEverywhere.DiME;

namespace ShiftEverywhere.DiMETest
{
    [TestClass]
    public class IdentityTests
    {

     [TestMethod]
        public void IssueTest1()
        {
            Dime.SetTrustedIdentity(null);
            ProfileVersion profile = ProfileVersion.One;
            Guid subjectId = Guid.NewGuid();
            KeyBox keypair = KeyBox.Generate(KeyType.Identity, profile);
            //string key = keypair.ToString();
            List<Capability> caps = new List<Capability> { Capability.Generic, Capability.Issue };
            Identity identity = IdentityIssuingRequest.Generate(keypair, caps).IssueIdentity(subjectId, 100, caps,  keypair,  null);
            //Identity identity = IdentityIssuingRequest.Generate(keypair, caps).IssueIdentity(subjectId, Dime.VALID_FOR_1_YEAR * 10, caps,  keypair,  null);
            //string id = identity.ToString();
            Assert.IsTrue(subjectId == identity.SubjectId);
            Assert.IsTrue(subjectId == identity.IssuerId);
            Assert.IsTrue(identity.HasCapability(caps[0]));
            Assert.IsTrue(identity.HasCapability(caps[1]));
            Assert.IsTrue(identity.HasCapability(Capability.Self));
            Assert.IsTrue(keypair.PublicKey == identity.IdentityKey);
            Assert.IsTrue(identity.IssuedAt != 0);
            Assert.IsTrue(identity.IssuedAt < identity.ExpiresAt);
            Assert.IsTrue(subjectId == identity.IssuerId);
        }

        [TestMethod]
        public void IssueTest2()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            Guid subjectId = Guid.NewGuid();
            KeyBox keypair = KeyBox.Generate(KeyType.Identity, ProfileVersion.One);
            //string key = keypair.ToString();
            List<Capability> caps = new List<Capability> { Capability.Generic, Capability.Identify };
            //List<Capability> caps = new List<Capability> { Capability.Generic, Capability.Identify, Capability.Issue };
            IdentityIssuingRequest iir = IdentityIssuingRequest.Generate(keypair, caps);
            Identity identity = IdentityIssuingRequest.Generate(keypair, caps).IssueIdentity(subjectId, Dime.VALID_FOR_1_YEAR, caps, Commons.IntermediateKeybox, Commons.IntermediateIdentity);
            //Identity identity = IdentityIssuingRequest.Generate(keypair, caps).IssueIdentity(subjectId, Dime.VALID_FOR_1_YEAR * 5, caps, Commons.TrustedKeybox, Commons.TrustedIdentity);
            //string id = identity.ToString();
            Assert.IsTrue(subjectId == identity.SubjectId);
            Assert.IsTrue(identity.HasCapability(caps[0]));
            Assert.IsTrue(identity.HasCapability(caps[1]));
            Assert.IsTrue(keypair.PublicKey == identity.IdentityKey);
            Assert.IsTrue(identity.IssuedAt != 0);
            Assert.IsTrue(identity.IssuedAt < identity.ExpiresAt);
            Assert.IsTrue(Commons.IntermediateIdentity.SubjectId == identity.IssuerId);
        }

       [TestMethod]
        public void IssueTest3()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            List<Capability> reqCaps = new List<Capability> { Capability.Issue };
            List<Capability> allowCaps = new List<Capability> { Capability.Generic, Capability.Identify };
            try {
                Identity identity = IdentityIssuingRequest.Generate(KeyBox.Generate(KeyType.Identity), reqCaps).IssueIdentity(Guid.NewGuid(), 100, allowCaps, Commons.TrustedKeybox, Commons.TrustedIdentity);
            } catch (IdentityCapabilityException) { return; } // All is well
            Assert.IsTrue(false, "Should not happen.");
        }

        [TestMethod]
        public void IssueTest4()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            KeyBox keypair = KeyBox.Generate(KeyType.Identity);
            List<Capability> caps = new List<Capability> { Capability.Issue, Capability.Generic };
            Identity identity = IdentityIssuingRequest.Generate(keypair, caps).IssueIdentity(Guid.NewGuid(), 100, caps, Commons.TrustedKeybox, Commons.TrustedIdentity);
            Assert.IsTrue(identity.HasCapability(Capability.Issue));
            Assert.IsTrue(identity.HasCapability(Capability.Generic));
        }

        [TestMethod]
        public void IsSelfSignedTest1()
        {
            Dime.SetTrustedIdentity(null);
            KeyBox keypair = KeyBox.Generate(KeyType.Identity);
            List<Capability> caps = new List<Capability> { Capability.Issue, Capability.Generic };
            Identity identity = IdentityIssuingRequest.Generate(keypair).IssueIdentity(Guid.NewGuid(), 100, null, keypair, null);
            Assert.IsTrue(identity.IsSelfSigned);
        }

        [TestMethod]
        public void IsSelfSignedTest2()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            List<Capability> caps = new List<Capability> { Capability.Generic };
            Identity identity = IdentityIssuingRequest.Generate(KeyBox.Generate(KeyType.Identity)).IssueIdentity(Guid.NewGuid(), 100, caps, Commons.IntermediateKeybox, Commons.IntermediateIdentity);
            Assert.IsFalse(identity.IsSelfSigned);
        }

        [TestMethod]
        public void VerifyTrustTest1()
        {
            try {
                Dime.SetTrustedIdentity(null);
                List<Capability> caps = new List<Capability> { Capability.Generic };
                KeyBox keypair = KeyBox.Generate(KeyType.Identity);
                Identity identity = IdentityIssuingRequest.Generate(keypair).IssueIdentity(Guid.NewGuid(), 100, null, keypair, null);
                Assert.IsTrue(identity.IsSelfSigned);
                identity.VerifyTrust();
            } catch (UntrustedIdentityException) { return; } // All is well
            Assert.IsTrue(false, "This should not happen.");
        }

        [TestMethod]
        public void VerifyTrustTest2()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            List<Capability> caps = new List<Capability> { Capability.Generic };
            Identity identity = IdentityIssuingRequest.Generate(KeyBox.Generate(KeyType.Identity)).IssueIdentity(Guid.NewGuid(), 100, caps, Commons.IntermediateKeybox, Commons.IntermediateIdentity);
            identity.VerifyTrust();
        }

        [TestMethod]
        public void VerifyTrustTest3()
        {
            Dime.SetTrustedIdentity(null);
            Capability[] caps = new Capability[1] { Capability.Generic };
            KeyBox keypair = KeyBox.Generate(KeyType.Identity);
            Identity identity = IdentityIssuingRequest.Generate(keypair).IssueIdentity(Guid.NewGuid(), 100, null, keypair, null);
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            try {
                identity.VerifyTrust();
            } catch (UntrustedIdentityException) { return; } // All is well
            Assert.IsTrue(false, "This should not happen.");
        }

        [TestMethod]
        public void VerifyTrustTest4()
        {
            Dime.SetTrustedIdentity(Commons.TrustedIdentity);
            Commons.IntermediateIdentity.VerifyTrust();
        }

        [TestMethod]
        public void ToStringTest1()
        {
            Dime.SetTrustedIdentity(null);
            Capability[] caps = new Capability[1] { Capability.Generic };
            KeyBox keypair = Crypto.GenerateKeyPair(ProfileVersion.One, KeyType.Identity);
            Identity identity = IdentityIssuingRequest.Generate(keypair).IssueIdentity(Guid.NewGuid(), 100, null, keypair, null);
            string exported = identity.ToString();
            Assert.IsNotNull(exported);
            Assert.IsTrue(exported.Length > 0);
            Assert.IsTrue(exported.StartsWith(Identity.IID));
            Assert.IsTrue(exported.Split(new char[] { '.' }).Length == 3);
        }

        [TestMethod]
        public void FromStringTest1()
        {
            Dime.SetTrustedIdentity(null);
            string exported = "aWQ.eyJ1aWQiOiI5OWYzMWJlMi0yMTc0LTRhY2UtYmI3Ny00NWVhZjg5NmEwZTMiLCJzdWIiOiI2ZDViZDYxMy0xMWIyLTQxNmYtOGE4ZC05YTE0Y2NjYjg1N2EiLCJpc3MiOiI2ZDViZDYxMy0xMWIyLTQxNmYtOGE4ZC05YTE0Y2NjYjg1N2EiLCJpYXQiOjE2MjYyMDgxMDYsImV4cCI6MTYyNjIwODIwNiwiaWt5IjoiQ1lIdDcyQ05jVFVjclJ6a1J3Z0UyNmFvb2tyNnIyZGRReEtqVFV6Wm1jb2hkOFJjUmdMQ1k5IiwiY2FwIjpbImdlbmVyaWMiLCJzZWxmIl19.tnYkcjBVrKGxmKBM17da3z+zAAKgmfzKqBMNicsK5TnIZTpEKe41cwM8UIbCIi6hmaj4ZYZF9ocqS5T0PgGODw";
            Identity identity = Identity.FromString(exported);
            Assert.IsNotNull(identity);
            Assert.AreEqual(new Guid("99f31be2-2174-4ace-bb77-45eaf896a0e3"), identity.UID);
            Assert.AreEqual(new Guid("6d5bd613-11b2-416f-8a8d-9a14cccb857a"), identity.SubjectId);
            Assert.AreEqual(1626208106, identity.IssuedAt);
            Assert.AreEqual(1626208206, identity.ExpiresAt);
            Assert.AreEqual(new Guid("6d5bd613-11b2-416f-8a8d-9a14cccb857a"), identity.IssuerId);
            Assert.AreEqual("CYHt72CNcTUcrRzkRwgE26aookr6r2ddQxKjTUzZmcohd8RcRgLCY9", identity.IdentityKey);
            Assert.IsTrue(identity.HasCapability(Capability.Generic));
            Assert.IsTrue(identity.HasCapability(Capability.Self));
            Assert.IsNull(identity.TrustChain);
        }

    }

}
