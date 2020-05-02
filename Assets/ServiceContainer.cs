// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using System.Collections.Generic;

public class ServiceContainer
{
    private static ServiceContainer _staticInstance;
    public static ServiceContainer Static => _staticInstance ??= new ServiceContainer();

    private readonly Dictionary<Type, object> RegisteredServices = new Dictionary<Type, object>();

    /// <summary>
    /// Manually registers any class or interface as a service, including ones that don't inherit from MonoBehaviour.
    /// Replaces the old service if the type it already registered.
    /// </summary>
    public void Add<T>(T service) where T : class
    {
        var type = typeof(T);

        if (RegisteredServices.ContainsKey(type))
            RegisteredServices[type] = service;
        else
            RegisteredServices.Add(type, service);
    }

    public void Remove<T>(T service) where T : class
    {
        var type = typeof(T);

        if (RegisteredServices.ContainsKey(type))
            RegisteredServices[type] = service;
        else
            RegisteredServices.Add(type, service);
    }
}
