global using VersionType = uint;
using JetBrains.Annotations;

namespace RZ.Foundation.MongoDb;

[PublicAPI]
public interface IHaveKey<T>
{
    T Id { get; set; }
}

[PublicAPI]
public interface IHaveVersion {
    DateTimeOffset Updated { get; }
    VersionType Version { get; }
}

[PublicAPI]
public interface ICanUpdateVersion<out T> : IHaveVersion
{
    T WithVersion(DateTimeOffset updated, uint next);
}