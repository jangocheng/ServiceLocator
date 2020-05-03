﻿// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using System.Collections.Generic;

/// <summary>
/// Provides a way to store references to your services for convenient access within components.
/// Any class or interface can be a service, including Singletons and MonoBehaviors.
/// ServiceContainers could contain one another to create a context hierarchy.
/// </summary>
/// <remarks>
/// Inspired by built-in System.ComponentModel.Design.ServiceContainer class.
/// https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.design.servicecontainer
/// </remarks>
public class ServiceContainer
{
    /// <summary>
    /// Use the provided methods instead of modifying it directly, whenever possible.
    /// </summary>
    public readonly Dictionary<Type, object> RegisteredServices = new Dictionary<Type, object>();

    /// <summary>
    /// Registers a service in the container, or replaces it if matching type is already registered.
    /// When specifying a service by interface, make sure not to omit the type.
    /// </summary>
    /// <typeparam name="T">Any class or interface service type, including ones that don't inherit from MonoBehaviour.</typeparam>
    /// <param name="service">Interface or class instance of the service. Could be a MonoBehavior singleton.</param>
    public void Add<T>(T service) where T : class
    {
        var type = typeof(T);

        if (RegisteredServices.ContainsKey(type))
            RegisteredServices[type] = service;
        else
            RegisteredServices.Add(type, service);
    }

    /// <summary>
    /// Looks up the registered service, or throws an exception if not found in the container.
    /// </summary>
    /// <typeparam name="T">Any class on interface service type, including ones that don't inherit from MonoBehaviour.</typeparam>
    /// <param name="isOptional">Missing optional services won't cause exceptions.</param>
    /// <returns>Service previously registered with Add method, or null if the service is optional.</returns>
    public T Get<T>(bool isOptional = false) where T : class
    {
        if (RegisteredServices.TryGetValue(typeof(T), out var service))
        {
            // Checking for destroyed Unity objects.
            if (service != null)
                return (T)service;

            if (!isOptional)
                throw new InvalidOperationException($"{nameof(T)} service was destroyed.");
        }

        if (!isOptional)
            throw new InvalidOperationException($"{nameof(T)} service not found in the container.");

        return null;
    }

    /// <summary>
    /// Removes the service from container.
    /// </summary>
    /// <typeparam name="T">Service type previously registered with Add method.</typeparam>
    public void Remove<T>() where T : class
    {
        RegisteredServices.Remove(typeof(T));
    }

    /// <summary>
    /// Removes the service from container.
    /// </summary>
    /// <param name="serviceType">Service type previously registered with Add method.</param>
    public void Remove(Type serviceType)
    {
        RegisteredServices.Remove(serviceType);
    }
}
