using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public struct SBook_Move
    {
        public readonly string Move_String;
        public readonly int Num_Times_Played;

        public SBook_Move(string move_string, int num_times_player)
        {
            Move_String = move_string;
            Num_Times_Played = num_times_player;
        }
    }

    public class AOpening_Book
    {
        private readonly Dictionary<string, SBook_Move[]> Moves_By_Position;
        private readonly Random Rand_Gen;

        public AOpening_Book(string file)
        {
            Rand_Gen = new Random();
            Span<string> entries = file.Trim(new char[] { ' ', '\n' }).Split("pos").AsSpan(1);
            Moves_By_Position = new Dictionary<string, SBook_Move[]>(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                string[] entry_data = entries[i].Trim('\n').Split('\n');
                string position_fen = entry_data[0].Trim();
                Span<string> all_move_data = entry_data.AsSpan(1);

                SBook_Move[] book_moves = new SBook_Move[all_move_data.Length];

                for (int move_index = 0; move_index < book_moves.Length; move_index++)
                {
                    string[] move_data = all_move_data[move_index].Split(' ');
                    book_moves[move_index] = new SBook_Move(move_data[0], int.Parse(move_data[1] ) );
                }

                Moves_By_Position.Add(position_fen, book_moves);
            }
        }

        public bool Has_Book_Move(string position_fen)
        {
            return Moves_By_Position.ContainsKey(Remove_Move_Counters_From_FEN(position_fen));
        }

        public bool Try_Get_Book_Move(ABoard board, out string move_string, double weight_pow = 0.5)
        {// WeightPow — значение от 0 до 1.
         // 0 означает, что все ходы выбираются с равной вероятностью, 1 означает, что ходы взвешиваются по количеству сыгранных ходов.
            string position_fen = AsFen_Utility.Get_Current_Fen(board, always_include_ep_square: false);
            weight_pow = Math.Clamp(weight_pow, 0, 1);
            
            if (Moves_By_Position.TryGetValue(Remove_Move_Counters_From_FEN(position_fen), out SBook_Move[] moves))
            {
                int total_play_count = 0;
                
                foreach (SBook_Move move in moves)
                {
                    total_play_count += Weighted_Play_Count(move.Num_Times_Played, weight_pow);
                }

                double[] weights = new double[moves.Length];
                double weight_sum = 0;
                
                for (int i = 0; i < moves.Length; i++)
                {
                    double weight = Weighted_Play_Count(moves[i].Num_Times_Played, weight_pow) / (double)total_play_count;
                    weight_sum += weight;
                    weights[i] = weight;
                }

                double[] prob_cumul = new double[moves.Length];
                
                for (int i = 0; i < weights.Length; i++)
                {
                    double prob = weights[i] / weight_sum;
                    prob_cumul[i] = prob_cumul[Math.Max(0, i - 1)] + prob;
                }


                double random = Rand_Gen.NextDouble();
                
                for (int i = 0; i < moves.Length; i++)
                {

                    if (random <= prob_cumul[i])
                    {
                        move_string = moves[i].Move_String;
                        return true;
                    }
                }
            }

            move_string = "Null";
            return false;
        }

        private int Weighted_Play_Count(int playCount, double weight_pow)
        {
            return (int)Math.Ceiling(Math.Pow(playCount, weight_pow));
        }

        private string Remove_Move_Counters_From_FEN(string fen)
        {
            string fen_a = fen.Substring(0, fen.LastIndexOf(' '));
            return fen_a.Substring(0, fen_a.LastIndexOf(' '));
        }
    }
}
