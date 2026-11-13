using TMPro;
using UnityEngine;

public class SelectionPopupScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayName;
    [SerializeField] TextMeshProUGUI description;

    public void SetUp(string name, string desc)
    {
        displayName.text = name;
        description.text = desc;
    }


}
