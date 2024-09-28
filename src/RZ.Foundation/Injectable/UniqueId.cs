using System;
using JetBrains.Annotations;

namespace RZ.Foundation.Injectable;

public interface IUniqueId
{
    Guid NewGuid();
}

[PublicAPI]
public class UniqueId : IUniqueId
{
    public Guid NewGuid() => Guid.NewGuid();
}