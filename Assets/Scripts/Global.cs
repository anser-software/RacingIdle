using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Baracuda.Monitoring;
public static class Global
{

    private static readonly Dictionary<Type, object>
        Services = new Dictionary<Type, object>();

    public static void Register<T>(object serviceInstance)
    {
        Services[typeof(T)] = serviceInstance;
    }

    public static T Get<T>()
    {
        return (T)Services[typeof(T)];
    }

    public static void Reset()
    {
        Services.Clear();
    }

}
