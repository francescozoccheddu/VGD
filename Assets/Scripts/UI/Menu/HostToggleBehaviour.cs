using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core.Data;

namespace Wheeled.Menu
{
    public sealed class HostToggleBehaviour : MonoBehaviour
    {

        public Text endPointText;
        public Text arenaText;
        private IPEndPoint m_endPoint;
        private int m_arena;

        public IPEndPoint EndPoint
        {
            get => m_endPoint;
            set
            {
                m_endPoint = value;
                endPointText.text = value.ToString();
            }
        }

        public int Arena
        {
            get => m_arena;
            set
            {
                m_arena = value;
                arenaText.text = Scripts.Scenes.arenas[value].name;
            }
        }

    }
}
