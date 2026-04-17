using System;
using System.Collections.Generic;
using UnityEngine;

public static class Publisher
{
    private static readonly Dictionary<Type, HashSet<ISubscriber>> _allSubscribers = new();

    public static void Subscribe(ISubscriber subscriber, Type messageType)
    {
        if (_allSubscribers.ContainsKey(messageType))
        {
            bool isAlreadySubscribed = !_allSubscribers[messageType].Add(subscriber);

            if (isAlreadySubscribed)
                Debug.LogWarning($"Subscriber {subscriber} is already subscribed to message type {messageType}");
        }
        else
        {
            HashSet<ISubscriber> subscriberList = new() { subscriber };
            _allSubscribers.Add(messageType, subscriberList);
        }
    }

    public static void Publish(IPublisherMessage message)
    {
        var messageType = message.GetType();

        if (_allSubscribers.ContainsKey(messageType))
        {
            var subscribers = new List<ISubscriber>(_allSubscribers[messageType]);

            foreach (var subscriber in subscribers)
            {
                subscriber.OnPublish(message);
            }
        }
    }

    public static void Unsubscribe(ISubscriber subscriber, Type messageType)
    {
        if (_allSubscribers.ContainsKey(messageType))
        {
            _allSubscribers[messageType].Remove(subscriber);
        }
    }

    public static void ClearAllSubscribers()
    {
        _allSubscribers.Clear();
    }
}
