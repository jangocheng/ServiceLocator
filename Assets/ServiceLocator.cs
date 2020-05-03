// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Provides static methods for finding and accessing your services.
/// Any class or interface could be interpreted as a service, including Singletons and MonoBehaviors.
/// When writing tests, you might want to use a separate ServiceContainer instance.
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
        new GarbageCollectionHook();
    }

    /// <summary>
    /// Use the container directly to register services that don't inherit from MonoBehavior.
    /// When writing tests, hot-swapping this container might be handy.
    /// </summary>
    public static ServiceContainer Container = new ServiceContainer();

    /// <summary>
    /// Finds a service by interface or class type.
    /// Any matching MonoBehaviors will be automatically found in the scene and cached (including disabled ones).
    /// Classes and interfaces that don't inherit MonoBehavior need to use the Register method.
    /// </summary>
    /// <typeparam name="T">Any class or interface type.</typeparam>
    public static T Get<T>(bool isOptional = false) where T : class
    {
        var type = typeof(T);

        if (Container.RegisteredServices.TryGetValue(type, out var service))
        {
            // Checking for destroyed Unity objects.
            if (service != null)
                return (T)service;

            // Remove destroyed Unity object.
            Container.Remove(type);
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
            Container.Add((T)service);

        return (T)service;
    }

    /// <summary>
    /// Hooks up to garbage collection event by exploiting a finalizer.
    /// </summary>
    private class GarbageCollectionHook
    {
        ~GarbageCollectionHook()
        {
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
            {
                PurgeDestroyedObjects();
                new GarbageCollectionHook();
            }
        }

        private static readonly List<Type> DestroyedServices = new List<Type>();

        /// <summary>
        /// Removes destroyed Unity objects from cache to let Garbage Collector pick them up.
        /// </summary>
        private static void PurgeDestroyedObjects()
        {
            foreach (var cachedService in Container.RegisteredServices)
                if (cachedService.Value == null)
                    DestroyedServices.Add(cachedService.Key);

            if (DestroyedServices.Count == 0)
                return;

            foreach (var destroyedService in DestroyedServices)
                Container.Remove(destroyedService);

            DestroyedServices.Clear();
        }
    }
}
