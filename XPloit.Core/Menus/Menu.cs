using System.Text;
namespace XPloit.Core.Menus
{
    public class Menu
    {
        Menu[] _Childs;
        /// <summary>
        /// Padre
        /// </summary>
        public Menu Parent { get; private set; }
        /// <summary>
        /// Childs
        /// </summary>
        public Menu[] Childs { get { return _Childs; } }
        /// <summary>
        /// Name
        /// </summary>
        public virtual string Name { get { return "?"; } }
        /// <summary>
        /// Allowed names
        /// </summary>
        public virtual string[] AllowedNames { get { return new string[] { Name }; } }
        /// <summary>
        /// Manual
        /// </summary>
        public virtual string Manual { get { return "?"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public Menu()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Menu(params Menu[] childs)
        {
            if (childs != null)
            {
                _Childs = childs;

                foreach (Menu m in _Childs)
                    m.Parent = this;
            }
            else
            {
                _Childs = null;
            }
        }
        /// <summary>
        /// Search menu by name
        /// </summary>
        /// <param name="menus">Menus</param>
        /// <param name="read">search</param>
        public static Menu SearchByName(Menu[] menus, string search)
        {
            if (menus == null) return null;

            search = search.ToLowerInvariant();
            foreach (Menu m in menus)
            {
                foreach (string name in m.AllowedNames)
                    if (name.ToLowerInvariant() == search) return m;
            }
            return null;
        }
        /// <summary>
        /// Get parent path
        /// </summary>
        /// <param name="separator">Separator</param>
        public string GetPath(string separator)
        {
            string sb = "";

            Menu m = this;

            while (m != null)
            {
                sb = m.Name + (sb != "" ? separator : "") + sb;
                m = this.Parent;
            }

            return sb;
        }
    }
}