using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class BeforeConstraint : StateConstraint
    {
        public Task Task { get; private set; }
        public BeforeConstraint(PropositionalSymbol symbol, Task task)
        {
            Symbol = symbol;
            Task = task;
        }
        public override string ToString()
        {
            return $"before({Symbol.Name}:{Task})";
        }
    }
}
