using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace htn_transformator
{
    /// <summary>
    /// Class for parsing the input domain, and for storing the result.
    /// </summary>
    internal class InputOutputDomain
    {
        private string inputFile { get; set; }
        private string outputFile { get; set; }
        /// <summary>
        /// Whether the following lines of the file should be ignored.
        /// </summary>
        private bool insideComment = false;
        public InputOutputDomain(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
        }
        /// <summary>
        /// Parses the PlanningDomain from the input file.
        /// </summary>
        /// <returns>Parsed PlanningDomain.</returns>
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
        /// <summary>
        /// Defines the grammar of the method. Parses a single line of input domain.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="line"></param>
        /// <exception cref="Exception"></exception>
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
            string constrPattern = $@"({tPattern}<{tPattern}(<{tPattern})*|before\([a-zA-Z0-9]+:{tPattern}\)|after\({tPattern}:[a-zA-Z0-9]+\)|between\({tPattern}:[a-zA-Z0-9]+:{tPattern}\))";
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
        /// <summary>
        /// Remove duplicate strings from the input parameter.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        private List<string> removeDuplicates(string[] strings)
        {
            return new HashSet<string>(strings).ToList();
        }
        /// <summary>
        /// Creates a new concrete task (Primitive/Compound) and appends to the Method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="task"></param>
        /// <param name="concreteTasks"></param>
        /// <exception cref="Exception"></exception>
        private void parseAndAppendTask(Method m, string task, Dictionary<string, Task> concreteTasks)
        {
            string[] taskAndIndex = task.Split('#');

            // A CompoundTask starts with a capital letter.
            if (char.IsUpper(task[0]))
            {
                CompoundTask ct = new CompoundTask(taskAndIndex[0], int.Parse(taskAndIndex[1]));
                concreteTasks[task] = ct;
                m.AppendTask(ct);
            }
            // A PrimitiveTask starts with a lower letter.
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
        /// <summary>
        /// Creates a new Constraint and appends to the Method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="con"></param>
        /// <param name="concreteTasks"></param>
        /// <exception cref="Exception"></exception>
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
        /// <summary>
        /// Creates new OrderingConstraint objects and appends to the Method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="con"></param>
        /// <param name="concreteTasks"></param>
        private void appendOrderings(Method m, string con, Dictionary<string, Task> concreteTasks)
        {
            string[] orderConstr = con.Split('<'); // Maybe be multiple tasks

            for (int i = 0; i < orderConstr.Length; i++)
            {
                for (int j = i + 1; j < orderConstr.Length; j++)
                {
                    OrderConstraint oc = new OrderConstraint(concreteTasks[orderConstr[i]], concreteTasks[orderConstr[j]]);
                    m.AppendOrderingConstraint(oc);
                }
            }
        }
        /// <summary>
        /// Stores the PlanningDomain to the outputFile, or standard output.
        /// </summary>
        /// <param name="d"></param>
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
