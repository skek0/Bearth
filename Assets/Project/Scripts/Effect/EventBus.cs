using System;
using System.Collections.Generic;

public static class EventBus
{
    static readonly Dictionary<Type, Delegate> events = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        if (events.TryGetValue(typeof(T), out var d)) events[typeof(T)] = Delegate.Combine(d, handler);
        else events[typeof(T)] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (!events.TryGetValue(typeof(T), out var d)) return;
        var nd = Delegate.Remove(d, handler);
        if (nd is null) events.Remove(typeof(T)); else events[typeof(T)] = nd;
    }

    public static void Publish<T>(T _event)
    {
        if (events.TryGetValue(typeof(T), out var d))
            (d as Action<T>)?.Invoke(_event);
    }
}
