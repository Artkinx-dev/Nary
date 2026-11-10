# 72-Hour MVP Execution Plan

**Deadline:** November 12, 2026, at 11:59 PM (WAT).
**Objective:** Deliver a Super Admin workflow (Flutter Web + .NET API) hosted on Vercel and Render/Neon.

## Phase 1: Foundation & Backend (Hours 0 - 24)
*Focus: Get the database running and the API locked down.*

*   **Hour 0-2:** Setup monorepo structure. Initialize Git. Create Neon PostgreSQL database. Save connection strings.
*   **Hour 2-6:** Scaffold .NET Projects (API, Core, Infrastructure). Define Entities and EF Core DbContext. Add explicit Data Annotations.
*   **Hour 6-8:** Create Initial Migration. Seed the default Super Admin user. Apply to Neon DB.
*   **Hour 8-12:** Implement JWT Authentication and the `AuthService`. Test login via Postman.
*   **Hour 12-18:** Build the core Controllers (Faculties, Departments, Users, Sessions). 
*   **Hour 18-20:** Implement the EF Core Audit Trail interceptor.
*   **Hour 20-24:** Finalize Swagger UI. **Critical step:** Run the API and export `swagger.json`. 

## Phase 2: Deployment & Frontend UI (Hours 24 - 48)
*Focus: Get the API on the internet and build the Flutter shell.*

*   **Hour 24-26 (Deployment):** Add Dockerfile to backend. Connect Render to GitHub monorepo (Set root directory to `backend`). Deploy the API. Ensure it connects to Neon.
*   **Hour 26-28:** Create Flutter project in the `frontend` folder. Use OpenAPI generator with `swagger.json` to instantly generate Dart models and API client.
*   **Hour 28-32:** Setup Flutter routing (GoRouter) and State Management (Riverpod or Provider). Build the Login Screen.
*   **Hour 32-36:** *The Render Cold-Start Fix.* Implement the 90-second timeout on the Flutter HTTP client and add the "Waking server..." loading UI to the login screen.
*   **Hour 36-44:** Build the Super Admin Dashboard shell (Sidebar, Appbar). Build DataTables for Faculties and Departments. Implement Creation Dialogs (Forms).
*   **Hour 44-48:** Build DataTables and Forms for User Management and Academic Sessions. 

## Phase 3: Integration, Polish & Submission (Hours 48 - 72)
*Focus: Tie everything together, write docs, and submit.*

*   **Hour 48-54:** Connect the Flutter UI to the generated Dart API client. Ensure JWT is passed in headers for all requests. Handle 401 Unauthorized errors (kick back to login).
*   **Hour 54-58:** Testing. Click through every flow as the evaluator would. Check Neon to ensure data is actually saving. Ensure duplicates fail gracefully with UI snackbars.
*   **Hour 58-60 (Deployment):** Deploy Flutter Web to Vercel or Firebase (Set root directory to `frontend`). 
*   **Hour 60-64:** Generate the Database Schema/ER Diagram (you can use a tool like dbdiagram.io or DBeaver export).
*   **Hour 64-68 (Documentation):** Write the root `README.md`. It must include:
    *   Live App URL.
    *   Test Credentials (Super Admin email/pass).
    *   Architecture explanation (Clean Architecture + Monorepo).
    *   Why you chose your stack.
    *   Explanation of the Render cold-start behavior.
*   **Hour 68-70 (Demonstration):** Record the 3-5 minute Loom video walkthrough. Show the Flutter UI, then show the Neon DB, then show the code structure.
*   **Hour 70-72:** Buffer for unexpected disasters. Submit the final links. Set up cron-job.org to ping your Render API every 14 minutes.