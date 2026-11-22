using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerModule : CoreModule, IPlayerControl
{
    CoreMove moveComponent;
    CoreStat sourceStat;   // DB 원본 (공유 객체이므로 수정 금지)

    protected override void Awake()
    {
        base.Awake();
        moveComponent = GetComponent<CoreMove>();
    }

    private void OnEnable()
    {
        PlayerProgress.UpgradeChanged += OnUpgradeChanged;
    }

    private void OnDisable()
    {
        PlayerProgress.UpgradeChanged -= OnUpgradeChanged;
    }

    /// <summary>
    /// 원본 스탯에 업그레이드 가산값을 더한 복사본을 적용한다.
    /// </summary>
    public override void ApplyCoreStat(CoreStat s)
    {
        sourceStat = s;
        base.ApplyCoreStat(PlayerProgress.BuildCoreStat(s));
    }

    private void OnUpgradeChanged(string upgradeId, int level)
    {
        if (sourceStat != null)
            ApplyCoreStat(sourceStat);
    }

    public void SetMoveInput(Vector2 moveinput)
    {
        moveComponent.SetMoveInput(moveinput);
    }

    public void SetRotationInput(Vector2 rotationinput)
    {
        moveComponent.SetLookTarget(Camera.main.ScreenToWorldPoint(rotationinput));
    }

    public void ClearRotationInput()
    {
        moveComponent.ClearLookTarget();
    }

public void AttackCommand()
    {
        base.Attack();
    }
}
