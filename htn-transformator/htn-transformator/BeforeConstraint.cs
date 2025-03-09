using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class BeforeConstraint : SingleTaskStateConstraint
    {
        public BeforeConstraint(PropositionalSymbol symbol, Task task) : base(symbol, task) { }
        public override string ToString()
        {
            return $"before({Symbol.Name}:{Task})";
        }
    }
}
