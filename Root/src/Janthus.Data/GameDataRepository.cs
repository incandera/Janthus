using Microsoft.EntityFrameworkCore;
using Janthus.Model.Entities;
using Janthus.Model.Services;

namespace Janthus.Data;

public class GameDataRepository : IGameDataProvider
{
    private readonly JanthusDbContext _context;

    private List<ActorType> _actorTypes;
    private List<CharacterClass> _classes;
    private List<ActorLevel> _levels;
    private List<SkillType> _skillTypes;
    private List<SkillLevel> _skillLevels;

    public GameDataRepository(JanthusDbContext context)
    {
        _context = context;
    }

    public void EnsureCreated()
    {
        _context.Database.EnsureCreated();
    }

    public List<ActorType> GetActorTypes()
    {
        _actorTypes ??= _context.ActorTypes.OrderBy(x => x.Name).ToList();
        return _actorTypes;
    }

    public List<Actor> GetBestiary()
    {
        return new List<Actor>();
    }

    public List<CharacterClass> GetClasses()
    {
        _classes ??= _context.CharacterClasses.OrderBy(x => x.Name).ToList();
        return _classes;
    }

    public CharacterClass GetClass(string name)
    {
        return GetClasses().SingleOrDefault(x => x.Name == name);
    }

    public List<ActorLevel> GetLevels()
    {
        _levels ??= _context.ActorLevels.OrderBy(x => x.Number).ToList();
        return _levels;
    }

    public ActorLevel GetLevel(int number)
    {
        return GetLevels().SingleOrDefault(x => x.Number == number);
    }

    public ActorLevel CalculateLevel(int sumOfAttributes)
    {
        var levels = GetLevels();
        var levelIndex = levels.FindIndex(x => x.MinimumSumOfAttributes > sumOfAttributes);

        if (levelIndex <= 0)
            return levels.Last();

        return levels[levelIndex - 1];
    }

    public List<SkillLevel> GetSkillLevels()
    {
        _skillLevels ??= _context.SkillLevels.OrderBy(x => x.Name).ToList();
        return _skillLevels;
    }

    public List<SkillType> GetSkillTypes()
    {
        _skillTypes ??= _context.SkillTypes.OrderBy(x => x.Name).ToList();
        return _skillTypes;
    }
}
