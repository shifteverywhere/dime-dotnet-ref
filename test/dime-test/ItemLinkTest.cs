//
//  IdentityTests.cs
//  DiME - Data Identity Message Envelope
//  A powerful universal data format that is built for secure, and integrity protected communication between trusted
//  entities in a network.
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2024 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Collections.Generic;
using DiME;
using DiME.Capability;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiME_test;

[TestClass]
public class ItemLinkTest
{
    [TestMethod]
    public void ItemLinkTest1() {
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        var link = new ItemLink(key, Dime.Crypto.DefaultSuiteName);
        Assert.IsNotNull(link);
        Assert.AreEqual(key.Header, link.ItemIdentifier);
        Assert.AreEqual(key.GenerateThumbprint(), link.Thumbprint);
        Assert.AreEqual(key.GetClaim<Guid>(Claim.Uid), link.UniqueId);
    }

    // itemLinkTest2 not relevant, as found in Java ref impl.
    
    [TestMethod]
    public void ItemLinkTest3() {
        var key = Key.Generate(KeyCapability.Sign);
        var link = new ItemLink(Key.ItemHeader, key.GenerateThumbprint(), key.GetClaim<Guid>(Claim.Uid), Dime.Crypto.DefaultSuiteName);
        Assert.IsNotNull(link);
        Assert.AreEqual(Key.ItemHeader, link.ItemIdentifier);
        Assert.AreEqual(key.GenerateThumbprint(), link.Thumbprint);
        Assert.AreEqual(key.GetClaim<Guid>(Claim.Uid), link.UniqueId);
        Assert.AreEqual(Dime.Crypto.DefaultSuiteName, link.CryptoSuiteName);
    }

    [TestMethod]
    public void ItemLinkTest4() {
        var key = Key.Generate(new List<KeyCapability>() {KeyCapability.Sign}, null);
        try {
            _ = new ItemLink("", key.GenerateThumbprint(), key.GetClaim<Guid>(Claim.Uid), Dime.Crypto.DefaultSuiteName);
            Assert.IsTrue(false, "Exception not thrown.");
        } catch (ArgumentException) { /* All is well, carry on. */ }
        try {
            _ = new ItemLink(Key.ItemHeader, "", key.GetClaim<Guid>(Claim.Uid), Dime.Crypto.DefaultSuiteName);
            Assert.IsTrue(false, "Exception not thrown.");
        } catch (ArgumentException) { /* All is well, carry on. */ }
        try {
            _ = new ItemLink(Key.ItemHeader, key.GenerateThumbprint(), key.GetClaim<Guid>(Claim.Uid), "");
            Assert.IsTrue(false, "Exception not thrown.");
        } catch (ArgumentException) { /* All is well, carry on. */ }
    }

    [TestMethod]
    public void ToEncodedTest1() {
        var key = Commons.AudienceKey.PublicCopy();
        var link = new ItemLink(key);
        var encoded = link.ToEncoded();
        Assert.IsNotNull(encoded);
        var compare = $"{key.Header}.{key.GetClaim<Guid>(Claim.Uid).ToString()}.{key.GenerateThumbprint()}.{Dime.Crypto.DefaultSuiteName}";
        Assert.AreEqual(compare, encoded);
        Assert.AreNotEqual(Commons.AudienceKey.GenerateThumbprint(), link.Thumbprint);
    }
    
    [TestMethod]
    public void ToEncodedTest2() {
        var key = Commons.AudienceKey.PublicCopy();
        var link = new ItemLink(key.Header, key.GenerateThumbprint(), key.GetClaim<Guid>(Claim.Uid), Dime.Crypto.DefaultSuiteName);
        var encoded = link.ToEncoded();
        Assert.IsNotNull(encoded);
        var compare = $"{key.Header}.{key.GetClaim<Guid>(Claim.Uid).ToString()}.{key.GenerateThumbprint()}.{Dime.Crypto.DefaultSuiteName}";
        Assert.AreEqual(compare, encoded);
        Assert.AreNotEqual(Commons.AudienceKey.GenerateThumbprint(), link.Thumbprint);
    }
    
    [TestMethod]
    public void ToEncodedTest3() {
        var key = Key.Generate(KeyCapability.Sign);
        var link = new ItemLink(key, "STN");
        var encoded = link.ToEncoded();
        Assert.IsNotNull(encoded);
        var compare = $"{key.Header}.{key.GetClaim<Guid>(Claim.Uid).ToString()}.{key.GenerateThumbprint()}";
        Assert.AreEqual(compare, encoded);
        Assert.AreEqual("STN", link.CryptoSuiteName);
    }

    [TestMethod]
    public void VerifyTest1() {
        var link = new ItemLink(Commons.AudienceKey);
        Assert.IsTrue(link.Verify(Commons.AudienceKey));
        Assert.IsFalse(link.Verify(Commons.IssuerKey));
        Assert.IsFalse(link.Verify(Commons.AudienceKey.PublicCopy()));
    }
    
    [TestMethod]
    public void VerifyTest2() 
    {
        var key = Key.Generate(KeyCapability.Sign);
        Assert.AreEqual(Dime.Crypto.DefaultSuiteName, key.CryptoSuiteName);
        var link = new ItemLink(key, "DSC");
        Assert.IsTrue(link.Verify(key));
        Assert.AreEqual("DSC", link.CryptoSuiteName);
    }

    [TestMethod]
    public void VerifyListTest1() {
        var link = new ItemLink(Commons.AudienceKey);
        Assert.IsTrue(Dime.IsIntegrityStateValid(ItemLink.Verify(new List<Item> { Commons.AudienceKey }, new List<ItemLink> { link })));
        Assert.IsFalse(Dime.IsIntegrityStateValid(ItemLink.Verify(new List<Item> { Commons.AudienceKey.PublicCopy() }, new List<ItemLink> { link })));
    }

    [TestMethod]
    public void VerifyListTest2() {
        var items = new List<Item> { Commons.AudienceKey, Commons.AudienceIdentity };
        var revItems = new List<Item> { Commons.AudienceIdentity, Commons.AudienceKey };
        var links = new List<ItemLink> { new ItemLink(Commons.AudienceKey), new ItemLink(Commons.AudienceIdentity) };
        Assert.IsTrue(Dime.IsIntegrityStateValid(ItemLink.Verify(items, links)));
        Assert.IsTrue(Dime.IsIntegrityStateValid(ItemLink.Verify(revItems, links)));
        Assert.IsTrue(Dime.IsIntegrityStateValid(ItemLink.Verify(new List<Item> { Commons.AudienceKey }, links)));
        Assert.IsTrue(Dime.IsIntegrityStateValid(ItemLink.Verify(new List<Item> { Commons.AudienceKey }, links)));
        Assert.IsFalse(Dime.IsIntegrityStateValid(ItemLink.Verify(new List<Item>(), links)));
        Assert.IsFalse(Dime.IsIntegrityStateValid(ItemLink.Verify(items, new List<ItemLink>())));
    }

    [TestMethod]
    public void ToEncodedListTest1() {
        var links = new List<ItemLink>() { new ItemLink(Commons.AudienceIdentity), new ItemLink(Commons.AudienceKey.PublicCopy()) };
        var encoded = ItemLink.ToEncoded(links);
        Assert.IsNotNull(encoded);
        Assert.IsTrue(encoded.StartsWith(Identity.ItemHeader));
        var components = encoded.Split(':');
        Assert.AreEqual(2, components.Length);
    }

    [TestMethod]
    public void ToEncodedListTest2() {
        var links = new List<ItemLink> { new ItemLink(Commons.AudienceIdentity) };
        var encoded = ItemLink.ToEncoded(links);
        Assert.IsNotNull(encoded);
        Assert.IsTrue(encoded.StartsWith(Identity.ItemHeader));
        var components = encoded.Split(':');
        Assert.AreEqual(1, components.Length);
    }

    [TestMethod]
    public void ToEncodedListTest3() {
        var encoded = ItemLink.ToEncoded(new List<ItemLink>());
        Assert.IsNull(encoded);
    }

    [TestMethod]
    public void FromEncodedTest1() {
        const string encoded = "KEY.c89b08d7-f472-4703-b5d3-3d23fd39e10d.68cd898db0b2535c912f6aa5f565306991ba74760b2955e7fb8dc91fd45276bc";
        var link = ItemLink.FromEncoded(encoded);
        Assert.IsNotNull(link);
        Assert.AreEqual("KEY", link.ItemIdentifier);
        Assert.AreEqual(Guid.Parse("c89b08d7-f472-4703-b5d3-3d23fd39e10d"), link.UniqueId);
        Assert.AreEqual("68cd898db0b2535c912f6aa5f565306991ba74760b2955e7fb8dc91fd45276bc", link.Thumbprint);
        Assert.AreEqual("STN", link.CryptoSuiteName);
    }

    [TestMethod]
    public void FromEncodedTest2() {
        const string encoded = "KEY.c89b08d7-f472-4703-b5d3-3d23fd39e10d.68cd898db0b2535c912f6aa5f565306991ba74760b2955e7fb8dc91fd45276bc.DSC";
        var link = ItemLink.FromEncoded(encoded);
        Assert.IsNotNull(link);
        Assert.AreEqual("KEY", link.ItemIdentifier);
        Assert.AreEqual(Guid.Parse("c89b08d7-f472-4703-b5d3-3d23fd39e10d"), link.UniqueId);
        Assert.AreEqual("68cd898db0b2535c912f6aa5f565306991ba74760b2955e7fb8dc91fd45276bc", link.Thumbprint);
        Assert.AreEqual("DSC", link.CryptoSuiteName);
    }
    
    [TestMethod]
    public void FromEncodedTest3() {
        try
        {
            ItemLink.FromEncoded(Commons.Payload);
            Assert.IsTrue(false, "Exception should have been thrown");
        }
        catch (FormatException)
        {
             /* All is well, carry on. */
        }
    }

    [TestMethod]
    public void FromEncodedListTest1() {
        var lnk1 = new ItemLink(Key.Generate(new List<KeyCapability>() { KeyCapability.Sign }, null)).ToEncoded();
        var lnk2 = new ItemLink(Key.Generate(new List<KeyCapability>() { KeyCapability.Exchange }, null)).ToEncoded();
        var lnk3 = new ItemLink(Key.Generate(new List<KeyCapability>() { KeyCapability.Encrypt }, null)).ToEncoded();
        var links = ItemLink.FromEncodedList($"{lnk1}:{lnk2}:{lnk3}");
        Assert.IsNotNull(links);
        Assert.AreEqual(3, links.Count);
    }

    [TestMethod]
    public void FromEncodedListTest2() {
        try 
        {
            ItemLink.FromEncodedList(Commons.Payload);
            Assert.IsTrue(false, "Exception should have been thrown");
        } catch (FormatException) 
        {
            /* All is well, carry on. */
        }
    }
    
}