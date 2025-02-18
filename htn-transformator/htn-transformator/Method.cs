using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class Method
    {
        public CompoundTask leftSide { get; private set; }
        public List<CompoundTask> rightSideCompound { get; set; } = new List<CompoundTask>();
        public List<PrimitiveTask> rightSidePrimitive { get; set; } = new List<PrimitiveTask>();
        public List<OrderConstraint> Orderings { get; set; } = new List<OrderConstraint>();
        public List<BeforeConstraint> Befores { get; set; } = new List<BeforeConstraint>();
        public List<AfterConstraint> Afters { get; set; } = new List<AfterConstraint>();
        public List<BetweenConstraint> Betweens { get; set; } = new List<BetweenConstraint>();
        public Method(CompoundTask leftSide)
        {
            this.leftSide = leftSide;
        }
        public bool IsTotallyOrdered()
        {
            throw new NotImplementedException();
        }
    }
}
