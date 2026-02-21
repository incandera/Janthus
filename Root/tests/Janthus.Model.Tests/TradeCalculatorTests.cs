using Xunit;
using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Data;

namespace Janthus.Model.Tests;

public class TradeCalculatorTests : IDisposable
{
    private readonly JanthusDbContext _context;
    private readonly GameDataRepository _repository;

    public TradeCalculatorTests()
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
    // Helpers
    // ---------------------------------------------------------------

    private static Item MakeItem(decimal tradeValue, Quality quality = null, Material material = null)
    {
        return new Item
        {
            Id = 1, Name = "Test Item", TradeValue = tradeValue,
            Slot = EquipmentSlot.None, Quality = quality, Material = material
        };
    }

    private static Alignment Neutral => new(LawfulnessType.Neutral, DispositionType.Neutral);
    private static Alignment LawfulGood => new(LawfulnessType.Lawful, DispositionType.Good);
    private static Alignment ChaoticEvil => new(LawfulnessType.Chaotic, DispositionType.Evil);

    private Skill MakeDiplomacySkill(string levelName)
    {
        var diplomacyType = _repository.GetSkillTypes().Find(s => s.Name == "Diplomacy");
        var level = _repository.GetSkillLevels().Find(l => l.Name == levelName);
        return new Skill { Id = 1, Type = diplomacyType, Level = level };
    }

    // ---------------------------------------------------------------
    // Buy Price — base
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_BaseValue_WithNeutralAlignments()
    {
        var item = MakeItem(100);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 1.0 (markup) * 1.0 (alignment) * 1.0 (no diplomacy) = 100
        Assert.Equal(100m, price);
    }

    [Fact]
    public void BuyPrice_MerchantMarkup_Applies()
    {
        var item = MakeItem(100);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.5m, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 1.5 = 150
        Assert.Equal(150m, price);
    }

    // ---------------------------------------------------------------
    // Buy Price — Quality and Material multipliers
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_QualityMultiplier_IncreasesPrice()
    {
        var quality = new Quality { Id = 1, Name = "Superior", TradeValueMultiplier = 1.5m };
        var item = MakeItem(100, quality: quality);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 1.5 = 150
        Assert.Equal(150m, price);
    }

    [Fact]
    public void BuyPrice_MaterialMultiplier_IncreasesPrice()
    {
        var material = new Material { Id = 1, Name = "Steel", TradeValueMultiplier = 2.0m };
        var item = MakeItem(100, material: material);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 2.0 = 200
        Assert.Equal(200m, price);
    }

    [Fact]
    public void BuyPrice_QualityAndMaterial_Stack()
    {
        var quality = new Quality { Id = 1, Name = "Superior", TradeValueMultiplier = 1.5m };
        var material = new Material { Id = 1, Name = "Steel", TradeValueMultiplier = 2.0m };
        var item = MakeItem(100, quality: quality, material: material);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 1.5 * 2.0 = 300
        Assert.Equal(300m, price);
    }

    [Fact]
    public void BuyPrice_ZeroQualityMultiplier_Ignored()
    {
        // TradeValueMultiplier of 0 is treated as "not applicable"
        var quality = new Quality { Id = 1, Name = "Common", TradeValueMultiplier = 0m };
        var item = MakeItem(100, quality: quality);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        Assert.Equal(100m, price);
    }

    [Fact]
    public void BuyPrice_ZeroMaterialMultiplier_Ignored()
    {
        var material = new Material { Id = 1, Name = "Unknown", TradeValueMultiplier = 0m };
        var item = MakeItem(100, material: material);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        Assert.Equal(100m, price);
    }

    // ---------------------------------------------------------------
    // Buy Price — merchant alignment modifiers
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_GoodMerchant_Discount()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Good);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 - 0.05 (good) = 0.95
        Assert.Equal(95m, price);
    }

    [Fact]
    public void BuyPrice_EvilMerchant_Surcharge()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 + 0.15 (evil) = 1.15
        Assert.Equal(115m, price);
    }

    [Fact]
    public void BuyPrice_LawfulMerchant_Discount()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Lawful, DispositionType.Neutral);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 - 0.05 (lawful) = 0.95
        Assert.Equal(95m, price);
    }

    [Fact]
    public void BuyPrice_ChaoticMerchant_Surcharge()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Chaotic, DispositionType.Neutral);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 + 0.10 (chaotic) = 1.10
        Assert.Equal(110m, price);
    }

    [Fact]
    public void BuyPrice_LawfulGoodMerchant_StackedDiscount()
    {
        var item = MakeItem(100);
        var merchantAlignment = LawfulGood;

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 - 0.05 (good) - 0.05 (lawful) = 0.90
        Assert.Equal(90m, price);
    }

    [Fact]
    public void BuyPrice_ChaoticEvilMerchant_StackedSurcharge()
    {
        var item = MakeItem(100);
        var merchantAlignment = ChaoticEvil;

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 + 0.15 (evil) + 0.10 (chaotic) = 1.25
        Assert.Equal(125m, price);
    }

    // ---------------------------------------------------------------
    // Buy Price — same disposition sympathy
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_SameDisposition_Good_ExtraDiscount()
    {
        var item = MakeItem(100);
        var playerAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Good);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Good);

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, playerAlignment, merchantAlignment, new List<Skill>(), _repository);

        // alignmentMod = 1.0 - 0.05 (good merchant) - 0.05 (sympathy) = 0.90
        Assert.Equal(90m, price);
    }

    [Fact]
    public void BuyPrice_SameDisposition_Neutral_NoSympathy()
    {
        var item = MakeItem(100);
        // Both neutral disposition — sympathy clause specifically excludes Neutral
        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, new List<Skill>(), _repository);

        Assert.Equal(100m, price);
    }

    // ---------------------------------------------------------------
    // Buy Price — Diplomacy skill discount
    // ---------------------------------------------------------------

    [Theory]
    [InlineData("Novice", 0.1)]     // midpoint = 0.1, discount = 0.1 * 0.15 = 0.015
    [InlineData("Apprentice", 0.3)] // midpoint = 0.3, discount = 0.045
    [InlineData("Journeyman", 0.5)] // midpoint = 0.5, discount = 0.075
    [InlineData("Expert", 0.7)]     // midpoint = 0.7, discount = 0.105
    [InlineData("Master", 0.9)]     // midpoint = 0.9, discount = 0.135
    public void BuyPrice_DiplomacySkill_AppliesDiscount(string levelName, double midpoint)
    {
        var item = MakeItem(1000); // use 1000 for easier math
        var skills = new List<Skill> { MakeDiplomacySkill(levelName) };

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, skills, _repository);

        var expectedDiscount = (decimal)midpoint * 0.15m;
        var expected = Math.Round(1000m * (1.0m - expectedDiscount));
        Assert.Equal(expected, price);
    }

    [Fact]
    public void BuyPrice_MasterDiplomacy_MaxDiscount()
    {
        var item = MakeItem(1000);
        var skills = new List<Skill> { MakeDiplomacySkill("Master") };

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.0m, Neutral, Neutral, skills, _repository);

        // 0.9 * 0.15 = 0.135 → 13.5% discount → 1000 * 0.865 = 865
        Assert.Equal(865m, price);
    }

    // ---------------------------------------------------------------
    // Sell Price — base
    // ---------------------------------------------------------------

    [Fact]
    public void SellPrice_BaseValue_50PercentOfTradeValue()
    {
        var item = MakeItem(100);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, Neutral, new List<Skill>(), _repository);

        Assert.Equal(50m, price);
    }

    [Fact]
    public void SellPrice_QualityAndMaterial_Apply()
    {
        var quality = new Quality { Id = 1, Name = "Superior", TradeValueMultiplier = 1.5m };
        var material = new Material { Id = 1, Name = "Steel", TradeValueMultiplier = 2.0m };
        var item = MakeItem(100, quality: quality, material: material);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, Neutral, new List<Skill>(), _repository);

        // 100 * 1.5 * 2.0 * 0.50 = 150
        Assert.Equal(150m, price);
    }

    // ---------------------------------------------------------------
    // Sell Price — merchant alignment (inverted)
    // ---------------------------------------------------------------

    [Fact]
    public void SellPrice_GoodMerchant_PaysMore()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Good);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // 100 * 0.50 * (1.0 + 0.10) = 55
        Assert.Equal(55m, price);
    }

    [Fact]
    public void SellPrice_EvilMerchant_PaysLess()
    {
        var item = MakeItem(100);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, merchantAlignment, new List<Skill>(), _repository);

        // 100 * 0.50 * (1.0 - 0.10) = 45
        Assert.Equal(45m, price);
    }

    // ---------------------------------------------------------------
    // Sell Price — Diplomacy skill bonus
    // ---------------------------------------------------------------

    [Fact]
    public void SellPrice_MasterDiplomacy_MaxBonus()
    {
        var item = MakeItem(1000);
        var skills = new List<Skill> { MakeDiplomacySkill("Master") };

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, Neutral, skills, _repository);

        // 1000 * 0.50 * (1.0 + 0.9 * 0.15) = 500 * 1.135 = 567.5 → 568
        Assert.Equal(568m, price);
    }

    [Fact]
    public void SellPrice_NoDiplomacy_BaseValue()
    {
        var item = MakeItem(1000);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, Neutral, new List<Skill>(), _repository);

        Assert.Equal(500m, price);
    }

    // ---------------------------------------------------------------
    // Minimum price floor
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_MinimumFloor_IsOne()
    {
        // Extremely cheap item with maximum discounts
        var item = MakeItem(1); // trade value = 1
        var price = TradeCalculator.CalculateBuyPrice(
            item, 0.01m, LawfulGood, LawfulGood, new List<Skill>(), _repository);

        Assert.True(price >= 1m, $"Buy price should never go below 1, got {price}");
    }

    [Fact]
    public void SellPrice_MinimumFloor_IsOne()
    {
        var item = MakeItem(1);
        var merchantAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        var price = TradeCalculator.CalculateSellPrice(
            item, Neutral, merchantAlignment, new List<Skill>(), _repository);

        Assert.True(price >= 1m, $"Sell price should never go below 1, got {price}");
    }

    // ---------------------------------------------------------------
    // Full scenario — all modifiers combined
    // ---------------------------------------------------------------

    [Fact]
    public void BuyPrice_AllModifiers_Combined()
    {
        // Superior Steel item, Chaotic Evil merchant, Good player with Journeyman Diplomacy
        var quality = new Quality { Id = 1, Name = "Superior", TradeValueMultiplier = 1.5m };
        var material = new Material { Id = 1, Name = "Steel", TradeValueMultiplier = 2.0m };
        var item = MakeItem(100, quality: quality, material: material);
        var playerAlignment = new Alignment(LawfulnessType.Neutral, DispositionType.Good);
        var merchantAlignment = ChaoticEvil;
        var skills = new List<Skill> { MakeDiplomacySkill("Journeyman") };

        var price = TradeCalculator.CalculateBuyPrice(
            item, 1.2m, playerAlignment, merchantAlignment, skills, _repository);

        // base = 100 * 1.5 * 2.0 = 300
        // markup = 300 * 1.2 = 360
        // alignment = 1.0 + 0.15 (evil) + 0.10 (chaotic) = 1.25
        // no sympathy (Good vs Evil, different disposition)
        // 360 * 1.25 = 450
        // diplomacy = 0.5 * 0.15 = 0.075 → 450 * 0.925 = 416.25 → 416
        Assert.Equal(416m, price);
    }
}
