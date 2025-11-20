using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorIndex;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;

    public bool Equals(PlayerData other)
    {
        return this.clientId == other.clientId 
            && this.colorIndex == other.colorIndex
            && this.playerName == other.playerName
            && this.playerId == other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorIndex);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
    }
}
