using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace RZ.Foundation.Testing;

[PublicAPI]
public static class MockExtensions
{
    public static ServiceCollection BuildFor<T>(this ServiceCollection services) where T : class =>
        services.BuildFor(typeof(T));

    public static ServiceCollection BuildFor(this ServiceCollection services, Type type) {
        var subscriptions = services.ToSeq().Map(i => i.ServiceType).ToHashSet();
        var ctorParams = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single().GetParameters();
        var missingParams = from p in ctorParams
                            where !typeof(ILogger).IsAssignableFrom(p.ParameterType) && !subscriptions.Contains(p.ParameterType)
                            select p;

        foreach (var p in missingParams){
            Debug.WriteLine($"Missing {p.ParameterType}");
            if (p.ParameterType.IsAbstract)
                services.AddSingleton(p.ParameterType, CreateMockByType(p.ParameterType).Object);
            else{
                services.AddTransient(p.ParameterType);
                services.BuildFor(p.ParameterType);
            }
        }
        return services;
    }

    public static ServiceCollection UseLogger(this ServiceCollection services, ITestOutputHelper output) {
        services.AddSingleton(output);
        services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
        return services;
    }

    public static T Create<T>(this IServiceProvider sp, params object[] parameters) =>
        ActivatorUtilities.CreateInstance<T>(sp, parameters);

    public static T BuildAndCreate<T>(this ServiceCollection services) where T : class =>
        services.BuildFor<T>().BuildServiceProvider().Create<T>();

    public static Mock CreateMockByType(Type type) {
        var ctor = typeof(Mock<>).MakeGenericType(type).GetConstructor([])!;
        return (Mock) ctor.Invoke(null);
    }
}