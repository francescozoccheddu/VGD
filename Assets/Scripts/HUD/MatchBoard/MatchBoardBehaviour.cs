using UnityEngine;

namespace Wheeled.HUD
{
    public sealed class MatchBoardBehaviour : MonoBehaviour
    {
        #region Private Fields

        private static Transform s_instance;

        #endregion Private Fields

        #region Public Methods

        public static void Add(MatchBoardEventBehaviour _event)
        {
            _event.transform.SetParent(s_instance);
            _event.transform.SetAsLastSibling();
        }

        #endregion Public Methods

        #region Private Methods

        private void Start()
        {
            s_instance = transform;
        }

        #endregion Private Methods
    }
}