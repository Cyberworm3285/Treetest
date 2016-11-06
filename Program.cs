using System;
using System.Collections.Generic;

using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;

using TreeTest.ExtensionMethods;

namespace TreeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Aufs Maul oder nicht? Das ist hier die Frage...
            Console.Title = "TreeTest";
            Console.ForegroundColor = ConsoleColor.Yellow;

            Dictionary<int,string> outputDict = new Dictionary<int, string>
            {
                { 0, "nicht aufs maul" },
                { 1, "aufs Maul" }
            };

            Dictionary<string, string> inputDict = new Dictionary<string, string>()
            {
                { "0_0", "nicht verwandt" },
                { "0_1", "irgendwie verwandt" },
                { "1_0", "zimmer sieht kacke aus" },
                { "1_1", "Zimmer is aufgeraeumt" },
                { "2_0", "is scheiße drauf" },
                { "2_1", "is nett" },
                { "3_0", "kann dich sau nicht leiden" },
                { "3_1", "findet dich nice" }
            };

            DecisionVariable[] decVars = new DecisionVariable[]
            {
                new DecisionVariable ("Verwandt", 2),
                new DecisionVariable ("Aufgeraeumt", 2),
                new DecisionVariable ("GoodMood", 2),
                new DecisionVariable ("Sympathie", 2),
            };
            int classCount = 2; // nur ja/nein
            DecisionTree dectree = new DecisionTree(decVars, classCount);

            ID3Learning id3 = new ID3Learning(dectree);
            C45Learning c45 = new C45Learning(dectree);

            int[][] inputs = new int[][]
            {
                //          V  A  G  S
                new int[] { 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 1 },
                new int[] { 0, 0, 1, 0 },
                new int[] { 0, 0, 1, 1 },
                new int[] { 0, 1, 0, 0 },
                new int[] { 0, 1, 0, 1 },
                new int[] { 0, 1, 1, 0 },
                new int[] { 0, 1, 1, 1 },
                //----------V--A--G--S---                       
                new int[] { 1, 0, 0, 0 },
                new int[] { 1, 0, 0, 1 },
                new int[] { 1, 0, 1, 0 },
                new int[] { 1, 0, 1, 1 },
                new int[] { 1, 1, 0, 0 },
                new int[] { 1, 1, 0, 1 },
                new int[] { 1, 1, 1, 0 },
                new int[] { 1, 1, 1, 1 },
            };
            int[] outputs = new int[]
            {
                1,1,0,0,1,0,0,0,
                1,1,1,0,0,0,0,0,
            };

            if (inputs.Length != outputs.Length) throw new IndexOutOfRangeException("mismatched Array lenths");

            if (true) id3.Learn(inputs, outputs);
            else c45.Learn(inputs, outputs);

            dectree.PrintTree(prevDecision:"Start").Write();
            Console.Write("\n--Written As Expression--\n" + dectree.ToRules().ToString());

            while (true)
            {
                string inputString = Console.ReadLine();
                string[] inputStrings = inputString.Split(new char[] { '-', '.', ',', ';', ':' });
                int[] inputAsInt = new int[] {
                    inputStrings[0].ToInt(replacementOnError: 0, intervall: new System.Drawing.Point(0,1)),
                    inputStrings[1].ToInt(replacementOnError: 0, intervall: new System.Drawing.Point(0,1)),
                    inputStrings[2].ToInt(replacementOnError: 0, intervall: new System.Drawing.Point(0,1)),
                    inputStrings[3].ToInt(replacementOnError: 0, intervall: new System.Drawing.Point(0,1)),
                };
                for (int i = 0; i < inputAsInt.Length; i++)
                {
                    string message;
                    inputDict.TryGetValue(i + "_" + inputAsInt[i], out message);
                    Console.WriteLine(message);
                }
                int result = dectree.Decide(inputAsInt);
                string convertedResult;
                outputDict.TryGetValue(result, out convertedResult);
                Console.WriteLine("==>" + convertedResult);
            }
        }
    }
}

namespace TreeTest.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static int ToInt(this string str, int replacementOnError, System.Drawing.Point intervall)
        {
            int r = replacementOnError;
            int.TryParse(str, out r);
            if (r < intervall.X || r > intervall.Y) return replacementOnError;
            return r;
        }

        public static List<string> PrintTree(this DecisionTree t, char progressChar = ' ', string prevDecision = "START", string prevSpacing = "", string arrowConnector = "")
        {
            return t.Root.PrintTraverse(progressChar, prevDecision, prevSpacing, arrowConnector);
        }

        public static List<string> PrintTraverse(this DecisionNode n, char progressChar = ' ', string prevDecision = "START" ,string prevSpacing = "", string arrowConnector = "")
        {
            string message = prevSpacing + arrowConnector + "═>(" + prevDecision + ")═>";
            if (arrowConnector == "╠")
            {
                int index = message.LastIndexOf("║");
                message = message.Remove(index, 1);
            }
            else if (arrowConnector == "╚")
            {
                int index = message.LastIndexOf(" ");
                message = message.Remove(index, 1);
            }
            if (n.IsLeaf)
            {
                return new List<string>() { message + n.Output.ToBoolString() };
            }
            else
            {
                int? c = 0;
                List<string> result = new List<string>
                {
                    message + n.Branches.Attribute.Name + "[INDEX: " + n.Branches.AttributeIndex + "]"
                };
                foreach (DecisionNode nn in n.Branches)
                {
                    string decision = c.ToBoolString();
                    string newPrevSpacing = prevSpacing + new string(progressChar, prevDecision.Length + 6) + ((c == 0) ? "║" : " ");
                    result.AddRange(nn.PrintTraverse(progressChar, decision, newPrevSpacing, (c==0)? "╠" : "╚"));
                    c++;
                }
                return result;
            }
        }

        public static string ToBoolString(this int? i)
        {
            if (i < 0 || i > 1 || i == null) return "[ERROR]";
            else return (i == 0) ? "FALSE" : "TRUE";
        }

        public static void Write(this List<string> list)
        {
            foreach (string s in list) Console.WriteLine(s);
        }
    }
}
