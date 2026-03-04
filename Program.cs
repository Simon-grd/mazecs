using System;

Maze.Run();

static class Maze
{
    const int Width   = 50;
    const int Height  = 20;
    const int OffsetY = 3;
    const int OffsetX = 0;
    const int CellW   = Width  / 2;
    const int CellH   = Height / 2;

    const string Title    = "╔══════════════════════════════════════════════════╗\n║            LABYRINTHE ASCII  C#                  ║\n╚══════════════════════════════════════════════════╝";
    const string Hint     = "  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter";
    const string WinMsg   = "  ╔════════════════════════════════╗\n  ║       FELICITATIONS !          ║\n  ║   Vous avez trouve la sortie ! ║\n  ╚════════════════════════════════╝";
    const string QuitMsg  = "\n  Partie abandonnée. À bientôt !";
    const string PressKey = "  Appuyez sur une touche pour quitter...";

    public static void Run()
    {
        var grid   = Generator.Build(Width, Height, CellW, CellH);
        var player = new Point(0, 0);
        var exit   = new Point((CellW - 1) * 2, (CellH - 1) * 2);

        grid[exit.X, exit.Y] = CellType.Exit;
        Renderer.DrawScreen(grid, Width, Height, OffsetX, OffsetY, Title, Hint);

        var won = false;
        while (!won)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape) break;

            var (dx, dy) = key switch
            {
                ConsoleKey.Z or ConsoleKey.UpArrow    => ( 0, -1),
                ConsoleKey.S or ConsoleKey.DownArrow  => ( 0,  1),
                ConsoleKey.Q or ConsoleKey.LeftArrow  => (-1,  0),
                ConsoleKey.D or ConsoleKey.RightArrow => ( 1,  0),
                _                                     => ( 0,  0),
            };

            if (dx == 0 && dy == 0) continue;

            var dest = new Point(player.X + dx, player.Y + dy);

            if (dest.X < 0 || dest.X >= Width || dest.Y < 0 || dest.Y >= Height) continue;
            if (grid[dest.X, dest.Y] == CellType.Wall) continue;

            if (grid[dest.X, dest.Y] == CellType.Exit) won = true;

            grid[player.X, player.Y] = CellType.Floor;
            Renderer.DrawCell(player.X, player.Y, CellType.Floor, OffsetX, OffsetY);

            player = dest;
            grid[player.X, player.Y] = CellType.Player;
            Renderer.DrawCell(player.X, player.Y, CellType.Player, OffsetX, OffsetY);
        }

        Renderer.DrawTextXY(0, OffsetY + Height + 2, won ? WinMsg  : QuitMsg,
                                                     won ? ConsoleColor.Green : ConsoleColor.Red);
        Renderer.DrawTextXY(0, OffsetY + Height + 7, PressKey);
        Console.CursorVisible = true;
        Console.ReadKey(true);
    }
}

static class Generator
{
    static readonly (int X, int Y)[] Dirs = [(0, -1), (1, 0), (0, 1), (-1, 0)];

    public static CellType[,] Build(int width, int height, int cellW, int cellH)
    {
        var grid    = new CellType[width, height];
        var visited = new bool[cellW, cellH];
        var rng     = new Random();

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                grid[x, y] = CellType.Wall;

        Carve(0, 0);
        grid[0, 0] = CellType.Player;
        return grid;

        void Carve(int cx, int cy)
        {
            visited[cx, cy]      = true;
            grid[cx * 2, cy * 2] = CellType.Floor;

            var order = (int[])[0, 1, 2, 3];
            rng.Shuffle(order);

            foreach (var dir in order)
            {
                var (dx, dy) = Dirs[dir];
                var (nx, ny) = (cx + dx, cy + dy);

                if (nx >= 0 && nx < cellW && ny >= 0 && ny < cellH && !visited[nx, ny])
                {
                    grid[cx * 2 + dx, cy * 2 + dy] = CellType.Floor;
                    Carve(nx, ny);
                }
            }
        }
    }
}

static class Renderer
{
    static readonly (ConsoleColor Color, string Pattern) WallStyle   = (ConsoleColor.DarkGray,  "█");
    static readonly (ConsoleColor Color, string Pattern) PlayerStyle = (ConsoleColor.Yellow,    "@");
    static readonly (ConsoleColor Color, string Pattern) ExitStyle   = (ConsoleColor.Green,     "★");
    static readonly (ConsoleColor Color, string Pattern) FloorStyle  = (ConsoleColor.DarkBlue,  "·");

    public static void DrawScreen(CellType[,] grid, int width, int height, int offsetX, int offsetY, string title, string hint)
    {
        Console.Clear();
        Console.CursorVisible = false;
        DrawTextXY(0, 0, title, ConsoleColor.Cyan);
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                DrawCell(x, y, grid[x, y], offsetX, offsetY);
        DrawTextXY(0, offsetY + height + 1, hint, ConsoleColor.DarkCyan);
    }

    public static void DrawCell(int cx, int cy, CellType cell, int offsetX, int offsetY)
    {
        var (color, pattern) = cell switch
        {
            CellType.Wall   => WallStyle,
            CellType.Player => PlayerStyle,
            CellType.Exit   => ExitStyle,
            _               => FloorStyle,
        };
        Console.SetCursorPosition(offsetX + cx, offsetY + cy);
        Console.ForegroundColor = color;
        Console.Write(pattern);
        Console.ResetColor();
    }

    public static void DrawTextXY(int x, int y, string text, ConsoleColor? color = null)
    {
        Console.SetCursorPosition(x, y);
        if (color.HasValue) Console.ForegroundColor = color.Value;
        Console.Write(text);
        Console.ResetColor();
    }
}

record struct Point(int X, int Y);

enum CellType { Floor, Wall, Player, Exit }