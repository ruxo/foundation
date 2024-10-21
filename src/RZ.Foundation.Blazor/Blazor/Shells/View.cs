using JetBrains.Annotations;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public static class View
{
    public static ViewMaker Model<T>() where T : ViewModel
        => factory => factory.Create<T>();
}