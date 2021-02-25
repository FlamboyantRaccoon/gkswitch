using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_Logic : MiniGameLogic
{

    public class ObstacleSpawnInfo
    {
        public SA_SnowArena.ObstacleType obstacleType;
        public float fX;
        public float fY;

        public ObstacleSpawnInfo(SA_SnowArena.ObstacleType _obstacleType, float _fX, float _fY)
        {
            fX = _fX;
            fY = _fY;
            obstacleType = _obstacleType;
        }
    }

    public class MobileObstacleSpawnInfo
    {
        public int[] path;
        public int nStartCell;
        public bool bReverse;
        public float fSpeed;

        public MobileObstacleSpawnInfo(int[] _path, int _nStartCell, bool _bReverse, float _fSpeed)
        {
            path = _path;
            nStartCell = _nStartCell;
            bReverse = _bReverse;
            fSpeed = _fSpeed;
        }
    }

    public class TargetSpawnInfo
    {
        public float fSpawnTime;
        public int nCellId;
        public bool bGold;

        public TargetSpawnInfo(float _fSpawnTime, int _nCellId, bool _bGold)
        {
            fSpawnTime = _fSpawnTime;
            nCellId = _nCellId;
            bGold = _bGold;
        }
    }

    public class ItemBotInfo
    {
        public float fSpawnTime;
        public float fTakeTime;

        public ItemBotInfo(float _fSpawnTime, float _fTakeTime)
        {
            fSpawnTime = _fSpawnTime;
            fTakeTime = _fTakeTime;
        }
    }

    private SA_SnowArena.snowData m_gameData;

    public List<ObstacleSpawnInfo> noMobileSpawns { get { return m_noMobileSpawns; } }
    public List<MobileObstacleSpawnInfo> mobileSpawns { get { return m_mobileSpawns; } }

    private List<ObstacleSpawnInfo> m_noMobileSpawns = new List<ObstacleSpawnInfo>();
    private List<MobileObstacleSpawnInfo> m_mobileSpawns = new List<MobileObstacleSpawnInfo>();
    private Queue<TargetSpawnInfo> m_targetSpawnQueue = new Queue<TargetSpawnInfo>();
    private uint m_nInitSeed;


    public void Init(SA_SnowArena.snowData gameData, byte nGameData )
    {
        m_fGameStartTime = Time.time;
        m_nGameDataId = nGameData;
        m_gameData = gameData;

        m_nInitSeed = (uint)(Random.Range(1, int.MaxValue));
        InitData(m_nInitSeed);
    }

    public Vector2[] ComputePathFromCells(int[] cells)
    {
        int nGridColumnCount = m_gameData.grid.nCols;
        int nGridRowCount = m_gameData.grid.nRows;
        float fColumnSize = SA_SnowArena.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = SA_SnowArena.s_gameArea.height / (float)nGridRowCount;

        Vector2[] path = new Vector2[cells.Length];
        for (int i = 0; i < path.Length; i++)
        {
            int nCellId = cells[i];
            int nX = nCellId % nGridColumnCount;
            int nY = (int)((nCellId) / nGridColumnCount);

            float fX = (nX + 0.5f) * fColumnSize + SA_SnowArena.s_gameArea.x;
            float fY = SA_SnowArena.s_gameArea.y - ((nY + 0.5f) * fRowSize);
            path[i] = new Vector2(fX, fY);
        }
        return path;
    }

    public TargetSpawnInfo CheckIfTargetToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_targetSpawnQueue.Count > 0)
        {
            TargetSpawnInfo spawn = m_targetSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_targetSpawnQueue.Dequeue();
                return spawn;
            }
        }

        return null;
    }

    private void InitData(uint nSeed)
    {
        RrRndHandler.RndSeed(nSeed);
        InitNoMobileObstacle();
        InitMobileObstacle();
        InitTargets();
    }

    private void InitNoMobileObstacle()
    {
        int nObstacleCount = Random.Range((int)m_gameData.nomobileObstacleRange.x, (int)m_gameData.nomobileObstacleRange.y + 1);
        if (nObstacleCount == 0)
        {
            return;
        }


        List<int> availableCells = GetGridCellsWithValue(1);
        int nGridColumnCount = m_gameData.grid.nCols;
        int nGridRowCount = m_gameData.grid.nRows;
        float fColumnSize = SA_SnowArena.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = SA_SnowArena.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(availableCells.Count));

        nObstacleCount = Mathf.Min(availableCells.Count, nObstacleCount);
        for (int i = 0; i < nObstacleCount; i++)
        {
            int nCellId = availableCells[(int)rndArray.ChooseValue(true)];
            int nX = nCellId % nGridColumnCount;
            int nY = (int)((nCellId) / nGridColumnCount);

            float fX = (nX + 0.5f) * fColumnSize + SA_SnowArena.s_gameArea.x;
            float fY = SA_SnowArena.s_gameArea.y - ((nY + 0.5f) * fRowSize);
            m_noMobileSpawns.Add(new ObstacleSpawnInfo(SA_SnowArena.ObstacleType.nomobile, fX, fY));
        }
    }

    private void InitMobileObstacle()
    {
        int nObstacleCount = Random.Range((int)m_gameData.mobileObstacleRange.x, (int)m_gameData.mobileObstacleRange.y + 1);
        if (nObstacleCount == 0)
        {
            return;
        }

        Dictionary<int, List<int>> mobileCells = ComputeMobileDictionnaryCells();

        //        int nGridColumnCount = m_gameData.grid.nCols;
        //        int nGridRowCount = m_gameData.grid.nRows;
        //float fColumnSize = SA_SnowArena.s_gameArea.width / (float)nGridColumnCount;
        //float fRowSize = SA_SnowArena.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(mobileCells.Count));

        nObstacleCount = Mathf.Min(mobileCells.Count, nObstacleCount);
        List<int> keyList = new List<int>(mobileCells.Keys);

        for (int i = 0; i < nObstacleCount; i++)
        {
            int nPathId = keyList[(int)rndArray.ChooseValue(true)];
            int[] path = ComputePath(mobileCells[nPathId]);
            int nStartCell = RrRndHandler.RndRange(0, path.Length);
            bool bSide = RrRndHandler.RndRange(0, 2) == 0;
            float fSpeed = RrRndHandler.RndRange(m_gameData.mobileSpeed.x, m_gameData.mobileSpeed.y);
            m_mobileSpawns.Add(new MobileObstacleSpawnInfo(path, nStartCell, bSide, fSpeed));
        }
    }

    private void InitTargets()
    {
        float fGameTime = 0;
        float fEndTime = m_gameData.gameTime + 10f;

        int nSequenceId = 0;

        List<int> availableCells = GetGridCellsWithValue(0);
        lwRndArray rndArray = new lwRndArray((uint)(availableCells.Count));

        while (fGameTime < fEndTime)
        {
            SA_SnowArena.SnowArenaDataSequence sequence = m_gameData.sequencesArray[nSequenceId];
            float fSequenceEndTime = nSequenceId < m_gameData.sequencesArray.Length - 1 ? m_gameData.sequencesArray[nSequenceId + 1].startTime : fEndTime;
            float fSpawnTime = (1f / (float)(sequence.itemsBySecond));

            while (fGameTime < fSequenceEndTime)
            {
                int nCellId = availableCells[(int)rndArray.ChooseValue(true)];
                bool bGold = Random.Range(0f, 1f) < sequence.goldTargetChance;

                m_targetSpawnQueue.Enqueue(new TargetSpawnInfo(fGameTime, nCellId, bGold));

                fGameTime += fSpawnTime;
            }
            nSequenceId++;

        }
    }

    public List<int> GetGridCellsWithValue(int nValue)
    {
        List<int> cells = new List<int>();
        SA_Grid grid = m_gameData.grid;
        for (int i = 0; i < grid.nRows; i++)
        {
            for (int j = 0; j < grid.nCols; j++)
            {
                if (grid.rows[i].row[j] == nValue)
                {
                    cells.Add(i * grid.nCols + j);
                }
            }
        }
        return cells;
    }

    private Dictionary<int, List<int>> ComputeMobileDictionnaryCells()
    {
        Dictionary<int, List<int>> mobileCells = new Dictionary<int, List<int>>();
        SA_Grid grid = m_gameData.grid;
        for (int i = 0; i < grid.nRows; i++)
        {
            for (int j = 0; j < grid.nCols; j++)
            {
                if (grid.rows[i].row[j] > 1)
                {
                    int nValue = grid.rows[i].row[j];
                    int nCellId = i * grid.nCols + j;
                    List<int> list = null;
                    if (mobileCells.TryGetValue(nValue, out list))
                    {
                        list.Add(nCellId);
                    }
                    else
                    {
                        list = new List<int>();
                        list.Add(nCellId);
                        mobileCells.Add(nValue, list);
                    }
                }
            }
        }
        return mobileCells;
    }

    private int[] ComputePath(List<int> cells)
    {
        List<int> path = new List<int>();
        List<int>[] nNeighbourg = new List<int>[cells.Count];
        int nGridColumnCount = m_gameData.grid.nCols;

        int nMinNeighbourg = -1;
        int nMinId = -1;
        for (int i = 0; i < cells.Count; i++)
        {
            nNeighbourg[i] = new List<int>();

            for (int j = 0; j < cells.Count; j++)
            {
                if (i != j)
                {
                    if (cells[i] % nGridColumnCount != 0 && cells[j] == cells[i] - 1)
                    {
                        nNeighbourg[i].Add(j);
                    }

                    if (cells[i] % nGridColumnCount != nGridColumnCount - 1 && cells[j] == cells[i] + 1)
                    {
                        nNeighbourg[i].Add(j);
                    }

                    if (cells[j] == cells[i] + nGridColumnCount || cells[j] == cells[i] - nGridColumnCount)
                    {
                        nNeighbourg[i].Add(j);
                    }
                }
            }

            if (nMinNeighbourg == -1 || nMinNeighbourg > nNeighbourg[i].Count)
            {
                nMinNeighbourg = nNeighbourg[i].Count;
                nMinId = i;
            }
        }

        // here I will have all the neighbourg
        path.Add(cells[nMinId]);
        bool bContinue = true;
        int nFirsId = nMinId;
        List<int> available = nNeighbourg[nMinId];

        int nPrevious = nMinId;
        while (bContinue)
        {
            int nNext = available[RrRndHandler.RndRange(0, available.Count)];
            available = nNeighbourg[nNext];
            available.Remove(nPrevious);
            path.Add(cells[nNext]);
            nPrevious = nNext;

            bContinue = available.Count > 0 && nPrevious != nFirsId;
        }

        return path.ToArray();
    }

    public List<ItemBotInfo> ComputeItemBotInfo(Vector2 timeToHitTarget)
    {
        TargetSpawnInfo[] spawnInfos = new TargetSpawnInfo[m_targetSpawnQueue.Count];
        m_targetSpawnQueue.CopyTo(spawnInfos, 0);

        List<ItemBotInfo> botInfos = new List<ItemBotInfo>();
        List<ItemBotInfo> currentTargets = new List<ItemBotInfo>();
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            // check how many candy
            float fSpawnTime = spawnInfos[i].fSpawnTime;
            for (int j = currentTargets.Count - 1; j >= 0; j--)
            {
                if (fSpawnTime > currentTargets[j].fTakeTime)
                {
                    currentTargets.RemoveAt(j);
                }
            }

            if (currentTargets.Count < m_gameData.nTargetMax)
            {
                float fTakeTime = spawnInfos[i].fSpawnTime + Random.Range(timeToHitTarget.x, timeToHitTarget.y);
                ItemBotInfo item = new ItemBotInfo(spawnInfos[i].fSpawnTime, fTakeTime);
                botInfos.Add(item);
                currentTargets.Add(item);
            }

        }
        return botInfos;
    }
}
