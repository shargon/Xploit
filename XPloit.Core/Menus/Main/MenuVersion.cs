namespace XPloit.Core.Menus.Main
{
    public class MenuVersion : Menu
    {
        public override string Name { get { return "Version"; } }
        public override string[] AllowedNames { get { return new string[] { "Version", "Ver" }; } }
        public override string Manual { get { return "Show version for XPloit"; } }
    }
}