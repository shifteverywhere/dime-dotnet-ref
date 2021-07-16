//
//  Envelope.cs
//  DiME - Digital Identity Message Envelope
//  Compact messaging format for assertion and practical use of digital identities
//
//  Released under the MIT licence, see LICENSE for more information.
//  Copyright © 2021 Shift Everywhere AB. All rights reserved.
//
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShiftEverywhere.DiME
{
    public class Envelope
    {
        public const string HEADER = "Di";
        public Guid? IssuerId { get { return (this._claims.HasValue) ? this._claims.Value.iss : null; } }
        public long? IssuedAt { get { return (this._claims.HasValue) ? this._claims.Value.iat : null; } } 
        public IList<Item> Items { get { return (this._items != null) ? this._items.AsReadOnly() : null; } }
        public bool IsSigned { get { return (this._signature != null); } }
        public bool IsAnonymous { get { return !this._claims.HasValue; } }

        public Envelope() { }

        public Envelope(Guid issuerId)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this._claims = new DimeClaims(issuerId, now);
        }

        public static Envelope Import(string exported)
        {
            if (!exported.StartsWith(Envelope.HEADER)) { throw new FormatException("Not a Dime envelope object, invalid header."); }
            string[] sections = exported.Split(Envelope._SECTION_DELIMITER);
            // 0: HEADER
            string[] components = sections[0].Split(Envelope._COMPONENT_DELIMITER);
            Envelope dime;
            if (components.Length == 2)
            {
                DimeClaims claims = JsonSerializer.Deserialize<DimeClaims>(Utility.FromBase64(components[1]));
                dime = new Envelope(claims);
            }
            else if (components.Length == 1) 
                dime = new Envelope();
            else 
                throw new FormatException($"Not a valid Dime envelope object, unexpected number of components in header, got: '{components.Length}', expexted: '1' or '2'");
            // 1 to LAST or LAST - 1 
            int endIndex = (dime.IsAnonymous) ? sections.Length : sections.Length - 1; // end index dependent on anonymous Dime or not
            List<Item> items = new List<Item>(endIndex - 1);
            for (int index = 1; index < endIndex; index++)
                items.Add(Item.FromEncoded(sections[index]));
            dime._items = items;
            dime._encoded = exported.Substring(0, exported.LastIndexOf(Envelope._SECTION_DELIMITER));
            if (!dime.IsAnonymous)
                dime._signature = sections.Last(); 
            return dime;
        }

        public Envelope AddItem(Item item)
        {
            if (this._signature != null) { throw new InvalidOperationException("Unable to add item, envelope is already signed."); }
            if (this._items == null)
                this._items = new List<Item>();
            this._items.Add(item);
            return this;
        }

        public Envelope SetItems(List<Item> items)
        {
            if (this._signature != null) { throw new InvalidOperationException("Uanle to set items, envelope is already signed."); }
            this._items = items.ToList();
            return this;
        }

        public Envelope Sign(KeyBox keybox)
        {
            if (this.IsAnonymous) { throw new InvalidOperationException("Unable to sign, envelope is anonymous."); }
            if (this._signature != null) { throw new InvalidOperationException("Unable to sign, envelope is already signed."); }
            if (this._items == null || this._items.Count == 0) { throw new InvalidOperationException("Unable to sign, at least one item must be attached before signing an envelope."); }
            this._signature = Crypto.GenerateSignature(Encode(), keybox);
            return this;
        }

        public void Verify(string publicKey)
        {
            Verify(new KeyBox(publicKey));
        }
        
        public Envelope Verify(KeyBox keybox)
        {
            if (this.IsAnonymous) { throw new InvalidOperationException("Unable to verify, envelope is anonymous."); }
            if (this._signature == null) { throw new InvalidOperationException("Unable to verify, envelope is not signed."); }
            Crypto.VerifySignature(Encode(), this._signature, keybox);
            return this;
        }

        public string Export()
        {
            if (!this.IsAnonymous)
            {
                if (this._signature == null) { throw new InvalidOperationException("Unable to export, envelope is not signed."); }
                return $"{Encode()}{Envelope._SECTION_DELIMITER}{this._signature}";
            }
            else
                return Encode();
        }

        public string Thumbprint()
        {
            return Utility.ToHex(Crypto.GenerateHash(Profile.Uno, this.Encode()));
        }

        internal const char _COMPONENT_DELIMITER = '.';
        internal const char _SECTION_DELIMITER = ':';

        #region -- PRIVATE --

         private List<Item> _items;
        private string _encoded;
        private string _signature;
        private DimeClaims? _claims;

        private struct DimeClaims
        {
            public Guid iss { get; set; }
            public long iat { get; set; }

            [JsonConstructor]
            public DimeClaims(Guid iss, long iat)
            {
                this.iss = iss;
                this.iat = iat;
            }

        }

        private Envelope(DimeClaims claims)
        {
            this._claims = claims;
        }

        private string Encode()
        {
            if (this._encoded == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Envelope.HEADER);
                if (!this.IsAnonymous)
                {
                    builder.Append(Envelope._COMPONENT_DELIMITER);
                    builder.Append(Utility.ToBase64(JsonSerializer.Serialize(this._claims)));
                }
                foreach(Item item in this._items)
                {
                    builder.Append(Envelope._SECTION_DELIMITER);
                    builder.Append(item.ToEncoded());
                }
                this._encoded = builder.ToString();
            }
            return this._encoded;
        }

        #endregion

    }

}

