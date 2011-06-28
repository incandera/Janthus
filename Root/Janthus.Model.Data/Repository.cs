using System;
using System.Collections.Generic;
using System.Linq;

namespace Janthus.Model.Data
{
    public class Repository : IDisposable
    {
        #region Properties...

        public JanthusModelEntities JanthusModelEntities { get; private set; }

        #endregion

        #region Constructors...

        public Repository()
        {
            if (Equals(JanthusModelEntities, null))
            {
                JanthusModelEntities = new JanthusModelEntities();
            }
        }

        #endregion

        #region Methods...

        public List<ActorType> GetActorTypes()
        {
            var query = from r in JanthusModelEntities.ActorTypes
                        orderby r.Name
                        select r;

            return query.ToList();
        }

        public List<Bestiary> GetBestiary()
        {
            var query = from r in JanthusModelEntities.Bestiaries
                        orderby r.Name
                        select r;

            return query.ToList();
        }

        public List<Class> GetClasses()
        {
            var query = from r in JanthusModelEntities.Classes
                        orderby r.Name
                        select r;

            return query.ToList();   
        }

        public Class GetClass(string name)
        {
            var query = from r in JanthusModelEntities.Classes
                        where r.Name == name
                        select r;

            return query.SingleOrDefault();
        }

        public List<Level> GetLevels()
        {
            var query = from r in JanthusModelEntities.Levels
                        orderby r.Number
                        select r;

            return query.ToList();                   
        }

        public Level GetLevel(int number)
        {
            var query = from r in JanthusModelEntities.Levels
                        where r.Number == number
                        select r;

            return query.SingleOrDefault();
        }

        public Level GetLevelBySumOfAttributes(int sumOfAttributes)
        {
            var query = from r in JanthusModelEntities.Levels
                        where r.MinimumSumOfAttributes >= sumOfAttributes
                        select r;

            return query.FirstOrDefault(); // Just return the first one which matches
        }

        public  List<SkillType> GetSkillTypes()
        {
            var query = from r in JanthusModelEntities.SkillTypes
                        orderby r.Name
                        select r;

            return query.ToList();
        }

        public List<SkillLevel> GetSkillLevels()
        {
            var query = from r in JanthusModelEntities.SkillLevels
                        orderby r.Name
                        select r;

            return query.ToList();   
        }

        #endregion

        #region IDisposable Implementation...

        public void Dispose()
        {
            if (!Equals(JanthusModelEntities, null))
            {
                JanthusModelEntities = null;
            }
        }

        #endregion
    }
}
