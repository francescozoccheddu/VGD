using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public sealed class HealthDisplayBehaviour : MonoBehaviour
    {
        #region Internal Properties

        internal int Health { get; set; }

        #endregion Internal Properties

        #region Public Fields

        public Text text;

        #endregion Public Fields

        #region Private Methods

        private void Update()
        {
            string newText = Health.ToString();
            if (text.text != newText)
            {
                text.text = newText;
            }
        }

        #endregion Private Methods
    }
}