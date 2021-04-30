using System;

namespace GamesToGo.API.GameExecution
{
    public class Token : IEquatable<Token>
    {
        public int TypeID { get; }

        private int count = 0;
        
        public Privacy Privacy { get; set; }

        public Token(int typeID)
        {
            TypeID = typeID;
        }

        public int Count
        {
            get => count;
            set
            {
                if (value < 0)
                    throw new InvalidOperationException(@$"Can't have less than 0 tokens (token type: {TypeID}");
            }
        }

        public Token Clone()
        {
            return new Token(TypeID)
            {
                Privacy = Privacy,
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Token other && Equals(other);
        }

        public bool Equals(Token other)
        {
            return TypeID == other?.TypeID;
        }

        public override int GetHashCode()
        {
            return TypeID;
        }
    }
}