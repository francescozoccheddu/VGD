using UnityEngine.UI;

namespace Wheeled.Menu
{
    internal static class WidgetsHelper
    {
        #region Public Methods

        public static void NotifyValueChanged(this InputField _this)
        {
            _this.onValueChanged.Invoke(_this.text);
        }

        public static void NotifyChildToggleValueChanged(this ToggleGroup _this)
        {
            for (int i = 0; i < _this.transform.childCount; i++)
            {
                _this.transform.GetChild(i).GetComponent<Toggle>()?.NotifyValueChanged();
            }
        }

        public static void NotifyValueChanged(this Toggle _this)
        {
            _this.onValueChanged.Invoke(_this.isOn);
        }

        #endregion Public Methods
    }
}