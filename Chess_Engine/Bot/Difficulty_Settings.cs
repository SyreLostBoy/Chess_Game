using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Bot
{
    public class ADifficulty_Settings
    {
        public int Max_Depth { get; set; }
        public int Min_Depth { get; set; }
        public bool Use_Advanced_Eval { get; set; }
        public bool Use_Extensions { get; set; }
        public bool Use_LMR { get; set; }
        public bool Use_Null_Move { get; set; }
        public int TT_Size_MB { get; set; }
        public bool Use_Endgame_Knowledge { get; set; }
        public int Move_Ordering_Quality { get; set; }
        public int Contempt_Factor { get; set; }
        public float Skill_Randomness { get; set; }
        public bool Use_Opening_Book { get; set; }
        public int Max_Book_Ply { get; set; }
        public int Max_Search_Time_Ms { get; set; }
        public int Min_Search_Time_Ms { get; set; }
        public bool Use_Time_Management { get; set; }

        public ADifficulty_Settings()
        {
            Max_Depth = 5;
            Min_Depth = 3;
            Use_Advanced_Eval = true;
            Use_Extensions = true;
            Use_LMR = true;
            Use_Null_Move = false;
            TT_Size_MB = 32;
            Use_Endgame_Knowledge = true;
            Move_Ordering_Quality = 2;
            Contempt_Factor = 25;
            Skill_Randomness = 0.1f;
            Use_Opening_Book = true;
            Max_Book_Ply = 16;
            Max_Search_Time_Ms = 5000;
            Min_Search_Time_Ms = 1000;
            Use_Time_Management = true;
        }

        public static ADifficulty_Settings Get_Preset(EDifficulty difficulty)
        {
            switch (difficulty)
            {
                case EDifficulty.Beginner:
                    return new ADifficulty_Settings 
                    {
                        Max_Depth = 3,
                        Min_Depth = 1,
                        Use_Advanced_Eval = false,
                        Use_Extensions = false,
                        Use_LMR = false,
                        Use_Null_Move = false,
                        TT_Size_MB = 16,
                        Use_Endgame_Knowledge = false,
                        Move_Ordering_Quality = 1,
                        Contempt_Factor = 50,
                        Skill_Randomness = 0.2f,
                        Use_Opening_Book = true,
                        Max_Book_Ply = 5,
                        Max_Search_Time_Ms = 2000,
                        Min_Search_Time_Ms = 500,
                        Use_Time_Management = false
                    };
                case EDifficulty.Intermediate:
                    return new ADifficulty_Settings
                    {
                        Max_Depth = 5,
                        Min_Depth = 3,
                        Use_Advanced_Eval = true,
                        Use_Extensions = true,
                        Use_LMR = true,
                        Use_Null_Move = false,
                        TT_Size_MB = 32,
                        Use_Endgame_Knowledge = true,
                        Move_Ordering_Quality = 2,
                        Contempt_Factor = 25,
                        Skill_Randomness = 0.1f,
                        Use_Opening_Book = true,
                        Max_Book_Ply = 16,
                        Max_Search_Time_Ms = 5000,
                        Min_Search_Time_Ms = 1000,
                        Use_Time_Management = true
                    };
                case EDifficulty.Advanced:
                    return new ADifficulty_Settings
                    {
                        Max_Depth = 8,
                        Min_Depth = 5,
                        Use_Advanced_Eval = true,
                        Use_Extensions = true,
                        Use_LMR = true,
                        Use_Null_Move = true,
                        TT_Size_MB = 64,
                        Use_Endgame_Knowledge = true,
                        Move_Ordering_Quality = 3,
                        Contempt_Factor = 0,
                        Skill_Randomness = 0.05f,
                        Use_Opening_Book = true,
                        Max_Book_Ply = 10,
                        Max_Search_Time_Ms = 5000,
                        Min_Search_Time_Ms = 2000,
                        Use_Time_Management = true
                    };
                case EDifficulty.Expert:
                    return new ADifficulty_Settings
                    {
                        Max_Depth = 32,
                        Min_Depth = 8,
                        Use_Advanced_Eval = true,
                        Use_Extensions = true,
                        Use_LMR = true,
                        Use_Null_Move = true,
                        TT_Size_MB = 128,
                        Use_Endgame_Knowledge = true,
                        Move_Ordering_Quality = 4,
                        Contempt_Factor = -25,
                        Skill_Randomness = 0f,
                        Use_Opening_Book = true,
                        Max_Book_Ply = 24,
                        Max_Search_Time_Ms = 7000,
                        Min_Search_Time_Ms = 2000,
                        Use_Time_Management = true
                    };
                case EDifficulty.Master:
                    return new ADifficulty_Settings
                    {
                        Max_Depth = 256,
                        Min_Depth = 8,
                        Use_Advanced_Eval = true,
                        Use_Extensions = true,
                        Use_LMR = true,
                        Use_Null_Move = true,
                        TT_Size_MB = 128,
                        Use_Endgame_Knowledge = true,
                        Move_Ordering_Quality = 5,
                        Contempt_Factor = -50,
                        Skill_Randomness = 0f,
                        Use_Opening_Book = true,
                        Max_Book_Ply = 24,
                        Max_Search_Time_Ms = 10000,
                        Min_Search_Time_Ms = 2000,
                        Use_Time_Management = true
                    };
                default: return new ADifficulty_Settings();
            }
        }

        public void Apply_Randomness(Random random)
        {
            if (Skill_Randomness > 0)
            {
                float random_factor = 1 + ((float)random.NextDouble() * 2 - 1) * Skill_Randomness;
                Max_Depth = (int)(Max_Depth * random_factor);
                Max_Depth = Math.Max(Min_Depth + 1, Max_Depth);
                Contempt_Factor += random.Next(-10, 10);
            }
        }
    }
}
