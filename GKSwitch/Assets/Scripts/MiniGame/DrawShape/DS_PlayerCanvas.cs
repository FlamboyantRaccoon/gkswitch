using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_PlayerCanvas : MonoBehaviour
{
    public delegate void AddScoreDlg(bool bGood, int playerId, Vector3 vPos);

    [SerializeField]
    private SplineController m_splineController;
    [SerializeField]
    private MeshFilter m_meshFilter;
    [SerializeField]
    private MeshFilter m_playerMeshFilter;
    [SerializeField]
    public Transform m_modelPlaneRect;
    [SerializeField]
    private Animator m_playerNeedle;
    [SerializeField]
    private Camera m_camera;

    public AddScoreDlg m_addScore;

    private int m_playerId;
    private lwLinesMesh m_modelShape;
    private lwLinesMesh m_playerShape;
    private Vector3 m_vLastInputPos;
    DS_DrawShape.ModelShowState m_modelShowState = DS_DrawShape.ModelShowState.visible;
    private int m_nPlayerInMeshPoint;
    private float m_fNeedleLastAnimTime = -1f;
    private float m_fAreaRangeSqrt;
    private float m_fStartDrawTimer = -1f;
    private bool m_bIsDrawStart = false;
    private Coroutine m_modelFadeOutRoutine;

    private Vector3 m_vAreaCenter = new Vector3(0f, 857f, 10f);
    private DS_DrawShape m_parentDrawShape;

    private string m_soundName;

    public void Init( int playerId, float fDrawingAreaRange, DS_DrawShape drawShape )
    {
        m_parentDrawShape = drawShape;
        m_playerId = playerId;
        m_vAreaCenter = m_modelPlaneRect.position;
        m_fAreaRangeSqrt = (fDrawingAreaRange / 2f) * (fDrawingAreaRange / 2f);
        m_playerNeedle.gameObject.SetActive(false);
        InitPlayerShape();
        m_soundName = "needle" + playerId;
        SoundManager.instance.PlayPersistentSound(m_soundName, m_parentDrawShape.m_sPickingSound);
        SoundManager.instance.PausePersistentSound(m_soundName);
    }

    public void SetCameraRegion( int playerCount)
    {
        if( playerCount==1 )
        {
            return;
        }
        float fStartX = (m_playerId % 2) * 0.5f;
        float fWidth = 0.5f;
        float fStartY = 0;
        float fHeight = 1f;

        if ( playerCount>2)
        {
            fHeight = 0.5f;
            fStartY = (1-((int)(m_playerId / 2))) * 0.5f;
        }
        m_camera.rect = new Rect(fStartX, fStartY, fWidth, fHeight);
    }

    public void Clean()
    {
        SoundManager.instance.StopPersistentSound(m_soundName);
        CleanAllMeshAndSplineStuff();
    }

    public void CleanAllMeshAndSplineStuff()
    {
        m_splineController.Reset();
        ClearSplineNode();

        GameObject.Destroy(m_playerMeshFilter.mesh);
        GameObject.Destroy(m_meshFilter.mesh);

        m_playerShape.ClearLines();
        m_modelShape.ClearLines();
    }

    public void StopPlaying()
    {
        SetEndNeedleAnim(true);
    }

    public void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if( !m_parentDrawShape.isPlaying)
        {
            return;
        }

        if (buttonPhase == RRPlayerInput.ButtonPhase.on || buttonPhase == RRPlayerInput.ButtonPhase.press)
        {
            Vector3 vInputPos = m_camera.ViewportToWorldPoint(new Vector3(v.x, v.y, 0)); //Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            vInputPos.z += 10f;

            // Check if not out of range position
            Vector3 vRadius = vInputPos - m_vAreaCenter;
            vRadius.z = 0;

            if (vRadius.sqrMagnitude > m_fAreaRangeSqrt)
            {
                SetEndNeedleAnim(true);
                return;
            }

            if (buttonPhase == RRPlayerInput.ButtonPhase.press || (m_vLastInputPos.y == 0f && m_vLastInputPos.x == 0f))
            {
                if (m_modelShowState == DS_DrawShape.ModelShowState.visible && m_parentDrawShape.m_bFadeModel)
                {
                    m_modelFadeOutRoutine = StartCoroutine(PlayModelFadeOut());
                }
                m_vLastInputPos = vInputPos;
                CheckDrawPoint(vInputPos);
                m_nPlayerInMeshPoint = 0;
                m_playerNeedle.gameObject.SetActive(true);
                m_playerNeedle.transform.position = m_vLastInputPos;
                m_fNeedleLastAnimTime = Time.time;
                SoundManager.instance.ResumePersistentSound(m_soundName);
                if (!m_bIsDrawStart)
                {
                    m_fStartDrawTimer = Time.realtimeSinceStartup;
                    m_bIsDrawStart = true;
                }
            }
            else
            {
                Vector3 vDir = (vInputPos - m_vLastInputPos);
                float fMagnitude = vDir.magnitude;
                if (fMagnitude > m_parentDrawShape.m_fMeshHeight)
                {
                    if (m_fNeedleLastAnimTime == -1f)
                    {
                        m_playerNeedle.SetBool("Move", true);
                        SoundManager.instance.ResumePersistentSound(m_soundName);
                        //m_pickingSound.start();
                    }
                    m_fNeedleLastAnimTime = Time.time;
                    vDir.Normalize();
                    int nMeshCount = (int)(fMagnitude / m_parentDrawShape.m_fMeshHeight);
                    float fMeshSize = fMagnitude / nMeshCount;
                    Vector3 vStart = m_vLastInputPos;

                    Vector3 vLocalStart = vStart - transform.position;
                    Vector3 vEnd = vLocalStart;

                    for (int i = 0; i < nMeshCount; i++)
                    {
                        vEnd = vLocalStart + vDir * fMeshSize;
                        CheckDrawPoint(vEnd + transform.position);


                        float fUvXStart = m_nPlayerInMeshPoint * m_parentDrawShape.m_fMaterialUvoffset;
                        float fUvXEnd = (m_nPlayerInMeshPoint + 1) * m_parentDrawShape.m_fMaterialUvoffset;
                        Vector2[] uv = new Vector2[4];
                        uv[0] = new Vector2(fUvXStart, 0f);
                        uv[1] = new Vector2(fUvXStart, 1f);
                        uv[2] = new Vector2(fUvXEnd, 0f);
                        uv[3] = new Vector2(fUvXEnd, 1f);

                        //Debug.Log("uv : " + uv[0] + " / " + uv[1] + " / " + uv[2] + " / " + uv[3]);

                        m_playerShape.AddLine(vLocalStart, vEnd, Color.white, m_parentDrawShape.gameData.fShapeWidth, uv, null, null, m_nPlayerInMeshPoint > 0);
                        vLocalStart = vEnd;
                        m_nPlayerInMeshPoint++;
                        m_modelShape.UseLastPoints = true;
                    }
                    m_playerShape.ApplyLines();
                    m_vLastInputPos = vLocalStart + transform.position;
                    m_playerNeedle.transform.position = m_vLastInputPos;
                }
                else
                {
                    CheckEndNeedleAnim();
                }
            }
        }
        else if (buttonPhase == RRPlayerInput.ButtonPhase.release)
        {
            SetEndNeedleAnim(true);
            //m_playerNeedle.SetBool("Move", false);
        }
    }

    private void CheckEndNeedleAnim(bool bHide = false)
    {
        if (m_fNeedleLastAnimTime != -1f && Time.time - m_fNeedleLastAnimTime > m_parentDrawShape.m_fNeedleLatencyAnimTime)
        {
            SetEndNeedleAnim(bHide);
        }
    }

    private void SetEndNeedleAnim(bool bHide)
    {
        m_fNeedleLastAnimTime = -1f;
        m_playerNeedle.SetBool("Move", false);

        if (bHide)
        {
            m_playerNeedle.gameObject.SetActive(false);
        }

        SoundManager.instance.PausePersistentSound(m_soundName);
    }

    private void CheckDrawPoint(Vector3 vPos)
    {
        //vPos -= transform.position;
        bool bGood = false;
        bool bValid = true;
        vPos.z -= 100f;
        Ray ray = new Ray(vPos, Vector3.forward);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.name == "ModelMesh")
            {
                bGood = true;
            }
            if (hits[i].transform.name == "PlayerMesh")
            {
                bGood = true;
                bValid = false;
            }
        }

        //m_pickingSound.setParameterByName("bGood", bGood ? 0.75f : 0.25f);
        if (bValid)
        {
            //m_drawShapeStats[bGood ? 0 : 1]++;
            m_addScore?.Invoke(bGood, m_playerId, m_vLastInputPos-transform.position);
        }
    }

    #region shapeGeneration
    public void OnNextShape()
    {
        //Debug.Log("OnNextShape");
        if (m_modelFadeOutRoutine != null)
        {
            StopCoroutine(m_modelFadeOutRoutine);
        }
        CleanAllMeshAndSplineStuff();
    }

    private void ClearSplineNode()
    {
        // clear children
        int nChildCount = m_splineController.transform.childCount;
        for (int i = nChildCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(m_splineController.transform.GetChild(i).gameObject);
        }
    }

    private void GenerateSplineNode(Vector2[] vPointsArray)
    {
        // clear children
        /*    int nChildCount = m_splineController.transform.childCount;
            for( int i=nChildCount-1; i>=0; i-- )
            {
                GameObject.Destroy(m_splineController.transform.GetChild(i).gameObject);
            }*/

        int nNodeCount = vPointsArray.Length;
        for (int nNodeId = 0; nNodeId < nNodeCount; nNodeId++)
        {
            GameObject node = new GameObject("node");
            node.transform.parent = m_splineController.transform;
            node.transform.localPosition = new Vector3(vPointsArray[nNodeId].x, vPointsArray[nNodeId].y, m_modelPlaneRect.position.z);
        }
    }

    public void GenerateModelShape(Vector2[] vPointsArray)
    {
        Debug.Log("GenerateModelShape");
        GenerateSplineNode(vPointsArray);
        m_modelShape = new lwLinesMesh();
        m_modelShape.loop = true;
        m_modelShape.UseContinuousLines = true;
        m_modelShape.UseLastPoints = true;
        m_modelShape.BorderSize = 0f;
        m_splineController.Init();
        m_modelShape.ClearLines();
        int nEdgeCount = 100;
        Vector2[] vVertexPos = new Vector2[nEdgeCount];
        float fTimeInter = 1f / ((float)nEdgeCount + 1);
        for (int nVertexId = 0; nVertexId < nEdgeCount; nVertexId++)
        {
            float fTime = (float)nVertexId * fTimeInter;
            vVertexPos[nVertexId] = m_splineController.GetPositionAtTime(fTime);
            /*            Debug.Log("vVertexPos ("+fTime+"): " + vVertexPos[nVertexId]);
                        GameObject obj = new GameObject("node_" + nVertexId);
                        obj.transform.position = vVertexPos[nVertexId];*/
        }

        float fSize = m_parentDrawShape.gameData.fShapeWidth;

        for (int nSegmentId = 0; nSegmentId < nEdgeCount; nSegmentId++)
        {
            int nNextId = (nSegmentId + 1) % vVertexPos.Length;
            Vector3 vStart = vVertexPos[nSegmentId];
            Vector3 vEnd = vVertexPos[nNextId];

            m_modelShape.AddLine(vStart, vEnd, Color.white, fSize, false);

        }
        m_modelShape.ApplyLines();

        MeshCollider collider = m_meshFilter.GetComponent<MeshCollider>();
        if (collider != null)
        {
            m_meshFilter.mesh = null;
            m_meshFilter.mesh = m_modelShape.mesh;

            collider.sharedMesh = null;
            collider.sharedMesh = m_modelShape.mesh;
        }
        else
        {
            m_meshFilter.mesh = m_modelShape.mesh;
            m_meshFilter.gameObject.AddComponent<MeshCollider>();
        }

        m_modelShowState = DS_DrawShape.ModelShowState.visible;
        m_fStartDrawTimer = -1f;
        m_bIsDrawStart = false;
    }

    private void InitPlayerShape()
    {
        m_playerShape = new lwLinesMesh();
        m_playerShape.loop = false;
        m_playerShape.UseLastPoints = true;
        m_playerShape.UseContinuousLines = false;
        m_playerShape.BorderSize = 0f;
        m_playerShape.ClearLines();

        m_playerShape.onUpdateMesh = OnPlayerUpdateMesh;
        m_playerMeshFilter.mesh = m_playerShape.mesh;

        MeshRenderer render = m_playerMeshFilter.gameObject.GetComponent<MeshRenderer>();
        Material mat = render.material;
        mat.color = GameContext.instance.m_settings.playerSettings[m_playerId].color;
    }


    private void OnPlayerUpdateMesh(Mesh mesh)
    {
        m_playerMeshFilter.mesh = mesh;
        MeshCollider collider = m_playerMeshFilter.GetComponent<MeshCollider>();
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }

    private IEnumerator PlayModelFadeOut()
    {
        m_modelShowState = DS_DrawShape.ModelShowState.fade;
        float fElapsedTime = 0;
        float fStartTime = Time.time;
        float fAnimTime = m_parentDrawShape.gameData.fModelFadeOutTime;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            float fCoeff = Mathf.Min(fElapsedTime / fAnimTime, 1f);
            Color col = new Color(1f, 1f, 1f, 1f - fCoeff);
            m_modelShape.ChangeLinesColor(col);
            if (fElapsedTime < fAnimTime)
            {
                yield return null;
            }
        }
        m_modelShowState = DS_DrawShape.ModelShowState.hide;
    }

    #endregion

}
