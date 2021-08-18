using System;
using System.IO;

namespace Movington.PhotoTransfer
{
    public static class AppConstants
    {
        public static string FilesFolderPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            ".movington");
    }
}