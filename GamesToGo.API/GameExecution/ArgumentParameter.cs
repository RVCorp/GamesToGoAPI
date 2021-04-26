
using System.Collections.Generic;
using System.Linq;

namespace GamesToGo.API.GameExecution
{
    public class ArgumentParameter
    {
        public ArgumentType Type { get; init; }

        public List<ArgumentParameter> Arguments { get; init; } = new List<ArgumentParameter>();

        public List<int> Result { get; init; } = new List<int>();

        public ArgumentParameter Clone()
        {
            return new ArgumentParameter
            {
                Type = Type,
                Arguments = Arguments.Select(arg => arg.Clone()).ToList(),
                Result = Result.Select(res => res).ToList(),
            };
        }
    }
}