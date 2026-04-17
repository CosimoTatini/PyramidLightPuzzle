using System;
using System.Collections.Generic;

public class StandaloneSubscriber<TMessage> : ISubscriber where TMessage : IPublisherMessage
{
    private readonly Action<TMessage> _callback;

    public StandaloneSubscriber(Action<TMessage> callback)
    {
        _callback = callback;
        Publisher.Subscribe(this, typeof(TMessage));
    }

    public void OnPublish(IPublisherMessage message)
    {
        if (message is TMessage typedMessage)
        {
            _callback?.Invoke(typedMessage);
            Publisher.Unsubscribe(this, typeof(TMessage));
        }
    }
}
