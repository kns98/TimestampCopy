﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        // Specify the source and destination directories
        string sourceDirectory = @"e:\OneDrive\Pictures";
        string destinationDirectory = @"d:\OneDrive\pictures";

        // Create hashsets to store FileEntry objects
        HashSet<FileEntry> sourceEntries = new HashSet<FileEntry>(new FileEntryComparer());
        HashSet<FileEntry> destinationEntries = new HashSet<FileEntry>(new FileEntryComparer());

        // Compute and store FileEntry objects for source and destination directories
        ComputeFileEntries(sourceDirectory, sourceEntries);
        ComputeFileEntries(destinationDirectory, destinationEntries);

        // Compare the hashsets and copy timestamps
        foreach (var sourceEntry in sourceEntries)
        {
            if (destinationEntries.Contains(sourceEntry))
            {
                string sourceFilePath = Path.Combine(sourceDirectory, sourceEntry.FileName);
                string destinationFilePath = Path.Combine(destinationDirectory, sourceEntry.FileName);

                // Copy timestamp from source to destination
                File.SetLastWriteTime(destinationFilePath, File.GetLastWriteTime(sourceFilePath));
            }
            else
            {
                Console.WriteLine($"File {sourceEntry.FileName} has different sizes or hashes in source and destination directories.");
            }
        }

        Console.WriteLine("Timestamps copied successfully.");
    }

    // Recursively compute and store FileEntry objects for files in a directory
    static void ComputeFileEntries(string directory, HashSet<FileEntry> entries)
    {
        if (Directory.Exists(directory))
        {
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        long length = new FileInfo(file).Length;

                        // Create and store a FileEntry object
                        var fileEntry = new FileEntry(hash, length, file.Substring(directory.Length));
                        entries.Add(fileEntry);
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
    public string MD5Hash { get; }
    public long Length { get; }
    public string FileName { get; }

    public FileEntry(string md5Hash, long length, string fileName)
    {
        MD5Hash = md5Hash;
        Length = length;
        FileName = fileName;
    }
}

// Custom comparer for FileEntry objects
class FileEntryComparer : IEqualityComparer<FileEntry>
{
    public bool Equals(FileEntry x, FileEntry y)
    {
        return x.MD5Hash == y.MD5Hash && x.Length == y.Length;
    }

    public int GetHashCode(FileEntry obj)
    {
        return (obj.MD5Hash + obj.Length.ToString()).GetHashCode();
    }
}
