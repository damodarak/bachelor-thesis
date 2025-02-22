using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class PrimitiveTask : Task
    {
        public PrimitiveTask(string name, int index) : base(name, index) { }
        public PrimitiveTask(int id, int index) : base(id, index) { }
    }
}
