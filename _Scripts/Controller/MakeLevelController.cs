using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Superpow;

public class MakeLevelController : BaseController {
    public TileRegion2 tileRegion;
    public GameLevel gameLevel;

    public InputField worldInput, levelInput, numColInput, numRowInput;
    public Text loadLevelText;
    public Button mapButton, piecesButton, applyButton;

    public int world, level, numRow, numCol;

    public List<Tile> listSlots = new List<Tile>();
    public List<Tile> listExtraTile = new List<Tile>();

    public bool doneGeneratePieces;

    private string piecesResult = "";
    private List<List<Tile>> pieces = new List<List<Tile>>();
    public int totalTiles = 0;

    public static MakeLevelController instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        tileRegion.LoadBottomBackground();

        worldInput.text = PlayerPrefs.GetInt("level_editor_world", 1).ToString();
        levelInput.text = PlayerPrefs.GetInt("level_editor_level", 1).ToString();
        numRowInput.text = "5";
        numColInput.text = "6";

        UpdateUI();
    }

    public void OnInputValueChanged()
    {
        if (worldInput.text == "-") worldInput.text = "";
        if (levelInput.text == "-") levelInput.text = "";
        if (numRowInput.text == "-") numRowInput.text = "";
        if (numColInput.text == "-") numColInput.text = "";

        if (!string.IsNullOrEmpty(worldInput.text))
        {
            int.TryParse(worldInput.text, out world);
        }

        if (!string.IsNullOrEmpty(levelInput.text))
        {
            int.TryParse(levelInput.text, out level);
        }

        if (!string.IsNullOrEmpty(numRowInput.text))
        {
            int.TryParse(numRowInput.text, out numRow);
            if (numRow < 1) numRowInput.text = "1";
            else if (numRow > 7) numRowInput.text = "7";
        }

        if (!string.IsNullOrEmpty(numColInput.text))
        {
            int.TryParse(numColInput.text, out numCol);
            if (numCol < 1) numColInput.text = "1";
            else if (numCol > 7) numColInput.text = "9";
        }

        UpdateLoadLevelText();
    }

    public void UpdateLoadLevelText()
    {
        var gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);
        loadLevelText.text = gameLevel == null ? "Add" : "Load";
    }

    public void OnLoadClick()
    {
        gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);

        if (gameLevel == null)
        {
#if UNITY_EDITOR
            string folderPath = "Assets/Hexa_Puzzle/Resources/Levels/World_" + world;
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Hexa_Puzzle/Resources/Levels", "World_" + world);

            gameLevel = ScriptableObject.CreateInstance<GameLevel>();
#endif
        }

        piecesResult = "";
        totalTiles = 0;
        pieces.Clear();
        listExtraTile.Clear();
        doneGeneratePieces = false;

        tileRegion.LoadBoardBackground();
        if (!string.IsNullOrEmpty(gameLevel.pieces))
        {
            tileRegion.LoadPieces(gameLevel);
            foreach (var p in tileRegion.pieces)
            {
                pieces.Add(p.tiles);
            }

            piecesResult = gameLevel.pieces;
            doneGeneratePieces = true;
        }
        else
        {
            tileRegion.ClearPieces();
        }

        LoadListSlots();
        UpdateUI();

        PlayerPrefs.SetInt("level_editor_world", world);
        PlayerPrefs.SetInt("level_editor_level", level);
    }

    private void UpdateUI()
    {
        if (gameLevel == null)
        {
            mapButton.interactable = false;
            piecesButton.interactable = false;
            applyButton.interactable = false;
        }
        else
        {
            mapButton.interactable = true;
            piecesButton.interactable = !string.IsNullOrEmpty(gameLevel.positions);
            applyButton.interactable = doneGeneratePieces;
        }
    }

    private void LoadListSlots()
    {
        listSlots.Clear();
        foreach (Transform slot in MonoUtils.instance.backgroundTilesTransform)
        {
            Tile tile = slot.GetComponent<Tile>();
			if (tile.isActive && tile.type == Tile.Type.Background) listSlots.Add(tile);
        }
    }

    public void GeneratePositions()
    {
        string result = "";

        LoadListSlots();

        foreach(var tile in listSlots)
        {
            result += tile.position.x + "," + tile.position.y + "|";
        }
        gameLevel.positions = result;
        print(result);

		CreateOrReplaceAsset(gameLevel, GetLevelPath(world, level));
		UpdateLoadLevelText();
		UpdateUI();
    }
    
    public void GeneratePieces()
    {
        List<Tile> piece = new List<Tile>();

        if (doneGeneratePieces)
        {
            // For extra pieces
            foreach (var tile in listSlots)
            {
                if (tile.isActive == false && !listExtraTile.Contains(tile))
                {
                    piece.Add(tile);
                }
            }

            if (piece.Count == 0) return;

            listExtraTile.AddRange(piece);

            string extraPiecesResult = "";
            foreach (var tile in piece)
            {
                extraPiecesResult += tile.position.x + "," + tile.position.y + "-";
            }
            extraPiecesResult += "0,0" + "-r|";
            print(extraPiecesResult);

            piecesResult += extraPiecesResult;

            gameLevel.pieces = piecesResult;
            tileRegion.LoadPieces(gameLevel);
            CreateOrReplaceAsset(gameLevel, GetLevelPath(world, level));
        }

        else
        {
            foreach (var tile in listSlots)
            {
                if (tile.isActive == false && !HasElement(tile))
                {
                    piece.Add(tile);
                }
            }

            if (piece.Count == 0) return;

            totalTiles += piece.Count;
            pieces.Add(piece);

            piecesResult = CreatePiecesResult();

            print(piecesResult);

            if (totalTiles == listSlots.Count)
            {
                doneGeneratePieces = true;
                gameLevel.pieces = piecesResult;
                tileRegion.LoadPieces(gameLevel);

                //Enable the backgrounds
                foreach (var tile in listSlots)
                {
                    tile.SetActive(true);
                }

                UpdateUI();
                CreateOrReplaceAsset(gameLevel, GetLevelPath(world, level));
            }
        }
    }

    private string CreatePiecesResult()
    {
        string result = "";

        foreach (var aPiece in pieces)
        {
            foreach (var tile in aPiece)
            {
                result += tile.position.x + "," + tile.position.y + "-";
            }
            result += "0,0" + "|";
        }

        return result;
    }

    public void AdjustPieces()
    {
        string result = "";
        foreach(var aPiece in tileRegion.pieces)
        {
            foreach (var position in aPiece.defaultPositions)
            {
                result += position.x + "," + position.y + "-";
            }
            result += aPiece.boardPositions[0].x + "," + aPiece.boardPositions[0].y;
            if (aPiece.isExtra) result += "-r";
            result += "|";
        }

        gameLevel.pieces = result;

        CreateOrReplaceAsset(gameLevel, GetLevelPath(world, level));
        piecesResult = gameLevel.pieces;
    }

    public void ApplyLevel()
    {
#if UNITY_EDITOR
        AdjustPieces();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = gameLevel;

        Toast.instance.ShowMessage("Done");
#endif
    }

    private T CreateOrReplaceAsset<T>(T asset, string path) where T : ScriptableObject
    {
#if UNITY_EDITOR
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
#else
        return null;
#endif
    }

    private bool HasElement(Tile element)
    {
        foreach(var piece in pieces)
        {
            foreach(var tile in piece)
            {
                if (element == tile) return true;
            }
        }
        return false;
    }

    private string GetLevelPath(int world, int level)
    {
        return "Assets/Hexa_Puzzle/Resources/Levels/World_" + world + "/Level_" + level + ".asset";
    }
}
