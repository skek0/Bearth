using System;
using System.Collections.Generic;
using UnityEngine;

public enum FactionType
{
    Neutral,
    Enemy,
    Mine
}

[RequireComponent(typeof(ModuleGuid))]
public abstract class Module : MonoBehaviour, IDamageable, IHoverable, IModuleInfoSource
{
    protected bool connectable = false;

    [SerializeField] protected string moduleId = "Module";
    private List<BaseModule> connectedModules = new();
    [SerializeField] protected FactionType faction = FactionType.Neutral;

    [Header("Temp Serialize")]
    [SerializeField]protected int maxHp = 1;
    [SerializeField]protected int currentHp = 1;
    [SerializeField]protected SpriteRenderer spriteRenderer = null;

    /// <summary>For saves</summary>
    public int Hp
    {
        get => currentHp;
        set => currentHp = value;
    }
    public FactionType Faction
    {
        get => faction;
        set => faction = value;
    }
    protected Rigidbody2D rigid;
    [SerializeField]protected float mass;

    public Rigidbody2D Rigid 
    {
        get
        {
            if (rigid == null)
                rigid = GetComponent<Rigidbody2D>();
            return rigid;
        }
    }

    public IReadOnlyList<BaseModule> ConnectedModules => connectedModules;
    public bool Connectable => connectable;
    public ModuleGuid ModuleGuid { get; private set; }
    public string ModuleId => moduleId;

    public string DisplayName => moduleId;

    public int CurrentHp => currentHp;

    public int MaxHp => maxHp;

    HashSet<string> tags;
    public void SetModuleId(string id) => moduleId = id;
    protected virtual void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Module");
        ModuleGuid = GetComponent<ModuleGuid>();
        spriteRenderer = transform.Find("Skin").GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
    }

    public virtual void ApplyBaseStat(BaseStat s)
    {
        moduleId = s.ModuleID;
        mass = s.Mass;
        maxHp = s.MaxHp;
        tags = SplitToHashset(s.Tags);
    }

    private HashSet<string> SplitToHashset(string tags)
    {
        HashSet<string> tagsHashSet = new();

        if (string.IsNullOrWhiteSpace(tags))
            return tagsHashSet;
        foreach (var item in tags.Split(","))
        {
            string trimmedTag = item.Trim();
            if (!string.IsNullOrEmpty(trimmedTag))
                tagsHashSet.Add(trimmedTag);
        }
        return tagsHashSet;
    }

    public void ApplyDamage(DamageData damage)
    {
        currentHp -= damage.Amount;
        
        if(currentHp <= 0)
        {
            currentHp = 0;
            OnDeath();

            for(int i = ConnectedModules.Count - 1; i >= 0; i--)
            {
                ConnectedModules[i].Detach(transform.position, true);
            }
            Destroy(gameObject);
        }
    }
    protected virtual void OnDeath() 
    {
        Destroy(spriteRenderer.material);
    }
    public void AddConnectedModule(BaseModule module)
    {
        connectedModules.Add(module);
    }
    public void RemoveConnectedModule(BaseModule module)
    {
        connectedModules.Remove(module);
    }

    public void OnHoverEnter()
    {

    }
    public void OnHoverStay()
    {

    }
    public void OnHoverExit()
    {

    }

    public virtual bool TryGetSpecialStat(out string none)
    {
        none = null;
        return false;
    }

}
