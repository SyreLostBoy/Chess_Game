using Chess_Engine.Core;
using Chess_UI.Board_Renderer;
using Chess_UI.Game_Controller;
using Chess_UI.Helpers;
using Chess_UI.Menus;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chess_UI
{
    public partial class MainWindow : Window
    {
        private AsGame_Controller Game_Controller;
        private AsBoard_Renderer Board_Renderer;
        private AsMain_Menu Main_Menu;

        private bool Is_Dragging = false;
        private Image Dragged_Piece, Dragged_Piece_Clone;
        private int Selected_Square = -1;
        private Dictionary<int, SMove> Move_Cache = new Dictionary<int, SMove>();
        private int Check_King_Square = -1;
        private Point Drag_Start_Point;
        private int Drag_Start_Square;
        private Canvas Drag_Canvas;

        public const int Drag_Piece_Width = 80;
        public const int Drag_Piece_Height = 80;
        public const double Drag_Piece_Offset = 30;
        public const double Drag_Piece_Opacity = 0.8;
        public const double Original_Piece_Opacity_When_Dragging = 0.3;

        public MainWindow()
        {
            InitializeComponent();
            
            Game_Controller = new AsGame_Controller();
            Board_Renderer = new AsBoard_Renderer(Board_Grid, Piece_Grid, Highlight_Grid);

            Initialize_Drag_Canvas();

            Game_Controller.On_Move_Made += On_Move_Made;
            Game_Controller.On_Game_Ended += On_Game_Ended;
            Game_Controller.On_Board_Enabled_Changed += On_Board_Enabled_Changed;

            Show_Main_Menu();
        }

        private void Show_Main_Menu()
        {
            Main_Menu = new AsMain_Menu();
            Main_Menu.On_Game_Started += Handle_Game_Started;
            Main_Menu.On_Show_Settings += Show_Settings_Menu;
            Main_Menu.On_Exit += On_Exit_Game;

            Main_Menu_Container.Content = Main_Menu;
        }

        private void Handle_Game_Started(SGame_Settings settings)
        {
            Apply_Game_Settings(settings);

            Main_Menu_Container.Content = null;

            Start_Game_With_Settings(settings);
        }

        private void Apply_Game_Settings(SGame_Settings settings)
        {
            Game_Controller.Set_Sound_System_Enabled(true);

            //Board_Renderer.Set_Highlight_Enabled(settings.Additional_Settings.Highlight_Moves);

            if (settings.Game_Mode != EGame_Mode.Human_Vs_Human)
            {
                Game_Controller.Set_Bot_Difficulty(settings.Bot_Difficulty);
            }
        }

        private void Start_Game_With_Settings(SGame_Settings settings)
        {
            bool playing_against_bot = settings.Game_Mode != EGame_Mode.Human_Vs_Human;
            bool bot_vs_bot = settings.Game_Mode == EGame_Mode.Bot_Vs_Bot;
            bool human_is_white = true;

            if (settings.Player_Is_White.HasValue)
            {
                human_is_white = settings.Player_Is_White.Value;
            }
            else
            {
                Random rand = new Random();
                human_is_white = rand.Next(0, 2) == 0;
            }

            if (bot_vs_bot)
            {
                Start_Bot_Vs_Bot_Game();
            }
            else
            {
                Start_New_Game(playing_against_bot, human_is_white);
            }
        }

        private void Start_Bot_Vs_Bot_Game()
        {//!!! Надо сделать
            throw new Exception();
        }

        private void Show_Settings_Menu()
        {
            SettingsMenu settings_menu = new SettingsMenu();
            settings_menu.On_Back += () => Show_Main_Menu();
            Main_Menu_Container.Content = settings_menu;
        }

        private void Initialize_Drag_Canvas()
        {
            Drag_Canvas = new Canvas();
            Drag_Canvas.IsHitTestVisible = false;
            Board_Grid.Children.Add(Drag_Canvas);
        }

        private void Start_New_Game(bool playing_against_bot, bool human_is_white)
        {
            Game_Controller.Start_New_Game(playing_against_bot, human_is_white);
            Board_Renderer.Flip_Board(!human_is_white);

            Clear_Selection();
            Board_Renderer.Draw_Board(Game_Controller.Board);
        }

        private void Clear_Selection()
        {
            Selected_Square = -1;
            Move_Cache.Clear();
            Dragged_Piece = null;
            Is_Dragging = false;
            
            Update_Highlights();
        }

        #region Обработчики событий игры

        private void On_Move_Made(SMove move)
        {
            Dispatcher.Invoke(() =>
            {
                Board_Renderer.Draw_Board(Game_Controller.Board);
                Update_Highlights();
            });
        }

        private void On_Game_Ended(EGame_Result result)
        {
            Dispatcher.Invoke(() =>
            {
                Show_Game_Over_Menu(result);
            });
        }

        private void On_Board_Enabled_Changed(bool enabled)
        {
            Dispatcher.Invoke(() =>
            {
                Board_Grid.IsEnabled = enabled;
            });
        }
        #endregion

        #region Обработчики мыши
        private void On_Board_Grid_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            if (!Board_Grid.IsEnabled || Is_Menu_On_Screen() )
            {
                return;
            }

            Point point = e.GetPosition(Board_Grid);
            int square = AsCoordinate_Helper.Get_Square_From_Position(point, Board_Grid.ActualWidth, Board_Renderer.Is_Board_Flipped);

            if (square >= 0 && square < 64)
            {
                int piece = Game_Controller.Board.Square[square];
                if (piece != (int)EPiece_Type.None && APiece.Is_Color(piece, Game_Controller.Board.Move_Color))
                {
                    Start_Dragging(square, point);
                }
                else if (Selected_Square != -1 && Move_Cache.TryGetValue(square, out SMove move))
                {
                    if (move.Is_Promotion)
                    {
                        Handle_Promotion(move.Start_Square, move.Target_Square);
                    }
                    else
                    {
                        Game_Controller.Try_Make_Move(move);
                    }
                    Clear_Selection();
                }
                else
                {
                    Clear_Selection();
                }
            }
        }

        private void On_Board_Grid_Mouse_Move(object sender, MouseEventArgs e)
        {
            if (Is_Dragging && e.LeftButton == MouseButtonState.Pressed && Dragged_Piece_Clone != null)
            {
                Point current_point = e.GetPosition(Board_Grid);
                Continue_Dragging(current_point);
            }
        }

        private void On_Board_Grid_Mouse_Up(object sender, MouseButtonEventArgs e)
        {
            if (Is_Dragging)
            {
                Point end_point = e.GetPosition(Board_Grid);
                int end_square = Board_Renderer.Get_Square_From_Position(end_point);
                End_Dragging(end_square);
            }
        }

        #endregion

        #region Drag & Drop Logic

        private void Start_Dragging(int square, Point point)
        {
            Is_Dragging = true;
            Drag_Start_Square = square;
            Drag_Start_Point = point;

            int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(square, Board_Renderer.Is_Board_Flipped);
            Dragged_Piece = Get_Image_By_Index(ui_index);

            if (Dragged_Piece.Source == null)
            {
                Is_Dragging = false;
                return;
            }

            Dragged_Piece_Clone = new Image
            {
                Source = Dragged_Piece.Source,
                Width = Drag_Piece_Width,
                Height = Drag_Piece_Height,
                Opacity = Drag_Piece_Opacity
            };

            Canvas.SetLeft(Dragged_Piece_Clone, point.X - Drag_Piece_Offset);
            Canvas.SetTop(Dragged_Piece_Clone, point.Y - Drag_Piece_Offset);

            Drag_Canvas.Children.Add(Dragged_Piece_Clone);
            Dragged_Piece.Opacity = Original_Piece_Opacity_When_Dragging;

            On_From_Square_Selected(square);
            Board_Grid.CaptureMouse();
        }

        private void Continue_Dragging(Point current_point)
        {
            if (Dragged_Piece_Clone == null)
            {
                return;
            }

            Canvas.SetLeft(Dragged_Piece_Clone, current_point.X - Drag_Piece_Offset);
            Canvas.SetTop(Dragged_Piece_Clone, current_point.Y - Drag_Piece_Offset);
        }

        private void End_Dragging(int end_square)
        {
            Is_Dragging = false;
            Board_Grid.ReleaseMouseCapture();

            if (Dragged_Piece_Clone != null)
            {
                Drag_Canvas.Children.Remove(Dragged_Piece_Clone);
                Dragged_Piece_Clone = null;
            }

            if (Dragged_Piece != null)
            {
                int ui_index = AsCoordinate_Helper.Get_UI_Index_From_Board_Square(Drag_Start_Square, Board_Renderer.Is_Board_Flipped);
                Dragged_Piece = Get_Image_By_Index(ui_index);
                Dragged_Piece.Opacity = 1.0;

                if (end_square == Drag_Start_Square)
                {
                    return;
                }

                if (end_square >= 0 && end_square < 64 && end_square != Drag_Start_Square)
                {
                    if (Move_Cache.TryGetValue(end_square, out SMove move))
                    {
                        if (move.Is_Promotion)
                        {
                            Handle_Promotion(move.Start_Square, move.Target_Square);
                        }
                        else
                        {
                            Game_Controller.Try_Make_Move(move);
                        }
                        Clear_Selection();
                    }
                    else
                    {
                        Clear_Selection();
                    }
                }
                else
                {
                    Clear_Selection();
                }
            }
        }

        #endregion

        private void On_From_Square_Selected(int square)
        {
            var moves = Game_Controller.Get_Legal_Moves_For_Square(square);
            Selected_Square = square;

            if (moves.Any())
            {
                Cache_Moves(moves);
            }
            else
            {
                Move_Cache.Clear();
            }

            Update_Highlights();
        }

        private void Update_Highlights()
        {
            SMove? last_move = Game_Controller.Get_Last_Move();
            Board_Renderer.Update_Higlights(Selected_Square, Move_Cache, Game_Controller.Board, last_move);
        }

        private Image Get_Image_By_Index(int index)
        {
            if (Piece_Grid.Children.Count > index && Piece_Grid.Children[index] is Image image)
            {
                return image;
            }

            return null;
        }

        private void Cache_Moves(IEnumerable<SMove> moves)
        {
            Move_Cache.Clear();
            
            foreach (SMove move in moves)
            {
                Move_Cache[move.Target_Square] = move;
            }
        }

        private void Handle_Promotion(int from_square, int to_square)
        {
            Show_Promotion_Menu(from_square, to_square);
        }

        private void Show_Promotion_Menu(int pawn_square, int target_square)
        {
            Promotion_Menu promotion_menu = new Promotion_Menu(Game_Controller.Board.Is_White_To_Move);
            
            promotion_menu.OnPieceSelected += (piece_type) =>
            {
                Complete_Promotion_Move(pawn_square, target_square, piece_type);
            };

            Menu_Container.Content = promotion_menu;
        }

        public void Complete_Promotion_Move(int from_square, int to_square, EPiece_Type promotion_type)
        {
            int flag = Get_Promotion_Flag(promotion_type);
            SMove promotion_move = new SMove(from_square, to_square, flag);

            Game_Controller.Try_Make_Move(promotion_move);
            Menu_Container.Content = null;
        }

        private int Get_Promotion_Flag(EPiece_Type piece_type)
        {
            switch (piece_type)
            {
                case EPiece_Type.Queen: return SMove.Promote_To_Queen_Flag;
                case EPiece_Type.Rook: return SMove.Promote_To_Rook_Flag;
                case EPiece_Type.Bishop: return SMove.Promote_To_Bishop_Flag;
                case EPiece_Type.Knight: return SMove.Promote_To_Knight_Flag;
                default: return SMove.Promote_To_Queen_Flag;
            }
        }

        private void Show_Pause_Menu()
        {
            PauseMenu pause_menu = new PauseMenu();
            pause_menu.OnContinue += On_Continue_Game;
            pause_menu.OnRestart += On_Restart_Game;
            Menu_Container.Content = pause_menu;
        }

        private void Show_Game_Over_Menu(EGame_Result result)
        {
            Game_Over_Menu game_over_menu = new Game_Over_Menu();
            game_over_menu.SetResult(result);
            game_over_menu.OnRestart += On_Restart_Game;
            game_over_menu.OnExit += On_Exit_Game;
            Menu_Container.Content = game_over_menu;
        }

        private void On_Continue_Game()
        {
            Menu_Container.Content = null;
        }

        private void On_Restart_Game()
        {
            Game_Controller.Start_New_Game(Game_Controller.Playing_Against_Bot, Game_Controller.Human_Is_White);
            Board_Renderer.Draw_Board(Game_Controller.Board);
            Clear_Selection();
            Menu_Container.Content = null;
        }

        private void On_Exit_Game()
        {
            this.Close();
        }

        private bool Is_Menu_On_Screen()
        {
            return Menu_Container.Content != null;
        }

        private void On_Window_Key_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Is_Menu_On_Screen())
                {
                    Menu_Container.Content = null;
                }
                else
                {
                    Show_Pause_Menu();
                }
            }
        }
    }
}