using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class BetweenConstraint : StateConstraint
    {
        public Task FromTask { get; private set; }
        public Task ToTask { get; private set; }
        public BetweenConstraint(PropositionalSymbol symbol, Task from, Task to)
        {
            if (from == to) throw new Exception("Between constraints must target different tasks!");

            Symbol = symbol;
            FromTask = from;
            ToTask = to;
        }
        public override string ToString()
        {
            return $"between({FromTask}:{Symbol.Name}:{ToTask})";
        }
    }
}
