using Steamworks;

namespace HackNetSteamCloudRemover
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SteamClient.Init(365450);
                bool exit = false;
                while (!exit)
                {
                    PrintMenu();
                    string input = Console.ReadLine();
                    if (GetInt(input,4,out int? output,1))
                    {
                        Mode operationMode = Enum.Parse<Mode>(output.Value.ToString());
                        switch (operationMode)
                        {
                            case Mode.Backup:
                            {
                                BackupMode();
                                break;
                            }
                            case Mode.Delete:
                            {
                                DeleteMode();
                                break;
                            }
                            case Mode.Restore:
                            {
                                RestoreMode();
                                break;
                            }
                            case Mode.Quit:
                            {
                                exit = true;
                                break;
                            }
                            default:
                            {
                                Console.WriteLine("Invalid option");
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong input");
                    }


                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static void RestoreMode()
        {
            if (!Directory.Exists("backup"))
            {
                Console.WriteLine("Backups directory not found");
                return;
            }

            var folderList = Directory.EnumerateDirectories("backup").ToList();
            if (folderList.Count == 0)
            {
                Console.WriteLine("No backups found");
                return;
            }
            var folderNames = new List<string>();
            for (var index = 0; index < folderList.Count; index++)
            {
                var folder = folderList[index];
                folder = Path.GetRelativePath("backup",folder);
                folderNames.Add(folder);

            }

           
            while (true)
            {
                Console.WriteLine("0: back");
                DisplayFiles(folderNames);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (GetInt(input, folderList.Count, out int? output))
                    {
                        if (output.Value == 0)
                        {
                            break;
                        }
                        RestoreBackup(folderList[output.Value - 1]);
                    }
                    else
                    {
                        Console.WriteLine("Wrong input");
                    }
                }
                else
                {
                    Console.WriteLine("Wrong input");
                } 
            }

        }

        private static void RestoreBackup(string folder)
        {
            BackupMode();
            Console.WriteLine(folder);
            var contentsList = Directory.EnumerateFileSystemEntries(folder);
            foreach (var contents in contentsList)
            {
                if (File.Exists(contents))
                {
                    ProcessFile(folder, contents);
                }
                else
                {
                    ProcessDirectory(folder,contents);
                }
            }
        }

        private static void ProcessDirectory(string backupRoot,string path)
        {
            var contentsList = Directory.EnumerateFileSystemEntries(path);
            foreach (var contents in contentsList)
            {
                if (File.Exists(contents))
                {
                    ProcessFile(backupRoot, contents);
                }
                else
                {
                    ProcessDirectory(backupRoot, contents);
                }
            }
        }

        private static void ProcessFile(string backupRoot, string originalPath)
        {
            var content = File.ReadAllBytes(originalPath);
            var path = Path.GetRelativePath(backupRoot, originalPath);
            Console.WriteLine(path);
            SteamRemoteStorage.FileWrite(path, content);
        }

        private static void PrintMenu()
        {
            Console.WriteLine("Choose mode:");
            Console.WriteLine("1: Backup");
            Console.WriteLine("2: Restore backup (backups and then deletes current files)");
            Console.WriteLine("3: Delete files (creates backup)");
            Console.WriteLine("4: Quit");
        }

        private static void DeleteMode()
        {
            if (SteamRemoteStorage.FileCount == 0)
            {
                Console.WriteLine("No files to delete");
                return;
            }
            BackupMode();
            DeleteAllFiles(SteamRemoteStorage.Files.ToList());
            Directory.Delete( Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/My Games/Hacknet", true);
            Console.WriteLine("Files deleted");
        }

        private static void DeleteAllFiles(List<string> fileList)
        {
            foreach (var file in fileList)
            {
                SteamRemoteStorage.FileDelete(file);
            }
        }

        private static bool GetInt(string input, int max, out int? output, int min = 0)
        {
            
            output = null;
            if (int.TryParse(input, out int result))
            {
                if (result > max)
                {
                    return false;
                }

                if (result < min)
                {
                    return false;
                }
            }
            
            output = result;
            return true;

        }

        private static void DisplayFiles(List<string> files, int diffrence = 1)
        {
            for (int i = 0; i < files.Count(); i++)
            {
                Console.WriteLine($"{i+diffrence}: {files[i]}");
            }
        }

        private static void BackupMode()
        {
            Console.WriteLine("Creating backup of cloud storage");
            if (!Directory.Exists("backup"))
            {
                Directory.CreateDirectory("backup");
            }

            string datePart = DateTime.Now.ToString("dd-MM-yyyy HH mm ss");
            foreach (var file in SteamRemoteStorage.Files)
            {
                Console.WriteLine("Backing file: " + file);
                var fileContent = SteamRemoteStorage.FileRead(file);
                if (fileContent != null)
                {
                    var savePath = Path.Combine("backup", datePart, file);
                    var pathDir = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(pathDir))
                    {
                        Directory.CreateDirectory(pathDir);
                    }

                    File.WriteAllBytes(savePath, fileContent);
                }
            }
        }

    }
}