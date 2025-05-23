﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    /// <summary>
    /// Struct representing a name of a Task. There is a one-to-one mapping of IDs and Names.
    /// </summary>
    internal struct TaskName : IEquatable<TaskName>
    {
        private static int nextTaskNameID = 0;
        private static Dictionary<string, int> nameToID = new();
        private static Dictionary<int, string> idToTaName = new();
        public string Name { get { return idToTaName[ID]; } } // to save up memory
        public int ID { get; init; }
        public TaskName(string name)
        {
            if (nameToID.ContainsKey(name))
            {
                ID = nameToID[name];
            }
            else
            {
                ID = nextTaskNameID++;
                nameToID.Add(name, ID);
                idToTaName.Add(ID, name);
            }
        }
        public TaskName(int taskNameID)
        {
            if (!idToTaName.ContainsKey(taskNameID)) throw new Exception("Creating TaskName with non-existing TaskNameID!");

            ID = taskNameID;
        }
        public bool Equals(TaskName other)
        {
            return ID == other.ID;
        }
        public override bool Equals(object? obj)
        {
            return obj is TaskName other && Equals(other);
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public static bool operator ==(TaskName left, TaskName right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TaskName left, TaskName right)
        {
            return !left.Equals(right);
        }
        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
