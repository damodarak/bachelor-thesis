using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class AfterConstraint : StateConstraint
    {
        public Task Task { get; private set; }
        public AfterConstraint(PropositionalSymbol symbol, Task task)
        {
            Symbol = symbol;
            Task = task;
        }
        public override string ToString()
        {
            return $"after({Task}:{Symbol.Name})";
        }
    }
}
