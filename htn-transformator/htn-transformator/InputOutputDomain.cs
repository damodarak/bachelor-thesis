using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace htn_transformator
{
    internal class InputOutputDomain
    {
        private string inputFile { get; set; }
        private string outputFile { get; set; }
        private bool insideComment = false;
        public InputOutputDomain(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
        }
        public PlanningDomain LoadDomain()
        {
            PlanningDomain domain = new PlanningDomain();

            using (StreamReader sr = new StreamReader(inputFile))
            {
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    parseAndAppendMethod(domain, line);
                }
            }

            return domain;
        }
        private void parseAndAppendMethod(PlanningDomain d, string line)
        {
            if (line.StartsWith("*/"))
            {
                insideComment = false;
                return;
            }

            if (line == "" || line[0] == '#' || insideComment) return;

            if (line.StartsWith("/*"))
            {
                insideComment = true;
                return;
            }


            string tPattern = "[a-zA-Z][a-zA-Z0-9]*#[0-9]+";
            string compoundPattern = "^[A-Z][a-zA-Z0-9]*-->";
            string subtasksPattern = $@"\((({tPattern})(,{tPattern})*)?\);";
            string constrPattern = $@"({tPattern}<{tPattern}(<{tPattern})*|before\([a-zA-Z]+:{tPattern}\)|after\({tPattern}:[a-zA-Z]+\)|between\({tPattern}:[a-zA-Z]+:{tPattern}\))";
            string constrsPattern = $@"\[({constrPattern}(,{constrPattern})*)?\]$";

            string completePattern = $"{compoundPattern}{subtasksPattern}{constrsPattern}";

            if (!Regex.IsMatch(line, completePattern))
            {
                throw new Exception("Incorrect domain format!");
            }

            string[] headAndRest = line.Split("-->");

            CompoundTask head = new CompoundTask(headAndRest[0], Task.HeadIndex);

            string[] subtasksAndConstr = headAndRest[1].Split(';');
            string[] tasks = subtasksAndConstr[0].Split(['(', ')', ','], StringSplitOptions.RemoveEmptyEntries);
            string[] constrs = subtasksAndConstr[1].Split(['[', ']', ','], StringSplitOptions.RemoveEmptyEntries);

            List<string> uniqueTasks = removeDuplicates(tasks);
            List<string> uniqueConstrs = removeDuplicates(constrs);

            Method method = new Method(head);
            Dictionary<string, Task> concreteTasks = new Dictionary<string, Task>();

            foreach (string task in uniqueTasks)
            {
                parseAndAppendTask(method, task, concreteTasks);
            }

            foreach (string con in uniqueConstrs)
            {
                parseAndAppendConstraint(method, con, concreteTasks);
            }

            d.AppendMethod(method);
        }
        private List<string> removeDuplicates(string[] strings)
        {
            return new HashSet<string>(strings).ToList();
        }
        private void parseAndAppendTask(Method m, string task, Dictionary<string, Task> concreteTasks)
        {
            string[] taskAndIndex = task.Split('#');

            if (char.IsUpper(task[0]))
            {
                CompoundTask ct = new CompoundTask(taskAndIndex[0], int.Parse(taskAndIndex[1]));
                concreteTasks[task] = ct;
                m.AppendTask(ct);
            }
            else if (char.IsLower(task[0]))
            {
                PrimitiveTask pt = new PrimitiveTask(taskAndIndex[0], int.Parse(taskAndIndex[1]));
                concreteTasks[task] = pt;
                m.AppendTask(pt);
            }
            else
            {
                throw new Exception("Task names must begin with a letter character!");
            }
        }
        private void parseAndAppendConstraint(Method m, string con, Dictionary<string, Task> concreteTasks)
        {
            if (con.Contains("<"))
            {
                appendOrderings(m, con, concreteTasks);
            }
            else if (con.Contains("before"))
            {
                string[] before = con.Split(['(', ')', ':'], StringSplitOptions.RemoveEmptyEntries);

                PropositionalSymbol ps = new PropositionalSymbol(before[1]);
                BeforeConstraint bc = new BeforeConstraint(ps, concreteTasks[before[2]]);
                m.AppendBefore(bc);
            }
            else if (con.Contains("after"))
            {
                string[] after = con.Split(['(', ')', ':'], StringSplitOptions.RemoveEmptyEntries);

                PropositionalSymbol ps = new PropositionalSymbol(after[2]);
                AfterConstraint ac = new AfterConstraint(ps, concreteTasks[after[1]]);
                m.AppendAfter(ac);
            }
            else if (con.Contains("between"))
            {
                string[] between = con.Split(['(', ')', ':'], StringSplitOptions.RemoveEmptyEntries);

                PropositionalSymbol ps = new PropositionalSymbol(between[2]);
                BetweenConstraint betc = new BetweenConstraint(ps, concreteTasks[between[1]], concreteTasks[between[3]]);
                m.AppendBetween(betc);
            }
            else
            {
                throw new Exception("Unknown constraint!");
            }
        }
        private void appendOrderings(Method m, string con, Dictionary<string, Task> concreteTasks)
        {
            string[] orderConstr = con.Split('<');

            for (int i = 0; i < orderConstr.Length; i++)
            {
                for (int j = i + 1; j < orderConstr.Length; j++)
                {
                    OrderConstraint oc = new OrderConstraint(concreteTasks[orderConstr[i]], concreteTasks[orderConstr[j]]);
                    m.AppendOrderingConstraint(oc);
                }
            }
        }
        public void StoreDomain(PlanningDomain d)
        {
            if (outputFile == "")
            {
                foreach (Method m in d.Methods)
                {
                    Console.WriteLine(m.ToString());
                }

                return;
            }

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                foreach (Method m in d.Methods)
                {
                    sw.WriteLine(m.ToString());
                }
            }
        }
    }
}
