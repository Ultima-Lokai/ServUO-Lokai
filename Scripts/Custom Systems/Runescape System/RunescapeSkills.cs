using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Server.Custom_Systems.Runescape_System
{
    /*  Attack
        Defense
        Strength
        Hitpoints
        Ranged
        Prayer
        Magic
        Cooking
        Woodcutting
        Fletching
        Fishing
        Firemaking
        Crafting
        Smithing
        Mining
        Herblore
        Agility
        Thieving
        Slayer
        Farming
        Runecrafting
        Hunter
        Construction
        Summoning
        Divining
     */

    class RunescapeSkills
    {
        public static void Initialize()
        {
            Console.WriteLine("Exp needed from Level 1 - Level 45: " + ShowSkillNeeded(2, 45) + " xp");
            Console.WriteLine("Exp needed from Level 45 - Level 99: " + ShowSkillNeeded(45, 99) + " xp");
            Console.WriteLine("Exp needed from Level 1 - Level 99: " + ShowSkillNeeded(2, 99) + " xp");
        }

        public static int ShowSkillNeeded(int currentLevel, int desiredLevel)
        {
            int totalpoints = 0;
            int mypoints = 0;

            for (int lvl = 1; lvl < desiredLevel; lvl++)
            {
                totalpoints += (int)Math.Floor(lvl + 300 * Math.Pow(2, (double)lvl / 7));
                if (lvl == currentLevel)
                    mypoints = totalpoints;
            }
            return (int)Math.Floor((double)(totalpoints - mypoints) / 4);
        }
    }
}
