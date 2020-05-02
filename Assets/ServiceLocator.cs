// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TODO ADD DESCRIPTION
/// </summary>
/// <remarks>
/// Unity-specific implementation of a pattern that simplifies IoC and Testing.
/// Made with ease of use and singleton compatibility in mind for simple dependency graphs.
/// Underdog alternative to Dependency Injection frameworks, like David vs Goliath.
/// </remarks>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> CachedServices = new Dictionary<Type, object>();

    /// <summary>
    /// Resolves a registered service by interface or class type.
    /// Any matching MonoBehaviors will be automatically found in the scene and cached (including disabled ones).
    /// Classes and interfaces that don't inherit MonoBehavior need to use the Register method.
    /// </summary>
    /// <typeparam name="T">Any class or interface type.</typeparam>
    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (CachedServices.TryGetValue(type, out var service))
        {
            // Checking for destroyed objects.
            if (service != null)
                return (T)service;

            CachedServices.Remove(type);
        }

        // Attempt to find the service then resort to lazy instantiation.
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
            CachedServices.Add(type, service);

        return (T)service;
    }
}
