using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GiftDialog : Dialog {
    public GameObject before, after, oneHint, multipleHints;
    public Text multipleHintText, multipleHintNumberText;
    public Animator boxAnimator;

    public int minHint = 1, maxHint = 2;
    private bool isOpeningBox;

    protected override void Start()
    {
        base.Start();
    }

    public void OnOkDialog()
    {
        if (isOpeningBox) return;
        isOpeningBox = true;

        boxAnimator.SetTrigger("open");
        Invoke("OpenBox", 1f);
    }

    private void OpenBox()
    {
        before.SetActive(false);
        after.SetActive(true);

        int num = Random.Range(minHint, maxHint + 1);
        if (num > 1)
        {
            multipleHints.SetActive(true);
            oneHint.SetActive(false);
            multipleHintText.text = "Congratulations! You got " + num + " hints";
            multipleHintNumberText.text = num + " x";
        }
        else
        {
            multipleHints.SetActive(false);
            oneHint.SetActive(true);
        }

        GameState.hint.ChangeValue(num);
    }
}
