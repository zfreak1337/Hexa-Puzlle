using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Superpow;

public class Tile : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler {
    public const float EDGE_SIZE = 42;
    public const float WIDTH = 2f * EDGE_SIZE;
    public const float HEIGHT = WIDTH * 0.866f;

    public enum Type { Background, Normal, Stone };
    public Type type = Type.Normal;

    public bool isActive = true;
    public bool hasCover = false;
    public Vector2 position, boardPosition;

    public Piece piece;
    public Piece2 piece2;

    private void Start()
    {
        if (Utils.IsMakingLevel())
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Utils.IsMakingLevel() && type == Type.Background)
        {
            isActive = !isActive;
            SetActive(isActive);
        }
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        GetComponent<Image>().SetColorAlpha(isActive ? 1 : 0);
        transform.GetChild(0).GetComponent<Text>().SetColorAlpha(isActive ? 1 : 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (piece != null) piece.OnDrag(eventData);
        if (piece2 != null) piece2.OnDrag(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (piece != null) piece.OnPointerDown(eventData);
        if (piece2 != null) piece2.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (piece != null) piece.OnEndDrag(eventData);
        if (piece2 != null) piece2.OnEndDrag(eventData);
    }
}
