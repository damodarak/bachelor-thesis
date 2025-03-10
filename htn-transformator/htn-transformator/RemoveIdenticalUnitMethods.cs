using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    // This transformation is implicitly used in the domain parsing
    internal class RemoveIdenticalUnitMethods : ITransformable
    {
        private PlanningDomain pd;
        public RemoveIdenticalUnitMethods(PlanningDomain d)
        {
            pd = d;
        }
        public PlanningDomain Transform()
        {
            for (int i = 0; i < pd.Methods.Count; i++)
            {
                if (pd.Methods[i].TaskCount() == 1 && pd.Methods[i].RightSideCompound.Count == 1 &&
                    pd.Methods[i].Head.TaskName == pd.Methods[i].RightSideCompound[0].TaskName)
                {
                    pd.RemoveMethod(pd.Methods[i]);
                    i--;
                }
            }

            return pd;
        }
    }
}
