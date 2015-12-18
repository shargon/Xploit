using XPloit.Core.Helpers;

namespace XPloit.Core.Multi
{
    public class User
    {
        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public Password Password { get; set; }

        /// <summary>
        /// Return true if UserName is email account
        /// </summary>
        public bool IsEmail { get { return StringHelper.IsValidEmail(UserName); } }

        public override string ToString()
        {
            return UserName;
        }
    }
}