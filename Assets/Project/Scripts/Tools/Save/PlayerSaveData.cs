using System;

[Serializable]
public class PlayerSaveData
{
    public int version = 1;
    public ShipSaveData ship;
}