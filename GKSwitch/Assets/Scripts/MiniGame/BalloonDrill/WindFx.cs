using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindFx : MonoBehaviour
{
    private const int WIND_FX_COUNT = 10;

    [SerializeField]
    GameObject m_WindFxPrefab;

    private GameObject[] m_windFx;

    // Use this for initialization
    void Awake ()
    {
        m_windFx = new GameObject[WIND_FX_COUNT];
        for( int i=0; i< WIND_FX_COUNT; i++ )
        {
            m_windFx[i] = GameObject.Instantiate(m_WindFxPrefab, transform);
            m_windFx[i].SetActive(false);
        }
    }
	
    public void SetWind( Vector2 vSpeed )
    {
//        m_windAmb = FMODUnity.RuntimeManager.CreateInstance("event:/Balloon/Wind");
 //       m_windAmb.start();
       for ( int nWindFx=0; nWindFx< WIND_FX_COUNT; nWindFx++ )
        {
            StartCoroutine(PlayWindFx(m_windFx[nWindFx], vSpeed));
        }
    }

    public void StopWind()
    {
  /*      if (m_windAmb.isValid())
        {
            m_windAmb.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            m_windAmb.release();
        }*/

        StopAllCoroutines();
        for (int nWindFx = 0; nWindFx < WIND_FX_COUNT; nWindFx++)
        {
            m_windFx[nWindFx].SetActive(false);
        }
    }

    private IEnumerator PlayWindFx( GameObject windFx, Vector2 vSpeed )
    {
        // reorient gameObject
        SpriteRenderer img = windFx.GetComponent<SpriteRenderer>();
        float fAngle = lwTools.ComputeAngleFromVector(vSpeed.normalized);
        windFx.transform.rotation = Quaternion.Euler(0f, 0f, fAngle);

        while (true )
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f) );
            Vector3 vStartPos = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-800f, 800f), 100f);
            windFx.transform.position = vStartPos;
            windFx.SetActive(true);

            float fElapsedTime = 0;
            float fStartTime = Time.time;
            float fAnimTime = Random.Range(0f, 2f);
            while (fElapsedTime < fAnimTime)
            {
                // Play with alpha
                fElapsedTime = Time.time - fStartTime;
                float fAlpha = Mathf.Sin( ( fElapsedTime / fAnimTime ) * Mathf.PI );
                Color col = img.color;
                col.a = fAlpha;
                img.color = col;

                windFx.transform.position = vStartPos + (4*(Vector3)vSpeed * fElapsedTime);
                yield return null;
            }
            windFx.SetActive(false);
        }
    }

}
