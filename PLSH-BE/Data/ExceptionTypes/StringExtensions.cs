namespace Data.ExceptionTypes;

public static class StringExtensions
{
  public static int LevenshteinDistance(string s1, string s2)
  {
    if (s1 == s2) return 0;
    if (s1.Length == 0) return s2.Length;
    if (s2.Length == 0) return s1.Length;

    var matrix = new int[s1.Length + 1, s2.Length + 1];
    for (int i = 0; i <= s1.Length; matrix[i, 0] = i++) { }
    for (int j = 0; j <= s2.Length; matrix[0, j] = j++) { }

    for (int i = 1; i <= s1.Length; i++)
    {
      for (int j = 1; j <= s2.Length; j++)
      {
        int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
        matrix[i, j] = Math.Min(
          Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
          matrix[i - 1, j - 1] + cost);
      }
    }

    return matrix[s1.Length, s2.Length];
  }
}