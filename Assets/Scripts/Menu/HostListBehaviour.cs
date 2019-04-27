using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Networking;

namespace Wheeled.Menu
{
    public sealed class HostListBehaviour : MonoBehaviour
    {
        #region Public Fields

        public GameObject loadingLabel;
        public GameObject arenaEntryPrefab;
        public GameObject listPrefab;

        #endregion Public Fields

        #region Private Fields

        private const float c_discoverPeriod = 1.0f;
        private readonly Dictionary<IPEndPoint, int> m_hosts = new Dictionary<IPEndPoint, int>();
        private ToggleGroup m_group;
        private int m_port;

        #endregion Private Fields

        #region Public Methods

        public void PortChanged(string _port)
        {
            if (PortValidatorBehaviour.IsValidPort(_port))
            {
                m_port = int.Parse(_port);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Discover()
        {
            GameLauncher.Instance.StartServerDiscovery(m_port);
        }

        private void Discovered(GameRoomInfo _info)
        {
            if (m_group != null)
            {
                if (!m_hosts.ContainsKey(_info.endPoint))
                {
                    m_hosts.Add(_info.endPoint, _info.map);
                    loadingLabel.SetActive(false);
                    GameObject entry = Instantiate(arenaEntryPrefab, m_group.transform);
                    entry.GetComponent<HostArenaEntryBehaviour>().Index = _info.map;
                    entry.GetComponent<Toggle>().group = m_group;
                }
            }
        }

        private void OnEnable()
        {
            Stop();
            GameLauncher.Instance.OnGameRoomDiscovered += Discovered;
            m_port = 9060;
            m_group = Instantiate(listPrefab, transform).GetComponent<ToggleGroup>();
            loadingLabel.SetActive(true);
            InvokeRepeating(nameof(Discover), c_discoverPeriod, c_discoverPeriod);
        }

        private void Stop()
        {
            GameLauncher.Instance.OnGameRoomDiscovered -= Discovered;
            m_hosts.Clear();
            if (m_group != null)
            {
                Destroy(m_group.gameObject);
            }
            m_group = null;
            CancelInvoke();
        }

        private void OnDisable()
        {
            Stop();
        }

        #endregion Private Methods
    }
}