using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace LFNet.Common.IO
{
    public static class PermissionHelper
    {
        public static bool CanModify(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            if (!HasWritePermission(filePath))
            {
                return false;
            }
            FileInfo info = new FileInfo(filePath);
            if (info.Exists)
            {
                return !info.IsReadOnly;
            }
            DirectoryInfo info2 = new DirectoryInfo(filePath);
            if (!info2.Exists)
            {
                return false;
            }
            return true;
        }

        public static bool CanRead(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            if (!HasReadPermission(filePath))
            {
                return false;
            }
            DirectoryInfo info = new DirectoryInfo(filePath);
            FileInfo info2 = new FileInfo(filePath);
            if (!info.Exists && !info2.Exists)
            {
                return false;
            }
            return true;
        }

        public static FileSystemSecurity GetFileSystemSecurity(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.GetAccessControl(filePath);
            }
            if (Directory.Exists(filePath))
            {
                return Directory.GetAccessControl(filePath);
            }
            return null;
        }

        public static bool HasPermission(string filePath, FileSystemRights rights)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                try
                {
                    FileSystemSecurity fileSystemSecurity = GetFileSystemSecurity(filePath);
                    if (fileSystemSecurity == null)
                    {
                        return false;
                    }
                    WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    foreach (FileSystemAccessRule rule in fileSystemSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                    {
                        if ((rule.FileSystemRights & rights) != 0)
                        {
                            if (rule.IdentityReference.Value.StartsWith("S-1-"))
                            {
                                SecurityIdentifier sid = new SecurityIdentifier(rule.IdentityReference.Value);
                                if (!principal.IsInRole(sid))
                                {
                                    continue;
                                }
                            }
                            if (principal.IsInRole(rule.IdentityReference.Value))
                            {
                                if (rule.AccessControlType == AccessControlType.Deny)
                                {
                                    return false;
                                }
                                if (rule.AccessControlType == AccessControlType.Allow)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool HasReadPermission(string filePath)
        {
            return HasPermission(filePath, FileSystemRights.Read);
        }

        public static bool HasWritePermission(string filePath)
        {
            return HasPermission(filePath, FileSystemRights.Write);
        }

        public static bool SetPermission(string filePath, FileSystemRights rights)
        {
            bool flag;
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            try
            {
                FileSystemAccessRule rule = new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, AccessControlType.Allow);
                FileSystemSecurity fileSystemSecurity = GetFileSystemSecurity(filePath);
                if (fileSystemSecurity != null)
                {
                    fileSystemSecurity.AddAccessRule(rule);
                    if (File.Exists(filePath))
                    {
                        File.SetAccessControl(filePath, fileSystemSecurity as FileSecurity);
                        goto Label_006A;
                    }
                    if (Directory.Exists(filePath))
                    {
                        Directory.SetAccessControl(filePath, fileSystemSecurity as DirectorySecurity);
                        goto Label_006A;
                    }
                }
                return false;
            Label_006A:
                flag = true;
            }
            catch (UnauthorizedAccessException)
            {
                flag = false;
            }
            return flag;
        }

        public static bool SetReadPermission(string filePath)
        {
            return SetPermission(filePath, FileSystemRights.Read);
        }

        public static bool SetWritePermission(string filePath)
        {
            if (!SetPermission(filePath, FileSystemRights.Write))
            {
                return false;
            }
            FileInfo info = new FileInfo(filePath);
            if (!info.Exists)
            {
                return true;
            }
            if (info.IsReadOnly)
            {
                info.IsReadOnly = false;
                info.Refresh();
            }
            return !info.IsReadOnly;
        }
    }
}

