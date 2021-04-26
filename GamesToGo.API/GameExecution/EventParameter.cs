using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public record EventParameter
    {
        public EventType Type { get; init; }

        public List<ArgumentParameter> Arguments { get; init; } = new List<ArgumentParameter>();
            
        public int Priority { get; init; }
            
        public ArgumentParameter Conditional { get; init; }

        public IReadOnlyList<ActionParameter> Actions { get; set; }
    }
}