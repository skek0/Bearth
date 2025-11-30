using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Module Basic Info")]
public class BasicInfo : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [TextArea] public string description;

    [Header("Resources")]
    [SerializeField] Sprite icon;

    [Header("Basic Stat")]
    [SerializeField] int maxHp = 10;
    [SerializeField] float mass = 1f;

    public string   Id          { get { return id; }}
    public string   DisplayName { get { return displayName; }}
    public string   Description { get { return description; }}
    public int      MaxHp       { get { return maxHp; }}
    public float    Mass        { get { return mass; }}
}