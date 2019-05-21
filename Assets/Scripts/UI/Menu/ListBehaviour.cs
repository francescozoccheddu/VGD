using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Wheeled.UI.Menu
{
    public sealed class ListBehaviour : MonoBehaviour
    {

        public interface IItemTemplate
        {

            int Index { get; set; }

        }

        public abstract class ItemPresenterBehaviour : MonoBehaviour, IItemTemplate
        {

            public int Index { get => m_index; set { m_index = value; Present(value); } }

            protected abstract void Present(int _index);

            private int m_index;

        }

        public int Count
        {
            get => m_count;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Positive int required", nameof(Count));
                }
                m_count = value;
                m_lastIndex = 0;
                if (enabled)
                {
                    Create();
                }
            }
        }

        public GameObject listPresenter;
        public GameObject itemPresenter;
        private ToggleGroup m_group;
        private int m_count;
        private int m_lastIndex;

        public int Index
        {
            get
            {
                return m_group?.ActiveToggles().FirstOrDefault()?.GetComponent<IItemTemplate>()?.Index ?? -1;
            }
            set
            {
                if (m_group != null)
                {
                    m_group.transform.GetChild(value).GetComponent<Toggle>().isOn = true;
                }
                m_lastIndex = value;
            }
        }

        public void Create()
        {
            Destroy();
            if (m_count >= 0)
            {
                m_group = Instantiate(listPresenter, transform).GetComponent<ToggleGroup>();
                for (int i=0; i < m_count; i++)
                {
                    GameObject presenter = Instantiate(itemPresenter, m_group.transform);
                    presenter.GetComponent<Toggle>().group = m_group;
                    presenter.GetComponent<IItemTemplate>().Index = i;
                }
                Index = m_lastIndex;
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
