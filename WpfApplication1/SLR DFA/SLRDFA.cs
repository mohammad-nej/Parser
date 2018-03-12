using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class SLRDFA
    {
        public List<SLRDFASTAT> stats;
        public Dictionary <string,SLRGrammer> grammer;
        public SLRDFA (Dictionary<string,SLRGrammer> gram)
        {
            grammer = gram;
            BuildSLRDFA();
        }
        private int stat_number = 0;

        private void BuildSLRDFA()
        {
            SLRDFASTAT stat;
            stats = new List<SLRDFASTAT>();

            
            stat = new SLRDFASTAT(null, "");
            stat.State_Number = 0;

            //Adding start'->. <start> as the first state
            SLRDFASTAT.Statement statem = new SLRDFASTAT.Statement();
            statem.isCore = true;
            statem.Left = "start'";
            statem.Right = new RightSide() { Right = "<start>", Left = "start'", isEpsilon = false };
            statem.Right.AddFollow("$");
            //statem.DotIndex = 0; it is zero by defualt
            foreach (var fr in grammer["start"].Firsts)
                statem.Right.AddFirst(fr);

            stat.AddStatement(statem);
            recursiveStateBuilder(stat,null);
        }
        private bool statsAreEqual (SLRDFASTAT first,SLRDFASTAT second)
        {
            bool flag = false;
            if (first.Statements.Count != second.Statements.Count)
                return false;
            
            for(int i = 0; i < first.Statements.Count; i++)
            {
                flag = false;
                for (int j = 0; j < second.Statements.Count; j++)
                {

                    if (
                        (first.Statements[i].Presentation == second.Statements[j].Presentation) &&
                        first.Statements[i].DotIndex == second.Statements[j].DotIndex
                        )
                        flag = true;
                }
                if (!flag)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Builds the LR DFA states recursivly
        /// </summary>
        /// <param name="current">Current state</param>
        /// <param name="parent">Parent</param>
        /// <returns></returns>
        private SLRDFASTAT recursiveStateBuilder (SLRDFASTAT current,SLRDFASTAT parent)
        {

            string afterDot = "";
            int index = 0;
    
            while (true)
            {

                if (index == current.Statements.Count)
                    break;
                int Dot=current.Statements[index].DotIndex; 
                if(!current.Statements[index].DotIsAtEnd)
                    afterDot = current.Statements[index].Right.words[Dot];
                else
                {
                    index++;
                    continue;
                }
                if(afterDot == "epsilon")
                {
                    //Since epsilon is not a token we always just pass the dot over epsilon 
                    current.Statements[index].DotIndex++;
                    continue;
                }
                SLRDFASTAT.Statement statem = new SLRDFASTAT.Statement();
                statem.Left = current.Statements[index].Left;
                statem.Right = current.Statements[index].Right;
                statem.DotIndex = current.Statements[index].DotIndex;
                statem.isCore = true;

                if (current.next_pointers.ContainsKey(afterDot))
                {
                    if (!current.next_pointers[afterDot].Statements.Contains(statem))
                        current.next_pointers[afterDot].Statements.Add(statem);
                }
                else
                {

                        SLRDFASTAT newStat = new SLRDFASTAT(current, afterDot);
                        newStat.AddStatement(statem);
                        current.next_pointers.Add(afterDot, newStat);
                }

                //Creating prediction set
                if (Parser.isRule(afterDot))
                 
                {
                    foreach (var right in grammer[Parser.returnRuleName(afterDot)].Rights)
                    {
                        statem = new SLRDFASTAT.Statement();

                        statem.Left = afterDot;
                        statem.Right = right;
                        statem.isCore = false;
                        
                        bool include = false;
                        foreach(var cr in current.Statements)
                        {
                            if (cr.Presentation == statem.Presentation && cr.DotIndex == statem.DotIndex)
                                include = true;
                        }
                        if (!include)
                            current.AddStatement(statem);
                    }
                }

                if (index == current.Statements.Count - 1)
                    break;

                index++;

            }
            
            foreach (var st in stats)
            {
                //we will add this state to list if it is not repetetive
                if (statsAreEqual(st, current))
                {
                    current.State_Number = st.State_Number;
                    st.parent.AddRange(current.parent);
                    return current;
                }
                
            }
            current.State_Number = stat_number++;
            stats.Add(current);

            for(int j = 0; j < current.next_pointers.Count; j++)
            {
                string key = current.next_pointers.Keys.ElementAt(j);
                foreach (var stat in current.next_pointers[key].Statements)
                {
                    stat.DotIndex++;
                }
                current.next_pointers[key] = recursiveStateBuilder(current.next_pointers[key], current);
            }
           
            return current;
        }



 
    }
}
