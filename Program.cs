using System;

// ============================================================
//  ASCII MAZE - C# Console
//  Grid : int[50, 20]  (width=50, height=20)
//  0 = floor   1 = wall   2 = player   3 = exit
//  Moves : Z/Q/S/D or arrows
//  ✅ Optimized: only modified cells are redrawn
//               via Console.SetCursorPosition()
// ============================================================

var grid = new int[50, 20];

const int width   = 50;
const int height  = 20;

// Vertical offset in console (number of title lines)
const int offsetY = 3;
const int offsetX = 0;

// ── Maze generation by "recursive backtracker" ──
const int cellW = width  / 2;   // 25
const int cellH = height / 2;   // 10

for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
        grid[x, y] = 1;

var stackX = new int[cellW * cellH];
var stackY = new int[cellW * cellH];
var stackTop = 0;

var visited = new bool[cellW, cellH];

var dx = new int[] { 0, 1, 0, -1 };
var dy = new int[] { -1, 0, 1, 0 };

var rng = new Random();

const int startCX = 0, startCY = 0;
visited[startCX, startCY] = true;
grid[startCX * 2, startCY * 2] = 0;

stackX[stackTop] = startCX;
stackY[stackTop] = startCY;
stackTop++;

while (stackTop > 0)
{
    var cx = stackX[stackTop - 1];
    var cy = stackY[stackTop - 1];

    var order = new int[] { 0, 1, 2, 3 };
    for (var i = 3; i > 0; i--)
    {
        var j = rng.Next(i + 1);
        var tmp = order[i]; order[i] = order[j]; order[j] = tmp;
    }

    var found = false;
    for (var d = 0; d < 4; d++)
    {
        var nx = cx + dx[order[d]];
        var ny = cy + dy[order[d]];
        if (nx >= 0 && nx < cellW && ny >= 0 && ny < cellH && !visited[nx, ny])
        {
            grid[cx * 2 + dx[order[d]], cy * 2 + dy[order[d]]] = 0;
            grid[nx * 2, ny * 2] = 0;
            visited[nx, ny] = true;
            stackX[stackTop] = nx;
            stackY[stackTop] = ny;
            stackTop++;
            found = true;
            break;
        }
    }
    if (!found) stackTop--;
}

// ── Player and exit positions ──
var playerX = 0;
var playerY = 0;
const int exitX = (cellW - 1) * 2;
const int exitY = (cellH - 1) * 2;

grid[playerX, playerY] = 2;
grid[exitX,   exitY  ] = 3;

// ── Full initial draw (only once) ──
Console.Clear();
Console.CursorVisible = false;

Console.SetCursorPosition(0, 0);
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║          🏃 LABYRINTHE ASCII  C#  🏃             ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.ResetColor();

for (var y = 0; y < height; y++)
{
    for (var x = 0; x < width; x++)
    {
        Console.SetCursorPosition(offsetX + x, offsetY + y);
        var cell = grid[x, y];
        if      (cell == 1) { Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("█"); }
        else if (cell == 2) { Console.ForegroundColor = ConsoleColor.Yellow;   Console.Write("@"); }
        else if (cell == 3) { Console.ForegroundColor = ConsoleColor.Green;    Console.Write("★"); }
        else                { Console.ForegroundColor = ConsoleColor.DarkBlue; Console.Write("·"); }
    }
}

Console.SetCursorPosition(0, offsetY + height + 1);
Console.ForegroundColor = ConsoleColor.DarkCyan;
Console.Write("  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter");
Console.ResetColor();

// ── Local action: redraw ONE cell via SetCursorPosition ──
void DrawCell(int cx, int cy)
{
    Console.SetCursorPosition(offsetX + cx, offsetY + cy);
    var cell = grid[cx, cy];
    if      (cell == 1) { Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("█"); }
    else if (cell == 2) { Console.ForegroundColor = ConsoleColor.Yellow;   Console.Write("@"); }
    else if (cell == 3) { Console.ForegroundColor = ConsoleColor.Green;    Console.Write("★"); }
    else                { Console.ForegroundColor = ConsoleColor.DarkBlue; Console.Write("·"); }
    Console.ResetColor();
}

// ── Game loop ──
var won = false;

while (!won)
{
    var key = Console.ReadKey(true).Key;

    var nx = playerX;
    var ny = playerY;

    if      (key == ConsoleKey.Z || key == ConsoleKey.UpArrow)    ny--;
    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)  ny++;
    else if (key == ConsoleKey.Q || key == ConsoleKey.LeftArrow)  nx--;
    else if (key == ConsoleKey.D || key == ConsoleKey.RightArrow) nx++;
    else if (key == ConsoleKey.Escape) break;

    if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny] != 1)
    {
        if (grid[nx, ny] == 3) won = true;

        grid[playerX, playerY] = 0;
        DrawCell(playerX, playerY);

        playerX = nx;
        playerY = ny;
        grid[playerX, playerY] = 2;
        DrawCell(playerX, playerY);
    }
}

// ── Victory screen ──
Console.SetCursorPosition(0, offsetY + height + 3);
if (won)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  ╔════════════════════════════════╗");
    Console.WriteLine("  ║   🎉  FÉLICITATIONS !  🎉      ║");
    Console.WriteLine("  ║   Vous avez trouvé la sortie ! ║");
    Console.WriteLine("  ╚════════════════════════════════╝");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n  Partie abandonnée. À bientôt !");
    Console.ResetColor();
}

Console.SetCursorPosition(0, offsetY + height + 8);
Console.WriteLine("  Appuyez sur une touche pour quitter...");
Console.CursorVisible = true;
Console.ReadKey(true);