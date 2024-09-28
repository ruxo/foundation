using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;

namespace RZ.Foundation.Blazor.MVVM;
public abstract record ViewResolution
{
    public sealed record Empty : ViewResolution
    {
        public static readonly ViewResolution Instance = new Empty();
    }

    public sealed record InvalidModel : ViewResolution
    {
        public static readonly ViewResolution Instance = new InvalidModel();
    }

    public sealed record ViewTypeNotFound(string ViewTypeName) : ViewResolution;

    public sealed record View(RenderFragment Content) : ViewResolution;

    internal sealed record ResolvedViewType(Type ViewType) : ViewResolution;
}

public interface IViewFinder
{
    ViewResolution Locate(ViewModel vm);
}

public class ViewFinder : IViewFinder
{
    readonly ConcurrentDictionary<Type, ViewResolution> viewLookup = new();

    public ViewResolution Locate(ViewModel vm) {
        var modelType = vm.GetType();
        var state = viewLookup.GetOrAdd(modelType, GetViewState);

        return state is ViewResolution.ResolvedViewType s? new ViewResolution.View(ForType(s.ViewType)) : state;

        RenderFragment ForType(Type vType) => builder => {
            builder.OpenComponent(0, vType);
            builder.AddAttribute(1, "ViewModel", vm);
            builder.CloseComponent();
        };
    }

    static ViewResolution GetViewState(Type modelType) {
        if (!modelType.Name.EndsWith("ViewModel"))
            return ViewResolution.InvalidModel.Instance;

        var typeName = modelType.Name[..^9]; // 9 = "ViewModel".Length
        var viewType = GetType(modelType, typeName) ?? GetType(modelType, typeName + "View");

        return viewType is null
                   ? new ViewResolution.ViewTypeNotFound(modelType.Namespace!)
                   : new ViewResolution.ResolvedViewType(viewType);
    }

    static Type? GetType(Type modelType, string typeName) {
        // get view type from the same namespace of the model
        var viewTypeFullName = $"{modelType.Namespace}.{typeName}";
        return modelType.Assembly.GetType(viewTypeFullName);
    }
}
