using System;
using System.Collections.Generic;
using System.Linq;
//
using Janthus.Model.Data;

namespace Janthus.Model
{
    public class DataProvider
    {
        #region Fields...

        private static List<ActorType> _actorTypes;
        private static List<Actor> _bestiary;
        private static List<Class> _classes;
        private static List<ActorLevel> _levels;
        private static List<SkillLevel> _skillLevels;
        private static List<SkillType> _skillTypes;

        #endregion

        #region Properties...

        public static List<ActorType> ActorTypes
        {
            get
            {
                if (Equals(_actorTypes, null))
                {
                    using (var repository = new Repository())
                    {
                        _actorTypes = new List<ActorType>();

                        var actorTypeQuery = repository.GetActorTypes();

                        foreach (var actorType in actorTypeQuery)
                        {
                            var domainActorType = new ActorType();

                            ToDomain(actorType, domainActorType);

                            _actorTypes.Add(domainActorType);
                        }
                    }
                }

                return _actorTypes;
            }
        }

        public static List<Actor> Bestiary
        {
            get
            {
                if (Equals(_bestiary, null))
                {
                    using (var repository = new Repository())
                    {
                        _bestiary = new List<Actor>();

                        var actorQuery = repository.GetBestiary();

                        foreach (var actor in actorQuery)
                        {
                            var currentActor = actor;
                            var domainActor = new Actor();
                            var domainActorType = ActorTypes.Where(x => Equals(x.Name, currentActor.ActorType.Name)).SingleOrDefault();

                            ToDomain(actor, domainActor);

                            domainActor.Type = domainActorType;

                            _bestiary.Add(domainActor);
                        }
                    }
                }

                return _bestiary;
            }
        }

        public static List<Class> Classes
        {
            get
            {
                if (Equals(_classes, null))
                {
                    using (var repository = new Repository())
                    {
                        _classes = new List<Class>();

                        var classQuery = repository.GetClasses();

                        foreach (var entityClass in classQuery)
                        {
                            var domainClass = new Class();

                            ToDomain(entityClass, domainClass);

                            _classes.Add(domainClass);
                        }
                    }
                }

                return _classes;
            }
        }

        public static List<ActorLevel> Levels
        {
            get
            {
                if (Equals(_levels, null))
                {
                    using (var repository = new Repository())
                    {
                        _levels = new List<ActorLevel>();

                        var levelQuery = repository.GetLevels();

                        foreach (var level in levelQuery)
                        {
                            var domainLevel = new ActorLevel();

                            ToDomain(level, domainLevel);

                            _levels.Add(domainLevel);
                        }
                    }
                }

                return _levels;
            }
        }

        public static List<SkillLevel> SkillLevels
        {
            get
            {
                if (Equals(_skillLevels, null))
                {
                    using (var repository = new Repository())
                    {
                        _skillLevels = new List<SkillLevel>();

                        var skillLevelQuery = repository.GetSkillLevels();

                        foreach (var skillLevel in skillLevelQuery)
                        {
                            var domainSkillLevel = new SkillLevel();

                            ToDomain(skillLevel, domainSkillLevel);

                            _skillLevels.Add(domainSkillLevel);
                        }
                    }
                }

                return _skillLevels;
            }
        }

        public static List<SkillType> SkillTypes
        {
            get
            {
                if (Equals(_skillTypes, null))
                {
                    using (var repository = new Repository())
                    {
                        _skillTypes = new List<SkillType>();

                        var skillTypeQuery = repository.GetSkillTypes();

                        foreach (var skillType in skillTypeQuery)
                        {
                            var domainSkillType = new SkillType();

                            ToDomain(skillType, domainSkillType);

                            _skillTypes.Add(domainSkillType);
                        }
                    }
                }

                return _skillTypes;
            }
        }

        #endregion

        #region Methods...

        public static void ToDomain(object source, object target)
        {
            var sourceType = source.GetType();
            var sourceProperties = sourceType.GetProperties();
            var targetType = target.GetType();
            var targetProperties = targetType.GetProperties();

            foreach (var property in sourceProperties)
            {
                var currentProperty = property;

                if (!targetProperties.Any(x => x.Name == currentProperty.Name)) { continue; }

                var targetProperty = targetType.GetProperty(property.Name);

                if (targetProperty.PropertyType == typeof(string)) { targetProperty.SetValue(target, property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(int)) { targetProperty.SetValue(target, (int)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(short)) { targetProperty.SetValue(target, (short)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(decimal)) { targetProperty.SetValue(target, (decimal)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(double)) { targetProperty.SetValue(target, (double)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(float)) { targetProperty.SetValue(target, (float)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(bool)) { targetProperty.SetValue(target, (bool)property.GetValue(source, null), null); }
                if (targetProperty.PropertyType == typeof(Guid)) { targetProperty.SetValue(target, new Guid(property.GetValue(source, null).ToString()), null); }
                if (targetProperty.PropertyType == typeof(DateTime)) { targetProperty.SetValue(target, (DateTime)property.GetValue(source, null), null); }
            }
        }

        #endregion
    }
}
