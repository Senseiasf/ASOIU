static void menu()
{
    string? str1; string? str2; bool k = true; int ans1; int ans2;
    do
    {
        Console.WriteLine("===Вычисление расстояния Левенштейна и Дамерау-Левенштейна===");
        Console.WriteLine("Введите две строки через пробел: ");
        string[] strs = (Console.ReadLine() ?? "").Split(' ');
        if (strs[0].ToUpperInvariant() == "EXIT")
        {
            k = false;
            continue;
        }
        if (strs.Length != 2)
        {
            Console.WriteLine("Вы ввели не 2 строки! Попробуйте ещё раз!");
            continue;
        }
        str1=strs[0]; str2=strs[1];
        str1=str1.ToUpperInvariant();
        str2=str2.ToUpperInvariant();
        ans1 = Levenshtein(str1, str2);
        ans2 = DamLev(str1, str2);
        Console.WriteLine($"Расстояние Левештейна: {ans1}; Дамерау-Левенштейна: {ans2}");
    } while (k==true);
}

static int Levenshtein (string s1, string s2)
{
    int m = s1.Length;
    int n = s2.Length;

    if (m==0 && n==0) { return 0; }
    if (m == 0) { return n; }
    if (n == 0) { return m; }

    int[,] matrix = new int[s1.Length+1, s2.Length+1];
    for (int i = 0; i <= s1.Length; i++) { matrix[i, 0] = i; }
    for (int j = 0; j <= s2.Length; j++) { matrix[0, j] = j; }    

    for (int i = 1; i<=s1.Length; i++)
    {
        for (int j = 1; j<=s2.Length; j++)
        {
            bool same = s1[i-1] != s2[j-1];
            matrix[i, j] = Math.Min(matrix[i - 1, j] + 1, Math.Min(matrix[i, j - 1] + 1, matrix[i - 1, j - 1] + Convert.ToInt32(same))) ;
        }
    }
    return matrix[m,n];
}
static int DamLev (string s1, string s2) 
{
    int m = s1.Length;
    int n = s2.Length;

    if (m == 0 && n == 0) { return 0; }
    if (m == 0) { return n; }
    if (n == 0) { return m; }

    int[,] matrix = new int[s1.Length+1, s2.Length+1];
    for (int i = 0; i <= s1.Length; i++) { matrix[i, 0] = i; }
    for (int j = 0; j <= s2.Length; j++) { matrix[0, j] = j; }

    for (int i = 1; i <= s1.Length; i++)
    {
        for (int j = 1; j <= s2.Length; j++)
        {
            bool same = s1[i-1] != s2[j-1];
            int num = Convert.ToInt32(same);
            matrix[i, j] = Math.Min(matrix[i - 1, j] + 1, Math.Min(matrix[i, j - 1] + 1, matrix[i - 1, j - 1] + num));
            if (i>1 && j>1 && s1[i-1] == s2[j-2] && s1[i-2] == s2[j-1]) { matrix[i, j] = Math.Min(matrix[i - 2, j - 2] + num, matrix[i, j]);  }
        }
    }
    return matrix[m, n];
}
menu();
