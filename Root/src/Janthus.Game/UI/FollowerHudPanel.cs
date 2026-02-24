using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;
using Janthus.Game.Actors;
using Janthus.Game.Input;

namespace Janthus.Game.UI;

public class FollowerHudPanel : UIPanel
{
    private readonly List<FollowerController> _followers;

    private const int FollowerHeight = 45;
    private const int FollowerHeightWithMana = 65;
    private const int PaddingTop = 10;
    private const int PaddingX = 10;

    private static readonly Color BgColor = new(20, 20, 30, 200);
    private static readonly Color BorderColor = new(80, 80, 100);

    public FollowerHudPanel(Texture2D pixelTexture, SpriteFontBase font,
                            List<FollowerController> followers, Rectangle bounds)
        : base(pixelTexture, font, bounds)
    {
        _followers = followers;
        IsVisible = false;
    }

    public override void Update(GameTime gameTime, InputManager input)
    {
        // Show only when followers exist
        var shouldShow = _followers.Count > 0;
        if (shouldShow != IsVisible)
            IsVisible = shouldShow;

        if (IsVisible)
        {
            // Adjust height dynamically (65px for followers with mana, 45px otherwise)
            var totalHeight = PaddingTop;
            foreach (var follower in _followers)
            {
                var actor = follower.Sprite.DomainActor;
                var hasMana = !actor.Status.Equals(ActorStatus.Dead) && actor is LeveledActor la && la.MaximumMana > 0;
                totalHeight += hasMana ? FollowerHeightWithMana : FollowerHeight;
            }
            Bounds = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, totalHeight);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible || _followers.Count == 0) return;

        DrawPanel(spriteBatch, BgColor, BorderColor);

        var x = Bounds.X + PaddingX;
        var y = Bounds.Y + PaddingTop;

        foreach (var follower in _followers)
        {
            var actor = follower.Sprite.DomainActor;
            var isDead = actor.Status == ActorStatus.Dead;
            var nameColor = isDead ? Color.DarkGray : Color.White;

            // Name + Level
            var followerLevel = "";
            if (actor is LeveledActor la)
                followerLevel = $" Lv.{ExperienceCalculator.CalculateLevelFromExperience(la.ExperiencePoints)}";
            var nameText = isDead ? $"{follower.Sprite.Label} (Dead)" : $"{follower.Sprite.Label}{followerLevel}";
            spriteBatch.DrawString(Font, nameText, new Vector2(x, y), nameColor);
            y += 20;

            if (!isDead && actor is LeveledActor leveled)
            {
                // HP bar
                var maxHp = leveled.MaximumHitPoints;
                var currentHp = (double)leveled.CurrentHitPoints;
                var hpFill = maxHp > 0 ? (float)(currentHp / maxHp) : 0;

                spriteBatch.DrawString(Font, "HP", new Vector2(x, y), Color.Red);
                DrawBar(spriteBatch, new Rectangle(x + 50, y + 2, 200, 12), hpFill, Color.Red, new Color(60, 20, 20));
                spriteBatch.DrawString(Font, $"{(int)currentHp}/{(int)maxHp}", new Vector2(x + 260, y), Color.White);
                y += 20;

                // Mana bar (only for magic-capable followers)
                if (leveled.MaximumMana > 0)
                {
                    var maxMp = leveled.MaximumMana;
                    var currentMp = (double)leveled.CurrentMana;
                    var mpFill = maxMp > 0 ? (float)(currentMp / maxMp) : 0;

                    spriteBatch.DrawString(Font, "MP", new Vector2(x, y), Color.CornflowerBlue);
                    DrawBar(spriteBatch, new Rectangle(x + 50, y + 2, 200, 12), mpFill, Color.CornflowerBlue, new Color(20, 20, 60));
                    spriteBatch.DrawString(Font, $"{(int)currentMp}/{(int)maxMp}", new Vector2(x + 260, y), Color.White);
                    y += 20;
                }
            }

            y += 5;
        }
    }
}
