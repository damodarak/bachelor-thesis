using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class ToGNF : ITransformable
    {
        private PlanningDomain domain;
        public ToGNF(PlanningDomain pd)
        {
            domain = pd;
        }
        public PlanningDomain Transform()
        {
            throw new NotImplementedException();
        }
    }
}
