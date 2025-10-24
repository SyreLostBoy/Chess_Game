using Chess_Logic;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace Sound_System
{
    public class AsSound_System
    {
        private SoundPlayer Player;
        private string Base_Audio_Path;

        private Dictionary<EMove_Type, string> Move_Sounds = new Dictionary<EMove_Type, string>
        {
            {EMove_Type.Normal, "Move.wav" },
            {EMove_Type.Double_Pawn_Move, "Move.wav" },
            {EMove_Type.En_Passant, "Capture.wav" },
            {EMove_Type.Castle_KS, "Move.wav" },
            {EMove_Type.Castle_QS, "Move.wav" },
            {EMove_Type.Pawn_Promotion, "Move.wav" }
        };

        public AsSound_System()
        {
            Player = new SoundPlayer();
            Base_Audio_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Audio");
        }

        public void Play_Move_Sound(EMove_Type move_type, bool has_captured = false)
        {
            string sound_path;

            if (has_captured)
            {
                sound_path = Path.Combine(Base_Audio_Path, "Capture.wav");
            }
            else
            {
                sound_path = Path.Combine(Base_Audio_Path, Move_Sounds[move_type]);
            }

            if (!Path.Exists(sound_path))
            {
                return;
            }

            Player.SoundLocation = sound_path;

            Player.Load();
            Player.Play();
        }
    }
}
