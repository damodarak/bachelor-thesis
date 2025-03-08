using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace htn_transformator
{
    class RemoveUnitMethods : ITransformable
    {
        private struct ConstraintHelper
        {
            public PropositionalSymbol ps;
            public Type type; // AfterConstraint/BeforeConstraint
        }

        private PlanningDomain d;
        private Dictionary<TaskName, Dictionary<TaskName, List<HashSet<ConstraintHelper>>>> unitsConstraints; // nullifies(Compound1,Compound2) = {{constr, ...  }, ... }
        private HashSet<ValueTuple<TaskName, TaskName>> toBeSearched;
        public RemoveUnitMethods(PlanningDomain d)
        {
            this.d = d;
            unitsConstraints = new();
            toBeSearched = new();
        }
        public PlanningDomain Transform()
        {
            foreach (var m in d.Methods)
            {
                unitMethodBase(m);
            }

            while (toBeSearched.Count != 0)
            {
                var enumerator = toBeSearched.GetEnumerator();
                enumerator.MoveNext();
                var toSearch = enumerator.Current;
                toBeSearched.Remove(toSearch);
                searchNullifiedUnitPair(toSearch);
            }

            // create new methods and remove unit methods

            todoLaterRemove();
            Console.ReadKey();
            return null;
        }
        private void todoLaterRemove()
        {
            foreach (var item1 in unitsConstraints)
            {
                foreach (var item2 in item1.Value)
                {
                    Console.WriteLine($"({item1.Key}, {item2.Key}): ");

                    foreach (var item3 in item2.Value)
                    {
                        if (item3.Count == 0) Console.Write($"(empty), ");
                        foreach (var item4 in item3)
                        {
                            string text = "after";
                            if (item4.type == typeof(BeforeConstraint)) text = "before";

                            Console.Write($"({item4.ps}, {text}), ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
        }
        private void searchNullifiedUnitPair(ValueTuple<TaskName, TaskName> pair)
        {
            List<Method> searchMethods = Common.MethodsWithHead(d.Methods, pair.Item2);

            for (int i = 0; i < searchMethods.Count; i++)
            {
                if (searchMethods[i].TaskCount() != 1 || 
                    searchMethods[i].RightSideCompound.Count != 1 ||
                    pair.Item1 == searchMethods[i].RightSideCompound[0].TaskName)
                {
                    searchMethods.RemoveAt(i);
                    i--;
                }
            }

            foreach (var m in searchMethods)
            {
                CompoundTask sub = m.RightSideCompound[0];

                // units[m.Head.TaskName] must exist at this point

                if (!unitsConstraints[pair.Item1].ContainsKey(sub.TaskName))
                {
                    unitsConstraints[pair.Item1][sub.TaskName] = new();
                }

                // unitsConstraints[pair.Item1][sub.TaskName] exists

                List<HashSet<ConstraintHelper>> firstPairCons = unitsConstraints[pair.Item1][pair.Item2];
                List<HashSet<ConstraintHelper>> secondPairCons = unitsConstraints[pair.Item2][sub.TaskName]; // always exists

                int secondLength = secondPairCons.Count;

                foreach (var firstCons in firstPairCons)
                {
                    for (int i = 0; i < secondLength; i++)
                    {
                        HashSet<ConstraintHelper> combined = new(firstCons);
                        combined.UnionWith(secondPairCons[i]);

                        if (!Common.ContaintsSet(unitsConstraints[pair.Item1][sub.TaskName], combined))
                        {
                            unitsConstraints[pair.Item1][sub.TaskName].Add(combined);
                            toBeSearched.Add(new(pair.Item1, sub.TaskName));
                        }
                    }
                }
            }
        }
        private void unitMethodBase(Method m)
        {
            if (!(m.TaskCount() == 1 && m.RightSideCompound.Count == 1)) return;

            if (!unitsConstraints.ContainsKey(m.Head.TaskName))
            {
                unitsConstraints[m.Head.TaskName] = new();
                
            }

            if (!unitsConstraints[m.Head.TaskName].ContainsKey(m.RightSideCompound[0].TaskName))
            {
                unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName] = new();
            }

            HashSet<ConstraintHelper> nulledConstraints = new();

            foreach (var after in m.Afters)
            {
                nulledConstraints.Add(new ConstraintHelper { ps = after.Symbol, type = typeof(AfterConstraint) });
            }
            foreach (var before in m.Befores)
            {
                nulledConstraints.Add(new ConstraintHelper { ps = before.Symbol, type = typeof(BeforeConstraint) });
            }

            if (!Common.ContaintsSet(unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName], nulledConstraints))
            {
                unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName].Add(nulledConstraints);
                toBeSearched.Add(new ValueTuple<TaskName, TaskName>(m.Head.TaskName, m.RightSideCompound[0].TaskName));
            }
        }
    }
}
