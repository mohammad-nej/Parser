using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using System.Windows.Forms;
using System.Windows.Controls;
namespace WpfApplication1
{
    class Report
    {
        //this class get the reports from parser and lexer
        private Parser Parser;
        private Lexer Lexer;
        public DataSet dataSet;

        public Report(Parser pars , Lexer lex)
        {
            Parser = pars;
            Lexer = lex;
            dataSet = new DataSet();
            CreatTables();
        }

        private void CreatTables()
        {
            //in tabe hameye tabel haiy ke mikhaim ro aval ye dor misaze

            //SLR Table
            dataSet.Tables.Add(CreateSLRTable());

            //First(s) Table
            dataSet.Tables.Add(CreatFirstTable());

            //Follow(s) Table
            dataSet.Tables.Add(CreatFollowTable());

            //SLR_DFA Table
            dataSet.Tables.Add(CreatSLR_DFA_Table());

            //Grammer
            dataSet.Tables.Add(CreatGrammerTable());

            //LL table
            dataSet.Tables.Add(CreateLLTable());

            //LR Table
            dataSet.Tables.Add(CreateLRTable());
        }
        private DataTable CreatFollowTable()
        {
            DataTable table = new DataTable("Follow(s)");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "Rule(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            //column.Unique = true;
            column.ColumnName = "Follow(s)";
            table.Columns.Add(column);

            //add rows 
            foreach (var rl in Parser.LRGrammer)
            {
                string allFollows = "";
                row = table.NewRow();
                row["Rule(s)"] = rl.Key;
                foreach (var frs in rl.Value.Follows)
                {
                    allFollows += frs + " ";
                }
                row["Follow(s)"] = allFollows;
                table.Rows.Add(row);
            }
            return table;

        }
        private DataTable CreatFirstTable()
        {
            DataTable table = new DataTable("First(s)");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "Rule(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            //column.Unique = true;
            column.ColumnName = "First(s)";
            table.Columns.Add(column);

            //add rows 
            foreach(var rl in Parser.LRGrammer)
            {
                string allFirsts = "";
                row = table.NewRow();
                row["Rule(s)"] = rl.Key;
                foreach(var frs in rl.Value.Firsts)
                {
                    allFirsts += frs + " ";
                }
                row["First(s)"] = allFirsts;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable CreateLRTable()
        {
            DataTable table = new DataTable("LR Table");
            DataColumn column;
            DataRow row;
            string Data = "";

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "State(s)";
            table.Columns.Add(column);


            //Add other columns
            for (int i = 0; i < Parser.LRTable.Tokens.Count; i++)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");

                if (Parser.LRTable.Tokens[i] == "(")
                    column.ColumnName = "Opened Prantheses";
                else if (Parser.LRTable.Tokens[i] == ")")
                    column.ColumnName = "Closed Prantheses";
                else
                    column.ColumnName = Parser.LRTable.Tokens[i];

                column.ReadOnly = true;
                table.Columns.Add(column);
            }

            //define primaryKey
            DataColumn[] Key = new DataColumn[1];
            Key[0] = table.Columns["State(s)"];
            table.PrimaryKey = Key;

            //Adding Rows
            foreach (var st in Parser.LRTable.DFA.stats)
            {
                row = table.NewRow();
                row["State(s)"] = st.State_Number.ToString();
                SLRTableKey key = new SLRTableKey();
                key.statNumber = st.State_Number;
                foreach (var tk in Parser.LRTable.Tokens)
                {

                    key.Token = tk;
                    Data = "";

                    if (!Parser.LRTable.Table.Keys.Contains(key))
                        continue;
                    //hala bayad format bandi ro dorost konim
                    if (Parser.LRTable.Table[key].Action.Count == 0)
                    {
                        Data = " ";
                    }

                    else
                    {
                        for (int k = 0; k < Parser.LRTable.Table[key].Action.Count; k++)
                        {
                            if (Parser.LRTable.Table[key].Action[k] == SLRTableCell.Type.SHIFT)
                            {
                                Data += "S";
                                Data += Parser.LRTable.Table[key].Number[k] + " ";
                            }

                            else if (Parser.LRTable.Table[key].Action[k] == SLRTableCell.Type.REDUCE)
                            {
                                Data += "R";
                                Data += Parser.LRTable.Table[key].Number[k] + " ";
                            }
                            else if (Parser.LRTable.Table[key].Action[k] == SLRTableCell.Type.ACCEPT)
                            {
                                Data = "ACCEPT";
                            }
                        }

                    }
                    //The characters below cause error while creating the Table so we have to 
                    //replace them with their names to avoid error!
                    if (key.Token == "(")
                        key.Token = "Opened Prantheses";
                    else if (key.Token == ")")
                        key.Token = "Closed Prantheses";
                    Data = Data.TrimEnd(' ');
                    row[key.Token] = Data;
                }
                table.Rows.Add(row);
            }

            return table;
        }
        private DataTable CreateSLRTable()
        {
            DataTable table = new DataTable("SLR Table");
            DataColumn column;
            DataRow row;
            string Data = "";

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "State(s)";
            table.Columns.Add(column);


            //Add other columns
            for (int i = 0; i < Parser.SLRTable.Tokens.Count; i++)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");

                if (Parser.SLRTable.Tokens[i] == "(")
                    column.ColumnName = "Opened Prantheses";
                else if (Parser.SLRTable.Tokens[i] == ")")
                    column.ColumnName = "Closed Prantheses";
                else
                column.ColumnName = Parser.SLRTable.Tokens[i];

                column.ReadOnly = true;
                //column.Unique = true;
                table.Columns.Add(column);
            }

            //define primaryKey
            DataColumn[] Key = new DataColumn[1];
            Key[0] = table.Columns["State(s)"];
            table.PrimaryKey = Key;

            //Adding Rows
            foreach (var st in Parser.SLRTable.DFA.stats)
            {
                row = table.NewRow();
                row["State(s)"] = st.State_Number.ToString();
                SLRTableKey key = new SLRTableKey();
                key.statNumber = st.State_Number;
                foreach (var tk in Parser.SLRTable.Tokens)
                {
                   
                    key.Token = tk;
                    Data = "";

                    if (!Parser.SLRTable.Table.Keys.Contains(key))
                        continue;
                    //hala bayad format bandi ro dorost konim
                    if (Parser.SLRTable.Table[key].Action.Count == 0)
                    {
                        Data = " ";
                    }

                    else
                    {
                        for (int k = 0; k < Parser.SLRTable.Table[key].Action.Count; k++)
                        {
                            if (Parser.SLRTable.Table[key].Action[k] == SLRTableCell.Type.SHIFT)
                            {
                                Data += "S";
                                Data += Parser.SLRTable.Table[key].Number[k]+" ";
                            }

                            else if (Parser.SLRTable.Table[key].Action[k] == SLRTableCell.Type.REDUCE)
                            {
                                Data += "R";
                                Data += Parser.SLRTable.Table[key].Number[k]+" ";
                            }
                            else if (Parser.SLRTable.Table[key].Action[k] == SLRTableCell.Type.ACCEPT)
                            {
                                Data = "ACCEPT";
                            }
                        }
                        
                    }
                    if (key.Token == "(")
                        key.Token = "Opened Prantheses";
                    else if (key.Token == ")")
                        key.Token = "Closed Prantheses";
                    Data = Data.TrimEnd(' ');
                    row[key.Token] = Data;
                }
                table.Rows.Add(row);
            }

            return table;
        }
        private DataTable CreatSLR_DFA_Table()
        {
            DataTable table = new DataTable("SLR DFA");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "State(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Description(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Parent(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Child(s)";
            table.Columns.Add(column);

            var dfa = Parser.SLRTable.DFA;



            for ( int i = 0; i < dfa.stats.Count; i++)
            {
                row = table.NewRow();
                row["State(s)"] = "S" + dfa.stats[i].State_Number;
                string disc = "";
                foreach( var rl in dfa.stats[i].Statements)
                {
                    disc += rl.Presentation + "\r\n";
                }
                row["Description(s)"] = disc;
                disc = "";
                
                //Parents
                foreach(var prt in dfa.stats[i].parent)
                {
                    disc += "S" + prt.State_Number+",";
                }
                disc = disc.TrimEnd(',');
                row["Parent(s)"] = disc;

                //Childs
                disc = ""
;               foreach(var child in dfa.stats[i].next_pointers)
                {

                    if (child.Value == null)
                        continue;
                    disc += "S" + dfa.stats[i].State_Number + " with " + child.Value.passedBy + " --> S" + child.Value.State_Number;
                    disc += "\r\n"; 
                }
                disc = disc.TrimEnd();
                row["Child(s)"] = disc;

                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable CreatGrammerTable()
        {
            DataTable table = new DataTable("Grammer");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Rule(s)";
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.ColumnName = "Right Side";
            table.Columns.Add(column);

            foreach(var rl in Parser.LRGrammer)
            {
                row = table.NewRow();
                row["Rule(s)"] = rl.Value.left;

                string right="";
                foreach( var rgt in rl.Value.Rights)
                {
                    right += rgt.Right + "    Number:"+rgt.Number+ "\r\n";
                }
                row["Right Side"] = right;
                table.Rows.Add(row);
            }
            return table;
        }
        private DataTable CreateLLTable()
        {
            DataTable table = new DataTable("LL Table");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "Token(s)";
            table.Columns.Add(column);

            foreach(var gram in Parser.LRGrammer)
            {
                if (gram.Key == "start'")
                    continue;
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ReadOnly = true;
                //column.Unique = true;
                column.ColumnName = gram.Key;
                if(!table.Columns.Contains(gram.Key))
                    table.Columns.Add(column);
            }
            KeyValuePair<string, int>[] tokens = Parser.tokenMap.ToArray();
            int counter=0;
            foreach (var tk in tokens)
            {
                
                row = table.NewRow();
                row["Token(s)"] = tk.Key;
                foreach (var gram in Parser.LRGrammer)
                {
                    if (gram.Key == "start'")
                        continue;
                    //row[gram.Key] = Parser.LLTable[counter][tk.Value];
                    string temp = "";
                    LLTableKey key = new LLTableKey();
                    key.Rule = gram.Key;
                    key.Token = tk.Key;
                    if(Parser.LLTable.ContainsKey(key))
                        foreach (var vl in Parser.LLTable[key])
                        {
                            temp += vl+",";
                        }
                    temp = temp.TrimEnd(',');
                    row[gram.Key] = temp;
                    counter++;
                }
                table.Rows.Add(row);
                counter = 0;
            }
            return table;
            
        }

       
    }
}
