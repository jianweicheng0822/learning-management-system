# Learning Management System (LMS)

A full-stack Learning Management System built with ASP.NET Core 9 MVC, Razor Views, and MySQL. Supports three user roles (Admin, Instructor, Student) with cookie-based authentication.

## Live Demo

**URL:** http://18.234.45.126

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@lms.com | Admin123! |
| Instructor | instructor@lms.com | Teach123! |
| Student | student@lms.com | Learn123! |

## Tech Stack

- **Framework:** ASP.NET Core 9 MVC with Razor Views
- **Database:** MySQL 8.0 with Entity Framework Core (Pomelo provider)
- **Authentication:** Cookie-based with ASP.NET Identity
- **Authorization:** Role-based access control (Admin, Instructor, Student)
- **Frontend:** Bootstrap 5, Razor Tag Helpers
- **Deployment:** Docker + Docker Compose on AWS EC2
- **CI/CD:** GitHub Actions (build, push to Docker Hub, deploy to EC2)

## Features

- **Courses** — Browse, create, edit, delete courses; enroll/unenroll students
- **Assignments** — CRUD operations within course context, due date tracking
- **Submissions** — Students submit work (text); instructors review submissions
- **Grades** — Instructors grade submissions (0–100 with feedback); students view grades
- **Accounts** — Registration, login/logout, profile management
- **Admin** — Full access to all resources across the system

## Project Structure

```
Learning Management System/
├── Controllers/           # MVC controllers
├── Views/                 # Razor views (Home, Account, Course, Assignment, Submission, Grade)
├── ViewModels/            # View models with validation
├── Models/                # EF Core entity models
├── Services/              # Business logic (interfaces + implementations)
├── Data/                  # DbContext, migrations, seed data
├── Middleware/             # Global exception handling
└── wwwroot/               # Static assets (CSS, JS)

Dockerfile                 # Multi-stage build (SDK → runtime)
docker-compose.yml         # App + MySQL containers
.github/workflows/         # CI/CD pipeline
LMS.Tests/                 # Unit tests
```

## Database Schema

```
User (ASP.NET Identity)
 ├── Instructor → Courses (one-to-many)
 └── Student → Enrollments (one-to-many)

Course
 ├── Assignments (one-to-many)
 └── Enrollments (one-to-many)

Assignment
 └── Submissions (one-to-many)

Submission
 └── Grade (one-to-one)
```

## Running Locally

### With Docker (recommended)

```bash
git clone https://github.com/jianweicheng0822/learning-management-system.git
cd learning-management-system
cp .env.example .env       # edit passwords if you want
docker compose up --build
```

Visit http://localhost and log in with any demo account above.

### Without Docker

Prerequisites: .NET 9 SDK, MySQL Server

```bash
cd "Learning Management System"
# Set your connection string in appsettings.json or user secrets
dotnet run
```

The app auto-runs migrations and seeds demo data on startup when `SEED_DEMO_DATA=true`.

## Deployment

The app deploys to AWS EC2 via GitHub Actions. On every push to `main`:

1. GitHub Actions builds a Docker image and pushes it to Docker Hub
2. SSHs into EC2 and pulls the new image
3. Restarts the app container (MySQL data persists across deploys)

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `DOCKERHUB_USERNAME` | Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub access token |
| `EC2_HOST` | EC2 public IP address |
| `EC2_USER` | SSH user (e.g. `ubuntu`) |
| `EC2_SSH_KEY` | PEM private key contents |

## Routes

| Route | Description |
|-------|-------------|
| `/` | Home / Dashboard |
| `/Account/Login` | Login page |
| `/Account/Register` | Registration page |
| `/Account/Profile` | User profile |
| `/Course` | Browse all courses |
| `/Course/Details/{id}` | Course details with assignments |
| `/Course/Create` | Create course (Instructor/Admin) |
| `/Course/MyCourses` | Instructor's courses |
| `/Course/Enrolled` | Student's enrolled courses |
| `/Assignment/Details/{id}` | Assignment details |
| `/Submission/Create?assignmentId={id}` | Submit assignment (Student) |
| `/Submission/MySubmissions` | Student's submissions |
| `/Grade/MyGrades?courseId={id}` | Student's grades for a course |
