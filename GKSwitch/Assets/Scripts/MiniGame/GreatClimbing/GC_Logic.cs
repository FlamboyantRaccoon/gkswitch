using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_Logic : MiniGameLogic
{
    public System.Func<int, int, float> computeAltFunc { set { m_computeAltFunc = value; } }

    private Dictionary<string, GC_GreatClimbing.WallEltData> m_wallEltData;
    private System.Func<int, int, float> m_computeAltFunc;

    private GC_GreatClimbing.GreatClimbingData m_gameData;
    private List<GC_GreatClimbing.GripData> m_waitingGripList;
    private float m_fWallEltSize;

    private uint m_nGlobalInitSeed;

    public void Init(GC_GreatClimbing.GreatClimbingData gameData, byte nGameData, float fWallEltSize)
    {
        m_wallEltData = new Dictionary<string, GC_GreatClimbing.WallEltData>();
        m_nGameDataId = nGameData;
        m_fWallEltSize = fWallEltSize;
        m_gameData = gameData;
        m_waitingGripList = new List<GC_GreatClimbing.GripData>();

        m_nGlobalInitSeed = (uint)Random.Range(1, int.MaxValue); //902396589; // 
        RrRndHandler.RndSeed(m_nGlobalInitSeed);
        GenerateFirstGrip();
    }

    public List<GC_GreatClimbing.WallEltData> GetWallElt(int nStartX, int nEndX, int nStartY, int nEndY)
    {
        List<int> coordList = new List<int>();
        for (int nY = nStartY; nY < nEndY; nY++)
        {
            for (int nX = nStartX; nX < nEndX; nX++)
            {
                coordList.Add(nX);
                coordList.Add(nY);
            }
        }
        return GetWallElt(coordList.ToArray());
    }

    public List<GC_GreatClimbing.WallEltData> GetWallElt(int[] coordList)
    {
        List<GC_GreatClimbing.WallEltData> wallList = new List<GC_GreatClimbing.WallEltData>();

        List<int> nXElts = new List<int>();
        List<int> nYElts = new List<int>();
        List<byte> nGripsByElt = new List<byte>();
        List<byte> nGripMask = new List<byte>();
        List<float> fGripXs = new List<float>();
        List<float> fGripYs = new List<float>();

        CheckAndGenerateNewGrip(GetMaxYInCoordArray(coordList));

        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        for (int nCoordIterator = 0; nCoordIterator < coordList.Length - 1; nCoordIterator += 2)
        {
            int nX = coordList[nCoordIterator];
            int nY = coordList[nCoordIterator + 1];

            string sKey = nX.ToString() + "," + nY.ToString();
            GC_GreatClimbing.WallEltData wallData;
            if (m_wallEltData.TryGetValue(sKey, out wallData))
            {
                wallList.Add(wallData);
            }
            else
            {

                nXElts.Add(nX);
                nYElts.Add(nY);

                List<GC_GreatClimbing.GripData> wallGrips = GetWallEltGrip(nX, nY);
                if (wallGrips != null && wallGrips.Count > 0)
                {
                    // add in net message only if they are somethings to delete
                    xList.Add(nX);
                    yList.Add(nY);
                }

                byte nGripCount = (byte)wallGrips.Count;
                nGripsByElt.Add(nGripCount);
                for (int nGrip = 0; nGrip < nGripCount; nGrip++)
                {
                    byte gripMask = (byte)((wallGrips[nGrip].nGripId << 4) | wallGrips[nGrip].nObstacleLife);
                    nGripMask.Add(gripMask);
                    fGripXs.Add(wallGrips[nGrip].vPos.x);
                    fGripYs.Add(wallGrips[nGrip].vPos.y);
                }
                int nLastEltId = nXElts.Count - 1;
                int nLastGripId = nGripMask.Count;

                wallData = new GC_GreatClimbing.WallEltData(nXElts[nLastEltId], nYElts[nLastEltId],
                    nGripCount > 0 ? (nGripMask.GetRange(nLastGripId - nGripCount, nGripCount)).ToArray() : new byte[] { },
                    nGripCount > 0 ? (fGripXs.GetRange(nLastGripId - nGripCount, nGripCount)).ToArray() : new float[] { },
                    nGripCount > 0 ? (fGripYs.GetRange(nLastGripId - nGripCount, nGripCount)).ToArray() : new float[] { });
                m_wallEltData.Add(sKey, wallData);
                wallList.Add(wallData);
            }
        }

        return wallList;
    }

    private int GetMaxYInCoordArray(int[] coordList)
    {
        int nY = 0;
        for (int i = 0; i < coordList.Length; i += 2)
        {
            if (coordList[i + 1] > nY)
            {
                nY = coordList[i + 1];
            }
        }
        return nY;
    }

    private void CheckAndGenerateNewGrip(int nWallEltY)
    {
        int nGripId = 0;
        while (nGripId < m_waitingGripList.Count)
        {
            if (m_waitingGripList[nGripId].nBlocY <= nWallEltY && !m_waitingGripList[nGripId].bAlreadyGenerateNext)
            {
                GC_GreatClimbing.GreatClimbingDataSequence sequence = GetSequence(m_waitingGripList[nGripId].nBlocX, m_waitingGripList[nGripId].nBlocY);
                Vector2 vCurrentPos = new Vector2((m_waitingGripList[nGripId].nBlocX + m_waitingGripList[nGripId].vPos.x) * m_fWallEltSize, (m_waitingGripList[nGripId].nBlocY + m_waitingGripList[nGripId].vPos.y) * m_fWallEltSize);

                int nChildCount = (int)RrRndHandler.RndRange(sequence.nextGripCount.x, sequence.nextGripCount.y);
                for (int nChildId = 0; nChildId < nChildCount; nChildId++)
                {
                    float fDist = RrRndHandler.RndRange(sequence.distMin, sequence.distMax);
                    float fAngle = 90f;
                    if (sequence.fAngle >= 0)
                    {
                        fAngle = (RrRndHandler.RndRange(-sequence.fAngle / 2, sequence.fAngle / 2) + 90f) * Mathf.Deg2Rad;
                    }
                    else
                    {
                        float fAngleRnd = RrRndHandler.RndRange(0f, 1f);
                        fAngle = (sequence.angleDispersionCurve.Evaluate(fAngleRnd) + 90f) * Mathf.Deg2Rad;
                    }
                    Vector2 vGripPos = new Vector2(Mathf.Cos(fAngle), Mathf.Sin(fAngle)) * fDist + vCurrentPos;
                    int nX = (int)(vGripPos.x > 0 ? vGripPos.x / m_fWallEltSize : (vGripPos.x / m_fWallEltSize) - 1);
                    int nY = (int)(vGripPos.y / m_fWallEltSize);

                    //Debug.Log("Test children : " + nX + ", " + nY + " from " + m_waitingGripList[nGripId].nBlocX + ", " + m_waitingGripList[nGripId].nBlocY + " (sequence : " + sequence.altitudeStart );

                    List<GC_GreatClimbing.GripData> gripList = GetWallEltGrip(nX, nY, false);
                    //Debug.Log("gripList.Count : " + gripList.Count);
                    bool bNeighborgValid = sequence.bAllowNeighborgForChildren;
                    if (!bNeighborgValid)
                    {
                        int nNeighborgCount = ComputeGripCountInNeighborg(nX, nY, m_waitingGripList[nGripId].nBlocX, m_waitingGripList[nGripId].nBlocY);
                        bNeighborgValid = nNeighborgCount == 0;
                        //Debug.Log("bNeighborgValid : " + nNeighborgCount );
                    }

                    if (nY >= m_waitingGripList[nGripId].nBlocY && gripList.Count < 1 && bNeighborgValid)
                    {
                        float fInsideX = (vGripPos.x - (nX * m_fWallEltSize)) / m_fWallEltSize;
                        float fInsideY = (vGripPos.y - (nY * m_fWallEltSize)) / m_fWallEltSize;

                        float fGripIdRnd = RrRndHandler.RndRange(0f, 1f);
                        int nGripSize = Mathf.RoundToInt(sequence.gripSizeProbability.Evaluate(fGripIdRnd));
                        float fGripLifeRnd = RrRndHandler.RndRange(0f, 1f);
                        int nObstaclesLife = Mathf.RoundToInt(sequence.gripObstacleLifeProbability.Evaluate(fGripLifeRnd));

                        GC_GreatClimbing.GripData gripData = new GC_GreatClimbing.GripData(nGripSize, fInsideX, fInsideY, nObstaclesLife, 0, nX, nY);
                        m_waitingGripList.Add(gripData);
                    }
                }
                m_waitingGripList[nGripId].bAlreadyGenerateNext = true;
            }
            nGripId++;
        }

        // test if there are a solution
        if (CheckIfWeNeedToAddAGrip())
        {
            CheckAndGenerateNewGrip(nWallEltY);
        }
    }

    private bool CheckIfWeNeedToAddAGrip()
    {
        List<Vector2> blockList = new List<Vector2>();

        for (int i = 0; i < m_waitingGripList.Count; i++)
        {
            if (!m_waitingGripList[i].bAlreadyGenerateNext)
            {
                return false;
            }

            if (blockList.Count == 0 || blockList[0].y == m_waitingGripList[i].nBlocY)
            {
                blockList.Add(new Vector2(m_waitingGripList[i].nBlocX, m_waitingGripList[i].nBlocY));
            }
            else if (blockList[0].y >= m_waitingGripList[i].nBlocY)
            {
                blockList.Clear();
                blockList.Add(new Vector2(m_waitingGripList[i].nBlocX, m_waitingGripList[i].nBlocY));
            }
        }


        bool bAdded = false;
        int nChildCount = blockList.Count;
        for (int nChildId = 0; nChildId < nChildCount; nChildId++)
        {
            GC_GreatClimbing.GreatClimbingDataSequence sequence = GetSequence((int)m_waitingGripList[nChildId].nBlocX, (int)m_waitingGripList[nChildId].nBlocY);
            // Vector2 vCurrentPos = new Vector2((m_waitingGripList[0].nBlocX + 0.5f) * m_fWallEltSize, (m_waitingGripList[0].nBlocY + 0.5f) * m_fWallEltSize);

            float fInsideX = 0.5f;
            float fInsideY = 0.5f;

            float fGripIdRnd = RrRndHandler.RndRange(0f, 1f);
            int nGripSize = Mathf.RoundToInt(sequence.gripSizeProbability.Evaluate(fGripIdRnd));
            float fGripLifeRnd = RrRndHandler.RndRange(0f, 1f);
            int nObstaclesLife = Mathf.RoundToInt(sequence.gripObstacleLifeProbability.Evaluate(fGripLifeRnd));

            GC_GreatClimbing.GripData gripData = new GC_GreatClimbing.GripData(nGripSize, fInsideX, fInsideY, nObstaclesLife, 0, (int)m_waitingGripList[nChildId].nBlocX, (int)m_waitingGripList[nChildId].nBlocY + 1);
            m_waitingGripList.Add(gripData);
            bAdded = true;
        }
        return bAdded;
    }

    private void GenerateFirstGrip()
    {
        int nX = 0;
        int nY = 1;
        GC_GreatClimbing.GreatClimbingDataSequence sequence = GetSequence(nX, nY);

        float fInsideX = RrRndHandler.RndRange(0f, 1f);
        float fInsideY = RrRndHandler.RndRange(0f, 1f);
        float fGripIdRnd = RrRndHandler.RndRange(0f, 1f);
        int nGripSize = Mathf.RoundToInt(sequence.gripSizeProbability.Evaluate(fGripIdRnd));
        float fGripLifeRnd = RrRndHandler.RndRange(0f, 1f);
        int nObstaclesLife = Mathf.RoundToInt(sequence.gripObstacleLifeProbability.Evaluate(fGripLifeRnd));

        m_waitingGripList.Add(new GC_GreatClimbing.GripData(nGripSize, fInsideX, fInsideY, nObstaclesLife, 0, nX, nY));
    }

    private List<GC_GreatClimbing.GripData> GetWallEltGrip(int nX, int nY, bool bRemove = true)
    {
        List<GC_GreatClimbing.GripData> list = new List<GC_GreatClimbing.GripData>();

        int nGripId = 0;
        while (nGripId < m_waitingGripList.Count)
        {
            if (m_waitingGripList[nGripId].nBlocX == nX && m_waitingGripList[nGripId].nBlocY == nY)
            {
                list.Add(m_waitingGripList[nGripId]);
                if (bRemove)
                {
                    m_waitingGripList.RemoveAt(nGripId);
                }
                else
                {
                    nGripId++;
                }
            }
            else
            {
                nGripId++;
            }
        }
        return list;
    }

    private int ComputeGripCountInNeighborg(int nX, int nY, int nOriginX, int nOriginY)
    {
        int nCount = 0;

        for (int nGripId = 0; nGripId < m_waitingGripList.Count; nGripId++)
        {
            if (m_waitingGripList[nGripId].nBlocY != nOriginY)
            {
                int nDeltaX = Mathf.Abs(m_waitingGripList[nGripId].nBlocX - nX);
                int nDeltaY = Mathf.Abs(m_waitingGripList[nGripId].nBlocY - nY);
                if (nDeltaX <= 1 && nDeltaY <= 1)
                {
                    nCount++;
                }
            }
        }

        return nCount;
    }


    private GC_GreatClimbing.GreatClimbingDataSequence GetSequence(int nX, int nY)
    {
        float fAltitude = m_computeAltFunc(nX, nY);
        int nSequenceId = 0;
        while (nSequenceId < m_gameData.sequencesArray.Length - 1 && fAltitude >= m_gameData.sequencesArray[nSequenceId + 1].altitudeStart)
        {
            nSequenceId++;
        }
        return m_gameData.sequencesArray[nSequenceId];
    }
}
