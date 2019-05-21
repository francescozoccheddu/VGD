using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Networking;

namespace Wheeled.UI.Menu
{
    public sealed class HostListBehaviour : MonoBehaviour
    {
        public GameObject loadingLabel;
        public GameObject listGroup;
        public GameObject hostTogglePrefab;
        public GameObject listPrefab;
        public InputField ipField;
        public InputField portField;
        public Transform listContent;

        private const float c_discoverPeriod = 1.0f;
        private readonly HashSet<IPEndPoint> m_hosts = new HashSet<IPEndPoint>();
        private ToggleGroup m_group;
        private int m_port;

        public void PortChanged(int? _port)
        {
            if (_port != null)
            {
                m_port = _port.Value;
            }
            UpdateCurrent(IPValidatorBehaviour.ParseIP(ipField.text), _port);
        }

        public void IPChanged(IPAddress _ip)
        {
            UpdateCurrent(_ip, PortValidatorBehaviour.ParsePort(portField.text));
        }

        private void UpdateCurrent(IPAddress _ip, int? _port)
        {
            if (m_group != null)
            {
                IPEndPoint endPoint = null;
                if (_ip != null && _port != null)
                {
                    endPoint = new IPEndPoint(_ip, _port.Value);
                }
                for (int i = 0; i < m_group.transform.childCount; i++)
                {
                    var child = m_group.transform.GetChild(i);
                    bool isOn = child.GetComponent<HostToggleBehaviour>().EndPoint.Equals(endPoint);
                    child.GetComponent<Toggle>().SetIsOnWithoutNotify(isOn);
                }
            }
        }

        private void Discover() => GameLauncher.Instance.StartServerDiscovery(m_port);

        private void Discovered(GameRoomInfo _info)
        {
            if (m_group != null)
            {
                if (!m_hosts.Contains(_info.endPoint))
                {
                    loadingLabel.SetActive(false);
                    listGroup.SetActive(true);
                    GameObject entry = Instantiate(hostTogglePrefab, m_group.transform);
                    Toggle toggle = entry.GetComponent<Toggle>();
                    toggle.group = m_group;
                    toggle.onValueChanged.AddListener(_isOn =>
                    {
                        if (_isOn)
                        {
                            ipField.text = _info.endPoint.Address.MapToIPv4().ToString();
                            portField.text = _info.endPoint.Port.ToString();
                        }
                        else
                        {
                            IPAddress address = IPValidatorBehaviour.ParseIP(ipField.text);
                            int? port = PortValidatorBehaviour.ParsePort(portField.text);
                            if (address != null && port != null)
                            {
                                IPEndPoint endPoint = new IPEndPoint(address, port.Value);
                                if (endPoint.Equals(_info.endPoint))
                                {
                                    ipField.text = "";
                                }
                            }
                        }
                    });
                    HostToggleBehaviour toggleBehaviour = entry.GetComponent<HostToggleBehaviour>();
                    toggleBehaviour.Arena = _info.arena;
                    toggleBehaviour.EndPoint = _info.endPoint;
                    m_hosts.Add(_info.endPoint);
                }
                else
                {
                    HostToggleBehaviour toggleBehaviour = m_group.GetComponentsInChildren<HostToggleBehaviour>()
                        .FirstOrDefault(_c => _c.EndPoint.Equals(_info.endPoint));
                    toggleBehaviour.Arena = _info.arena;
                }
            }
        }

        private void OnEnable()
        {
            Stop();
            GameLauncher.Instance.OnGameRoomDiscovered += Discovered;
            m_port = 9060;
            m_group = Instantiate(listPrefab, listContent).GetComponent<ToggleGroup>();
            loadingLabel.SetActive(true);
            InvokeRepeating(nameof(Discover), c_discoverPeriod, c_discoverPeriod);
        }

        private void Stop()
        {
            CancelInvoke();
            GameLauncher.Instance.OnGameRoomDiscovered -= Discovered;
            m_hosts.Clear();
            if (m_group != null)
            {
                Destroy(m_group.gameObject);
            }
            m_group = null;
            listGroup.SetActive(false);
        }

        private void OnDisable() => Stop();
    }
}