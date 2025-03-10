using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Class representing before constraint targeting a single task in a single method.
    /// In each valid plan the PropositiolSymbol Symbol must hold before the first PrimitiveTask to which Task decomposes.
    /// </summary>
    internal class BeforeConstraint : SingleTaskStateConstraint
    {      
        public BeforeConstraint(PropositionalSymbol symbol, Task task) : base(symbol, task) { }
        public override string ToString()
        {
            return $"before({Symbol.Name}:{Task})";
        }
    }
}
