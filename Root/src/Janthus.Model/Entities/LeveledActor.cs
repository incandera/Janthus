using Janthus.Model.Enums;
using Janthus.Model.Services;

namespace Janthus.Model.Entities;

public class LeveledActor : Actor
{
    private CharacterAttribute _constitution;
    private CharacterAttribute _dexterity;
    private CharacterAttribute _intelligence;
    private CharacterAttribute _luck;
    private CharacterAttribute _attunement;
    private CharacterAttribute _strength;
    private CharacterAttribute _willpower;

    public CharacterAttribute Constitution
    {
        get { return _constitution ??= new CharacterAttribute(); }
        set { _constitution = value; }
    }

    public CharacterAttribute Dexterity
    {
        get { return _dexterity ??= new CharacterAttribute(); }
        set { _dexterity = value; }
    }

    public CharacterAttribute Intelligence
    {
        get { return _intelligence ??= new CharacterAttribute(); }
        set { _intelligence = value; }
    }

    public CharacterAttribute Luck
    {
        get { return _luck ??= new CharacterAttribute(); }
        set { _luck = value; }
    }

    public CharacterAttribute Attunement
    {
        get { return _attunement ??= new CharacterAttribute(); }
        set { _attunement = value; }
    }

    public CharacterAttribute Strength
    {
        get { return _strength ??= new CharacterAttribute(); }
        set { _strength = value; }
    }

    public CharacterAttribute Willpower
    {
        get { return _willpower ??= new CharacterAttribute(); }
        set { _willpower = value; }
    }

    public int SumOfAttributes =>
        Constitution.Value + Dexterity.Value + Intelligence.Value +
        Luck.Value + Attunement.Value + Strength.Value + Willpower.Value;

    public ActorLevel Level => _dataProvider?.CalculateLevel(SumOfAttributes);
    public double MaximumHitPoints => CharacterCalculator.CalculateHitPoints(Constitution, Strength, Willpower);
    public double MaximumMana => CharacterCalculator.CalculateMana(Attunement, Intelligence, Willpower);

    public override List<Attack> AttackList { get; set; } = new();

    // Equipment system (runtime-only, not DB-persisted)
    public Dictionary<EquipmentSlot, Item> Equipment { get; set; } = new();

    public int EffectiveStrength => Strength.Value + GetEquipmentBonus(i => i.StrengthBonus);
    public int EffectiveDexterity => Dexterity.Value + GetEquipmentBonus(i => i.DexterityBonus);
    public int EffectiveConstitution => Constitution.Value + GetEquipmentBonus(i => i.ConstitutionBonus);
    public int EffectiveLuck => Luck.Value + GetEquipmentBonus(i => i.LuckBonus);

    public decimal TotalEquipmentAttackRating
    {
        get
        {
            var total = 0m;
            foreach (var kvp in Equipment)
                total += kvp.Value.AttackRating;
            return total;
        }
    }

    public decimal TotalEquipmentArmorRating
    {
        get
        {
            var total = 0m;
            foreach (var kvp in Equipment)
                total += kvp.Value.ArmorRating;
            return total;
        }
    }

    private int GetEquipmentBonus(Func<Item, int> selector)
    {
        var total = 0;
        foreach (var kvp in Equipment)
            total += selector(kvp.Value);
        return total;
    }

    private readonly IGameDataProvider _dataProvider;

    private Dictionary<string, Func<CharacterAttribute>> AttributeAccessors => new()
    {
        ["Constitution"] = () => Constitution,
        ["Dexterity"] = () => Dexterity,
        ["Intelligence"] = () => Intelligence,
        ["Luck"] = () => Luck,
        ["Attunement"] = () => Attunement,
        ["Strength"] = () => Strength,
        ["Willpower"] = () => Willpower,
    };

    public LeveledActor() { }

    public LeveledActor(IGameDataProvider dataProvider, string rollAsClass, int level)
    {
        _dataProvider = dataProvider;

        var random = new Random();
        var targetLevel = dataProvider.GetLevel(level);
        var nextLevel = dataProvider.GetLevel(level + 1);
        var distributablePointDifference = nextLevel.MinimumSumOfAttributes - targetLevel.MinimumSumOfAttributes;

        var distributablePoints = targetLevel.MinimumSumOfAttributes + random.Next(0, distributablePointDifference);

        var domainClass = dataProvider.GetClass(rollAsClass);

        Constitution.Value = (int)Math.Round(distributablePoints * domainClass.ConstitutionRollWeight);
        Dexterity.Value = (int)Math.Round(distributablePoints * domainClass.DexterityRollWeight);
        Intelligence.Value = (int)Math.Round(distributablePoints * domainClass.IntelligenceRollWeight);
        Luck.Value = (int)Math.Round(distributablePoints * domainClass.LuckRollWeight);
        Attunement.Value = (int)Math.Round(distributablePoints * domainClass.AttunementRollWeight);
        Strength.Value = (int)Math.Round(distributablePoints * domainClass.StrengthRollWeight);
        Willpower.Value = (int)Math.Round(distributablePoints * domainClass.WillpowerRollWeight);

        var rollWeights = new List<KeyValuePair<string, double>>
        {
            new("Constitution", domainClass.ConstitutionRollWeight),
            new("Dexterity", domainClass.DexterityRollWeight),
            new("Intelligence", domainClass.IntelligenceRollWeight),
            new("Luck", domainClass.LuckRollWeight),
            new("Attunement", domainClass.AttunementRollWeight),
            new("Strength", domainClass.StrengthRollWeight),
            new("Willpower", domainClass.WillpowerRollWeight),
        };

        if (SumOfAttributes < distributablePoints)
        {
            var orderedRollWeights = rollWeights.OrderByDescending(x => x.Value).ToList();

            do
            {
                var attrName = orderedRollWeights[0].Key;
                orderedRollWeights.RemoveAt(0);

                var attribute = AttributeAccessors[attrName]();
                attribute.Value++;
            } while (SumOfAttributes < distributablePoints);
        }
        else if (SumOfAttributes > distributablePoints)
        {
            var orderedRollWeights = rollWeights.OrderBy(x => x.Value).ToList();

            do
            {
                var attrName = orderedRollWeights[0].Key;
                orderedRollWeights.RemoveAt(0);

                var attribute = AttributeAccessors[attrName]();

                if (attribute.Value == 1) continue;

                attribute.Value--;
            } while (SumOfAttributes > distributablePoints);  // BUG FIX: was < in original
        }
    }

    public LeveledActor(int constitution, int dexterity, int intelligence,
                        int luck, int attunement, int strength, int willpower)
    {
        Constitution.Value = constitution;
        Dexterity.Value = dexterity;
        Intelligence.Value = intelligence;
        Luck.Value = luck;
        Attunement.Value = attunement;
        Strength.Value = strength;
        Willpower.Value = willpower;
    }
}
