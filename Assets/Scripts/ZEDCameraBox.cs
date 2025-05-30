using UnityEngine;

public class ZEDCameraBox : MonoBehaviour
{
    private Renderer rend;
    private Material mat;
    void Awake() {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }
    // public void Toggle() // Need function for selecting and moving
    // {
    //     isSelected = !isSelected;
    //     UpdateVisual();
    //     SocketManager.Instance.SendSelectionUpdate(id, isSelected);
        
    // }

    public void onHover()
    {
        mat.color = new Color(1, 1, 1, 0.2f);
        // Possibly show adjust dialog box
    }

    public void unHover()
    {
        mat.color = new Color(1, 1, 1, 0.05f);
    }

}
