using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class SLRDFASTAT
    {
        public class Statement
        {
            public string Left { get; set; }
            private bool dotIsAtEnd;
            private int dotIndex;
            private RightSide right;
            public bool DotIsAtEnd {
                get
                {
                    return dotIsAtEnd;
                }
            }
            public RightSide Right {
                get
                {
                    return right;
                }
                set
                {
                    right = value;

                    //When we declare a right, we must update the presention
                    if (dotIndex == right.words.Length)
                    {
                        dotIsAtEnd = true;
                    }
                    Presentation = Left + " -> ";
                    for (int i = 0; i < right.words.Length; i++)
                    {
                        if (i == dotIndex)
                            Presentation += ". ";
                        Presentation += right.words[i]+" ";
                    }

                    if (dotIndex == right.words.Length)//When dot reaches the End of the rule
                        Presentation += ". ";
                    Presentation = Presentation.TrimEnd(' ');

                    //++++++++++++++++++++++++++
                }
            }
            public int DotIndex {
                get
                {
                    return dotIndex;
                }
                set
                {
                    dotIndex = value;

                    //When we move the Dot, we have to update the presentation
                    if(dotIndex == Right.words.Length)
                    {
                        dotIsAtEnd = true;
                    }
                    Presentation = Left + " -> ";
                    for (int i = 0; i < Right.words.Length; i++)
                    {
                        if (i == dotIndex)
                            Presentation += ". ";
                        Presentation += Right.words[i]+" ";
                    }

                    if (dotIndex == Right.words.Length)//When dot reaches the End of the rule
                        Presentation += ". ";
                    Presentation = Presentation.TrimEnd(' ');
                }
            }
            public List<string> lookahead { get; set; }
            public bool isCore { get; set; }
            public string Presentation { get; set; }
            public Statement()
            {
                dotIsAtEnd = false;
                Right = new RightSide();
                DotIndex = 0;
                lookahead = new List<string>(10);
                isCore = false;
            }
        }

        public List<Statement> Statements { get; set; }
        public string passedBy;
        public List<SLRDFASTAT> parent;
        public Dictionary<string,SLRDFASTAT> next_pointers;
        public int State_Number;
 
        public SLRDFASTAT(SLRDFASTAT prt,string passed)
        {
            State_Number = -1;//meghdare default ro -1 mizarim ke age ye 
            //stat dg badesh be hich ja naraft meghdaresh 0 nabashe

            parent = new List<SLRDFASTAT>();
            if (prt != null)
                parent.Add(prt);
            passedBy = passed;

            Statements = new List<Statement>();
            next_pointers = new Dictionary<string, SLRDFASTAT>();
        }

        public void AddStatement (Statement st)
        {
            Statements.Add(st);
        }
    }
}
