using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 업그레이드 창. UpgradeSpecDB의 항목마다 UpgradeRow를 하나씩 생성한다.
/// </summary>
public class UpgradePopup : Popup
{
    [SerializeField] TextMeshProUGUI scrapText;
    [SerializeField] Transform rowContainer;
    [SerializeField] UpgradeRow rowPrefab;
    [SerializeField] Button closeButton;

    private readonly List<UpgradeRow> rows = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        if (closeButton != null) closeButton.onClick.AddListener(Cancel);

        PlayerProgress.ScrapChanged += OnScrapChanged;
        PlayerProgress.UpgradeChanged += OnUpgradeChanged;

        BuildRows();
        Refresh();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (closeButton != null) closeButton.onClick.RemoveListener(Cancel);

        PlayerProgress.ScrapChanged -= OnScrapChanged;
        PlayerProgress.UpgradeChanged -= OnUpgradeChanged;
    }

    private void BuildRows()
    {
        if (rows.Count > 0) return;

        foreach (var spec in UpgradeSpecDB.Ordered)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.Bind(spec);
            rows.Add(row);
        }
    }

    private void OnScrapChanged(int _) => Refresh();
    private void OnUpgradeChanged(string _, int __) => Refresh();

    private void Refresh()
    {
        if (scrapText != null) scrapText.text = $"Scrap: {PlayerProgress.Scrap}";
        foreach (var row in rows) row.Refresh();
    }
}
