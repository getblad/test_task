using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Console;

namespace DiskpartVolumeInfo
{
    class Program
    {
        private static void FileSearch()
        {
             try
             {
                 var startInfo = new ProcessStartInfo
                 {
                     FileName = "cmd.exe",
                     RedirectStandardInput = true,
                     RedirectStandardOutput = true,
                     RedirectStandardError = true,
                     CreateNoWindow = true,
                     UseShellExecute = false
                 };

                 var process = new Process
                 {
                     StartInfo = startInfo
                 };

                 process.Start();
                 var streamWriter = process.StandardInput;
                 var streamReader = process.StandardOutput;
                 var streamError = process.StandardError;
                 
                 
                 streamWriter.WriteLine("cd /d" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                 //Using dir command as Find is only in Unix systems
                 streamWriter.WriteLine("dir /s /b /a-d \"*.txt\"");
                 streamWriter.WriteLine("exit");
                 process.WaitForExit(150000);

                 var output = streamReader.ReadToEnd();
                 var error = streamError.ReadToEnd();
                 if (!string.IsNullOrWhiteSpace(error))
                 {
                     WriteLine("Error output from cmd:");
                     WriteLine(error);
                     return;
                 }

                 var filePaths = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                 var fileCount = 0;
                 long totalSizeBytes = 0;
                 var smallestSizeBytes = long.MaxValue;
                 long largestSizeBytes = 0;
                 var smallestFileName = "";
                 var largestFileName = "";

                 foreach (var filePath in filePaths)
                 {
                     if (!filePath.EndsWith(".txt") || filePath.EndsWith("*.txt")) continue;
                     var fileSizeBytes = new FileInfo(filePath).Length;
                     totalSizeBytes += fileSizeBytes;
                     fileCount++;

                     if (fileSizeBytes < smallestSizeBytes)
                     {
                         smallestSizeBytes = fileSizeBytes;
                         smallestFileName = Path.GetFileName(filePath);
                     }

                     if (fileSizeBytes > largestSizeBytes)
                     {
                         largestSizeBytes = fileSizeBytes;
                         largestFileName = Path.GetFileName(filePath);
                     }
                 }

                 WriteLine($"Number of text files: {fileCount}");
                 WriteLine($"Total size of text files: { totalSizeBytes } B");
                 WriteLine($"Smallest file: {smallestFileName}, Size: {smallestSizeBytes } B");
                 WriteLine($"Largest file: {largestFileName}, Size: {largestSizeBytes } B");
                
             }
             catch (Exception ex)
             {
                 WriteLine("An error occurred: " + ex.Message);
             }
        }
        private static void EvaluateMaxAndMinVolume()
        {
            const string outputFilePath = "diskpart_output.txt";
            try
            {
                RunDiskpartAndSaveOutput(outputFilePath);
                var volumeInfo = new List<(string, long)>();
                var lines = File.ReadAllLines(outputFilePath);
                File.Delete(outputFilePath);
                foreach (var line in lines)
                {
                    if (!line.StartsWith("  Volume")) continue;
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
                    var driveLetter = parts.Skip(2).FirstOrDefault(part => part.Length == 1);
                    if (string.IsNullOrWhiteSpace(driveLetter))
                    {
                        driveLetter = parts.Skip(2).FirstOrDefault()?.Trim();
                        if (driveLetter is "" or " ") driveLetter = "No letter/label";
            
                    }
                    var sizeStr = Regex.Match(line, @"\b\d+(\.\d+)?\s*(B|MB|GB|TB)\b").Value;
            
                    if (string.IsNullOrWhiteSpace(sizeStr)) continue;
                    var sizeValue = double.Parse(Regex.Match(sizeStr, @"\d+(\.\d+)?").Value);
                    var sizeUnit = Regex.Match(sizeStr, @"(B|MB|GB|TB)").Value;
                    var volumeSizeBytes = SizeToBytes(sizeValue, sizeUnit);
                    if(volumeSizeBytes == 0) continue;
                    volumeInfo.Add((driveLetter, volumeSizeBytes)!);
                } 
            
                var largestVolume = volumeInfo.OrderByDescending(item => item.Item2).FirstOrDefault();
                var smallestVolume = volumeInfo.OrderBy(item => item.Item2).FirstOrDefault();
            
                WriteLine(
                    $"Largest volume: Letter: {largestVolume.Item1}, Size: {largestVolume.Item2 / (1024.0 * 1024 * 1024):F2} GiB");
                WriteLine(
                    $"Smallest volume: Letter: {smallestVolume.Item1}, Size: {smallestVolume.Item2 / (1024.0 * 1024 * 1024):F2} GiB");
            }
            catch
            {
                WriteLine("Error occured");
            }
        }
        private static void RunDiskpartAndSaveOutput(string outputFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            var process = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                process.Start();
                var streamWriter = process.StandardInput;
                var streamReader = process.StandardOutput;
                var streamError = process.StandardError;
                streamWriter.WriteLine("list volume");
                streamWriter.WriteLine("exit");
                process.WaitForExit();
                var output = streamReader.ReadToEnd();
                var error = streamError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    WriteLine("Error output from cmd:");
                    WriteLine(error);
                    return;
                }
                File.WriteAllText(outputFilePath, output);

            }
            catch (Exception ex)
            {
                WriteLine($"Exception happened: {ex.Message}");
                throw new Exception();
            }
        
        }

        static void Main(string[] args)
        {
            EvaluateMaxAndMinVolume();
            FileSearch();
        }

        private static long SizeToBytes(double value, string? unit)
        {
            return unit switch
            {
                "B" => (long)value,
                "MB" => (long)(value * 1024 * 1024),
                "GB" => (long)(value * 1024 * 1024 * 1024),
                "TB" => (long)(value * 1024 * 1024 * 1024 * 1024),
                _ => 0
            };
        }
        private static long BytesSizeTo(double value, string? unit)
                 {
                     return unit switch
                     {
                         "B" => (long)value,
                         "MB" => (long)(value / (1024 * 1024)),
                         "GB" => (long)(value / ( 1024 * 1024 * 1024)),
                         "TB" => (long)(value / 1024 /1024 / 1024 / 1024),
                         _ => 0
                     };
                 }
    }
    
}
