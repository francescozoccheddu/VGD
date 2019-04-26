using System;
using UnityEngine;
using UnityEngine.Events;

namespace Wheeled.Menu
{
    public abstract class ValidatorBehaviour : MonoBehaviour
    {
        #region Public Classes

        [Serializable]
        public sealed class ValidatedEvent : UnityEvent<bool> { }

        #endregion Public Classes

        #region Public Fields

        public ValidatedEvent validated = new ValidatedEvent();

        #endregion Public Fields
    }
}