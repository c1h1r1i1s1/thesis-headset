using UnityEngine;

public class CategoryItem : MonoBehaviour
{
    private int categoryId;
    private string objectCategory;
    private bool selected;

    public delegate void OnToggleSelection(int categoryId, bool selected);
    public static event OnToggleSelection ToggleSelection;

    public void Init(int objectId, string objectCategoryIn)
    {
        categoryId = objectId;
        objectCategory = objectCategoryIn;
        selected = false;
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = objectCategory;
    }

    public void ToggleCategorySelection() {
        selected = !selected;
        ToggleSelection?.Invoke(categoryId, selected);
    }
}
