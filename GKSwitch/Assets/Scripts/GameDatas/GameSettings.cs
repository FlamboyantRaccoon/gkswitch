using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting", menuName = "GameData/GameSetting", order = 1)]
public class GameSettings : ScriptableObject
{
    [System.Serializable]
    public class PlayerSetting
    {
        public Color color;
        public Sprite banner;
    }

    public PlayerSetting[] playerSettings;

}
