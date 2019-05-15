using System;
using UnityEngine;
using UnityEngine.Events;

namespace Wheeled.Menu
{
    public abstract class ValidatorBehaviour : MonoBehaviour
    {
        [Serializable]
        public sealed class ValidatedEvent : UnityEvent<bool> { }

        public ValidatedEvent validated = new ValidatedEvent();
    }
}