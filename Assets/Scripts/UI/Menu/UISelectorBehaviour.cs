using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wheeled.UI.Menu
{
    public sealed class UISelectorBehaviour : MonoBehaviour
    {

        public string[] selectionButtons;

        private void Update()
        {
            if (selectionButtons.Select(_n => Input.GetButtonDown(_n) || Input.GetAxisRaw(_n) != 0.0f).Any(_b => _b))
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if (selected?.activeInHierarchy != true)
                {
                    GameObject.FindGameObjectWithTag("UIDefaultSelection")?.GetComponentInChildren<Selectable>()?.Select();
                }
            }
        }

    }
}
