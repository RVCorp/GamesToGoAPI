using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public record EventParameter
    {
        public EventType Type { get; init; }

        public IReadOnlyList<ArgumentParameter> Arguments { get; init; }
            
        public int Priority { get; init; }
            
        public ArgumentParameter Conditional { get; init; }

        public IReadOnlyList<ActionParameter> Actions { get; init; }
    }
}