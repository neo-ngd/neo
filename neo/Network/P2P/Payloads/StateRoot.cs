using Neo.Cryptography;
using Neo.IO;
using Neo.IO.Json;
using Neo.Persistence;
using Neo.Plugins;
using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.IO;

namespace Neo.Network.P2P.Payloads
{
    public class StateRoot : IInventory
    {
        public uint Version;
        public uint Index;
        public UInt256 PreHash;
        public UInt256 StateRoot_;
        public Witness Witness;

        InventoryType IInventory.InventoryType => InventoryType.StateRoot;

        private UInt256 _hash = null;

        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new UInt256(Crypto.Default.Hash256(this.GetHashData()));
                }
                return _hash;
            }
        }

        Witness[] IVerifiable.Witnesses
        {
            get
            {
                return new[] { Witness };
            }
        }

        public int Size =>
            sizeof(uint) +          //Version
            sizeof(uint) +          //Index 
            PreHash.Size +          //PrevHash
            StateRoot_.Size +       //StateRoot
            1 +                     //Witness array count
            Witness.Size;           //Witness   

        public void Deserialize(BinaryReader reader)
        {
            this.DeserializeUnsigned(reader);
            Witness[] witnesses = reader.ReadSerializableArray<Witness>();
            if (witnesses.Length != 1) return;
            Witness = witnesses[0];
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            Index = reader.ReadUInt32();
            PreHash = reader.ReadSerializable<UInt256>();
            StateRoot_ = reader.ReadSerializable<UInt256>();
        }

        public void Serialize(BinaryWriter writer)
        {
            this.SerializeUnsigned(writer);
            writer.Write(Witness is null ? new Witness[] { } : new Witness[] { Witness });
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Index);
            writer.Write(PreHash);
            writer.Write(StateRoot_);
        }

        public bool Verify(Snapshot snapshot)
        {
            if (!this.VerifyWitnesses(snapshot)) return false;
            return true;
        }

        public UInt160[] GetScriptHashesForVerifying(Snapshot snapshot)
        {
            Console.WriteLine($"corresponding StateRootIndex: {Index}");
            return new UInt160[] { snapshot.GetBlock(Index).NextConsensus };
        }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            Plugin.Log(nameof(StateRoot), level, message);
        }

        public byte[] GetMessage()
        {
            return this.GetHashData();
        }

        public JObject ToJson()
        {
            var json = new JObject();
            json["version"] = Version;
            json["index"] = Index;
            json["prehash"] = PreHash.ToString();
            json["stateroot"] = StateRoot_.ToString();
            json["witness"] = Witness is null ? new JArray() : new JArray(Witness.ToJson());
            return json;
        }
    }
}
