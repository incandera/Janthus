using Janthus.Model.Entities;

namespace Janthus.Model.Services;

public interface IGameDataProvider
{
    List<ActorType> GetActorTypes();
    List<Actor> GetBestiary();
    List<CharacterClass> GetClasses();
    CharacterClass GetClass(string name);
    List<ActorLevel> GetLevels();
    ActorLevel GetLevel(int number);
    ActorLevel CalculateLevel(int sumOfAttributes);
    List<SkillLevel> GetSkillLevels();
    List<SkillType> GetSkillTypes();
}
