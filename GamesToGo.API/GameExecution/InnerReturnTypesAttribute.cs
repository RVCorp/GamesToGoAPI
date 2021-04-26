using System;

namespace GamesToGo.API.GameExecution
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InnerReturnTypesAttribute : Attribute
    {
        public ArgumentReturnType[] InnerReturnTypes { get; }
        public InnerReturnTypesAttribute(ArgumentReturnType[] returnTypes)
        {
            InnerReturnTypes = returnTypes;
        }
    }
}