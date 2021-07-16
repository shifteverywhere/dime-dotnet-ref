//
//  Crypto.cs
//  DiME - Digital Identity Message Envelope
//  Compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;
using NSec.Cryptography;

namespace ShiftEverywhere.DiME
{
    public static class Crypto
    {
        public const Profile DEFUALT_PROFILE = Profile.Uno;
        
        public static bool SupportedProfile(Profile profile)
        {
            return profile == Crypto.DEFUALT_PROFILE;
        }
        
        public static string GenerateSignature(string data, KeyBox keybox)
        {
            if (keybox == null) { throw new ArgumentNullException(nameof(keybox), "Unable to sign, keybox must not be null."); }
            if (!Crypto.SupportedProfile(keybox.Profile)) { throw new NotSupportedException(); }
            if (keybox.RawKey == null) { throw new ArgumentNullException(nameof(keybox), "Unable to sign, key in keybox must not be null."); }
            if (keybox.Type != KeyType.Identity) { throw new ArgumentException($"Unable to sign, wrong key type provided, got: {keybox.Type}, expected: KeyType.Identity."); }
            Key key = Key.Import(SignatureAlgorithm.Ed25519, keybox.RawKey, KeyBlobFormat.RawPrivateKey);
            byte[] rawSignature = SignatureAlgorithm.Ed25519.Sign(key, Encoding.UTF8.GetBytes(data));
            return System.Convert.ToBase64String(Utility.Prefix((byte)keybox.Profile, rawSignature)).Trim('=');
        }

        public static void VerifySignature(string data, string signature, KeyBox keybox)
        {
            if (keybox == null) { throw new ArgumentNullException(nameof(keybox), "Unable to verify signature, keybox must not be null."); }
            if (!Crypto.SupportedProfile(keybox.Profile)) { throw new UnsupportedProfileException(); }
            if (data == null) { throw new ArgumentNullException(nameof(data), "Data must not be null."); }
            if (signature == null) { throw new ArgumentNullException(nameof(signature), "Signature must not be null."); }
            if (keybox.RawPublicKey == null) { throw new ArgumentNullException(nameof(keybox), "Unable to sign, public key in keybox must not be null."); }
            if (keybox.Type != KeyType.Identity) { throw new ArgumentException($"Unable to sign, wrong key type provided, got: {keybox.Type}, expected: KeyType.Identity."); }
            byte[] rawSignature = Utility.FromBase64(signature);
            if ((Profile)rawSignature[0] != keybox.Profile) { throw new KeyMismatchException("Signature profile does not match key profile version."); }
            PublicKey verifyKey = PublicKey.Import(SignatureAlgorithm.Ed25519, keybox.RawPublicKey, KeyBlobFormat.RawPublicKey);
            if (!SignatureAlgorithm.Ed25519.Verify(verifyKey, Encoding.UTF8.GetBytes(data), Utility.SubArray(rawSignature, 1)))
            {
                throw new IntegrityException();
            }
        }

        public static KeyBox GenerateKeyBox(Profile profile, KeyType type)
        {
            if (!Crypto.SupportedProfile(profile)) { throw new UnsupportedProfileException(); }
            Key key;
            KeyCreationParameters parameters = new KeyCreationParameters();
            parameters.ExportPolicy = KeyExportPolicies.AllowPlaintextExport;
            switch (type)
            {
                case KeyType.Identity:
                    key = new Key(SignatureAlgorithm.Ed25519, parameters);
                    break;
                case KeyType.Exchange:
                    key = new Key(KeyAgreementAlgorithm.X25519, parameters);
                    break;
                default:
                    throw new ArgumentException("Unkown key type.", nameof(type));
            }
            return new KeyBox(Guid.NewGuid(), 
                               type, 
                               Crypto.ExportKey(key, KeyBlobFormat.RawPrivateKey),
                               Crypto.ExportKey(key, KeyBlobFormat.RawPublicKey),
                               profile);
        }

        #region -- KEY AGREEMENT --

        public static Key GenerateSharedSecret(KeyBox localKeybox, KeyBox remoteKeybox, byte[] salt, byte[] info)
        {  
            if (localKeybox.Profile != remoteKeybox.Profile) { throw new KeyMismatchException("Unable to generate shared key, source keys from diffrent profiles."); }
            if (!SupportedProfile(localKeybox.Profile)) { throw new UnsupportedProfileException(); }
            if (localKeybox.Type != KeyType.Exchange || remoteKeybox.Type != KeyType.Exchange) { throw new KeyMismatchException("Keys must be of type 'Exchange'."); }
            Key privateKey = Key.Import(KeyAgreementAlgorithm.X25519, localKeybox.RawKey, KeyBlobFormat.RawPrivateKey);
            PublicKey publicKey = PublicKey.Import(KeyAgreementAlgorithm.X25519, remoteKeybox.RawPublicKey, KeyBlobFormat.RawPublicKey);
            SharedSecret shared = KeyAgreementAlgorithm.X25519.Agree(privateKey, publicKey);
            return KeyDerivationAlgorithm.HkdfSha256.DeriveKey(shared, salt, info, AeadAlgorithm.ChaCha20Poly1305);  
        }

        #endregion

        #region -- ENCRYPTION/DECRYPTION --

        public static byte[] Encrypt(byte[] plainText, Key key)     
        {
            if (plainText == null || plainText.Length == 0) { throw new ArgumentNullException(nameof(plainText), "Plain text to encrypt must not be null and not have a length of 0."); }
            if (key == null) { throw new ArgumentNullException(nameof(key), "Key must not be null."); }
            byte[] nonce = Utility.RandomBytes(12);
            byte[] cipherText = AeadAlgorithm.ChaCha20Poly1305.Encrypt(key, nonce, null, plainText);
            byte[] attached = Utility.Combine(nonce, cipherText);
            return Utility.Prefix((byte)Crypto.DEFUALT_PROFILE, attached);
        }

        public static byte[] Decrypt(byte[] cipherText, Key key)
        {
            if (cipherText == null ||cipherText.Length == 0) { throw new ArgumentNullException(nameof(cipherText), "Cipher text to decrypt must not be null and not have a length of 0."); }
            if (key == null) { throw new ArgumentNullException(nameof(key), "Key must not be null."); }
            if (!Crypto.SupportedProfile((Profile)cipherText[0])) { throw new UnsupportedProfileException(); }
            byte[] nonce = Utility.SubArray(cipherText, 1, 12);
            byte[] data = Utility.SubArray(cipherText, 13);
            return AeadAlgorithm.ChaCha20Poly1305.Decrypt(key, nonce, null, data);
        }

        #endregion

        #region -- HASHING --

        public static byte[] GenerateHash(Profile profile, string data)
        {
            return Crypto.GenerateHash(profile, Encoding.UTF8.GetBytes(data));
        }

        public static byte[] GenerateHash(Profile profile, byte[] data)
        {
            if (!Crypto.SupportedProfile(profile)) { throw new UnsupportedProfileException(); }
            return HashAlgorithm.Blake2b_256.Hash(data);
        }

        #endregion

        private static byte[] ExportKey(Key key, KeyBlobFormat keyBlobFormat)
        {
            var blob = new byte[key.GetExportBlobSize(keyBlobFormat)];
            var blobSpan = new Span<byte>(blob);
            int blobSize = 0;
            key.TryExport(keyBlobFormat, blobSpan, out blobSize);
            return blob;
        }

    }

}
