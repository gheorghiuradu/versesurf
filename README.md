**[Verse Surf - A music lyrics party game](https://store.steampowered.com/app/1315390/Verse_Surf/)**
![Verse Surf video](movie480_vp9.gif)

Verse Surf is a music party game running on desktop and web mobile. It was formerly [on steam](https://store.steampowered.com/app/1315390/Verse_Surf/).

## How to play
Step 1. Along with up to 8 players (minimum 2), you can use your phone or tablet as a game controller and join your room.

Step 2. Listen to the song sample and enter on your game controller of choise a fake lyric to fit the snippet and fool the other players.

Vote your favorite lyric - you’ll get a “thanks” if you vote a friends’ lyric, but keep in mind that the truth always makes you a winner.

Step 3. T-t-t-tie breaker! Last round is a Speed Round. Answer correctly and you get points for each second left on the clock.

## How to build
The game is built using Unity, .NET Core, SignalR and Blazor. **It was using Steam, Playfab (Azure Game Services) and Google Cloud Platform, so they need to be removed before running locally.**
**If you manage to do so, please create a pull request here.**
The main projects are:
- Unity project under MusicTV/Songquiz
- MusicServer, MusicWebClient, MusicApiServerless, BackOffice -> Open MusicServer/MusicServer.csproj
  
## How to configure
Please use the included backoffice to upload songs and create playlists. There are also tools for music licensing included, if you require them.
The playlists were previously stored in Firezone and Google Cloud Storage, but they need to be refactored to use local storage. **Please create a PR with this change if you do this.**

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
