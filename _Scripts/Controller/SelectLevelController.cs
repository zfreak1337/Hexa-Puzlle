using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectLevelController : BaseController {
    public Text worldText;
    public SnapScrollRect scroll;
     
    protected override void Start()
    {
        base.Start();
        worldText.text = Const.WORLD_NAME[GameState.chosenWorld - 1].ToString();
        scroll.SetPage((LevelController.GetUnlockLevel(GameState.chosenWorld) - 1) / 20);
        CUtils.ShowInterstitialAd();
    }
}
