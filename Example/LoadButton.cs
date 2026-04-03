using SimpleContentLoader.Example;
using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    [SerializeField]
    MyContentLoader contentLoader;
    [SerializeField]
    MyContentConfig config;
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        _ = contentLoader.Load(config);
    }
}
