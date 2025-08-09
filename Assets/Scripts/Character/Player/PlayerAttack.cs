using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    PlayerModule playerModule;

    private void Awake()
    {
        playerModule = GetComponent<PlayerModule>();
    }

    public void AttackCommand()
    {
        playerModule.Attack();
    }
}
