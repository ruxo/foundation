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
    public Guid NewGuid() => Guid.NewGuid();
}