# Engine
Chess Engine in C# (UCI protocol and modifications for Chessaria (http://chessaria.com/))

# News
. .NET Framework 3.5
. UCI_LimitStrength and UCI_Elo ({0, 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000})
. Play with holes and walls into the game. 

# Components

Book - Polyglot format

EngineConsole - UCI protocol and testing Chessaria (possible defines: CHESSARIA) - Default: 

EngineDLL - Core of the engine (possible defines: TABLEBASES, BOOK, CHESSARIA) - Default: TABLEBASE;BOOK

Tablebases - Some Tablebases included int gtb format (kbk, kbkb, kbkn, kbnk, knkn, kpk, kpkp, kqk, kqkr, krk, krkb)

# Tournaments

Este motor incluye dentro de su ejecutable un libro de aperturas propio y una serie de tablas de finales de 3 piezas. No es por lo tanto adecuado por el momento para jugar torneos donde existan reglas específicas sobre el funcionamiento del motor.
(This engine includes into the executable an own book and endtables. It is therefore not suitable for the moment to play tournaments where there are specific rules on the operation of the engine.)

# Credits

Debido a la imposibilidad de poder mantener las versiones anteriores de Alfil a un nivel aceptable y con el propósito de dar a la comunidad un nuevo motor de ajedrez con ideas nuevas y pensando siempre en la “interacción con el humano” más que en los rankings y en la velocidad. Se ha tomado la determinación de hacer un nuevo motor opensource en C#.
Esta nueva versión en C# toma las ideas y el conocimiento adquirido tras más de 10 años de versiones de Alfil, comenzando de nuevo desde la base de los mejores programas opensource, añadiendo mejoras y comportamientos diferentes basados en programas opensource como:

(Since it is not possible to keep earlier versions of Alfil to an acceptable level, and in order to give the community a new chess engine with new ideas and always thinking about the “human interaction” rather than in the rankings and in speed. It has taken the decision to make a new engine opensource in C#.
This new version takes the ideas and knowledge gained from over 10 years of Alfil versions, starting again from the source of the best engines, adding over time, improvements and behaviors different programs based on opensource programs as:)

https://github.com/Alfilchess/Engine/tree/master/Credits

Este es un nuevo proyecto en C# pretende servir como herramienta de trabajo (en castellano) para futuros motores y herramientas de ajedrez.

(This is a new project in C# mainly from stockfish but intended to serve as a working tool (in Spanish) for future chess engines and tools.)

Everyone is invited to collaborate.
