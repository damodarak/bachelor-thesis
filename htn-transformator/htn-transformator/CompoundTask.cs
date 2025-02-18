using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class CompoundTask : Task
    {
        public CompoundTask(string name, int index) : base(name, index) { }
    }
}
