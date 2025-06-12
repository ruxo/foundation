using System;
using JetBrains.Annotations;

namespace RZ.Foundation.Injectable;

[PublicAPI]
public interface IUniqueId
{
    Guid NewGuid();
    Guid NewGuid7();
}

[PublicAPI]
public class UniqueId : IUniqueId
{
    #if NET9_0_OR_GREATER
    public Guid NewGuid()  => Guid.NewGuid();
    public Guid NewGuid7() =>  Guid.CreateVersion7();
#else
    public Guid NewGuid() => Guid.NewGuid();
    public Guid NewGuid7() { throw new NotImplementedException(); }
    #endif
}