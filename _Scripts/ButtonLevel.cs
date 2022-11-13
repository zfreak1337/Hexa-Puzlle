using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonLevel : MonoBehaviour {

    private int page, level;
    public Text levelText;
    public Sprite pass, current, locked;

    private void Start()
    {
        int index = transform.GetSiblingIndex();
        int pageIndex = transform.parent.GetSiblingIndex();
        level = pageIndex * 20 + index + 1;

        levelText.text = level.ToString();

        int unlockedLevel = LevelController.GetUnlockLevel(GameState.chosenWorld);
        GetComponent<Image>().sprite = level < unlockedLevel ? pass : level == unlockedLevel ? current : locked;

        if (level > unlockedLevel)
        {
            levelText.gameObject.SetActive(false);
            GetComponent<Button>().interactable = false;
        }
    }

    public void OnClick()
    {
        GameState.chosenLevel = level;
        CUtils.LoadScene(3, true);
        Sound.instance.PlayButton();
    }
}
