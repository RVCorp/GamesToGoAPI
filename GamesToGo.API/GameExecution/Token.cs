using System;

namespace GamesToGo.API.GameExecution
{
    public class Token
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
                if(value < 0)
                    throw new InvalidOperationException(@$"Can't have less than 0 tokens (token type: {TypeID}");
            }
        }

        public Token CloneEmpty()
        {
            return new Token(TypeID);
        }

        public static Token operator +(Token a, Token b)
        {
            if(!a.Equals(b))
                throw new ArgumentException($"Can't add different Token types (received types {a.TypeID} & {b.TypeID})");
            a.Count += b.Count;
            return a;
        }

        public override bool Equals(object obj)
        {
            return obj is Token other && Equals(other);
        }

        private bool Equals(Token other)
        {
            return TypeID == other.TypeID;
        }

        public override int GetHashCode()
        {
            return TypeID;
        }
    }
}