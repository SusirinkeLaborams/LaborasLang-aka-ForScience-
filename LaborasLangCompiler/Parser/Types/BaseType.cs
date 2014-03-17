using System;
namespace LaborasLangCompiler.Parser.Types
{
    abstract class BaseType
    {
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }
}