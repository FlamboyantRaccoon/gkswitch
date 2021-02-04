using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BD_Logic : MiniGameLogic
{
    public int objectiveId { get; private set; }
    public bool bIsInitialized { get { return m_gameData != null; } }

    public System.Action<int> onObjectiveChangeDlg { set { m_onObjectiveChangeDlg = value; } }
    public System.Action<int, Vector2> onDisruptiveElementAppear { set { m_onDisruptiveElementAppear = value; } }
    public System.Action<int> onDisruptiveElementDisAppear { set { m_onDisruptiveElementDisAppear = value; } }
    public System.Action<int, Vector2> onDisruptiveElementAlterate { set { m_onDisruptiveElementAlterate = value; } }
    public System.Func<int, BD_BalloonDrill.BalloonDrillData> recoverGameData { set { m_recoverGameData = value; } }

    private bool m_bRefill;
    private System.Action<int> m_onObjectiveChangeDlg;
    private System.Action<int, Vector2> m_onDisruptiveElementAppear;
    private System.Action<int, Vector2> m_onDisruptiveElementAlterate;
    private System.Action<int> m_onDisruptiveElementDisAppear;
    private System.Func<int, BD_BalloonDrill.BalloonDrillData> m_recoverGameData;

    private BD_BalloonDrill.BalloonDrillData m_gameData;
    private int m_gameDataCurrentSequenceId = -1;
    private byte m_nFuturObjectiveId;
    
    private Queue<BD_BalloonDrill.BalloonSpawn> m_balloonSpawnQueue = new Queue<BD_BalloonDrill.BalloonSpawn>();

    public void Init(int nSpawnCount, byte nGameData, BD_BalloonDrill.BalloonDrillData gameData)
    {
        m_fGameStartTime = Time.time;
        objectiveId = -1;
        m_nFuturObjectiveId = (byte)(Random.Range(0, 6));
        m_nGameDataId = nGameData;
        m_gameData = gameData;
    }

    public void SelectObjective()
    {
        OnObjectiveChange(m_nFuturObjectiveId);
    }

    public BD_BalloonDrill.BalloonSpawn CheckIfBalloonToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_balloonSpawnQueue.Count > 0)
        {
            BD_BalloonDrill.BalloonSpawn spawn = m_balloonSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_balloonSpawnQueue.Dequeue();
                if (m_balloonSpawnQueue.Count == 0)
                {
                    m_bRefill = true;
                }
                return spawn;
            }
        }
        else
        {
            m_bRefill = true;
        }

        return null;
    }

    public void Update()
    {
        if (m_fGameStartTime < 0)
        {
            return;
        }

        if (m_gameData != null)
        {
            int nCurrentSequence = m_gameDataCurrentSequenceId;
            GetCurrentSequence();
            if (nCurrentSequence != m_gameDataCurrentSequenceId)
            {
                int xPreviousMask = nCurrentSequence >= 0 ? m_gameData.sequencesArray[nCurrentSequence].GetDisruptiveMask() : 0;
                int xNewMask = m_gameData.sequencesArray[m_gameDataCurrentSequenceId].GetDisruptiveMask();
                int xUnion = xPreviousMask & xNewMask;

                if (xPreviousMask != xNewMask)
                {
                    //    RpcDisableDisruptiveElement(xPreviousMask - xUnion);
                    DisableDisruptiveElement(xPreviousMask - xUnion);
                    //    RpcSetDisruptiveElement(xNewMask - xUnion, m_gameData.sequencesArray[m_gameDataCurrentSequenceId].disruptiveElementSpeed );
                    SetDisruptiveElement(xNewMask - xUnion, m_gameData.sequencesArray[m_gameDataCurrentSequenceId].disruptiveElementSpeed);
                }

                if (xUnion != 0)
                {
                    AlterateDisruptiveElement(xUnion, m_gameData.sequencesArray[m_gameDataCurrentSequenceId].disruptiveElementSpeed);
                    //    RpcAlterateDisruptiveElement(xUnion, m_gameData.sequencesArray[m_gameDataCurrentSequenceId].disruptiveElementSpeed);
                }
            }
        }

        if (m_bRefill)
        {
            SetGameRefill();
            m_bRefill = false;
        }
    }

    private void OnObjectiveChange(int nObjectivId)
    {
        objectiveId = nObjectivId;
        m_onObjectiveChangeDlg?.Invoke(objectiveId);
    }

    public void SetGameRefill()
    {
        uint nSeed = (uint)Random.Range(1, int.MaxValue);
        RefillWithSeed(nSeed);
    }

    public void RefillWithSeed(uint seed)
    {
        if (!bIsInitialized)
        {
            return;
        }

        RrRndHandler.RndSeed(seed);
        float fGameTime = m_fGameStartTime <= 0 ? 0 : Time.time - m_fGameStartTime;

        int nBalloonCount = 100;
        float fSpawnerRatioPos;
        float fSpeed;
        int nColorId;
        float fSpawnTime = fGameTime;
        float fDepth;

        for (int nBalloonSpawn = 0; nBalloonSpawn < nBalloonCount; nBalloonSpawn++)
        {
            BD_BalloonDrill.BalloonDrillDataSequence dataSequence = GetSequence(fSpawnTime);
            float fTime = 1f / dataSequence.balloonBySecond;
            fSpawnTime += fTime;

            fSpawnerRatioPos = RrRndHandler.RndRange(0, 1f);
            fSpeed = RrRndHandler.RndRange(dataSequence.speed.x, dataSequence.speed.y);
            int nPercent = RrRndHandler.RndRange(0, 100);
            int nObjective = objectiveId != -1 ? objectiveId : m_nFuturObjectiveId;
            nColorId = nObjective;
            if (nPercent >= dataSequence.goodColorPercent) // Have to change color
            {
                nColorId = RrRndHandler.RndRange(0, dataSequence.colorCount - 1);
                if (nColorId >= nObjective)
                {
                    nColorId++;
                }
            }

            fDepth = RrRndHandler.RndRange(0, 1f);

            m_balloonSpawnQueue.Enqueue(new BD_BalloonDrill.BalloonSpawn(fSpawnerRatioPos,
                                                                         fSpeed,
                                                                         nColorId,
                                                                         fSpawnTime,
                                                                         fDepth));
        }
    }

    private BD_BalloonDrill.BalloonDrillDataSequence GetCurrentSequence()
    {
        float fCurrentTime = m_fGameStartTime <= 0 ? 0 : Time.time - m_fGameStartTime;
        while (m_gameDataCurrentSequenceId < m_gameData.sequencesArray.Length - 1 &&
            fCurrentTime >= m_gameData.sequencesArray[m_gameDataCurrentSequenceId + 1].startTime)
        {
            m_gameDataCurrentSequenceId++;
        }
        Debug.Assert(m_gameDataCurrentSequenceId >= 0 && m_gameDataCurrentSequenceId < m_gameData.sequencesArray.Length);
        return m_gameData.sequencesArray[m_gameDataCurrentSequenceId];
    }

    private BD_BalloonDrill.BalloonDrillDataSequence GetSequence(float fTime)
    {
        int nSequenceId = 0;
        while (nSequenceId < m_gameData.sequencesArray.Length - 1 &&
            fTime >= m_gameData.sequencesArray[nSequenceId + 1].startTime)
        {
            nSequenceId++;
        }
        Debug.Assert(nSequenceId >= 0 && nSequenceId < m_gameData.sequencesArray.Length);
        return m_gameData.sequencesArray[nSequenceId];
    }

    private void SetDisruptiveElement(int xMask, Vector2 vSpeed)
    {
        m_onDisruptiveElementAppear?.Invoke(xMask, vSpeed);
    }

    private void DisableDisruptiveElement(int xMask)
    {
        m_onDisruptiveElementDisAppear?.Invoke(xMask);
    }

    private void AlterateDisruptiveElement(int xMask, Vector2 vSpeed)
    {
        m_onDisruptiveElementAlterate?.Invoke(xMask, vSpeed);
    }
}
