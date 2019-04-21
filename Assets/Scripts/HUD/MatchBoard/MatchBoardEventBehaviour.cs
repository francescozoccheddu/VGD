using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.HUD
{
    public abstract class MatchBoardEventBehaviour : MonoBehaviour
    {
        #region Public Fields

        public Text text;

        #endregion Public Fields

        #region Public Methods

        public void Destroy()
        {
            Destroy(gameObject);
        }

        #endregion Public Methods

        #region Protected Methods

        protected abstract string GetText();

        protected string GetName(string _name)
        {
            if (_name != null)
            {
                return string.Format("<b>{0}</b>", _name);
            }
            else
            {
                return string.Format("<i>{0}</i>", "Unknown");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void Update()
        {
            string newText = GetText();
            if (text.text != newText)
            {
                text.text = newText;
            }
        }

        #endregion Private Methods
    }
}