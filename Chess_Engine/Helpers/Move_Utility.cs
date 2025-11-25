using Chess_Engine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Helpers
{
    public static class AsMove_Utility
    {
        public static SMove Get_Move_From_UCI_Name(string move_name, ABoard board)
        {
            int start_square = AsBoard_Helper.Square_Index_From_Name(move_name.Substring(0, 2));
            int target_square = AsBoard_Helper.Square_Index_From_Name(move_name.Substring(2, 2));

            int moved_piece = board.Square[start_square];
            EPiece_Type moved_piece_type = APiece.Get_Piece_Type(moved_piece);
            SCoordinate start_coord = new SCoordinate(start_square);
            SCoordinate target_coord = new SCoordinate(target_square);

            // Определяем флаг хода
            int move_flag = SMove.No_Flag;

            if (moved_piece_type == EPiece_Type.Pawn)
            {
                // Превращение
                if (move_name.Length > 4)
                {
                    move_flag = move_name[^1] switch
                    {
                        'q' => SMove.Promote_To_Queen_Flag,
                        'r' => SMove.Promote_To_Rook_Flag,
                        'n' => SMove.Promote_To_Knight_Flag,
                        'b' => SMove.Promote_To_Bishop_Flag,
                        _ => SMove.No_Flag
                    };
                }
                // Двойной ход пешки
                else if (Math.Abs(target_coord.Rank_Index - start_coord.Rank_Index) == 2)
                {
                    move_flag = SMove.Pawn_Double_Move_Flag;
                }
                // Взятие на проходе
                else if (start_coord.File_Index != target_coord.File_Index && board.Square[target_square] == 0)
                {
                    move_flag = SMove.En_Passant_Capture_Flag;
                }
            }
            else if (moved_piece_type == EPiece_Type.King)
            {
                if (Math.Abs(start_coord.File_Index - target_coord.File_Index) > 1)
                {
                    move_flag = SMove.Castle_Flag;
                }
            }

            return new SMove(start_square, target_square, move_flag);
        }

        /// <summary>
        /// Получить алгебраическое имя хода (с указанием превращения)
        /// Примеры: "e2e4", "e7e8q"
        /// </summary>
        public static string Get_Move_Name_UCI(SMove move)
        {
            string start_square_name = AsBoard_Helper.Square_Name_From_Index(move.Start_Square);
            string target_square_name = AsBoard_Helper.Square_Name_From_Index(move.Target_Square);
            string move_name = start_square_name + target_square_name;

            if (move.Is_Promotion)
            {
                switch (move.Move_Flag)
                {
                    case SMove.Promote_To_Rook_Flag:
                        move_name += "r";
                        break;
                    case SMove.Promote_To_Knight_Flag:
                        move_name += "n";
                        break;
                    case SMove.Promote_To_Bishop_Flag:
                        move_name += "b";
                        break;
                    case SMove.Promote_To_Queen_Flag:
                        move_name += "q";
                        break;
                }
            }

            return move_name;
        }

        /// <summary>
        /// Получить имя хода в Стандартной Алгебраической Нотации (SAN)
        /// Примеры: "e4", "Bxf7+", "O-O", "Rh8#", "Nfd2"
        /// Примечание: ход еще не должен быть сделан на доске
        /// </summary>
        public static string Get_Move_Name_SAN(SMove move, ABoard board)
        {
            if (move.Is_Null)
            {
                return "Null";
            }

            int moved_piece = board.Square[move.Start_Square];
            EPiece_Type moved_piece_type = APiece.Get_Piece_Type(moved_piece);
            EPiece_Type captured_piece_type = APiece.Get_Piece_Type(board.Square[move.Target_Square]);

            // Рокировка
            if (move.Move_Flag == SMove.Castle_Flag)
            {
                int delta = move.Target_Square - move.Start_Square;
                return delta == 2 ? "O-O" : "O-O-O";
            }

            AMove_Generator move_generator = new AMove_Generator();
            string move_notation = moved_piece_type == EPiece_Type.Pawn ? "" : char.ToUpper(APiece.Get_Piece_Symbol(moved_piece)).ToString();

            // Проверка неоднозначности в нотации (например, если e2 может быть достигнута через Nfe2 и Nbe2)
            if (moved_piece_type != EPiece_Type.Pawn && moved_piece_type != EPiece_Type.King)
            {
                Resolve_Notation_Ambiguity(move, board, moved_piece_type, ref move_notation);
            }

            // Обозначение взятия
            if (captured_piece_type != EPiece_Type.None || move.Move_Flag == SMove.En_Passant_Capture_Flag)
            {
                Add_Capture_Notation(move, moved_piece_type, ref move_notation);
            }

            // Целевое поле
            move_notation += AsBoard_Helper.Square_Name_From_Index(move.Target_Square);

            // Превращение
            if (move.Is_Promotion)
            {
                move_notation += "=" + char.ToUpper(APiece.Get_Piece_Symbol((int)Get_Promotion_Piece_Type(move)));
            }

            // Проверка шаха и мата
            Add_Check_Mate_Notation(move, board, ref move_notation);

            return move_notation;
        }

        /// <summary>
        /// Получить ход из заданного имени в SAN нотации (например, "Nxf3", "Rad1", "O-O", и т.д.)
        /// Данная доска должна содержать позицию до того, как ход был сделан
        /// </summary>
        public static SMove Get_Move_From_SAN(ABoard board, string algebraic_move)
        {
            AMove_Generator move_generator = new AMove_Generator();

            // Удаляем ненужную информацию из строки хода
            string clean_move = algebraic_move.Replace("+", "").Replace("#", "").Replace("x", "").Replace("-", "");
            Span<SMove> all_moves = move_generator.Generate_Moves(board);

            // Специальные случаи рокировки
            if (clean_move == "OO")
            {
                return Find_Castling_Move(all_moves, board, is_kingside: true);
            }
            else if (clean_move == "OOO")
            {
                return Find_Castling_Move(all_moves, board, is_kingside: false);
            }

            // Поиск подходящего хода
            foreach (SMove move in all_moves)
            {
                if (Is_Move_Matching_SAN(move, board, clean_move, algebraic_move))
                {
                    return move;
                }
            }

            return SMove.Null_Move; // Возвращаем нулевой ход если не нашли
        }

        // Вспомогательные методы

        private static void Resolve_Notation_Ambiguity(SMove move, ABoard board, EPiece_Type moved_piece_type, ref string move_notation)
        {
            AMove_Generator move_generator = new AMove_Generator();
            Span<SMove> all_moves = move_generator.Generate_Moves(board);

            foreach (SMove alt_move in all_moves)
            {
                if (alt_move.Start_Square != move.Start_Square && alt_move.Target_Square == move.Target_Square)
                {
                    EPiece_Type alt_piece_type = APiece.Get_Piece_Type(board.Square[alt_move.Start_Square]);

                    if (alt_piece_type == moved_piece_type)
                    {
                        SCoordinate from_coord = new SCoordinate(move.Start_Square);
                        SCoordinate alt_from_coord = new SCoordinate(alt_move.Start_Square);

                        if (from_coord.File_Index != alt_from_coord.File_Index)
                        {
                            move_notation += AsBoard_Helper.File_Names[from_coord.File_Index];
                            break;
                        }
                        else if (from_coord.Rank_Index != alt_from_coord.Rank_Index)
                        {
                            move_notation += AsBoard_Helper.Rank_Names[from_coord.Rank_Index];
                            break;
                        }
                    }
                }
            }
        }

        private static void Add_Capture_Notation(SMove move, EPiece_Type moved_piece_type, ref string move_notation)
        {
            if (moved_piece_type == EPiece_Type.Pawn)
            {
                move_notation += AsBoard_Helper.File_Names[AsBoard_Helper.File_Index(move.Start_Square)];
            }

            move_notation += "x";
        }

        private static void Add_Check_Mate_Notation(SMove move, ABoard board, ref string move_notation)
        {
            board.Make_Move(move, is_search: true);

            AMove_Generator move_generator = new AMove_Generator();
            Span<SMove> legal_responses = move_generator.Generate_Moves(board);

            if (move_generator.Is_In_Check())
            {
                move_notation += legal_responses.Length == 0 ? "#" : "+";
            }

            board.Unmake_Move(move, is_search: true);
        }

        private static EPiece_Type Get_Promotion_Piece_Type(SMove move)
        {
            return move.Move_Flag switch
            {
                SMove.Promote_To_Queen_Flag => EPiece_Type.Queen,
                SMove.Promote_To_Rook_Flag => EPiece_Type.Rook,
                SMove.Promote_To_Knight_Flag => EPiece_Type.Knight,
                SMove.Promote_To_Bishop_Flag => EPiece_Type.Bishop,
                _ => EPiece_Type.Queen
            };
        }

        private static SMove Find_Castling_Move(Span<SMove> moves, ABoard board, bool is_kingside)
        {
            foreach (SMove move in moves)
            {
                EPiece_Type piece_type = APiece.Get_Piece_Type(board.Square[move.Start_Square]);

                if (piece_type == EPiece_Type.King && move.Move_Flag == SMove.Castle_Flag)
                {
                    int delta = move.Target_Square - move.Start_Square;
                    if ((is_kingside && delta == 2) || (!is_kingside && delta == -2))
                    {
                        return move;
                    }
                }
            }

            return SMove.Null_Move;
        }

        private static bool Is_Move_Matching_SAN(SMove move, ABoard board, string clean_move, string original_move)
        {
            int move_from_index = move.Start_Square;
            int move_to_index = move.Target_Square;
            int moved_piece = board.Square[move_from_index];
            EPiece_Type moved_piece_type = APiece.Get_Piece_Type(moved_piece);
            SCoordinate from_coord = new SCoordinate(move_from_index);
            SCoordinate to_coord = new SCoordinate(move_to_index);

            // Ход пешки
            if (AsBoard_Helper.File_Names.Contains(clean_move[0].ToString()))
            {
                if (moved_piece_type != EPiece_Type.Pawn) return false;
                if (AsBoard_Helper.File_Names.IndexOf(clean_move[0]) != from_coord.File_Index) return false;

                return Is_Pawn_Move_Matching(clean_move, original_move, move, to_coord);
            }
            // Ход фигуры
            else
            {
                char move_piece_char = clean_move[0];
                if (APiece.Get_Piece_Type_From_Symbol(move_piece_char) != moved_piece_type) return false;

                return Is_Piece_Move_Matching(clean_move, move, from_coord, to_coord);
            }
        }

        private static bool Is_Pawn_Move_Matching(string clean_move, string original_move, SMove move, SCoordinate to_coord)
        {
            // Превращение
            if (original_move.Contains('='))
            {
                if (to_coord.Rank_Index != 0 && to_coord.Rank_Index != 7) return false;

                // Взятие с превращением
                if (clean_move.Length == 5)
                {
                    char target_file = clean_move[1];
                    if (AsBoard_Helper.File_Names.IndexOf(target_file) != to_coord.File_Index) return false;
                }

                char promotion_char = original_move[original_move.Length - 1];
                EPiece_Type promotion_type = APiece.Get_Piece_Type_From_Symbol(promotion_char);
                EPiece_Type move_promotion_type = Get_Promotion_Piece_Type(move);

                return promotion_type == move_promotion_type;
            }
            // Обычный ход пешки
            else
            {
                char target_file = clean_move[clean_move.Length - 2];
                char target_rank = clean_move[clean_move.Length - 1];

                return AsBoard_Helper.File_Names.IndexOf(target_file) == to_coord.File_Index &&
                       target_rank.ToString() == (to_coord.Rank_Index + 1).ToString();
            }
        }

        private static bool Is_Piece_Move_Matching(string clean_move, SMove move, SCoordinate from_coord, SCoordinate to_coord)
        {
            char target_file = clean_move[clean_move.Length - 2];
            char target_rank = clean_move[clean_move.Length - 1];

            if (AsBoard_Helper.File_Names.IndexOf(target_file) != to_coord.File_Index) return false;
            if (target_rank.ToString() != (to_coord.Rank_Index + 1).ToString()) return false;

            // Разрешение неоднозначности
            if (clean_move.Length == 4)
            {
                char disambiguation_char = clean_move[1];

                if (AsBoard_Helper.File_Names.Contains(disambiguation_char.ToString()))
                {
                    return AsBoard_Helper.File_Names.IndexOf(disambiguation_char) == from_coord.File_Index;
                }
                else
                {
                    return disambiguation_char.ToString() == (from_coord.Rank_Index + 1).ToString();
                }
            }

            return true;
        }
    }
}
