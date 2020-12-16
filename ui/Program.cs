using System;
using System.Threading;

class Program
{
	const sbyte Bomb = -1;
	const sbyte Empty = 0;
	const char BoardSymbolNone = ' ';
	const char BoardSymbolBomb = 'x';
	const char BoardSymbolQuestion = '?';
	const char BoardSymbolHidden = '*';

    sbyte[,] field;
    char[,] board;

    int sizeX;
	int sizeY;
	int bombCount;

    int cursorX;
	int cursorY;

	int boardPosX;
	int boardPosY;
	int statusPosY;
	static void Main()
	{
		int minWidth = Math.Min(5, Console.WindowWidth);
		int maxWidth = Math.Max(minWidth, Console.WindowWidth);
		int minHeight = Math.Min(5, Console.WindowHeight - 10);
		int maxHeight = Math.Max(minHeight, Console.WindowHeight - 10);
		if (minWidth < 5 || minHeight < 5)
		{
			Console.WriteLine("Вiкно занадто мале");
		}

		int sizeX = readLineNumber("Введiть розмiр поля по горизонталi", minWidth, maxWidth);
		int sizeY = readLineNumber("Введiть розмiр поля по вертикалi", minHeight, maxHeight);
		int bombCount = readLineNumber("Задайте кiлькiсть бомб", 5, sizeX * sizeY / 2);

		Program m = new Program(sizeX, sizeY, bombCount);
		m.run();
	}

	static int readLineNumber(string label, int min, int max)
	{
		int value = 0;
		string input;

		do
		{
			do
			{
				Console.Write("{0} [{1}-{2}]:", label, min, max);
				input = Console.ReadLine();
			} while (!int.TryParse(input, out value));
			if (value < min || value > max)
			{
				value = 0;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Помилка.");
				Console.ResetColor();
			}
		} while (value == 0); 
		return value;
	}

	public Program(int size_x, int size_y, int bomb_Count)
	{
		sizeX = size_x;
		sizeY = size_y;
		bombCount = bomb_Count;
	}

	void run()
	{
		Console.Clear();
		printHelp();

		init();
		dropBombs();
		prepareField();

		showBoard();

		Console.CursorVisible = false;

		bool quit = false;

		while (!quit)
		{
			showCursor();
			if (Console.KeyAvailable)
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);
				switch (keyInfo.Key)
				{
					case ConsoleKey.UpArrow:
						moveCursor(0, -1);
						break;
					case ConsoleKey.DownArrow:
						moveCursor(0, +1);
						break;
					case ConsoleKey.LeftArrow:
						moveCursor(-1, 0);
						break;
					case ConsoleKey.RightArrow:
						moveCursor(1, 0);
						break;
					case ConsoleKey.Spacebar:
						markPosition();
						break;
					case ConsoleKey.Enter:
						if (isBomb(cursorX, cursorY))
						{
							quit = true;
							showStatusMessage("Програш");
						}
						else if (!isOpened(cursorX, cursorY))
						{
							showFieldPosition(cursorX, cursorY);
						}

						break;
					case ConsoleKey.Q:
						quit = true;
						showStatusMessage("Вихід");
						break;
				}
			}
			if (isWin())
			{
				quit = true;
				showStatusMessage("Перемога!");
			}
			Thread.Sleep(100);
		}
		finish();
	}

	void init()
	{
		field = new sbyte[sizeX, sizeY];
		board = new char[sizeX, sizeY];

		cursorX = sizeX / 2;
		cursorY = sizeY / 2;

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				field[x, y] = Empty;
				board[x, y] = BoardSymbolHidden;
			}
		}
		boardPosX = Console.CursorLeft;
		boardPosY = Console.CursorTop;
		statusPosY = boardPosY + sizeY + 1;
	}

	void prepareField()
	{
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				if (!isBomb(x, y))
				{
					field[x, y] = calculatePositionValue(x, y);
				}
			}
		}
	}

	sbyte calculatePositionValue(int posX, int posY)
	{
		int minX = Math.Max(posX - 1, 0);
		int maxX = Math.Min(posX + 1, sizeX - 1);
		int minY = Math.Max(posY - 1, 0);
		int maxY = Math.Min(posY + 1, sizeY - 1);

		sbyte result = 0;
		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				if (isBomb(x, y))
				{
					result++;
				}
			}
		}
		return result;
	}

	void finish()
	{
		showField();
		showCursor();
		Console.SetCursorPosition(0, statusPosY + 1);
		Console.WriteLine("Bye!");
		Console.CursorVisible = true;
	}

	void printHelp()
	{
		Console.WriteLine("Розмір поля {0}x{1}, кількість мін - {2}", sizeX, sizeY, bombCount);
		Console.WriteLine("За допомогою стрілок оберіть клітинку.");
		Console.WriteLine("Поставте на ній флажок, використовуючи пробіл,");
		Console.WriteLine("або відкрийте за допомогою Enter.");
		Console.WriteLine("Q - вийти з гри.");
	}


	void showStatusMessage(string message)
	{
		ConsoleColor fg = ConsoleColor.Blue;
		ConsoleColor bg = ConsoleColor.Black;

		writeXY(0, statusPosY, message, fg, bg);
	}

	void showField()
	{
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				showFieldPosition(x, y);
			}
		}
	}

	void showBoard()
	{
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				showBoardPosition(x, y);
			}
		}
	}

	void showBoardPosition(int posX, int posY, bool invertColors = false)
	{
		char symbol = board[posX, posY];

		ConsoleColor fg = ConsoleColor.Gray;
		ConsoleColor bg = ConsoleColor.Black;

		if (BoardSymbolBomb == symbol)
		{
			fg = ConsoleColor.Red;
		}
		else if (BoardSymbolQuestion == symbol)
		{
			fg = ConsoleColor.DarkCyan;
		}

		if (invertColors)
		{
			ConsoleColor tmp = bg;
			bg = fg;
			fg = tmp;
		}

		writeOnBoard(posX, posY, symbol, fg, bg);

		if (BoardSymbolNone == symbol)
		{
			uncover(posX, posY);
		}
	}

	void showFieldPosition(int posX, int posY)
	{
		char symbol;
		if (Bomb == field[posX, posY])
		{
			symbol = BoardSymbolBomb;
		}
		else if (Empty == field[posX, posY])
		{
			symbol = BoardSymbolNone;
		}
		else
		{
			symbol = Convert.ToChar(field[posX, posY].ToString());
		}
		board[posX, posY] = symbol;
		showBoardPosition(posX, posY);
	}

	void markPosition()
	{
		char current = board[cursorX, cursorY];
		char next;

		if (BoardSymbolHidden == current)
		{
			next = BoardSymbolBomb;
		}
		else if (BoardSymbolBomb == current)
		{
			next = BoardSymbolQuestion;
		}
		else if (BoardSymbolQuestion == current)
		{
			next = BoardSymbolHidden;
		}
		else
		{
			return;
		}
		board[cursorX, cursorY] = next;
		showBoardPosition(cursorX, cursorY);
	}

	void uncover(int posX, int posY)
	{
		char symbol = board[posX, posY];

		if (BoardSymbolNone != symbol)
		{
			return;
		}

		int minX = Math.Max(posX - 1, 0);
		int maxX = Math.Min(posX + 1, sizeX - 1);
		int minY = Math.Max(posY - 1, 0);
		int maxY = Math.Min(posY + 1, sizeY - 1);

		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				if (!isOpened(x, y))
				{
					showFieldPosition(x, y);
					if (BoardSymbolNone == symbol)
					{
						uncover(x, y);
					}
				}
			}
		}
	}

	bool isWin()
	{
		char symbol;
		int markCount = 0;
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				symbol = board[x, y];
				if (BoardSymbolHidden == symbol
					|| BoardSymbolQuestion == symbol
				)
				{
					return false;
				}
				if (BoardSymbolBomb == symbol)
				{
					markCount++;
				}
			}
		}
		return markCount == bombCount;
	}

	void moveCursor(sbyte deltaX, sbyte deltaY)
	{
		showBoardPosition(cursorX, cursorY);
		cursorY = Math.Max(cursorY + deltaY, 0);
		cursorY = Math.Min(cursorY, sizeY - 1);
		cursorX = Math.Max(cursorX + deltaX, 0);
		cursorX = Math.Min(cursorX, sizeX - 1);
		showCursor();
	}

	void showCursor()
	{
		showBoardPosition(cursorX, cursorY, true);
	}

	void dropBombs()
	{
		int bombPlanted = 0;
		int randX;
		int randY;

		Random ran = new Random();
		do
		{
			randX = ran.Next(sizeX);
			randY = ran.Next(sizeY);
			if (field[randX, randY] != Bomb)
			{
				field[randX, randY] = Bomb;
				bombPlanted++;
			}
		} while (bombPlanted < bombCount);
	}

	bool isOpened(int posX, int posY)
	{
		return BoardSymbolHidden != board[posX, posY];
	}

	bool isBomb(int posX, int posY)
	{
		return Bomb == field[posX, posY];
	}

	void writeOnBoard(int posX, int posY, char ch, ConsoleColor fgColor = ConsoleColor.Gray, ConsoleColor bgColor = ConsoleColor.Black)
	{
		writeXY(posX + boardPosX, posY + boardPosY, ch.ToString(), fgColor, bgColor);
	}

	void writeXY(int x, int y, string str, ConsoleColor fgColor = ConsoleColor.Gray, ConsoleColor bgColor = ConsoleColor.Black)
	{
		Console.ForegroundColor = fgColor;
		Console.BackgroundColor = bgColor;

		Console.SetCursorPosition(x, y);
		Console.Write(str);
		Console.ResetColor();
	}
}
