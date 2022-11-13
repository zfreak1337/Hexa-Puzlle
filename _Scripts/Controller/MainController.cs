using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Superpow;

public class MainController : BaseController {
    public TileRegion tileRegion;
    public int level = 1;
    public int world = 1;
    public LevelPrefs levelPrefs;
    public Text levelText;

    [HideInInspector]
    public GameLevel gameLevel;

    public static MainController instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        //gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);
        level = GameState.chosenLevel;
        world = GameState.chosenWorld;
        gameLevel = Resources.Load<GameLevel>("Levels/World_" + world + "/Level_" + level);

        string strLevelPrefs = Utils.GetLevelData(world, level);
        if (string.IsNullOrEmpty(strLevelPrefs))
        {
            levelPrefs = new LevelPrefs();
            levelPrefs.piecesPrefs = new List<PiecePrefs>();
        }
        else
        {
            levelPrefs = JsonUtility.FromJson<LevelPrefs>(strLevelPrefs);
        }

        tileRegion.Load(gameLevel);
        GameState.canPlay = true;
        levelText.text = "Level " + level.ToString();

        ProcessLevelGift();

        Utils.IncreaseNumMoves(world, level);
    }

    public void Replay()
    {
        GameState.canPlay = true;

        foreach (var piece in tileRegion.pieces)
        {
            piece.MoveToBottom();
        }

        Sound.instance.Play(Sound.Others.Replay);
    }

    public void ShowHint()
    {
        if (GameState.hint.GetValue() <= 0)
        {
            if (Purchaser.instance.isEnable)
                DialogController.instance.ShowDialog(DialogType.Shop);
            return;
        }
        bool isShown = tileRegion.ShowHint();
        if (isShown) AddHint(-1);
    }

    public void AddHint(int num)
    {
        GameState.hint.ChangeValue(num);
    }

    public void ProcessLevelGift()
    {
        if (level % 4 == 3)
        {
            bool received = Utils.IsGiftReceived(world, level);
            if (!received)
            {
                Utils.ReceiveGift(world, level);
                Timer.Schedule(this, 0.5f, () =>
                {
                    DialogController.instance.ShowDialog(DialogType.LevelGift);
                });
            }
        }
    }

    public void OnComplete(int numTile)
    {
        GameState.canPlay = false;
        SavePrefs();

        int unlockedLevel = LevelController.GetUnlockLevel(world);
        if (level == unlockedLevel)
        {
            LevelController.SetUnlockLevel(world, unlockedLevel + 1);
        }

        Timer.Schedule(this, numTile * 0.03f + 0.7f, () =>
        {
            DialogController.instance.ShowDialog(DialogType.Complete);
        });

        Sound.instance.Play(Sound.Others.Complete);
    }

    private void SavePrefs()
    {
        string data = JsonUtility.ToJson(levelPrefs);
        Utils.SetLevelData(world, level, data);
    }
}
