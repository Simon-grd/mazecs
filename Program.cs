using System;

// ── Dimensions ──
const int WIDTH    = 50;
const int HEIGHT   = 20;
const int OFFSET_Y = 3;
const int OFFSET_X = 0;
const int CELL_W   = WIDTH  / 2;
const int CELL_H   = HEIGHT / 2;

// ── Colors ──
const ConsoleColor COLOR_WALL   = ConsoleColor.DarkGray;
const ConsoleColor COLOR_PLAYER = ConsoleColor.Yellow;
const ConsoleColor COLOR_EXIT   = ConsoleColor.Green;
const ConsoleColor COLOR_FLOOR  = ConsoleColor.DarkBlue;
const ConsoleColor COLOR_TITLE  = ConsoleColor.Cyan;
const ConsoleColor COLOR_HINT   = ConsoleColor.DarkCyan;
const ConsoleColor COLOR_WIN    = ConsoleColor.Green;
const ConsoleColor COLOR_QUIT   = ConsoleColor.Red;

// ── UI strings ──
const string MSG_TITLE = """
╔══════════════════════════════════════════════════╗
║          🏃 LABYRINTHE ASCII  C#  🏃             ║
╚══════════════════════════════════════════════════╝
""";
const string MSG_HINT  = "  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter";
const string MSG_WIN   = """
  ╔════════════════════════════════╗
  ║   🎉  FÉLICITATIONS !  🎉      ║
  ║   Vous avez trouvé la sortie ! ║
  ╚════════════════════════════════╝
""";
const string MSG_QUIT  = "\n  Partie abandonnée. À bientôt !";
const string MSG_PRESS = "  Appuyez sur une touche pour quitter...";

// ── Grid, player, exit ──
var grid   = new CellType[WIDTH, HEIGHT];
var playerX = 0;
var playerY = 0;
const int EXIT_X = (CELL_W - 1) * 2;
const int EXIT_Y = (CELL_H - 1) * 2;

GenerateMaze(grid, playerX, playerY);
grid[EXIT_X, EXIT_Y] = CellType.Exit;

// ── Initial screen draw ──
DrawScreen(grid);

// ── Game loop ──
var won = false;

while (!won)
{
    var key = Console.ReadKey(true).Key;

    var nx = playerX;
    var ny = playerY;

    switch (key)
    {
        case ConsoleKey.Z or ConsoleKey.UpArrow:    ny--; break;
        case ConsoleKey.S or ConsoleKey.DownArrow:  ny++; break;
        case ConsoleKey.Q or ConsoleKey.LeftArrow:  nx--; break;
        case ConsoleKey.D or ConsoleKey.RightArrow: nx++; break;
        case ConsoleKey.Escape:                     goto EndLoop;
    }

    if (nx >= 0 && nx < WIDTH && ny >= 0 && ny < HEIGHT && grid[nx, ny] != CellType.Wall)
    {
        if (grid[nx, ny] == CellType.Exit) won = true;

        grid[playerX, playerY] = CellType.Floor;
        DrawCell(playerX, playerY);

        playerX = nx;
        playerY = ny;
        grid[playerX, playerY] = CellType.Player;
        DrawCell(playerX, playerY);
    }
}

EndLoop:

// ── End screen ──
if (won)
    DrawTextXY(0, OFFSET_Y + HEIGHT + 3, MSG_WIN,  COLOR_WIN);
else
    DrawTextXY(0, OFFSET_Y + HEIGHT + 3, MSG_QUIT, COLOR_QUIT);

DrawTextXY(0, OFFSET_Y + HEIGHT + 8, MSG_PRESS);
Console.CursorVisible = true;
Console.ReadKey(true);


// ════════════════════════════════════════════════════════════
//  FUNCTIONS
// ════════════════════════════════════════════════════════════

// ── Place cursor at (x,y), optionally set color, print text, reset color ──
void DrawTextXY(int x, int y, string text, ConsoleColor? color = null)
{
    Console.SetCursorPosition(x, y);
    if (color.HasValue) Console.ForegroundColor = color.Value;
    Console.WriteLine(text);
    Console.ResetColor();
}

// ── Draw a single cell using a switch expression (tuple deconstruction) ──
void DrawCell(int cx, int cy)
{
    var (color, pattern) = grid[cx, cy] switch
    {
        CellType.Wall   => (COLOR_WALL,   "█"),
        CellType.Player => (COLOR_PLAYER, "@"),
        CellType.Exit   => (COLOR_EXIT,   "★"),
        _               => (COLOR_FLOOR,  "·"),
    };

    Console.SetCursorPosition(OFFSET_X + cx, OFFSET_Y + cy);
    Console.ForegroundColor = color;
    Console.Write(pattern);
    Console.ResetColor();
}

// ── Draw the full initial game screen ──
void DrawScreen(CellType[,] g)
{
    Console.Clear();
    Console.CursorVisible = false;

    DrawTextXY(0, 0, MSG_TITLE, COLOR_TITLE);

    for (var y = 0; y < HEIGHT; y++)
        for (var x = 0; x < WIDTH; x++)
            DrawCell(x, y);

    DrawTextXY(0, OFFSET_Y + HEIGHT + 1, MSG_HINT, COLOR_HINT);
}

// ── Generate the maze using recursive backtracker (Step 5: no intermediate arrays) ──
void GenerateMaze(CellType[,] g, int startPX, int startPY)
{
    // Fill with walls
    for (var y = 0; y < HEIGHT; y++)
        for (var x = 0; x < WIDTH; x++)
            g[x, y] = CellType.Wall;

    var rng     = new Random();
    var visited = new bool[CELL_W, CELL_H];

    var dirX = new int[] {  0, 1,  0, -1 };
    var dirY = new int[] { -1, 0,  1,  0 };

    // Recursive carving — replaces the explicit stack (stackX, stackY)
    Carve(startPX / 2, startPY / 2);

    // Place player at start
    g[startPX, startPY] = CellType.Player;

    void Carve(int cx, int cy)
    {
        visited[cx, cy]   = true;
        g[cx * 2, cy * 2] = CellType.Floor;

        var order = new int[] { 0, 1, 2, 3 };
        rng.Shuffle(order);

        foreach (var dir in order)
        {
            var nx = cx + dirX[dir];
            var ny = cy + dirY[dir];

            if (nx >= 0 && nx < CELL_W && ny >= 0 && ny < CELL_H && !visited[nx, ny])
            {
                // Carve the wall between (cx,cy) and (nx,ny)
                g[cx * 2 + dirX[dir], cy * 2 + dirY[dir]] = CellType.Floor;
                Carve(nx, ny);
            }
        }
    }
}


// ════════════════════════════════════════════════════════════
//  ENUMS
// ════════════════════════════════════════════════════════════

enum CellType { Floor = 0, Wall = 1, Player = 2, Exit = 3 }