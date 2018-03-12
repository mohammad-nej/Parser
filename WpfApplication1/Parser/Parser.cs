using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
namespace WpfApplication1
{
    class Parser
    {
        
        Lexer Lexer;
        LexerOutPut LexerOutput;

        public bool isLL { get; set; }
        public bool isLR { get; set; }
        public bool isSLR { get; set; }
        public bool grammerFormatOk { get; set; }
        public bool isLeftRecursive { get; set; }
        public bool ParserStop { get; set; }
        public bool DetailedDescription { get; set; }
        public bool Resume { get; set; }

        public string notLLReason { get; set; }
        public string notLRReason { get; set; }
        public string notSLRReason { get; set; }
        public string LeftRecursiveReason { get; set; }

        public SLRDFA SLRDFATable;

        public Dictionary<string, int> tokenMap = new Dictionary<string, int>();

        public Dictionary<string ,SLRGrammer> LRGrammer = new Dictionary<string,SLRGrammer>();

        public SLRTable SLRTable;
        public SLRTable LRTable;

        public Dictionary<LLTableKey, List<string>> LLTable = new Dictionary<LLTableKey, List<string>>();
        private List<string> stackItems = new List<string>();

        private System.Windows.Controls.CheckBox Finish;
        private System.Windows.Controls.DataGrid StackGrid;
        private System.Data.DataTable stackTable;
        
        public Stack<string> SLRStack = new Stack<string>();
        
        
        
        public Parser(Lexer lex,LexerOutPut output
            ,string Path, System.Windows.Controls.CheckBox finish,System.Windows.Controls.DataGrid stack)
        {
            LeftRecursiveReason = "";
            DetailedDescription = false;
            StackGrid = stack;
            IntializeDataTable();

            LexerOutput = output;
            Resume = true;//bara stop/resume kardan moghe pars
            Finish = finish;
            Lexer = lex;
           
            grammerFormatOk=readGrammerFromFile(Path);
            if (grammerFormatOk)
            {
                ExctractTokens();
                nonRecursiveFirstCalculator();
                followCreater();

                isLeftRecursive = !DetecteLeftRecursion();

                fillTable();
                isLL = checkLLGrammer();

                SLRDFATable = new SLRDFA(LRGrammer);

                SLRTable = new SLRTable(SLRDFATable, tokenMap,"SLR");
                LRTable = new SLRTable(SLRDFATable, tokenMap, "LR");

                isSLR = checkSLRGrammer();
                isLR = checkLRGrammer();
            }
        }
        ///<summary>
        ///This functions is used to give extra info to user based on the Detailed checkbox 
        ///</summary>
        private bool Desceription(Stack<string> stack,ref int counter,ref string description)
        {
            //This function Checks if the Detailed checkbox is checked or not
            //if checked, Then it shows the detaied description of every single stop of the parse proccess.
            if (DetailedDescription)
            {
                getStackItems(stack, ++counter, description, Lexer.RemainingInput(LexerOutput.output));
                if (!Wait())
                {
                    return false;
                }
                description = "";
            }
            
            return true; 
        }
        private bool DetecteLeftRecursion()
        {
            bool flag=true;
            List<RightSide> loops = new List<RightSide>();
            foreach (var gram in LRGrammer)
            {
                if (gram.Key == "start'")
                    continue;
                List<string> startRule = new List<string>();
                if (!DetectLeftRecursion(gram.Key, startRule, loops))
                {
                    flag = false;

                }

            }
            return flag;

        }
        ///<summary>
        ///Read the Grammer and also check for errors
        ///</summary>
        public bool readGrammerFromFile(string path)
        {
            string left = "";
            string right = "";
            bool isEpsilon = false;

            bool result = true;
            string Reasons = "Input format is incorrect:\r\n----------------\r\n";

            int ruleNumber = 1;//Tu grammer SLR shomare rule ro 1 shuru mikonim chun ghanun 0
            //ghanune start' -><start> e 
            string line = "";
            int line_number = 0;
 
            SLRGrammer gram2 = new SLRGrammer();
            gram2.left = "start'";
            gram2.Rights.Add(new RightSide { isEpsilon = false, Right = "<start>", Number = 0, Left="start'" });
            gram2.Rights[0].AddFollow("$");
            gram2.Follows = new List<string>();
            gram2.Follows.Add("$");
            LRGrammer.Add("start'",gram2);

            try
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("*"))
                            continue;
                        line_number++;
                        isEpsilon = false;

                        left = line.Substring(0, line.IndexOf("->"));
                        right = line.Substring(line.IndexOf("->") + 2).TrimEnd(' ');
                        if(line_number == 1 && left != "start")
                        {
                            //gereamer shuru bayad start bashad
                            Reasons += "First rule should be <start>.\r\n" + line + "\r\n----------------\r\n";
                            result = false;
                        }
                        if (left == "start'")
                        {
                            Reasons += "<start'> is not allowed!  ==>  " + line + "\r\n----------------\r\n";
                            result = false;
                        }
                        if(left.Contains(".")|| left.Contains("<")||left.Contains(">")
                            || left.Contains("$") || right.Contains(".") || right.Contains("$")){
                            Reasons += "'<>.$' are not allowed :" + line + "\r\n----------------\r\n";
                            result = false;
                        }
                        var words = right.Split(' ');
                        foreach (var wd in words)
                        {
                            if (wd == "<start'>")
                            {
                                Reasons += "<start'> is not allowed!\r\n" + line + "\r\n----------------\r\n";
                                result = false;
                            }
                            if (wd.StartsWith("<"))
                            {
                                if(!wd.EndsWith("\\>"))
                                    if (!wd.EndsWith(">"))
                                    {
                                        Reasons += "Mismatch brackets : " + line + "  ==>  " + "word: " + wd + "\r\n----------------\r\n";
                                        result = false;
                                    }
                            }
                            if (wd.EndsWith(">"))
                            {
                                if(!wd.EndsWith("\\>"))
                                    if (!wd.StartsWith("<") && !wd.StartsWith("\\"))
                                    {
                                        Reasons += "Mismatch brackets : " + line + "  ==>  " + "word: " + wd + "\r\n----------------\r\n";
                                        result = false;
                                    }
                            }
                            if(wd == "")
                            {
                                Reasons += "Redundant space character detected  ==>  " + line + "\r\n----------------\r\n";
                                result = false;
                            }
                        }

                        if (right == "epsilon")
                          isEpsilon = true;

                        if (!LRGrammer.ContainsKey(left))
                        {
                            SLRGrammer gram = new SLRGrammer();
                            gram.left = left;
                            gram.Rights.Add(new RightSide { Number = ruleNumber, isEpsilon = isEpsilon, Right = right, Left=left });
                            gram.isEpsilon = isEpsilon;
                            LRGrammer.Add(left, gram);

                        }else
                        {
                            LRGrammer[left].Rights.Add(new RightSide{ Right =right, Number = ruleNumber,Left=left, isEpsilon=isEpsilon });
                            if (isEpsilon)
                                LRGrammer[left].isEpsilon = true;
                        }
                        ruleNumber++;
                    }
                }
            }
            catch (Exception f)
            {
                System.Windows.MessageBox.Show("Error: "+f.Message);
                return false;
            }
            if (!result)
            {
                System.Windows.Forms.MessageBox.Show(Reasons, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return result;
            }

            return result;
        }
        //******************************************** Calculating Firsts********************************************
        private void nonRecursiveFirstCalculator()
        {
            string firstWord ;
            bool anythingAdded;
            bool sthAdded = false ;
            bool epsilonAdded = false;
            string[] words;
            int couter = 0;
            do
            {
                couter++;
                anythingAdded = false;
                sthAdded = false;

                foreach (var gram in LRGrammer)
                { 
                    foreach (var rl in gram.Value.Rights)
                    {
                        words =rl.words;
                        for (int j = 0; j < words.Length; j++)
                        {
                            if (j != 0 && !epsilonAdded)
                                break;
                            epsilonAdded = false;
                            firstWord = words[j];
                            if (Parser.isRule(firstWord))
                            {
                                if (LRGrammer.ContainsKey(Parser.returnRuleName(firstWord)))
                                    foreach (var fr in LRGrammer[Parser.returnRuleName(firstWord)].Firsts)
                                    {
                                        if (fr == "epsilon")
                                            epsilonAdded = true;
                                        rl.AddFirst(fr);

                                            sthAdded = LRGrammer[gram.Value.left].AddFirst(fr);
                                            if (sthAdded)
                                                anythingAdded = true;

                                    }
                            }
                            else
                            {

                                rl.AddFirst(firstWord);
                                sthAdded = LRGrammer[gram.Value.left].AddFirst(firstWord);
                                if (sthAdded)
                                    anythingAdded = true;
                                break;
                            }
                        }
                    }
            }
            } while (anythingAdded);
            
        }
     
        //***********                                   Firsts                                                *********
        //*************************************************************************************************************

        //**************************************************Follow*****************************************************
        
        void followCreater()
        {

            createFollows();
        }

        ///<summary>
        ///Detecte if the input is a Rule or not.
        ///</summary>
        public static bool isRule (string word)
        {
            return word.StartsWith("<") && word.EndsWith(">");
        }
        ///<summary>
        ///Get a string like "<rule_name>" and returns "rule_name"
        ///</summary>
        public static string returnRuleName(string tmp)
        {
            //< va > ra az aval akhare esme yek rule hazf mikonad
            if (tmp.Length < 2)
                return "";
            return tmp.Substring(1, tmp.Length - 2);
        }
        bool createFollows ()
        {
            bool anythingAdded = false;
            bool followAdded = false;
            string rule_one = "";
            bool epsilon_added = true;
            string rule_two = "";
            int k = 0;
            do
            {
                anythingAdded = false;
                followAdded = false;
                foreach (var gramm in LRGrammer)
                {
                    foreach (var rl in gramm.Value.Rights)
                    {
                        string[] words = rl.words;
                        for (int i = 0; i < words.Length ; i++)
                        {
                            
                            if (isRule(words[i]))
                            {
                                k = 0;
                                rule_one = returnRuleName(words[i]);
                                do
                                {
                                    k++;
                                    epsilon_added = false;
                                    if (i + k < words.Length)
                                        if (isRule(words[i + k]))
                                        {
                                            //agar 2 ghanun poshte ham bashand mesle A->NM
                                            rule_two = returnRuleName(words[i + k]);
                                            foreach (var frs in LRGrammer[rule_two].Firsts)
                                                if (frs == "epsilon")
                                                    epsilon_added = true;
                                                else
                                                {

                                                    followAdded = LRGrammer[rule_one].AddFollow(frs);
                                                    if (followAdded)
                                                        anythingAdded = true;
                                                }
                                        }
                                        else
                                        //agar yek ghanun va yek payane poshte ham bashand mesle A->Nc
                                        {

                                            followAdded = LRGrammer[rule_one].AddFollow(words[i + k]);
                                            if (followAdded)
                                                anythingAdded = true;
                                        }
                                } while (epsilon_added);


                                //hala nobate ghanun 3vom ast yani agar A->aN angah Follow[N]=Follow[A]
                                k = 0;

                                do
                                {
                                    k++;
                                    epsilon_added = false;
                                    if (k <= words.Length)
                                        if (isRule(words[words.Length - k]))
                                        {
                                            foreach (var flw in gramm.Value.Follows)
                                            {
                                                //chun darim ye Follow ro tu ye Follow dige mirizim
                                                //pas emkan nadre ke epsilon dashte bashim inja
                                                followAdded = LRGrammer[returnRuleName(words[words.Length - k])].AddFollow(flw);
                                                if (followAdded)
                                                    anythingAdded = true;
                                            }
                                            if (LRGrammer[returnRuleName(words[words.Length - k])].Firsts.Contains("epsilon"))
                                                epsilon_added = true;
                                        }
                                } while (epsilon_added);
                            }
                        }
                    }
                }
            } while (anythingAdded);
            return anythingAdded;
        }

        //*****************************************************LL Parser using LLTable**************************************************

        private bool checkLLGrammer() {
            bool flag1 = true;
            bool flag2 = true;
            string reason = "\r\n";
            foreach(var cell in LLTable)
            {
                if (cell.Value.Count > 1)
                {
                    reason += "LLTable["+cell.Key.Rule+"]["+cell.Key.Token+"]= ";
                    foreach(var cl in cell.Value)
                    {
                        reason += cl + ",";
                    }
                    reason = reason.TrimEnd(',');
                    reason += "\r\n";
                    flag1 = false;
                }
                
            }
            notLLReason = reason;
            if (isLeftRecursive)
            {
                notLLReason += "This grammer is left-recursive !";
                flag2 = false;

            }
            return flag1 && flag2;
            
        }
        ///<summary>
        ///Function that deteces Left-recursion loops in the grammer
        ///</summary>
        private bool  DetectLeftRecursion(string  left,List<string> current,List<RightSide> loop)
        {
            //in tabe bazgashty az chap budanro taskhis mide hata be surate chand marhale tu dar tu
            //yani A-><F>... F-><B>.... B-><A>.... ro tashkhis mide
            bool flag = true;
            string first = "";
            string[] words;
            bool result = true;
            bool containsEpsilon = false;

            current.Add(left);
            foreach (var rght in LRGrammer[left].Rights)
            {
                flag = true;
                    if (loop.Contains(rght))
                    {
                        flag = false;
                        break;
                    }
                if (!flag)
                    continue;
                words = rght.words;
                containsEpsilon = true;
                for(int j=0;j<words.Length&&containsEpsilon;j++)
                {
                    first = words[j];
                    containsEpsilon = false;
                    if (isRule(first))
                    {
                        first = returnRuleName(first);
                        if (LRGrammer[first].Firsts.Contains("epsilon"))
                        {
                            containsEpsilon = true;

                        }
                        if (current[0] == first)
                        {
                            LeftRecursiveReason += "\r\nLeft recursion:\r\n ";
                            for (int i = 0; i < current.Count; i++)
                            {
                                foreach (var rt in LRGrammer[current[i]].Rights)
                                {
                                    if (i < current.Count - 1)
                                    {
                                        if (rt.Right
                                            .StartsWith("<" + current[i + 1] + ">"))
                                        {
                                            loop.Add(rt);
                                            LeftRecursiveReason += current[i] + "->" + rt.Right + "\r\n";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (rt.Right.StartsWith("<" + first + ">"))
                                        {
                                            loop.Add(rt);
                                            LeftRecursiveReason += current[i] + "->" + rt.Right + "\r\n";
                                            break;
                                        }
                                    }

                                }

                            }
                            return false;
                        }
                        if (!current.Contains(first))
                            result = DetectLeftRecursion(first, current, loop);
                        if (!result)
                            return false;

                    }//End of if(isRule)
                }
            }
            return true;
        }
        private bool checkLRGrammer()
        {
            //grammeri LR e tu jadvalesh halate 2taiy yani Shift/Reduce ya Reduce/Reduce nadashte bashim
            notLRReason = "";
            bool flag = true;
            foreach (var cell in LRTable.Table)
            {
                if (cell.Value.Action.Count > 1)
                {
                    notLRReason += "action[S" + cell.Key.statNumber + "," + cell.Key.Token + "]= ";
                    for (int i = 0; i < cell.Value.Action.Count; i++)
                    {
                        if (cell.Value.Action[i] == SLRTableCell.Type.REDUCE)
                            notLRReason += "R" + cell.Value.Number[i] + ",";
                        if (cell.Value.Action[i] == SLRTableCell.Type.SHIFT)
                            notLRReason += "S" + cell.Value.Number[i] + ",";
                    }
                    notLRReason = notLRReason.TrimEnd(',');
                    notLRReason += "\r\n";
                    flag = false;
                }
            }
            notLRReason = notLRReason.TrimEnd();
            return flag;
        }

        private bool checkSLRGrammer()
        {
            //grammeri SLR e tu jadvalesh halate 2taiy yani Shift/Reduce ya Reduce/Reduce nadashte bashim
            notSLRReason = "";
            bool flag = true;
            foreach (var cell in SLRTable.Table)
            {
                if (cell.Value.Action.Count > 1)
                {
                    notSLRReason += "action[S" + cell.Key.statNumber + "," + cell.Key.Token + "]= ";
                    for (int i = 0; i < cell.Value.Action.Count; i++)
                    {
                        if (cell.Value.Action[i] == SLRTableCell.Type.REDUCE)
                            notSLRReason += "R" + cell.Value.Number[i]+",";
                        if (cell.Value.Action[i] == SLRTableCell.Type.SHIFT)
                            notSLRReason += "S" + cell.Value.Number[i] + ",";
                    }
                    notSLRReason = notSLRReason.TrimEnd(',');
                    notSLRReason += "\r\n";
                    flag = false;
                }
            }
            notSLRReason= notSLRReason.TrimEnd();
            return flag;
        }

        private void ExctractTokens()
        {
            int token_counter = 0;
            List<string> tokens = new List<string>();

            foreach (var gram in LRGrammer)
            {
                foreach (var rght in gram.Value.Rights)
                {
                    string[] words = rght.words;
                    for (int j = 0; j < words.Length; j++)
                        if (!(words[j].StartsWith("<") && words[j].EndsWith(">")))
                        {
                            if (!tokens.Contains(words[j]))
                            {
                                tokenMap.Add(words[j], token_counter);
                                token_counter++;
                                tokens.Add(words[j]);
                            }
                        }
                }
            }
            string token = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                token += tokens[i] + " ";
            }
            token = token.TrimEnd(' ');
            System.IO.File.WriteAllText("Tokens.txt", token);
        }
        ///<summary>
        ///Function used for filling the LL parse table
        ///</summary>
        private void fillTable()
        {
         
            KeyValuePair<string, int>[] tokens = tokenMap.ToArray();

            foreach (var gram in LRGrammer)
            {

                if (gram.Key == "start'")
                    continue;
                foreach (var rght in gram.Value.Rights)
                {

                    for (int j = 0; j < tokens.Length; j++)
                    {
                        LLTableKey key = new LLTableKey();
                        LLTableValue value = new LLTableValue();
                        key.Rule = gram.Key;
                        key.Token = tokens[j].Key;
                        var integer = findNextRule(gram.Key, tokens[j].Key);
                        if (!LLTable.ContainsKey(key))
                            LLTable.Add(key, new List<string>());
                        foreach (var num in integer)
                            if (!LLTable[key].Contains(num.ToString()))
                                LLTable[key].Add(num.ToString());
                    }
                }       
            }
        }

        ///<summary>
        ///Function for parsing LL grammers
        ///</summary>
        public string parsWithTable() {
            string reason = "Done";
            ParserStop = false;
            string report = "";

            int step_counter = 0;

            Stack<string> stack =new  Stack<string>();
            
            
            stackTable.Clear();
            stackItems.Clear();
            stack.Clear();

            string currentToken="";

            if (LexerOutput.output == LexerOutPut.OutPut.TYPE)
                currentToken = Lexer.seeToken().Type;
            else if (LexerOutput.output == LexerOutPut.OutPut.TEXT)
                currentToken = Lexer.seeToken().text;

            report += "Current Token = " + currentToken;
            report = report.TrimStart();
            if(!Desceription(stack,ref step_counter,ref report))
            {
                return "Parser Stopped!";
            }


            stack.Push("<start>");
             getStackItems(stack,++step_counter,"Push -> "+"start",Lexer.RemainingInput(LexerOutput.output));
             if (!Wait())
             {
                 return "Parser Stoped!";
             }
          
            string topOfStack;
            while (stack.Count!=0)//yani ta zamani ke stack khali nashode 
            {
                
                topOfStack = stack.Pop();
                getStackItems(stack,++step_counter ,"Pop -> "+topOfStack, Lexer.RemainingInput(LexerOutput.output));
                if (!Wait())
                {
                    return "Parser Stoped!";
                }
              

                if (topOfStack == "epsilon")
                    continue;

                if (topOfStack != currentToken)
                {
                    string tableValue;
                    report = "";
                    tableValue = "-1";
                
                    //agar gerammer peida shod
                    LLTableKey key = new LLTableKey();
                    key.Rule = returnRuleName(topOfStack);
                    key.Token = currentToken;

                    if (LLTable.ContainsKey(key)) {
                        var value = LLTable[key];
                        if (value.Count > 1)
                        {
                            reason = "Parse Failed!\r\nReason:\r\n";
                            reason += "LL Table[" + key.Rule + "][" + key.Token + "]= ";
                            foreach(var vl in value)
                            {
                                reason += vl + ",";
                            }
                            reason = reason.TrimEnd(',');
                            return reason;
                        }
                        tableValue = value[0];
                        report += "LL Table[" + key.Rule + "," + key.Token + "]= " +tableValue+"\r\n" ;
                    }
                    if(tableValue == "-1")
                    {
                        //yek bar ham be ezaye epsilon check mikonim
                        //agar in ham -1 bashe dg tamume XD
                        if (tokenMap.ContainsKey("epsilon"))
                        {
                            LLTableKey key2 = new LLTableKey();
                            key2.Rule = returnRuleName(topOfStack);
                            key2.Token = "epsilon";

                            if (LLTable.ContainsKey(key2))
                            {
                                if (LLTable[key2].Count > 1)
                                {
                                    reason = "";
                                    reason += "LL Table[" + key.Rule + "][" + key.Token + "]= ";
                                    foreach (var vl in LLTable[key2])
                                    {
                                        reason += vl + ",";
                                    }
                                    reason = reason.TrimEnd(',');
                                    return reason;
                                }
                                tableValue = LLTable[key2][0];
                                report += "LL Table[" + key2.Rule + "," + key2.Token + "]= " + tableValue+"\r\n";
                            }
                        }

                    }
                    if (tableValue == "-1")
                    {
                        //agar hamchenan tableVAlue -1 bashe yani error darim
                        reason = "Parse Failed!\r\nReason:\r\n";
                        reason += "LL Table[" + key.Rule + "][" + key.Token + "]= -1 ";
                        return reason;
                    }
                    else
                    {
                        var rule = findGrammer(Convert.ToInt32(tableValue));
                        //agar grammer peida shod mohtaviate grammer ro be surate varune tu stack push mikonim

                        string[] temp = rule.words;

                        report += "Based on: " + rule.Left + " -> " + rule.Right + "\r\n";
                        for (int i = temp.Length - 1; i >= 0; i--)
                        {

                            stack.Push(temp[i]);//be tore varune tu stack push
                            report += "Push -> " + temp[i]+"\r\n";
                            if (!Desceription(stack,ref step_counter, ref report))
                            {
                                return "Parser Stopped!";
                            }

                        }
                        if (!DetailedDescription)
                        {
                            getStackItems(stack, ++step_counter, report.TrimStart(), Lexer.RemainingInput(LexerOutput.output));
                            if (!Wait())
                            {
                                return "Parser Stoped!";
                            }
                        }
                    
                    }
                }
                else { 
                 //yani age balaye stack hamun tokeni ke alan mikhaim bud dar in surat bayad faghat stack ro pop konim va berim token badi
                //stack ke ghablan pop shode :D pas faghat bayad berim token badi
                    Lexer.gotoNextToken();


                    if (LexerOutput.output == LexerOutPut.OutPut.TYPE)
                        currentToken = Lexer.seeToken().Type;
                    else if (LexerOutput.output == LexerOutPut.OutPut.TEXT)
                        currentToken = Lexer.seeToken().text;
                    report = "Current Token = " + currentToken + "\r\n";
                    if (!Desceription(stack, ref step_counter, ref report))
                    {
                        return "Parser Stopped!";
                    }

                }

                }
            if (currentToken != "$")
                return "Failed!";
            return reason;
        }
        ///<summary>
        ///Finds the Right side of the grammer by it's number.
        ///</summary>
        private RightSide findGrammer(int num)
        {
            foreach(var gram in LRGrammer)
            {
                foreach (var rgh in gram.Value.Rights)
                    if (rgh.Number == num)
                        return rgh;
            }
            return null;
        }

        private List<int> findNextRule(string rule, string currentToken)
        {
            bool flag = true;
            var gram = LRGrammer[rule].Firsts;
            List<int> indexs = new List<int>();

                if (LRGrammer.ContainsKey(rule))
                {
                    foreach(var rght in LRGrammer[rule].Rights)
                    {

                        if (rght.mini_Firsts.Contains(currentToken))
                        {
                            flag = false;
                            indexs.Add(rght.Number);//chun grammer[0] dar vaghe start' e ! 
                        }
                        else if (rght.mini_Firsts.Contains("epsilon"))
                        {
                            if(flag)
                                indexs.Add(rght.Number);
                        }
                    }
            }
            return indexs;
        }
        //********************                                  Pars using LLTable                                      ************
        //**************************************************************************************************************************







        //*************************************************** SLR Parser *************************************************************


        ///<summary>
        ///Intialize the DataTable that is used for keep tracking the Input stack while parsing.
        ///</summary>
        private void IntializeDataTable()
        {
            stackTable = new System.Data.DataTable("Stack Data Table");
            System.Data.DataColumn column;

            //Id
            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = true;
            column.ColumnName = "Id";
            stackTable.Columns.Add(column);

            //Stack items
            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Stack";
            stackTable.Columns.Add(column);

            //Input column
            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Input";
            stackTable.Columns.Add(column);


            //Description(s)
            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Description";
            stackTable.Columns.Add(column);


            StackGrid.ItemsSource = stackTable.DefaultView;

        }
        public void getStackItems(Stack<string> stk,int counter,string desc,string input)
        {
            string items = "";
            string temp;
            Stack<string> backUP = new Stack<string>();
            while (stk.Count != 0)
            {
                temp = stk.Pop();
                items = temp + items;
                backUP.Push(temp);
            }
            while (backUP.Count != 0)
                stk.Push(backUP.Pop());
            
            System.Data.DataRow row;
            row = stackTable.NewRow();
            row["Id"] = counter;
            row["Stack"] = items;
            row["Input"] = input;
            row["Description"] = desc;
            stackTable.Rows.Add(row);
            StackGrid.Items.Refresh();
            StackGrid.ScrollIntoView(StackGrid.Items.GetItemAt(StackGrid.Items.Count - 1));
        }
        private bool Wait()
        {
            Resume = false;

            if (Finish.IsChecked == true)
            {
                return true;
            }
            while (!Resume)
            {
                System.Threading.Thread.Sleep(20);
                System.Windows.Forms.Application.DoEvents();
            }
            
            if (ParserStop)
                return false;
            return true;
        }

        ///<summary>
        ///Function used for parsing LR,SLR grammers, return a string at the end
        ///</summary>
        public string SLRParser(SLRTable GrammerTable)
        {
            
            string reason = "Pars OK!";
            ParserStop = false;
            SLRTableKey key;
            SLRTableCell value;
            
            stackTable.Clear();
            stackItems.Clear();
            SLRStack.Clear();

            string Token="";
            int currentState = 0;
            int step_counter = 0;
            string description = "";

            key.Token = "";
            key.statNumber = currentState;

            if (LexerOutput.output == LexerOutPut.OutPut.TYPE)
                key.Token = Lexer.seeToken().Type;
            else if (LexerOutput.output == LexerOutPut.OutPut.TEXT)
                key.Token = Lexer.seeToken().text;

            SLRStack.Push(currentState.ToString());
            description = "Push -> " + currentState.ToString()+"\r\nCurrent State= "+currentState+"\r\nCurrnet Token= "+key.Token;
            if (!Desceription(SLRStack, ref step_counter, ref description))
            {
                return "Parser Stopped!";
            }



            if (GrammerTable.Table.ContainsKey(key))
                value = GrammerTable.Table[key];
            else
            {
                reason="Error: Unidentified token :" + key.Token;
                return reason;
            }

            if (LexerOutput.output == LexerOutPut.OutPut.TYPE)
                Token = Lexer.seeToken().Type;
            else if (LexerOutput.output == LexerOutPut.OutPut.TEXT)
                Token = Lexer.seeToken().text;

            while (true)
            {

                //bayad check konim bebinim shift e ya reduce                           
                if(value.Action.Count == 0)
                {
                    //yani age be hich ja narim be ezaye in dastan
                    //dar in halat yani error darim
                    reason = "SLR Table[S" + key.statNumber + "," + key.Token + "] is empty!";
                    return reason;
                    
                }else if(value.Action.Count > 1)
                {
                    //dar in halat yani ma ba yek Shift/Reduce movajeh hastim va bazam error e
                    string message = "Pars Error: action[S" + currentState + "," + Token + "]= ";
                    for(int i = 0; i < value.Action.Count; i++)
                    {
                        if (value.Action[i] == SLRTableCell.Type.REDUCE)
                            message += "R" + value.Number[i] + ",";
                        else
                        {
                            message += "S" + value.Number[i] + ",";
                        }
                        
                    }
                    message = message.TrimEnd(',');
                    return message;
                }else if (value.Action[0] == SLRTableCell.Type.SHIFT)
                {
                    //Yani dar yek halate shifty hastim
                    description = "action[S" + currentState + "," + Token + "] = " + "Shift " + value.Number[0]+"\r\n";

                    SLRStack.Push(Token);
                    description += "Push -> " + Token+"\r\n";
                    if (!Desceription(SLRStack, ref step_counter, ref description))
                    {
                        return "Parser Stopped!";
                    }

                    SLRStack.Push(value.Number[0]);
                    description += "Push ->" + value.Number[0] + "\r\n";
                    description += "Shift Completed.\r\nCurrent State= "+value.Number[0];
                    if (!Desceription(SLRStack, ref step_counter, ref description))
                    {
                        return "Parser Stopped!";
                    }


                    currentState = Convert.ToInt32(value.Number[0]);
                    Lexer.gotoNextToken();
                    if (!DetailedDescription)
                    {
                        getStackItems(SLRStack, ++step_counter, description, Lexer.RemainingInput(LexerOutput.output));
                        if (!Wait())
                        {
                            return "Parser Stoped!";
                        }
                    }
                }
                else if (value.Action[0] == SLRTableCell.Type.REDUCE)
                {
                    
                    
                    string temp = "";
                    foreach(var gram in LRGrammer)
                    {
                       foreach(var rght in gram.Value.Rights)
                        {
                            if (rght.Number.ToString() == value.Number[0])
                                temp = gram.Value.left+" -> "+rght.Right;
                        }
                    }
                    if(!value.isEpsilon)
                        description = "action[S" + currentState + "," + Token + "] = " + "Reduce " + value.Number[0];
                    else
                        description = "action[S" + currentState + "," + "epsilon" + "] = " + "action[S" + currentState + "," + Token + "] = " + "Reduce " + value.Number[0];
                    description += "\r\n";

                    var words = temp.Split(' ');
                    description += "Based on :" + temp + "\r\n";
                    if (!value.isEpsilon)
                    {
                       
                        
                        //agar be khatere epsilon be ye halate kaheshi umade bashim
                        //chun epsilon token nist va ma be ezash ghablan hichi push nakardim tu stack
                        //pas alan ham nabayad chizi az tu stack pop konim be ezaye epsilon
                        string poped = "";
                        for (int i = words.Length-1; i >= 2; i--)
                        {
                            //chun temp : A -> c D .   ye hamchin formaty dare be tedade
                            //samte raste ghanun az tu stack pop mikonim
                            //va chun har bar be ezaye har token ye bar ham shomare 
                            //ghanun ro neveshtim pas be ezaye har words[i] bayad 2bar pop konim
                            if(SLRStack.Count < 2)
                            {
                                return "Unexpected error!";
                            }
                            poped =SLRStack.Pop();
                            description += "Pop -> " + poped+"\r\n";
                            if (!Desceription(SLRStack, ref step_counter, ref description))
                            {
                                return "Parser Stopped!";
                            }


                            poped =SLRStack.Pop();
                            description += "Pop -> " + poped + "\r\n";
                            if (!Desceription(SLRStack, ref step_counter, ref description))
                            {
                                return "Parser Stopped!";
                            }


                            if (poped != words[i])
                            {
                                reason = "Error: Unexpected token : " + poped + "\r\n Expected: " + words[i];
                                break;
                            }
                        }
                    }
                    
                    //    Left == words[0]

                    //alan in ja bayad ye shift dashte bashim ba current state ghabli + un tokenii ke jadidan gzoashtim
                    //tu stack badam dobare bayad push beshe hame chi tu stack
                    string tempState = SLRStack.Pop();
                    description += "Pop -> " + tempState+"\r\nCurrent State= "+tempState+"\r\n";
                    if (!Desceription(SLRStack, ref step_counter, ref description))
                    {
                        return "Parser Stopped!";
                    }



                    SLRStack.Push(tempState);
                    description += "Push -> " + tempState + "\r\n";
                    if (!Desceription(SLRStack, ref step_counter, ref description))
                    {
                        return "Parser Stopped!";
                    }

                    SLRTableKey tempKey;
                    tempKey.statNumber = Convert.ToInt32(tempState);
                    tempKey.Token = "<"+words[0]+">";     //left
                    if (GrammerTable.Table.ContainsKey(tempKey))
                    {
                        value = GrammerTable.Table[tempKey];
                        if (value.Action.Count == 1 && value.Action[0] == SLRTableCell.Type.SHIFT)
                        {

                            description += "goto[S" + tempKey.statNumber + "," + tempKey.Token + "] = " + "Shift " + value.Number[0];
                            if (!Desceription(SLRStack, ref step_counter, ref description))
                            {
                                return "Parser Stopped!";
                            }


                            SLRStack.Push(tempKey.Token);
                            description += "\r\nPush -> <" + words[0] + ">\r\n";
                            if (!Desceription(SLRStack, ref step_counter, ref description))
                            {
                                return "Parser Stopped!";
                            }


                            SLRStack.Push(value.Number[0]);
                            description += "Push -> " + value.Number[0] + "\r\n";
                            description += "\r\nReduce Completed.\r\nCurrent State= "+value.Number[0];
                            getStackItems(SLRStack, ++step_counter, description, Lexer.RemainingInput(LexerOutput.output));
                            if (!Wait())
                            {
                                return "Parser Stoped!";
                            }
                            description = "";
                            
                            currentState = Convert.ToInt32(value.Number[0]);
                        }
                    }
                    else
                    {
                        return "<" + words[0] + "> is not present in the Dictionary!";
                    }            
                }//End of Reduce
                else if (value.Action[0] == SLRTableCell.Type.ACCEPT)
                {

                    return reason;
                }
                key.statNumber = currentState;

                if (LexerOutput.output == LexerOutPut.OutPut.TYPE)
                    Token = Lexer.seeToken().Type;//.text;
                else if (LexerOutput.output == LexerOutPut.OutPut.TEXT)
                    Token = Lexer.seeToken().text;

                description += "\r\nCurrent Token= " + Token;
                if (!Desceription(SLRStack, ref step_counter, ref description))
                {
                    return "Parser Stopped!";
                }


                key.Token = Token;
                if(GrammerTable.Table.ContainsKey(key))
                    value = GrammerTable.Table[key];
                else
                {
                    return "Error: Unexpected token :" + key.Token;
                }
            }//End of while
        }//End of function
    }
}

