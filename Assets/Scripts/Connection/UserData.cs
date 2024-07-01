using Gameplay;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Connection
{
    public struct UserData : INetworkSerializable
    {
        public ulong clientId;
        public Belonging side;
        public FixedString32Bytes playerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out clientId);
                reader.ReadValueSafe(out side);
                reader.ReadValueSafe(out playerName);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(clientId);
                writer.WriteValueSafe(side);
                writer.WriteValueSafe(playerName);
            }
        }
    }

    public struct ApprovalData : Utils.ICustomSerialiser<ApprovalData>
    {
        public int prefabId;
        public uint roomPassword;
        public string playerName;

        public ApprovalData Deserialize(byte[] bytes)
        {
            prefabId = Utils.Converter.FromByteArrayToInt(bytes[0..4]);
            roomPassword = (uint)Utils.Converter.FromByteArrayToInt(bytes[4..8]);
            playerName = Utils.Converter.FromByteArrayToString(bytes[8..]);
            return this;
        }

        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Utils.Converter.FromIntToByteArray(prefabId));
            bytes.AddRange(Utils.Converter.FromIntToByteArray((int)roomPassword));
            bytes.AddRange(Utils.Converter.FromStringToByteArray(playerName));
            return bytes.ToArray();
        }
    }
}
