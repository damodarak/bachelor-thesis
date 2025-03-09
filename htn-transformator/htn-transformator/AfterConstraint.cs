using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class AfterConstraint : SingleTaskStateConstraint
    {
        public AfterConstraint(PropositionalSymbol symbol, Task task) : base(symbol, task) { }
        public override string ToString()
        {
            return $"after({Task}:{Symbol.Name})";
        }
    }
}
