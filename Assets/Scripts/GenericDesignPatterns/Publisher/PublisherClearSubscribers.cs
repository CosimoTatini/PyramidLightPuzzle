using UnityEngine;

public class PublisherClearSubscribers : MonoBehaviour
{
    public void ClearAllSubscribers()
    {
        // This method is intended to clear all subscribers from the publisher.
        // It should be called when you want to reset the state of the publisher.
        Publisher.ClearAllSubscribers();
    }
}
