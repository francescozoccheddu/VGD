using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wheeled.Core;

namespace Wheeled.UI.Menu
{
    public sealed class TutorialLauncherBehaviour : MonoBehaviour
    {

        public void StartTutorial()
        {
            GameLauncher.Instance.StartTutorial();
        }
            
    }
}
