using Chess_Engine.Core;
using Chess_Engine.Helpers;
using System.Collections.ObjectModel;

public class AsFen_Utility
{
    public const string Start_Position_Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static SPosition_Info Position_From_Fen(string fen)
    {
        SPosition_Info loaded_position_info = new(fen);
        return loaded_position_info;
    }

    public static string Get_Current_Fen(ABoard board, bool always_include_ep_square = true)
    {
        /// <summary>
        /// Возвращает FEN строку текущей позиции
        // Если always_include_ep_square равно true, то поле en passant будет включено
        // в FEN строку, даже если ни одна вражеская пешка не находится в позиции для захвата.
        /// </summary>

        string fen = Build_Board_Fen(board);
        fen += Build_Side_To_Move_Fen(board);
        fen += Build_Castling_Fen(board);
        fen += Build_En_Passant_Fen(board, always_include_ep_square);
        fen += Build_Move_Counters_Fen(board);

        return fen;
    }

    private static string Build_Board_Fen(ABoard board)
    {
        string fen = "";

        for (int rank = 7; rank >= 0; rank--)
        {
            int empty_files_count = 0;

            for (int file = 0; file < 8; file++)
            {
                int square_index = rank * 8 + file;
                int piece = board.Square[square_index];

                if (piece != 0)
                {
                    if (empty_files_count != 0)
                    {
                        fen += empty_files_count;
                        empty_files_count = 0;
                    }

                    char piece_symbol = APiece.Get_Piece_Symbol(piece);
                    fen += piece_symbol;
                }
                else
                {
                    empty_files_count++;
                }
            }

            if (empty_files_count != 0)
            {
                fen += empty_files_count;
            }

            if (rank != 0)
            {
                fen += '/';
            }
        }

        return fen;
    }

    private static string Build_Side_To_Move_Fen(ABoard board)
    {
        return $" {(board.Is_White_To_Move ? 'w' : 'b')}";
    }

    private static string Build_Castling_Fen(ABoard board)
    {
        bool white_kingside = (board.Current_Game_State.Castling_Rights & 1) == 1;
        bool white_queenside = (board.Current_Game_State.Castling_Rights >> 1 & 1) == 1;
        bool black_kingside = (board.Current_Game_State.Castling_Rights >> 2 & 1) == 1;
        bool black_queenside = (board.Current_Game_State.Castling_Rights >> 3 & 1) == 1;

        string castling_fen = "";
        castling_fen += white_kingside ? "K" : "";
        castling_fen += white_queenside ? "Q" : "";
        castling_fen += black_kingside ? "k" : "";
        castling_fen += black_queenside ? "q" : "";

        if (castling_fen.Length == 0)
        {
            castling_fen = "-";
        }

        return $" {castling_fen}";
    }

    private static string Build_En_Passant_Fen(ABoard board, bool always_include_ep_square)
    {
        int ep_file_index = board.Current_Game_State.En_Passant_File - 1;
        int ep_rank_index = board.Is_White_To_Move ? 5 : 2;

        bool is_en_passant = ep_file_index != -1;
        bool include_ep = always_include_ep_square || En_Passant_Can_Be_Captured(ep_file_index, ep_rank_index, board);

        if (is_en_passant && include_ep)
        {
            string square_name = AsBoard_Helper.Square_Name_From_Coordinate(ep_file_index, ep_rank_index);
            return $" {square_name}";
        }

        return " -";
    }

    private static string Build_Move_Counters_Fen(ABoard board)
    {
        return $" {board.Current_Game_State.Fifty_Move_Counter} {(board.Ply_Count / 2) + 1}";
    }

    private static bool En_Passant_Can_Be_Captured(int ep_file_index, int ep_rank_index, ABoard board)
    {
        int direction = board.Is_White_To_Move ? -1 : 1;
        SCoordinate capture_from_a = new SCoordinate(ep_file_index - 1, ep_rank_index + direction);
        SCoordinate capture_from_b = new SCoordinate(ep_file_index + 1, ep_rank_index + direction);

        int ep_capture_square = new SCoordinate(ep_file_index, ep_rank_index).Square_Index;
        int friendly_pawn = APiece.Make_Piece(EPiece_Type.Pawn, board.Move_Color);

        return Can_Capture(capture_from_a) || Can_Capture(capture_from_b);

        bool Can_Capture(SCoordinate from)
        {
            bool is_pawn_on_square = board.Square[from.Square_Index] == friendly_pawn;
            if (from.Is_Valid_Square() && is_pawn_on_square)
            {
                SMove move = new SMove(from.Square_Index, ep_capture_square, SMove.En_Passant_Capture_Flag);
                board.Make_Move(move);
                board.Make_Null_Move();
                bool was_legal_move = !board.Calculate_In_Check_State();

                board.Unmake_Null_Move();
                board.Unmake_Move(move);
                return was_legal_move;
            }

            return false;
        }
    }

    public static string Flip_Fen(string fen)
    {
        string flipped_fen = "";
        string[] sections = fen.Split(' ');

        string[] fen_ranks = sections[0].Split('/');

        for (int i = fen_ranks.Length - 1; i >= 0; i--)
        {
            string rank = fen_ranks[i];
            foreach (char character in rank)
            {
                flipped_fen += Invert_Case(character);
            }
            if (i != 0)
            {
                flipped_fen += '/';
            }
        }

        flipped_fen += $" {(sections[1][0] == 'w' ? 'b' : 'w')}";

        string castling_rights = sections[2];
        string flipped_rights = "";
        foreach (char character in "kqKQ")
        {
            if (castling_rights.Contains(character))
            {
                flipped_rights += Invert_Case(character);
            }
        }
        flipped_fen += $" {(flipped_rights.Length == 0 ? "-" : flipped_rights)}";

        string ep_section = sections[3];
        string flipped_ep = ep_section[0] + "";
        if (ep_section.Length > 1)
        {
            flipped_ep += ep_section[1] == '6' ? '3' : '6';
        }
        flipped_fen += $" {flipped_ep}";
        flipped_fen += $" {sections[4]} {sections[5]}";

        return flipped_fen;

        char Invert_Case(char character)
        {
            return char.IsLower(character) ? char.ToUpper(character) : char.ToLower(character);
        }
    }
}

public readonly struct SPosition_Info
{
    public readonly string Fen;
    public readonly ReadOnlyCollection<int> Squares;

    // Права рокировок
    public readonly bool White_Castle_Kingside;
    public readonly bool White_Castle_Queenside;
    public readonly bool Black_Castle_Kingside;
    public readonly bool Black_Castle_Queenside;

    // En-passant файл (1 — a-файл, 8 — h-файл, 0 — нет)
    public readonly int Ep_File;
    public readonly bool White_To_Move;

    // Количество полуходов с момента последнего взятия или продвижения пешки
    // (начинается с 0 и увеличивается после каждого хода игрока)
    public readonly int Fifty_Move_Ply_Count;

    // Общее количество ходов, сыгранных в партии
    // (начинается с 1 и увеличивается после хода черных)
    public readonly int Move_Count;

    public SPosition_Info(string fen)
    {
        Fen = fen;
        int[] square_pieces = new int[64];

        string[] sections = fen.Split(' ');

        int file = 0;
        int rank = 7;

        foreach (char symbol in sections[0])
        {
            if (symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(symbol))
                {
                    file += (int)char.GetNumericValue(symbol);
                }
                else
                {
                    int piece_color = char.IsUpper(symbol) ? APiece.White : APiece.Black;
                    EPiece_Type piece_type = APiece.Get_Piece_Type_From_Symbol(symbol);
                    square_pieces[rank * 8 + file] = (int)piece_type | piece_color;
                    file++;
                }
            }
        }

        Squares = new ReadOnlyCollection<int>(square_pieces);

        White_To_Move = sections[1] == "w";

        string castling_rights = sections[2];
        White_Castle_Kingside = castling_rights.Contains('K');
        White_Castle_Queenside = castling_rights.Contains('Q');
        Black_Castle_Kingside = castling_rights.Contains('k');
        Black_Castle_Queenside = castling_rights.Contains('q');

        Ep_File = 0;
        Fifty_Move_Ply_Count = 0;
        Move_Count = 0;

        if (sections.Length > 3)
        {
            string en_passant_file_name = sections[3][0].ToString();
            if (AsBoard_Helper.File_Names.Contains(en_passant_file_name))
            {
                Ep_File = AsBoard_Helper.File_Names.IndexOf(en_passant_file_name) + 1;
            }
        }

        if (sections.Length > 4)
        {
            int.TryParse(sections[4], out Fifty_Move_Ply_Count);
        }

        if (sections.Length > 5)
        {
            int.TryParse(sections[5], out Move_Count);
        }
    }
}