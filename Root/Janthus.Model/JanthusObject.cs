using System;

namespace Janthus.Model
{
    public class JanthusObject
    {
        public int Id { get; set; }
        public Guid InternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
