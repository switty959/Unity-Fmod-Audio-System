using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIAudioFeedback : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private AudioID clickSound = AudioID.ButtonClick;
    [SerializeField] private AudioID hoverSound = AudioID.ButtonHover;
    [SerializeField] private AudioID selectSound = AudioID.ButtonSelect;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayClick);
    }

    public void PlayClick()
    {
        if (button.IsInteractable())
        {
            Play(clickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable())
        {
            Play(hoverSound);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.IsInteractable())
        {
            Play(selectSound);
        }
    }

    private static void Play(AudioID id)
    {
        if (id != AudioID.None)
        {
            AudioEvents.Play(new AudioRequest(id));
        }
    }
}