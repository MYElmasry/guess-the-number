# Guess The Number

A full-stack technical assessment application where authenticated users play a server-side "Guess the Number" game (1–43), track their best score, and request hints after several failed attempts.

## Overview

Users can register, sign in with secure HttpOnly cookie authentication, play the guessing game, and persist their best score in PostgreSQL. The secret number never leaves the server.

## Features

- User registration and login
- Secure logout with cookie clearing
- JWT authentication stored in an HttpOnly cookie
- Guess the Number game with server-side secret generation
- Best score tracking per user
- Hint bonus feature after 3 failed attempts
- Swagger/OpenAPI in development
- Dockerized PostgreSQL for local development
- Production-ready backend Dockerfile for Render
- Frontend ready for Vercel deployment

## Architecture

```text
React (Vite) → ASP.NET Core Web API → PostgreSQL
```

Clean architecture on the backend:

- `GuessNumber.API` — HTTP endpoints, middleware, auth configuration
- `GuessNumber.Application` — business logic and service interfaces
- `GuessNumber.Domain` — entities and constants
- `GuessNumber.Infrastructure` — EF Core, JWT, password hashing

## Tech Stack

**Backend:** .NET 10, ASP.NET Core Web API, EF Core, PostgreSQL, Npgsql, JWT, xUnit

**Frontend:** React, TypeScript, Vite, React Router, Axios

**Infrastructure:** Docker, Docker Compose

## Project Structure

```text
Client/GuessNumber.Web/          React frontend
Server/GuessNumber.API/          Web API entry point
Server/GuessNumber.Application/  Business services
Server/GuessNumber.Domain/       Domain entities
Server/GuessNumber.Infrastructure/ EF Core + security
tests/GuessNumber.Application.Tests/ Unit tests
docker-compose.yml               Local PostgreSQL
```

## Local Development

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker Desktop

You do **not** need PostgreSQL installed locally.

### 1. Start PostgreSQL

From the repository root:

```bash
docker compose up -d postgres
```

Verify PostgreSQL is healthy:

```bash
docker compose ps
```

You should see the `guessnumber-postgres` container running and healthy.

Default local credentials:

- Host: `localhost`
- Port: `5432`
- Database: `guessnumber`
- Username: `guessuser`
- Password: `guesspass`

### 2. Run the backend

```bash
cd Server/GuessNumber.API
dotnet run
```

The API runs at `http://localhost:5080`.

Swagger is available in development at `http://localhost:5080/swagger`.

Migrations are applied automatically on startup.

### 3. Run the frontend

```bash
cd Client/GuessNumber.Web
npm install
npm run dev
```

The frontend runs at `http://localhost:5173`.

Create `Client/GuessNumber.Web/.env` if needed:

```env
VITE_API_URL=http://localhost:5080
```

## Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName \
  --project Server/GuessNumber.Infrastructure/GuessNumber.Infrastructure.csproj \
  --startup-project Server/GuessNumber.API/GuessNumber.API.csproj \
  --output-dir Persistence/Migrations
```

Apply migrations locally:

```bash
dotnet ef database update \
  --project Server/GuessNumber.Infrastructure/GuessNumber.Infrastructure.csproj \
  --startup-project Server/GuessNumber.API/GuessNumber.API.csproj
```

The API also applies pending migrations automatically on startup.

## Environment Variables

### Backend (local defaults in `appsettings.json`)

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `FrontendUrl` | Allowed frontend origin for CORS |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `PORT` | Render-provided port in production |

See `Server/GuessNumber.API/appsettings.Example.json` for placeholders.

### Frontend

| Variable | Description |
|---|---|
| `VITE_API_URL` | Backend API base URL |

## Testing

Run unit tests from the repository root:

```bash
dotnet test
```

Tests cover guess evaluation, attempt counting, best score updates, invalid guesses, and cross-user game access protection.

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and set auth cookie |
| POST | `/api/auth/logout` | Yes | Clear auth cookie |
| GET | `/api/auth/me` | Yes | Current user info |
| POST | `/api/game/start` | Yes | Start a new game |
| POST | `/api/game/{gameId}/guess` | Yes | Submit a guess |
| POST | `/api/game/{gameId}/hint` | Yes | Request a hint |
| GET | `/api/health` | No | Health check |

## Bonus Feature: Hint System

After **3 failed attempts** in an active game, users can request a hint.

Hints are generated server-side and never reveal the exact secret number. Examples:

- "The number is odd."
- "The number is even."
- "The number is between 20 and 30."

Each additional hint narrows the range without exposing the answer.

## Security Considerations

- Passwords are hashed with ASP.NET Core `PasswordHasher`
- Password hashes are never returned to the client
- Authentication uses JWT stored in an HttpOnly cookie
- Production cookies use `Secure` and `SameSite=None` for cross-origin deployment
- Game secret numbers are generated with `RandomNumberGenerator`
- All game logic and validation happen on the server
- Protected endpoints require authentication
- Users can only access their own games

## Deployment

Target architecture:

- **Frontend:** Vercel
- **Backend:** Render (Docker)
- **Database:** Neon PostgreSQL

### Step 1 — Create Neon PostgreSQL

1. Create a free account at [https://neon.tech](https://neon.tech)
2. Create a new project
3. Copy the PostgreSQL connection string
4. Keep it safe — you will use it as `ConnectionStrings__DefaultConnection` on Render

Apply migrations to Neon from your machine:

```bash
$env:ConnectionStrings__DefaultConnection="YOUR_NEON_CONNECTION_STRING"
dotnet ef database update \
  --project Server/GuessNumber.Infrastructure/GuessNumber.Infrastructure.csproj \
  --startup-project Server/GuessNumber.API/GuessNumber.API.csproj
```

On Linux/macOS, use `export ConnectionStrings__DefaultConnection=...`.

### Step 2 — Deploy Backend to Render

1. Push this repository to GitHub
2. In Render, create a **Web Service**
3. Connect the GitHub repository
4. Choose **Docker** deployment
5. Set **Dockerfile Path** to `Server/GuessNumber.API/Dockerfile`
6. Set **Root Directory** to the repository root (`.`)
7. Add environment variables:

| Key | Example |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Host=...;Database=...;Username=...;Password=...` |
| `Jwt__Key` | `your-long-random-secret-at-least-32-characters` |
| `Jwt__Issuer` | `GuessNumber` |
| `Jwt__Audience` | `GuessNumber` |
| `FrontendUrl` | `https://your-app.vercel.app` |

Render sets `PORT` automatically.

Verify deployment:

- `https://YOUR-BACKEND.onrender.com/api/health`

### Step 3 — Deploy Frontend to Vercel

1. Import the GitHub repository into Vercel
2. Set **Root Directory** to `Client/GuessNumber.Web`
3. Framework preset: **Vite**
4. Add environment variable:

```env
VITE_API_URL=https://YOUR-BACKEND.onrender.com
```

5. Deploy

Redeploy after changing environment variables.

### Step 4 — Configure CORS

After Vercel gives you the frontend URL, update Render:

```env
FrontendUrl=https://your-app.vercel.app
```

Redeploy the backend so cookies and CORS work across domains.

### Step 5 — Verify Production

Use this checklist:

- [ ] Register a new account
- [ ] Login
- [ ] See best score message
- [ ] Start a game
- [ ] Make higher/lower guesses
- [ ] Request a hint after 3 attempts
- [ ] Complete a game
- [ ] Verify best score updates
- [ ] Logout
- [ ] Login again and confirm best score persists
- [ ] Confirm unauthenticated API calls return 401

## Backend Docker Image (local test)

From the repository root:

```bash
docker build -f Server/GuessNumber.API/Dockerfile -t guessnumber-api .
```

## Database Schema

**Users**

| Column | Type | Notes |
|---|---|---|
| Id | uuid | Primary key |
| Email | varchar(256) | Unique |
| PasswordHash | text | Hashed password |
| BestScore | int? | Lowest guess count |
| CreatedAt | timestamp | UTC |

**Games**

| Column | Type | Notes |
|---|---|---|
| Id | uuid | Primary key |
| UserId | uuid | FK to Users |
| SecretNumber | int | Server-side only |
| AttemptCount | int | Number of guesses |
| IsCompleted | bool | Game finished |
| HintsUsed | int | Hint count |
| CreatedAt | timestamp | UTC |

## Manual Deployment Steps Summary

1. Create Neon project and copy connection string
2. Run EF migrations against Neon
3. Deploy backend to Render using the Dockerfile
4. Configure backend environment variables on Render
5. Deploy frontend to Vercel with `VITE_API_URL`
6. Update `FrontendUrl` on Render with the Vercel URL
7. Run the production verification checklist

## License

Assessment project — use as needed for interview evaluation.
