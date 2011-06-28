using System.Collections.Generic;

namespace Janthus.Model
{
    public interface IAttributeModifier
    {
        List<decimal> AttributeMultiplier { get; set; }
    }
}
