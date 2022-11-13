using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonWorld : MonoBehaviour {
    public Text progressText;
    private int world;

	private void Start ()
    {
        world = transform.GetSiblingIndex() + 1;

        int passLevel = LevelController.GetUnlockLevel(world) - 1;
        progressText.text = passLevel + "/60";
	}

    public void OnClick()
    {
        GameState.chosenWorld = world;
        CUtils.LoadScene(2, true);
        Sound.instance.PlayButton();
    }
}
