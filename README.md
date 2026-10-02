# AI School Operations Manager — Backend & AI

##  Project Overview

**AI School Operations Manager** is an AI-powered school management system designed to **automate and simplify day-to-day school operations**. It provides a centralized backend for managing students, teachers, parents, attendance, academic performance, fees, admissions, meetings, notifications, and school documents.

The system also integrates **Google Gemini** to provide a natural-language AI assistant that can access real school data through secure backend tools. Users can ask questions such as *“Which students have low attendance?”* or *“Show students with declining academic performance”*, and the AI retrieves the relevant information from the system and provides a meaningful response.

The project combines **ASP.NET Core, SQL Server, JWT-based security, and Generative AI** to create a secure and intelligent platform for automating school operations and supporting data-driven decision-making.

---

## Tech Stack

### Backend

* C# / .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* JWT Bearer Authentication
* Swagger / OpenAPI
* LINQ
* Dependency Injection
* IHttpClientFactory

### AI

* Google Gemini
* Google.GenAI .NET SDK
* Gemini Function Calling
* Custom AI Tool System
* Custom AI Orchestrator

---

##  Backend Architecture

The backend follows a RESTful architecture with clear separation between:

```text
Controllers
    ↓
Services / Business Logic
    ↓
Entity Framework Core
    ↓
SQL Server
```

The backend is responsible for:

* Data management
* Business rules
* Authentication
* Authorization
* Validation
* Analytics
* AI tool execution

---

##  Authentication & Authorization

The application uses **ASP.NET Core Identity + JWT authentication**.

### Roles

* Admin
* Teacher
* Student
* Parent

Authorization is enforced at the backend using:

* Role-based authorization
* Ownership checks
* Controller/action-level authorization

For example, users can only access resources they are authorized to view.

The system also uses a consistent **soft-delete pattern** with `IsActive`, allowing records to be activated/deactivated without permanently deleting historical data.

---

##  Database

The application uses **SQL Server with Entity Framework Core Code First**.

Main entities include:

* ApplicationUser
* Student
* Teacher
* Parent
* Attendance
* AcademicPerformance
* FeeRecord
* Meeting
* Notification
* AdmissionApplication
* Document

The database is managed through EF Core migrations and maintains relationships, foreign keys, indexes, and validation rules.

---

#  Backend Modules

### Students

Provides student management, searching, details, activation/deactivation, and authorization-based access.

### Teachers

Provides teacher management, searching, details, and lifecycle management.

### Parents

Manages parent records and their relationship with students.

### Attendance

Provides attendance records and attendance analytics.

Example:

```text
Students with attendance below 75%
```

### Academic Performance

Manages student performance and provides analytics for:

* Poor performance
* Declining performance

### Fees

Provides:

* Fee records
* Fee summaries
* Outstanding fees
* Overdue fees

### Meetings

Manages meetings between teachers, students, and related users.

### Notifications

Manages school notifications and supports controlled notification creation through the AI system.

### Admissions

Manages admission applications and provides filtering by admission status.

### Documents

Stores school documents and allows controlled retrieval of document information and content.

---

#  AI Integration

The AI assistant is integrated directly into the ASP.NET Core backend.

The AI uses **Google Gemini with native Function Calling**.

Instead of giving Gemini direct access to the database, the system gives it a controlled set of tools.

### Architecture

```text
User Question
     ↓
POST /api/chat
     ↓
ChatController
     ↓
AIOrchestrator
     ↓
Google Gemini
     ↓
Function Call
     ↓
AI Tool
     ↓
Backend API
     ↓
SQL Server
     ↓
Tool Result
     ↓
Gemini
     ↓
Natural-Language Response
```

This allows Gemini to answer questions using **real application data**.

---

#  AI Architecture

The AI layer consists of:

### `ITool`

Common interface implemented by every AI tool.

### `AIToolContext`

Carries the authenticated user's JWT through the AI pipeline.

### `ToolRegistry`

Registers the available tools and creates their Gemini function declarations.

### `AIOrchestrator`

Handles the Gemini conversation and function-calling loop.

It:

1. Sends the user's question and available tools to Gemini.
2. Receives a function call when data/action is required.
3. Executes the selected tool.
4. Sends the result back to Gemini.
5. Continues until Gemini produces the final answer.

### `ToolResult`

Provides a common success/error response structure for tools.

### `ToolHttpHelper`

Creates authenticated HTTP requests from AI tools to backend APIs.

---

#  AI Tools

The project currently contains **20 AI tools**.

### Students & Parents

* `SearchStudentsTool`
* `GetStudentDetailsTool`
* `SearchTeachersTool`
* `SearchParentsTool`
* `GetParentsByStudentTool`

### Attendance & Academics

* `AttendanceTool`
* `PoorPerformanceTool`
* `DecliningPerformanceTool`

### Fees

* `FeeSummaryTool`
* `OutstandingFeesTool`
* `OverdueFeesTool`

### Admissions

* `AdmissionSummaryTool`
* `AdmissionsByStatusTool`
* `AdmissionDetailsTool`

### Documents

* `DocumentCatalogTool`
* `ReadDocumentTool`

### Meetings & Notifications

* `GetMyMeetingsTool`
* `GetMeetingDetailsTool`
* `CreateNotificationTool`
* `GetStudentNotificationsTool`

---

#  Secure AI Architecture

A major part of the AI design is **JWT propagation**.

The AI does not bypass backend authorization.

```text
User JWT
   ↓
ChatController
   ↓
AIOrchestrator
   ↓
AIToolContext
   ↓
AI Tool
   ↓
Backend API + same JWT
   ↓
Authorization
```

Therefore, AI requests are subject to the same backend authorization rules as normal API requests.

The AI also does **not directly query SQL Server**.

---

#  Backend Analytics

The backend contains dedicated services for deterministic analytics.

### AttendanceService

Calculates attendance percentages and supports threshold-based filtering.

### AcademicPerformanceService

Identifies:

* Students performing below a threshold
* Students showing declining performance across exams

This keeps important calculations in backend business logic instead of relying on the LLM.

---

#  AI Chat API

The main AI endpoint is:

```http
POST /api/chat
```

Example request:

```json
{
  "message": "Which students have attendance below 75%?"
}
```

Gemini can select:

```text
AttendanceTool
```

The tool calls the backend attendance API, retrieves the real data, and Gemini converts the result into a natural-language response.

---

#  Example

### User

> Which students are performing poorly in Mathematics?

### AI

```text
Gemini
   ↓
PoorPerformanceTool
   ↓
Academic Performance API
   ↓
Database
   ↓
Results
   ↓
Gemini
   ↓
Natural-language answer
```

The response is based on actual school data rather than generated or assumed information.

---


#  Architecture Highlights

The core design principle is:

> **Gemini decides what information or capability is needed; the backend remains responsible for data, business rules, and authorization.**

```text
                AI School Operations Manager

                       User
                        │
                        ▼
                  ASP.NET Core API
                        │
          ┌─────────────┴─────────────┐
          │                           │
          ▼                           ▼
   Authentication                AI Orchestrator
   & Authorization                     │
          │                            ▼
          │                         Gemini
          │                            │
          │                       Function Call
          │                            │
          │                            ▼
          │                       AI Tool
          │                            │
          └──────────────┬─────────────┘
                         ▼
                    Backend APIs
                         │
                         ▼
                      EF Core
                         │
                         ▼
                    SQL Server
```

---


##  Running the Backend

```bash
dotnet restore
dotnet build
dotnet run
```

Development API:

```text
https://localhost:7003
```

Swagger:

```text
https://localhost:7003/swagger
```

---

##  Future AI Enhancements

Potential future extensions include:

* RAG-based school knowledge retrieval
* Embeddings and semantic search
* Vector database integration
* More advanced AI workflows
* AI memory
* Audit logging
* Production deployment

---

##  Project Focus

This project demonstrates the integration of **traditional backend engineering with Generative AI**:

**ASP.NET Core + SQL Server + Secure APIs + Business Logic + Gemini Function Calling**

The AI is not treated as a replacement for the backend. Instead, it acts as an intelligent interface over a secure, structured school-management system.
