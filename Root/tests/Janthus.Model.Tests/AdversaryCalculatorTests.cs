using Xunit;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;

namespace Janthus.Model.Tests;

public class AdversaryCalculatorTests
{
    // ---------------------------------------------------------------
    // Base disposition rules (ignoring flip chance)
    // ---------------------------------------------------------------

    [Fact]
    public void GoodVsEvil_BaseAdversary()
    {
        // Good vs Evil should be adversaries (base), but lawfulness can flip
        // Use Lawful vs Lawful (25% flip). Pick a seed that does NOT flip.
        var good = new Alignment(LawfulnessType.Neutral, DispositionType.Good);
        var evil = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        // Neutral vs Neutral lawfulness = 0% flip chance
        var result = AdversaryCalculator.IsAdversary(good, evil, 42);

        Assert.True(result);
    }

    [Fact]
    public void EvilVsGood_BaseAdversary()
    {
        var evil = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);
        var good = new Alignment(LawfulnessType.Neutral, DispositionType.Good);

        var result = AdversaryCalculator.IsAdversary(evil, good, 42);

        Assert.True(result);
    }

    [Fact]
    public void GoodVsGood_NotAdversary()
    {
        var good1 = new Alignment(LawfulnessType.Neutral, DispositionType.Good);
        var good2 = new Alignment(LawfulnessType.Neutral, DispositionType.Good);

        var result = AdversaryCalculator.IsAdversary(good1, good2, 42);

        Assert.False(result);
    }

    [Fact]
    public void EvilVsEvil_NotAdversary()
    {
        var evil1 = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);
        var evil2 = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        var result = AdversaryCalculator.IsAdversary(evil1, evil2, 42);

        Assert.False(result);
    }

    [Theory]
    [InlineData(DispositionType.Good)]
    [InlineData(DispositionType.Evil)]
    [InlineData(DispositionType.Neutral)]
    public void NeutralDisposition_NeverBaseAdversary(DispositionType otherDisposition)
    {
        var neutral = new Alignment(LawfulnessType.Neutral, DispositionType.Neutral);
        var other = new Alignment(LawfulnessType.Neutral, otherDisposition);

        // Neutral vs anything = not adversary (base), and Neutral lawfulness = 0% flip
        var result = AdversaryCalculator.IsAdversary(neutral, other, 42);

        Assert.False(result);
    }

    [Theory]
    [InlineData(DispositionType.Good)]
    [InlineData(DispositionType.Evil)]
    [InlineData(DispositionType.Neutral)]
    public void AnyVsNeutralDisposition_NeverBaseAdversary(DispositionType selfDisposition)
    {
        var self = new Alignment(LawfulnessType.Neutral, selfDisposition);
        var neutral = new Alignment(LawfulnessType.Neutral, DispositionType.Neutral);

        var result = AdversaryCalculator.IsAdversary(self, neutral, 42);

        Assert.False(result);
    }

    // ---------------------------------------------------------------
    // Flip chance — lawfulness interactions
    // ---------------------------------------------------------------

    [Fact]
    public void ChaoticVsChaotic_50PercentFlipChance()
    {
        // Good vs Good + Chaotic vs Chaotic: base=false, 50% flip
        var a = new Alignment(LawfulnessType.Chaotic, DispositionType.Good);
        var b = new Alignment(LawfulnessType.Chaotic, DispositionType.Good);

        // Over many seeds, roughly half should flip to adversary
        int adversaryCount = 0;
        int trials = 1000;
        for (int seed = 0; seed < trials; seed++)
        {
            if (AdversaryCalculator.IsAdversary(a, b, seed))
                adversaryCount++;
        }

        double rate = (double)adversaryCount / trials;
        Assert.True(rate > 0.40 && rate < 0.60,
            $"Chaotic vs Chaotic should flip ~50% of the time, got {rate:P1}");
    }

    [Fact]
    public void ChaoticVsLawful_25PercentFlipChance()
    {
        // Good vs Evil + Chaotic vs Lawful: base=true (adversary), 25% flip
        var chaotic = new Alignment(LawfulnessType.Chaotic, DispositionType.Good);
        var lawful = new Alignment(LawfulnessType.Lawful, DispositionType.Evil);

        int notAdversaryCount = 0;
        int trials = 1000;
        for (int seed = 0; seed < trials; seed++)
        {
            if (!AdversaryCalculator.IsAdversary(chaotic, lawful, seed))
                notAdversaryCount++;
        }

        double flipRate = (double)notAdversaryCount / trials;
        Assert.True(flipRate > 0.15 && flipRate < 0.35,
            $"Chaotic vs Lawful should flip ~25% of the time, got {flipRate:P1}");
    }

    [Fact]
    public void LawfulVsLawful_25PercentFlipChance()
    {
        // Good vs Good + Lawful vs Lawful: base=false, 25% flip to adversary
        var a = new Alignment(LawfulnessType.Lawful, DispositionType.Good);
        var b = new Alignment(LawfulnessType.Lawful, DispositionType.Good);

        int adversaryCount = 0;
        int trials = 1000;
        for (int seed = 0; seed < trials; seed++)
        {
            if (AdversaryCalculator.IsAdversary(a, b, seed))
                adversaryCount++;
        }

        double rate = (double)adversaryCount / trials;
        Assert.True(rate > 0.15 && rate < 0.35,
            $"Lawful vs Lawful should flip ~25% of the time, got {rate:P1}");
    }

    [Fact]
    public void NeutralLawfulnessVsNeutralLawfulness_ZeroFlipChance()
    {
        // Good vs Evil + Neutral vs Neutral: base=true, 0% flip
        var a = new Alignment(LawfulnessType.Neutral, DispositionType.Good);
        var b = new Alignment(LawfulnessType.Neutral, DispositionType.Evil);

        // Should always be adversary regardless of seed
        for (int seed = 0; seed < 100; seed++)
        {
            Assert.True(AdversaryCalculator.IsAdversary(a, b, seed),
                $"Neutral lawfulness should have 0% flip, but flipped at seed {seed}");
        }
    }

    // ---------------------------------------------------------------
    // Determinism — same seed, same result
    // ---------------------------------------------------------------

    [Fact]
    public void SameSeed_SameResult()
    {
        var a = new Alignment(LawfulnessType.Chaotic, DispositionType.Good);
        var b = new Alignment(LawfulnessType.Chaotic, DispositionType.Evil);

        var result1 = AdversaryCalculator.IsAdversary(a, b, 12345);
        var result2 = AdversaryCalculator.IsAdversary(a, b, 12345);

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void DifferentSeeds_CanProduceDifferentResults()
    {
        var a = new Alignment(LawfulnessType.Chaotic, DispositionType.Good);
        var b = new Alignment(LawfulnessType.Chaotic, DispositionType.Evil);

        // With 50% flip chance, different seeds should produce different results eventually
        bool seenTrue = false, seenFalse = false;
        for (int seed = 0; seed < 100; seed++)
        {
            var result = AdversaryCalculator.IsAdversary(a, b, seed);
            if (result) seenTrue = true;
            else seenFalse = true;
            if (seenTrue && seenFalse) break;
        }

        Assert.True(seenTrue && seenFalse,
            "Different seeds should produce both adversary and non-adversary results");
    }
}
