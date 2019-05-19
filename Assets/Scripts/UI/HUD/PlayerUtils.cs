using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wheeled.Core.Data;
using Wheeled.Gameplay.Player;

namespace Wheeled.UI.HUD
{

    public static class PlayerUtils
    {

        public static string GetName(this IReadOnlyPlayer _player, bool _colored = false)
        {

            string name = _player?.Info?.name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return string.Format("Player {0}", _player.Id);
            }
            else
            {
                return name;
            }
        }

        public static string GetColoredName(this IReadOnlyPlayer _player)
        {
            return string.Format("<color=\"#{0}\">{1}</color>", ColorUtility.ToHtmlStringRGB(_player.GetColor()), _player.GetName());
        }

     

    }

}
