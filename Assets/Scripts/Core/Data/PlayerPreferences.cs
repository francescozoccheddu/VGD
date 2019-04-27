using System.Linq;
using UnityEngine;
using Wheeled.Gameplay.Player;

namespace Wheeled.Core.Data
{
    internal static class PlayerPreferences
    {
        #region Public Properties

        public static int HeadIndex
        {
            get
            {
                int index = PlayerPrefs.GetInt("head", default);
                if (IsValidHeadIndex(index))
                {
                    return index;
                }
                else
                {
                    PlayerPrefs.DeleteKey("head");
                    return 0;
                }
            }
            set
            {
                if (IsValidHeadIndex(value))
                {
                    PlayerPrefs.SetInt("head", value);
                }
            }
        }

        public static int ColorIndex
        {
            get
            {
                int index = PlayerPrefs.GetInt("color", default);
                if (IsValidColorIndex(index))
                {
                    return index;
                }
                else
                {
                    PlayerPrefs.DeleteKey("color");
                    return 0;
                }
            }
            set
            {
                if (IsValidColorIndex(value))
                {
                    PlayerPrefs.SetInt("color", value);
                }
            }
        }

        public static HeadScript Head => Scripts.PlayerPreferences.heads[HeadIndex];

        public static Color Color => Scripts.PlayerPreferences.colors[ColorIndex];

        public static string Name
        {
            get
            {
                string name = PlayerPrefs.GetString("name", null);
                if (IsValidName(name))
                {
                    return name;
                }
                else
                {
                    PlayerPrefs.DeleteKey("name");
                    return null;
                }
            }
            set
            {
                if (IsValidName(value))
                {
                    PlayerPrefs.SetString("name", value);
                }
            }
        }

        public static PlayerInfo Info => new PlayerInfo
        {
            color = (byte) ColorIndex,
            head = (byte) HeadIndex,
            name = Name
        };

        #endregion Public Properties

        #region Public Methods

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static bool IsValidName(string _name)
        {
            return _name != null && _name.All(char.IsLetterOrDigit) && _name.Length > 0 && _name.Length <= 16;
        }

        #endregion Public Methods

        #region Private Methods

        private static bool IsValidColorIndex(int _index)
        {
            return _index >= 0 && _index < Scripts.PlayerPreferences.colors.Length;
        }

        private static bool IsValidHeadIndex(int _index)
        {
            return _index >= 0 && _index < Scripts.PlayerPreferences.heads.Length;
        }

        #endregion Private Methods
    }
}