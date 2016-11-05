using System;
using System.IO;
using System.Net;
using System.Collections.Generic;


namespace AchievementsAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            //Using True Achievements
            string gamertag;
            const int TYPES = 4;
            // Console.Write("What is your gamertag?\nGamertag: ");
            gamertag = "N1NJA GEORGE";//Console.ReadLine();


            String trueURL = getTrueURL(gamertag);
            String trueHTML = getHtml(trueURL);
            String playerID = getPlayerID(trueHTML);

            String gamesPageURL = trueGamesURL(playerID);
            String gamesPageHTML = getHtml(gamesPageURL);
            String allGamesInfo = getPlainText(gamesPageHTML);
            String[] roughGamesData = gamesBreakdown(allGamesInfo, getNoRows(gamesPageHTML));
            String[] gamesData = removeWhiteSpaceLines(roughGamesData);
            String[,] finalizedData = getFinalizedData(gamesData, TYPES);
            List<Game> Games = new List<Game>();

            for (int i = 0; i < Games.Length; i++)
            {
                Games[i] = new Game();
            }

                makeGameObjects(finalizedData, Games);

            String[] titles = new String[finalizedData.GetLength(0)];

            titles = allGames(finalizedData);

            for (int gameNum = 0; gameNum < 100; gameNum++)
            {
                String URLend = getGamePageExtension(gamesPageHTML, titles, gameNum);
                String gameURL = "http://www.trueachievements.com" + URLend;
                String gameHTML = getHtml(gameURL);
                String achData = gamePlainText(gameHTML, Games, gameNum); //will change this method to void when it finishes Games data
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\Jake\\Documents\\Achievements Access\\TestAA.txt");

            for (int i = 0; i < Games.Length; i++)
            {
                file.WriteLine(i + 1 + ". " + Games[i].name);
                file.WriteLine("Total GS: " + Games[i].totalGS + "  Current GS: " + Games[i].currentGS);
                file.WriteLine("Total Ach: " + Games[i].totalACH + "  Current Ach: " + Games[i].currentACH);
                for (int j = 0; j < Games[i].totalACH; j++)
                {
                    file.WriteLine("    " + Games[i].achievementNames[j] + "- " + Games[i].achievementGS[j] + "GS - " + Games[i].unlocked[j] + "- " + Games[i].achievementDesc[j]);
                }
                file.WriteLine();
                Console.Write(".");
            }

            file.Close();





            //TO DO
            //1. instead of getting HTML from where I am now, get it from same page with "show all" activated
            //*2. parse through string. make an array of strings, one line from the current string is an object in the array.// DONE
            //*3. go through array and parse each string into constituate parts. Game Objects? 2D array?// DONE
            //*4. display data. Game title, Total Ach, Won Ach, Remaining Ach, Total GS, Won GS, Remaining GS// DONE
            //*5. put "Display All"  in a method. User can decide whether to display all or just info for a certain game. //DONE
            //*6. User can pick a game//Done
            //7. Display ach info for a game
            //*8. Find better way to get game page URL. Currently doesnt work when encountering special character(s) ie. II, &, ', etc. Try getting from games page HTML directly instead of building it. //DONE

            Console.WriteLine("\n\ndone");
            Console.Read();
        }

        static String getTrueURL(String gamertag)
        {
            String trueURLstart = "http://www.trueachievements.com/";
            String trueURLend = ".htm";

            if (gamertag.Contains(" "))
            {
                return trueURLstart + modify(gamertag, "+") + trueURLend;
            }
            else
            {
                return trueURLstart + gamertag + trueURLend;
            }
        }


        static String modify(String str, String token)
        {
            return str.Replace(" ", token);
        }


        static String getHtml(string trueURL)
        {
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString(trueURL);
                return htmlCode;
            }
        }


        static String getPlayerID(String HTMLofMain)
        {
            int index = indexOfFirstDigit(HTMLofMain);
            int ID_length = lengthOfID(HTMLofMain, index);

            String playerID = HTMLofMain.Substring(index, ID_length);

            return playerID;
        }

        static int indexOfFirstDigit(String HTML)
        {
            String prefix = "<a href=\"/mygamecollection.aspx?gamerid=";
            int index = 0;
            int count = HTML.IndexOf(prefix);

            bool passValue = false;
            while (!passValue)
            {
                if (Char.IsDigit(HTML, count))
                {
                    index = count;
                    passValue = true;
                }
                count++;
            }

            return index;
        }


        static int lengthOfID(String HTML, int index)
        {
            bool passEnd = false;
            int indexOut = 0;
            int startingValue = index;
            while (!passEnd)
            {
                if (!Char.IsDigit(HTML, index))
                {
                    indexOut = index;
                    passEnd = true;
                }
                index++;
            }
            return indexOut - startingValue;
        }


        static String trueGamesURL(String PlayerID)
        {
            String trueURLstart = "http://www.trueachievements.com/gamergames.aspx?gamerid=";
            return trueURLstart + PlayerID;
        }

        static int getNoRows(String HTML)
        {
            string line;
            int rows = 0;
            using (StringReader reader = new StringReader(HTML))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    rows++;
                }
            }
            return rows;
        }


        static String getPlainText(String HTML)
        {
            int linesInHTML = getNoRows(HTML);
            int noLines = 0;
            string line;
            bool inTable = false;
            String final = "";

            using (StringReader reader = new StringReader(HTML))
            {
                String editedString = "";
                while ((line = reader.ReadLine()) != null)
                {

                    bool inTag = false;
                    if (line.Contains("maintable"))
                    {
                        inTable = true;
                    }
                    else if (line.Contains("</table>"))
                    {
                        inTable = false;
                    }
                    for (int i = 0; i <= line.Length - 1; i++)
                    {
                        if (line[i] == '<')
                        {
                            inTag = true;
                        }
                        else if (line[i] == '>')
                        {
                            inTag = false;
                        }

                        if (inTag == false && line[i] != '>' && inTable && !line.Contains("<th"))
                        {
                            editedString += line[i];
                        }

                    }
                    if (line.Contains("</td>"))
                    {
                        editedString += "  ";
                    }
                    if (line.Contains("</tr>"))
                    {
                        editedString += "\n";
                    }
                    if (noLines == linesInHTML - 1)
                    {
                        final = editedString;
                    }
                    noLines++;
                }
            }
            return final;
        }

        static String[] gamesBreakdown(String gamesInfo, int rows)
        {
            String[] gamesRows = new String[rows];
            String line;
            using (StringReader reader = new StringReader(gamesInfo))
            {
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    gamesRows[i] = line;
                    i++;
                }
            }
            return gamesRows;
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static String[] removeWhiteSpaceLines(String[] arrayToClean)
        {
            int goodRows = 0;
            for (int i = 0; i < arrayToClean.Length; i++)
            {
                if (!IsNullOrWhiteSpace(arrayToClean[i]))
                {
                    goodRows++;
                }
            }

            String[] cleanedArray = new String[goodRows];
            int row = 0;
            for (int i = 0; i < arrayToClean.Length; i++)
            {
                if (!IsNullOrWhiteSpace(arrayToClean[i]))
                {
                    cleanedArray[row] = arrayToClean[i];
                    row++;
                }
            }

            return cleanedArray;
        }


        static String[,] getFinalizedData(String[] xArray, int yLength)
        {
            String[,] finalizedData = new String[xArray.Length, yLength];

            for (int i = 0; i < xArray.Length; i++)
            {
                String line = xArray[i];
                for (int j = 0; j < yLength; j++)
                {
                    finalizedData[i, j] = line.Substring(IndexOfOccurence(line, j) + 2, (IndexOfOccurence(line, j + 1) - IndexOfOccurence(line, j) - 2));
                }
            }

            return finalizedData;
        }

        static int IndexOfOccurence(string line, int occurence)
        {
            String match = "  ";
            int i = 1;
            int index = 0;

            while (i <= occurence && (index = line.IndexOf(match, index + 1)) != -1)
            {
                if (i == occurence)
                    return index;

                i++;
            }

            return 0;
        }

        static String getCurrent(String data)
        {
            String temp = data.Remove(data.IndexOf(" of"));
            return temp;
        }

        static String getTotal(String data)
        {
            data = data.Replace(data.Substring(0, data.IndexOf(" of") + 4), "");
            String temp = data;
            return temp;
        }

        static int getRemaining(int current, int total)
        {
            return (total - current);
        }


        static void makeGameObjects(String[,] finalizedData, Game[] allGames)
        {
            //Creating all game objects'
            for (int i = 0; i < allGames.Length; i++)
            {
                finalizedData[i, 3] = finalizedData[i, 3].Replace(",", "");
                finalizedData[i, 3] = finalizedData[i, 3].Replace("(", "");
                finalizedData[i, 3] = finalizedData[i, 3].Replace(")", "");

                allGames[i].name = finalizedData[i, 0];
                allGames[i].totalGS = int.Parse(getTotal(finalizedData[i, 3]));
                allGames[i].currentGS = int.Parse(getCurrent(finalizedData[i, 3]));

                allGames[i].currentACH = int.Parse(getCurrent(finalizedData[i, 1]));

                Console.Write(".");
            }
        }

        
        static String[] allGames(String[,] finalizedData)
        {
            String[] titles = getNamesArray(finalizedData);
            return titles;
        }


        static String[] getNamesArray(String[,] finalizedData)
        {

            String[] gameTitles = new String[finalizedData.GetLength(0)];
            for (int i = 0; i < finalizedData.GetLength(0); i++)
            {
                gameTitles[i] = finalizedData[i, 0];
            }
            return gameTitles;
        }

        static String getGamePageExtension(String HTML, String[] titles, int gameNum)
        {
            String line = "";
            String gameURL = "";
            int i = 0;
            using (StringReader reader = new StringReader(HTML))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    i++;
                    if (line.Contains(titles[gameNum]) && line.Contains("smallgame"))
                    {
                        gameURL = line.Substring(31, line.Length - 31);
                        gameURL = gameURL.Substring(0, gameURL.IndexOf('>') - 1);
                    }
                }
            }
            return gameURL;
        }

        static String gamePlainText(String HTML, Game[] Games, int gameNum)
        {
            int linesInHTML = getNoRows(HTML);
            int noLines = 0;
            int countToTotal = 0;
            string line;
            Games[gameNum].totalACH = getTotalAch(HTML, Games);
            for (int i = 0; i < Games.Length; i++)
            {
                Games[i].achievementDesc = new string[Games[i].totalACH];
                Games[i].achievementGS = new int[Games[i].totalACH];
                Games[i].achievementNames = new string[Games[i].totalACH];
                Games[i].unlocked = new bool[Games[i].totalACH];
            }
            bool inTable = false;
            String editedString = "";

            using (StringReader reader = new StringReader(HTML))
            {
                int linesDown = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    string desc = "";
                    
                    bool inTag = false;
                    if (line.Contains("maincolumnpanel achievementpanel"))
                    {
                        inTable = true;
                        linesDown = 0;
                    }
                    else if (linesDown > 19)
                    {
                        inTable = false;
                    }

                    if (line.Contains("maincolumnpanel achievementpanel red"))
                    {
                        Games[gameNum].unlocked[countToTotal] = false;
                        countToTotal++;
                    }
                    else if (line.Contains("maincolumnpanel achievementpanel green"))
                    {
                        Games[gameNum].unlocked[countToTotal] = true;
                        countToTotal++;
                    }

                    if (line.Contains("mainlink"))
                    {
                        editedString += "\n\n";
                    }
                    if (line.Contains("<div class=\"subheader\">"))
                    {
                        if (line.Contains("</span> <img class=\"itemprog\""))
                        {
                            desc = line.Substring(line.IndexOf("Description"), line.Length - line.IndexOf("Description") - line.IndexOf("</span> <img class=\"itemprog\""));
                        }
                        else if (line.Contains("</span></div><div"))
                        {
                            desc = line.Substring(line.IndexOf("Description"), line.Length - line.IndexOf("Description") - line.IndexOf("</span></div><div"));
                        }

                        Games[gameNum].achievementDesc[countToTotal - 1] = desc;
                    }
                    for (int i = 0; i <= line.Length - 1; i++)
                    {
                        if (line[i] == '<')
                        {
                            inTag = true;
                        }
                        else if (line[i] == '>')
                        {
                            inTag = false;
                        }


                        if (inTag == false && line[i] != '>' && inTable && linesDown < 20)
                        {
                            editedString += line[i];
                        }

                    }
                    noLines++;
                    linesDown++;
                }
            }
            return editedString;

        }

        static int getTotalAch(string HTML, Game[] Games)
        {
            string line;
            int achs = 0;
            using (StringReader reader = new StringReader(HTML))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("maincolumnpanel achievementpanel"))
                    {
                        achs++;
                    }
                }
            }
            return achs;
        }
    }




}




