using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxController : MonoBehaviour
{
    public Button doneButton;
    public BoundingBoxManager bboxManager;

    void Start()
    {
        doneButton.onClick.AddListener(OnDonePressed);
    }

    public void OnDonePressed()
    {
        bboxManager.PosConfim();
        gameObject.SetActive(false);
    }
}
