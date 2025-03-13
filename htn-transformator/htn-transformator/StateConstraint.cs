using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Represents abstract class that holds a PropositionalSymbol that needs to hold for a valid plan.
    /// </summary>
    internal abstract class StateConstraint : Constraint
    {
        /// <summary>
        /// Is set in the child Constructors.
        /// </summary>
        public PropositionalSymbol Symbol { get; protected set; }
    }
}
