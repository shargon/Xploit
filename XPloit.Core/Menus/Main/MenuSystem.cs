namespace XPloit.Core.Menus.Main
{
    public class MenuSystem : Menu
    {
        public override string Name { get { return "System"; } }
        public override string[] AllowedNames { get { return new string[] { "System", "Sys" }; } }
        public override string Manual { get { return "Execute local system command line"; } }
    }
}