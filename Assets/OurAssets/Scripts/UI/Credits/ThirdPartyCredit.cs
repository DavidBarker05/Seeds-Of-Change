using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class ThirdPartyCredit : MonoBehaviour
{
    [SerializeField]
    string m_AssetName;
    [SerializeField]
    string m_CreatorName;
    [SerializeField]
    string m_URL;
    [SerializeField]
    TMP_Text m_AssetNameText;
    [SerializeField]
    TMP_Text m_CreatorNameText;
    [SerializeField]
    HyperlinkHandler m_URLText;

    void OnValidate() => UpdateFields();
    void Awake() => UpdateFields();

    void UpdateFields()
    {
        if (m_AssetNameText) m_AssetNameText.text = m_AssetName;
        if (m_CreatorNameText) m_CreatorNameText.text = $"By {m_CreatorName}";
        if (m_URLText) m_URLText.GetComponent<TMP_Text>().text = $"<color=#006CFF><u><link=\"{m_URL}\">{m_URL}</link></u></color>";
    }
}
