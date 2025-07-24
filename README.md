# Puzzle Collaborativo Real-Time

Questo è il repository Frontend del progetto "Puzzle Collaborativo in Real-Time". È un'applicazione full-stack pensata per mostrare un'interazione in tempo reale tra più utenti su un puzzle grafico.
L'idea è che chiunque si connetta possa collaborare per risolvere il puzzle, e ogni movimento di un pezzo è visibile istantaneamente a tutti gli altri giocatori.

## Ho diviso il progetto in due repository principali:

**PuzzleBackendApp**: Il backend dell'app, gestisce lo stato del puzzle e la comunicazione in tempo reale. [Link qui](https://github.com/Mogan0/PuzzleBackendApp)

**PuzzleFrontendApp**: L'interfaccia utente web, quella con cui interagisci per giocare. [Link qui](https://github.com/Mogan0/PuzzleFrontendApp/)

Ho scelto di containerizzare entrambi i servizi con Docker.

### Tecnologie Utilizzate

**Frontend**: Flutter (Dart) - per un'interfaccia web reattiva.

**Backend**: ASP.NET Core (C#) - per l'API e la gestione della logica.

**Comunicazione Real-Time**: ASP.NET Core SignalR - Permette la sincronizzazione istantanea con WebSocket/Long Polling.

**Container**: Docker - per isolare e far girare tutto.

**Server Web Proxy**: Nginx (dentro il container Flutter) - per servire il frontend e inoltrare le richieste SignalR.

# Per avviare il progetto PuzzleBackendApp [Link qui](https://github.com/Mogan0/PuzzleBackendApp)

Per avviare l'applicazione, ti servirà Docker Desktop (o un setup Docker equivalente) sul computer.

## 1. Clona i Repository del backend

    git clone https://github.com/Mogan0/PuzzleBackendApp
    cd PuzzleBackendApp

## 2. Costruisci l'Immagine Docker del Backend

Assicurati di essere nella directory PuzzleBackendApp.
Bash

    docker build -t puzzle-backend .

## 3. Avvia il Container

Ora puoi avviare il servizio. Assicurati che la porta 5000.

    docker run -d -p 5000:5000 --name puzzle-backend-container puzzle-backend


# Per avviare il progetto PuzzleFrontendApp [Link qui](https://github.com/Mogan0/PuzzleFrontendApp/)

## 1. Clona il repository del frontend
    git clone https://github.com/Mogan0/PuzzleFrontendApp
    cd PuzzleFrontendApp

## 3. Costruisci l'Immagine Docker del Frontend

    cd ../PuzzleFrontendApp # Se sei ancora nella cartella del backend
    docker build --no-cache -t puzzle-frontend .

### Avvia il container del frontend
    docker run -d -p 80:80 --name puzzle-frontend-container puzzle-frontend

## Avviare web browser

*Avviare un web browser qualsiasi e andare all'indirizzo* http://localhost 




## BONUS: Accedere da Altri Dispositivi (Rete Locale)

Se vuoi testarlo con un cellulare o un altro PC sulla tua rete locale, ecco come:

Trova l'IP del tuo PC: Sul computer dove hai avviato Docker, cerca il suo indirizzo IP locale (es. 192.168.1.100).

Aggiorna l'URL nel Frontend: Nel codice Flutter (in lib/PuzzleHome.dart) alla riga 33, cambia l'URL di SignalR per usare l'IP del tuo PC invece di localhost. Ad esempio: http://192.168.1.100/puzzlehub.
Dopo questa modifica, dovrai ricostruire l'immagine Docker del frontend e riavviare il suo container.

Aggiorna le impostazioni CORS nel Backend: Nel Program.cs del backend, aggiungi il tuo IP locale alla lista WithOrigins per i CORS.
Anche qui, dopo questa modifica, dovrai ricostruire l'immagine Docker del backend e riavviare il suo container.
