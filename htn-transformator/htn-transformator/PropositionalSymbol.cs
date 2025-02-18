using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal struct PropositionalSymbol
    {
        private static int nextPropID = 0;
        private static Dictionary<string, int> nameToInt = new Dictionary<string, int>();

        public int PropID { get; private set; }
        public string Name { get; private set; }
        public  PropositionalSymbol(string name)
        {
            Name = name;

            if (nameToInt.ContainsKey(name))
            {
                PropID = nameToInt[name];
            }
            else
            {
                PropID = nextPropID++;
                nameToInt.Add(name, PropID);
            }
        }
    }
}
