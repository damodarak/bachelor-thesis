using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Representing the concrete CompoundTask of the concrete Method in a PlanningDomain.
    /// </summary>
    internal class CompoundTask : Task
    {
        public CompoundTask(string name, int index) : base(name, index) { }
        public CompoundTask(int id, int index) : base(id, index) { }
        public CompoundTask(Task father, HashSet<PropositionalSymbol> symbols, int index) : base(father, symbols, index) { }
        public CompoundTask(TaskName tn, int index) : base(tn, index) { }
    }
}
