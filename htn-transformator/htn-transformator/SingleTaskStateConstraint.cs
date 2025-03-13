using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Represents StateConstraints that targets only one task.
    /// </summary>
    abstract class SingleTaskStateConstraint : StateConstraint
    {
        public Task Task { get; protected set; }
        public SingleTaskStateConstraint(PropositionalSymbol ps, Task t)
        {
            Symbol = ps;
            Task = t;
        }
    }
}
