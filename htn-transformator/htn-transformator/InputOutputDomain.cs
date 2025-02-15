using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    appendMethod(domain, line);
                }
            }

            return domain;
        }
        private void appendMethod(PlanningDomain d, string line)
        {
            string[] methodPart = line.Split("-->");

            if (!char.IsUpper(methodPart[0][0]))
            {
                Console.WriteLine("Left side of a method must be a compound task!");
                throw new Exception();
            }

            CompoundTask head = new CompoundTask(methodPart[0]);
        }
        public void StoreDomain(PlanningDomain d)
        {
            throw new NotImplementedException();
        }
    }
}
