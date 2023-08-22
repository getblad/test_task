using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Console;

namespace DiskpartVolumeInfo
{
    class Program
    {


        static void Main(string[] args)
        {
            EvaluateVolumes.maxAndMinVolume();
            FileSearch.searchForTxtFiles();

        }

    }
}
