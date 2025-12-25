using Chess_Engine.Helpers;
using Chess_Engine.Helpers.Bitboard.Magics;

using static Chess_Engine.Core.AsPrecomputed_Move_Data;

namespace Chess_Engine.Core
{
    public enum EPromotion_Mode
    {
        All,
        Queen_Only,
        Queen_And_Knight
    };

    public class AMove_Generator
    {
        public const int Max_Moves = 256;
        public EPromotion_Mode Promotion_Mode = EPromotion_Mode.All;

        ABoard Board;
        int Curr_Move_Index;
        bool Generate_Quiet_Moves;

        bool Is_White_To_Move;
        int Friendly_Color, Opponent_Color;
        int Friendly_King_Square;
        int Friendly_Index;
        int Enemy_Index;

        bool In_Check, In_Double_Check;
        ulong Check_Ray_Bitmask;
        ulong Pin_Rays;
        ulong Not_Pin_Rays;
        ulong Opponent_Attack_Map_No_Pawns;
        public ulong Opponent_Attack_Map;
        public ulong Opponent_Pawn_Attack_Map;
        ulong Opponent_Sliding_Attack_Map;

        ulong Enemy_Pieces, Friendly_Pieces;
        ulong All_Pieces;
        ulong Empty_Squares;
        ulong Empty_Or_Enemy_Squares;
        ulong Move_Type_Mask;

        const int White_Pawn_Push_Dir = 1;
        const int Black_Pawn_Push_Dir = -1;
        const int Squares_Per_Rank = 8;

        public Span<SMove> Generate_Moves(ABoard board, bool captures_only = false)
        {
            Span<SMove> moves = new SMove[Max_Moves];
            Generate_Moves(board, ref moves, captures_only);
            return moves;
        }

        public int Generate_Moves(ABoard board, ref Span<SMove> moves, bool captures_only = false)
        {
            Board = board;
            Generate_Quiet_Moves = !captures_only;

            Initialize_Generator_State();
            Generate_King_Moves(moves);

            if (!In_Double_Check)
            {
                Generate_Sliding_Moves(moves);
                Generate_Knight_Moves(moves);
                Generate_Pawn_Moves(moves);
            }

            moves = moves.Slice(0, Curr_Move_Index);
            return moves.Length;
        }

        public bool Is_In_Check()
        {
            return In_Check;
        }

        private void Initialize_Generator_State()
        {
            Curr_Move_Index = 0;
            In_Check = false;
            In_Double_Check = false;
            Check_Ray_Bitmask = 0;
            Pin_Rays = 0;

            Is_White_To_Move = Board.Move_Color == APiece.White;
            Friendly_Color = Board.Move_Color;
            Opponent_Color = Board.Opponent_Color;
            Friendly_King_Square = Board.King_Square[Board.Move_Color_Index];
            Friendly_Index = Board.Move_Color_Index;
            Enemy_Index = 1 - Friendly_Index;

            Enemy_Pieces = Board.Color_Bitboards[Enemy_Index];
            Friendly_Pieces = Board.Color_Bitboards[Friendly_Index];
            All_Pieces = Board.All_Pieces_Bitboard;
            Empty_Squares = ~All_Pieces;
            Empty_Or_Enemy_Squares = Empty_Squares | Enemy_Pieces;
            Move_Type_Mask = Generate_Quiet_Moves ? ulong.MaxValue : Enemy_Pieces;

            Calculate_Attack_Data();
        }

        private void Generate_King_Moves(Span<SMove> moves)
        {
            Generate_King_Normal_Moves(moves);

            if (!In_Check && Generate_Quiet_Moves)
            {
                Generate_Castling_Moves(moves);
            }
        }

        private void Generate_King_Normal_Moves(Span<SMove> moves)
        {
            ulong legal_mask = ~(Opponent_Attack_Map | Friendly_Pieces);
            ulong king_moves = AsPrecomputed_Move_Data.King_Attack_Bitboards[Friendly_King_Square] & legal_mask & Move_Type_Mask;

            while (king_moves != 0)
            {
                int target_square = AsBitboard_Utility.Pop_LSB(ref king_moves);
                moves[Curr_Move_Index++] = new SMove(Friendly_King_Square, target_square);
            }
        }

        private void Generate_Castling_Moves(Span<SMove> moves)
        {
            ulong castle_blockers = Opponent_Attack_Map | Board.All_Pieces_Bitboard;

            if (Board.Current_Game_State.Has_Kingside_Castle_Right(Board.Is_White_To_Move))
            {
                Try_Generate_Kingside_Castle(castle_blockers, moves);
            }

            if (Board.Current_Game_State.Has_Queenside_Castle_Right(Board.Is_White_To_Move))
            {
                Try_Generate_Queenside_Castle(castle_blockers, moves);
            }
        }

        private void Try_Generate_Kingside_Castle(ulong castle_blockers, Span<SMove> moves)
        {
            ulong castle_mask = Board.Is_White_To_Move ? AsBit_Masks.White_Kingside_Mask : AsBit_Masks.Black_Kingside_Mask;

            if ((castle_mask & castle_blockers) == 0)
            {
                int target_square = Board.Is_White_To_Move ? AsBoard_Helper.g1 : AsBoard_Helper.g8;
                moves[Curr_Move_Index++] = new SMove(Friendly_King_Square, target_square, SMove.Castle_Flag);
            }
        }

        private void Try_Generate_Queenside_Castle(ulong castle_blockers, Span<SMove> moves)
        {
            ulong castle_mask = Board.Is_White_To_Move ? AsBit_Masks.White_Queenside_Mask_2 : AsBit_Masks.Black_Queenside_Mask_2;
            ulong castle_block_mask = Board.Is_White_To_Move ? AsBit_Masks.White_Queenside_Mask : AsBit_Masks.Black_Queenside_Mask;

            if ((castle_mask & castle_blockers) == 0 && (castle_block_mask & Board.All_Pieces_Bitboard) == 0)
            {
                int target_square = Board.Is_White_To_Move ? AsBoard_Helper.c1 : AsBoard_Helper.c8;
                moves[Curr_Move_Index++] = new SMove(Friendly_King_Square, target_square, SMove.Castle_Flag);
            }
        }

        private void Generate_Sliding_Moves(Span<SMove> moves)
        {
            ulong move_mask = Empty_Or_Enemy_Squares & Check_Ray_Bitmask & Move_Type_Mask;

            Generate_Slider_Moves(move_mask, moves, true);
            Generate_Slider_Moves(move_mask, moves, false);
        }

        private void Generate_Slider_Moves(ulong move_mask, Span<SMove> moves, bool is_orthogonal)
        {
            ulong sliders = is_orthogonal ? Board.Friendly_Orthogonal_Sliders : Board.Friendly_Diagonal_Sliders;

            if (In_Check)
            {
                sliders &= ~Pin_Rays;
            }
            
            while (sliders != 0)
            {
                int start_square = AsBitboard_Utility.Pop_LSB(ref sliders);
                Generate_Slider_Moves_For_Square(start_square, move_mask, is_orthogonal, moves);
            }
        }

        private void Generate_Slider_Moves_For_Square(int start_square, ulong move_mask, bool is_orthogonal, Span<SMove> moves)
        {
            ulong move_squares = is_orthogonal ? AsBitboard_Magics.Get_Rook_Attacks(start_square, All_Pieces) : AsBitboard_Magics.Get_Bishop_Attacks(start_square, All_Pieces);

            move_squares &= move_mask;

            if (Is_Pinned(start_square))
            {
                move_squares &= AsPrecomputed_Move_Data.Align_Mask[start_square, Friendly_King_Square];
            }

            while (move_squares != 0)
            {
                int target_square = AsBitboard_Utility.Pop_LSB(ref move_squares);
                moves[Curr_Move_Index++] = new SMove(start_square, target_square);
            }
        }

        private void Generate_Knight_Moves(Span<SMove> moves)
        {
            ulong knights = Get_Unpinned_Knights();
            ulong move_mask = Empty_Or_Enemy_Squares & Check_Ray_Bitmask & Move_Type_Mask;

            while (knights != 0)
            {
                int knight_square = AsBitboard_Utility.Pop_LSB(ref knights);
                Generate_Knight_Moves_For_Square(knight_square, move_mask, moves);
            }
        }

        private ulong Get_Unpinned_Knights()
        {
            int friendly_knight_piece = APiece.Make_Piece(EPiece_Type.Knight, Board.Move_Color);
            return Board.Piece_Bitboards[friendly_knight_piece] & Not_Pin_Rays;
        }

        private void Generate_Knight_Moves_For_Square(int knight_square, ulong move_mask, Span<SMove> moves)
        {
            ulong move_squares = AsPrecomputed_Move_Data.Knight_Attack_Bitboards[knight_square] & move_mask;

            while (move_squares != 0)
            {
                int target_square = AsBitboard_Utility.Pop_LSB(ref move_squares);
                moves[Curr_Move_Index++] = new SMove(knight_square, target_square);
            }
        }

        private void Generate_Pawn_Moves(Span<SMove> moves)
        {
            int push_dir = Get_Pawn_Push_Direction();
            int push_offset = push_dir * Squares_Per_Rank;
            ulong pawns = Get_Friendly_Pawns();

            if (Generate_Quiet_Moves)
            {
                Generate_Pawn_Pushes(pawns, push_dir, push_offset, moves);
            }

            Generate_Pawn_Captures(pawns, push_dir, moves);
            Generate_En_Passant_Moves(pawns, push_dir, push_offset, moves);
        }

        private int Get_Pawn_Push_Direction()
        {
            return Board.Is_White_To_Move ? White_Pawn_Push_Dir : Black_Pawn_Push_Dir;
        }

        ulong Get_Friendly_Pawns()
        {
            return Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, Board.Move_Color)];
        }

        private void Generate_Pawn_Pushes(ulong pawns, int push_dir, int push_offset, Span<SMove> moves)
        {
            ulong single_push = Calculate_Single_Push(pawns, push_offset);
            Generate_Single_Pushes(single_push, push_offset, moves);
            Generate_Double_Pushes(single_push, push_offset, moves);
        }

        private ulong Calculate_Single_Push(ulong pawns, int push_offset)
        {
            return AsBitboard_Utility.Shift(pawns, push_offset) & Empty_Squares;
        }

        private void Generate_Single_Pushes(ulong single_push, int push_offset, Span<SMove> moves)
        {
            ulong promotion_rank_mask = Get_Promotion_Rank_Mask();
            ulong single_push_no_promotions = single_push & ~promotion_rank_mask & Check_Ray_Bitmask;
            ulong push_promotions = single_push & promotion_rank_mask & Check_Ray_Bitmask;

            Generate_Basic_Pawn_Moves(single_push_no_promotions, push_offset, 0, moves);
            Generate_Pawn_Promotions(push_promotions, push_offset, false, moves);
        }

        private void Generate_Double_Pushes(ulong single_push, int push_offset, Span<SMove> moves)
        {
            ulong double_push_target_rank_mask = Board.Is_White_To_Move ? AsBitboard_Utility.Rank_4 : AsBitboard_Utility.Rank_5;
            ulong double_push = AsBitboard_Utility.Shift(single_push, push_offset) & Empty_Squares & double_push_target_rank_mask & Check_Ray_Bitmask;

            Generate_Basic_Pawn_Moves(double_push, push_offset * 2, SMove.Pawn_Double_Move_Flag, moves);
        }

        private void Generate_Pawn_Captures(ulong pawns, int push_dir, Span<SMove> moves)
        {
            (ulong capture_left, ulong capture_right) = Calculate_Pawn_Captures(pawns, push_dir);
            ulong promotion_rank_mask = Get_Promotion_Rank_Mask();

            Process_Pawn_Captures(capture_left, push_dir * 7, promotion_rank_mask, moves);
            Process_Pawn_Captures(capture_right, push_dir * 9, promotion_rank_mask, moves);
        }

        private (ulong capture_left, ulong capture_right) Calculate_Pawn_Captures(ulong pawns, int push_dir)
        {
            ulong left_file_mask = Board.Is_White_To_Move ? AsBitboard_Utility.Not_A_File : AsBitboard_Utility.Not_H_File;
            ulong right_file_mask = Board.Is_White_To_Move ? AsBitboard_Utility.Not_H_File : AsBitboard_Utility.Not_A_File;

            ulong capture_left = AsBitboard_Utility.Shift(pawns & left_file_mask, push_dir * 7) & Enemy_Pieces;
            ulong capture_right = AsBitboard_Utility.Shift(pawns & right_file_mask, push_dir * 9) & Enemy_Pieces;

            return (capture_left, capture_right);
        }

        private void Process_Pawn_Captures(ulong capture_squares, int offset, ulong promotion_rank_mask, Span<SMove> moves)
        {
            ulong normal_captures = capture_squares & ~promotion_rank_mask & Check_Ray_Bitmask;
            ulong promotion_captures = capture_squares & promotion_rank_mask & Check_Ray_Bitmask;

            Generate_Basic_Pawn_Moves(normal_captures, offset, 0, moves);
            Generate_Pawn_Promotions(promotion_captures, offset, true, moves);
        }

        private ulong Get_Promotion_Rank_Mask()
        {
            return Board.Is_White_To_Move ? AsBitboard_Utility.Rank_8 : AsBitboard_Utility.Rank_1;
        }

        private void Generate_Basic_Pawn_Moves(ulong target_squares, int offset, int move_flag, Span<SMove> moves)
        {
            while (target_squares != 0)
            {
                int target_square = AsBitboard_Utility.Pop_LSB(ref target_squares);
                int start_square = target_square - offset;

                if (Can_Pawn_Move_Safely(start_square, target_square))
                {
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, move_flag);
                }
            }
        }

        private void Generate_Pawn_Promotions(ulong promotion_squares, int offset, bool is_capture, Span<SMove> moves)
        {
            while (promotion_squares != 0)
            {
                int target_square = AsBitboard_Utility.Pop_LSB(ref promotion_squares);
                int start_square = target_square - offset;

                if (Can_Pawn_Promote_Safely(start_square, target_square, is_capture))
                {
                    Generate_Promotions(start_square, target_square, moves);
                }
            }
        }

        private bool Can_Pawn_Move_Safely(int start_square, int target_square)
        {
            return !Is_Pinned(start_square) || AsPrecomputed_Move_Data.Align_Mask[start_square, Friendly_King_Square] == AsPrecomputed_Move_Data.Align_Mask[target_square, Friendly_King_Square];
        }

        private bool Can_Pawn_Promote_Safely(int start_square, int target_square, bool is_capture)
        {
            return !Is_Pinned(start_square) || (is_capture && Can_Pawn_Move_Safely(start_square, target_square));
        }

        private void Generate_En_Passant_Moves(ulong pawns, int push_dir, int push_offset, Span<SMove> moves)
        {
            if (Board.Current_Game_State.En_Passant_File <= 0)
            {
                return;
            }

            (int target_square, int captured_pawn_square) = Calculate_En_Passant_Squares(push_dir, push_offset);

            if (!AsBitboard_Utility.Contains_Square(Check_Ray_Bitmask, captured_pawn_square))
            {
                return;
            }

            ulong pawns_that_can_capture_ep = pawns & AsBitboard_Utility.Get_Pawn_Attacks(1ul << target_square, !Board.Is_White_To_Move);

            while (pawns_that_can_capture_ep != 0)
            {
                int start_square = AsBitboard_Utility.Pop_LSB(ref pawns_that_can_capture_ep);

                if (Can_Pawn_Move_Safely(start_square, target_square) && !In_Check_After_En_Passant(start_square, target_square, captured_pawn_square))
                {
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.En_Passant_Capture_Flag);
                }
            }
        }

        private (int target_square, int captured_pawn_square) Calculate_En_Passant_Squares(int push_dir, int push_offset)
        {
            int ep_file_index = Board.Current_Game_State.En_Passant_File - 1;
            int ep_rank_index = Board.Is_White_To_Move ? 5 : 2;
            int target_square = ep_rank_index * 8 + ep_file_index;
            int captured_pawn_square = target_square - push_offset;
            
            return (target_square, captured_pawn_square);
        }

        private void Generate_Promotions(int start_square, int target_square, Span<SMove> moves)
        {
            moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.Promote_To_Queen_Flag);

            if (Generate_Quiet_Moves)
            {
                if (Promotion_Mode == EPromotion_Mode.All)
                {
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.Promote_To_Knight_Flag);
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.Promote_To_Rook_Flag);
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.Promote_To_Bishop_Flag);
                }
                else if (Promotion_Mode == EPromotion_Mode.Queen_And_Knight)
                {
                    moves[Curr_Move_Index++] = new SMove(start_square, target_square, SMove.Promote_To_Knight_Flag);
                }
            }
        }

        private bool Is_Pinned(int square)
        {
            return ((Pin_Rays >> square) & 1) != 0;
        }

        private void Calculate_Attack_Data()
        {
            Gen_Sliding_Attack_Map();

            int start_dir_index = 0;
            int end_dir_index = 8;

            if (Board.Queens[Enemy_Index].Count == 0)
            {
                start_dir_index = (Board.Rooks[Enemy_Index].Count > 0) ? 0 : 4;
                end_dir_index = (Board.Bishops[Enemy_Index].Count > 0) ? 8 : 4;
            }

            for (int dir = start_dir_index; dir < end_dir_index; dir++)
            {
                bool is_diagonal = dir > 3;
                ulong slider = is_diagonal ? Board.Enemy_Diagonal_Sliders : Board.Enemy_Orthogonal_Sliders;

                if ((AsPrecomputed_Move_Data.Dir_Ray_Mask[dir, Friendly_King_Square] & slider) == 0)
                {
                    continue;
                }

                Find_Checks_And_Pins_In_Direction(dir);

                if (In_Double_Check)
                {
                    break;
                }
            }

            Not_Pin_Rays = ~Pin_Rays;

            ulong opponent_knight_attacks = Calculate_Knight_Threats();

            Calculate_Pawn_Threats(opponent_knight_attacks);

            int enemy_king_square = Board.King_Square[Enemy_Index];
            Opponent_Attack_Map_No_Pawns = Opponent_Sliding_Attack_Map | opponent_knight_attacks | AsPrecomputed_Move_Data.King_Attack_Bitboards[enemy_king_square];
            Opponent_Attack_Map = Opponent_Attack_Map_No_Pawns | Opponent_Pawn_Attack_Map;

            if (!In_Check)
            {
                Check_Ray_Bitmask = ulong.MaxValue;
            }
        }

        private void Find_Checks_And_Pins_In_Direction(int direction)
        {
            bool is_diagonal = direction > 3;
            int n = AsPrecomputed_Move_Data.Num_Squares_To_Edge[Friendly_King_Square][direction];
            int direction_offset = AsPrecomputed_Move_Data.Direction_Offsets[direction];
            bool is_friendly_piece_along_ray = false;
            ulong ray_mask = 0;

            for (int i = 0; i < n; i++)
            {
                int square_index = Friendly_King_Square + direction_offset * (i + 1);
                ray_mask |= 1ul << square_index;
                int piece = Board.Square[square_index];
                EPiece_Type piece_type = APiece.Get_Piece_Type(piece);

                if (piece_type == EPiece_Type.None)
                {
                    continue;
                }

                if (APiece.Is_Color(piece, Friendly_Color))
                {
                    if (!is_friendly_piece_along_ray)
                    {
                        is_friendly_piece_along_ray = true;
                    }
                    else
                    {
                        break;
                    }
                }
                else if ((is_diagonal && APiece.Is_Diagonal_Slider(piece)) || (!is_diagonal && APiece.Is_Orthogonal_Slider(piece)))
                {
                    if (is_friendly_piece_along_ray)
                    {
                        Pin_Rays |= ray_mask;
                    }
                    else
                    {
                        Check_Ray_Bitmask |= ray_mask;
                        In_Double_Check = In_Check;
                        In_Check = true;
                    }

                    break;
                }
                else
                {
                    break;
                }
            }
        }

        private ulong Calculate_Knight_Threats()
        {
            ulong opponent_knight_attacks = 0;
            ulong knights = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Knight, Board.Opponent_Color)];
            ulong friendly_king_board = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.King, Board.Move_Color)];

            while (knights != 0)
            {
                int knight_square = AsBitboard_Utility.Pop_LSB(ref knights);
                ulong knight_attacks = AsPrecomputed_Move_Data.Knight_Attack_Bitboards[knight_square];
                opponent_knight_attacks |= knight_attacks;

                if ((knight_attacks & friendly_king_board) != 0)
                {
                    In_Double_Check = In_Check;
                    In_Check = true;
                    Check_Ray_Bitmask |= 1ul << knight_square;
                }
            }

            return opponent_knight_attacks;
        }

        private void Calculate_Pawn_Threats(ulong opponent_knight_attacks)
        {
            ulong opponent_pawns_board = Board.Piece_Bitboards[APiece.Make_Piece(EPiece_Type.Pawn, Board.Opponent_Color)];
            Opponent_Pawn_Attack_Map = AsBitboard_Utility.Get_Pawn_Attacks(opponent_pawns_board, !Is_White_To_Move);

            if (AsBitboard_Utility.Contains_Square(Opponent_Pawn_Attack_Map, Friendly_King_Square))
            {
                In_Double_Check = In_Check;
                In_Check = true;
                ulong possible_pawn_attack_origins = Board.Is_White_To_Move ? AsBitboard_Utility.White_Pawn_Attacks[Friendly_King_Square] : AsBitboard_Utility.Black_Pawn_Attacks[Friendly_King_Square];
                ulong pawn_check_map = opponent_pawns_board & possible_pawn_attack_origins;
                Check_Ray_Bitmask |= pawn_check_map;
            }
        }

        private void Gen_Sliding_Attack_Map()
        {
            Opponent_Sliding_Attack_Map = 0;

            Update_Slide_Attack(Board.Enemy_Orthogonal_Sliders, true);
            Update_Slide_Attack(Board.Enemy_Diagonal_Sliders, false);
        }

        private void Update_Slide_Attack(ulong piece_board, bool ortho)
        {
            ulong blockers = Board.All_Pieces_Bitboard & ~(1ul << Friendly_King_Square);

            while (piece_board != 0)
            {
                int start_square = AsBitboard_Utility.Pop_LSB(ref piece_board);
                ulong move_board = AsBitboard_Magics.Get_Slider_Attacks(start_square, blockers, ortho);
                Opponent_Sliding_Attack_Map |= move_board;
            }
        }

        private bool In_Check_After_En_Passant(int start_square, int target_square, int ep_capture_square)
        {
            ulong enemy_ortho = Board.Enemy_Orthogonal_Sliders;

            if (enemy_ortho != 0)
            {
                ulong masked_blockers = (All_Pieces ^ (1ul << ep_capture_square | 1ul << start_square | 1ul << target_square));
                ulong rook_attacks = AsBitboard_Magics.Get_Rook_Attacks(Friendly_King_Square, masked_blockers);
                return (rook_attacks & enemy_ortho) != 0;
            }

            return false;
        }

    }
}
