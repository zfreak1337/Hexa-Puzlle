using UnityEngine;
using System.Collections;

public class BallanceHint : MonoBehaviour {

	void Start () {
        if (Purchaser.instance.isEnable == false) gameObject.SetActive(false);
	}
}
