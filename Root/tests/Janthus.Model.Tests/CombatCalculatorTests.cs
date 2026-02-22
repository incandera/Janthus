using Xunit;
using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Data;

namespace Janthus.Model.Tests;

public class CombatCalculatorTests : IDisposable
{
    private readonly JanthusDbContext _context;
    private readonly GameDataRepository _repository;

    public CombatCalculatorTests()
    {
        var options = new DbContextOptionsBuilder<JanthusDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new JanthusDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _repository = new GameDataRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    // ---------------------------------------------------------------
    // Helper factories
    // ---------------------------------------------------------------

    private static LeveledActor MakeActor(int con, int dex, int intel, int luck,
                                           int att, int str, int will)
    {
        return new LeveledActor(con, dex, intel, luck, att, str, will);
    }

    private static Item MakeWeapon(int id, string name, decimal attackRating,
                                    int strBonus = 0, int dexBonus = 0,
                                    int conBonus = 0, int luckBonus = 0)
    {
        return new Item
        {
            Id = id, Name = name, Slot = EquipmentSlot.Weapon,
            AttackRating = attackRating, ArmorRating = 0,
            StrengthBonus = strBonus, DexterityBonus = dexBonus,
            ConstitutionBonus = conBonus, LuckBonus = luckBonus
        };
    }

    private static Item MakeArmor(int id, string name, EquipmentSlot slot,
                                   decimal armorRating,
                                   int conBonus = 0, int dexBonus = 0,
                                   int strBonus = 0, int luckBonus = 0)
    {
        return new Item
        {
            Id = id, Name = name, Slot = slot,
            AttackRating = 0, ArmorRating = armorRating,
            ConstitutionBonus = conBonus, DexterityBonus = dexBonus,
            StrengthBonus = strBonus, LuckBonus = luckBonus
        };
    }

    private static void EquipItem(PlayerCharacter actor, Item item)
    {
        actor.Inventory.Add(new InventoryItem(item));
        CombatCalculator.Equip(actor, actor.Inventory, item);
    }

    private static void EquipItem(NonPlayerCharacter actor, Item item)
    {
        actor.Inventory.Add(new InventoryItem(item));
        CombatCalculator.Equip(actor, actor.Inventory, item);
    }

    // helper: actor with Inventory list (PlayerCharacter or NonPlayerCharacter)
    private static PlayerCharacter MakePlayer(int con, int dex, int intel, int luck,
                                               int att, int str, int will)
    {
        var pc = new PlayerCharacter();
        pc.Constitution.Value = con;
        pc.Dexterity.Value = dex;
        pc.Intelligence.Value = intel;
        pc.Luck.Value = luck;
        pc.Attunement.Value = att;
        pc.Strength.Value = str;
        pc.Willpower.Value = will;
        return pc;
    }

    private static NonPlayerCharacter MakeNpc(int con, int dex, int intel, int luck,
                                               int att, int str, int will)
    {
        return new NonPlayerCharacter(con, dex, intel, luck, att, str, will,
            new Alignment(LawfulnessType.Neutral, DispositionType.Neutral));
    }

    private Skill MakeCombatSkill(string levelName)
    {
        var combatType = _repository.GetSkillTypes().Find(s => s.Name == "Combat");
        var level = _repository.GetSkillLevels().Find(l => l.Name == levelName);
        return new Skill { Id = 1, Type = combatType, Level = level };
    }

    // ---------------------------------------------------------------
    // Attack Rating — base formula
    // ---------------------------------------------------------------

    [Fact]
    public void AttackRating_NoEquipment_NoSkill_UsesStrength()
    {
        // Formula: EffectiveStr * 1.5 + TotalEquipAR, no skill = ×1.0
        var actor = MakeActor(5, 5, 5, 5, 5, 8, 5);

        var ar = CombatCalculator.CalculateAttackRating(actor, new List<Skill>(), _repository);

        // 8 * 1.5 = 12.0
        Assert.Equal(12.0m, ar);
    }

    [Fact]
    public void AttackRating_WithWeapon_AddsEquipmentRating()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 8, 5);
        var sword = MakeWeapon(100, "Test Sword", 10);
        EquipItem(player, sword);

        var ar = CombatCalculator.CalculateAttackRating(player, player.Skills, _repository);

        // (8 * 1.5 + 10) * 1.0 = 22.0
        Assert.Equal(22.0m, ar);
    }

    [Fact]
    public void AttackRating_WeaponStrengthBonus_IncreasesEffectiveStrength()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 8, 5);
        var sword = MakeWeapon(100, "Mighty Sword", 10, strBonus: 3);
        EquipItem(player, sword);

        var ar = CombatCalculator.CalculateAttackRating(player, player.Skills, _repository);

        // EffectiveStr = 8 + 3 = 11; (11 * 1.5 + 10) * 1.0 = 26.5
        Assert.Equal(26.5m, ar);
    }

    // ---------------------------------------------------------------
    // Armor Rating — base formula
    // ---------------------------------------------------------------

    [Fact]
    public void ArmorRating_NoEquipment_NoSkill_UsesConstitution()
    {
        var actor = MakeActor(10, 5, 5, 5, 5, 5, 5);

        var dr = CombatCalculator.CalculateArmorRating(actor, new List<Skill>(), _repository);

        // 10 * 0.5 = 5.0
        Assert.Equal(5.0m, dr);
    }

    [Fact]
    public void ArmorRating_WithArmor_AddsEquipmentRating()
    {
        var player = MakePlayer(10, 5, 5, 5, 5, 5, 5);
        var cuirass = MakeArmor(101, "Test Cuirass", EquipmentSlot.Cuirass, 8);
        EquipItem(player, cuirass);

        var dr = CombatCalculator.CalculateArmorRating(player, player.Skills, _repository);

        // (10 * 0.5 + 8) * 1.0 = 13.0
        Assert.Equal(13.0m, dr);
    }

    [Fact]
    public void ArmorRating_ConstitutionBonus_IncreasesEffectiveConstitution()
    {
        var player = MakePlayer(10, 5, 5, 5, 5, 5, 5);
        var cuirass = MakeArmor(101, "Hardy Cuirass", EquipmentSlot.Cuirass, 8, conBonus: 4);
        EquipItem(player, cuirass);

        var dr = CombatCalculator.CalculateArmorRating(player, player.Skills, _repository);

        // EffectiveCon = 10 + 4 = 14; (14 * 0.5 + 8) * 1.0 = 15.0
        Assert.Equal(15.0m, dr);
    }

    // ---------------------------------------------------------------
    // Combat Skill modifier — affects attack, armor, hit chance
    // ---------------------------------------------------------------

    [Theory]
    [InlineData("Novice", 0.1)]    // avg (0.0 + 0.2)/2 = 0.1
    [InlineData("Apprentice", 0.3)] // avg (0.2 + 0.4)/2 = 0.3
    [InlineData("Journeyman", 0.5)] // avg (0.4 + 0.6)/2 = 0.5
    [InlineData("Expert", 0.7)]     // avg (0.6 + 0.8)/2 = 0.7
    [InlineData("Master", 0.9)]     // avg (0.8 + 1.0)/2 = 0.9
    public void AttackRating_CombatSkill_AppliesMultiplier(string levelName, double expectedMod)
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        player.Skills.Add(MakeCombatSkill(levelName));

        var ar = CombatCalculator.CalculateAttackRating(player, player.Skills, _repository);

        // baseRating = 10 * 1.5 = 15; skillMod * 0.5; result = 15 * (1 + mod * 0.5)
        var expected = 15.0m * (1.0m + (decimal)expectedMod * 0.5m);
        Assert.Equal(expected, ar);
    }

    [Theory]
    [InlineData("Novice", 0.1)]
    [InlineData("Apprentice", 0.3)]
    [InlineData("Journeyman", 0.5)]
    [InlineData("Expert", 0.7)]
    [InlineData("Master", 0.9)]
    public void ArmorRating_CombatSkill_AppliesMultiplier(string levelName, double expectedMod)
    {
        var player = MakePlayer(10, 5, 5, 5, 5, 5, 5);
        player.Skills.Add(MakeCombatSkill(levelName));

        var dr = CombatCalculator.CalculateArmorRating(player, player.Skills, _repository);

        // baseRating = 10 * 0.5 = 5; skillMod * 0.3; result = 5 * (1 + mod * 0.3)
        var expected = 5.0m * (1.0m + (decimal)expectedMod * 0.3m);
        Assert.Equal(expected, dr);
    }

    [Fact]
    public void AttackRating_MasterCombat_FullSetup()
    {
        // Realistic scenario: Str 10, weapon AR 15, +2 str bonus, Master Combat skill
        var player = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        var sword = MakeWeapon(100, "Fine Sword", 15, strBonus: 2);
        EquipItem(player, sword);
        player.Skills.Add(MakeCombatSkill("Master"));

        var ar = CombatCalculator.CalculateAttackRating(player, player.Skills, _repository);

        // EffectiveStr = 12; base = 12 * 1.5 + 15 = 33; mod = 0.9 * 0.5 = 0.45
        // 33 * 1.45 = 47.85
        Assert.Equal(47.85m, ar);
    }

    // ---------------------------------------------------------------
    // Equipment system — Equip / Unequip
    // ---------------------------------------------------------------

    [Fact]
    public void Equip_AddsItemToEquipmentSlot()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var sword = MakeWeapon(100, "Sword", 10);
        player.Inventory.Add(new InventoryItem(sword));

        CombatCalculator.Equip(player, player.Inventory, sword);

        Assert.True(player.Equipment.ContainsKey(EquipmentSlot.Weapon));
        Assert.Same(sword, player.Equipment[EquipmentSlot.Weapon]);
    }

    [Fact]
    public void Equip_RemovesFromInventory()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var sword = MakeWeapon(100, "Sword", 10);
        player.Inventory.Add(new InventoryItem(sword, 2));

        CombatCalculator.Equip(player, player.Inventory, sword);

        var remaining = player.Inventory.Find(i => i.Item.Id == sword.Id);
        Assert.NotNull(remaining);
        Assert.Equal(1, remaining.Quantity);
    }

    [Fact]
    public void Equip_SwapsExistingItem()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var sword1 = MakeWeapon(100, "Old Sword", 5);
        var sword2 = MakeWeapon(101, "New Sword", 15);
        player.Inventory.Add(new InventoryItem(sword1));
        player.Inventory.Add(new InventoryItem(sword2));

        CombatCalculator.Equip(player, player.Inventory, sword1);
        var previous = CombatCalculator.Equip(player, player.Inventory, sword2);

        Assert.Same(sword1, previous);
        Assert.Same(sword2, player.Equipment[EquipmentSlot.Weapon]);
        // Old sword should be back in inventory
        Assert.NotNull(player.Inventory.Find(i => i.Item.Id == sword1.Id));
    }

    [Fact]
    public void Unequip_ReturnsItemToInventory()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var sword = MakeWeapon(100, "Sword", 10);
        player.Inventory.Add(new InventoryItem(sword));
        CombatCalculator.Equip(player, player.Inventory, sword);

        var result = CombatCalculator.Unequip(player, player.Inventory, EquipmentSlot.Weapon);

        Assert.True(result);
        Assert.False(player.Equipment.ContainsKey(EquipmentSlot.Weapon));
        Assert.NotNull(player.Inventory.Find(i => i.Item.Id == sword.Id));
    }

    [Fact]
    public void Unequip_EmptySlot_ReturnsFalse()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);

        var result = CombatCalculator.Unequip(player, player.Inventory, EquipmentSlot.Weapon);

        Assert.False(result);
    }

    [Fact]
    public void Equip_NoSlot_ReturnsNull()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var consumable = new Item { Id = 200, Name = "Potion", Slot = EquipmentSlot.None };
        player.Inventory.Add(new InventoryItem(consumable));

        var result = CombatCalculator.Equip(player, player.Inventory, consumable);

        Assert.Null(result);
        Assert.Empty(player.Equipment);
    }

    // ---------------------------------------------------------------
    // Multiple equipment pieces — stacking bonuses
    // ---------------------------------------------------------------

    [Fact]
    public void MultipleEquipment_BonusesStack()
    {
        var player = MakePlayer(5, 5, 5, 3, 5, 5, 5);
        // Helmet: +1 Con, AR 2
        var helmet = MakeArmor(110, "Helmet", EquipmentSlot.Helmet, 2, conBonus: 1);
        // Cuirass: +2 Con, AR 6
        var cuirass = MakeArmor(111, "Cuirass", EquipmentSlot.Cuirass, 6, conBonus: 2);
        // Boots: +1 Dex, AR 1
        var boots = MakeArmor(112, "Boots", EquipmentSlot.Boots, 1, dexBonus: 1);
        // Weapon: +2 Str, Attack 12
        var weapon = MakeWeapon(113, "Weapon", 12, strBonus: 2);

        EquipItem(player, helmet);
        EquipItem(player, cuirass);
        EquipItem(player, boots);
        EquipItem(player, weapon);

        // Effective attributes
        Assert.Equal(8, player.EffectiveConstitution);  // 5 + 1 + 2
        Assert.Equal(6, player.EffectiveDexterity);     // 5 + 1
        Assert.Equal(7, player.EffectiveStrength);       // 5 + 2
        Assert.Equal(3, player.EffectiveLuck);           // 3 + 0

        // Total equipment ratings
        Assert.Equal(12m, player.TotalEquipmentAttackRating);    // only weapon
        Assert.Equal(9m, player.TotalEquipmentArmorRating);      // 2 + 6 + 1
    }

    // ---------------------------------------------------------------
    // Damage — size modifier
    // ---------------------------------------------------------------

    [Fact]
    public void Damage_EqualSize_NoModifier()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        attacker.SizeMultiplier = 1.0m;
        attacker.CurrentHitPoints = 100;
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        defender.SizeMultiplier = 1.0m;
        defender.CurrentHitPoints = 100;

        // Use fixed seed for deterministic luck roll
        var rng = new Random(42);
        var dmg = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, rng);

        // attackRating = 10 * 1.5 = 15; armorRating = 5 * 0.5 = 2.5
        // rawDamage = max(1, 15 - 2.5 * 0.5) = max(1, 13.75) = 13.75
        // sizeRatio = 1.0 → sizeMod = 1.0
        // netLuck = 0 → bias=0, variance=0 → luckMod = 1.0
        // finalDamage = round(13.75 * 1.0 * 1.0) = 14
        Assert.Equal(14, dmg);
    }

    [Fact]
    public void Damage_LargeAttacker_IncreasedDamage()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        attacker.SizeMultiplier = 2.0m;
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        defender.SizeMultiplier = 1.0m;

        var rng = new Random(42);
        var dmgLarge = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, rng);

        // sizeRatio = 2.0 → sizeMod = 1 + (2-1)*0.5 = 1.5
        // rawDamage = 13.75, netLuck = 0 → luckMod ≈ 1.0
        // finalDamage = round(13.75 * 1.5 * 1.0) ≈ 21
        Assert.True(dmgLarge > 14, $"Large attacker should deal more than baseline 14, got {dmgLarge}");
    }

    [Fact]
    public void Damage_SmallAttacker_ReducedDamage()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        attacker.SizeMultiplier = 0.5m;
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        defender.SizeMultiplier = 1.0m;

        var rng = new Random(42);
        var dmgSmall = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, rng);

        // sizeRatio = 0.5 → sizeMod = 0.5 (full penalty for small)
        // finalDamage = round(13.75 * 0.5 * luckMod)
        Assert.True(dmgSmall < 14, $"Small attacker should deal less than baseline 14, got {dmgSmall}");
    }

    [Fact]
    public void Damage_SizeAsymmetry_SmallAttackerPenalizedMoreThanLargeBenefits()
    {
        // The formula: sizeRatio < 1 → sizeMod = sizeRatio (full penalty)
        //              sizeRatio >= 1 → sizeMod = 1 + (ratio - 1) * 0.5 (half benefit)
        // So ratio=2 gives sizeMod=1.5, but ratio=0.5 gives sizeMod=0.5
        // This is intentionally asymmetric: being small hurts more than being big helps.

        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        // Large attacker: ratio 4.0 → sizeMod = 1 + 3*0.5 = 2.5
        attacker.SizeMultiplier = 4.0m;
        defender.SizeMultiplier = 1.0m;
        var dmgLarge = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(0));

        // Small attacker: ratio 0.25 → sizeMod = 0.25
        attacker.SizeMultiplier = 0.25m;
        defender.SizeMultiplier = 1.0m;
        var dmgSmall = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(0));

        // 2.5x multiplier for 4x larger vs 0.25x multiplier for 4x smaller
        Assert.True(dmgLarge > dmgSmall * 2,
            $"Size asymmetry: 4x large={dmgLarge} should be more than 2× the 4x small={dmgSmall}");
    }

    [Fact]
    public void Damage_ZeroSize_DefaultsToOne()
    {
        // SizeMultiplier of 0 should be treated as 1
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        attacker.SizeMultiplier = 0m;
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        defender.SizeMultiplier = 0m;

        var rng = new Random(42);
        var dmg = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, rng);

        // Should behave identically to size = 1
        Assert.Equal(14, dmg);
    }

    [Fact]
    public void Damage_SizeRatio_ClampedAt4x()
    {
        // sizeRatio = clamp(attacker/defender, 0.25, 4.0)
        // Even a 10x size advantage is capped at 4.0 ratio
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        attacker.SizeMultiplier = 10.0m;
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        defender.SizeMultiplier = 1.0m;
        var dmg10x = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(0));

        attacker.SizeMultiplier = 4.0m;
        var dmg4x = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(0));

        Assert.Equal(dmg4x, dmg10x);
    }

    // ---------------------------------------------------------------
    // Damage — luck modifier
    // ---------------------------------------------------------------

    [Fact]
    public void Damage_EqualLuck_NoBiasOrVariance()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 10, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        // With equal luck (both 5), netLuck=0 → bias=0, variance=0 → luckMod=1.0
        // Every seed should produce the same damage
        var damages = new HashSet<int>();
        for (int seed = 0; seed < 50; seed++)
        {
            damages.Add(CombatCalculator.CalculateDamage(
                attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(seed)));
        }

        Assert.Single(damages); // Should always produce the same value
    }

    [Fact]
    public void Damage_HighLuckAdvantage_BiasFavorsAttacker()
    {
        var attacker = MakePlayer(5, 5, 5, 15, 5, 10, 5); // luck = 15
        var defender = MakeNpc(5, 5, 5, 3, 5, 5, 5);      // luck = 3

        // netLuck = 12, bias = 0.24, variance = 0.48
        // Average luckMod ≈ 1.24 (biased upward)
        long totalDamage = 0;
        int trials = 1000;
        for (int i = 0; i < trials; i++)
        {
            totalDamage += CombatCalculator.CalculateDamage(
                attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(i));
        }
        var avgDamage = (double)totalDamage / trials;

        // Baseline (equal luck) = 14 damage. With luck bias, average should be higher.
        Assert.True(avgDamage > 15.0, $"High luck advantage should raise average damage above 15, got {avgDamage:F2}");
    }

    [Fact]
    public void Damage_HighLuckDisadvantage_BiasFavorsDefender()
    {
        var attacker = MakePlayer(5, 5, 5, 3, 5, 10, 5);  // luck = 3
        var defender = MakeNpc(5, 5, 5, 15, 5, 5, 5);     // luck = 15

        // netLuck = -12, bias = -0.24, variance = 0.48
        long totalDamage = 0;
        int trials = 1000;
        for (int i = 0; i < trials; i++)
        {
            totalDamage += CombatCalculator.CalculateDamage(
                attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(i));
        }
        var avgDamage = (double)totalDamage / trials;

        // With negative luck bias, average should be below baseline
        Assert.True(avgDamage < 13.0, $"High luck disadvantage should lower average damage below 13, got {avgDamage:F2}");
    }

    [Fact]
    public void Damage_LuckMod_ClampedBetweenHalfAndDouble()
    {
        // Even extreme luck differences can't push damage below 0.5x or above 2.0x
        var attacker = MakePlayer(5, 5, 5, 50, 5, 10, 5);  // extreme luck
        var defender = MakeNpc(5, 5, 5, 1, 5, 5, 5);

        int minDmg = int.MaxValue, maxDmg = int.MinValue;
        for (int i = 0; i < 1000; i++)
        {
            var d = CombatCalculator.CalculateDamage(
                attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(i));
            minDmg = Math.Min(minDmg, d);
            maxDmg = Math.Max(maxDmg, d);
        }

        // rawDamage ≈ 13.75 (attacker luck bonus doesn't affect attack/armor ratings)
        // EffectiveLuck 50 - 1 = netLuck 49: bias = 0.98, variance = 1.96
        // luckMod clamped to [0.5, 2.0]
        // min possible = round(13.75 * 0.5) = 7; max possible = round(13.75 * 2.0) = 28
        Assert.True(minDmg >= 1, "Damage should never go below 1");
        Assert.True(maxDmg <= 28, $"Damage should be bounded by luckMod clamp, got {maxDmg}");
    }

    [Fact]
    public void Damage_LuckEquipmentBonus_AffectsOutcome()
    {
        var attacker = MakePlayer(5, 5, 5, 3, 5, 10, 5);
        var amulet = new Item
        {
            Id = 150, Name = "Lucky Charm", Slot = EquipmentSlot.Accessory,
            LuckBonus = 10
        };
        EquipItem(attacker, amulet);

        // EffectiveLuck = 3 + 10 = 13
        Assert.Equal(13, attacker.EffectiveLuck);

        var defender = MakeNpc(5, 5, 5, 3, 5, 5, 5); // luck = 3

        long totalDamage = 0;
        int trials = 500;
        for (int i = 0; i < trials; i++)
        {
            totalDamage += CombatCalculator.CalculateDamage(
                attacker, new List<Skill>(), defender, new List<Skill>(), _repository, new Random(i));
        }
        var avgDamage = (double)totalDamage / trials;

        // netLuck = 10, bias = 0.2 → average luckMod ≈ 1.2 → average damage ≈ 16.5
        Assert.True(avgDamage > 15.0, $"Luck bonus from equipment should boost avg damage above 15, got {avgDamage:F2}");
    }

    // ---------------------------------------------------------------
    // Damage — minimum floor of 1
    // ---------------------------------------------------------------

    [Fact]
    public void Damage_HighArmorVsLowAttack_MinimumOne()
    {
        // Weak attacker vs heavily armored defender
        var attacker = MakePlayer(5, 5, 5, 5, 5, 1, 5); // str 1
        var defender = MakeNpc(20, 5, 5, 5, 5, 5, 5);   // con 20

        // Give defender heavy armor
        var armor = MakeArmor(120, "Plate", EquipmentSlot.Cuirass, 30, conBonus: 5);
        EquipItem(defender, armor);

        var rng = new Random(42);
        var dmg = CombatCalculator.CalculateDamage(
            attacker, new List<Skill>(), defender, new List<Skill>(), _repository, rng);

        Assert.True(dmg >= 1, "Damage should always be at least 1");
    }

    // ---------------------------------------------------------------
    // Hit Chance — base formula
    // ---------------------------------------------------------------

    [Fact]
    public void RollHit_EqualDex_NoSkill_BaseChance70Percent()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // Should be ~70% with equal dex and no skill
        Assert.True(hitRate > 0.65 && hitRate < 0.75,
            $"Base hit rate should be ~70%, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_HighDexAdvantage_IncreasesHitChance()
    {
        var attacker = MakePlayer(5, 10, 5, 5, 5, 5, 5); // dex 10
        var defender = MakeNpc(5, 3, 5, 5, 5, 5, 5);     // dex 3

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // 0.7 + (10-3)*0.03 = 0.7 + 0.21 = 0.91
        Assert.True(hitRate > 0.87 && hitRate < 0.95,
            $"High dex advantage should give ~91% hit rate, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_HighDexDisadvantage_DecreasesHitChance()
    {
        var attacker = MakePlayer(5, 3, 5, 5, 5, 5, 5);  // dex 3
        var defender = MakeNpc(5, 10, 5, 5, 5, 5, 5);    // dex 10

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // 0.7 + (3-10)*0.03 = 0.7 - 0.21 = 0.49
        Assert.True(hitRate > 0.44 && hitRate < 0.54,
            $"High dex disadvantage should give ~49% hit rate, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_DexEquipmentBonus_AffectsHitChance()
    {
        var attacker = MakePlayer(5, 3, 5, 5, 5, 5, 5);
        var boots = MakeArmor(130, "Swift Boots", EquipmentSlot.Boots, 1, dexBonus: 5);
        EquipItem(attacker, boots);

        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // EffectiveDex = 3+5=8; 0.7 + (8-5)*0.03 = 0.79
        Assert.True(hitRate > 0.74 && hitRate < 0.84,
            $"Dex equipment bonus should raise hit rate to ~79%, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_CombatSkill_IncreasesHitChance()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        attacker.Skills.Add(MakeCombatSkill("Master"));
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, attacker.Skills, defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // 0.7 + 0 + 0.9*0.2 = 0.7 + 0.18 = 0.88
        Assert.True(hitRate > 0.83 && hitRate < 0.93,
            $"Master Combat skill should give ~88% hit rate, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_HitChance_ClampedAt95Percent()
    {
        // Even with extreme dex and skill, cap at 95%
        var attacker = MakePlayer(5, 30, 5, 5, 5, 5, 5);
        attacker.Skills.Add(MakeCombatSkill("Master"));
        var defender = MakeNpc(5, 1, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, attacker.Skills, defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        Assert.True(hitRate > 0.92 && hitRate < 0.98,
            $"Hit chance should be capped near 95%, got {hitRate:P1}");
    }

    [Fact]
    public void RollHit_HitChance_FloorAt10Percent()
    {
        // Even a hopelessly outmatched attacker has 10% chance
        var attacker = MakePlayer(5, 1, 5, 5, 5, 5, 5);
        var defender = MakeNpc(5, 30, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        Assert.True(hitRate > 0.06 && hitRate < 0.14,
            $"Hit chance floor should be ~10%, got {hitRate:P1}");
    }

    // ---------------------------------------------------------------
    // Consumable items — TryConsumeItem
    // ---------------------------------------------------------------

    [Fact]
    public void TryConsumeItem_HealthPotion_Restores40PercentHP()
    {
        var actor = MakePlayer(10, 5, 5, 5, 5, 8, 6);
        // MaxHP = (10*0.5 + 8*0.25 + 6*0.25)*10 = 85
        actor.CurrentHitPoints = 40;
        var potion = new Item { Id = 300, Name = "Health Potion", Slot = EquipmentSlot.None };
        actor.Inventory.Add(new InventoryItem(potion, 3));

        var msg = CombatCalculator.TryConsumeItem(actor, actor.Inventory, potion);

        // Restore = 85 * 0.4 = 34; 40 + 34 = 74
        Assert.NotNull(msg);
        Assert.Equal(74m, actor.CurrentHitPoints);
        Assert.Equal(2, actor.Inventory.Find(i => i.Item.Id == potion.Id).Quantity);
    }

    [Fact]
    public void TryConsumeItem_HealthPotion_CapsAtMaxHP()
    {
        var actor = MakePlayer(10, 5, 5, 5, 5, 8, 6);
        actor.CurrentHitPoints = 80; // very close to max 85
        var potion = new Item { Id = 300, Name = "Health Potion", Slot = EquipmentSlot.None };
        actor.Inventory.Add(new InventoryItem(potion));

        CombatCalculator.TryConsumeItem(actor, actor.Inventory, potion);

        Assert.Equal(85m, actor.CurrentHitPoints);
    }

    [Fact]
    public void TryConsumeItem_ManaPotion_Restores40PercentMP()
    {
        var actor = MakePlayer(5, 5, 10, 5, 12, 5, 8);
        // MaxMana = (12*0.5 + 10*0.25 + 8*0.25)*10 = 105
        actor.CurrentMana = 20;
        var potion = new Item { Id = 301, Name = "Mana Potion", Slot = EquipmentSlot.None };
        actor.Inventory.Add(new InventoryItem(potion));

        var msg = CombatCalculator.TryConsumeItem(actor, actor.Inventory, potion);

        // Restore = 105 * 0.4 = 42; 20 + 42 = 62
        Assert.NotNull(msg);
        Assert.Equal(62m, actor.CurrentMana);
    }

    [Fact]
    public void TryConsumeItem_EquippableItem_ReturnsNull()
    {
        var actor = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var sword = MakeWeapon(100, "Sword", 10);
        actor.Inventory.Add(new InventoryItem(sword));

        var msg = CombatCalculator.TryConsumeItem(actor, actor.Inventory, sword);

        Assert.Null(msg);
    }

    [Fact]
    public void TryConsumeItem_UnknownConsumable_ReturnsNull()
    {
        var actor = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var junk = new Item { Id = 999, Name = "Random Junk", Slot = EquipmentSlot.None };
        actor.Inventory.Add(new InventoryItem(junk));

        var msg = CombatCalculator.TryConsumeItem(actor, actor.Inventory, junk);

        Assert.Null(msg);
    }

    [Fact]
    public void TryConsumeItem_LastPotion_RemovesFromInventory()
    {
        var actor = MakePlayer(10, 5, 5, 5, 5, 8, 6);
        actor.CurrentHitPoints = 40;
        var potion = new Item { Id = 300, Name = "Health Potion", Slot = EquipmentSlot.None };
        actor.Inventory.Add(new InventoryItem(potion, 1));

        CombatCalculator.TryConsumeItem(actor, actor.Inventory, potion);

        Assert.Null(actor.Inventory.Find(i => i.Item.Id == potion.Id));
    }

    // ---------------------------------------------------------------
    // Magic combat — helper factories
    // ---------------------------------------------------------------

    private Skill MakeMagicSkill(string levelName)
    {
        var magicType = _repository.GetSkillTypes().Find(s => s.Name == "Magic");
        var level = _repository.GetSkillLevels().Find(l => l.Name == levelName);
        return new Skill { Id = 2, Type = magicType, Level = level };
    }

    private static Operation MakeSpell(int id, string name, decimal basePower, decimal manaCost, float range)
    {
        return new Operation
        {
            Id = id, Name = name, BasePower = basePower, ManaCost = manaCost,
            Range = range, EffectType = EffectType.Magical, CooldownSeconds = 1.5
        };
    }

    // ---------------------------------------------------------------
    // Magic Attack Rating — base formula
    // ---------------------------------------------------------------

    [Fact]
    public void MagicAttackRating_NoSkill_UsesIntelligence()
    {
        // Formula: EffectiveInt * 1.5 + BasePower, no skill = ×1.0
        var actor = MakeActor(5, 5, 8, 5, 5, 5, 5);
        var spell = MakeSpell(1, "Test Spell", 10, 5, 6f);

        var mar = CombatCalculator.CalculateMagicAttackRating(actor, new List<Skill>(), spell, _repository);

        // 8 * 1.5 + 10 = 22.0
        Assert.Equal(22.0m, mar);
    }

    [Theory]
    [InlineData("Novice", 0.1)]
    [InlineData("Apprentice", 0.3)]
    [InlineData("Journeyman", 0.5)]
    [InlineData("Expert", 0.7)]
    [InlineData("Master", 0.9)]
    public void MagicAttackRating_MagicSkill_AppliesMultiplier(string levelName, double expectedMod)
    {
        var player = MakePlayer(5, 5, 10, 5, 5, 5, 5);
        player.Skills.Add(MakeMagicSkill(levelName));
        var spell = MakeSpell(1, "Test Spell", 8, 5, 6f);

        var mar = CombatCalculator.CalculateMagicAttackRating(player, player.Skills, spell, _repository);

        // baseRating = 10 * 1.5 + 8 = 23; result = 23 * (1 + mod * 0.5)
        var expected = 23.0m * (1.0m + (decimal)expectedMod * 0.5m);
        Assert.Equal(expected, mar);
    }

    // ---------------------------------------------------------------
    // Magic Resistance — base formula
    // ---------------------------------------------------------------

    [Fact]
    public void MagicResistance_UsesWillpowerAndAttunement()
    {
        // Formula: (EffectiveWillpower * 0.5 + EffectiveAttunement * 0.3 + EquipAR * 0.2) * skillMod
        var actor = MakeActor(5, 5, 5, 5, 8, 5, 10);

        var mr = CombatCalculator.CalculateMagicResistance(actor, new List<Skill>(), _repository);

        // 10 * 0.5 + 8 * 0.3 + 0 * 0.2 = 5.0 + 2.4 = 7.4
        Assert.Equal(7.4m, mr);
    }

    // ---------------------------------------------------------------
    // Magic Damage — full calculation
    // ---------------------------------------------------------------

    [Fact]
    public void MagicDamage_EqualStats_ProducesReasonableOutput()
    {
        var attacker = MakePlayer(5, 5, 8, 5, 6, 5, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 8);
        var spell = MakeSpell(1, "Magic Missile", 8, 5, 6f);

        var rng = new Random(42);
        var dmg = CombatCalculator.CalculateMagicDamage(
            attacker, new List<Skill>(), spell, defender, new List<Skill>(), _repository, rng);

        Assert.True(dmg >= 1, "Magic damage should be at least 1");
    }

    [Fact]
    public void MagicDamage_HighIntelligence_IncreasedDamage()
    {
        var weakAttacker = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var strongAttacker = MakePlayer(5, 5, 15, 5, 5, 5, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);
        var spell = MakeSpell(1, "Magic Missile", 8, 5, 6f);

        var rng1 = new Random(42);
        var dmgWeak = CombatCalculator.CalculateMagicDamage(
            weakAttacker, new List<Skill>(), spell, defender, new List<Skill>(), _repository, rng1);
        var rng2 = new Random(42);
        var dmgStrong = CombatCalculator.CalculateMagicDamage(
            strongAttacker, new List<Skill>(), spell, defender, new List<Skill>(), _repository, rng2);

        Assert.True(dmgStrong > dmgWeak,
            $"High intelligence should increase magic damage: strong={dmgStrong}, weak={dmgWeak}");
    }

    [Fact]
    public void MagicDamage_HighWillpowerDefender_ReducedDamage()
    {
        var attacker = MakePlayer(5, 5, 10, 5, 6, 5, 5);
        var weakDefender = MakeNpc(5, 5, 5, 5, 3, 5, 3);
        var strongDefender = MakeNpc(5, 5, 5, 5, 8, 5, 15);
        var spell = MakeSpell(1, "Magic Missile", 8, 5, 6f);

        var rng1 = new Random(42);
        var dmgVsWeak = CombatCalculator.CalculateMagicDamage(
            attacker, new List<Skill>(), spell, weakDefender, new List<Skill>(), _repository, rng1);
        var rng2 = new Random(42);
        var dmgVsStrong = CombatCalculator.CalculateMagicDamage(
            attacker, new List<Skill>(), spell, strongDefender, new List<Skill>(), _repository, rng2);

        Assert.True(dmgVsStrong < dmgVsWeak,
            $"High willpower defender should take less damage: vsStrong={dmgVsStrong}, vsWeak={dmgVsWeak}");
    }

    // ---------------------------------------------------------------
    // Magic Hit — probability
    // ---------------------------------------------------------------

    [Fact]
    public void RollMagicHit_BaseChance75Percent()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 5, 5, 5);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 5);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollMagicHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        Assert.True(hitRate > 0.70 && hitRate < 0.80,
            $"Base magic hit rate should be ~75%, got {hitRate:P1}");
    }

    [Fact]
    public void RollMagicHit_HighAttunement_IncreasesChance()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 10, 5, 5); // att 10
        var defender = MakeNpc(5, 5, 5, 5, 3, 5, 5);     // will 5

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollMagicHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        // 0.75 + (10-5)*0.03 = 0.75 + 0.15 = 0.90
        Assert.True(hitRate > 0.85 && hitRate < 0.95,
            $"High attunement should give ~90% magic hit rate, got {hitRate:P1}");
    }

    [Fact]
    public void RollMagicHit_CappedAt95Percent()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 30, 5, 5);
        attacker.Skills.Add(MakeMagicSkill("Master"));
        var defender = MakeNpc(5, 5, 5, 5, 1, 5, 1);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollMagicHit(attacker, attacker.Skills, defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        Assert.True(hitRate > 0.92 && hitRate < 0.98,
            $"Magic hit chance should be capped near 95%, got {hitRate:P1}");
    }

    [Fact]
    public void RollMagicHit_FloorAt15Percent()
    {
        var attacker = MakePlayer(5, 5, 5, 5, 1, 5, 1);
        var defender = MakeNpc(5, 5, 5, 5, 5, 5, 30);

        int hits = 0;
        int trials = 10000;
        for (int i = 0; i < trials; i++)
        {
            if (CombatCalculator.RollMagicHit(attacker, new List<Skill>(), defender, _repository, new Random(i)))
                hits++;
        }

        double hitRate = (double)hits / trials;
        Assert.True(hitRate > 0.11 && hitRate < 0.19,
            $"Magic hit chance floor should be ~15%, got {hitRate:P1}");
    }

    // ---------------------------------------------------------------
    // SelectOperation — spell selection
    // ---------------------------------------------------------------

    [Fact]
    public void SelectOperation_PicksHighestPowerInRange()
    {
        var actor = MakePlayer(5, 5, 10, 5, 8, 5, 5);
        actor.CurrentMana = 100;
        var magicMissile = MakeSpell(1, "Magic Missile", 8, 5, 6f);
        var fireball = MakeSpell(2, "Fireball", 18, 15, 5f);
        var skill = MakeMagicSkill("Journeyman");
        skill.ConferredOperationList.Add(magicMissile);
        skill.ConferredOperationList.Add(fireball);
        actor.Skills.Add(skill);

        var selected = CombatCalculator.SelectOperation(actor, actor.Skills, 4.0f);

        Assert.NotNull(selected);
        Assert.Equal("Fireball", selected.Name);
    }

    [Fact]
    public void SelectOperation_RespectsManaCost()
    {
        var actor = MakePlayer(5, 5, 10, 5, 8, 5, 5);
        actor.CurrentMana = 10; // enough for Magic Missile (5) but not Fireball (15)
        var magicMissile = MakeSpell(1, "Magic Missile", 8, 5, 6f);
        var fireball = MakeSpell(2, "Fireball", 18, 15, 5f);
        var skill = MakeMagicSkill("Journeyman");
        skill.ConferredOperationList.Add(magicMissile);
        skill.ConferredOperationList.Add(fireball);
        actor.Skills.Add(skill);

        var selected = CombatCalculator.SelectOperation(actor, actor.Skills, 4.0f);

        Assert.NotNull(selected);
        Assert.Equal("Magic Missile", selected.Name);
    }

    [Fact]
    public void SelectOperation_NoOperations_ReturnsNull()
    {
        var actor = MakePlayer(5, 5, 5, 5, 5, 8, 5);
        actor.CurrentMana = 100;
        // No skills with operations — melee-only actor

        var selected = CombatCalculator.SelectOperation(actor, actor.Skills, 1.0f);

        Assert.Null(selected);
    }

    // ---------------------------------------------------------------
    // Effective attributes — equipment bonuses (Intelligence, Attunement, Willpower)
    // ---------------------------------------------------------------

    [Fact]
    public void EffectiveIntelligence_IncludesEquipmentBonus()
    {
        var player = MakePlayer(5, 5, 8, 5, 5, 5, 5);
        var amulet = new Item
        {
            Id = 200, Name = "Arcane Amulet", Slot = EquipmentSlot.Accessory,
            IntelligenceBonus = 3
        };
        EquipItem(player, amulet);

        Assert.Equal(11, player.EffectiveIntelligence);
    }

    [Fact]
    public void EffectiveAttunement_IncludesEquipmentBonus()
    {
        var player = MakePlayer(5, 5, 5, 5, 6, 5, 5);
        var amulet = new Item
        {
            Id = 201, Name = "Mystic Amulet", Slot = EquipmentSlot.Accessory,
            AttunementBonus = 4
        };
        EquipItem(player, amulet);

        Assert.Equal(10, player.EffectiveAttunement);
    }

    [Fact]
    public void EffectiveWillpower_IncludesEquipmentBonus()
    {
        var player = MakePlayer(5, 5, 5, 5, 5, 5, 7);
        var helmet = new Item
        {
            Id = 202, Name = "Helm of Will", Slot = EquipmentSlot.Helmet,
            WillpowerBonus = 2
        };
        EquipItem(player, helmet);

        Assert.Equal(9, player.EffectiveWillpower);
    }
}
