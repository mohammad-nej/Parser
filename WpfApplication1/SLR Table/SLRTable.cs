using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace WpfApplication1
{
    class SLRTable
    {
        public Dictionary<SLRTableKey,SLRTableCell> Table;
        public  SLRDFA DFA;
        Dictionary<string,SLRGrammer> Grammer;
        public List<string> Tokens;
        public List<string> Terminals;

        struct StatAndNumber
        {
            public List<SLRTableCell.Type> type;
            public List<RightSide> RightSide;
        }
        private void showTable()
        {

            DataTable table = new DataTable("SLR Table");
            DataColumn column;
            DataRow row;
            string Data="";

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = true;
            column.Unique = true;
            column.ColumnName = "State(s)";
            table.Columns.Add(column);


            //Add other columns
            for(int i=0;i< Tokens.Count; i++)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = Tokens[i];
                column.ReadOnly = true;
                table.Columns.Add(column);
            }

            //define primaryKey
            DataColumn[] Key = new DataColumn[1];
            Key[0] = table.Columns["State(s)"];
            table.PrimaryKey = Key;
            
            //Adding Rows
            foreach(var st in DFA.stats) {
                row = table.NewRow();
                row["State(s)"] = st.State_Number.ToString();
                SLRTableKey key = new SLRTableKey();
                key.statNumber = st.State_Number;
                foreach(var tk in Tokens)
                {
                    key.Token = tk;
                    Data = "";
                    if (!Table.Keys.Contains(key))
                        continue;
                    //hala bayad format bandi ro dorost konim
                    if(Table[key].Action.Count == 0 || !Table.ContainsKey(key))
                    {
                        Data = " ";
                    }else if ( Table[key].Action.Count > 1)
                    {
                        Data = "Shift/Reduce";
                    }else
                    {
                        if (Table[key].Action[0] == SLRTableCell.Type.SHIFT)
                            Data = "S";
                        else if (Table[key].Action[0] == SLRTableCell.Type.REDUCE)
                        {
                            Data = "R";
                        }
                        Data += Table[key].Number;
                    }

                    
                    row[key.Token] = Data;
                }
                table.Rows.Add(row);
            }
        }
       
        public SLRTable(SLRDFA dfa,Dictionary<string , int> tokens,string Type)
        {
            
            Table = new Dictionary<SLRTableKey, SLRTableCell>();
            DFA = dfa;
            Tokens = new List<string>();
            Terminals = new List<string>();
            Grammer = dfa.grammer;
            foreach(var tk in tokens)
            {

                if (tk.Key == "epsilon")
                    continue;
                Tokens.Add(tk.Key);
                Terminals.Add(tk.Key);
            }
            Tokens.Add("$");//moteghaiere payane reshte
            foreach (var tk in Grammer.Keys)
            {
                Tokens.Add("<"+tk+">");
            }
            Terminals.Add("$");
            
            if(Type == "SLR")
                CreateSLRTable();
            if (Type == "LR")
                CreateLRTable(); 
        
        }
        private void CreateLRTable() {
            //Aval taklif halat haiy ke be espilon miran ro malum mikonim
            //Chun epsilon token nist pas dar Tokens vojud nadare bayad jodagane hesab beshe
            for (int i = 0; i < DFA.stats.Count; i++)
            {
                ContainsEpsilonForLR(i);
                for (int j = 0; j < Tokens.Count; j++)
                {

                    SLRTableKey key = new SLRTableKey();
                    SLRTableCell value = new SLRTableCell();
                    value.Action = new List<SLRTableCell.Type>();
                    value.Number = new List<string>();

                    //Key ro darim dorost mikonoim
                    key.statNumber = DFA.stats[i].State_Number;//shomare halat ha az 0 shuru mishe
                    key.Token = Tokens[j];
 
                    if (DFA.stats[i].next_pointers.ContainsKey(key.Token) || DFA.stats[i].next_pointers.Count==0)
                    {

                        var result = defineCellType(DFA.stats[i], Tokens[j]);
                        value.Action = result.type;

                        if (value.Action.Count == 0)
                        {
                            if (!Table.ContainsKey(key))
                                Table.Add(key, value);
                        }
                        for (int k = 0; k < value.Action.Count; k++)
                        {
                            if (value.Action[k] == SLRTableCell.Type.SHIFT)
                            {
                                value.Number.Add(DFA.stats[i].next_pointers[key.Token].State_Number.ToString());
                                if (!Table.ContainsKey(key))
                                {
                                    var tmpValue = new SLRTableCell();
                                    tmpValue.Action = new List<SLRTableCell.Type>();
                                    tmpValue.Number = new List<string>();
                                    tmpValue.Action.Add(value.Action[k]);
                                    tmpValue.Number.Add(value.Number[k]);

                                    Table.Add(key, tmpValue);//Agar be ezaye yek halat chand ta Action dashte bashim
                                                             //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone
                                }

                                else
                                {
                                    SLRTableCell cell = new SLRTableCell();
                                    cell.Action.Add(value.Action[k]);
                                    cell.Number.Add(value.Number[k]);
                                    if (!ContainsKeyValue(key, cell))
                                    {
                                        if (!Table.ContainsValue(cell))
                                        {
                                            Table[key].Action.Add(cell.Action[0]);
                                            Table[key].Number.Add(cell.Number[0]);
                                        }
                                    }
                                }
                            }
                            else if (value.Action[k] == SLRTableCell.Type.REDUCE)

                            {

                                value.Number.Add(result.RightSide[k].Number.ToString());

                                //hala ke fahmidim be koja mirim bayad be ezaye majmue follow e ghanun reduce ro anjam bedim
                                //aval bayad follow haye un ghanun ro peida konoim
                                string tokenBackUp = key.Token;

                                foreach (var tk in Terminals)
                                {
                                    key.Token = tk;


                                    if (!Table.ContainsKey(key))
                                    {
                                        var tmpValue = new SLRTableCell();
                                        tmpValue.Action = new List<SLRTableCell.Type>();
                                        tmpValue.Number = new List<string>();
                                        tmpValue.Action.Add(value.Action[k]);
                                        tmpValue.Number.Add(value.Number[k]);

                                        Table.Add(key, tmpValue);//Agar be ezaye yek halat chand ta Action dashte bashim
                                                                 //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone

                                    }

                                    else
                                    {
                                        SLRTableCell cell = new SLRTableCell();
                                        cell.Action.Add(value.Action[k]);
                                        cell.Number.Add(value.Number[k]);
                                        if (!ContainsKeyValue(key, cell))
                                        {
                                            Table[key].Action.Add(cell.Action[0]);
                                            Table[key].Number.Add(cell.Number[0]);
                                        }
                                    }
                                }
                                key.Token = tokenBackUp;//restoring the back up after finishing

                            }
                        }

                    }

                }
            }
            //Hala ke kar tamum shode bayad halate khateme ro malum konim
            //State 1 be ezaye $ mishe halate accept 
            SLRTableKey key2 = new SLRTableKey();
            SLRTableCell value2 = new SLRTableCell();
            value2.Action = new List<SLRTableCell.Type>();
            value2.Number = new List<string>();
            value2.Action.Add(SLRTableCell.Type.ACCEPT);
            value2.Number.Add("1");
            key2.Token = "$";
            key2.statNumber = 1;

            Table[key2] = value2;
        }
        bool ContainsKeyValue(SLRTableKey key , SLRTableCell cell)
        {
            if (!Table.ContainsKey(key))
                return false;
            if (Table[key].Action.Count == 0)
                return false;
            for(int i = 0; i < Table[key].Action.Count; i++)
            {
                if (Table[key].Action[i] == cell.Action[0])
                    if (Table[key].Number[i] == cell.Number[0])
                        return true;
            }
            return false;
        }
        StatAndNumber defineCellType (SLRDFASTAT stat,string token) {
           
            //moshakhas mikone ke halat shift ie ya kaheshi
            StatAndNumber result = new StatAndNumber();
            result.type = new List<SLRTableCell.Type>();
            result.RightSide = new List<RightSide>();
            string[] words;
            foreach(var rl in stat.Statements)
            {

                words = rl.Right.words;
                for (int i = 0; i <words.Length; i++)
                    if (words[i] == token)
                    {

                        if (i == rl.DotIndex)
                        {
                            if (!result.type.Contains(SLRTableCell.Type.SHIFT))
                            {
                                result.type.Add(SLRTableCell.Type.SHIFT);//yani agar noghte poshte token bud
                                result.RightSide.Add(rl.Right);
                            }

                        }
                        else if (i==words.Length-1 && i+1== rl.DotIndex)
                        {
                                result.type.Add(SLRTableCell.Type.REDUCE);//agar noghte bade token bud
                                result.RightSide.Add(rl.Right);

                        }
                    }
            }
            
            return result;
        }

        private void ContainsEpsilon(int stat_number) {
            //agar dar yek halat ghanuni dashtim bashim ke be epsilon bere mesle 
            //A-> epsilon dar in surat chun epsilon token nist in ghanun hatman bayad be surate
            //kaheshi zaher beshe bana bar in bayad ye rast az rush rad shim 

            //Chun epsilon jozve list token haye ma nist banar in tu tabee CreateSLRTable dakhele 
            //halghe haye for in ghanun ha zaher nemishan bana bar in bayad ba ye tabe joda mohasebe beshan
            foreach (var rl in DFA.stats[stat_number].Statements)
            {
                
                //Shomare grammer ro dar miarim
                int number=-1;
                if (rl.Right.Right == "epsilon")
                    number = rl.Right.Number;
                else
                    continue;
                       

                        //be ezaye follow haye ghanun reduce mikonim
                        foreach (var flws in Grammer[rl.Right.Left].Follows)
                        {

                            SLRTableKey key = new SLRTableKey();
                            key.statNumber = stat_number;

                            SLRTableCell value = new SLRTableCell();

                            //yani agar ghanun be surate A -> epsilon. bud
                            //bayad be ezaye follow haye A reduce konim 
                            value.Number = new List<string>();
                            value.Action = new List<SLRTableCell.Type>();
                            value.isEpsilon = true;
                            value.Number.Add(number.ToString());
                            value.Action.Add(SLRTableCell.Type.REDUCE);

                            key.Token = flws;
                            
                            if (!Table.ContainsKey(key))
                            {
                                
                                Table.Add(key, value);//Agar be ezaye yek halat chand ta Action dashte bashim
                                                      //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone
                            }

                            else
                            {
                                //bayad check konim tekrari add nakonim

                                if (!ContainsKeyValue(key, value))
                                {
                                    Table[key].Action.Add(value.Action[value.Action.Count - 1]);
                                    Table[key].Number.Add(value.Number[value.Action.Count - 1]); //+= value.Number[k] + " ";
                                }
                            }
                        }


                    
            }
        }

        private void ContainsEpsilonForLR(int stat_number)
        {
            //agar dar yek halat ghanuni dashtim bashim ke be epsilon bere mesle 
            //A-> epsilon dar in surat chun epsilon token nist in ghanun hatman bayad be surate
            //kaheshi zaher beshe bana bar in bayad ye rast az rush rad shim 

            //Chun epsilon jozve list token haye ma nist bana bar in tu tabee CreateSLRTable dakhele 
            //halghe haye for in ghanun ha zaher nemishan bana bar in bayad ba ye tabe joda mohasebe beshan

            foreach (var rl in DFA.stats[stat_number].Statements)
            {
                int number = -1;
                if (rl.Right.Right == "epsilon")
                    number = rl.Right.Number;
                else
                    continue;
               
                        //be ezaye follow haye ghanun reduce mikonim
                        foreach (var tk in Terminals)
                        {

                            SLRTableCell value = new SLRTableCell();
                           
                            //yani agar ghanun be surate A -> epsilon. bud
                            //bayad be ezaye follow haye A reduce konim 
                            value.Number = new List<string>();
                            value.Action = new List<SLRTableCell.Type>();

                            SLRTableKey key = new SLRTableKey();
                            key.statNumber = stat_number;
                            key.Token = tk;

                            value.Number.Add(number.ToString());
                            value.Action.Add(SLRTableCell.Type.REDUCE);
                            value.isEpsilon = true;


                            if (!Table.ContainsKey(key))
                            {

                                Table.Add(key, value);//Agar be ezaye yek halat chand ta Action dashte bashim
                                                      //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone
                            }

                            else
                            {
                                //bayad check konim tekrari add nakonim

                                if (!ContainsKeyValue(key, value))
                                {
                                    Table[key].Action.Add(value.Action[value.Action.Count - 1]);
                                    Table[key].Number.Add(value.Number[value.Action.Count - 1]);
                                }
                            }
                        }


                    
            }
        }
        private void CreateSLRTable()
        {
            //Aval taklif halat haiy ke be espilon miran ro malum mikonim
            //chun epsilon token nist va be ezaye epsilon ham jaiy nemirim va 
            //hich vaght ba epsilon be jaiy shift ham nemikonim pas in halat ha hamashun elzaman
            //faghat dar DFA.state[0] hastan va dg niazi nist baghie state ha ro donabelshun begardim


            for (int i = 0; i < DFA.stats.Count; i++)
            {
                ContainsEpsilon(i);
                for(int j = 0; j < Tokens.Count; j++)
                {

                    SLRTableKey key = new SLRTableKey();
                    SLRTableCell value = new SLRTableCell();
                    value.Action = new List<SLRTableCell.Type>();
                    value.Number = new List<string>();
                    
                    //Key ro darim dorost mikonoim
                    key.statNumber = DFA.stats[i].State_Number;//shomare halat ha az 0 shuru mishe
                    key.Token = Tokens[j];

                    if (DFA.stats[i].next_pointers.ContainsKey(key.Token)||DFA.stats[i].next_pointers.Count==0)
                    {
                        //Tashkhise inke halate Shift e ya Reduce (ya har 2)
                        var result= defineCellType(DFA.stats[i], Tokens[j]);
                        value.Action = result.type;
                        
                        if (value.Action.Count == 0)
                        {
                            if (!Table.ContainsKey(key))
                                Table.Add(key, value);
                        }
                        for (int k = 0; k < value.Action.Count; k++)
                        {
                            if (value.Action[k] == SLRTableCell.Type.SHIFT)
                            {
                                value.Number.Add(DFA.stats[i].next_pointers[key.Token].State_Number.ToString());
                                if (!Table.ContainsKey(key))
                                {
                                    var tmpValue = new SLRTableCell();
                                    tmpValue.Action = new List<SLRTableCell.Type>();
                                    tmpValue.Number = new List<string>();
                                    tmpValue.Action.Add(value.Action[k]);
                                    tmpValue.Number.Add(value.Number[k]);

                                    Table.Add(key, tmpValue);//Agar be ezaye yek halat chand ta Action dashte bashim
                                                             //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone
                                }

                                else
                                {
                                    SLRTableCell cell = new SLRTableCell();
                                    cell.Action.Add(value.Action[k]);
                                    cell.Number.Add(value.Number[k]);
                                    if (!ContainsKeyValue(key, cell))
                                    {
                                        if (!Table.ContainsValue(cell))
                                        {
                                            Table[key].Action.Add(cell.Action[0]);
                                            Table[key].Number.Add(cell.Number[0]);
                                        }
                                    }
                                }

                            }
                            else if (value.Action[k] == SLRTableCell.Type.REDUCE)

                            {

                                    value.Number.Add(result.RightSide[k].Number.ToString());

                                    //hala ke fahmidim be koja mirim bayad be ezaye majmue follow e ghanun reduce ro anjam bedim
                                    //aval bayad follow haye un ghanun ro peida konoim

                                    string tokenBackUp = key.Token;
                                    foreach (var flws in Grammer[result.RightSide[k].Left].Follows)
                                    {

                                        key.Token = flws;
                                        
                                        if (!Table.ContainsKey(key))
                                        {
                                            var tmpValue = new SLRTableCell();
                                            tmpValue.Action = new List<SLRTableCell.Type>();
                                            tmpValue.Number = new List<string>();
                                            tmpValue.Action.Add(value.Action[k]);
                                            tmpValue.Number.Add(value.Number[k]);

                                            Table.Add(key, tmpValue);//Agar be ezaye yek halat chand ta Action dashte bashim
                                            //nabayad mostaghim value ro add koniom chun unvaght hame ro ba ham add mikone

                                        }
                                            
                                        else
                                        {
                                            SLRTableCell cell = new SLRTableCell();
                                            cell.Action.Add(value.Action[k]);
                                            cell.Number.Add(value.Number[k]);
                                            if (!ContainsKeyValue(key, cell))
                                            {
                                                Table[key].Action.Add(cell.Action[0]);
                                                Table[key].Number.Add(cell.Number[0]);
                                            }
                                        }
                                    }
                                    key.Token = tokenBackUp;//restoring the back up after finishing
                            }
                        }

                    }
                    
                }
            }

            //Hala ke kar tamum shode bayad halate khateme ro malum konim
            //State 1 be ezaye $ mishe halate accept 
            SLRTableKey key2 = new SLRTableKey();
            SLRTableCell value2 = new SLRTableCell();
            value2.Action = new List<SLRTableCell.Type>();
            value2.Number = new List<string>();
            value2.Action.Add(SLRTableCell.Type.ACCEPT);
            value2.Number.Add("1");
            key2.Token = "$";
            key2.statNumber = 1;

            Table[key2] = value2;
        }

    }
}
