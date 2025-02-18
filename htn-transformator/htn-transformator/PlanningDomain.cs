using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class PlanningDomain
    {
        private List<Method> methods = new List<Method>();
        public PlanningDomain() { }
        public void AppendMethod(Method m)
        {
            throw new NotImplementedException(); // checks
        }
        public bool IsTotallyOrdered()
        {
            foreach (Method m in methods)
            {
                if (!m.IsTotallyOrdered()) return false;
            }

            return true;
        }
    }
}
