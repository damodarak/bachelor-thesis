using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal abstract class StateConstraint : Constraint
    {
        public PropositionalSymbol Symbol { get; protected set; }
    }
}
