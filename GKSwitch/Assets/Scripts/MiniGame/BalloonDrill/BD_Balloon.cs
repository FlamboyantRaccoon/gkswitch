using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BD_Balloon : MonoBehaviour 
{
    [SerializeField]
    public Sprite[] m_spritesArray;
    [SerializeField]
    public Color[] m_colorArray;
    [SerializeField]
    SpriteRenderer m_balloonSprite;
    [SerializeField]
    Animator m_animator;
    [SerializeField]
    Collider2D m_collider;
    [SerializeField]
    ParticlePlayground.PlaygroundParticlesC m_explosionFlatFx;
    [SerializeField]
    SpriteRenderer[] m_explosionSprites;

    public int colorId { get { return m_nColorId; } }

    private int m_nColorId;
    System.Func<float> m_getSpeedMul;
    System.Func<Vector2> m_getMoveModificator;
    System.Action<BD_Balloon> m_deleteAction;
    System.Func<int,int,Vector3,bool> m_isGoodTouch;
    System.Func<bool> m_canDestroyBalloon;
    private float m_fSpeed;
    private Vector2 m_vDir;

    public void Setup(int nColorId, Vector2 vDir, float fSpeed, 
        System.Func<float> getSpeedMul, 
        System.Func<Vector2> getMoveModificator, 
        System.Action<BD_Balloon> deleteAction,
        System.Func<bool> canDestroyBalloon,
        System.Func<int,int,Vector3,bool> isGoodTouch)
    {
        SetColorId(nColorId);
        m_getSpeedMul = getSpeedMul;
        m_getMoveModificator = getMoveModificator;
        m_canDestroyBalloon = canDestroyBalloon;
        m_isGoodTouch = isGoodTouch;
        m_fSpeed = fSpeed;
        m_vDir = vDir;
        m_deleteAction = deleteAction;
        m_animator.Play("Balloon_on");
        m_collider.enabled = true;
    }

    public void Update()
    {
        float fSpeed = (m_getSpeedMul != null ? m_getSpeedMul() : 1f) * m_fSpeed;
        Vector3 vMove = m_vDir * Time.deltaTime * fSpeed;
        transform.Translate(vMove);

        if( m_getMoveModificator!=null )
        {
            Vector2 vEffect = m_getMoveModificator() * Time.deltaTime;
            transform.Translate(vEffect);
        }

        if( transform.position.y > 2000f )
        {
            DeleteBalloon();
        }
    }

    public void OnEndExplodeAnim()
    {
        DeleteBalloon();
    }

    public void OnShoot( int playerId)
    {
        //Debug.Log("pouett on meee " + m_nColorId );
        if(m_canDestroyBalloon())
        {
            m_collider.enabled = false;
            bool bGood = m_isGoodTouch != null ? m_isGoodTouch(m_nColorId, playerId, transform.position) : false;
            if( bGood )
            {
                m_animator.SetTrigger("Explosion");
            }
            else
            {
                GradientColorKey[] keys = m_explosionFlatFx.lifetimeColor.colorKeys;
                Color color = m_nColorId < m_colorArray.Length ? m_colorArray[m_nColorId] : Color.white;
                for( int nKeyId = 0; nKeyId<keys.Length; nKeyId++ )
                {
                    keys[nKeyId].color = color;
                }
                m_explosionFlatFx.lifetimeColor.SetKeys(keys, m_explosionFlatFx.lifetimeColor.alphaKeys);

                for( int nExlposionSpriteId=0; nExlposionSpriteId<m_explosionSprites.Length; nExlposionSpriteId++ )
                {
                    m_explosionSprites[nExlposionSpriteId].color = color;
                }
                m_animator.SetTrigger("Explosion_wrong");
            }
        }
    }

    private void SetColorId( int nColorId)
    {
        m_nColorId = nColorId;
        if (m_spritesArray != null && m_nColorId >=0 && m_nColorId < m_spritesArray.Length && m_spritesArray[m_nColorId] != null)
        {
            m_balloonSprite.sprite = m_spritesArray[m_nColorId];
        }
    }

    private void DeleteBalloon()
    {
        if( m_deleteAction!=null )
        {
            m_deleteAction(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

}
