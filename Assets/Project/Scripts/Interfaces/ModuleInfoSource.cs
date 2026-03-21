using UnityEngine;

public interface IModuleInfoSource
{
    string DisplayName { get; }
    int CurrentHp { get; }
    int MaxHp { get; }
    bool TryGetSpecialStat(out string specialStat);
}