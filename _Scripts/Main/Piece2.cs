using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Piece2 : Piece {

    private void Start()
    {
        foreach(Tile tile in tiles)
        {
            tile.GetComponent<Image>().sprite = MonoUtils.instance.tileSprites[id % MonoUtils.instance.tileSprites.Length];
        }
    }

    public new void OnDrag(PointerEventData eventData)
    {
        if (status != Status.Dragging) return;

        Vector3 moveDelta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - beginTouchPosition;
        transform.position = beginPosition  + moveDelta;

        TileRegion2.instance.CheckMatch(this);
    }

    public new void OnPointerDown(PointerEventData eventData)
    {
        beginPosition = transform.position;
        beginTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.SetParent(MonoUtils.instance.dragRegion);

        status = Status.Dragging;

        matches.Clear();
    }

    public new void OnEndDrag(PointerEventData eventData)
    {
        if (matches.Count == tiles.Count)
        {
            iTween.MoveTo(gameObject, iTween.Hash("position", matches[0].transform.position, "speed", 10f, "oncomplete", "CompleteMoveToBoard"));
        }
        else
        {
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * Const.SCALED_TILE, "speed", 4f));
            iTween.MoveTo(gameObject, iTween.Hash("position", beginPosition, "speed", 15f, "oncomplete", "CompleteMoveToBottom"));
        }

        status = Status.OnTween;
    }

    private void CompleteMoveToBoard()
    {
        status = Status.OnBottom;
        transform.SetParent(MonoUtils.instance.pieceRegion);
        if (matches.Count != 0) boardPositions.Clear();
        foreach (var tile in matches)
        {
            boardPositions.Add(tile.position);
        }

        MakeLevelController.instance.AdjustPieces();
    }

    private void CompleteMoveToBottom()
    {
        status = Status.OnBottom;
        transform.SetParent(MonoUtils.instance.pieceRegion);
    }
}
