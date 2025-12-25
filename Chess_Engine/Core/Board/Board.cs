using Chess_Engine.Helpers;
using Chess_Engine.Helpers.Bitboard.Magics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine.Core
{
    public class ABoard
    {
        public const int White_Index = 0;
        public const int Black_Index = 1;

        public readonly int[] Square; // Хранит код фигуры для каждой клетки
        public int[] King_Square; // Хранит индексы клеток для черного и белого королей

        // Битборды
        public ulong[] Piece_Bitboards; // Битборды для фигур каждого типа и цвета
        public ulong[] Color_Bitboards;
        public ulong All_Pieces_Bitboard;
        public ulong Friendly_Orthogonal_Sliders;
        public ulong Friendly_Diagonal_Sliders;
        public ulong Enemy_Orthogonal_Sliders;
        public ulong Enemy_Diagonal_Sliders;

        public int Total_Piece_Count_Without_Pawns_And_Kings;

        // Списки фигур
        public APiece_List[] Rooks;
        public APiece_List[] Bishops;
        public APiece_List[] Queens;
        public APiece_List[] Knights;
        public APiece_List[] Pawns;

        // Ходящая сторона
        public bool Is_White_To_Move;
        public int Move_Color => Is_White_To_Move ? APiece.White : APiece.Black;
        public int Opponent_Color => Is_White_To_Move ? APiece.Black : APiece.White;
        public int Move_Color_Index => Is_White_To_Move ? White_Index : Black_Index;
        public int Opponent_Color_Index => Is_White_To_Move ? Black_Index : White_Index;

        public Stack<ulong> Repetition_Position_History; // Список хэшированных позиций с момента последнего хода пешки или взятия

        public int Ply_Count; // Счетчик полуходов (Plies)
        public int Fify_Move_Counter => Current_Game_State.Fifty_Move_Counter;
        public SGame_State Current_Game_State;
        public ulong Zobrist_Key => Current_Game_State.Zobrist_Key;
        public string Current_FEN => AsFen_Utility.Get_Current_Fen(this);
        public string Game_Start_FEN => Start_Position_Info.Fen;
        public List<SMove> All_Game_Moves;

        private APiece_List[] All_Piece_Lists;
        Stack<SGame_State> Game_State_History;
        SPosition_Info Start_Position_Info;
        bool Cached_In_Check_Value;
        bool Has_Cached_In_Check_Value;

        public ABoard()
        {
            Square = new int[64];
        }

        public void Make_Move(SMove move, bool is_search = false)
        {
            int start_square = move.Start_Square;
            int target_square = move.Target_Square;
            int move_flag = move.Move_Flag;
            bool is_promotion = move.Is_Promotion;
            bool is_en_passant = move_flag == SMove.En_Passant_Capture_Flag;

            int moved_piece = Square[start_square];
            EPiece_Type moved_piece_type = APiece.Get_Piece_Type(moved_piece);

            int captured_piece = is_en_passant ? APiece.Make_Piece(EPiece_Type.Pawn, Opponent_Color) : Square[target_square];
            EPiece_Type captured_piece_type = APiece.Get_Piece_Type(captured_piece);

            // Сохраняем текущее состояние перед изменением
            int prev_castle_state = Current_Game_State.Castling_Rights;
            int prev_en_passant_file = Current_Game_State.En_Passant_File;
            ulong new_zobrist_key = Current_Game_State.Zobrist_Key;
            int new_castling_rights = prev_castle_state;
            int new_en_passant_file = 0;

            // Обновляем Zobrist ключ для старой позиции фигуры и стороны
            new_zobrist_key ^= AsZobrist_Hasher.Side_To_Move;
            new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[moved_piece, start_square];
            new_zobrist_key ^= AsZobrist_Hasher.En_Passant_File[prev_en_passant_file];

            // Обработка взятия
            if (captured_piece_type != EPiece_Type.None)
            {
                int capture_square = target_square;

                if (is_en_passant)
                {
                    capture_square = target_square + (Is_White_To_Move ? -8 : 8);
                    Square[capture_square] = (int)EPiece_Type.None;
                }

                if (captured_piece_type != EPiece_Type.Pawn)
                {
                    Total_Piece_Count_Without_Pawns_And_Kings--;
                }

                All_Piece_Lists[captured_piece].Remove_Piece(capture_square);
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[captured_piece], capture_square);
                AsBitboard_Utility.Toggle_Square(ref Color_Bitboards[Opponent_Color_Index], capture_square);
                new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[captured_piece, capture_square];
            }

            // Обработка превращения
            if (is_promotion)
            {
                Total_Piece_Count_Without_Pawns_And_Kings++;

                EPiece_Type promotion_piece_type = move_flag switch
                {
                    SMove.Promote_To_Queen_Flag => EPiece_Type.Queen,
                    SMove.Promote_To_Knight_Flag => EPiece_Type.Knight,
                    SMove.Promote_To_Rook_Flag => EPiece_Type.Rook,
                    SMove.Promote_To_Bishop_Flag => EPiece_Type.Bishop,
                    _ => EPiece_Type.None
                };

                int promotion_piece = APiece.Make_Piece(promotion_piece_type, Move_Color);

                // Удаляем пешку и добавляем новую фигуру
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[moved_piece], start_square);
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[promotion_piece], target_square);
                AsBitboard_Utility.Toggle_Squares(ref Color_Bitboards[Move_Color_Index], start_square, target_square);

                All_Piece_Lists[moved_piece].Remove_Piece(start_square);
                All_Piece_Lists[promotion_piece].Add_Piece(target_square);

                Square[start_square] = (int)EPiece_Type.None;
                Square[target_square] = promotion_piece;

                new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[promotion_piece, target_square];
            }
            else
            {
                // Обычный ход
                Move_Piece(moved_piece, start_square, target_square);
                new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[moved_piece, target_square];
            }

            // Обработка короля
            if (moved_piece_type == EPiece_Type.King)
            {
                King_Square[Move_Color_Index] = target_square;
                new_castling_rights &= (Is_White_To_Move) ? 0b1100 : 0b0011;

                if (move_flag == SMove.Castle_Flag)
                {
                    int rook_piece = APiece.Make_Piece(EPiece_Type.Rook, Move_Color);
                    bool is_kingside = target_square == AsBoard_Helper.g1 || target_square == AsBoard_Helper.g8;
                    int castling_rook_from_index = is_kingside ? target_square + 1 : target_square - 2;
                    int castling_rook_to_index = is_kingside ? target_square - 1 : target_square + 1;

                    Move_Piece(rook_piece, castling_rook_from_index, castling_rook_to_index);
                    new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[rook_piece, castling_rook_from_index];
                    new_zobrist_key ^= AsZobrist_Hasher.Pieces_Array[rook_piece, castling_rook_to_index];
                }
            }

            // Двойной ход пешки
            if (move_flag == SMove.Pawn_Double_Move_Flag)
            {
                int file = AsBoard_Helper.File_Index(start_square) + 1;
                new_en_passant_file = file;
                new_zobrist_key ^= AsZobrist_Hasher.En_Passant_File[file];
            }

            // Обновление прав на рокировку
            if (prev_castle_state != 0)
            {
                if (target_square == AsBoard_Helper.h1 || start_square == AsBoard_Helper.h1)
                    new_castling_rights &= SGame_State.Clear_White_Kingside_Mask;
                else if (target_square == AsBoard_Helper.a1 || start_square == AsBoard_Helper.a1)
                    new_castling_rights &= SGame_State.Clear_White_Queenside_Mask;

                if (target_square == AsBoard_Helper.h8 || start_square == AsBoard_Helper.h8)
                    new_castling_rights &= SGame_State.Clear_Black_Kingside_Mask;
                else if (target_square == AsBoard_Helper.a8 || start_square == AsBoard_Helper.a8)
                    new_castling_rights &= SGame_State.Clear_Black_Queenside_Mask;
            }

            // Обновление Zobrist ключа для прав на рокировку
            if (new_castling_rights != prev_castle_state)
            {
                new_zobrist_key ^= AsZobrist_Hasher.Castling_Rights[prev_castle_state];
                new_zobrist_key ^= AsZobrist_Hasher.Castling_Rights[new_castling_rights];
            }

            // Обновление состояния
            Is_White_To_Move = !Is_White_To_Move;
            Ply_Count++;

            int new_fifty_move_counter = Current_Game_State.Fifty_Move_Counter + 1;

            // Сброс счетчика для ходов пешкой или взятий
            if (moved_piece_type == EPiece_Type.Pawn || captured_piece_type != EPiece_Type.None)
            {
                if (!is_search)
                {
                    Repetition_Position_History.Clear();
                }
                new_fifty_move_counter = 0;
            }

            All_Pieces_Bitboard = Color_Bitboards[White_Index] | Color_Bitboards[Black_Index];
            Update_Slider_Bitboards();

            // Создание нового состояния
            SGame_State new_state = new SGame_State(captured_piece_type, new_en_passant_file, new_castling_rights, new_fifty_move_counter, new_zobrist_key);
            Game_State_History.Push(new_state);
            Current_Game_State = new_state;
            Has_Cached_In_Check_Value = false;

            if (!is_search)
            {
                Repetition_Position_History.Push(new_state.Zobrist_Key);
                All_Game_Moves.Add(move);
            }

        }

        public void Unmake_Move(SMove move, bool is_search = false)
        {
            // Сохраняем текущее состояние перед отменой
            SGame_State current_state = Current_Game_State;

            // Восстанавливаем сторону
            Is_White_To_Move = !Is_White_To_Move;

            int moved_from = move.Start_Square;
            int moved_to = move.Target_Square;
            int move_flag = move.Move_Flag;

            bool undoing_en_passant = move_flag == SMove.En_Passant_Capture_Flag;
            bool undoing_promotion = move.Is_Promotion;
            bool undoing_capture = current_state.Captured_Piece_Type != EPiece_Type.None;

            // Определяем какая фигура была перемещена
            int moved_piece = undoing_promotion ?
                APiece.Make_Piece(EPiece_Type.Pawn, Move_Color) :
                Square[moved_to];
            EPiece_Type moved_piece_type = APiece.Get_Piece_Type(moved_piece);
            EPiece_Type captured_piece_type = current_state.Captured_Piece_Type;

            // Отмена превращения
            if (undoing_promotion)
            {
                int promotion_piece = Square[moved_to];

                // Удаляем фигуру превращения и возвращаем пешку
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[promotion_piece], moved_to);
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[moved_piece], moved_from);
                AsBitboard_Utility.Toggle_Squares(ref Color_Bitboards[Move_Color_Index], moved_to, moved_from);

                All_Piece_Lists[promotion_piece].Remove_Piece(moved_to);
                All_Piece_Lists[moved_piece].Add_Piece(moved_from);

                Square[moved_to] = (int)EPiece_Type.None;
                Square[moved_from] = moved_piece;

                Total_Piece_Count_Without_Pawns_And_Kings--;
            }
            else
            {
                // Отмена обычного хода
                Move_Piece(moved_piece, moved_to, moved_from);
            }

            // Отмена рокировки
            if (moved_piece_type == EPiece_Type.King)
            {
                King_Square[Move_Color_Index] = moved_from;

                if (move_flag == SMove.Castle_Flag)
                {
                    int rook_piece = APiece.Make_Piece(EPiece_Type.Rook, Move_Color);
                    bool is_kingside = moved_to == AsBoard_Helper.g1 || moved_to == AsBoard_Helper.g8;
                    int rook_from = is_kingside ? moved_to + 1 : moved_to - 2;
                    int rook_to = is_kingside ? moved_to - 1 : moved_to + 1;

                    Move_Piece(rook_piece, rook_to, rook_from);
                }
            }

            // Восстановление взятой фигуры
            if (undoing_capture)
            {
                int capture_square = moved_to;
                int captured_piece = APiece.Make_Piece(captured_piece_type, Opponent_Color);

                if (undoing_en_passant)
                {
                    capture_square = moved_to + (Is_White_To_Move ? -8 : 8);
                }

                // Восстанавливаем взятую фигуру
                AsBitboard_Utility.Toggle_Square(ref Piece_Bitboards[captured_piece], capture_square);
                AsBitboard_Utility.Toggle_Square(ref Color_Bitboards[Opponent_Color_Index], capture_square);
                All_Piece_Lists[captured_piece].Add_Piece(capture_square);
                Square[capture_square] = captured_piece;

                if (captured_piece_type != EPiece_Type.Pawn)
                {
                    Total_Piece_Count_Without_Pawns_And_Kings++;
                }
            }

            // Обновление битбордов
            All_Pieces_Bitboard = Color_Bitboards[White_Index] | Color_Bitboards[Black_Index];
            Update_Slider_Bitboards();

            // Восстановление истории
            Game_State_History.Pop();
            Current_Game_State = Game_State_History.Peek();
            Ply_Count--;
            Has_Cached_In_Check_Value = false;

            if (!is_search)
            {
                if (Repetition_Position_History.Count > 0)
                {
                    Repetition_Position_History.Pop();
                }
                if (All_Game_Moves.Count > 0)
                {
                    All_Game_Moves.RemoveAt(All_Game_Moves.Count - 1);
                }
            }
        }

        public void Make_Null_Move()
        {
            Is_White_To_Move = !Is_White_To_Move;

            Ply_Count++;

            ulong new_zobrist_key = Current_Game_State.Zobrist_Key;
            new_zobrist_key ^= AsZobrist_Hasher.Side_To_Move;
            new_zobrist_key ^= AsZobrist_Hasher.En_Passant_File[Current_Game_State.En_Passant_File];

            SGame_State new_state = new SGame_State(EPiece_Type.None, 0, Current_Game_State.Castling_Rights, Current_Game_State.Fifty_Move_Counter + 1, new_zobrist_key);
            Current_Game_State = new_state;
            Game_State_History.Push(Current_Game_State);
            Update_Slider_Bitboards();
            Has_Cached_In_Check_Value = true;
            Cached_In_Check_Value = false;
        }

        public void Unmake_Null_Move()
        {
            Is_White_To_Move = !Is_White_To_Move;
            Ply_Count--;
            Game_State_History.Pop();
            Current_Game_State = Game_State_History.Peek();
            Update_Slider_Bitboards();
            Has_Cached_In_Check_Value = true;
            Cached_In_Check_Value = false;
        }

        public bool Is_In_Check()
        {
            if (Has_Cached_In_Check_Value)
            {
                return Cached_In_Check_Value;
            }

            Cached_In_Check_Value = Calculate_In_Check_State();
            Has_Cached_In_Check_Value = true;

            return Cached_In_Check_Value;
        }

        public bool Calculate_In_Check_State()
        {
            int king_square = King_Square[Move_Color_Index];
            ulong blockers = All_Pieces_Bitboard;

            if (Enemy_Orthogonal_Sliders != 0)
            {
                ulong rook_attacks = AsBitboard_Magics.Get_Rook_Attacks(king_square, blockers);

                if ((rook_attacks & Enemy_Orthogonal_Sliders) != 0)
                {
                    return true;
                }
            }

            if (Enemy_Diagonal_Sliders != 0)
            {
                ulong bishop_attacks = AsBitboard_Magics.Get_Bishop_Attacks(king_square, blockers);

                if ((bishop_attacks & Enemy_Diagonal_Sliders) != 0)
                {
                    return true;
                }
            }

            ulong enemy_knights = Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Knight, Opponent_Color)];

            if ((AsBitboard_Utility.Knight_Attacks[king_square] & enemy_knights) != 0)
            {
                return true;
            }

            ulong enemy_pawns = Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, Opponent_Color)];
            ulong pawn_attack_mask = Is_White_To_Move ? AsBitboard_Utility.White_Pawn_Attacks[king_square] : AsBitboard_Utility.Black_Pawn_Attacks[king_square];

            if ((pawn_attack_mask & enemy_pawns) != 0)
            {
                return true;
            }

            return false;
        }

        public void Load_Start_Position()
        {
            Load_Position(AsFen_Utility.Start_Position_Fen);
        }

        public void Load_Position(string fen)
        {
            SPosition_Info pos_info = AsFen_Utility.Position_From_Fen(fen);
            Load_Position(pos_info);
        }

        public void Load_Position(SPosition_Info pos_info)
        {
            Start_Position_Info = pos_info;
            Initialize();

            for (int square_index = 0; square_index < 64; square_index++)
            {
                int piece = pos_info.Squares[square_index];
                EPiece_Type piece_type = APiece.Get_Piece_Type(piece);
                int color_index = APiece.Is_White(piece) ? White_Index : Black_Index;
                Square[square_index] = piece;

                if (piece == (int)EPiece_Type.None)
                {
                    continue;
                }

                AsBitboard_Utility.Set_Square(ref Piece_Bitboards[piece], square_index);
                AsBitboard_Utility.Set_Square(ref Color_Bitboards[color_index], square_index);

                if (piece_type == EPiece_Type.King)
                {
                    King_Square[color_index] = square_index;
                }
                else
                {

                    All_Piece_Lists[piece].Add_Piece(square_index);
                }

                Total_Piece_Count_Without_Pawns_And_Kings += (piece_type is EPiece_Type.Pawn or EPiece_Type.King) ? 0 : 1;
            }

            Is_White_To_Move = pos_info.White_To_Move;

            All_Pieces_Bitboard = Color_Bitboards[White_Index] | Color_Bitboards[Black_Index];
            Update_Slider_Bitboards();

            // Создаем SGame_State
            int white_castle = ((pos_info.White_Castle_Kingside) ? 1 << 0 : 0) | ((pos_info.White_Castle_Queenside) ? 1 << 1 : 0);
            int black_castle = ((pos_info.Black_Castle_Kingside) ? 1 << 2 : 0) | ((pos_info.White_Castle_Queenside) ? 1 << 3 : 0);
            int castling_rights = white_castle | black_castle;

            Ply_Count = (pos_info.Move_Count - 1) * 2 + (Is_White_To_Move ? 0 : 1);

            Current_Game_State = new SGame_State(EPiece_Type.None, pos_info.Ep_File, castling_rights, pos_info.Fifty_Move_Ply_Count, 0);
            ulong zobrist_key = AsZobrist_Hasher.Calculate_Zobrist_Key(this);
            Current_Game_State = new SGame_State(EPiece_Type.None, pos_info.Ep_File, castling_rights, pos_info.Fifty_Move_Ply_Count, zobrist_key);

            Repetition_Position_History.Push(zobrist_key);

            Game_State_History.Push(Current_Game_State);
        }

        public override string ToString()
        {
            return AsBoard_Helper.CreateDiagram(this, Is_White_To_Move);
        }

        public static ABoard Create_Board(string fen = AsFen_Utility.Start_Position_Fen)
        {
            ABoard board = new ABoard();
            board.Load_Position(fen);
            return board;
        }

        public static ABoard Create_Board(ABoard source_board)
        {
            ABoard board = new ABoard();
            board.Load_Position(source_board.Start_Position_Info);

            for (int i = 0; i < source_board.All_Game_Moves.Count; i++)
            {
                board.Make_Move(source_board.All_Game_Moves[i]);
            }

            return board;
        }

        // Обновление списков фигур/битовых досок на основе предоставленной информации о ходах.
        // Не учитывает следующие моменты, которые необходимо обрабатывать отдельно:
        // 1. Удаление взятой фигуры
        // 2. Перемещение ладьи при рокировке
        // 3. Удаление пешки с 1-й/8-й горизонтали при превращении пешки
        // 4. Добавление превращенной фигуры при превращении пешки

        private void Move_Piece(int piece, int start_square, int target_square)
        {
            if (piece == 0)
            {
                return;
            }

            AsBitboard_Utility.Toggle_Squares(ref Piece_Bitboards[piece], start_square, target_square);
            AsBitboard_Utility.Toggle_Squares(ref Color_Bitboards[Move_Color_Index], start_square, target_square);

            All_Piece_Lists[piece].Move_Piece(start_square, target_square);
            Square[start_square] = (int)EPiece_Type.None;
            Square[target_square] = piece;
        }

        private void Update_Slider_Bitboards()
        {
            int friendly_rook = APiece.Make_Piece(EPiece_Type.Rook, Move_Color);
            int friendly_queen = APiece.Make_Piece(EPiece_Type.Queen, Move_Color);
            int friendly_bishop = APiece.Make_Piece(EPiece_Type.Bishop, Move_Color);

            int enemy_rook = APiece.Make_Piece(EPiece_Type.Rook, Opponent_Color);
            int enemy_queen = APiece.Make_Piece(EPiece_Type.Queen, Opponent_Color);
            int enemy_bishop = APiece.Make_Piece(EPiece_Type.Bishop, Opponent_Color);


            Friendly_Orthogonal_Sliders = Piece_Bitboards[friendly_rook] | Piece_Bitboards[friendly_queen];
            Friendly_Diagonal_Sliders = Piece_Bitboards[friendly_bishop] | Piece_Bitboards[friendly_queen];

            Enemy_Orthogonal_Sliders = Piece_Bitboards[enemy_rook] | Piece_Bitboards[enemy_queen];
            Enemy_Diagonal_Sliders = Piece_Bitboards[enemy_bishop] | Piece_Bitboards[enemy_queen];
        }

        private void Initialize()
        {
            All_Game_Moves = new List<SMove>();
            King_Square = new int[2];
            Array.Clear(Square);

            Repetition_Position_History = new Stack<ulong>(capacity: 64);
            Game_State_History = new Stack<SGame_State>(capacity: 64);

            Current_Game_State = new SGame_State();
            Ply_Count = 0;

            Knights = new APiece_List[] { new APiece_List(10), new APiece_List(10) };
            Pawns = new APiece_List[] { new APiece_List(8), new APiece_List(8) };
            Rooks = new APiece_List[] { new APiece_List(10), new APiece_List(10) };
            Bishops = new APiece_List[] { new APiece_List(10), new APiece_List(10) };
            Queens = new APiece_List[] { new APiece_List(9), new APiece_List(9) };

            All_Piece_Lists = new APiece_List[APiece.Max_Piece_Index + 1];

            for (int i = 0; i < All_Piece_Lists.Length; i++)
            {
                All_Piece_Lists[i] = new APiece_List(1);
            }

            All_Piece_Lists[APiece.White_Pawn] = Pawns[White_Index];
            All_Piece_Lists[APiece.White_Knight] = Knights[White_Index];
            All_Piece_Lists[APiece.White_Bishop] = Bishops[White_Index];
            All_Piece_Lists[APiece.White_Rook] = Rooks[White_Index];
            All_Piece_Lists[APiece.White_Queen] = Queens[White_Index];
            All_Piece_Lists[APiece.White_King] = new APiece_List(1);

            All_Piece_Lists[APiece.Black_Pawn] = Pawns[Black_Index];
            All_Piece_Lists[APiece.Black_Knight] = Knights[Black_Index];
            All_Piece_Lists[APiece.Black_Bishop] = Bishops[Black_Index];
            All_Piece_Lists[APiece.Black_Rook] = Rooks[Black_Index];
            All_Piece_Lists[APiece.Black_Queen] = Queens[Black_Index];
            All_Piece_Lists[APiece.Black_King] = new APiece_List(1);

            Total_Piece_Count_Without_Pawns_And_Kings = 0;

            Piece_Bitboards = new ulong[APiece.Max_Piece_Index + 1];
            Color_Bitboards = new ulong[2];
            All_Pieces_Bitboard = 0;
        }
    }
}
