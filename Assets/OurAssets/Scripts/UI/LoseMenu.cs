using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LoseMenu : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("loseText")]
    TMP_Text m_loseText;
    [SerializeField, TextArea, FormerlySerializedAs("familyLoseMessage")]
    string m_FamilyLoseMessage;
    [SerializeField, TextArea, FormerlySerializedAs("communityLoseMessage")]
    string m_CommunityLoseMessage;
    [SerializeField, TextArea, FormerlySerializedAs("familyAndCommunityLoseMessage")]
    string m_FamilyAndCommunityLoseMessage;

    public void UpdateLoseText(int familyFood, int communityFood) => m_loseText.text = familyFood <= 0 ? (communityFood <= 0 ? m_FamilyAndCommunityLoseMessage : m_FamilyLoseMessage) : m_CommunityLoseMessage;
}
