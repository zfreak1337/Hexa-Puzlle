using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;
using Superpow;

public class TileRegion : MonoBehaviour
{
    public Vector2 size;

    private Dictionary<Vector2, Tile> slots = new Dictionary<Vector2, Tile>();
    public List<Piece> pieces = new List<Piece>();
    private List<Piece> hintPieces = new List<Piece>();

    public static TileRegion instance;
    private LevelPrefs levelPrefs;

    private void Awake()
    {
        instance = this;
    }

    private void LoadBoardBackground()
    {
        for (int col = 0; col < size.y; col++)
        {
            for (int row = 0; row < size.x; row++)
            {
                Tile tile = Instantiate(MonoUtils.instance.tile_background);
                tile.transform.SetParent(transform);
                tile.transform.localScale = Vector3.one;
                Vector3 position = col % 2 == 0 ? new Vector3(col * 1.5f * Tile.EDGE_SIZE, row * Tile.HEIGHT) :
                        new Vector3(col * 1.5f * Tile.EDGE_SIZE, row * Tile.HEIGHT + Tile.HEIGHT / 2);
                tile.transform.localPosition = position;
                tile.position = new Vector2(col, row);
                tile.transform.GetChild(0).GetComponent<Text>().text = col + "," + row;
            }
        }
    }

    private void LoadBottomBackground()
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
                tile.transform.GetChild(0).GetComponent<Text>().text = col + "," + row;
                tile.transform.GetChild(0).GetComponent<Text>().fontSize = 25;
            }
        }
    }

    public void Load(GameLevel gameLevel)
    {
        levelPrefs = MainController.instance.levelPrefs;

        List<string> positions = CUtils.BuildListFromString<string>(gameLevel.positions);
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        foreach (var value in positions)
        {
            string[] values = value.Split(',');
            int col = int.Parse(values[0]);
            int row = int.Parse(values[1]);

            Tile tile = Instantiate(MonoUtils.instance.tile_background2);
            tile.transform.SetParent(MonoUtils.instance.backgroundTilesTransform);
            tile.transform.localScale = Vector3.one;
            Vector3 position = GetLocalPosition(col, row);
            tile.transform.localPosition = position;
            tile.position = new Vector2(col, row);
        }

        foreach (var value in positions)
        {
            string[] values = value.Split(',');
            int col = int.Parse(values[0]);
            int row = int.Parse(values[1]);


            Tile tile = Instantiate(values.Length == 2 ? MonoUtils.instance.tile_background : MonoUtils.instance.tile_stone);
            tile.transform.SetParent(MonoUtils.instance.backgroundTilesTransform);
            tile.transform.localScale = Vector3.one;
            Vector3 position = GetLocalPosition(col, row);
            tile.transform.localPosition = position;
            tile.position = new Vector2(col, row);

            if (values.Length == 2)
            {
                slots.Add(tile.position, tile);
            }

            if (position.x < minX) minX = position.x;
            if (position.x > maxX) maxX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.y > maxY) maxY = position.y;
        }

        float regionWidth = maxX - minX + Tile.WIDTH + (minX - Tile.WIDTH / 2) * 2; ;
        float regionHeight = maxY - minY + Tile.HEIGHT + (minY - Tile.HEIGHT / 2) * 2;

        transform.localPosition = GetComponent<RectTransform>().localPosition - new Vector3(regionWidth / 2, regionHeight / 2);
        GetComponent<RectTransform>().sizeDelta = new Vector2(regionWidth, regionHeight);

        LoadPieces(gameLevel);
    }

    private void LoadPieces(GameLevel gameLevel)
    {
        List<string> data = CUtils.BuildListFromString<string>(gameLevel.pieces);
        int id = 0;
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        foreach (var oneData in data)
        {
            List<string> positions = CUtils.BuildListFromString<string>(oneData, '-');
            Vector2 bottomPosition = Vector2.zero;

            List<Vector2> defaultPositions = new List<Vector2>();

            bool isRedundant = positions[positions.Count - 1] == "r";

            if (isRedundant)
                positions.RemoveAt(positions.Count - 1);

            int i = 0;

            for (i = 0; i < positions.Count; i++)
            {
                string[] values = positions[i].Split(',');
                int col = int.Parse(values[0]);
                int row = int.Parse(values[1]);
                Vector2 position = new Vector2(col, row);

                if (i == positions.Count - 1)
                {
                    bottomPosition = position;
                }

                if (i != positions.Count - 1)
                {
                    defaultPositions.Add(position);
                }
            }

            PiecePrefs piecesPrefs = levelPrefs.piecesPrefs.Find(x => x.id == id);

            bool isOnBoard = piecesPrefs != null;
            float scaleFactor = isOnBoard ? 1 : Const.SCALED_TILE;
            Transform parent = isOnBoard ? MonoUtils.instance.piecesTransform : MonoUtils.instance.piecesBottomTransform;

            Piece piece = CreatePiece(defaultPositions, parent);
            piece.isExtra = isRedundant;
            pieces.Add(piece);

            piece.id = id++;
            piece.bottomPosition = bottomPosition;

            piece.transform.localScale = Vector3.one * scaleFactor;

            if (isOnBoard)
            {
                piece.status = Piece.Status.OnBoard;

                Vector2 boardPosition = Utils.ConvertToVector2(piecesPrefs.boardPosition);
                piece.transform.localPosition = GetLocalPosition(boardPosition);
                piece.boardPositions = GetMatchPositions(piece, boardPosition);
                piece.UpdateTileBoardPosition();
                foreach (var pos in piece.boardPositions)
                {
                    if (slots.ContainsKey(pos))
                        slots[pos].hasCover = true;
                    else
                    {
                        levelPrefs.piecesPrefs = new List<PiecePrefs>();

                        Transform root = MonoUtils.instance.piecesTransform;
                        for (int k = root.childCount - 1; k >= 0; k--)
                        {
                            DestroyImmediate(root.GetChild(k).gameObject);
                        }

                        root = MonoUtils.instance.piecesBottomTransform;
                        for (int k = root.childCount - 1; k >= 0; k--)
                        {
                            DestroyImmediate(root.GetChild(k).gameObject);
                        }

                        foreach (var aPos in slots.Keys)
                        {
                            slots[aPos].hasCover = false;
                        }

                        pieces.Clear();

                        LoadPieces(gameLevel);
                        return;
                    }
                }
            }
            else
            {
                piece.transform.localPosition = GetLocalPosition(bottomPosition) * Const.SCALED_TILE;
            }

            i = 0;
            foreach (var position in GetMatchPositions(piece, piece.bottomPosition))
            {
                Tile tile = Instantiate(MonoUtils.instance.tile_background_bottom);
                tile.transform.SetParent(MonoUtils.instance.bottomRegionBGTransform);
                tile.transform.localScale = Vector3.one * Const.SCALED_TILE;
                tile.transform.localPosition = GetLocalPosition(position) * Const.SCALED_TILE;
                tile.position = position;

                var localPosition = tile.transform.localPosition;

                if (localPosition.x < minX) minX = localPosition.x;
                if (localPosition.x > maxX) maxX = localPosition.x;
                if (localPosition.y < minY) minY = localPosition.y;
                if (localPosition.y > maxY) maxY = localPosition.y;

                if (i == 0) piece.bottomBackground = tile;
                i++;
            }
        }

        float regionWidth = maxX - minX + Tile.WIDTH + (minX - Tile.WIDTH / 2) * 2; ;
        float regionHeight = maxY - minY + Tile.HEIGHT + (minY - Tile.HEIGHT / 2) * 2;

        var bgRT = MonoUtils.instance.bottomRegionBGTransform.GetComponent<RectTransform>();
        bgRT.localPosition = new Vector3(0, 13.7f) - new Vector3(regionWidth / 2, regionHeight / 2);
        bgRT.sizeDelta = new Vector2(regionWidth, regionHeight);

        var piecesRT = MonoUtils.instance.piecesBottomTransform.GetComponent<RectTransform>();
        piecesRT.localPosition = bgRT.localPosition;
        piecesRT.sizeDelta = bgRT.sizeDelta;
    }

    private Piece CreatePiece(List<Vector2> positions, Transform parent)
    {
        Piece piece = Instantiate(MonoUtils.instance.piecePrefab);
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
            tile.piece = piece;

            // Add shadows
            Tile tileS = Instantiate(MonoUtils.instance.tile_shadow);
            tileS.transform.SetParent(piece.shadows.transform);
            tileS.transform.localScale = Vector3.one;
            tileS.transform.localPosition = tile.transform.localPosition + new Vector3(5, -5);

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

    public bool CheckMatch(Piece piece)
    {
        float minDistance = float.MaxValue;
        Tile matchSlot = null;

        foreach (Tile slot in slots.Values)
        {
            if (slot.hasCover) continue;
            float distance = Vector3.Distance(piece.tileCenter.transform.position, slot.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                matchSlot = slot;
            }
        }

        piece.ResetMatchColor();
        piece.matches = new List<Tile>();

        if (minDistance < 0.4f)
        {
            bool isMatch = true;

            List<Vector2> matchPositions = GetMatchPositions(piece, matchSlot.position);

            foreach (var matchPos in matchPositions)
            {
                if (!slots.ContainsKey(matchPos) || slots[matchPos].hasCover)
                {
                    isMatch = false;
                    break;
                }
                else
                {
                    piece.matches.Add(slots[matchPos]);
                }
            }
            if (isMatch)
            {
                piece.HighlightMatchColor();
            }
            return isMatch;
        }
        return false;
    }

    public void ClearCovers(Piece piece)
    {
        foreach (var pos in piece.boardPositions)
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

    public bool ShowHint()
    {
        foreach (Piece piece in pieces)
        {
            if (hintPieces.Find(x => x.id == piece.id)) continue;
            if (piece.isExtra) continue;

            if (piece.status == Piece.Status.OnBoard)
            {
                var samePieces = FindSamePieces(piece);
                bool rightPos = false;
                foreach (var samePiece in samePieces)
                {
                    if (piece.boardPositions[0] == samePiece.center)
                    {
                        rightPos = true;
                        break;
                    }
                }

                if (!rightPos)
                {
                    ShowHint(piece);
                    return true;
                }
            }
        }

        foreach (Piece piece in pieces)
        {
            if (hintPieces.Find(x => x.id == piece.id)) continue;
            if (piece.isExtra) continue;

            if (piece.status == Piece.Status.OnBottom)
            {
                bool isSuccess = ShowHint(piece);
                if (isSuccess) return true;
                else continue;
            }
        }
        return false;
    }

    private bool ShowHint(Piece piece)
    {
        var samePieces = FindSamePieces(piece);
        foreach (var samePiece in samePieces)
        {
            if (samePiece.isExtra) continue;
            Piece onPos = samePieces.Find(x => x.status == Piece.Status.OnBoard && x.boardPositions[0] == samePiece.center);
            if (onPos == null)
            {
                var hintPiece = CreatePiece(samePiece.defaultPositions, MonoUtils.instance.hintPiecesTransform);
                hintPiece.transform.localScale = Vector3.one * 7f;
                hintPiece.transform.localPosition = GetLocalPosition(samePiece.center);
                hintPiece.id = piece.id;
                iTween.ScaleTo(hintPiece.gameObject, Vector3.one, 0.3f);
                hintPieces.Add(hintPiece);
                return true;
            }
        }
        return false;
    }

    private List<Piece> FindSamePieces(Piece sample)
    {
        List<Piece> result = new List<Piece>();
        foreach (Piece piece in pieces)
        {
            if (piece == sample) continue;
            List<Vector2> matchPositions = GetMatchPositions(piece, sample.center);
            if (Compare2List(matchPositions, sample.defaultPositions))
            {
                result.Add(piece);
            }
        }
        result.Insert(0, sample);
        return result;
    }

    private bool Compare2List(List<Vector2> list1, List<Vector2> list2)
    {
        if (list1.Count != list2.Count) return false;
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i]) return false;
        }
        return true;
    }

    public void CheckGameComplete()
    {
        bool isComplete = true;
        foreach (var slot in slots.Values)
        {
            if (!slot.hasCover)
            {
                isComplete = false;
                break;
            }
        }

        if (isComplete)
        {
            SavePrefs();

            List<Tile> fadedTiles = GetTileOnBoard();
            fadedTiles = fadedTiles.OrderByDescending(x => x, new TileComparer()).ToList();
            StartCoroutine(FadeTiles(fadedTiles));

            hintPieces.Clear();
            foreach (Transform hint in MonoUtils.instance.hintPiecesTransform)
            {
                Destroy(hint.gameObject);
            }

            MainController.instance.OnComplete(fadedTiles.Count);
        }

    }

    private List<Tile> GetTileOnBoard()
    {
        List<Tile> result = new List<Tile>();
        foreach (var piece in pieces)
        {
            if (piece.status == Piece.Status.OnBoard) result.AddRange(piece.tiles);
        }
        return result;
    }

    private IEnumerator FadeTiles(List<Tile> fadedTiles)
    {
        foreach (Tile tile in fadedTiles)
        {
            tile.GetComponent<Image>().CrossFadeAlpha(0, 0.5f, true);
            yield return new WaitForSeconds(0.03f);
        }
    }

    public class TileComparer : IComparer<Tile>
    {
        public int Compare(Tile x, Tile y)
        {
            var fx = F(x);
            var fy = F(y);

            if (fx < fy) return -1;
            if (fx == fy) return 0;
            return 1;
        }

        double F(Tile a) // your calculation
        {
            return a.boardPosition.x + a.boardPosition.y;
        }
    }

    private void SavePrefs()
    {
        levelPrefs.piecesPrefs = new List<PiecePrefs>();
        foreach (Piece piece in pieces)
        {
            if (piece.status == Piece.Status.OnBoard)
            {
                var piecePrefs = new PiecePrefs();
                piecePrefs.id = piece.id;
                piecePrefs.boardPosition = Utils.ConvertToString(piece.boardPositions[0]);
                levelPrefs.piecesPrefs.Add(piecePrefs);
            }
        }
    }
}
