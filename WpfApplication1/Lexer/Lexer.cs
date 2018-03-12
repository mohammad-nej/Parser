using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data;
namespace WpfApplication1
{
    class Lexer
    {
        
        System.Windows.Controls.RichTextBox workingBox = new System.Windows.Controls.RichTextBox();
        

        public string[] CTN { get; set; } //Char to Number

        int[][] DFA;

        int[] Non_Final;

        int currentState { get; set; }
        string mainText { get; set; }
        public bool FirstTime;
        private string[] keyWords { get; set; }
        private string commentCharacter { get; set; }
        private string[] delimeters { get; set; }
        public bool allVariablesFilled { get; set; }
        private Visualizer visual;
       
        private List<WordPointer> listOfTokens = new List<WordPointer>();
        private int currentTokenIndex { get; set; }

        //***************************************Constructur************************************
        public Lexer(System.Windows.Controls.RichTextBox main//,System.Windows.Controls.ListView grid
           ,string DFApath,string KeyWordspath )
        {
            currentState = 0;
            workingBox = main;
            FirstTime = true;
            visual = new Visualizer(workingBox);
            FillDFA(DFApath);
            ReadKeywordsFromFile(KeyWordspath);
        }
        
        //***********************Search for the CharNum***************Forms**************************
        short SearchForCharNum(char currentChar)
        {  
            string character;
            
            if (((int)currentChar >= 97) && ((int)currentChar <= 122) ||((int)currentChar>=65 && (int)currentChar<=90) )//English letter
                character = "A";
            else if (((int)currentChar >= 48) && ((int)currentChar <= 57))//Number
                character = "2";
            else if (currentChar == '\n')//to fix some errors
                character = "\r";
            else if (currentChar == '"')
                character = "\"";
            else
                character = currentChar.ToString();
            
            for (int i = 0; i < CTN.Length; i++)
            {
                if (CTN[i] == character)
                    return (short)i;

            }
            return -1;
        }
        //**************************************Filling DFA************************************
        
        /// <summary>
        /// Read Lexer DFA from file
        /// </summary>
        public void FillDFA(string path)
        {
            string line = "";
            int lineNumber = 0;
            int table_X;
            if (!File.Exists(path))
            {
                return;

            }

            using (StreamReader sr = File.OpenText(path))
            {
                while ((line = sr.ReadLine()) != null)
                {

                    if (line.StartsWith("*"))//skip the comments
                        continue;
                    switch (lineNumber)
                    {
                        case 0:
                            CTN = line.Split(' ');
                            for (int i = 0; i < CTN.Length;i++ )
                            {
                                if (CTN[i] == "enter")
                                    CTN[i] = "\r";
                                if (CTN[i] == "space")
                                    CTN[i] = " ";
                            }
                            lineNumber++;
                            break;
                        case 1:
                            Non_Final = line.Split().Select(int.Parse).ToArray();
                            lineNumber++;
                            break;
                        case 2:
                            table_X = Convert.ToInt32(line);
                            lineNumber++;
                            
                            DFA = new int[table_X][];
                            break;
                        default:
                            {
                                
                                int[] DFA_Y;
                                DFA_Y = line.Split().Select(int.Parse).ToArray();
                                DFA[lineNumber - 3] = DFA_Y;
                            }
                            lineNumber++;
                            break;
                    }//end of switch
                }//end of while


            }
        }
        public bool isFinal(int state)
        {
            //ru halat haye nonFinal search mikone agar did ye halat tu nonFinal ha nabud pas final e va true pas mide
            for (int i = 0; i < Non_Final.Length; i++)
            {
                if (state == Non_Final[i])
                    return false;
            }
            return true;
        }


        /// <summary>
        /// gets a startPoint and start lexing untill it found a token or gets error
        /// </summary>
        public DFAOut traceViaDFA(TextPointer startPoint)
        {
            DFAOut outPut = new DFAOut();
            outPut.accept = false;
            
            TextPointer nextPostion;
            int currentState = 1;//Since 0 is used for rejecting, starting state is 1.
            int charNumber = 0;
            int CurrentSateCopy = currentState;
            string wholeText = "";
            TextRange text = new TextRange(startPoint, startPoint);//just to avoid null exception
            TextRange textNext;
            
            char currentChar;
            bool notEnd = true;
            while (notEnd) {                
                nextPostion = startPoint.GetNextInsertionPosition(LogicalDirection.Forward);
                if (nextPostion == null)
                    break;
                 text = new TextRange(startPoint, nextPostion);
                 
                 if (text.Text == "\r\n")
                 {
                     currentChar = '\r';
                 }
                 else {
                     currentChar = text.Text.ToCharArray()[0];
                 }
                charNumber = SearchForCharNum(currentChar);
                if (charNumber != -1)
                    currentState = DFA[currentState][charNumber];
                else
                {
                        //You can make the Lexer to accept any character on certains states . for example you might want the
                        //lexer to accept any character like persian language or ... on comments and inside strings. then you should
                        //uncomment the code below and replace the states with your own.
                        //Keep in mind that there is another part like this just a few lines below that you should change it too.

                         //if(currentState!=14&&currentState!=15&&currentState!=6&&currentState!=7&&currentState!=8&&currentState!=10&&currentState!=11){
                          // startPoint = nextPostion;
                        //break;
                    //}
                }
                if (currentState == 0)
                {
                    break;
                }
                if (isFinal(currentState)) { 
                     
                    outPut.accept = true;
                        if (nextPostion.GetNextInsertionPosition(LogicalDirection.Forward) != null)
                        {
                            textNext = new TextRange(nextPostion, nextPostion.GetNextInsertionPosition(LogicalDirection.Forward));
                            
                            if (textNext.Text == "\r\n")
                            {
                                currentChar = '\r';
                            }
                            else
                            {
                                currentChar = textNext.Text.ToCharArray()[0];
                            }

                            charNumber = SearchForCharNum(currentChar);
                            if (charNumber != -1)
                                CurrentSateCopy = DFA[currentState][charNumber];
                            else
                            {
                            //If you want lexer to accept any character on certain circumstance you should enter them here
                            //like you want any characters to be accepted inside comments or strings
                            //if (currentState != 14 && currentState != 15 && currentState != 6 && currentState != 7 && currentState != 8 && currentState != 10 && currentState != 11)
                            //{

                            //  notEnd = false;
                            //}  
                        }

                        if (!isFinal(CurrentSateCopy))
                            {
                                notEnd = false;
                            }
                            
                        }
                    
                }//end of final
                else {
                    
                     startPoint = nextPostion; 
                    outPut.accept = false;
                }
                    startPoint = nextPostion;
                    wholeText += text.Text;
            }
            //Decalaring types
            if ((currentState >= 6) && (currentState <= 12))
            {
                outPut.text = "comment";
            }
            else if ((currentState >= 14) && (currentState <= 16))
                outPut.text = "string";
            else if (currentState == 2)
                outPut.text = "id";
            else if (currentState == 0||currentState==1)
                outPut.text = "Lexer Error";
            else if (currentState == 3)
                outPut.text = "num";
            else
                outPut.text = wholeText;
            outPut.pointer = startPoint;//noghte akhar ro bar migardune
            return outPut;

        }

        public TextPointer getCurrentChar(TextPointer current, LogicalDirection direction)
        {
            // in tabe ye textPointer migire va az unja ta berese be ye delimeter dar jahati behesh migim harekat mikone
            //va dar akhar texpointer noghte payani ro bar migardune
            TextPointer nextPosition = current;
            string test;
            while (1 == 1)
            {
                nextPosition = current.GetNextInsertionPosition(direction);
                if (nextPosition == null)
                    break;
                test = new TextRange(current, nextPosition).Text;
                for (int i = 0; i < delimeters.Length; i++)
                    if (test == delimeters[i]) {
                        break;
                    }
                    current = nextPosition;
            }
            return current;
        }

        /// <summary>
        /// This function select a part of the text based on it's color. it is used for multi-line comments and strings
        /// </summary>
        public TextPointer getColorPoint(TextPointer current, LogicalDirection direction,TextPointer enterdPosition,Brush brush)
        {
            
            
            TextPointer nextPosition = current;
            TextPointer StartingPosition = current;
            bool isGreen = true;
            TextRange text = new TextRange(current, nextPosition);
            while (isGreen)
            {
                nextPosition = current.GetNextInsertionPosition(direction);
                if (nextPosition == null)
                    break;
                 text = new TextRange(current, nextPosition);
                 
                
                    if (text.GetPropertyValue(TextElement.ForegroundProperty) != brush)
                    {

                        isGreen = false;
                    }
                    else
                        current = nextPosition; 
            }
            return current;
        }
        
        public bool ReadKeywordsFromFile(string path)
        {

            string line = "";
            int lineNumber = 0;
            try
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("*"))
                            continue;
                        if (lineNumber == 0)
                        {
                            keyWords = line.Split();
                            lineNumber++;
                        }

                        else if (lineNumber == 1)
                        {
                            delimeters = line.Split();
                            for (int i = 0; i < delimeters.Length; i++)
                            {
                                if (delimeters[i] == "space")
                                {
                                    delimeters[i] = " ";
                                    continue;
                                }
                                if (delimeters[i] == "enter")
                                    delimeters[i] = "\r\n";
                            }
                            lineNumber++;
                        }
                        else { /*vase age painesh chizi ezafe kardim eror nade */ }

                    }
                    allVariablesFilled = true;
                }
            }
            catch (Exception )
            {
                System.Windows.MessageBox.Show("Unable to read from file:" + path);
                return false;
            }
            return true;
        }
        /// <summary>
        /// This function use for lexing while the user is typing the input, it might significantly reduce 
        /// the speed.
        /// </summary>
        /// <param name="beforTextEnter"></param>
        /// <returns></returns>
        public bool getLastChar(TextPointer beforTextEnter)
        {
            if (FirstTime == false)
                return false ;
            FirstTime = false;
            bool lexError = false;
            TextPointer currentPosition;
            TextPointer endOfCurrent = workingBox.Selection.End;
            currentPosition = workingBox.Selection.End.GetInsertionPosition(LogicalDirection.Backward);

            TextPointer enteredPosition = currentPosition;
            GroupWordPointer GroupOfWords = new GroupWordPointer(enteredPosition);


            if (currentPosition == null)
            {
                FirstTime = true;
                return false;
            }
            TextPointer startOFWord = getCurrentChar(workingBox.Selection.Start, LogicalDirection.Backward);
            TextRange delemiter = new TextRange(workingBox.Selection.Start, startOFWord);
            string text = workingBox.Selection.Text;
            if (delemiter.Text == "")//a delimeter here
            {
                if (startOFWord.GetNextInsertionPosition(LogicalDirection.Backward) != null)
                    if (new TextRange(startOFWord, startOFWord.GetNextInsertionPosition(LogicalDirection.Backward)).GetPropertyValue(TextElement.ForegroundProperty) == Brushes.DarkGreen)
                    {
                        Brush brush = Brushes.DarkGreen;
                        startOFWord = getColorPoint(startOFWord, LogicalDirection.Backward, enteredPosition, Brushes.DarkGreen);
                    }
                    else if (new TextRange(startOFWord, startOFWord.GetNextInsertionPosition(LogicalDirection.Backward)).GetPropertyValue(TextElement.ForegroundProperty) == Brushes.DarkOrange)
                    {

                        Brush brush = Brushes.DarkOrange;
                        startOFWord = getColorPoint(startOFWord, LogicalDirection.Backward, enteredPosition, brush);

                    }
                    else { 
                    if ( workingBox.Selection.Start.GetNextInsertionPosition(LogicalDirection.Backward)!=null)
                        startOFWord=getCurrentChar(workingBox.Selection.Start.GetNextInsertionPosition(LogicalDirection.Backward),LogicalDirection.Backward);
                    }

            }
            else
            {
                if (startOFWord.GetNextInsertionPosition(LogicalDirection.Forward) != null)
                {
                    if (new TextRange(startOFWord, startOFWord.GetNextInsertionPosition(LogicalDirection.Forward)).GetPropertyValue(TextElement.ForegroundProperty) == Brushes.DarkGreen)
                        startOFWord = getColorPoint(startOFWord, LogicalDirection.Backward, enteredPosition, Brushes.DarkGreen);
                    else if (new TextRange(startOFWord, startOFWord.GetNextInsertionPosition(LogicalDirection.Forward)).GetPropertyValue(TextElement.ForegroundProperty) == Brushes.DarkOrange)
                        startOFWord = getColorPoint(startOFWord, LogicalDirection.Backward, enteredPosition, Brushes.DarkOrange);
                }

            }
            
            
            List<WordPointer> listOFWords = new List<WordPointer>();
            DFAOut outPut = new DFAOut();
            outPut.pointer = startOFWord;
            while (true)
            {
                outPut = traceViaDFA(startOFWord);
                WordPointer current = new WordPointer(startOFWord, outPut.pointer);
                current.lexingOK = outPut.accept;
                if (outPut.text == "comment")
                    current.isComment = true;
                if (outPut.text == "string")
                    current.isString = true;
                
                 if (current.text=="")//traceDFA returns "" when encounters a delimeter 
                    break;
                 current.Type = outPut.text;
                 if (current.lexingOK == false)
                     lexError = true;
                GroupOfWords.addWord(current);
                listOFWords.Add(current);
               startOFWord = outPut.pointer;

            }
            workingBox.Selection.Select(enteredPosition, enteredPosition);
            
            WordType text3 = new WordType();
            text3.type = WordType.boundType.TEXT;

            for (int i = 0; i < GroupOfWords.arrayIndex; i++)
            {
                visual.ColorText(GroupOfWords.GroupOfWords[i], text3, enteredPosition);
            }

            text3.type = WordType.boundType.LEXERROR;

            for (int i = 0; i < GroupOfWords.arrayIndex; i++)
            {
                if (GroupOfWords.GroupOfWords[i].lexingOK == false)
                    visual.ColorText(GroupOfWords.GroupOfWords[i], text3, enteredPosition);
            }

            for (int i = 0; i < GroupOfWords.arrayIndex; i++)
            {
                text3.type = WordType.boundType.KEYWORD;
                for (int j = 0; j < keyWords.Length; j++)
                {
                    if (GroupOfWords.GroupOfWords[i].text == keyWords[j])
                    {
                        GroupOfWords.GroupOfWords[i].Type = keyWords[j];
                        visual.ColorText(GroupOfWords.GroupOfWords[i], text3, enteredPosition);// yani range keyWordColor
                        break;
                    }
                }

            }

            text3.type = WordType.boundType.COMMENT;
            for (int i = 0; i < GroupOfWords.arrayIndex; i++)
            {
                if (GroupOfWords.GroupOfWords[i].isComment == true)
                {
                    visual.ColorText(GroupOfWords.GroupOfWords[i], text3, enteredPosition);
                }
            }
            text3.type = WordType.boundType.STRING;
            for (int i = 0; i < GroupOfWords.arrayIndex; i++)
            {
                if (GroupOfWords.GroupOfWords[i].isString == true)
                {
                    visual.ColorText(GroupOfWords.GroupOfWords[i], text3, enteredPosition);
                }

                
            }
           
            FirstTime = true;
            return lexError;
        }

        public bool staticLexer() {
            FirstTime = false;
            bool lexError = false;
            currentTokenIndex = 0;
            if (listOfTokens.Count != 0)
                listOfTokens.RemoveRange(0, listOfTokens.Count);
            DFAOut output = new DFAOut();
            output.pointer = workingBox.Document.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
            TextPointer starOfWord = workingBox.Document.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
            WordPointer current;
            while (true) {
                output = traceViaDFA(output.pointer);
                current = new WordPointer(starOfWord, output.pointer);
                current.lexingOK = output.accept;
                if (output.text == "comment")
                    current.isComment = true;
                if (output.text == "string")
                    current.isString = true;
                
                if (current.text == "")//traceDFA return "" when encouters with a delimeter 
                    break;
                if (current.lexingOK == false)
                {
                    lexError = true;
                }
                if (current.text.Contains(">"))
                    current.text=current.text.Replace(">", "\\>");
                if (current.text.Contains("<"))
                    current.text = current.text.Replace("<", "\\<");
                if (current.text.Contains("$"))
                    current.text = current.text.Replace("<$", "\\$");

                current.Type = output.text;

                if(current.text!="\r\n"&&current.isComment==false&&current.text!=" ")
                listOfTokens.Add(current);
                starOfWord = output.pointer;
            }

            
            //This part is for coloring the text
            TextPointer enteredPosition = workingBox.Document.ContentStart;
            WordType text3 = new WordType();
            text3.type = WordType.boundType.TEXT;

            for (int i = 0; i < listOfTokens.Count; i++)
            {
                visual.ColorText(listOfTokens[i], text3, enteredPosition);//aval hame matn ro siah mikonim
            }

            text3.type = WordType.boundType.LEXERROR;

            for (int i = 0; i < listOfTokens.Count; i++)
            {
                if (listOfTokens[i].lexingOK == false)
                    visual.ColorText(listOfTokens[i], text3, enteredPosition);
            }

            for (int i = 0; i < listOfTokens.Count; i++)
            {
                                text3.type = WordType.boundType.KEYWORD;
                for (int j = 0; j < keyWords.Length; j++)
                {
                    if (listOfTokens[i].text == keyWords[j])
                    {
                        listOfTokens[i].Type = keyWords[j];
                        visual.ColorText(listOfTokens[i], text3, enteredPosition);// yani range keyWordColor
                        break;
                    }
                }

            }

            text3.type = WordType.boundType.COMMENT;
            for (int i = 0; i < listOfTokens.Count; i++)
            {
                if (listOfTokens[i] .isComment == true)
                {
                    visual.ColorText(listOfTokens[i], text3, enteredPosition);
                }
            }
            text3.type = WordType.boundType.STRING;
            for (int i = 0; i < listOfTokens.Count; i++)
            {
                if (listOfTokens[i].isString == true)
                {
                    visual.ColorText(listOfTokens[i], text3, enteredPosition);
                }


            }

            currentTokenIndex = 0;
             
            FirstTime = true;
            return lexError;
        }
        public struct DFAOut
        {
            public TextPointer pointer { get; set; }
            public bool accept
            {
                get;
                set;
            }
            public string text { get; set; }
            
        };
        public void gotoNextToken() {
            currentTokenIndex++;
        }
        public string RemainingInput(LexerOutPut.OutPut lexer_output)
        {
            string output = "";
            if(currentTokenIndex<listOfTokens.Count)
                for(int i = currentTokenIndex; i < listOfTokens.Count;i++) {
                    if (lexer_output == LexerOutPut.OutPut.TEXT)
                        output += listOfTokens[i].text;
                    else if (lexer_output == LexerOutPut.OutPut.TYPE)
                        output += listOfTokens[i].Type;
                }
            if (!output.EndsWith("$"))
            {
                output += "$";
            }
            
            return output;
        }
        /// <summary>
        /// returns the current token.
        /// </summary>
        /// <returns>a Token</returns>
        public WordPointer seeToken() {
            if (currentTokenIndex < listOfTokens.Count)
                return listOfTokens[currentTokenIndex];
            else
                return new WordPointer("$");
        }
    }
}

