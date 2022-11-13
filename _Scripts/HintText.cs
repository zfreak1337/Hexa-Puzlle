using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HintText : MonoBehaviour {

    private void Start()
    {
        GameState.hint.onValueChanged += OnValueChanged;
        OnValueChanged();
    }

    private void OnValueChanged()
    {
        GetComponent<Text>().text = GameState.hint.GetValue().ToString();
    }

    private void OnDestroy()
    {
        GameState.hint.onValueChanged -= OnValueChanged;
    }
}
