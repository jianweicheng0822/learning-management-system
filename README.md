# Learning Management System (LMS) - Backend API

A production-ready REST API backend for a Learning Management System built with ASP.NET Core and MySQL.

## Tech Stack

- **Framework:** ASP.NET Core (.NET 10)
- **Database:** MySQL with Entity Framework Core (Pomelo provider)
- **Authentication:** ASP.NET Identity + JWT Bearer tokens
- **API Documentation:** Swagger / OpenAPI
- **Architecture:** MVC + Service layer pattern

## System Modules

### 1. User Management
- Student & Instructor registration/login
- JWT-based authentication
- Role-based authorization (Admin, Instructor, Student)
- Profile management

### 2. Course Management
- Instructors create/update/delete courses
- Students enroll/unenroll from courses
- View all courses or filtered by instructor/enrollment

### 3. Assignment System
- Instructors create assignments with due dates per course
- Students submit assignments (text or file path)
- One submission per student per assignment

### 4. Grading System
- Instructors grade student submissions (0-100 scale)
- Feedback support
- Students view their grades per course

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

## Project Structure

```
/Controllers      API endpoints
/Models           Entity Framework entity models
/Data             DbContext and database configuration
/DTOs             Request/Response data transfer objects
/Services         Business logic layer (interfaces + implementations)
/Middleware        Global exception handling
```

## API Endpoints

### Auth
| Method | Endpoint              | Auth     | Description        |
|--------|-----------------------|----------|--------------------|
| POST   | /api/auth/register    | Public   | Register user      |
| POST   | /api/auth/login       | Public   | Login, get JWT     |
| GET    | /api/auth/profile     | Any      | Get own profile    |
| PUT    | /api/auth/profile     | Any      | Update profile     |

### Courses
| Method | Endpoint                    | Auth       | Description          |
|--------|-----------------------------|------------|----------------------|
| GET    | /api/courses                | Public     | List all courses     |
| GET    | /api/courses/{id}           | Public     | Course details       |
| POST   | /api/courses                | Instructor | Create course        |
| PUT    | /api/courses/{id}           | Instructor | Update course        |
| DELETE | /api/courses/{id}           | Instructor | Delete course        |
| POST   | /api/courses/{id}/enroll    | Student    | Enroll in course     |
| DELETE | /api/courses/{id}/enroll    | Student    | Unenroll             |
| GET    | /api/courses/my-courses     | Instructor | My taught courses    |
| GET    | /api/courses/enrolled       | Student    | My enrolled courses  |

### Assignments
| Method | Endpoint                                      | Auth       | Description          |
|--------|-----------------------------------------------|------------|----------------------|
| GET    | /api/courses/{courseId}/assignments            | Any        | List assignments     |
| GET    | /api/courses/{courseId}/assignments/{id}       | Any        | Assignment details   |
| POST   | /api/courses/{courseId}/assignments            | Instructor | Create assignment    |
| PUT    | /api/courses/{courseId}/assignments/{id}       | Instructor | Update assignment    |
| DELETE | /api/courses/{courseId}/assignments/{id}       | Instructor | Delete assignment    |

### Submissions
| Method | Endpoint                                          | Auth       | Description            |
|--------|---------------------------------------------------|------------|------------------------|
| POST   | /api/assignments/{assignmentId}/submissions       | Student    | Submit assignment      |
| GET    | /api/assignments/{assignmentId}/submissions       | Instructor | View all submissions   |
| GET    | /api/assignments/{assignmentId}/submissions/{id}  | Any        | View submission        |
| GET    | /api/submissions/mine                             | Student    | My submissions         |

### Grades
| Method | Endpoint                                | Auth       | Description          |
|--------|-----------------------------------------|------------|----------------------|
| POST   | /api/submissions/{submissionId}/grade   | Instructor | Grade submission     |
| PUT    | /api/submissions/{submissionId}/grade   | Instructor | Update grade         |
| GET    | /api/courses/{courseId}/grades          | Student    | My grades in course  |

## Getting Started

### Prerequisites
- .NET 10 SDK
- MySQL Server

### Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/YOUR_USERNAME/lms-system.git
   cd lms-system
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
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Open Swagger UI:** `https://localhost:5001/swagger`

### Example Request/Response

**Register:**
```bash
POST /api/auth/register
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Pass123",
  "role": "Student"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expiration": "2026-03-12T10:00:00Z",
    "user": {
      "id": "abc-123",
      "fullName": "John Doe",
      "email": "john@example.com",
      "roles": ["Student"]
    }
  }
}
```

## Linux Deployment (Ubuntu)

```bash
# Install .NET runtime
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Install MySQL
sudo apt-get install -y mysql-server
sudo mysql_secure_installation

# Publish the app
dotnet publish -c Release -o /var/www/lms

# Create systemd service
sudo tee /etc/systemd/system/lms.service > /dev/null <<EOF
[Unit]
Description=LMS API

[Service]
WorkingDirectory=/var/www/lms
ExecStart=/usr/bin/dotnet /var/www/lms/Learning\ Management\ System.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl enable lms
sudo systemctl start lms
```
