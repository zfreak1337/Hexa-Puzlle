using UnityEngine;
using System.Collections;

public class CompleteDialog : Dialog {

	public void OnReplayClick()
    {
        Close();
        Sound.instance.PlayButton();
        MainController.instance.Replay();
    }

    public void OnNextClick()
    {
        Close();
        Sound.instance.PlayButton();
        if (GameState.chosenLevel == Const.MAX_LEVEL)
        {
            CUtils.LoadScene(1, true);
        }
        else
        {
            GameState.chosenLevel++;
            CUtils.LoadScene(3, true);
        }
    }
}
