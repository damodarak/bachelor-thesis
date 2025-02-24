using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
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
                    pd.Methods[i].Head.TaskName.ID == pd.Methods[i].RightSideCompound[0].TaskName.ID)
                {
                    pd.Methods.RemoveAt(i);
                    i--;
                }
            }

            return pd;
        }
    }
}
