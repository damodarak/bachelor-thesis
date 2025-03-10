using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Each transformation class must implement this interface.
    /// </summary>
    internal interface ITransformable
    {
        public PlanningDomain Transform();
    }
}
