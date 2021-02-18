using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public record ArgumentParameter
    {
        public ArgumentType Type { get; init; }

        public List<ArgumentParameter> Arguments { get; init; }

        public int? Result { get; init; }
    }
}