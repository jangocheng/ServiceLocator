// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TODO ADD DESCRIPTION
/// When writing tests, you might want to use ServiceContainer directly.
/// </summary>
/// <remarks>
/// Unity-specific implementation of a pattern that simplifies IoC and Testing.
/// Made with singleton compatibility and minimal integration effort in mind.
/// Underdog alternative to Dependency Injection frameworks, like David vs Goliath.
/// </remarks>
public static class ServiceLocator
{
    static ServiceLocator()
    {
        new GarbageCollectionDetector();
    }

    private static readonly ServiceContainer ServiceContainer = new ServiceContainer();

    /// <summary>
    /// Resolves a registered service by interface or class type.
    /// Any matching MonoBehaviors will be automatically found in the scene and cached (including disabled ones).
    /// Classes and interfaces that don't inherit MonoBehavior need to use the Register method.
    /// </summary>
    /// <typeparam name="T">Any class or interface type.</typeparam>
    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (ServiceContainer.RegisteredServices.TryGetValue(type, out var service))
        {
            // Checking for destroyed Unity objects.
            if (service != null)
                return (T)service;

            ServiceContainer.Remove(type);
        }

        if (type.IsSubclassOf(typeof(MonoBehaviour)))
        {
            if (type.IsInterface)
            {
                var monoBehaviors = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var monoBehaviour in monoBehaviors)
                {
                    if (monoBehaviour is T serviceInterface)
                    {
                        service = serviceInterface;
                        break;
                    }
                }
            }
            else
            {
                service = UnityEngine.Object.FindObjectOfType(type);
            }
        }

        if (service != null)
            ServiceContainer.Add((T)service);

        return (T)service;
    }

    /// <summary>
    /// Hooks up to garbage collection event by exploiting a finalizer.
    /// </summary>
    private class GarbageCollectionDetector
    {
        ~GarbageCollectionDetector()
        {
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
            {
                PurgeDestroyedObjectsFromCache();
                new GarbageCollectionDetector();
            }
        }
    }

    private static readonly List<Type> DestroyedServices = new List<Type>();
    /// <summary>
    /// Removes destroyed Unity objects from cache to let Garbage Collector pick them up.
    /// </summary>
    private static void PurgeDestroyedObjectsFromCache()
    {
        foreach (var cachedService in ServiceContainer.RegisteredServices)
            if (cachedService.Value == null)
                DestroyedServices.Add(cachedService.Key);

        if (DestroyedServices.Count == 0)
            return;

        foreach (var destroyedService in DestroyedServices)
            ServiceContainer.Remove(destroyedService);

        DestroyedServices.Clear();
    }
}
