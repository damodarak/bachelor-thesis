using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class Method
    {
        public CompoundTask LeftSide { get; private set; }
        public List<CompoundTask> RightSideCompound { get; set; } = new List<CompoundTask>();
        public List<PrimitiveTask> RightSidePrimitive { get; set; } = new List<PrimitiveTask>();
        public List<OrderConstraint> Orderings { get; set; } = new List<OrderConstraint>();
        public List<BeforeConstraint> Befores { get; set; } = new List<BeforeConstraint>();
        public List<AfterConstraint> Afters { get; set; } = new List<AfterConstraint>();
        public List<BetweenConstraint> Betweens { get; set; } = new List<BetweenConstraint>();
        public Method(CompoundTask leftSide)
        {
            LeftSide = leftSide;
        }
        public bool IsTotallyOrdered()
        {
            int count = TaskCount();
            return Orderings.Count == ((count - 1) * count) / 2;
        }
        public bool IsEmpty()
        {
            return TaskCount() == 0;
        }
        public int TaskCount()
        {
            return RightSideCompound.Count + RightSidePrimitive.Count;
        }
        public int ConstraintsCount()
        {
            return Befores.Count + Afters.Count + Betweens.Count + Betweens.Count + Orderings.Count;
        }
    }
}
