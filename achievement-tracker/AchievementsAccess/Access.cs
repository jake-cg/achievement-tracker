using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AchievementsAccess
{
    class Access
    {
        static void Main(string[] args)
        {
            List<Game> Games = new List<Game>();

            //Using True Achievements
            string gamertag;
            // Console.Write("What is your gamertag?\nGamertag: ");
            gamertag = "Taken By Chip";//Console.ReadLine();

            #region Get Games Data

            String trueURL;
            String trueHTML;
            String playerID;

            #region Account Home URL
            String trueURLstart = "http://www.trueachievements.com/";
            String trueURLend = ".htm";

            if (gamertag.Contains(" "))
            {
                trueURL = trueURLstart + gamertag.Replace(" ", "+") + trueURLend;
            }
            else
            {
                trueURL = trueURLstart + gamertag + trueURLend;
            }
            #endregion

            #region Account Home HTML
            using (WebClient client = new WebClient())
            {
                trueHTML = client.DownloadString(trueURL);
            }
            #endregion

            #region Player ID

            int index = 0;
            int count = trueHTML.IndexOf("<a href=\"/gamerachievements.aspx?gamerid=") + 41;

            bool passValue = false;
            while (!passValue)
            {
                if (Char.IsDigit(trueHTML, count))
                {
                    index = count;
                    passValue = true;
                }
                count++;
            }

            bool passEnd = false;
            int indexOut = 0;
            int startingValue = index;
            while (!passEnd)
            {
                if (!Char.IsDigit(trueHTML, index))
                {
                    indexOut = index;
                    passEnd = true;
                }
                index++;
            }
            var idLength = indexOut - startingValue;

            playerID = trueHTML.Substring(startingValue, idLength);
            #endregion

            #region Games Pages HTMLs
            List<String> gamesPageURLs = new List<String>();
            List<String> gamesPageHTMLs = new List<String>();

            int page = 0, iter = 0;

            do
            {
                iter = page;
                gamesPageURLs.Add("http://www.trueachievements.com/mygamecollection.aspx?gamerid=" + playerID +
                "?executeformfunction&function=AjaxList&params=oGameCollection%7C%26ddlSortBy%3DLastunlock%26ddlDLCInc" +
                "lusionSetting%3DDLCIOwn%26sddGenre%3D%20%26sddSubGenre%3D%20%26sddGameMediaID%3D%20%26ddlStartedStatus%" +
                "3D0%26asdGamePropertyID%3D-1%26GameView%3DoptListView%26chkExcludeNotOwned%3DTrue%26MultiEditMode%3DoptS" +
                "ingleEdit%26chkColTitleimage%3DTrue%26chkColTitlename%3DTrue%26chkColPlatform%3DTrue%26chkColSiteScore%3DTr" +
                "ue%26chkColOfficialScore%3DTrue%26chkColItems%3DTrue%26chkColCompletionpercentage%3DTrue%26chkColMyrating%3DTru" +
                "e%26chkColLastunlock%3DTrue%26chkColOwnershipstatus%3DTrue%26chkColPlaystatus%3DTrue%26chkColGamenotes%3DTrue%26t" +
                "xtBaseComparisonGamerID%3D436972%26oGameCollection_Order%3DLastunlock%26oGameCollection_Page%3D" + (page + 1) + "%26oGameCollec" +
                "tion_ItemsPerPage%3D30%26oGameCollection_ResponsiveMode%3DFalse%26oGameCollection_ShowAll%3DFalse%26txtGamerID%" +
                "3D" + playerID + "%26txtGameRegionID%3D0%26txtUseAchievementsForProgress%3DTrue HTTP/1.1");

                using (WebClient client = new WebClient())
                {
                    gamesPageHTMLs.Add(client.DownloadString(gamesPageURLs[page]));
                }
                page++;
            } while (!gamesPageHTMLs[iter].Contains("warningspanel"));

            gamesPageHTMLs.Remove(gamesPageHTMLs[gamesPageHTMLs.Count - 1]);

            #endregion

            #endregion

            #region Get Achievements Data

            for (int i = 0; i < gamesPageHTMLs.Count; i++)
            {
                //Remove top links
                gamesPageHTMLs[i] = gamesPageHTMLs[i].Substring(gamesPageHTMLs[i].IndexOf("</div>") + 7);

                //Remove bottom links
                gamesPageHTMLs[i] = gamesPageHTMLs[i].Substring(0, gamesPageHTMLs[i].IndexOf("pagination") - 13);

                //Split into individual games
                List<String> gamesData = new List<String>(gamesPageHTMLs[i].Split(new String[] { "<tr" }, StringSplitOptions.None));

                //Remove header and footer
                gamesData.RemoveAt(0);
                gamesData.RemoveAt(0);
                gamesData.RemoveAt(gamesData.Count - 1);

                //Make Game
                for (int j = 0; j < gamesData.Count; j++)
                {
                    //Get game data
                    List<String> data = new List<String>(gamesData[j].Split(new String[] { "<td" }, StringSplitOptions.None));

                    //Remove header and unused fields
                    data.RemoveAt(0);
                    data.RemoveAt(0);
                    data.RemoveAt(1);
                    data.RemoveAt(1);
                    data.RemoveAt(3);
                    data.RemoveAt(3);
                    data.RemoveAt(3);
                    data.RemoveAt(3);
                    data.RemoveAt(3);

                    //Url
                    data[0] = data[0].Substring(data[0].IndexOf("<a href=") + 9);
                    String gameUrl = data[0].Substring(0, data[0].IndexOf(" title=") - 1);

                    //Title
                    data[0] = data[0].Substring(data[0].IndexOf(" title="));
                    String gameTitle = data[0].Substring(8, data[0].IndexOf("\">") - 8);


                    //Gamerscore
                    data[1] = data[1].Substring(data[1].IndexOf(">") + 1);
                    data[1] = data[1].Substring(0, data[1].IndexOf("<"));
                    String[] gs = data[1].Split('/');

                    int currentGamerscore = int.Parse(gs[0].Trim(), NumberStyles.AllowThousands);
                    int totalGamerscore = int.Parse(gs[1].Trim(), NumberStyles.AllowThousands);


                    //Achievements
                    data[2] = data[2].Substring(data[2].IndexOf(">") + 1);
                    data[2] = data[2].Substring(0, data[2].IndexOf("<"));
                    String[] ach = data[2].Split('/');

                    int currentAchievement = int.Parse(ach[0].Trim(), NumberStyles.AllowThousands);
                    int totalAchievement = int.Parse(ach[1].Trim(), NumberStyles.AllowThousands);

                    //Get game page HTML
                    String gameHTML;
                    using (WebClient client = new WebClient())
                    {
                        gameHTML = client.DownloadString(gameUrl);
                    }

                    //Trim gameHTML
                    gameHTML = gameHTML.Substring(gameHTML.IndexOf("pagetitle"));
                    gameHTML = gameHTML.Substring(0, gameHTML.IndexOf("sidebar"));

                    List<String> gameData;
                    List<DLC> dlc = new List<DLC>();
                    Game game;

                    //If the game has DLC
                    if (gamesData.Contains("block topmargin"))
                    {
                        
                        
                        gameData = new List<String>(gameHTML.Split(new String[] { "block topmargin" }, StringSplitOptions.None));
                        for (int k = 0; k < gameData.Count - 1; k++)
                        {
                            dlc.Add(new DLC(gameData[k + 1]));
                        }
                        game = new Game(gameData[0]);
                        game.dlc = dlc;
                    }
                    else
                    {
                        game = new Game(gameHTML);
                    }

                    //need to parse source in Game for ach data. Then If there is DLC, parse that too. Note: Blops 3 did NOT show DLC. Bug. 
                    int o = 0;

                    
                    //Add fields to game
                    game.url = gameUrl;
                    game.title = gameTitle;
                    game.currentGS = currentGamerscore;
                    game.totalGS = totalGamerscore;
                    game.currentAch = currentAchievement;
                    game.dlc = dlc;

                    Games.Add(game);
                }

            }

            #endregion

            for (int i = 0; i < Games.Count; i++)
            {

                Console.WriteLine((i + 1) + ": " + Games[i].title);
            }

            Console.ReadLine();

        }
    }
}
