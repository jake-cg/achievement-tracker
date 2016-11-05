using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsAccess
{
    public class DLC
    {
        public String source;
        public String title;
        public int totalGS;
        public int currentGS;
        public int totalAch;
        public int currentAch;
        public List<Achievement> achievements;

        public DLC(String src)
        {
            source = src;
            achievements = new List<Achievement>();
        }
    }
}