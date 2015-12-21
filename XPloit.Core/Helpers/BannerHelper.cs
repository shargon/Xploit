using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Helpers
{
    public class BannerHelper
    {
        /// <summary>
        /// Get a Random banner
        /// </summary>
        /// <param name="command">Command</param>
        public static void GetRandomBanner(ICommandLayer command)
        {
            Random r = new Random();

            switch (r.Next(3))
            {
                case 0: GetStarWarsBanner(command); break;
                case 1: GetWolfBanner(command); break;
                default: GetNyanCatBanner(command); break;
            }
        }

        public static void GetStarWarsBanner(ICommandLayer command)
        {
            command.SetForeColor(ConsoleColor.DarkGray);
            command.Write(@"                                           /~\                            ", true);
            command.Write(@"                                          |oo )    “I’ve got a very bad   ", true);
            command.Write(@"                                          _\=/_     feeling about this.”  ", true);
            command.Write(@"                          ___            /  _  \                          ", true);
            command.Write(@"                         / ()\          //|/.\|\\                         ", true);
            command.Write(@"                       _|_____|_       ||  \_/  ||                        ", true);
            command.Write(@"                      | | === | |      || |\ /| ||                        ", true);
            command.Write(@"                      |_|  O  |_|       # \_ _/ #                         ", true);
            command.Write(@"                       ||  O  ||          | | |                           ", true);
            command.Write(@"                       ||__*__||          | | |                           ", true);
            command.Write(@"                      |~ \___/ ~|         []|[]                           ", true);
            command.Write(@"                      /=\ /=\ /=\         | | |                           ", true);
            command.Write(@"      ________________[_]_[_]_[_]________/_]_[_\_________________________ ", true);
        }
        public static void GetWolfBanner(ICommandLayer command)
        {
            command.SetForeColor(ConsoleColor.DarkGray);
            command.Write(" ########################################################################## ", true);
            command.Write(" #[                                               ]######################## ", true);
            command.Write(" #[           --- Welcome to XPloit ---           ]############ /\"  ####### ", true);
            command.Write(" #[                                               ]########  _-`\"\"\"', ##### ", true);
            command.Write(" #[              " + DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss") + "              ]#####  _-\"       )  #### ", true);
            command.Write(" #[                                               ]### _-\"          |  #### ", true);
            command.Write(" ################################################## _-\"            ;  ##### ", true);
            command.Write(" ######################################## __---___-\"              |  ###### ", true);
            command.Write(" #####################################  _\"   ,,                  ;  `,,  ## ", true);
            command.Write(" ################################### _-\"    ;''                 |  ,'  ; ## ", true);
            command.Write(" #################################  _\"      '                    `\"'   ; ## ", true);
            command.Write(" ##########################  __---;                                 ,' #### ", true);
            command.Write(" ####################### __\"\"  ___                                ,' ###### ", true);
            command.Write(" ################### _-\"\"   -\"\" _                               ,' ######## ", true);
            command.Write(" ################## `-_         _                              ; ########## ", true);
            command.Write(" ####################  \"\"----\"\"\"   ;                          ; ########### ", true);
            command.Write(" ######################  /          ;                        ; ############ ", true);
            command.Write(" ####################  /             ;                      ; ############# ", true);
            command.Write(" ##################  /                `                    ; ############## ", true);
            command.Write(" ################  /                                      ; ############### ", true);
        }
        public static void GetNyanCatBanner(ICommandLayer command)
        {
            //1
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 80), true);
            command.Write(StringHelper.Replicate(" ", 80), true);
            command.Write(StringHelper.Replicate(" ", 80), true);
            //2
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 30), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 24), true);
            //3
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 24), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 30), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 22), true);
            //4
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 22), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 20), true);
            //5
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 22), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ",4), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 14), true);

            //6
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 22), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false); 
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
           
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), true);
            //7
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), true);
            //8
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), true);
            //9
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 22), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), true);
            //10
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 26), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 10), true);
            //11
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 10), true);
            //12
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 10), true);
            command.SetBackgroundColor(ConsoleColor.Black);
            //13
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 10), true);
            command.SetBackgroundColor(ConsoleColor.Black);
            //14
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 22), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), true);
            command.SetBackgroundColor(ConsoleColor.Black);
            //15
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 10), false);
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 14), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 14), true);
            command.SetBackgroundColor(ConsoleColor.Black);
            //16
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 20), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 18), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16), true);
            command.SetBackgroundColor(ConsoleColor.Black);
            //17
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 32), false);

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 20), true);
            //18
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 8), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ",2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 20), true);
            //19
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6), false);

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 4), false);
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 20), true);
            //20
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 62), true);
            //20
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12), false);
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2), false);
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 66), true);
            //21
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 80), true);
          

            command.SetBackgroundColor(ConsoleColor.Black);
        }
    }
}