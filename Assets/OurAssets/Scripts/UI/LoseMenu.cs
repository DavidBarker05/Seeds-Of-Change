using TMPro;
using UnityEngine;

public class LoseMenu : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI loseText;
    [SerializeField, TextArea]
    string familyLoseMessage;
    [SerializeField, TextArea]
    string communityLoseMessage;
    [SerializeField, TextArea]
    string familyAndCommunityLoseMessage;
    
    public void UpdateLoseText(int familyFood, int communityFood) => loseText.text = familyFood <= 0 ? (communityFood <= 0 ? familyAndCommunityLoseMessage : familyLoseMessage) : communityLoseMessage;
}
