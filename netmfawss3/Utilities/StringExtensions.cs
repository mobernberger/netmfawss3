using System.Text;

namespace netmfawss3.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replace all occurances of the 'find' string with the 'replace' string.
        /// </summary>
        /// <param name="content">Original string to operate on</param>
        /// <param name="find">String to find within the original string</param>
        /// <param name="replace">String to be used in place of the find string</param>
        /// <returns>Final string after all instances have been replaced.</returns>
        public static string Replace(this string content, string find, string replace)
        {
            const int startFrom = 0;
            int findItemLength = find.Length;

            int firstFound = content.IndexOf(find, startFrom);
            var returning = new StringBuilder();

            string workingString = content;

            while ((firstFound = workingString.IndexOf(find, startFrom)) >= 0)
            {
                returning.Append(workingString.Substring(0, firstFound));
                returning.Append(replace);

                // the remaining part of the string.
                workingString = workingString.Substring(firstFound + findItemLength,
                    workingString.Length - (firstFound + findItemLength));
            }

            returning.Append(workingString);

            return returning.ToString();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return (value == null || value == "");
        }
    }
}