using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Superpow;

public class TileRegion2 : MonoBehaviour {

    private Dictionary<Vector2, Tile> slots = new Dictionary<Vector2, Tile>();
    private Dictionary<Vector2, Tile> bottomSlots = new Dictionary<Vector2, Tile>();


    public List<Piece> pieces = new List<Piece>();

    public static TileRegion2 instance;
    private GameLevel gameLevel;

    private void Awake()
    {
        instance = this;
    }

    public void LoadBoardBackground()
    {
        Transform root = MonoUtils.instance.backgroundTilesTransform;
        for(int i = root.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(root.GetChild(i).gameObject);
        }
        
        slots.Clear();

        gameLevel = MakeLevelController.instance.gameLevel;
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        if (string.IsNullOrEmpty(gameLevel.positions))
        {
            for (int col = 0; col < MakeLevelController.instance.numCol; col++)
            {
                for (int row = 0; row < MakeLevelController.instance.numRow; row++)
                {
                    Tile tile = Instantiate(MonoUtils.instance.tile_background);
                    tile.transform.SetParent(MonoUtils.instance.backgroundTilesTransform);
                    tile.transform.localScale = Vector3.one;
                    Vector3 position = GetLocalPosition(col, row);
                    tile.transform.localPosition = position;
                    tile.position = new Vector2(col, row);
                    tile.transform.GetChild(0).GetComponent<Text>().text = col + "," + row;
                    tile.type = Tile.Type.Background;

                    if (position.x < minX) minX = position.x;
                    if (position.x > maxX) maxX = position.x;
                    if (position.y < minY) minY = position.y;
                    if (position.y > maxY) maxY = position.y;
                }
            }
        }

        else
        {
            List<string> positions = CUtils.BuildListFromString<string>(gameLevel.positions);
            foreach (var value in positions)
            {
                string[] values = value.Split(',');
                int col = int.Parse(values[0]);
                int row = int.Parse(values[1]);
                
				Tile tile = Instantiate(values.Length == 2? MonoUtils.instance.tile_background : MonoUtils.instance.tile_stone);
                tile.transform.SetParent(MonoUtils.instance.backgroundTilesTransform);
                tile.transform.localScale = Vector3.one;
                Vector3 position = GetLocalPosition(col, row);
                tile.transform.localPosition = position;
                tile.position = new Vector2(col, row);
                //tile.type = Tile.Type.Background;
                
				if (values.Length == 2)
				{
					tile.transform.GetChild(0).GetComponent<Text>().text = col + "," + row;
					slots.Add(tile.position, tile);
				}

                if (position.x < minX) minX = position.x;
                if (position.x > maxX) maxX = position.x;
                if (position.y < minY) minY = position.y;
                if (position.y > maxY) maxY = position.y;
                
            }
        }

        float regionWidth = maxX - minX + Tile.WIDTH + (minX - Tile.WIDTH / 2) * 2; ;
        float regionHeight = maxY - minY + Tile.HEIGHT + (minY - Tile.HEIGHT / 2) * 2;

        transform.localPosition = new Vector3(0, 163) - new Vector3(regionWidth / 2, regionHeight / 2);
        GetComponent<RectTransform>().sizeDelta = new Vector2(regionWidth, regionHeight);
    }

    public void LoadBottomBackground()
    {
        for (int col = 0; col < 17; col++)
        {
            for (int row = 0; row < 7; row++)
            {
                Tile tile = Instantiate(MonoUtils.instance.tile_background);
                tile.transform.SetParent(MonoUtils.instance.bottomRegion);
                tile.transform.localScale = Vector3.one * Const.SCALED_TILE;
                Vector3 position = GetLocalPosition(col, row);
                tile.transform.localPosition = position * Const.SCALED_TILE;
                tile.position = new Vector2(col, row);
                tile.transform.GetComponent<Image>().SetColorAlpha(0.4f);
                tile.type = Tile.Type.Background;
                bottomSlots.Add(tile.position, tile);
            }
        }
    }

    public void ClearPieces()
    {
        pieces.Clear();
        foreach (Transform child in MonoUtils.instance.pieceRegion)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadPieces(GameLevel gameLevel)
    {
        ClearPieces();

        List<string> data = CUtils.BuildListFromString<string>(gameLevel.pieces);
        int id = 0;
        foreach(var oneData in data)
        {
            List<string> positions = CUtils.BuildListFromString<string>(oneData, '-');
            Vector2 bottomPosition = Vector2.zero;

            List<Vector2> defaultPositions = new List<Vector2>();

            bool isExtra = positions[positions.Count - 1] == "r";

            if (isExtra)
                positions.RemoveAt(positions.Count - 1);

            for (int i = 0; i < positions.Count; i++)
            {
                string[] values = positions[i].Split(',');
                int col = int.Parse(values[0]);
                int row = int.Parse(values[1]);
                Vector2 position = new Vector2(col, row);

                if (i ==  positions.Count - 1)
                {
                    bottomPosition = position;
                }

                if (i != positions.Count - 1)
                {
                    defaultPositions.Add(position);
                }
            }

            
            float scaleFactor = Const.SCALED_TILE;
            Transform parent = MonoUtils.instance.pieceRegion;

            Piece piece = CreatePiece(defaultPositions, parent);
            piece.boardPositions = GetMatchPositions(piece, bottomPosition);
            piece.isExtra = isExtra;
            pieces.Add(piece);

            piece.id = id++;
            piece.bottomPosition = bottomPosition;

            piece.transform.localScale = Vector3.one * scaleFactor;
            piece.transform.localPosition = GetLocalPosition(bottomPosition) * Const.SCALED_TILE;
        }
    }

    private Piece CreatePiece(List<Vector2> positions, Transform parent)
    {
        Piece2 piece = Instantiate(MonoUtils.instance.piece2Prefab);
        piece.center = positions[0];
        piece.transform.SetParent(parent);
        piece.defaultPositions.AddRange(positions);

        Vector3 translateVector = GetLocalPosition(piece.center);

        foreach (var position in piece.defaultPositions)
        {
            int col = (int)position.x;
            int row = (int)position.y;

            Tile tile = Instantiate(MonoUtils.instance.tile_normal);
            tile.transform.SetParent(piece.transform);
            tile.transform.localScale = Vector3.one;
            Vector3 localPosition = GetLocalPosition(col, row);
            tile.transform.localPosition = localPosition - translateVector;
            tile.position = new Vector2(col, row);
            tile.piece2 = piece;
            tile.type = Tile.Type.Normal;

            piece.tiles.Add(tile);
        }

        foreach (var pos in piece.defaultPositions)
        {
            piece.tilePositions.Add(pos - positions[0]);
        }

        return piece;
    }

    private Vector3 GetLocalPosition(int col, int row)
    {
        return col % 2 == 0 ? new Vector3((col * 1.5f + 1) * Tile.EDGE_SIZE, (row + 0.5f) * Tile.HEIGHT) :
                        new Vector3((col * 1.5f + 1) * Tile.EDGE_SIZE, (row + 1) * Tile.HEIGHT);
    }

    private Vector3 GetLocalPosition(Vector2 position)
    {
        return GetLocalPosition((int)position.x, (int)position.y);
    }

    public bool CheckMatch(Piece2 piece)
    {
        float minDistance = float.MaxValue;
        Tile matchSlot = null;

        foreach(Tile slot in bottomSlots.Values)
        {
            if (slot.hasCover) continue;
            float distance = Vector3.Distance(piece.tileCenter.transform.position, slot.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                matchSlot = slot;
            }
        }

        piece.matches = new List<Tile>();

        if (minDistance < 0.2f)
        {
            bool isMatch = true;

            List<Vector2> matchPositions = GetMatchPositions(piece, matchSlot.position);

            foreach(var matchPos in matchPositions)
            {
                if (!bottomSlots.ContainsKey(matchPos) || bottomSlots[matchPos].hasCover)
                {
                    isMatch = false;
                    break;
                }
                else
                {
                    piece.matches.Add(bottomSlots[matchPos]);
                }
            }
            
            return isMatch;
        }
        return false;
    }

    public void ClearCovers(Piece piece)
    {
        foreach(var pos in piece.boardPositions)
        {
            slots[pos].hasCover = false;
        }
    }

    private List<Vector2> GetMatchPositions(Piece piece, Vector2 centerPosition)
    {
        List<Vector2> matchPositions = new List<Vector2>();
        int deltaX = (int)(centerPosition.x - piece.tileCenter.position.x);
        if (deltaX % 2 == 0)
        {
            matchPositions.AddRange(piece.tilePositions);
        }
        else
        {
            int modifier = centerPosition.x % 2 == 0 ? -1 : 1;
            foreach (var pos in piece.tilePositions)
            {
                float newY = pos.y;
                if (((int)pos.x) % 2 == 1)
                {
                    newY += modifier;
                }
                matchPositions.Add(new Vector2(pos.x, newY));
            }
        }

        for (int i = 0; i < matchPositions.Count; i++) matchPositions[i] += centerPosition;

        return matchPositions;
    }
}
