using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SA_Grid
{
    [System.Serializable]
    public class rowData
    {
        public int[] row;
    }

    public int nRows;
    public int nCols;

    public rowData[] rows = new rowData[10];

}
