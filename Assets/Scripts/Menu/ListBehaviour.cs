using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    public sealed class ListBehaviour : MonoBehaviour
    {

        public interface IItemTemplate
        {

            object Item { get; set; }

        }
        public abstract class ItemPresenterBehaviour : MonoBehaviour, IItemTemplate
        {

            public object Item { get => m_item; set { m_item = value; Present(value); } }

            protected abstract void Present(object _item);

            private object m_item;

        }

        public object[] Items
        {
            get => m_items;
            set
            {
                m_items = value;
                if (enabled)
                {
                    Create();
                }
            }
        }

        public GameObject listPresenter;
        public GameObject itemPresenter;
        private object[] m_items;
        private ToggleGroup m_group;

        public object Value { get => Items[Index]; set => Index = Array.IndexOf(Items, value); }

        public int Index
        {
            get
            {
                IItemTemplate presenter = m_group.ActiveToggles().FirstOrDefault().GetComponent<IItemTemplate>();
                return presenter == null ? -1 : Array.IndexOf(Items, presenter.Item);
            }
            set => m_group.transform.GetChild(value).GetComponent<Toggle>().isOn = true;
        }

        public void Create()
        {
            Destroy();
            if (Items != null)
            {
                m_group = Instantiate(listPresenter, transform).GetComponent<ToggleGroup>();
                foreach (object item in Items)
                {
                    GameObject presenter = Instantiate(itemPresenter, m_group.transform);
                    presenter.GetComponent<Toggle>().group = m_group;
                    presenter.GetComponent<IItemTemplate>().Item = item;
                }
                Index = 0;
            }
        }

        public void Destroy()
        {
            if (m_group?.gameObject != null)
            {
                Destroy(m_group.gameObject);
            }
            m_group = null;
        }

        private void OnEnable() => Create();

    }

}
