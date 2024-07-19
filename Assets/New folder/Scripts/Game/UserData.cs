using Gameplay;
using System;
using Unity.Netcode;
using Utils;

namespace Connection
{
    public struct UserData : INetworkSerializable
    {
        public ulong clientId;
        public Belonging side;
        public bool isDead;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out clientId);
                reader.ReadValueSafe(out side);
                reader.ReadValueSafe(out isDead);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(clientId);
                writer.WriteValueSafe(side);
                writer.WriteValueSafe(isDead);
            }
        }
    }

    [Serializable]
    public struct ApprovalData : ICustomSerialiser<ApprovalData>
    {
        public int prefabId;

        public ApprovalData Deserialize(byte[] bytes)
        {
            return (ApprovalData)Converter.ByteArrayToObject(bytes);
        }

        public byte[] Serialize()
        {
            return Converter.ObjectToByteArray(this);
        }
    }
}
