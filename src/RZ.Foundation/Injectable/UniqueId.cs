using System;
using JetBrains.Annotations;

namespace RZ.Foundation.Injectable;

[PublicAPI]
public interface IUniqueId
{
    Guid NewGuid();
}

[PublicAPI]
public class UniqueId : IUniqueId
{
    #if NET9_0_OR_GREATER
    public Guid NewGuid() => Guid.CreateVersion7();
    #else
    public Guid NewGuid() => Guid.NewGuid();
    #endif
}