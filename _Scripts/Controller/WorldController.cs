using UnityEngine;
using System.Collections;

public class WorldController : BaseController {

	public void OnDailyGiftReached()
    {
        Timer.Schedule(this, 0.5f, () =>
        {
            DialogController.instance.ShowDialog(DialogType.DailyGift);
        });
    }
}
