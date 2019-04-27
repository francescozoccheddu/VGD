using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ListBehaviour : MonoBehaviour
    {
        #region Public Interfaces

        public interface IListItem
        {
            #region Public Properties

            int Index { get; set; }

            #endregion Public Properties
        }

        #endregion Public Interfaces

        #region Public Classes

        public abstract class ListItemBehaviour : MonoBehaviour, IListItem
        {
            #region Public Properties

            public int Index { get => m_index; set => SetIndex(m_index = value); }

            #endregion Public Properties

            #region Private Fields

            private int m_index;

            #endregion Private Fields

            #region Protected Methods

            protected abstract void SetIndex(int _index);

            #endregion Protected Methods
        }

        #endregion Public Classes

        #region Public Fields

        public GameObject itemPrefab;
        public GameObject groupPrefab;

        #endregion Public Fields

        #region Private Fields

        private ToggleGroup m_group;

        #endregion Private Fields

        #region Public Methods

        public void CreateChilds(int _count)
        {
            if (m_group != null)
            {
                Destroy(m_group.gameObject);
            }
            m_group = Instantiate(groupPrefab, transform).GetComponent<ToggleGroup>();
            m_group.allowSwitchOff = false;
            for (int i = 0; i < _count; i++)
            {
                GameObject item = Instantiate(itemPrefab, m_group.transform);
                item.GetComponent<Toggle>().group = m_group;
                item.GetComponent<IListItem>().Index = i;
            }
        }

        public int GetSelectedIndex()
        {
            return m_group.ActiveToggles().FirstOrDefault().GetComponent<IListItem>().Index;
        }

        public void SetSelectedIndex(int _index)
        {
            m_group.transform.GetChild(_index).GetComponent<Toggle>().isOn = true;
        }

        #endregion Public Methods
    }
}