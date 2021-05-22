using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShiftEverywhere.DiME;

namespace ShiftEverywhere.DiMETest
{
    [TestClass]
    public class IdentityIssuingRequestTests
    {
        [TestMethod]
        public void GenerateRequestTest1()
        {
            try 
            {
                IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey), 0);
            } 
            catch (Exception e) 
            {
                if (e is ArgumentException) { return; }
                throw e;
            } 
            Assert.IsTrue(false, $"Expected NotSupportedException not thrown");
        }
    
        [TestMethod]
        public void GenerateRequestTest2()
        {
            try 
            {
                IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.ExchangeKey));
            } 
            catch (Exception e) 
            {
                if (e is ArgumentException) { return; }
                throw e;
            } 
            Assert.IsTrue(false, $"Expected ArgumentException not thrown");
        }

        [TestMethod]
        public void GenerateRequestTest3()
        {
            IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
        }

        [TestMethod]
        public void VerifyTest1()
        {
            IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
            iir.Verify();
        }

        [TestMethod]
        public void VerifyTest2()
        {
            Keypair keypair1 = Keypair.GenerateKeypair(KeypairType.IdentityKey);
            Keypair keypair2 = Keypair.GenerateKeypair(KeypairType.IdentityKey);
            IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest( new Keypair(Guid.NewGuid(), KeypairType.IdentityKey, keypair1.publicKey, keypair2.privateKey), 1);
            iir.Verify();
        }

        [TestMethod]
        public void ThumbprintTest1()
        {
            IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
            string thumbprint = iir.Thumbprint();
            Assert.IsNotNull(thumbprint);
            Assert.IsTrue(thumbprint.Length > 0, "Thumbprint should not be empty string");
            Assert.IsTrue(thumbprint == iir.Thumbprint(), "Diffrent thumbprints produced from same claim");
        }

        [TestMethod]
        public void ThumbprintTest2()
        {
            IdentityIssuingRequest iir1 = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
            IdentityIssuingRequest iir2 = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
            Assert.IsFalse(iir1.Thumbprint() == iir2.Thumbprint(), "Thumbprints of diffrent iirs should not be the same");
        }

        [TestMethod]
        public void ExportTest1()
        {
            IdentityIssuingRequest iir = IdentityIssuingRequest.GenerateRequest(Keypair.GenerateKeypair(KeypairType.IdentityKey));
            string encoded = iir.Export();
            Assert.IsNotNull(encoded);
            Assert.IsTrue(encoded.Length > 0);
            Assert.IsTrue(encoded.StartsWith("i1"));
        }

        [TestMethod]
        public void ImportTest1()
        {
            string encoded = "i1.eyJpYXQiOjE2MjEzNjg2MzEsImlreSI6Ik1Db3dCUVlESzJWd0F5RUFcdTAwMkJKVWpIS0JkdGQwbEdta1V4SHI2TXJwSUhaNEpCRk0vSkFHN0gyTEFOTVkifQ.eU8NdFHI58waVsKOT5mSfPuCwofPwHkudlva+NOtZ6ZbOe4uwNPIoLnOko6rbej1jpyyA8Sw/5zxNQg1jBp3Dw";
            IdentityIssuingRequest iir = IdentityIssuingRequest.Import(encoded);
            Assert.IsNotNull(iir);
            Assert.AreEqual(iir.profile, 1);
            Assert.AreEqual(iir.issuedAt, 1621368631);
            Assert.AreEqual(iir.identityKey, "MCowBQYDK2VwAyEA\u002BJUjHKBdtd0lGmkUxHr6MrpIHZ4JBFM/JAG7H2LANMY");
        }

    }

}