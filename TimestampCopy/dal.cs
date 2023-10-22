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

        public void SerializeToFile(Dictionary<FileEntry, string> sourceEntries)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var entry in sourceEntries.Keys)
                    {
                        writer.WriteLine($"{sourceEntries[entry]},{entry.Length},{entry.FileName}");
                    }
                }
                Console.WriteLine("Serialization successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while serializing: {ex.Message}");
            }
        }

        public Dictionary<FileEntry, string> DeserializeFromFile()
        {
            var fileEntries = new Dictionary<FileEntry, string>();

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
                                fileEntries.Add(new FileEntry(length, fileName),md5Hash);
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

        public Dictionary<FileEntry, string> DeserializeFromText(string text)
        {
            var fileEntries = new Dictionary<FileEntry, string>();

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
                        fileEntries.Add(new FileEntry(length, fileName),md5Hash);
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

