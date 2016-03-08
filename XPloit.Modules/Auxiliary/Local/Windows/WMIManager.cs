using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace Auxiliary.Local.Windows
{
    [OnlyFor(Windows = true)]
    public class WMIManager : Module
    {
        public interface IWMIPayload
        {
            bool Run(ManagementScope connection);

            string GetWmicparams();
            bool ProcessOutPut(string output);
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "WMI call"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/windows/desktop/aa394554(v=vs.85).aspx") ,
                    new Reference(EReferenceType.URL,"https://www.aldeid.com/wiki/Wmic-linux")
                };
            }
        }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(IWMIPayload)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "The server and namespace for the ManagementScope")]
        public string WMIPath { get; set; }
        [ConfigurableProperty(Description = "Describes the impersonation level to be used to connect to WMI")]
        public ImpersonationLevel ImpersonationLevel { get; set; }

        #region Optional
        [ConfigurableProperty(Optional = true, Description = "Sets the user name to be used for the connection operation")]
        public string UserName { get; set; }
        [ConfigurableProperty(Optional = true, Description = "Sets the password for the specified user")]
        public string Password { get; set; }
        #endregion
        #endregion

        public WMIManager()
        {
            string computerName = ".";

            WMIPath = String.Format(@"\\{0}\ROOT\CIMV2", computerName);
            UserName = "";
            Password = "";
            ImpersonationLevel = ImpersonationLevel.Impersonate;
        }

        ManagementScope Connect()
        {
            ConnectionOptions connOptions = new ConnectionOptions();
            connOptions.Impersonation = this.ImpersonationLevel;

            if (!string.IsNullOrEmpty(UserName))
            {
                connOptions.Username = UserName;
                connOptions.Password = Password;
            }

            connOptions.EnablePrivileges = true;
            ManagementScope manScope = new ManagementScope(WMIPath, connOptions);

            manScope.Connect();
            return manScope;
        }

        public override ECheck Check()
        {
            ManagementScope cnx = Connect();
            return cnx.IsConnected ? ECheck.Ok : ECheck.Error;
        }

        string MakeArgs()
        {
            // LINUX https://www.aldeid.com/wiki/Wmic-linux

            // WINDOWS
            /*
            [modificadores globales] <comando>

            Los siguientes modificadores globales están disponibles:
            /NAMESPACE           Ruta del espacio de nombres contra el que opera el alias.
            /ROLE                Ruta para el rol que contiene las definiciones de alias.
            /NODE                Servidores contra los que operará el alias.
            /IMPLEVEL            Nivel de imitación de cliente.
            /AUTHLEVEL           Nivel de autenticación de cliente.
            /LOCALE              Id. de idioma que debe usar el cliente.
            /PRIVILEGES          Habilita o deshabilita todos los privilegios.
            /TRACE               Información de depuración de salidas a stderr.
            /RECORD              Registra todos los comandos de entrada y las salidas.
            /INTERACTIVE         Establece o restablece el modo interactivo.
            /FAILFAST            Establece o vuelve a establecer el modo FailFast.
            /USER                Usuario para utilizar durante la sesión.
            /PASSWORD            Contraseña para utilizar durante la sesión.
            /OUTPUT              Especifica el modo para la redirección de salida.
            /APPEND              Especifica el modo para la redirección de salida.
            /AGGREGATE           Establece o restablece el modo de agregación.
            /AUTHORITY           Especifica el <tipo de autoridad> de la conexión.
            /?[:<BRIEF|FULL>]    Información de uso.

            Para obtener más información sobre un modificador global específico, escriba:
            nombre_modificador /?

            Los alias siguientes no están disponibles en el rol actual:
            ALIAS                    - Acceso a los alias disponibles en el sistema local
            BASEBOARD                - Administración de la placa base (también conocida como tarjeta madre o tarjeta del sistema).
            BIOS                     - Administración de servicios básicos de entrada/salida (BIOS).
            BOOTCONFIG               - Administración de la configuración de arranque.
            CDROM                    - Administración de CD-ROM.
            COMPUTERSYSTEM           - Administración de sistemas.
            CPU                      - Administración de CPU.
            CSPRODUCT                - Información del producto del sistema desde SMBIOS.
            DATAFILE                 - Administración de DataFile.
            DCOMAPP                  - Administración de aplicación DCOM.
            DESKTOP                  - Administración de escritorio de usuario.
            */
            string ret = "";

            if (!string.IsNullOrEmpty(UserName)) ret += "/user:\"" + UserName + "\"";
            if (!string.IsNullOrEmpty(Password)) ret += " /password:\"" + Password + "\"";
            if (!string.IsNullOrEmpty(WMIPath)) ret += " \"/node:" + WMIPath + "\"";

            return ret + " /PRIVILEGES";
        }
        public override bool Run()
        {
            IWMIPayload check = (IWMIPayload)this.Payload;
            /*
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.RedirectStandardError = true;
            pi.RedirectStandardOutput = true;
            pi.FileName = "wmic";
            pi.UseShellExecute = false;
            pi.Arguments = (MakeArgs() + " " + check.GetWmicparams()).Trim();

            Process pr = Process.Start(pi);

            pr.WaitForExit();

            string read = pr.StandardError.ReadToEnd().Trim();
            if (!string.IsNullOrEmpty(read)) WriteError(read);
            read = pr.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(read))
            {
                if (check.ProcessOutPut(read))
                    return true;
                WriteError(read);
            }

            return false;
            */
            return check.Run(Connect());
        }
    }
}