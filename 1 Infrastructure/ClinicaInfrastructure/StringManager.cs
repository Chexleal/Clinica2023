using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ClinicaInfrastructure;

public static class StringManager
{
    public static IEnumerable<string> ReadLines(Stream stream,
        Encoding encoding)
    {
        using (var reader = new StreamReader(stream, encoding))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }


    public static string CutOnMax(string text, int max)
    {
        if (text.Length > max)
            return text.Substring(0, max - 1) + " ...";

        return text;
    }

    public static string CutOnMaxT(this string text, int max)
    {
        if (text == null)
            return "";
        if (text.Length > max)
            return text.Substring(0, max - 1) + " ...";

        return text;
    }

    public static string ClearUtf8(this string str)
    {
        string replaceValue = str.Replace(System.Environment.NewLine, "").Replace(System.Environment.NewLine, "");
        replaceValue = replaceValue.Replace("\"", string.Empty);
        replaceValue = replaceValue.Replace("'", string.Empty);
        replaceValue = Regex.Replace(replaceValue, @"[^\u0000-\u007F]+", string.Empty);

        return replaceValue;
    }

    /// <summary>
    /// Retorna un substring con la cadena, hasta la primera aparición del "filter"
    /// Si "filter" no existe. Retorna la cadena original.
    /// </summary>
    /// <param name="text">Cadena original de trabajo</param>
    /// <param name="filter">Filtro para cortar la cadena</param>
    /// <returns></returns>
    public static string SubStringByFilter(this string text, string filter)
    {
        var index = text.Trim().IndexOf(filter);

        try
        {
            if (index > 0)
                return text.Trim().Substring(0, index);
            else
                return text.Trim();
        }
        catch (Exception e)
        {

        }

        return text;
    }

    public static string RemoveTildes(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string RemoveTildes_v2(this string text)
    {
        return string.Concat(Regex.Replace(text, @"(?i)[\p{L}-[ña-z]]+", m =>
                m.Value.Normalize(NormalizationForm.FormD)
            )
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
    }

    public static string ReplaceSpecialCharsOcr(this string text)
    {
        return text.Trim().ToUpper().Replace("-", "").Replace(" ", "").Replace("O", "0").Replace("I", "1").Replace("|", "1");
    }

    public static double ComparePercentage(this string textOne, string textTwo)
    {
        textOne = textOne.ToUpper();
        textTwo = textTwo.ToUpper();

        if (string.IsNullOrEmpty(textOne) || string.IsNullOrEmpty(textTwo)) return 0.0;

        if (textOne.Equals(textTwo)) return 1.0;

        int sourceWordCount = textOne.Length;
        int targetWordCount = textTwo.Length;

        try
        {
            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    int cost = (textTwo[j - 1] == textOne[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            int stepsToSame = distance[sourceWordCount, targetWordCount];

            return 1.0 - ((double)stepsToSame / (double)Math.Max(textOne.Length, textTwo.Length));
        }
        catch (Exception e)
        {
            return 0.0;
        }
    }

    public static string RecursiveHtmlDecode(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;
        var tmp = HttpUtility.HtmlDecode(str);
        while (tmp != str)
        {
            str = tmp;
            tmp = HttpUtility.HtmlDecode(str);
        }
        return str;
    }

    public static string DeleteSpace(string s)
    {
        s = s.Replace(" ", "").Trim();
        return s;

    }

    public static List<string> RegExp(this string str, string expression)
    {
        Regex r = new Regex(expression);
        r.IsMatch(str);
        MatchCollection matches = r.Matches(str);
        List<string> response = new List<string>();
        foreach (Match match in matches)
        {
            response.Add(match.Value);
        }
        return response;
    }

    public static string Base64Encode(this string toEncode, Encoding encoding = null) => encoding == null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode)) : Convert.ToBase64String(encoding.GetBytes(toEncode));
    public static string Base64Decode(this string encodeData, Encoding encoding = null) => encoding?.GetString(Convert.FromBase64String(encodeData)) ?? Encoding.UTF8.GetString(Convert.FromBase64String(encodeData));




    public static List<List<string>> RegExpGrp(this string str, string expression)
    {
        Regex r = new Regex(expression);
        r.IsMatch(str);
        MatchCollection matches = r.Matches(str);
        List<List<string>> response = new List<List<string>>();
        List<string> partial = new List<string>();
        foreach (Match match in matches)
        {
            partial = new List<string>();
            GroupCollection groups = match.Groups;
            foreach (var grp in groups)
            {
                partial.Add(grp.ToString());
            }
            response.Add(partial);
        }
        return response;
    }
    public static IEnumerable<T> SplitByCharacter<T>(this string text, params char[] splits)
    {
        if (String.IsNullOrEmpty(text))
            return Enumerable.Empty<T>();

        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter.CanConvertFrom(typeof(string)))
        {
            return text.Split(splits).Select(s => (T)converter.ConvertFromString(s));
        }
        else throw new InvalidOperationException("Error al transformar");


    }

    public static string ClearSpecialCharacter(string word)
    {
        word ??= string.Empty;
        //string cleanWord = Regex.Replace(word, @"(\s+|@|&|'|\(|\)|<|>|#)", string.Empty);
        string cleanWord = Regex.Replace(word, @"(_|'|<|>|#|\*|\@|\&)", string.Empty);
        //cleanWord = RemoveTildes(cleanWord);
        return cleanWord;
    }

    public static bool ContainsSpecialCharacter(string word)
    {
        word ??= string.Empty;
        char[] charactersToCheck = { '_', '\'', '<', '>', '#','@','*','&' };
        if (charactersToCheck.Any(word.Contains)) return true;
        return false;
    }

    public static string ConvertWord(List<string> toTranslate)
    {
        var codes = GetLetterByCode();

        var word = "";

        foreach (var item in toTranslate)
        {
            try
            {
                //Console.WriteLine(item.Length);

                if (item.Length == 10)
                {
                    var first = item.Substring(0, item.Length / 2).Replace("<", "");
                    var second = item.Substring(item.Length / 2).Replace(">", "");
                    codes.TryGetValue(first, out char valorfirst);
                    word = word + valorfirst;
                    codes.TryGetValue(second, out char valorsecond);
                    word = word + valorsecond;
                }
                else
                {
                    var code = item.Replace("<", "").Replace(">", "");
                    var letter = codes.ContainsKey(code);

                    if (letter)
                    {
                        codes.TryGetValue(code, out char valor);
                        word = word + valor;
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }


        }

        return word;
    }

    private static Dictionary<string, char> GetLetterByCode()
    {
        var charactersDictionary = new Dictionary<string, char>();

        charactersDictionary.Add("0003", ' ');
        charactersDictionary.Add("000A", '\'');
        charactersDictionary.Add("000B", '(');
        charactersDictionary.Add("000C", ')');
        charactersDictionary.Add("000F", ',');
        charactersDictionary.Add("0010", '-');
        charactersDictionary.Add("0011", '.');
        charactersDictionary.Add("0012", '/');

        charactersDictionary.Add("0013", '0');
        charactersDictionary.Add("0014", '1');
        charactersDictionary.Add("0015", '2');
        charactersDictionary.Add("0016", '3');
        charactersDictionary.Add("0017", '4');
        charactersDictionary.Add("0018", '5');
        charactersDictionary.Add("0019", '6');
        charactersDictionary.Add("001A", '7');
        charactersDictionary.Add("001B", '8');
        charactersDictionary.Add("001C", '9');

        charactersDictionary.Add("001D", ':');
        charactersDictionary.Add("0023", '@');

        charactersDictionary.Add("0024", 'A');
        charactersDictionary.Add("0025", 'B');
        charactersDictionary.Add("0026", 'C');
        charactersDictionary.Add("0027", 'D');
        charactersDictionary.Add("0028", 'E');
        charactersDictionary.Add("0029", 'F');
        charactersDictionary.Add("002A", 'G');
        charactersDictionary.Add("002B", 'H');
        charactersDictionary.Add("002C", 'I');
        charactersDictionary.Add("002D", 'J');
        charactersDictionary.Add("002E", 'K');
        charactersDictionary.Add("002F", 'L');
        charactersDictionary.Add("0030", 'M');
        charactersDictionary.Add("0031", 'N');
        charactersDictionary.Add("0032", 'O');
        charactersDictionary.Add("0033", 'P');
        charactersDictionary.Add("0034", 'Q');
        charactersDictionary.Add("0035", 'R');
        charactersDictionary.Add("0036", 'S');
        charactersDictionary.Add("0037", 'T');
        charactersDictionary.Add("0038", 'U');
        charactersDictionary.Add("0039", 'V');
        charactersDictionary.Add("003A", 'W');
        charactersDictionary.Add("003B", 'X');
        charactersDictionary.Add("003C", 'Y');
        charactersDictionary.Add("003D", 'Z');

        charactersDictionary.Add("0042", '_');

        charactersDictionary.Add("0044", 'a');
        charactersDictionary.Add("0045", 'b');
        charactersDictionary.Add("0046._", 'c');
        charactersDictionary.Add("0047", 'd');
        charactersDictionary.Add("0048", 'e');
        charactersDictionary.Add("0049", 'f');
        charactersDictionary.Add("004A", 'g');
        charactersDictionary.Add("004B", 'h');
        charactersDictionary.Add("004C", 'i');
        charactersDictionary.Add("004D", 'j');
        charactersDictionary.Add("004E", 'k');
        charactersDictionary.Add("004F", 'l');
        charactersDictionary.Add("0050", 'm');
        charactersDictionary.Add("0051", 'n');
        charactersDictionary.Add("0052", 'o');
        charactersDictionary.Add("0053", 'p');
        charactersDictionary.Add("0054", 'q');
        charactersDictionary.Add("0055", 'r');
        charactersDictionary.Add("0056", 's');
        charactersDictionary.Add("0057", 't');
        charactersDictionary.Add("0058", 'u');
        charactersDictionary.Add("0059", 'v');
        charactersDictionary.Add("005A", 'w');
        charactersDictionary.Add("005B", 'x');
        charactersDictionary.Add("005C", 'y');
        charactersDictionary.Add("005D", 'z');

        charactersDictionary.Add("0062", 'Ш');
        charactersDictionary.Add("0063", 'Р');
        charactersDictionary.Add("006A", 'И');
        charactersDictionary.Add("006B", 'А');
        charactersDictionary.Add("006C", 'М');
        charactersDictionary.Add("006D", 'в');
        charactersDictionary.Add("006E", 'Ф');
        charactersDictionary.Add("0070", 'Е');
        charactersDictionary.Add("0072", 'Б');
        charactersDictionary.Add("0073", 'Н');
        charactersDictionary.Add("0076", 'С');
        charactersDictionary.Add("007A", 'К');
        charactersDictionary.Add("007B", 'В');
        charactersDictionary.Add("007C", 'О');
        charactersDictionary.Add("007D", 'к');
        charactersDictionary.Add("007E", 'З');
        charactersDictionary.Add("0080", 'Г');
        charactersDictionary.Add("0081", 'П');
        charactersDictionary.Add("0082", 'у');
        charactersDictionary.Add("0085", '»');
        charactersDictionary.Add("0088", 'т');
        charactersDictionary.Add("008D", '’');
        charactersDictionary.Add("0090", 'У');
        charactersDictionary.Add("0091", 'Т');
        charactersDictionary.Add("00A1", 'Ц');
        charactersDictionary.Add("00A2", '№');
        charactersDictionary.Add("00AA", 'э');
        charactersDictionary.Add("00AB", 'я');
        charactersDictionary.Add("00AC", 'і');
        charactersDictionary.Add("00AD", 'б');
        charactersDictionary.Add("00AE", 'й');
        charactersDictionary.Add("00AF", 'р');
        charactersDictionary.Add("00B0", 'с');
        charactersDictionary.Add("00B2", 'х');
        charactersDictionary.Add("00B5", '“');
        charactersDictionary.Add("00B9", 'п');
        charactersDictionary.Add("00BA", 'о');
        charactersDictionary.Add("00BD", '«');
        charactersDictionary.Add("00C1", 'ф');
        charactersDictionary.Add("00C8", 'а');
        charactersDictionary.Add("00CB", 'е');
        charactersDictionary.Add("00CE", 'ж');
        charactersDictionary.Add("00CF", 'з');
        charactersDictionary.Add("00D2", 'и');
        charactersDictionary.Add("00D3", 'н');
        charactersDictionary.Add("00DC", '–');
        charactersDictionary.Add("00E3", 'л');

        return charactersDictionary;
    }

    public static string Capitalize(this string str)
    {
        if (String.IsNullOrEmpty(str))
            return "";

        return char.ToUpper(str[0]) + str.Substring(1);
    }

    public static string Decapitalize(this string str)
    {
        if (String.IsNullOrEmpty(str))
            return "";

        return char.ToLower(str[0]) + str.Substring(1);
    }
}