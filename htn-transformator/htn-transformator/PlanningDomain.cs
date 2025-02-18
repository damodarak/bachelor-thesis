using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class PlanningDomain
    {
        private List<Method> methods = new List<Method>();
        public List<Method> Methods { get { return methods; } }
        public PlanningDomain() { }
        public void AppendMethod(Method m)
        {
            if (m.IsEmpty() && m.ConstraintsCount() > 0)
            {
                throw new Exception("Empty methods cannot have any constraints!");
            }

            if (m.RightSidePrimitive.Count == 0 &&
                m.TaskCount() == 1 &&
                m.LeftSide.TaskName.TaskNameID == m.RightSideCompound[0].TaskName.TaskNameID)
            {
                return; // we can discard unit methods even though there are state constraints
            }

            methods.Add(m);
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
