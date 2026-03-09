**[Verse Surf - A music lyrics party game](https://store.steampowered.com/app/1315390/Verse_Surf/)**
![Verse Surf video](movie480_vp9.gif)

Verse Surf is a music party game running on desktop and web mobile. It was formerly [on steam](https://store.steampowered.com/app/1315390/Verse_Surf/).

## How to play

Step 1. Along with up to 8 players (minimum 2), you can use your phone or tablet as a game controller and join your room.

Step 2. Listen to the song sample and enter on your game controller of choise a fake lyric to fit the snippet and fool the other players.

Vote your favorite lyric - you’ll get a “thanks” if you vote a friends’ lyric, but keep in mind that the truth always makes you a winner.

Step 3. T-t-t-tie breaker! Last round is a Speed Round. Answer correctly and you get points for each second left on the clock.

## How to build

The game is built using Unity, .NET Core, SignalR and Razor pages.
The main projects are:

- Unity project under MusicTV/Songquiz
- MusicServer, a .NET Core project hosting the game server and the backoffice
- PlayVerseSurf, a HTML and JavaScript presentation site for the game

There is no database, all game data is stored in memory and lost when the server is restarted. The game uses a simple JSON file to store the list of songs and their lyrics.

You can build the game by opening the MusicTV/Songquiz project in Unity and building it for your target platform. The MusicServer project can be built using Visual Studio or the .NET CLI. Also check out the GitHub Actions workflows for automated builds and deployments.

## MIT License

Copyright (c) 2023 SHOPSOFT SRL

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
