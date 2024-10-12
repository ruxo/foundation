using MongoDB.Driver;

namespace RZ.Foundation.MongoDb;

public interface IHaveKey<out T>
{
    T? Id { get; }
}

public interface IHaveVersion {
    DateTimeOffset LastUpdate { get; }
    ulong Version { get; }
}

public interface IPersistentModel<out T,TKey> : IHaveKey<TKey>, IHaveVersion
{
    static abstract T WithKey(TKey id);
    static abstract T WithVersion(DateTimeOffset lastUpdate);
}