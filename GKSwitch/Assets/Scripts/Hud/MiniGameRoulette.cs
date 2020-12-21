using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameRoulette : MonoBehaviour
{
    [SerializeField]
    MiniGameCard miniGameCardPrefab;


    [Header("Config")]
    [Tooltip("angle in degree for the first card, just notice that it was hide for last turn")]
    [SerializeField]
    private AnimationCurve m_rotationInTime;
    [SerializeField]
    private float m_varianceY = 50f;
    [SerializeField]
    private float m_varianceX = 450f;

    MiniGameCard[] miniGameCards;
    private int m_rouletteTurn;
    private bool m_bWaitCardAnimation;
    private int m_nCurrentCard;

    public void Init()
    {
        MiniGameManager.MiniGames[] miniGames = BattleContext.instance.m_battleAvailableGames;
        int nMiniGameCount = miniGames.Length;

        if( miniGameCards!=null )
        {
            for( int i=0; i<miniGameCards.Length; i++ )
            {
                GameObject.Destroy(miniGameCards[i].gameObject);
            }
        }

        miniGameCards = new MiniGameCard[nMiniGameCount];
        for( int nCardId=0; nCardId<nMiniGameCount; nCardId++)
        {
            miniGameCards[nCardId] = GameObject.Instantiate(miniGameCardPrefab, transform);
            miniGameCards[nCardId].Init((int)miniGames[nCardId]);
            SetCardAngle(miniGameCards[nCardId], 0f);
        }
        m_rouletteTurn = (int)(((m_rotationInTime.keys[m_rotationInTime.length - 1].value) / 360f) - 1);
        SelectCard(0);
    }

    public void Hide()
    {
        for (int nCardId = 0; nCardId < miniGameCards.Length; nCardId++)
        {
            miniGameCards[nCardId].gameObject.SetActive(false);
        }
    }

    public void Start()
    {
        //LaunchRouletteAnimation();
    }

    public void LaunchRouletteAnimation( System.Action action )
    {
        InitCards();
        StartCoroutine(LaunchRouletteAnimationRoutine( action ));
    }

    public void IncrementSelection( int increment)
    {
        m_nCurrentCard = (m_nCurrentCard + miniGameCards.Length + increment) % miniGameCards.Length;
        SelectCard(m_nCurrentCard);
    }

    public MiniGameManager.MiniGames GetSelectedMiniGame()
    {
        MiniGameManager.MiniGames[] miniGames = BattleContext.instance.m_battleAvailableGames;
        return miniGames[m_nCurrentCard];
    }

    private IEnumerator LaunchRouletteAnimationRoutine(System.Action action )
    {
        float fElapsedTime = 0;
        float fStartTime = Time.time;

        
        for (int nCardId = 0; nCardId < miniGameCards.Length; nCardId++)
        {
            miniGameCards[nCardId].gameObject.SetActive(false);
        }
        int nCardShown = 0;

        miniGameCards[0].gameObject.SetActive(true);
        nCardShown = 1;
        int nCardArrive = 0;

        m_bWaitCardAnimation = true;
        miniGameCards[0].onEndSelectionAnim = OnEndCardIntro;
        miniGameCards[0].PlaySelectedAnim();
        while( m_bWaitCardAnimation )
        {
            yield return null;
        }
        miniGameCards[0].onEndSelectionAnim = null;

        if( miniGameCards.Length > 1 )
        {
            //FMODUnity.RuntimeManager.PlayOneShot("event:/Menu/Spinning"); // "event:/GoodMove");

            while (nCardArrive < miniGameCards.Length - 1)
            {
                fElapsedTime = Time.time - fStartTime;
                float fStartAngle = (m_rotationInTime.Evaluate(fElapsedTime)) * Mathf.Deg2Rad;
                float fAngleOffset = (360f / miniGameCards.Length) * Mathf.Deg2Rad;

                for (int nCardId = 0; nCardId < miniGameCards.Length; nCardId++)
                {
                    float fAngle = fStartAngle - nCardId * fAngleOffset;
                    if (fAngle > 0)
                    {
                        // check start
                        if (nCardId == nCardShown - 1 && nCardShown < miniGameCards.Length)
                        {
                            miniGameCards[nCardShown].gameObject.SetActive(true);
                            SetCardAngle(miniGameCards[nCardShown], 0f);
                            nCardShown++;
                        }

                        // check end
                        int nTurn = (int)(fAngle / (2 * Mathf.PI));
                        if (nTurn >= m_rouletteTurn)
                        {
                            SetCardAngle(miniGameCards[nCardId], 0f);
                            miniGameCards[nCardId].SetOrderInLayer(91 + nCardId);
                            nCardArrive = Mathf.Max(nCardArrive, nCardId);
                        }
                        else
                        {
                            SetCardAngle(miniGameCards[nCardId], fStartAngle - nCardId * fAngleOffset);
                        }
                    }
                }
                yield return null;
            }

            m_bWaitCardAnimation = true;
            int nLast = miniGameCards.Length - 1;
            miniGameCards[nLast].onEndSelectionAnim = OnCardReadyToShowLevels;
            miniGameCards[nLast].PlaySelectedAnim();
            while (m_bWaitCardAnimation)
            {
                yield return null;
            }
            miniGameCards[nLast].onEndSelectionAnim = null;
        }

        yield return new WaitForSeconds(0.3f);
        if( action!=null )
        {
            action();
        }
    }

    public void ReLaunchRouletteAnimation(System.Action action)
    {
        
        StartCoroutine(ReLaunchRouletteAnimationRoutine(action));
    }

    private IEnumerator ReLaunchRouletteAnimationRoutine(System.Action action)
    {
        // clean levels
        int nLast = miniGameCards.Length - 1;
        m_bWaitCardAnimation = true;
        miniGameCards[nLast].HideLevels(OnEndCardIntro);
        while (m_bWaitCardAnimation)
        {
            yield return null;
        }
        InitCards();

        float fElapsedTime = 0;
        float fStartTime = Time.time;
        //FMODUnity.RuntimeManager.PlayOneShot("event:/Menu/Spinning"); // "event:/GoodMove");
        int nCardArrive = 0;
        
        while (nCardArrive < miniGameCards.Length - 1)
        {
            fElapsedTime = Time.time - fStartTime;
            float fStartAngle = (m_rotationInTime.Evaluate(fElapsedTime)) * Mathf.Deg2Rad;
            float fAngleOffset = (360f / miniGameCards.Length) * Mathf.Deg2Rad;

            for (int nCardId = 0; nCardId < miniGameCards.Length; nCardId++)
            {
                float fAngle = fStartAngle - nCardId * fAngleOffset;
                if (fAngle > 0)
                {
                    // check end
                    int nTurn = (int)(fAngle / (2 * Mathf.PI));
                    if (nTurn >= m_rouletteTurn)
                    {
                        SetCardAngle(miniGameCards[nCardId], 0f);
                        miniGameCards[nCardId].SetOrderInLayer(91 + nCardId);
                        nCardArrive = Mathf.Max(nCardArrive, nCardId);
                    }
                    else
                    {
                        SetCardAngle(miniGameCards[nCardId], fStartAngle - nCardId * fAngleOffset);
                    }
                }
            }
            yield return null;
        }

        m_bWaitCardAnimation = true;
        nLast = miniGameCards.Length - 1;
        miniGameCards[nLast].onEndSelectionAnim = OnCardReadyToShowLevels;
        miniGameCards[nLast].PlaySelectedAnim();
        while (m_bWaitCardAnimation)
        {
            yield return null;
        }
        miniGameCards[nLast].onEndSelectionAnim = null;

        yield return new WaitForSeconds(0.3f);
        if (action != null)
        {
            action();
        }
    }

    private void OnCardReadyToShowLevels()
    {
        int nLast = miniGameCards.Length - 1;
        miniGameCards[nLast].ShowLevels( OnEndCardIntro );
    }

    private void OnEndCardIntro()
    {
        m_bWaitCardAnimation = false;
    }

    private void SetCardAngle( MiniGameCard card, float fAngleInRad )
    {
        
        float fSin = Mathf.Sin(fAngleInRad);
        float fCos = Mathf.Cos(fAngleInRad);
        float fX = -fSin * m_varianceX;
        float fY = -fCos * m_varianceY;
        int nAngleInDeg = ((int)(fAngleInRad * Mathf.Rad2Deg)) % 360;
        if( nAngleInDeg > 180 )
        {
            nAngleInDeg = 360 - nAngleInDeg;
        }
        int nOrder = 181 - nAngleInDeg;
        card.SetPositionAndOrderInLayer(new Vector3(fX, fY, 0f), nOrder);

    }

    private void InitCards()
    {
        int nSelectedGame = (int)BattleContext.instance.selectedMiniGame;
        lwRndArray rnd = new lwRndArray((uint)miniGameCards.Length);

        MiniGameManager.MiniGames[] miniGames = BattleContext.instance.m_battleAvailableGames;
        int nSelectedId = -1;
        int nIterator = 0;
        while( nSelectedId==-1 && nIterator <miniGames.Length )
        {
            if( nSelectedGame == (int)miniGames[nIterator] )
            {
                nSelectedId = nIterator;
            }
            else
            {
                nIterator++;
            }
        }

        if(nSelectedId!=-1 )
        {
            rnd.SetValueAsChoosen((uint)nSelectedId);
        }

        miniGameCards[miniGameCards.Length - 1].Init(nSelectedGame);
        for( int nCardId = 0; nCardId< miniGameCards.Length - 1; nCardId++ )
        {
            miniGameCards[nCardId].Init((int)miniGames[(int)rnd.ChooseValue()]);
        }
    }

    private void SelectCard( int nCardId )
    {
        float fDeltaAngle = 360f / miniGameCards.Length;
        float fAngle = 0;
        for( int i=0; i<miniGameCards.Length; i++ )
        {
            fAngle = (i * fDeltaAngle) * Mathf.Deg2Rad;
            int index = (nCardId + i) % miniGameCards.Length;
            SetCardAngle(miniGameCards[index], fAngle);
        }
    }

}
