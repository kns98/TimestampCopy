using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TimestampCopy;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

class Program
{
    static Dictionary<FileEntry,string> sourceEntries = new Dictionary<FileEntry, string>();
    static Dictionary<FileEntry,string> destinationEntries = new Dictionary<FileEntry, string>();

    static readonly string filePath = "file_entries.txt";
    static FileEntryDAL fileEntryDAL = new FileEntryDAL(filePath);
    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Ctrl+C pressed. Handling the Ctrl+C event...");

        // You can add your own code here to perform cleanup or take specific actions.
        // For example, you can gracefully shut down your application.

        // To terminate the program, set the Cancel property of the event args to true.
        // If you don't set it to true, the program will continue running after handling Ctrl+C.
        fileEntryDAL.SerializeToFile(sourceEntries);


    }

    static void Main(string[] args)
    {        // Subscribe to the ProcessExit event
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

        // Specify the source and destination directories
        string sourceDirectory = @"e:\OneDrive\Pictures";
        string destinationDirectory = @"d:\OneDrive\pictures";


        string datFile = "file_entries.txt";

        if (File.Exists(datFile))
        {
            sourceEntries = fileEntryDAL.DeserializeFromFile();
        }


        // Compute and store FileEntry objects for source and destination directories
        ComputeFileEntries(sourceDirectory, sourceEntries);
        ComputeFileEntries(destinationDirectory, destinationEntries);

        // Compare the hashsets and copy timestamps
        foreach (var sourceEntry in sourceEntries.Keys)
        {
            if (destinationEntries.Values.Contains(sourceEntries[sourceEntry]))
            {
                string sourceFilePath = Path.Combine(sourceDirectory, sourceEntry.FileName);
                string destinationFilePath = Path.Combine(destinationDirectory, sourceEntry.FileName);

                Console.WriteLine("Setting timestamp from " + sourceFilePath + " to " + destinationFilePath);
                // Copy timestamp from source to destination
                File.SetLastWriteTime(destinationFilePath, File.GetLastWriteTime(sourceFilePath));
            }
            else
            {
                Console.WriteLine($"File {sourceEntry.FileName} has different sizes or hashes in source and destination directories.");
            }
        }
        fileEntryDAL.SerializeToFile(sourceEntries);
        Console.WriteLine("Timestamps copied successfully.");
    }
    static void OnProcessExit(object sender, EventArgs e)
    {
        // This method will be called just before the application exits.
        // You can perform cleanup or any necessary actions here.
        Console.WriteLine("Exiting the application. Performing cleanup...");

        fileEntryDAL.SerializeToFile(sourceEntries);

    }

    // Recursively compute and store FileEntry objects for files in a directory
    static void ComputeFileEntries(string directory, Dictionary<FileEntry, string> entries)
    {
        if (Directory.Exists(directory))
        {
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                var check = new FileEntry(file.Length, file);

                if (!entries.ContainsKey(check))
                {
                    try
                    {
                        using (FileStream stream = File.OpenRead(file))
                        {
                            Console.WriteLine("Hashing " + file + " ... ");
                            using (MD5 md5 = MD5.Create())
                            {
                                byte[] hashBytes = md5.ComputeHash(stream);
                                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                long length = new FileInfo(file).Length;

                                // Create and store a FileEntry object
                                var fileEntry = new FileEntry(length, file);
                                entries.Add(fileEntry,hash);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            // Recursively process subdirectories
            foreach (string subDir in Directory.GetDirectories(directory))
            {
                ComputeFileEntries(subDir, entries);
            }
        }
    }
}

// Class to represent a file entry with MD5 hash, length, and file name
class FileEntry
{
    public long Length { get; }
    public string FileName { get; }

    public FileEntry(long length, string fileName)
    {
        Length = length;
        FileName = fileName;
    }
}


