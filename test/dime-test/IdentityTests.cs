//
//  IdentityTests.cs
//  DiME - Data Integrity Message Envelope
//  A powerful universal data format that is built for secure, and integrity protected communication between trusted
//  entities in a network.
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2024 Shift Everywhere AB. All rights reserved.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using DiME;
using DiME.Capability;
using DiME.Exceptions;
using DiME.KeyRing;

namespace DiME_test;

[TestClass]
public class IdentityTests
{

    [TestMethod]
    public void GetHeaderTest1()
    {
        var identity = new Identity();
        Assert.AreEqual("ID", identity.Header);
        Assert.AreEqual("ID", Identity.ItemHeader);
    }
    
    [TestMethod]
    public void ClaimTest1() 
    {
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var identity = IdentityIssuingRequest.Generate(Commons.AudienceKey, caps).SelfIssue(Guid.NewGuid(), Dime.ValidFor1Year, Commons.AudienceKey, Commons.SystemName);
        Assert.IsNotNull(identity.GetClaim<string>(Claim.Pub));
        Assert.AreEqual(Commons.AudienceKey.GetClaim<string>(Claim.Pub), identity.GetClaim<string>(Claim.Pub));
    }

    [TestMethod]
    public void ClaimTest2() 
    {
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var identity = IdentityIssuingRequest.Generate(Commons.AudienceKey, caps).SelfIssue(Guid.NewGuid(), Dime.ValidFor1Year, Commons.AudienceKey, Commons.SystemName);
        identity.Strip();
        identity.PutClaim(Claim.Amb, new List<string>() { "one", "two" });
        Assert.IsNotNull(identity.GetClaim<List<string>>(Claim.Amb));
        identity.PutClaim(Claim.Aud, Guid.NewGuid());
        Assert.IsNotNull(identity.GetClaim<Guid>(Claim.Aud));
        Assert.AreNotEqual(default, identity.GetClaim<Guid>(Claim.Aud));
        identity.PutClaim(Claim.Cmn, Commons.CommonName);
        Assert.IsNotNull(identity.GetClaim<string>(Claim.Cmn));
        identity.PutClaim(Claim.Ctx, Commons.Context);
        Assert.IsNotNull(identity.GetClaim<string>(Claim.Ctx));
        identity.PutClaim(Claim.Exp, DateTime.UtcNow);
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Exp));
        Assert.AreNotEqual(default, identity.GetClaim<DateTime>(Claim.Exp));
        identity.PutClaim(Claim.Iat, DateTime.UtcNow);
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Iat));
        Assert.AreNotEqual(default, identity.GetClaim<DateTime>(Claim.Iat));
        identity.PutClaim(Claim.Iss, Guid.NewGuid());
        Assert.IsNotNull(identity.GetClaim<Guid>(Claim.Iss));
        Assert.AreNotEqual(default, identity.GetClaim<Guid>(Claim.Iss));
        identity.PutClaim(Claim.Isu, Commons.IssuerUrl);
        Assert.IsNotNull(identity.GetClaim<string>(Claim.Isu));
        identity.PutClaim(Claim.Kid, Guid.NewGuid());
        Assert.IsNotNull(identity.GetClaim<Guid>(Claim.Kid));
        Assert.AreNotEqual(default, identity.GetClaim<Guid>(Claim.Kid));
        identity.PutClaim(Claim.Mtd, new List<string>() { "abc", "def" });
        Assert.IsNotNull(identity.GetClaim<List<string>>(Claim.Mtd));
        var pri = new Dictionary<string, object>
        {
            ["tag"] = Commons.Payload
        };
        identity.PutClaim(Claim.Pri, pri);
        Assert.IsNotNull(identity.GetClaim<Dictionary<string, object>>(Claim.Pri));
        Assert.AreNotEqual(default,identity.GetClaim<Dictionary<string, object>>(Claim.Pri));
        identity.PutClaim(Claim.Sub, Guid.NewGuid());
        Assert.IsNotNull(identity.GetClaim<Guid>(Claim.Sub));
        Assert.AreNotEqual(default, identity.GetClaim<Guid>(Claim.Sub));
        identity.PutClaim(Claim.Sys, Commons.SystemName);
        Assert.IsNotNull(identity.GetClaim<string>(Claim.Sys));
        identity.PutClaim(Claim.Uid, Guid.NewGuid());
        Assert.IsNotNull(identity.GetClaim<Guid>(Claim.Uid));
        Assert.AreNotEqual(default, identity.GetClaim<Guid>(Claim.Uid));
        try { identity.PutClaim(Claim.Cap, new List<KeyCapability>() { KeyCapability.Encrypt }); Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentException) { /* all is well */ }
        try { identity.PutClaim(Claim.Key,Commons.IssuerKey.Secret); Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentException) { /* all is well */ }
        try { identity.PutClaim(Claim.Lnk, new ItemLink(Commons.IssuerKey)); Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentException) { /* all is well */ }
        try { identity.PutClaim(Claim.Mim, Commons.Mimetype); Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentException) { /* all is well*/ }
        try { identity.PutClaim(Claim.Pub, Commons.IssuerKey.Public); Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentException) { /* all is well */ }
    }

    [TestMethod]
    public void ClaimTest3() 
    {
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var identity = IdentityIssuingRequest.Generate(Commons.AudienceKey, caps).SelfIssue(Guid.NewGuid(), Dime.ValidFor1Year, Commons.AudienceKey, Commons.SystemName);
        try { identity.RemoveClaim(Claim.Iss); Assert.IsTrue(false, "Exception not thrown."); } catch (InvalidOperationException) { /* all is well */ }
        try { identity.PutClaim(Claim.Exp, DateTime.UtcNow); } catch (InvalidOperationException) { /* all is well */ }
    }

    [TestMethod]
    public void ClaimTest4() 
    {
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var identity = IdentityIssuingRequest.Generate(Commons.AudienceKey, caps).SelfIssue(Guid.NewGuid(), Dime.ValidFor1Year, Commons.AudienceKey, Commons.SystemName);
        identity.Strip();
        identity.RemoveClaim(Claim.Iss);
        identity.PutClaim(Claim.Iat, DateTime.UtcNow);
    }
    
    [TestMethod]
    public void IssueTest1()
    {
        Commons.ClearKeyRing();
        var subjectId = Guid.NewGuid();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);            
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var identity = IdentityIssuingRequest.Generate(key, caps).SelfIssue(subjectId, Dime.ValidFor1Year * 10, key, Commons.SystemName);
        Assert.AreEqual(Commons.SystemName, identity.GetClaim<string>(Claim.Sys));
        Assert.AreEqual(subjectId, identity.GetClaim<Guid>(Claim.Sub));
        Assert.AreEqual(subjectId, identity.GetClaim<Guid>(Claim.Iss));
        Assert.IsTrue(identity.HasCapability(caps[0]));
        Assert.IsTrue(identity.HasCapability(caps[1]));
        Assert.IsTrue(identity.HasCapability(IdentityCapability.Self));
        Assert.IsNotNull(identity.PublicKey);
        Assert.AreEqual(key.Public, identity.PublicKey.Public);
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Iat));
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Exp));
        Assert.IsTrue(identity.GetClaim<DateTime>(Claim.Iat) < identity.GetClaim<DateTime>(Claim.Exp));
        Assert.AreEqual(subjectId, identity.GetClaim<Guid>(Claim.Iss));
    }

    [TestMethod]
    public void IssueTest2()
    {
        Commons.InitializeKeyRing();
        var subjectId = Guid.NewGuid();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Identify };
        var iir = IdentityIssuingRequest.Generate(key, caps);
        var identity = iir.Issue(subjectId, Dime.ValidFor1Year, Commons.IntermediateKey, Commons.IntermediateIdentity, true, caps);
        Assert.AreEqual(Commons.TrustedIdentity.GetClaim<string>(Claim.Sys), identity.GetClaim<string>(Claim.Sys));
        Assert.AreEqual(subjectId, identity.GetClaim<Guid>(Claim.Sub));
        Assert.IsTrue(identity.HasCapability(caps[0]));
        Assert.IsTrue(identity.HasCapability(caps[1]));
        Assert.IsNotNull(identity.PublicKey);
        Assert.AreEqual(key.Public, identity.PublicKey.Public);
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Iat));
        Assert.IsNotNull(identity.GetClaim<DateTime>(Claim.Exp));
        Assert.IsTrue(identity.GetClaim<DateTime>(Claim.Iat) < identity.GetClaim<DateTime>(Claim.Exp));
        Assert.IsTrue(Commons.IntermediateIdentity.GetClaim<Guid>(Claim.Sub) == identity.GetClaim<Guid>(Claim.Iss));
    }

    [TestMethod]
    public void IssueTest3()
    {
        Commons.InitializeKeyRing();
        var reqCaps = new List<IdentityCapability> { IdentityCapability.Issue };
        var allowCaps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Identify };
        try {
            _ = IdentityIssuingRequest.Generate(Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null), reqCaps).Issue(Guid.NewGuid(), 100L, Commons.TrustedKey, Commons.TrustedIdentity, true, allowCaps);
        } catch (CapabilityException) { return; } // All is well
        Assert.IsTrue(false, "Should not happen.");
    }

    [TestMethod]
    public void IssueTest4()
    {
        Commons.InitializeKeyRing();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var caps = new List<IdentityCapability> { IdentityCapability.Issue, IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(key, caps).Issue(Guid.NewGuid(), Dime.ValidFor1Minute, Commons.TrustedKey, Commons.TrustedIdentity, true, caps);
        Assert.IsTrue(identity.HasCapability(IdentityCapability.Issue));
        Assert.IsTrue(identity.HasCapability(IdentityCapability.Generic));
    }

    [TestMethod]
    public void IssueTest5()
    {
        Commons.ClearKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Issue };
        try { _ = IdentityIssuingRequest.Generate(Key.Generate(KeyCapability.Sign), caps).Issue(Guid.NewGuid(), Dime.ValidFor1Minute, Commons.TrustedKey, null, true, caps);  Assert.IsTrue(false, "Exception not thrown."); } catch (ArgumentNullException) { /* all is well */ }
    }

    [TestMethod]
    public void IsSelfSignedTest1()
    {
        Commons.ClearKeyRing();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var identity = IdentityIssuingRequest.Generate(key).SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName);
        Assert.IsTrue(identity.IsSelfSigned);
    }

    [TestMethod]
    public void IsSelfSignedTest2()
    {
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null)).Issue(Guid.NewGuid(), 100L, Commons.IntermediateKey, Commons.IntermediateIdentity, true, caps);
        Assert.IsFalse(identity.IsSelfSigned);
    }

    [TestMethod]
    public void VerifyTest1()
    {
        Commons.ClearKeyRing();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var identity = IdentityIssuingRequest.Generate(key)
            .SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName);
        Assert.IsTrue(identity.IsSelfSigned);
        Assert.IsFalse(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void VerifyTest2()
    {
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null)).Issue(Guid.NewGuid(), 100L, Commons.IntermediateKey, Commons.IntermediateIdentity, true, caps);
        Assert.IsTrue(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void VerifyTest3()
    {
        Commons.ClearKeyRing();
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var identity = IdentityIssuingRequest.Generate(key).SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName);
        Commons.InitializeKeyRing();
        Assert.IsFalse(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void VerifyTest4()
    {
        Commons.InitializeKeyRing();
        Assert.IsTrue(Dime.IsIntegrityStateValid(Commons.IntermediateIdentity.Verify()));
    }
        
    [TestMethod]
    public void VerifyTest5()
    {
        Commons.InitializeKeyRing();
        Assert.IsTrue(Dime.IsIntegrityStateValid(Commons.AudienceIdentity.Verify()));
    }
        
    [TestMethod]
    public void VerifyTest6()
    {
        Commons.ClearKeyRing();
        Assert.IsTrue(Dime.IsIntegrityStateValid(Commons.AudienceIdentity.Verify(Commons.IntermediateIdentity)));
    }
        
    [TestMethod]
    public void VerifyTest7()
    {
        Commons.ClearKeyRing();
        Assert.IsFalse(Dime.IsIntegrityStateValid(Commons.AudienceIdentity.Verify(Commons.IssuerIdentity)));
    }
        
    [TestMethod]
    public void VerifyTest8() 
    {
        Commons.InitializeKeyRing();
        var nodeCaps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var key1 = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var node1 = IdentityIssuingRequest.Generate(key1, nodeCaps).Issue(Guid.NewGuid(), 100L, Commons.TrustedKey, Commons.TrustedIdentity, true, nodeCaps, nodeCaps);
        var key2 = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var node2 = IdentityIssuingRequest.Generate(key2, nodeCaps).Issue(Guid.NewGuid(), 100L, key1, node1, true, nodeCaps, nodeCaps);
        var key3 = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var node3 = IdentityIssuingRequest.Generate(key3, nodeCaps).Issue(Guid.NewGuid(), 100L, key2, node2, true, nodeCaps, nodeCaps);
        var leafCaps = new List<IdentityCapability> { IdentityCapability.Generic };
        var leaf = IdentityIssuingRequest.Generate(Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null), leafCaps).Issue(Guid.NewGuid(), 100L, key3, node3, true, leafCaps, leafCaps);
        Assert.AreEqual(IntegrityState.Complete, leaf.Verify());
        Commons.ClearKeyRing();
        Assert.IsFalse(Dime.IsIntegrityStateValid(leaf.Verify()));
        Assert.AreEqual(IntegrityState.Intact, leaf.Verify(node1));
        Assert.AreEqual(IntegrityState.Intact, leaf.Verify(node2));
        Assert.AreEqual(IntegrityState.Intact, leaf.Verify(node3));
        Assert.IsFalse(Dime.IsIntegrityStateValid(leaf.Verify(Commons.IntermediateIdentity)));
    }
        
    [TestMethod]
    public void VerifyTest9() 
    {
        Commons.InitializeKeyRing();
        var nodeCaps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Issue };
        var key1 = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var node1 = IdentityIssuingRequest.Generate(key1, nodeCaps).Issue(Guid.NewGuid(), 100L, Commons.TrustedKey, Commons.TrustedIdentity, false, nodeCaps, nodeCaps);
        var key2 = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var node2 = IdentityIssuingRequest.Generate(key2, nodeCaps).Issue(Guid.NewGuid(), 100L, key1, node1, false, nodeCaps, nodeCaps);
        var leafCaps = new List<IdentityCapability> { IdentityCapability.Generic };
        var leaf = IdentityIssuingRequest.Generate(Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null), leafCaps).Issue(Guid.NewGuid(), 100L, key2, node2, false, leafCaps, leafCaps);
        Assert.IsFalse(Dime.IsIntegrityStateValid(leaf.Verify()));
        Assert.IsFalse(Dime.IsIntegrityStateValid(leaf.Verify(node1)));
        Assert.IsTrue(Dime.IsIntegrityStateValid(leaf.Verify(node2)));
        Assert.IsFalse(Dime.IsIntegrityStateValid(leaf.Verify(Commons.IntermediateIdentity)));
    }
        
    [TestMethod] 
    public void VerifyTest10()
    {
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(Key.Generate(KeyCapability.Sign)).Issue(Guid.NewGuid(), 1L, Commons.TrustedKey, Commons.TrustedIdentity, false, caps, caps);
        Thread.Sleep(1001);
        Assert.IsFalse(Dime.IsIntegrityStateValid(identity.Verify()));
        Dime.GracePeriod = 1L;
        Assert.IsTrue(Dime.IsIntegrityStateValid(identity.Verify()));
        Dime.GracePeriod = 0L;
    }

    [TestMethod]
    public void VerifyTest11() 
    {
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(Key.Generate(KeyCapability.Sign)).Issue(Guid.NewGuid(), 1L, Commons.TrustedKey, Commons.TrustedIdentity, false, caps, caps);
        Thread.Sleep(2000);
        Dime.TimeModifier = -2L;
        Assert.IsTrue(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void VerifyTest12() 
    {
        Dime.TimeModifier = -2L;
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic };
        var identity = IdentityIssuingRequest.Generate(Key.Generate(KeyCapability.Sign)).Issue(Guid.NewGuid(), 1L, Commons.TrustedKey, Commons.TrustedIdentity, false, caps, caps);
        Thread.Sleep(2000);
        Assert.IsFalse(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void ExportTest1()
    {
        Commons.InitializeKeyRing();
        var caps = new List<IdentityCapability> { IdentityCapability.Generic, IdentityCapability.Identify };
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var identity = IdentityIssuingRequest.Generate(key, caps).Issue(Guid.NewGuid(), Dime.ValidFor1Year, Commons.IntermediateKey, Commons.IntermediateIdentity, true, caps);
        var exported = identity.Export();
        Assert.IsNotNull(exported);
        Assert.IsTrue(exported.Length > 0);
        Assert.IsTrue(exported.StartsWith($"{Envelope.ItemHeader}:{Identity.ItemHeader}"));
        Assert.AreEqual(4, exported.Split(new[] { '.' }).Length);
    }

    [TestMethod]
    public void ImportTest1()
    {
        Commons.InitializeKeyRing();
        var identity = Item.Import<Identity>(Commons.EncodedIssuerIdentity);
        Assert.IsNotNull(identity);
        Assert.AreEqual(Commons.SystemName, identity.GetClaim<string>(Claim.Sys));
        Assert.AreEqual(Commons.IssuerIdentity.GetClaim<Guid>(Claim.Uid), identity.GetClaim<Guid>(Claim.Uid));
        Assert.AreEqual(Commons.IssuerIdentity.GetClaim<Guid>(Claim.Sub), identity.GetClaim<Guid>(Claim.Sub));
        Assert.AreEqual(Commons.IssuerIdentity.GetClaim<DateTime>(Claim.Iat), identity.GetClaim<DateTime>(Claim.Iat));
        Assert.AreEqual(Commons.IssuerIdentity.GetClaim<DateTime>(Claim.Exp), identity.GetClaim<DateTime>(Claim.Exp));
        Assert.AreEqual(Commons.IntermediateIdentity.GetClaim<Guid>(Claim.Sub), identity.GetClaim<Guid>(Claim.Iss));
        Assert.IsNotNull(identity.PublicKey);
        Assert.IsNotNull(Commons.IssuerIdentity.PublicKey);
        Assert.AreEqual(Commons.IssuerIdentity.PublicKey.Public, identity.PublicKey.Public);
        Assert.IsTrue(identity.HasCapability(IdentityCapability.Generic));
        Assert.IsTrue(identity.HasCapability(IdentityCapability.Identify));
        Assert.IsNotNull(identity.TrustChain);
        Assert.IsTrue(Dime.IsIntegrityStateValid(identity.Verify()));
    }

    [TestMethod]
    public void AmbitTest1() {
        var ambitList = new List<string>() { "global", "administrator" };
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
            
        var identity1 = IdentityIssuingRequest.Generate(key).SelfIssue(Guid.NewGuid(), 100, key, Commons.SystemName, ambitList);
        Assert.IsNotNull(identity1);
        var ambit1 = identity1.GetClaim<List<string>>(Claim.Amb);
        Assert.IsNotNull(ambit1);
        Assert.AreEqual(2, ambit1.Count);
        Assert.IsTrue(identity1.HasAmbit(ambitList[0]));
        Assert.IsTrue(identity1.HasAmbit(ambitList[1]));

        var identity2 = Item.Import<Identity>(identity1.Export());
        Assert.IsNotNull(identity2);
        var ambit2 = identity2.GetClaim<List<string>>(Claim.Amb);
        Assert.IsNotNull(ambit2);
        Assert.AreEqual(2, ambit2.Count);
        Assert.IsTrue(identity2.HasAmbit(ambitList[0]));
        Assert.IsTrue(identity2.HasAmbit(ambitList[1]));
    }

    [TestMethod]
    public void MethodsTest1() {
        var methods = new List<string> { "dime", "sov" };
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);

        var identity1 = IdentityIssuingRequest.Generate(key).SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName, null, methods);
        Assert.IsNotNull(identity1);
        var methods1 = identity1.GetClaim<List<string>>(Claim.Mtd);
        Assert.IsNotNull(methods1);
        Assert.AreEqual(2, methods1.Count);
        Assert.IsTrue(methods1.Contains(methods[0]));
        Assert.IsTrue(methods1.Contains(methods[1]));

        var identity2 = Item.Import<Identity>(identity1.Export());
        Assert.IsNotNull(identity2);
        var methods2 = identity2.GetClaim<List<string>>(Claim.Mtd);
        Assert.IsNotNull(methods2);
        Assert.AreEqual(2, methods2.Count);
        Assert.IsTrue(methods2.Contains(methods[0]));
        Assert.IsTrue(methods2.Contains(methods[1]));
    }

    [TestMethod]
    public void PrinciplesTest1() {
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var principles = new Dictionary<string, dynamic>
        {
            ["tag"] = Commons.Payload,
            ["nbr"] = new[] { "one" , "two", "three" }
        };
        var identity = IdentityIssuingRequest.Generate(key, new List<IdentityCapability>() { IdentityCapability.Generic }, principles).SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName);
        Assert.IsNotNull(identity.Principles);
        Assert.AreEqual( Commons.Payload, identity.Principles["tag"]);
        var nbr = (string[])identity.Principles["nbr"]; // This identity if not exported, string[] is expected
        Assert.AreEqual(3, nbr.Length);
        Assert.AreEqual("two", nbr[1]);
    }

    [TestMethod]
    public void PrinciplesTest2() {
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var principles = new Dictionary<string, dynamic>
        {
            ["tag"] =  Commons.Payload,
            ["nbr"] = new[] { "one" , "two", "three" }
        };
        var identity1 =  IdentityIssuingRequest.Generate(key, new List<IdentityCapability>() { IdentityCapability.Generic }, principles).SelfIssue(Guid.NewGuid(), 100L, key, Commons.SystemName);
        var identity2 = Item.Import<Identity>(identity1.Export());
        Assert.IsNotNull(identity2.Principles);
        Assert.AreEqual( Commons.Payload, identity2.Principles["tag"]);
        var nbr = (List<string>) identity2.Principles["nbr"]; 
        Assert.AreEqual(3, nbr.Count);
        Assert.AreEqual("three", nbr[2]);
    }
    
}