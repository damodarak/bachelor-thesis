using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class OrderConstraint : Constraint
    {
        public Task first { get; private set; }
        public Task second { get; private set; }
        public OrderConstraint(Task first, Task second)
        {
            if (first == second) throw new Exception("Ordering constraints must target different tasks!");

            this.first = first;
            this.second = second;
        }
        public override string ToString()
        {
            return $"{first}<{second}";
        }
    }
}
