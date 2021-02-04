using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_MealElt : MonoBehaviour
{
    /*private System.Action<PointerEventData> m_onBeginDragDlg;
    private System.Action<PointerEventData> m_onDragDlg;
    private System.Action<PointerEventData> m_onEndDragDlg;

    public System.Action<PointerEventData> onDragDlg { set { m_onDragDlg = value; } }
    public System.Action<PointerEventData> onEndDragDlg { set { m_onEndDragDlg = value; } }
    public System.Action<PointerEventData> onBeginDragDlg { set { m_onBeginDragDlg = value; } }*/

    private Collider2D m_collider;

    public static BQ_MealElt sDraggedElt = null;
    public BQ_Meal m_meal { get; set; }

    public void EnableCollider()
    {
        if (m_collider != null)
        {
            m_collider.enabled = true;
        }
    }

    public void DisableCollider()
    {
        if (m_collider != null)
        {
            m_collider.enabled = false;
        }
    }


    private void Awake()
    {
        m_collider = gameObject.GetComponent<Collider2D>();
        float fAngle = Random.Range(-45f, 45f);
        transform.localRotation = Quaternion.Euler(0f, 0f, fAngle);
    }

    /*
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (sDraggedElt != null)
        {
            return;
        }
        sDraggedElt = this;
        if (m_onBeginDragDlg != null)
        {
            m_onBeginDragDlg(eventData);
        }
        //        Debug.Log("OnBeginDrag");
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (sDraggedElt != this)
        {
            return;
        }

        if (m_onDragDlg != null)
        {
            m_onDragDlg(eventData);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (sDraggedElt != this)
        {
            return;
        }
        sDraggedElt = null;
        if (m_onEndDragDlg != null)
        {
            m_onEndDragDlg(eventData);
        }
    }*/
}
