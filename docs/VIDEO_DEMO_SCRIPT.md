# RaceDay video walkthrough (about 8–10 minutes)

Record the actual running application and narrate in your own words. Do not show passwords, Azure keys or other secrets. Keep the upload unlisted and paste its URL into the README and assessment submission.

1. **Introduction (30 seconds):** State the project name, PROG6212 part, your name/student number and what the system does.
2. **Solution structure (60 seconds):** Show `RaceDay.Api`, shared contracts, integration tests and `RaceDay.Mvc`. Explain that MVC calls the API and never talks directly to SQL Server.
3. **Database and API (90 seconds):** Show the Part 1 ERD/SQL, EF Core migration, then open Swagger. Point out session-based authentication, role checks, request/response descriptions and event/category/enrolment/result routes.
4. **Organiser flow (2 minutes):** Register/sign in as an Organiser. Create an event, add/edit a category, view entries, show ownership boundaries and attempt result entry before the event date (the API should reject it). Change the event date to the past, enrol a participant, then record a finish time and place.
5. **Participant flow (2 minutes):** Sign in as a Participant. Browse the event, choose a category, show My Entries and My Results, update the profile. Demonstrate that organiser-only screens are unavailable to this role.
6. **Image storage (60 seconds):** With Azure settings supplied privately, upload an event banner and profile image and show that their URLs render. Do not display the connection string.
7. **Automated tests and CI (60 seconds):** Run `dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj`, show the passing summary, then open the real green GitHub Actions run and show the workflow file.
8. **Docker (60 seconds):** Show the Docker build and run commands from the README. Open the MVC website served by the container and demonstrate it can still reach the API.
9. **Close (15 seconds):** Summarise the roles, main API resources and where the source is hosted.
