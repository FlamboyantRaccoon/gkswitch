using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting", menuName = "GameData/GameSetting", order = 1)]
public class GameSettings : ScriptableObject
{
    [System.Serializable] public class MiniGameSprite : lwEnumArray<MiniGameManager.MiniGames, Sprite> { }; // dummy definition to use Unity serialization

    [System.Serializable]
    public class PlayerSetting
    {
        public Color color;
        public Sprite banner;
        public Sprite cursor;
        public Material textMaterial;
    }

    public PlayerSetting[] playerSettings;
    public MiniGameSprite m_miniGameSprites;
}
