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
        private HashSet<ValueTuple<TaskName, TaskName>> searched;
        public RemoveUnitMethods(PlanningDomain d)
        {
            this.d = d;
            unitsConstraints = new();
            toBeSearched = new();
            searched = new();
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
                searched.Add(toSearch);
                searchNullifiedUnitPair(toSearch);
            }

            // create new methods and remove unit methods

            foreach (var pair in searched)
            {
                List<Method> appendMethods = Common.MethodsWithHead(d.Methods, pair.Item2);

                for (int i = 0; i < appendMethods.Count; i++)
                {
                    if (appendMethods[i].isUnit())
                    {
                        appendMethods.RemoveAt(i);
                        i--;
                    }
                }

                constructNewNonUnitMethods(appendMethods, pair);
            }

            for (int i = 0; i < d.Methods.Count; i++)
            {
                if (d.Methods[i].isUnit())
                {
                    d.RemoveMethod(d.Methods[i]);
                    i--;
                }
            }

            return d;
        }
        private void constructNewNonUnitMethods(List<Method> appendMethods, ValueTuple<TaskName, TaskName> pair)
        {
            foreach (var m in appendMethods)
            {
                foreach (var constraintsSet in unitsConstraints[pair.Item1][m.Head.TaskName])
                {
                    Method connected = new Method(new CompoundTask(pair.Item1, -1), m);
                    var ordering = connected.TaskOrdering();
                    foreach (var constr in constraintsSet)
                    {
                        if (constr.type == typeof(BeforeConstraint))
                        {
                            var bc = new BeforeConstraint(constr.ps, ordering[0]);
                            connected.AppendBefore(bc);
                        }
                        else
                        {
                            var ac = new AfterConstraint(constr.ps, ordering[connected.TaskCount() - 1]);
                            connected.AppendAfter(ac);
                        }
                    }
                    d.AppendMethod(connected);
                }
            }
        }
        private void searchNullifiedUnitPair(ValueTuple<TaskName, TaskName> pair)
        {
            List<Method> searchMethods = Common.MethodsWithHead(d.Methods, pair.Item2);

            for (int i = 0; i < searchMethods.Count; i++)
            {
                if (!searchMethods[i].isUnit() ||
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
            if (!m.isUnit()) return;

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
