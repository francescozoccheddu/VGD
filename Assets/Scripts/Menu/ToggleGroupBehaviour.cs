using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.Menu
{
    [RequireComponent(typeof(ToggleGroup))]
    public sealed class ToggleGroupBehaviour : MonoBehaviour
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
        public GameObject template;
        private object[] m_items;

        public object Value { get => Items[Index]; set => Index = Array.IndexOf(Items, value); }

        public int Index
        {
            get
            {
                ToggleGroup group = GetComponent<ToggleGroup>();
                IItemTemplate presenter = group.ActiveToggles().FirstOrDefault().GetComponent<IItemTemplate>();
                return presenter == null ? -1 : Array.IndexOf(Items, presenter.Item);
            }
            set => transform.GetChild(value).GetComponent<Toggle>().isOn = true;
        }

        public void Create()
        {
            Destroy();
            if (Items != null)
            {
                ToggleGroup group = GetComponent<ToggleGroup>();
                foreach (object item in Items)
                {
                    GameObject presenter = Instantiate(template, transform);
                    presenter.GetComponent<Toggle>().group = group;
                    presenter.GetComponent<IItemTemplate>().Item = item;
                }
                Index = 0;
            }
        }

        public void Destroy()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnEnable() => Create();

    }

}
