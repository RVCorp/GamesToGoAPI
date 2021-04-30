using System.Collections.Generic;
using System.Linq;

namespace GamesToGo.API.GameExecution
{
    public class EventParameter
    {
        public EventType Type { get; init; }

        public List<ArgumentParameter> Arguments { get; init; } = new List<ArgumentParameter>();
            
        public int Priority { get; init; }
            
        public ArgumentParameter Conditional { get; init; }

        public IReadOnlyList<ActionParameter> Actions { get; set; }

        public EventParameter Clone()
        {
            return new EventParameter
            {
                Type = Type,
                Arguments = Arguments.Select(arg => arg.Clone()).ToList(),
                Priority = Priority,
                Conditional = Conditional?.Clone(),
                Actions = Actions.Select(act => act.Clone()).ToList(),
            };
        }
    }
}