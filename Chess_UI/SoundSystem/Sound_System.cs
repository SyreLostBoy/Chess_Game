using Chess_Logic;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace Sound_System
{
    public class AsSound_System
    {
        private SoundPlayer Player;

        public AsSound_System()
        {
            Player = new SoundPlayer();
        }

        public void Play_Move_Sound(EMove_Type move_type, bool has_captured = false)
        {
            string sound_path;

            if (has_captured)
            {
                sound_path = Path.Combine(AsConfig.Audio_Base_Path, "Capture.wav");
            }
            else
            {
                sound_path = Path.Combine(AsConfig.Audio_Base_Path, AsConfig.Move_Sounds[move_type]);
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
