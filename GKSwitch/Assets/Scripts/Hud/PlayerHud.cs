using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHud : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar;
    [SerializeField]
    private TMP_Text m_playerName;
    [SerializeField]
    private TMP_Text m_playerTitle;
    [SerializeField]
    private TMP_Text m_playerLevel;
    [SerializeField]
    private Image m_playerBlason;
    [SerializeField]
    private TMP_Text m_playerScore;
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private Image m_banner;
    [SerializeField]
    private Material[] m_scoreTextMaterial;
    
    [SerializeField]
    private Transform m_EmoticoneChoiceContainer;
    [SerializeField]
    private Transform m_EmoticonePlayedContainer;

    private ParticlePlayground.PlaygroundParticlesC m_blasonfxSparkles;
    //private List<EmoticoneButton> m_emoticoneList = new List<EmoticoneButton>();
    private float m_fLastEmoticoneTimer = 0f;

    private float m_fBotIconeTime = -1f;

/*    public void SetBanner( int nBannerId, string sBanName, Customisation.BannerType banType, Material fontMaterial = null )
    {
        Debug.Assert(m_banner != null);
        m_banner.sprite = Customisation.LoadBannerSprite(sBanName, banType);

        if( fontMaterial!=null )
        {
            m_playerScore.fontSharedMaterial = fontMaterial;
        }
        else if ( m_scoreTextMaterial!=null && m_scoreTextMaterial.Length>nBannerId && m_scoreTextMaterial[nBannerId]!=null)
        {
            m_playerScore.fontSharedMaterial = m_scoreTextMaterial[nBannerId];
        }
    }*/

    public void SetInfos( int nPlayerId )
    {
        m_fBotIconeTime = Time.realtimeSinceStartup + 1f;

        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        GKPlayerData playerData = BattleContext.instance.GetPlayer(nPlayerId);

        m_avatar.sprite = toasties.GetToasty(playerData.sToastyId).avatar;
        m_banner.sprite = gameSettings.playerSettings[nPlayerId].banner;

        /*if (m_EmoticoneChoiceContainer != null )
        {
            string[] emoList = PlayerData.instance.validEmoticonesId;

            if ( PlayerData.instance.IsFTUEEnded() && emoList.Length > 0 )
            {
                if (m_emoticoneList.Count > 0)
                {
                    for (int i = 0; i < m_emoticoneList.Count; i++)
                    {
                        GameObject.Destroy(m_emoticoneList[i].gameObject);
                    }
                    m_emoticoneList.Clear();
                }

                EmoticoneButton buttonPrefab = HudManager.instance.emoticoneButtonPrefab;

                for (int nEmoId = 0; nEmoId < emoList.Length; nEmoId++)
                {
                    EmoticoneButton btn = GameObject.Instantiate(buttonPrefab, m_EmoticoneChoiceContainer);
                    btn.SetEmoticone(emoList[nEmoId]);
                    btn.onButtonPress = OnEmoticone;
                    m_emoticoneList.Add(btn);
                }
                m_EmoticoneChoiceContainer.gameObject.SetActive(true);
            }
            else
            {
                m_EmoticoneChoiceContainer.gameObject.SetActive(false);
            }
            
        }*/
    }

    public void OnDisable()
    {
        if(m_EmoticonePlayedContainer!=null )
        {
            lwTools.DestroyAllChildren(m_EmoticonePlayedContainer.gameObject);
        }
    }

    
    public void SetScore(int nScore, bool bAdd )
    {
        Debug.Assert(m_playerScore != null);
        m_playerScore.text = nScore.ToString();
        if( m_animator!=null )
        {
            m_animator.SetTrigger(bAdd ? "Win" : "Lose");
        }
    }

    public void SetWinning( bool bWinning )
    {
        m_animator.SetBool("1st", bWinning);
    }

    
    public void PlayEmoticone( string sId )
    {
        if( m_EmoticonePlayedContainer != null )
        {
            /*EmoticonePlayed emo = GameObject.Instantiate(HudManager.instance.emoticonePlayedPrefab, m_EmoticonePlayedContainer);
            emo.SetEmoticone(sId);*/
        }
    }

    private void OnEmoticone( string sEmoticoneId )
    {
        if( Time.realtimeSinceStartup - m_fLastEmoticoneTimer > 0.5f )
        {
            m_fLastEmoticoneTimer = Time.realtimeSinceStartup;
//            BattleContext.instance.GetPlayer(0).CmdPlayEmoticone(sEmoticoneId);
        }
    }

}
