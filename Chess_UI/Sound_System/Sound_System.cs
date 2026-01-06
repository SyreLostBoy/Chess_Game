using Chess_Engine.Core;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace Sound_System
{
    public class AsSound_System
    {
        public bool Enabled { get; set; }

        public readonly static string Audio_Base_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Audio");
        
        private SoundPlayer Player;



        public AsSound_System()
        {
            Player = new SoundPlayer();
        }

        public void Play_Move_Sound(SMove move, bool has_captured = false)
        {
            string sound_path;


            if (has_captured)
            {
                sound_path = Path.Combine(Audio_Base_Path, "Capture.wav");
            }
            else
            {
                sound_path = Path.Combine(Audio_Base_Path, "Move.wav");
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
