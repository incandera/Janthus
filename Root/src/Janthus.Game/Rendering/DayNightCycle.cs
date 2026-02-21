using Microsoft.Xna.Framework;

namespace Janthus.Game.Rendering;

public class DayNightCycle
{
    public float TimeOfDay { get; set; } = 10f; // hours [0, 24)
    public float TimeScale { get; set; } = 60f; // 1 real second = 60 game seconds = 1 game minute

    // Ambient color keyframes: (hour, color)
    private static readonly (float hour, Color color)[] Keyframes =
    {
        (0f,  new Color(20, 20, 60)),    // Midnight — dark blue
        (5f,  new Color(30, 25, 50)),    // Pre-dawn — deep indigo
        (6f,  new Color(180, 120, 80)),  // Dawn — warm orange
        (8f,  new Color(220, 210, 200)), // Morning — warm white
        (12f, new Color(240, 240, 235)), // Noon — near-white
        (16f, new Color(230, 220, 200)), // Afternoon — warm
        (18f, new Color(200, 130, 80)),  // Dusk — orange/red
        (20f, new Color(40, 30, 60)),    // Twilight — purple
        (24f, new Color(20, 20, 60)),    // Midnight wrap
    };

    public void Update(float deltaTimeSeconds)
    {
        TimeOfDay += deltaTimeSeconds * TimeScale / 3600f;
        if (TimeOfDay >= 24f)
            TimeOfDay -= 24f;
    }

    public Color GetAmbientColor()
    {
        var t = TimeOfDay;

        for (int i = 0; i < Keyframes.Length - 1; i++)
        {
            var (h0, c0) = Keyframes[i];
            var (h1, c1) = Keyframes[i + 1];

            if (t >= h0 && t < h1)
            {
                var lerp = (t - h0) / (h1 - h0);
                return Color.Lerp(c0, c1, lerp);
            }
        }

        return Keyframes[0].color;
    }

    public float GetVisionRadiusMultiplier()
    {
        // Full vision during day, reduced at night
        var t = TimeOfDay;
        if (t >= 7f && t <= 17f) return 1.0f;
        if (t >= 20f || t <= 4f) return 0.5f;

        // Transition zones
        if (t > 4f && t < 7f) return 0.5f + 0.5f * ((t - 4f) / 3f);
        if (t > 17f && t < 20f) return 1.0f - 0.5f * ((t - 17f) / 3f);

        return 1.0f;
    }
}
