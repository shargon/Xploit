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
            command.WriteLine(@"                                           /~\                            ");
            command.WriteLine(@"                                          |oo )    “I’ve got a very bad   ");
            command.WriteLine(@"                                          _\=/_     feeling about this.”  ");
            command.WriteLine(@"                          ___            /  _  \                          ");
            command.WriteLine(@"                         / ()\          //|/.\|\\                         ");
            command.WriteLine(@"                       _|_____|_       ||  \_/  ||                        ");
            command.WriteLine(@"                      | | === | |      || |\ /| ||                        ");
            command.WriteLine(@"                      |_|  O  |_|       # \_ _/ #                         ");
            command.WriteLine(@"                       ||  O  ||          | | |                           ");
            command.WriteLine(@"                       ||__*__||          | | |                           ");
            command.WriteLine(@"                      |~ \___/ ~|         []|[]                           ");
            command.WriteLine(@"                      /=\ /=\ /=\         | | |                           ");
            command.WriteLine(@"      ________________[_]_[_]_[_]________/_]_[_\_________________________ ");
        }
        public static void GetWolfBanner(ICommandLayer command)
        {
            command.SetForeColor(ConsoleColor.DarkGray);
            command.WriteLine(" ########################################################################## ");
            command.WriteLine(" #[                                               ]######################## ");
            command.WriteLine(" #[           --- Welcome to XPloit ---           ]############ /\"  ####### ");
            command.WriteLine(" #[                                               ]########  _-`\"\"\"', ##### ");
            command.WriteLine(" #[              " + DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss") + "              ]#####  _-\"       )  #### ");
            command.WriteLine(" #[                                               ]### _-\"          |  #### ");
            command.WriteLine(" ################################################## _-\"            ;  ##### ");
            command.WriteLine(" ######################################## __---___-\"              |  ###### ");
            command.WriteLine(" #####################################  _\"   ,,                  ;  `,,  ## ");
            command.WriteLine(" ################################### _-\"    ;''                 |  ,'  ; ## ");
            command.WriteLine(" #################################  _\"      '                    `\"'   ; ## ");
            command.WriteLine(" ##########################  __---;                                 ,' #### ");
            command.WriteLine(" ####################### __\"\"  ___                                ,' ###### ");
            command.WriteLine(" ################### _-\"\"   -\"\" _                               ,' ######## ");
            command.WriteLine(" ################## `-_         _                              ; ########## ");
            command.WriteLine(" ####################  \"\"----\"\"\"   ;                          ; ########### ");
            command.WriteLine(" ######################  /          ;                        ; ############ ");
            command.WriteLine(" ####################  /             ;                      ; ############# ");
            command.WriteLine(" ##################  /                `                    ; ############## ");
            command.WriteLine(" ################  /                                      ; ############### ");
        }
        public static void GetNyanCatBanner(ICommandLayer command)
        {
            //1
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 80));
            command.WriteLine(StringHelper.Replicate(" ", 80));
            command.WriteLine(StringHelper.Replicate(" ", 80));
            //2
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 30));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 24));
            //3
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 24));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 30));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 22));
            //4
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Red);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 22));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 20));
            //5
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 22));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 14));

            //6
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 22));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 12));
            //7
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.DarkYellow);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 12));
            //8
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 12));
            //9
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 22));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 12));
            //10
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Yellow);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 26));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 10));
            //11
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 10));
            //12
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Black);
            //13
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Green);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Black);
            //14
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 22));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.DarkMagenta);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            //15
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 10));
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Cyan);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Magenta);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 14));
            command.SetBackgroundColor(ConsoleColor.Black);
            //16
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 20));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 18));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            //17
            command.SetBackgroundColor(ConsoleColor.DarkCyan);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 32));

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 20));
            //18
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 8));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));

            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.DarkGray);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 20));
            //19
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6));

            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 4));
            command.SetBackgroundColor(ConsoleColor.Black);
            command.Write(StringHelper.Replicate(" ", 6));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 20));
            //20
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 16));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 62));
            //20
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.Write(StringHelper.Replicate(" ", 12));
            command.SetBackgroundColor(ConsoleColor.White);
            command.Write(StringHelper.Replicate(" ", 2));
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 66));
            //21
            command.SetBackgroundColor(ConsoleColor.DarkBlue);
            command.WriteLine(StringHelper.Replicate(" ", 80));


            command.SetBackgroundColor(ConsoleColor.Black);
        }
    }
}