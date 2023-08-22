using System.Diagnostics;
using static System.Console;
namespace DiskpartVolumeInfo;

//Task 1
public static class FileSearch
{
    
    public static void searchForTxtFiles()
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
                 
                 
                 streamWriter.WriteLine("cd /d " + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                 //Using dir command as Find is only in Unix systems
                 streamWriter.WriteLine("dir /s /b  *.txt");
                 streamWriter.WriteLine("exit");
                 process.WaitForExit(50000);

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
        }}