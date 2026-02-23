using System.Collections.Generic;
using UnityEngine;

public enum FactionType
{
    Neutral,
    Enemy,
    Mine
}

[RequireComponent(typeof(ModuleGuid))]
public abstract class Module : MonoBehaviour, IDamageable
{
    protected bool connectable = false;

    [SerializeField] protected string moduleId = "Module";
    [Header("디버깅용")]
    [SerializeField] private List<BaseModule> connectedModules = new();
    [SerializeField] protected FactionType faction = FactionType.Neutral;

    [SerializeField]protected int maxHp = 1;
    [SerializeField]protected int hp = 1;
    string typeId;
    string type;
    int tier;
    string rarity;
    float Mass;
    int price;
    string prefabPath;

    /// <summary>For saves</summary>
    public int Hp
    {
        get => hp;
        set => hp = value;
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
    public string TypeId { get => typeId; }

    public void SetModuleId(string id) => moduleId = id;
    protected virtual void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Module");
        ModuleGuid = GetComponent<ModuleGuid>();
    }

    protected virtual void Start()
    {
        /// 임시 주입 : 원래는 로드를 통한 주입
        ApplyBaseStat(ModuleSpecDB.BaseStats[ModuleId]);
    }

    public void ApplyBaseStat(BaseStat s)
    {
        typeId = s.TypeID;
        type = s.Type;
        tier = s.Tier;
        rarity = s.Rarity;
        mass = s.Mass;
        maxHp = s.MaxHp;
        price = s.Price;
        prefabPath = s.PrefabPath;
    }

    public void ApplyDamage(DamageData damage)
    {
        hp -= damage.Amount;
        
        if(hp <= 0)
        {
            hp = 0;
            OnDeath();

            for(int i = ConnectedModules.Count - 1; i >= 0; i--)
            {
                ConnectedModules[i].Detach(transform.position, true);
            }
            Destroy(gameObject);
        }
    }
    protected virtual void OnDeath() {}
    public void AddConnectedModule(BaseModule module)
    {
        connectedModules.Add(module);
    }
    public void RemoveConnectedModule(BaseModule module)
    {
        connectedModules.Remove(module);
    }
}
