using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class RemoveBetween : ITransformable
    {
        private PlanningDomain domain;
        public RemoveBetween(PlanningDomain pd) 
        {
            domain = pd;
        }
        public PlanningDomain Transform()
        {
            throw new NotImplementedException();
        }
    }
}
