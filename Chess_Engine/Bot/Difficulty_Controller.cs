using Chess_Engine.Core;
using Chess_Engine.Core.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chess_Engine.Bot
{
    public class ADifficulty_Controller
    {
        public EDifficulty Current_Difficulty { get; private set; }
        public ADifficulty_Settings Current_Settings { get; private set; }
        private readonly Random random = new Random();
        private string Settings_Path = "";

        public ADifficulty_Controller()
        {
            Current_Difficulty = EDifficulty.Intermediate;
            Current_Settings = ADifficulty_Settings.Get_Preset(Current_Difficulty);
            Load_Settings();
        }

        public void Set_Difficulty(EDifficulty difficulty)
        {
            Current_Difficulty = difficulty;
            Current_Settings = ADifficulty_Settings.Get_Preset(Current_Difficulty);
            Current_Settings.Apply_Randomness(random);
            Save_Settings();
        }

        public void Update_Settings(ADifficulty_Settings settings)
        {
            Current_Settings = settings;
            Current_Difficulty = EDifficulty.Master;
            Save_Settings();
        }

        public void Adjust_For_Position(ABoard board)
        {
            if (Current_Difficulty <= EDifficulty.Advanced)
            {
                if (board.Ply_Count < 10)
                {
                    Current_Settings.Max_Depth = Math.Max(Current_Settings.Max_Depth - 1, Current_Settings.Min_Depth);
                }
                else if (Is_Endgame(board))
                {
                    Current_Settings.Max_Depth += 2;
                }
            }
        }

        private bool Is_Endgame(ABoard board)
        {
            int total_material = 0;

            // Используем списки фигур вместо bitboards для подсчета
            total_material  += board.Pawns[ABoard.White_Index].Count * AEvaluator.Pawn_Value;
            total_material  += board.Pawns[ABoard.Black_Index].Count * AEvaluator.Pawn_Value;
            total_material  += board.Knights[ABoard.White_Index].Count * AEvaluator.Knight_Value;
            total_material  += board.Knights[ABoard.Black_Index].Count * AEvaluator.Knight_Value;
            total_material  += board.Bishops[ABoard.White_Index].Count * AEvaluator.Bishop_Value;
            total_material  += board.Bishops[ABoard.Black_Index].Count * AEvaluator.Bishop_Value;
            total_material  += board.Rooks[ABoard.White_Index].Count * AEvaluator.Rook_Value;
            total_material  += board.Rooks[ABoard.Black_Index].Count * AEvaluator.Rook_Value;
            total_material  += board.Queens[ABoard.White_Index].Count * AEvaluator.Queen_Value;
            total_material += board.Queens[ABoard.Black_Index].Count * AEvaluator.Queen_Value;

            return total_material <= AEvaluator.Rook_Value * 2;
        }

        private void Save_Settings()
        {
            try
            {
                var data = new
                {
                    Difficulty = Current_Difficulty,
                    Settings = Current_Settings
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Settings_Path, json);
            }
            catch { /* Ignore save errors */ }
        }

        private void Load_Settings()
        {
            try
            {
                if (File.Exists(Settings_Path))
                {
                    string json = File.ReadAllText(Settings_Path);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    if (data != null && data.ContainsKey("Difficulty"))
                    {
                        var difficulty = (EDifficulty)Enum.Parse(typeof(EDifficulty), data["Difficulty"].ToString());
                        Set_Difficulty(difficulty);
                    }
                }
            }
            catch { /* Ignore load errors */ }
        }
    }
}
