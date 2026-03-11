# Learning Management System (LMS)

A full-stack Learning Management System built with ASP.NET Core MVC, Razor Views, and MySQL. Features cookie-based authentication for the web UI and JWT authentication for the REST API.

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 10) with Razor Views
- **Database:** MySQL with Entity Framework Core (Pomelo provider)
- **Authentication:** Cookie-based (MVC) + JWT Bearer (API)
- **Authorization:** ASP.NET Identity with role-based access (Admin, Instructor, Student)
- **Frontend:** Bootstrap 5, Razor Tag Helpers
- **API Documentation:** Swagger / OpenAPI
- **Deployment:** Linux (Ubuntu) with nginx reverse proxy, HTTPS via Let's Encrypt

## Features

### Web Interface (MVC + Razor Views)
- **Home** — Landing page with role-based dashboard links
- **Account** — Registration, login/logout, profile management with cookie authentication
- **Courses** — Browse, create, edit, delete courses; enroll/unenroll students
- **Assignments** — CRUD operations within course context, due date tracking
- **Submissions** — Students submit work; instructors review all submissions
- **Grades** — Instructors grade submissions (0–100); students view grades with course averages

### REST API (JWT Authentication)
- Full CRUD API endpoints for all resources
- Swagger UI for interactive API testing
- JWT Bearer token authentication

## Project Structure

```
Learning Management System/
├── Controllers/           # MVC controllers (web) + API controllers
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── CourseController.cs
│   ├── AssignmentController.cs
│   ├── SubmissionController.cs
│   ├── GradeController.cs
│   ├── AuthController.cs          # API
│   ├── CoursesController.cs       # API
│   ├── AssignmentsController.cs   # API
│   ├── SubmissionsController.cs   # API
│   └── GradesController.cs        # API
├── Views/
│   ├── Shared/            # Layout, navigation, error page
│   ├── Home/              # Landing page, privacy
│   ├── Account/           # Login, register, profile
│   ├── Course/            # CRUD views, enrollment
│   ├── Assignment/        # CRUD views
│   ├── Submission/        # Submit, review, my submissions
│   └── Grade/             # Grade, edit grade, my grades
├── ViewModels/            # MVC view models with validation
├── Models/                # EF Core entity models
├── DTOs/                  # API request/response objects
├── Services/              # Business logic (interfaces + implementations)
├── Data/                  # DbContext configuration
├── Middleware/             # Global exception handling
├── wwwroot/css/           # Static assets
└── Deployment/            # nginx, systemd, deploy script
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

## Getting Started

### Prerequisites
- .NET 10 SDK
- MySQL Server

### Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/jianweicheng0822/learning-management-system.git
   cd learning-management-system
   ```

2. **Configure the database connection** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=LmsDb;User=root;Password=YOUR_PASSWORD;"
     }
   }
   ```

3. **Run migrations:**
   ```bash
   cd "Learning Management System"
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access the app:**
   - Web UI: `https://localhost:5001`
   - Swagger API: `https://localhost:5001/swagger`

## MVC Routes

| Route | Description |
|-------|-------------|
| `/` | Home / Dashboard |
| `/Account/Login` | Login page |
| `/Account/Register` | Registration page |
| `/Account/Profile` | User profile |
| `/Course` | Browse all courses |
| `/Course/Details/{id}` | Course details with assignments |
| `/Course/Create` | Create course (Instructor) |
| `/Course/MyCourses` | Instructor's courses |
| `/Course/Enrolled` | Student's enrolled courses |
| `/Assignment/Details/{id}` | Assignment details |
| `/Submission/Create?assignmentId={id}` | Submit assignment (Student) |
| `/Submission/MySubmissions` | Student's submissions |
| `/Grade/MyGrades?courseId={id}` | Student's grades for a course |

## API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/register | Public | Register user |
| POST | /api/auth/login | Public | Login, get JWT |
| GET | /api/auth/profile | Bearer | Get own profile |
| PUT | /api/auth/profile | Bearer | Update profile |

### Courses
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/courses | Public | List all courses |
| GET | /api/courses/{id} | Public | Course details |
| POST | /api/courses | Instructor | Create course |
| PUT | /api/courses/{id} | Instructor | Update course |
| DELETE | /api/courses/{id} | Instructor | Delete course |
| POST | /api/courses/{id}/enroll | Student | Enroll in course |
| DELETE | /api/courses/{id}/enroll | Student | Unenroll |
| GET | /api/courses/my-courses | Instructor | My taught courses |
| GET | /api/courses/enrolled | Student | My enrolled courses |

### Assignments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/courses/{courseId}/assignments | Bearer | List assignments |
| GET | /api/courses/{courseId}/assignments/{id} | Bearer | Assignment details |
| POST | /api/courses/{courseId}/assignments | Instructor | Create assignment |
| PUT | /api/courses/{courseId}/assignments/{id} | Instructor | Update assignment |
| DELETE | /api/courses/{courseId}/assignments/{id} | Instructor | Delete assignment |

### Submissions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/assignments/{assignmentId}/submissions | Student | Submit assignment |
| GET | /api/assignments/{assignmentId}/submissions | Instructor | View submissions |
| GET | /api/assignments/{assignmentId}/submissions/{id} | Bearer | View submission |
| GET | /api/submissions/mine | Student | My submissions |

### Grades
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/submissions/{submissionId}/grade | Instructor | Grade submission |
| PUT | /api/submissions/{submissionId}/grade | Instructor | Update grade |
| GET | /api/courses/{courseId}/grades | Student | My grades in course |

## Deployment (Linux / Ubuntu)

### Quick Deploy

```bash
# Run the automated deployment script
chmod +x Deployment/deploy.sh
sudo ./Deployment/deploy.sh
```

### Manual Setup

1. **Install prerequisites:**
   ```bash
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-10.0 mysql-server nginx certbot python3-certbot-nginx
   sudo mysql_secure_installation
   ```

2. **Publish the application:**
   ```bash
   dotnet publish "Learning Management System" -c Release -o /var/www/lms
   sudo chown -R www-data:www-data /var/www/lms
   ```

3. **Configure systemd service:**
   ```bash
   sudo cp Deployment/lms.service /etc/systemd/system/lms.service
   sudo systemctl daemon-reload
   sudo systemctl enable lms
   sudo systemctl start lms
   ```

4. **Configure nginx with HTTPS:**
   ```bash
   # Update domain in Deployment/nginx/lms.conf
   sudo cp Deployment/nginx/lms.conf /etc/nginx/sites-available/lms
   sudo ln -s /etc/nginx/sites-available/lms /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl reload nginx

   # Get SSL certificate
   sudo certbot --nginx -d your-domain.com
   ```

5. **Verify:**
   ```bash
   sudo systemctl status lms
   curl https://your-domain.com
   ```
