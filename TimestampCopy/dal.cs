using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimestampCopy
{


class FileEntryDAL
    {
        private string filePath;

        public FileEntryDAL(string filePath)
        {
            this.filePath = filePath;
        }

        public void SerializeToFile(HashSet<FileEntry> sourceEntries)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var entry in sourceEntries)
                    {
                        writer.WriteLine($"{entry.MD5Hash},{entry.Length},{entry.FileName}");
                    }
                }
                Console.WriteLine("Serialization successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while serializing: {ex.Message}");
            }
        }

        public HashSet<FileEntry> DeserializeFromFile()
        {
            var fileEntries = new HashSet<FileEntry>(new FileEntryComparer());

            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 3)
                            {
                                var md5Hash = parts[0];
                                var length = long.Parse(parts[1]);
                                var fileName = parts[2];
                                fileEntries.Add(new FileEntry(md5Hash, length, fileName));
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File not found for deserialization.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deserializing: {ex.Message}");
            }

            return fileEntries;
        }

        public HashSet<FileEntry> DeserializeFromText(string text)
        {
            var fileEntries = new HashSet<FileEntry>(new FileEntryComparer());

            try
            {
                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var md5Hash = parts[0];
                        var length = long.Parse(parts[1]);
                        var fileName = parts[2];
                        fileEntries.Add(new FileEntry(md5Hash, length, fileName));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deserializing from text: {ex.Message}");
            }

            return fileEntries;
        }
    }


}

