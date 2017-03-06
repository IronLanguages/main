using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Iron7.Common
{
    public class SoundEffectPlayer
    {
        
        private readonly SoundEffect soundEffect;

        public SoundEffectPlayer(SoundEffect soundEffect)
        {
            this.soundEffect = soundEffect;
        }

        public void Play()
        {
            FrameworkDispatcher.Update();
            soundEffect.Play();
        }
    }
}
