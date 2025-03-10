using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Class representing after constraint targeting a single task in a single method.
    /// In each valid plan the PropositiolSymbol Symbol must hold after the last PrimitiveTask to which Task decomposes.
    /// </summary>
    internal class AfterConstraint : SingleTaskStateConstraint
    {
        public AfterConstraint(PropositionalSymbol symbol, Task task) : base(symbol, task) { }
        public override string ToString()
        {
            return $"after({Task}:{Symbol.Name})";
        }
    }
}
