using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 업그레이드 창의 한 줄: 이름 / 레벨 / 효과 / 비용 / 업그레이드 버튼
/// </summary>
public class UpgradeRow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI effectText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button upgradeButton;

    private UpgradeSpec spec;

    public void Bind(UpgradeSpec upgradeSpec)
    {
        spec = upgradeSpec;
        if (nameText != null) nameText.text = spec.DisplayName;
    }

    private void OnEnable()
    {
        upgradeButton.onClick.AddListener(OnClickUpgrade);
    }

    private void OnDisable()
    {
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
    }

    private void OnClickUpgrade()
    {
        if (spec == null) return;
        PlayerProgress.TryUpgrade(spec.UpgradeID);
    }

    public void Refresh()
    {
        if (spec == null) return;

        int level = PlayerProgress.GetLevel(spec.UpgradeID);
        bool isMax = level >= spec.MaxLevel;

        if (levelText != null) levelText.text = $"Lv {level} / {spec.MaxLevel}";
        if (effectText != null) effectText.text = BuildEffectText(level, isMax);

        if (costText != null)
            costText.text = isMax ? "MAX" : spec.Levels[level].Cost.ToString();

        upgradeButton.interactable = PlayerProgress.CanUpgrade(spec.UpgradeID);
    }

    // 예: "MaxTorque +4 (다음 +3)"
    private string BuildEffectText(int level, bool isMax)
    {
        var total = PlayerProgress.GetTotalAdd(spec.UpgradeID, level);
        var next = isMax ? null : spec.Levels[level].Add;

        var sb = new StringBuilder();
        var keys = new System.Collections.Generic.HashSet<string>(total.Keys);
        if (next != null) keys.UnionWith(next.Keys);

        foreach (var key in keys)
        {
            total.TryGetValue(key, out float cur);
            sb.Append($"{key} +{cur:0.##}");
            if (next != null && next.TryGetValue(key, out float add))
                sb.Append($" (다음 +{add:0.##})");
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }
}
