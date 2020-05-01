// Banana Party 2020.
// https://assetstore.unity.com/publishers/3621
// https://forcepusher.tumblr.com/
// Do whatever you want. No warranties though.

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unity implementation of a pattern that simplifies IoC and Testing.
/// Made with ease of use and singleton compatibility in mind for simple dependency graphs.
/// Underdog alternative to Dependency Injection frameworks, like David vs Goliath.
/// </summary>
public static class ServiceLocator
{
    private const string LazyServiceGameObjectNamePostfix = "Service";

    /// <summary>
    /// Use the provided methods instead of modifying this dictionary directly, but sometimes you have to do it by hand.
    /// </summary>
    public static readonly Dictionary<Type, object> RegisteredServices = new Dictionary<Type, object>();

    /// <summary>
    /// Resolves a registered service by interface or class type.
    /// Any matching MonoBehaviors will be automatically found in the scene and registered (including disabled ones).
    /// Classes and interfaces that don't inherit MonoBehavior need to use the Register method.
    /// </summary>
    /// <typeparam name="T">Any class or interface type.</typeparam>
    /// <param name="lazyInitialization">Creates the specified service (and spawns a GameObject in case of a MonoBehavior) whenever not found.
    /// Causes exceptions when used with interfaces or classes without a parameter-less constructor.</param>
    public static T Get<T>(bool lazyInitialization = false) where T : class
    {
        var type = typeof(T);

        if (RegisteredServices.TryGetValue(type, out var service))
        {
            // Checking for destroyed objects.
            if (service != null)
                return (T)service;

            RegisteredServices.Remove(type);
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

                if (service == null && lazyInitialization)
                    throw new ArgumentException($"Service {type.Name} can't use lazy initialization because it's an interface.");
            }
            else
            {
                service = UnityEngine.Object.FindObjectOfType(type);
                if (service == null && lazyInitialization)
                    service = new GameObject(type.Name + LazyServiceGameObjectNamePostfix, type).GetComponent<T>();
            }
        }
        else
        {
            // This might throw an unhandled exception, and for a good reason. Lazy initialization is a technical debt.
            if (lazyInitialization)
                service = Activator.CreateInstance(type, true);
        }

        if (service != null)
            RegisteredServices.Add(type, service);

        return (T)service;
    }

    /// <summary>
    /// Manually registers any class or interface as a service, including ones that don't inherit from MonoBehaviour.
    /// Replaces the old service if the type it already registered.
    /// </summary>
    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);

        if (RegisteredServices.ContainsKey(type))
            RegisteredServices[type] = service;
        else
            RegisteredServices.Add(type, service);
    }

    /// <summary>
    /// Destroys MonoBehavior services and disposes any IDisposable services.
    /// </summary>
    /// <param name="service">Helps to automatically resolve the generic type instead of specifying it by hand.</param>
    public static void Destroy<T>(T service = null) where T : class
    {
        object serviceObject = service;
        if (serviceObject == null)
            if (!RegisteredServices.TryGetValue(typeof(T), out serviceObject))
                return;

        if (serviceObject is IDisposable disposableService)
            disposableService.Dispose();

        if (serviceObject is MonoBehaviour monoBehaviourService)
        {
            // Destroy lazy service GameObjects, but don't touch the custom ones.
            if (monoBehaviourService.gameObject.name.EndsWith(LazyServiceGameObjectNamePostfix))
                UnityEngine.Object.Destroy(monoBehaviourService.gameObject);
            else
                UnityEngine.Object.Destroy(monoBehaviourService);
        }

        RegisteredServices.Remove(typeof(T));
    }

    /// <summary>
    /// Destroys whatever was registered as a service. Useful when tearing down tests.
    /// </summary>
    public static void DestroyAll()
    {
        var servicesToDestroy = new List<Type>(RegisteredServices.Keys);
        foreach (var service in servicesToDestroy)
            Destroy(service);

        servicesToDestroy.Clear();
    }
}
