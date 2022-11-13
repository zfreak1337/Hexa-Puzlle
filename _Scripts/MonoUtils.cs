using UnityEngine;
using System.Collections;

public class MonoUtils : MonoBehaviour {
    public Tile tile_background, tile_background2, tile_background_bottom, tile_shadow, tile_stone;
    public Tile tile_normal;
    public Piece piecePrefab;
    public Piece2 piece2Prefab;
    public Transform dragRegion, bottomRegion, pieceRegion;
    public Sprite[] tileSprites;
    public Transform hintPiecesTransform, backgroundTilesTransform, piecesTransform, piecesBottomTransform,
        bottomRegionBGTransform, highlightsTransform;

    public static MonoUtils instance;

    private void Awake()
    {
        instance = this;
    }
}
