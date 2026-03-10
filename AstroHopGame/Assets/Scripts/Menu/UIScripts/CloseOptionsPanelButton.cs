using UnityEngine;

public class CloseOptionsPanelButton : MonoBehaviour
{
    private BoxCollider2D col;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (col.OverlapPoint(worldPoint))
            {
                MenuUIScript.instance.languageDropdown.Hide();
                ManagerScript.instance.CloseOptionsPanel();
            }
        }
    }
}