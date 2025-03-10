using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Class representing between constraint targeting two tasks in a single method.
    /// In each valid plan the PropositiolSymbol Symbol must hold after the last PrimitiveTask to which Task FromTask decomposes
    /// until the previous PrimitiveTask to which Task ToTask decomposes.
    /// </summary>
    internal class BetweenConstraint : StateConstraint
    {
        public Task FromTask { get; private set; }
        public Task ToTask { get; private set; }
        /// <summary>
        /// Tasks from and to cannot be identical.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <exception cref="Exception"></exception>
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
