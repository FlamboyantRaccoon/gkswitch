using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameCardBase : MonoBehaviour
{
    [System.Serializable] public class MiniGameSprite : lwEnumArray<MiniGameManager.MiniGames, Sprite> { }; // dummy definition to use Unity serialization

    [SerializeField]
    Image m_miniGameImage;
    [SerializeField]
    protected MiniGameSprite m_miniGameSpriteArray;
    [SerializeField]
    private TMP_Text m_miniGameName;
    [SerializeField]
    protected TMP_Text m_miniGameLevel;

    public void InitGame(int nMiniGameId)
    {
        if (nMiniGameId >= 0 && nMiniGameId < m_miniGameSpriteArray.nLength)
        {
            m_miniGameImage.sprite = m_miniGameSpriteArray[nMiniGameId];
        }

        if (nMiniGameId < System.Enum.GetNames(typeof(MiniGameManager.MiniGames)).Length)
        {
            MiniGameManager.MiniGames miniGame = (MiniGameManager.MiniGames)nMiniGameId;
            SetLevel(0);
            string sMiniGameName = lwLanguageManager.instance.GetString("str_miniGameTitle_" + miniGame.ToString());
            if (sMiniGameName != "")
            {
                m_miniGameName.text = sMiniGameName;
            }

           
        }
        else
        {
            string sMiniGameName = lwLanguageManager.instance.GetString("str_miniGameNextTitle");
            if (sMiniGameName != "")
            {
                m_miniGameName.text = sMiniGameName;
            }
            m_miniGameLevel.text = "";
        }
    }

    public void SetLevel(int nLevel)
    {
        m_miniGameLevel.text = (nLevel + 1).ToString();
    }
}
