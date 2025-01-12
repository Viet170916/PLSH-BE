using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog;

namespace Common.Library
{
    [ExcludeFromCodeCoverage]
    public static class FileExtension
    {
        public static bool RemoveFile(string source)
        {
            var result = false;
            try
            {
                File.Delete(source);
                result = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
            return result;
        }
    }
}
