//
//  Dime.cs
//  DiME - Digital Identity Message Envelope
//  Compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;

namespace ShiftEverywhere.DiME
{
    public abstract class Dime
    {
        #region -- PUBLIC --

        public const string HEADER = "DiME";
        public const long VALID_FOR_1_YEAR = 365 * 24 * 60 * 60; 

        ///<summary>A shared trusted identity that acts as the root identity in the trust chain.</summary>
        public static Identity TrustedIdentity { get { lock(Dime._lock) { return Dime._trustedIdentity; } } }

        public ProfileVersion Profile { get { return this._profile; } protected set { Crypto.SupportedProfile(value); this._profile = value; } }
        public abstract Guid Id { get; }

        public bool HasVerifyToken { get { return (this._verifiedToken != null); } }

        ///<summary>Set the shared trusted identity, which forms the basis of the trust chain. All identities will be verified
        /// from a trust perspecitve using this identity. For the trust chain to hold, then all identities must be either issued
        /// by this identity or other identities (with the 'issue' capability) that has been issued by this identity.
        ///<param name="identity">The identity to set as the trusted identity.</param>
        public static void SetTrustedIdentity(Identity identity)
        {
            lock(Dime._lock)
            {
                Dime._trustedIdentity = identity;
            }
        }

        ///<summary>Creates an object from an encoded DiME item string.</summary>
        ///<param name="encoded">The encoded DiME item string to decode.</param>
        ///<returns>An initialized DiME item object.</returns>
        public static T Import<T>(string encoded) where T: Dime, new()
        {
            string encodedDime = (encoded.StartsWith(Dime.HEADER)) ? encoded.Substring(encoded.IndexOf(Dime._SECTION_DELIMITER) + 1) : encoded;
            T item = new T();
             // TODO: refactor parsing
            string[] sections = encoded.Split(Dime._SECTION_DELIMITER);
            if (sections.Length == 4)
            {
                item._verifiedToken = sections[3];
            }
            item.Populate(encodedDime);
            return item;
        }

        public string Thumbprint()
        {
            return Crypto.GenerateHash(this.Profile, this.Encoded());
        }

        ///<summary>Attaches a verified token to the DiME object. This should only be done after an object has been verified by a 
        /// trusted identity (i.e. central routing service).<summary>
        ///<param name="verifier">The identity that is attaching a verified token.</param>
        ///<param name="privateKey">The private key to use for the verified token.</param>
        /// <exception cref="ArgumentNullException">If passed objects are null.</exception> 
        public void SetVerifiedToken(Identity verifier, string privateKey)
        {
            if (verifier == null) { throw new ArgumentNullException(nameof(verifier), "Verifier identity may not be null."); }
            if (privateKey == null) { throw new ArgumentNullException(nameof(privateKey), "Private key may not be null."); }
            string token = $"{(int)verifier.Profile}{Dime._COMPONENT_DELIMITER}{verifier.SubjectId}{Dime._COMPONENT_DELIMITER}{this.Thumbprint()}";
            this._verifiedToken = Utility.ToBase64($"{token}{Dime._COMPONENT_DELIMITER}{Crypto.GenerateSignature(verifier.Profile, token, privateKey)}");
        }

        public void ValidateVerifiedToken(Identity verifier)
        {
            if (this._verifiedToken != null)
            {
                if (verifier == null) { throw new ArgumentNullException(nameof(verifier), "Verifier identity may not be null."); }
                byte[] tokenBytes = Utility.FromBase64(this._verifiedToken);
                string token = System.Text.Encoding.UTF8.GetString(tokenBytes, 0, tokenBytes.Length);
                string[] components = token.Split(new char[] { Dime._COMPONENT_DELIMITER });
                if (int.Parse(components[0]) != (int)verifier.Profile) { throw new IntegrityException("Verifier profile version mismatch."); }
                if (components[1] != verifier.SubjectId.ToString()) { throw new IntegrityException("Verifier subject id mismatch."); }
                if (components[2] != this.Thumbprint()) { throw new IntegrityException("Thumbprint mismatch."); }
                Crypto.VerifySignature(verifier.Profile, token.Substring(0, token.LastIndexOf(Dime._COMPONENT_DELIMITER)), components[3], verifier.IdentityKey);
            }
        }

        #endregion

        #region -- INTERNAL --

        internal const char _COMPONENT_DELIMITER = '.';
        internal const char _ARRAY_ITEM_DELIMITER = ';';
        internal const char _SECTION_DELIMITER = ':';  

        public string Export()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Dime.HEADER);
            builder.Append(Dime._SECTION_DELIMITER);
            builder.Append(this.Encoded(true));
            if (this._verifiedToken != null)
            {
                builder.Append(Dime._SECTION_DELIMITER);
                builder.Append(this._verifiedToken);
            }
            return builder.ToString();
        }

        internal abstract void Populate(string encoded);

        internal abstract string Encoded(bool includeSignature = false);
        
        #endregion

        #region -- PROTECTED --

        #endregion

        #region -- PRIVATE --

        private static readonly object _lock = new object();
        private static Identity _trustedIdentity;
        private ProfileVersion _profile;

        private string _verifiedToken;

        #endregion

    }

}