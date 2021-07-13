//
//  KeyBox.cs
//  DiME - Digital Identity Message Envelope
//  A secure and compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShiftEverywhere.DiME
{
    public class KeyBox: DimeItem
    {
        #region -- PUBLIC --

        public const string IID = "aW8uZGltZWZvcm1hdC5reWI"; // base64 of io.dimeformat.kyb
        public override string ItemIdentifier { get { return KeyBox.IID; } }
        public ProfileVersion Profile { get; private set; }
         public Guid? IssuerId { get { return this._claims.iss; } }
        /// <summary></summary>
        public override Guid UID { get { return this._claims.kid; } }
         public long? IssuedAt { get { return this._claims.iat; } }
        /// <summary></summary>
        public KeyType Type { get; private set; }
        /// <summary></summary>
        public string Key { get { return this._claims.key; } }
        /// <summary></summary>
        public string PublicKey { get { return this._claims.pub; } }

        public KeyBox() { }

        /// <summary></summary>
        public static KeyBox Generate(KeyType type, ProfileVersion profile = Crypto.DEFUALT_PROFILE)
        {
            return Crypto.GenerateKeyPair(profile, type);
        }

        public static KeyBox FromBase58Key(string encodedKey)
        {
            KeyBox keybox = new KeyBox();
            keybox.DecodeKey(encodedKey);
            return keybox;
        }

        public KeyBox PublicOnly()
        {
            return new KeyBox(this.UID, this.Type, null, this.RawPublicKey, this.Profile);
        }

        public new static KeyBox FromString(string encoded)
        {
            KeyBox keybox = new KeyBox();
            keybox.Decode(encoded);
            return keybox;
        }

        #endregion

        #region -- INTERNAL --

        internal byte[] RawKey { get; private set; }
        internal byte[] RawPublicKey { get; private set; }

        internal KeyBox(Guid id, KeyType type, byte[] key, byte[] publickey, ProfileVersion profile = Crypto.DEFUALT_PROFILE)
        {
            if (!Crypto.SupportedProfile(profile)) { throw new UnsupportedProfileException(); }
            long iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this._claims = new _KeyBoxClaims(
                null, 
                id, 
                iat, 
                EncodeKey(key, (byte)type, (byte)KeyVariant.Private, (byte)profile), 
                EncodeKey(publickey, (byte)type, (byte)KeyVariant.Public, (byte)profile));
            this.Type = type;
            this.Profile = profile;
            this.RawKey = key;
            this.RawPublicKey = publickey;
        }

        #endregion

        # region -- PROTECTED --

        protected override void Decode(string encoded)
        {
            string[] components = encoded.Split(new char[] { Dime._COMPONENT_DELIMITER });
            if (components.Length != KeyBox._NBR_EXPECTED_COMPONENTS) { throw new DataFormatException($"Unexpected number of components for identity issuing request, expected {KeyBox._NBR_EXPECTED_COMPONENTS}, got {components.Length}."); }
            if (components[KeyBox._IDENTIFIER_INDEX] != KeyBox.IID) { throw new DataFormatException($"Unexpected object identifier, expected: \"{KeyBox.IID}\", got \"{components[KeyBox._IDENTIFIER_INDEX]}\"."); }
            byte[] json = Utility.FromBase64(components[KeyBox._CLAIMS_INDEX]);
            this._claims = JsonSerializer.Deserialize<_KeyBoxClaims>(json);
            DecodeKey(this._claims.key);
            DecodeKey(this._claims.pub);
            this._encoded = encoded;
        }

        protected override string Encode()
        {
            if (this._encoded == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(KeyBox.IID);
                builder.Append(Dime._COMPONENT_DELIMITER);
                if (!this._claims.includeKey && this.Type != KeyType.Secret)
                {
                    builder.Append(Utility.ToBase64(JsonSerializer.Serialize(new _KeyBoxClaims(Guid.NewGuid(), this._claims.kid, this._claims.iat, null, this._claims.pub))));
                }
                else
                {
                    builder.Append(Utility.ToBase64(JsonSerializer.Serialize(this._claims)));
                }
                this._encoded = builder.ToString();
            }
            return this._encoded;
        }

        #endregion

        #region -- PRIVATE --

        private const int _NBR_EXPECTED_COMPONENTS = 2;
        private const int _IDENTIFIER_INDEX = 0;
        private const int _CLAIMS_INDEX = 1;
        private _KeyBoxClaims _claims;

        private class _KeyBoxClaims
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Guid? iss { get; set; }
            public Guid kid { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public long? iat { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string key { get; set; }
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string pub { get; set; }

            [JsonIgnore]
            public bool includeKey;

            [JsonConstructor]
            public _KeyBoxClaims(Guid? iss, Guid kid, long? iat, string key, string pub)
            {
                this.iss = iss;
                this.kid = kid;
                this.iat = iat;
                this.key = key;
                this.pub = pub;
                this.includeKey = true;
            }

            public bool ShouldSerializeKey()
            {
                return this.includeKey;
            }
        }
        
        private string EncodeKey(byte[] key, byte type, byte variant, byte profile)
        {
            if (key == null) return null;
            byte combinedType = (byte)((uint)type | (uint)variant);
            byte[] prefix = { 0x04, profile, combinedType, 0x00 };
            return Base58.Encode(Utility.Combine(prefix, key));
        }

        private void DecodeKey(string encodedKey)
        {
            if (encodedKey != null)
            {
                byte[] bytes = Base58.Decode(encodedKey);
                ProfileVersion profile = (ProfileVersion)bytes[1];
                if (this.Profile != ProfileVersion.Undefined && profile != this.Profile) { throw new DataFormatException("Cryptographic profile version mismatch."); }
                this.Profile = profile;
                KeyType type = (KeyType)((byte)((uint)bytes[2] & 0xFE));
                if (this.Type != KeyType.Undefined && type != this.Type) { throw new DataFormatException("Key type mismatch."); }
                this.Type = type;
                KeyVariant variant = (KeyVariant)((byte)((uint)bytes[2] & 0x01));
                switch (variant)
                {
                    case KeyVariant.Public:
                        this.RawPublicKey = Utility.SubArray(bytes, 4);
                        break;
                    case KeyVariant.Private:
                        this.RawKey = Utility.SubArray(bytes, 4);
                        break;
                }
            }
        }

        #endregion
    }

}