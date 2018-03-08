# Engine
Chess Engine Alfil sf Battles in C# (new UCI protocol and modifications for Chessaria AICE (http://chessaria.com/))

# News
. NET Core, .NET Standard and Mac compilation (CHESSARIA define)

. Null move in FEN

. Faster Clear Hash option

. .NET Framework 3.5

. UCI_LimitStrength and UCI_Elo ({0, 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000})

. Play with holes and walls into the game.

. Play without King.

. Level option (1 - 16). New algorithm using Bernstein Binomial

. LevelELO. Use Old algorithm != Chessaria LevelELO

. Collectible white and black

. draw command. Draw board from the actual position

. elo command. Calculate Elo using LCT II 


# CHESSARIA
. Change turn when oponent can not move, not draw.

. Play with any number of pieces in the board

. Play without kings

. Walls and holes

. Collectibles black and white

. Missions (capture, domination and checkmate) 

. Select enpassant or not for each colorMissions (capture, domination and checkmate) 

. Select row for 2 steps pawn for each color

. Select promotion line different to 1 or 8

. LevelELO. Using new algorithm

# Components

Book - Polyglot format (BOOK define)

EngineConsole - UCI protocol and testing Chessaria - Default: 

EngineDLL - Core of the engine (possible defines: TABLEBASES, BOOK, CHESSARIA) - Default: TABLEBASE;BOOK

egtb, lzma, pawn_common, egtb_probe - Engine can read tablebases in gtb format (gtb.cp4)

Log - Log.txt file with input and output commands and more

# Tournaments

Este motor incluye dentro de su ejecutable un libro de aperturas propio y una serie de tablas de finales de 3 piezas. No es por lo tanto adecuado por el momento para jugar torneos donde existan reglas específicas sobre el funcionamiento del motor.
(This engine includes into the executable an own book and endtables. It is therefore not suitable for the moment to play tournaments where there are specific rules on the operation of the engine.)

# Credits

Debido a la imposibilidad de poder mantener las versiones anteriores de Alfil a un nivel aceptable y con el propósito de dar a la comunidad un nuevo motor de ajedrez con ideas nuevas y pensando siempre en la “interacción con el humano” más que en los rankings y en la velocidad. Se ha tomado la determinación de hacer un nuevo motor opensource en C#.
Esta nueva versión en C# toma las ideas y el conocimiento adquirido tras más de 10 años de versiones de Alfil, comenzando de nuevo desde la base de los mejores programas opensource como Stockfish, añadiendo mejoras y comportamientos diferentes basados en programas opensource como:

(Since it is not possible to keep earlier versions of Alfil to an acceptable level, and in order to give the community a new chess engine with new ideas and always thinking about the “human interaction” rather than in the rankings and in speed. It has taken the decision to make a new engine opensource in C#.
This new version takes the ideas and knowledge gained from over 10 years of Alfil versions, starting again from the source of the best engines like stockfish, adding over time, improvements and behaviors different programs based on opensource programs as:)

https://github.com/Alfilchess/Engine/tree/master/Credits

Este es un nuevo proyecto en C# pretende servir como herramienta de trabajo (en castellano) para futuros motores y herramientas de ajedrez, entre ellas el motor de Chessaria AICE.

(This is a new project in C# mainly from stockfish but intended to serve as a working tool (in Spanish) for future chess engines and tools, like Chessaria AICE.)

Everyone is invited to collaborate.
