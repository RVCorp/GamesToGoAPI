using System.Collections.Generic;

namespace GamesToGo.API.GameExecution
{
    public record ActionParameter
    {
    public ActionType Type { get; init; }

    public IReadOnlyList<ArgumentParameter> Arguments { get; init; }
            
    public ArgumentParameter Conditional { get; init; }
    }
}