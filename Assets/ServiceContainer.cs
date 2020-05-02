﻿// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using System.Collections.Generic;

/// <summary>
/// Basic implementation inspired by built-in System.ComponentModel.Design.ServiceContainer class.
/// https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.design.servicecontainer
/// </summary>
public class ServiceContainer
{
    private static ServiceContainer _staticInstance;
    public static ServiceContainer Static => _staticInstance ??= new ServiceContainer();

    private readonly Dictionary<Type, object> _registeredServices = new Dictionary<Type, object>();

    /// <summary>
    /// Manually registers any class or interface as a service, including ones that don't inherit from MonoBehaviour.
    /// Replaces the old service if the type it already registered.
    /// </summary>
    public void Add<T>(T service) where T : class
    {
        var type = typeof(T);

        if (_registeredServices.ContainsKey(type))
            _registeredServices[type] = service;
        else
            _registeredServices.Add(type, service);
    }

    public T Get<T>(bool isOptional = false) where T : class
    {
        var type = typeof(T);

        if (_registeredServices.TryGetValue(type, out var service))
        {
            // Checking for destroyed Unity objects.
            if (service != null)
                return (T)service;

            _registeredServices.Remove(type);
        }

        if (isOptional)
            return null;

        throw new InvalidOperationException($"{type.Name} service not found in the container.");
    }

    public void Remove<T>(T service) where T : class
    {
        var type = typeof(T);

        if (_registeredServices.ContainsKey(type))
            _registeredServices[type] = service;
        else
            _registeredServices.Add(type, service);
    }
}
