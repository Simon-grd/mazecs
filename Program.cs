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
const string MSG_TITLE = "╔══════════════════════════════════════════════════╗\n║          🏃 LABYRINTHE ASCII  C#  🏃             ║\n╚══════════════════════════════════════════════════╝";
const string MSG_HINT  = "  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter";
const string MSG_WIN_1 = "  ╔════════════════════════════════╗";
const string MSG_WIN_2 = "  ║   🎉  FÉLICITATIONS !  🎉      ║";
const string MSG_WIN_3 = "  ║   Vous avez trouvé la sortie ! ║";
const string MSG_WIN_4 = "  ╚════════════════════════════════╝";
const string MSG_QUIT  = "\n  Partie abandonnée. À bientôt !";
const string MSG_PRESS = "  Appuyez sur une touche pour quitter...";

// ── Grid ──
var grid = new CellType[WIDTH, HEIGHT];

for (var y = 0; y < HEIGHT; y++)
    for (var x = 0; x < WIDTH; x++)
        grid[x, y] = CellType.Wall;

var stackX   = new int[CELL_W * CELL_H];
var stackY   = new int[CELL_W * CELL_H];
var stackTop = 0;
var visited  = new bool[CELL_W, CELL_H];

var dirX = new int[] { 0, 1, 0, -1 };
var dirY = new int[] { -1, 0, 1, 0 };
var rng  = new Random();

const int START_CX = 0, START_CY = 0;
visited[START_CX, START_CY]      = true;
grid[START_CX * 2, START_CY * 2] = CellType.Floor;

stackX[stackTop] = START_CX;
stackY[stackTop] = START_CY;
stackTop++;

while (stackTop > 0)
{
    var cx = stackX[stackTop - 1];
    var cy = stackY[stackTop - 1];

    var order = new int[] { 0, 1, 2, 3 };
    rng.Shuffle(order);

    var found = false;
    foreach (var dir in order)
    {
        var nx = cx + dirX[dir];
        var ny = cy + dirY[dir];
        if (nx >= 0 && nx < CELL_W && ny >= 0 && ny < CELL_H && !visited[nx, ny])
        {
            grid[cx * 2 + dirX[dir], cy * 2 + dirY[dir]] = CellType.Floor;
            grid[nx * 2, ny * 2]                          = CellType.Floor;
            visited[nx, ny]  = true;
            stackX[stackTop] = nx;
            stackY[stackTop] = ny;
            stackTop++;
            found = true;
            break;
        }
    }
    if (!found) stackTop--;
}

// ── Player and exit ──
var playerX = 0;
var playerY = 0;
const int EXIT_X = (CELL_W - 1) * 2;
const int EXIT_Y = (CELL_H - 1) * 2;

grid[playerX, playerY] = CellType.Player;
grid[EXIT_X,  EXIT_Y ] = CellType.Exit;

// ── Full initial draw — reuses DrawCell ──
Console.Clear();
Console.CursorVisible = false;

Console.SetCursorPosition(0, 0);
Console.ForegroundColor = COLOR_TITLE;
Console.WriteLine(MSG_TITLE);
Console.ResetColor();

for (var y = 0; y < HEIGHT; y++)
    for (var x = 0; x < WIDTH; x++)
        DrawCell(x, y);

Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 1);
Console.ForegroundColor = COLOR_HINT;
Console.Write(MSG_HINT);
Console.ResetColor();

// ── Game loop ──
var won = false;

while (!won)
{
    var key = Console.ReadKey(true).Key;

    var nx = playerX;
    var ny = playerY;

    // ✅ switch on key (replaces if/else if chain)
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
Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 3);
if (won)
{
    Console.ForegroundColor = COLOR_WIN;
    Console.WriteLine(MSG_WIN_1);
    Console.WriteLine(MSG_WIN_2);
    Console.WriteLine(MSG_WIN_3);
    Console.WriteLine(MSG_WIN_4);
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = COLOR_QUIT;
    Console.WriteLine(MSG_QUIT);
    Console.ResetColor();
}

Console.SetCursorPosition(0, OFFSET_Y + HEIGHT + 8);
Console.WriteLine(MSG_PRESS);
Console.CursorVisible = true;
Console.ReadKey(true);

// ── DrawCell — switch expression with tuple deconstruction ──
void DrawCell(int cx, int cy)
{
    // ✅ switch expression returning a tuple, deconstructed into (color, pattern)
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

// ── Enum ──
enum CellType { Floor = 0, Wall = 1, Player = 2, Exit = 3 }