using TMPro;
using UnityEngine;

public class RefreshScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countText;

    public void SetCount(int count) { countText.text = count.ToString(); }
}
