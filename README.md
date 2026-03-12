# MiniAuth Blog

Blog platform built with .NET 8 and Angular 21. Includes JWT authentication, posts CRUD, and comments.

## Tech Stack

**Backend:** .NET 8, Entity Framework Core, PostgreSQL, JWT Bearer Auth

**Frontend:** Angular 21 (standalone components, Signals, RxJS), Tailwind CSS

## Project Structure

```
src/
  MiniAuth.Api/            # Controllers, middleware, Program.cs
  MiniAuth.Application/    # Services, DTOs, request models
  MiniAuth.Domain/         # Entities (Post, Comment, User)
  MiniAuth.Infrastructure/ # EF Core, repositories, DbContext
frontend/
  src/app/
    core/                  # Services, guards, interceptors, models
    features/
      auth/                # Login, Register
      posts/               # Post list, detail, create modal
```

## Running locally

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker (for PostgreSQL)

### Database

```bash
docker compose up -d
dotnet ef database update --project src/MiniAuth.Infrastructure --startup-project src/MiniAuth.Api
```

### Backend

```bash
dotnet run --project src/MiniAuth.Api --urls "http://localhost:5167"
```

### Frontend

```bash
cd frontend
npm install
ng serve
```

Open http://localhost:4200

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/auth/register | No | Create account |
| POST | /api/auth/login | No | Login (returns JWT) |
| GET | /api/posts | Yes | List posts (paginated, filter by title) |
| GET | /api/posts/:id | Yes | Get post by ID |
| POST | /api/posts | Yes | Create post |
| PUT | /api/posts/:id | Yes | Update post |
| DELETE | /api/posts/:id | Yes | Delete post |
| GET | /api/posts/:id/comments | No | List comments |
| POST | /api/posts/:id/comments | Yes | Add comment |
| DELETE | /api/posts/:id/comments/:cid | Yes | Delete own comment |
