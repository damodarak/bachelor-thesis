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
            if (line == "") return;

            string t = "[a-zA-Z][a-zA-Z0-9]*#[0-9]+";
            string compound = @"^[A-Z][a-zA-Z0-9]*-->";
            string subtasks = $@"\((({t})(,{t})*)?\);";
            string constrPattern = $@"({t}<{t}|before\([a-zA-Z]+:{t}\)|after\({t}:[a-zA-Z]+\)|between\({t}:[a-zA-Z]+:{t}\))";
            string constrs = $@"\[({constrPattern}(,{constrPattern})*)?\]$";

            string complete = $"{compound}{subtasks}{constrs}";

            if (!Regex.IsMatch(line, complete))
            {
                Console.WriteLine("Incorrect domain format!");
                throw new Exception();
            }

            string[] headAndRest = line.Split("-->");

            CompoundTask head = new CompoundTask(headAndRest[0]);

            string[] subtasksAndConstr = headAndRest[1].Split(';');
            string[] tasks = subtasksAndConstr[0].Split(['(', ')', ','], StringSplitOptions.RemoveEmptyEntries);
            string[] constr = subtasksAndConstr[1].Split(['[', ']', ','], StringSplitOptions.RemoveEmptyEntries);

            Method method = new Method(head);
            Dictionary<string, Task> concreteTasks = new Dictionary<string, Task>();

            foreach (string task in tasks)
            {
                string[] taskAndIndex = task.Split('#');

                if (char.IsUpper(task[0]))
                {
                    CompoundTask ct = new CompoundTask(taskAndIndex[0]);
                    concreteTasks[task] = ct;
                    method.rightSideCompound.Add(ct);
                }
                else if (char.IsLower(task[0]))
                {
                    PrimitiveTask pt = new PrimitiveTask(taskAndIndex[0]);
                    concreteTasks[task] = pt;
                    method.rightSidePrimitive.Add(pt);
                }
                else
                {
                    Console.WriteLine("Task names must begin with a letter character!");
                    throw new Exception();
                }
            }
        }
        public void StoreDomain(PlanningDomain d)
        {
            throw new NotImplementedException();
        }
    }
}
