namespace Mutagen.Bethesda.Analyzers.Cli;

public static class IntroConstants
{
    // Colors from Synthesis repo
    // PurpleSlightlyLight: #372773 (RGB: 55, 39, 115)
    // DarkPurple: #281d52 (RGB: 40, 29, 82)
    // LightPurple: #7458db (RGB: 116, 88, 219)
    // Salmon: #e16063 (RGB: 225, 96, 99)
    // PaleBlue: #57f1fc (RGB: 87, 241, 252)

    private const string SalmonForeground = "\x1b[38;2;225;96;99m";
    private const string PurpleForeground = "\x1b[38;2;55;39;115m";
    private const string LightPurpleForeground = "\x1b[38;2;116;88;219m";
    private const string MediumPurpleForeground = "\x1b[38;2;85;65;170m"; // Slightly darker than LightPurple
    private const string DarkPurpleBackground = "\x1b[48;2;40;29;82m";
    private const string PaleBlueForeground = "\x1b[38;2;87;241;252m";
    private const string Reset = "\x1b[0m";

    public static string TextSplash =
        """"
         ██████   ██████             █████
        ░░██████ ██████             ░░███
         ░███░█████░███  █████ ████ ███████    ██████    ███████  ██████  ████████
         ░███░░███ ░███ ░░███ ░███ ░░░███░    ░░░░░███  ███░░███ ███░░███░░███░░███
         ░███ ░░░  ░███  ░███ ░███   ░███      ███████ ░███ ░███░███████  ░███ ░███
         ░███      ░███  ░███ ░███   ░███ ███ ███░░███ ░███ ░███░███░░░   ░███ ░███
         █████     █████ ░░████████  ░░█████ ░░████████░░███████░░██████  ████ █████
        ░░░░░     ░░░░░   ░░░░░░░░    ░░░░░   ░░░░░░░░   ░░░░░███░░░░░░  ░░░░ ░░░░░
                                                        ███ ░███
          █████████                        ████        ░░██████
         ███░░░░░███                      ░░███         ░░░░░░
        ░███    ░███  ████████    ██████   ░███  █████ ████  █████████  ██████  ████████   █████
        ░███████████ ░░███░░███  ░░░░░███  ░███ ░░███ ░███  ░█░░░░███  ███░░███░░███░░███ ███░░
        ░███░░░░░███  ░███ ░███   ███████  ░███  ░███ ░███  ░   ███░  ░███████  ░███ ░░░ ░░█████
        ░███    ░███  ░███ ░███  ███░░███  ░███  ░███ ░███    ███░   █░███░░░   ░███      ░░░░███
        █████   █████ ████ █████░░████████ █████ ░░███████   █████████░░██████  █████     ██████
        ░░░░░   ░░░░░ ░░░░ ░░░░░  ░░░░░░░░ ░░░░░   ░░░░░███  ░░░░░░░░░  ░░░░░░  ░░░░░     ░░░░░░
                                                  ███ ░███
                                                 ░░██████
                                                  ░░░░░░
        """";

    public static string MushroomSplash =
        """"
                __.....__
             .'" _  o    "`.
           .' O (_)     () o`.
          .           O       .
         . ()   o__...__    O  .
        . _.--"""       """--._ .
        :"                     ";
         `-.__    :   :    __.-'
              """-:   :-"""
                 J     L
                 :     :
                J       L
                :       :
                `._____.'
        """";

    public static string BrailleMushroom =
        """
        ⠀⠀⠀⠀⠀⢀⢴⠒⡖⣒⢄⠀⠀⠀⠀
        ⠀⠀⠀⠀⣔⣉⡸⠊⠀⢇⡙⣷⠀⠀⠀
        ⠀⢀⡠⠮⢂⡔⡡⣏⡉⣰⠸⣝⡇⠀⠀
        ⣠⠻⣉⡠⠟⢮⠗⠒⡼⣦⢟⡉⢻⣄⠀
        ⠡⣷⡊⠩⢍⢉⣩⣉⣉⠭⠒⠋⠵⣻⣡
        ⠀⠀⠉⠓⠓⣟⠆⢠⢸⣴⣦⣧⣴⠿⠋
        ⠀⠀⠀⠀⠰⢽⡖⣿⠼⠀⠀⠀⠀⠀⠀
        ⠀⠀⠀⠀⠀⢸⠁⠈⠄⠀⠀⠀⠀⠀⠀
        ⠀⠀⠀⠀⠀⡎⠀⠈⠊⢢⠀⠀⠀⠀⠀
        ⠀⠀⠀⠀⠀⠣⣀⣀⣀⠴⠃⠀⠀⠀⠀
        """;

    // Characters on line 5 that should be darker purple (stem attachment)
    private static readonly HashSet<char> StemAttachment = ['⣟', '⠆', '⢠', '⢸'];

    // Determines if a position should be a salmon "spot" using pseudo-random pattern
    private static bool IsSalmonSpot(int lineNum, int charIndex)
    {
        // Group characters to create larger spots, with fewer overall
        var hash = (lineNum * 5 + (charIndex / 3) * 7) % 13;
        return hash < 8; // Larger spots, ~60% coverage
    }

    // Finds the index of the first non-empty braille character in a line
    private static int FindFirstNonEmpty(string line)
    {
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] != '⠀') return i;
        }
        return -1;
    }

    public static void PrintBrailleMushroom()
    {
        // Ensure UTF-8 encoding for braille characters in Windows PowerShell
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine();
        var lines = BrailleMushroom.Split('\n');

        for (var lineNum = 0; lineNum < lines.Length; lineNum++)
        {
            var line = lines[lineNum];
            // Lines 1-4 are the cap area where spots should be salmon
            var isCapArea = lineNum >= 1 && lineNum <= 4;
            // Lines 6-9 are the stem (bottom 4 lines) - darker purple
            var isStemArea = lineNum >= 6 && lineNum <= 9;
            // Line 5 has stem attachment characters
            var isStemAttachmentLine = lineNum == 5;
            // Find leftmost non-empty character for highlight
            var firstNonEmpty = FindFirstNonEmpty(line);

            for (var charIndex = 0; charIndex < line.Length; charIndex++)
            {
                var ch = line[charIndex];
                // Only highlight the first non-empty character on each line
                var isLeftEdge = charIndex == firstNonEmpty;

                if (ch == '⠀') // Empty braille
                {
                    Console.Write(ch);
                }
                else if (isLeftEdge)
                {
                    // Pale blue highlight on the leftmost edge
                    Console.Write(PaleBlueForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else if (isStemArea)
                {
                    // Slightly darker purple for the stem
                    Console.Write(MediumPurpleForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else if (isStemAttachmentLine && StemAttachment.Contains(ch))
                {
                    // Slightly darker purple for stem attachment characters
                    Console.Write(MediumPurpleForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else if ((isCapArea || isStemAttachmentLine) && IsSalmonSpot(lineNum, charIndex))
                {
                    // Salmon spots scattered across the cap and line 5 (non-stem parts)
                    Console.Write(SalmonForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else
                {
                    // Light purple for everything else
                    Console.Write(LightPurpleForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
            }
            Console.WriteLine();
        }
    }

    public static void PrintMushroom()
    {
        Console.WriteLine();

        // Track if we're "inside" the mushroom for background coloring
        foreach (var line in MushroomSplash.Split('\n'))
        {
            var insideMushroom = false;
            var firstRimSeen = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                var isRimChar = ch is '.' or '\'' or '"' or '-' or '_' or ':' or 'J' or 'L' or ';' or '`';
                var isCircleChar = ch is 'o' or 'O' or '(' or ')';

                if (isRimChar)
                {
                    if (!firstRimSeen)
                    {
                        firstRimSeen = true;
                        insideMushroom = true;
                    }
                    Console.Write(PaleBlueForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else if (isCircleChar)
                {
                    Console.Write(SalmonForeground);
                    Console.Write(ch);
                    Console.Write(Reset);
                }
                else if (ch == ' ' && insideMushroom)
                {
                    // Check if there's a rim character after this on the line
                    var hasRimAfter = false;
                    for (var j = i + 1; j < line.Length; j++)
                    {
                        var nextCh = line[j];
                        if (nextCh is '.' or '\'' or '"' or '-' or '_' or ':' or 'J' or 'L' or ';' or '`')
                        {
                            hasRimAfter = true;
                            break;
                        }
                    }

                    if (hasRimAfter)
                    {
                        // Use speckled look like the text shadow
                        Console.Write(PurpleForeground);
                        Console.Write('░');
                        Console.Write(Reset);
                    }
                    else
                    {
                        Console.Write(ch);
                    }
                }
                else
                {
                    Console.Write(ch);
                }
            }
            Console.WriteLine();
        }
    }

    public static void PrintTextSplash()
    {
        // Ensure UTF-8 encoding for braille characters in Windows PowerShell
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine();
        PrintColoredText(TextSplash);
        Console.WriteLine();
        // PrintMushroom();
        // Console.WriteLine();
        // PrintBrailleMushroom();
        // Console.WriteLine();
    }

    private static void PrintColoredText(string text)
    {
        foreach (var ch in text)
        {
            if (ch == '█')
            {
                // Solid blocks in salmon
                Console.Write(SalmonForeground);
                Console.Write(ch);
                Console.Write(Reset);
            }
            else if (ch == '░')
            {
                // Shade/shadow characters in purple
                Console.Write(PurpleForeground);
                Console.Write(ch);
                Console.Write(Reset);
            }
            else
            {
                Console.Write(ch);
            }
        }
    }
}
