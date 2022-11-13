using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomeController : BaseController {
    private const int FACEBOOK = 0;

    public void OnClick(int index)
    {
        switch (index)
        {
            case FACEBOOK:
                CUtils.LikeFacebookPage(ConfigController.Config.facebookPageID);
                break;
        }
        Sound.instance.PlayButton();
    }
}
