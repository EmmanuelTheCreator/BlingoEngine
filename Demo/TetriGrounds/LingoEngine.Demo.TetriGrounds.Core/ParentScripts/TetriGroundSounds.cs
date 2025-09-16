// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Core;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    internal static class TetriGroundSounds
    {
        private static Random _random = new();
        public static void SoundPlayGong(this ILingoPlayer player) => player.Sound.PuppetSound(6, "S_Gong");
        public static void SoundPlayBtnStart(this ILingoPlayer player) => player.Sound.PuppetSound(5, "S_BtnStart");
        public static void SoundPlayRemoveRow(this ILingoPlayer player) => player.Sound.PuppetSound(4, "S_DeleteRow");
        public static void SoundPlayClick2(this ILingoPlayer player) => player.Sound.PuppetSound(5, "S_Click2");
        public static void SoundPlayGo(this ILingoPlayer player) => player.Sound.PuppetSound(4, "S_GO");
        public static void SoundPlayLevelUp(this ILingoPlayer player) => player.Sound.PuppetSound(3, "S_LevelUp");
        public static void SoundPlayShhh1(this ILingoPlayer player) => player.Sound.PuppetSound(3, "S_Shhh1");
        public static void SoundPlayTerminated(this ILingoPlayer player) => player.Sound.PuppetSound(4, "S_Terminated");
        public static void SoundPlayDied(this ILingoPlayer player) => player.Sound.PuppetSound(2, "S_Died");
        public static void SoundPlayNature(this ILingoPlayer player)
        {
            player.Sound.Channel(1)!.Volume = 40;
            player.Sound.PuppetSound(1, "S_Nature");
        }

        public static void SoundStopNature(this ILingoPlayer player) => player.Sound.Channel(1)!.Stop();


        public static void SoundPlayRowsDeleted(this ILingoPlayer player, int rows)
        {
            if (rows < 2) return;
            if (rows > 4) rows = 4;
            var sound = "S_RowsDeleted_" + rows.ToString();
            player.Sound.PuppetSound(2, sound);
        }

        public static void SoundPlayBlockDown(this ILingoPlayer player,int rowsPercentLeft)
        {
            //Console.WriteLine($"rowsLeft {rowsLeft}");
            var soundId = Math.Round((float)(rowsPercentLeft-2) / 2f)+1;// _random.Next(4) + 1;
            if (_random.Next(3) == 1) 
                soundId += 1; // chance to play a higher sound
            if (soundId < 1) soundId = 1;
            if (soundId > 5) soundId = 5;
            var sound = "S_BlockFall" + soundId.ToString();
            //Console.WriteLine($"Play sound {sound} {rowsPercentLeft}");
            player.Sound.PuppetSound(5, sound);
        }
    }
}
