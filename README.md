# RaceDay — PROG6212 Parts 2 and 3

RaceDay is a race and event management solution. Part 2 provides an ASP.NET Core REST API using Entity Framework Core and SQL Server. Part 3 provides an ASP.NET Core MVC website that calls the API; the website does not access the database directly.

## Architecture

```text
Browser → RaceDay.Mvc website → RaceDay.Api → SQL Server
                                      └──────→ Azure Blob Storage (optional image uploads)
```

The API owns authentication, session state, database access, role checks and event ownership checks. The MVC app forwards the API session cookie when it makes authenticated requests.

## Features

- Organiser and Participant registration and login; passwords are stored as salted hashes.
- Organiser event and category management, enrolment review, status updates and results entry.
- Participant event browsing, enrolment, profile editing and result viewing.
- API validation, role-based access, ownership checks, EF Core migrations and Swagger documentation.
- Optional JPEG, PNG and WebP banner/profile uploads through Azure Blob Storage.
- xUnit integration tests using an isolated EF Core InMemory database.

## Solution layout

```text
RaceDay.Api/                REST API, EF Core models, migrations and controllers
RaceDay.Contracts/          Shared API request and response DTOs
RaceDay.Api.Tests/          API integration tests
RaceDay.Mvc/                MVC website and API client
docs/                       Database plan, endpoint reference, feedback actions and demo script
.github/workflows/ci.yml    GitHub Actions build, test and Docker-image checks
Dockerfile                  MVC website image
Dockerfile.Api              API image
RaceDay.sln
```

## Requirements

- .NET 8 SDK
- SQL Server Express or LocalDB for local database-backed use
- Docker Desktop for container builds
- An Azure subscription and Azure SQL Database for a cloud database deployment
- An Azure Storage account for image uploads

## Local database and migrations

The development default in `RaceDay.Api/appsettings.json` is SQL Server Express:

```text
Server=.\SQLEXPRESS;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Change it locally with user secrets or the `ConnectionStrings__DefaultConnection` environment variable. Never commit credentials.

From the solution root, restore and create/update the database schema:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
dotnet restore RaceDay.sln
dotnet ef database update --project RaceDay.Api/RaceDay.Api.csproj --startup-project RaceDay.Api/RaceDay.Api.csproj
```

The migrations include the Part 1 schema plus the event fields and image URL fields. See [`docs/RaceDay_Database.sql`](docs/RaceDay_Database.sql) and [`docs/RaceDay_ERD.pdf`](docs/RaceDay_ERD.pdf).

## Run locally

Start the API in one PowerShell window:

```powershell
dotnet run --project RaceDay.Api --no-launch-profile --urls http://0.0.0.0:5100
```

Open [http://localhost:5100/swagger](http://localhost:5100/swagger). Register and log in there before trying protected API actions.

Start the MVC website in a second window:

```powershell
dotnet run --project RaceDay.Mvc
```

The MVC app defaults to `http://localhost:5100/` for its API base URL. Set `ApiSettings__BaseUrl` if the API listens at a different URL.

## Docker images and containers

Build the API and MVC images separately from the solution root:

```powershell
docker build -f Dockerfile.Api -t raceday-api:part3 .
docker build -f Dockerfile -t raceday-mvc:part3 .
```

The API listens on port `8080` inside its container. The MVC container also listens on
port `8080`. Run both containers on the same Docker network:

```powershell
docker network create raceday-network
docker run -d --name raceday-api --network raceday-network -p 5100:8080 raceday-api:part3
docker run -d --name raceday-mvc --network raceday-network -p 5000:8080 `
  -e "ApiSettings__BaseUrl=http://raceday-api:8080/" raceday-mvc:part3
```

Open the website at <http://localhost:5000> and API Swagger at
<http://localhost:5100/swagger/index.html>. The API container needs a reachable SQL
Server database; its checked-in SQL Express setting is intended for local development,
not for a Linux container. Use environment variables or an env file for cloud settings.

Useful Docker commands:

```powershell
docker ps
docker logs -f raceday-api
docker logs -f raceday-mvc
docker stop raceday-mvc raceday-api
docker rm raceday-mvc raceday-api
```

For local Docker Hub publishing, tag each image with your own Docker Hub ID, sign in, then push both tags:

```powershell
docker tag raceday-api:part3 <docker-id>/raceday-api:part3
docker tag raceday-mvc:part3 <docker-id>/raceday-mvc:part3
docker login -u <docker-id>
docker push <docker-id>/raceday-api:part3
docker push <docker-id>/raceday-mvc:part3
```

Use a Docker Hub access token with **Read & Write** permission. Keep the API and MVC in separate Container Apps. The API container listens on port 8080. Configure the MVC app's `ApiSettings__BaseUrl` to the public API base URL, ending in `/`.

## Azure deployment and database

The API's checked-in development connection string points to local SQL Express and cannot be used by a cloud container. Create an Azure SQL Database, apply the migrations to it, store its connection string as a Container App secret, and reference that secret as `ConnectionStrings__DefaultConnection` in the API app. Configure the MVC app with `ApiSettings__BaseUrl` set to the API app's HTTPS URL.

Follow [`docs/AZURE_DEPLOYMENT_GUIDE.md`](docs/AZURE_DEPLOYMENT_GUIDE.md) for portal steps, settings and verification. Azure SQL and Azure Storage can incur charges; check the portal estimate and remove unused resources when finished.

## Azure Blob Storage uploads

Set `AzureBlobStorage__ConnectionString` and `AzureBlobStorage__ContainerName` in the **API** process through a secret/environment variable. The expected container name is `raceday-images`. The API returns 503 for upload routes when storage is not configured; other API functions can still run. See the Azure deployment guide and test a banner/profile upload only after configuring storage.

For a manual Azure Storage setup:

1. Create an Azure Storage account using Standard performance and locally redundant storage.
2. In the storage account, create a Blob container named `raceday-images`.
3. Use **Blob** anonymous read access if the assessment environment permits it, because the API returns public blob URLs for images displayed by the MVC website.
4. Copy the storage account connection string from **Security + networking** → **Access keys**.
5. Configure the API without committing the secret:

```powershell
$env:AzureBlobStorage__ConnectionString = Read-Host "Paste the Azure Blob connection string"
$env:AzureBlobStorage__ContainerName = "raceday-images"
```

The API accepts JPEG, PNG and WebP files up to 5 MB. Event banners are uploaded through
`POST /api/images/events/{eventId}/banner`; profile images use
`POST /api/images/profile`. Never put a storage key or connection string in this
README, source control, screenshots or the video.

For Docker or Azure Container Apps, provide the same settings as environment variables
using the double-underscore names. A private env file can contain:

```text
ConnectionStrings__DefaultConnection=<Azure SQL connection string>
AzureBlobStorage__ConnectionString=<Azure Blob connection string>
AzureBlobStorage__ContainerName=raceday-images
```

Keep the file outside Git and add its filename to `.gitignore`.

## API endpoint groups

Swagger documents the complete API at `/swagger/index.html`. The main endpoint groups
are:

- `/api/auth` — registration, login and logout.
- `/api/events` — event creation, editing, browsing and ownership checks.
- `/api/events/{eventId}/categories` — category management.
- `/api/enrolments` — participant enrolment and organiser entry review.
- `/api/results` — finish times and positions for completed events.
- `/api/profile` — participant profile details and image upload.
- `/api/images` — event banner and profile image upload routes.

Authentication uses a server-side session cookie. Organiser operations require the
Organiser role, and participants can only access their own entries, profile and results.

## Azure Container Apps configuration

Deploy the API and MVC as separate Container Apps. The API container uses target port
`8080` and should have external ingress. Configure these API settings through Container
App secrets/environment variables:

```text
ConnectionStrings__DefaultConnection=<Azure SQL connection string>
AzureBlobStorage__ConnectionString=<Azure Blob connection string>
AzureBlobStorage__ContainerName=raceday-images
```

Configure the MVC Container App with:

```text
ApiSettings__BaseUrl=https://<api-container-app-hostname>/
```

Use the API HTTPS base URL with a trailing slash. Do not use `localhost` between Azure
Container Apps. The complete portal and smoke-test procedure is in
[`docs/AZURE_DEPLOYMENT_GUIDE.md`](docs/AZURE_DEPLOYMENT_GUIDE.md).

## Tests and CI

Run the automated tests from the solution root:

```powershell
dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

The tests use an isolated EF Core InMemory database and do not require Azure credentials.
The CI workflow is located at [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

The GitHub Actions workflow restores and builds the solution, runs the integration tests, and builds both API and MVC container images. A workflow file in a ZIP is not proof of a successful hosted run: push it to your GitHub repository, confirm the run is green, and capture the real run URL/screenshot. A ZIP also does not preserve Git commit history.

## Completed project change summary

The following items summarise the implemented project work. They are listed as a
project summary and do not replace the actual Git commit history:

1. Added the RaceDay solution structure.
2. Added the ASP.NET Core REST API project.
3. Added the shared contracts project.
4. Added the ASP.NET Core MVC website.
5. Added Entity Framework Core database access.
6. Added the SQL Server development connection configuration.
7. Added database migrations.
8. Added user registration.
9. Added session-based login and logout.
10. Added password hashing.
11. Added Organiser and Participant roles.
12. Added organiser event management.
13. Added event category management.
14. Added participant enrolment.
15. Added organiser entry review.
16. Added race result recording and validation.
17. Added ownership and authorisation checks.
18. Added participant profile management.
19. Added Azure Blob event-banner uploads.
20. Added Azure Blob profile-image uploads.
21. Added Swagger API documentation.
22. Added API integration tests.
23. Added Dockerfiles for the API and MVC applications.
24. Added GitHub Actions, deployment documentation and assessment guidance.

## Assessment evidence still to supply

- A real GitHub repository URL with the required meaningful commit history.
- A successful GitHub Actions run for the submitted commit.
- Current Swagger and test-result screenshots.
- A narrated video recording uploaded to the required location, with its real link in the submission. The script in [`docs/VIDEO_DEMO_SCRIPT.md`](docs/VIDEO_DEMO_SCRIPT.md) is a guide, not a substitute for the video.
- Live Azure Blob upload evidence if you claim that feature as demonstrated.

Use [`docs/SELF_COMPLETION_CHECKLIST.md`](docs/SELF_COMPLETION_CHECKLIST.md) before submission. This package cannot manufacture external GitHub history, hosted workflow evidence, Azure credentials, or a recording.

For an item-by-item response to the supplied Part 3 assessment, see [`docs/PART3_FEEDBACK_ACTIONS.md`](docs/PART3_FEEDBACK_ACTIONS.md).
