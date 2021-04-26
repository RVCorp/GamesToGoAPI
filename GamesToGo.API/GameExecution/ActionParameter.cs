using System.Collections.Generic;
using System.Linq;

namespace GamesToGo.API.GameExecution
{
    public class ActionParameter
    {
        public ActionType Type { get; init; }

        public IList<ArgumentParameter> Arguments { get; init; } = new List<ArgumentParameter>();

        public ArgumentParameter Conditional { get; init; }

        public ActionParameter Clone()
        {
            return new ActionParameter
            {
                Type = Type,
                Arguments = Arguments.Select(arg => arg.Clone()).ToList(),
                Conditional = Conditional.Clone(),
            };
        }
    }
}