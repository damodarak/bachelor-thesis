using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal struct PropositionalSymbol : IEquatable<PropositionalSymbol>
    {
        private static int nextPropID = 0;
        private static Dictionary<string, int> nameToInt = new Dictionary<string, int>();
        private static Dictionary<int, string> idToName = new Dictionary<int, string>();
        public int ID { get; private set; }
        public string Name { get; private set; }
        public  PropositionalSymbol(string name)
        {
            Name = name;

            if (nameToInt.ContainsKey(name))
            {
                ID = nameToInt[name];
            }
            else
            {
                ID = nextPropID++;
                nameToInt.Add(name, ID);
                idToName.Add(ID, name);
            }
        }
        public PropositionalSymbol(int id)
        {
            ID = id;
            Name = idToName[id];
        }
        public bool Equals(PropositionalSymbol other)
        {
            return ID == other.ID;
        }
        public override bool Equals(object? obj)
        {
            return obj is PropositionalSymbol other && Equals(other);
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public static bool operator ==(PropositionalSymbol left, PropositionalSymbol right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PropositionalSymbol left, PropositionalSymbol right)
        {
            return !left.Equals(right);
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
