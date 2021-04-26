using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public record ElementParameter
    {
        public string Name { get; init; }
            
        public string Value { get; init; }

        public List<string> ExtraLines { get; } = new List<string>();
    }
}