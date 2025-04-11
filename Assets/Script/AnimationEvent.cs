using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public Action<string> aniEventCb; // Animation event callback
    public void OnAnimationEvent(string eventName)
    {
        if (aniEventCb != null)
        {
            aniEventCb(eventName); // Invoke the callback with the event name
        }
    }
}
