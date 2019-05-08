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

        public UnityEngine.Object[] items;
        public GameObject template;

        public object Value { get => items[Index]; set => Index = Array.IndexOf(items, value); }

        public int Index
        {
            get
            {
                ToggleGroup group = GetComponent<ToggleGroup>();
                IItemTemplate presenter = group.ActiveToggles().FirstOrDefault().GetComponent<IItemTemplate>();
                return presenter == null ? -1 : Array.IndexOf(items, presenter.Item);
            }
            set => transform.GetChild(value).GetComponent<Toggle>().isOn = true;
        }

        public void Create()
        {
            ToggleGroup group = GetComponent<ToggleGroup>();
            foreach (UnityEngine.Object item in items)
            {
                GameObject presenter = Instantiate(template, transform);
                presenter.GetComponent<Toggle>().group = group;
                presenter.GetComponent<IItemTemplate>().Item = item;
            }
        }

        public void Destroy()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                foreach (Transform child in transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            };
            if (items != null && template != null)
            {
                Create();
            }
        }

        private void OnEnable()
        {
            Destroy();
            Create();
        }

    }
}
