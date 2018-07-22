# EcoServerDemon

EcoServerDemon ist ein Restart/Update-Tool für das Soiel Eco von StrangeLoopGames.
Zurzeit wird hier besonders in Bezug auf die Updatefunction von EcoServerDemon nur
Steamversionen unterstützt.

## Features
In der Datei "ecoloader.json" werden alle notwendigen Einstellungen vorgenommen.
Erläuterungen zu den einzelnen Einstellungen der Datei:
* "server_path" - Der Pfad in dem sich die "EcoServer.exe" befindet
* "restarts" - Angaben von Uhrzeiten an den der EcoServer neu gestartet werden soll. Die Angaben erfolgen jeweils im Format "HH:MM" im folgenden Beispiel wird der Server jeweils um 0:00, 6:00 und 18:00 neu gestartet: 
> "restart": [ "0:00", "06:00", "18:00"]
* "updates" - siehe "restart"
* "steam" - Hier folgen die Notwendigen Einstellungen zum Steamaccount
* "cmd_path" - Der Pfad wo sich die "steamcmd.exe" befindet. Diese kann unter (https://developer.valvesoftware.com/wiki/SteamCMD) heruntergeladen werden. Auf dieser Seite stehen auch weitere Informationen zu steamcmd.
* "username" - Steambenutzername 
* "password" - Steampassword
 
## Installation
Die Sourcen können heruntergeladen und mit Visual Studio erstellt werden. Gestartet wird dann die
Anwendung mit "EcoServerDemon.exe"

## Hinweis
Die Updateeinstellungen in der ecoloader.json haben die höchste Priorität. Sollte sich also in den Restarts eine gleiche Zeitangabe wie in den Updates stehen, so wird hier die Zeitangabe des Updates herangezogen, da der Server nach einem Update ebenfalls wieder neu gestartet wird.


