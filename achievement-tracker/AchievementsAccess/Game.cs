using System;
using System.Collections.Generic;

namespace AchievementsAccess
{
    public class Game
    {
        String source;
        public String title;
        public int totalGS;
        public int currentGS;
        public int totalAch;
        public int currentAch;
        public List<Achievement> achievements;
        public List<DLC> dlc;
        public String url;

        public Game(String src)
        {
            source = src;
            achievements = new List<Achievement>();
            dlc = new List<DLC>();
        }
    }
}
