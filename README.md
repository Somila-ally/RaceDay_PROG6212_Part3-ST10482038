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

The migrations include the Part 1 schema plus Part 2 event fields and the Part 3 image URL fields. See [`docs/Part2_Schema_Notes.md`](docs/Part2_Schema_Notes.md) and [`docs/RaceDay_Database.sql`](docs/RaceDay_Database.sql).

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

## Docker images

Build the API and MVC images separately from the solution root:

```powershell
docker build -f Dockerfile.Api -t raceday-api:latest .
docker build -f Dockerfile -t raceday-mvc:latest .
```

For local Docker Hub publishing, tag each image with your own Docker Hub ID, sign in, then push both tags:

```powershell
docker tag raceday-api:latest <docker-id>/raceday-api:latest
docker tag raceday-mvc:latest <docker-id>/raceday-mvc:latest
docker login -u <docker-id>
docker push <docker-id>/raceday-api:latest
docker push <docker-id>/raceday-mvc:latest
```

Use a Docker Hub access token with **Read & Write** permission. Keep the API and MVC in separate Container Apps. The API container listens on port 8080. Configure the MVC app's `ApiSettings__BaseUrl` to the public API base URL, ending in `/`.

## Azure deployment and database

The API's checked-in development connection string points to local SQL Express and cannot be used by a cloud container. Create an Azure SQL Database, apply the migrations to it, store its connection string as a Container App secret, and reference that secret as `ConnectionStrings__DefaultConnection` in the API app. Configure the MVC app with `ApiSettings__BaseUrl` set to the API app's HTTPS URL.

Follow [`docs/AZURE_DEPLOYMENT_GUIDE.md`](docs/AZURE_DEPLOYMENT_GUIDE.md) for portal steps, settings and verification. Azure SQL and Azure Storage can incur charges; check the portal estimate and remove unused resources when finished.

## Azure Blob Storage uploads

Set `AzureBlobStorage__ConnectionString` and `AzureBlobStorage__ContainerName` in the **API** process through a secret/environment variable. The expected container name is `raceday-images`. The API returns 503 for upload routes when storage is not configured; other API functions can still run. See the Azure deployment guide and test a banner/profile upload only after configuring storage.

## Tests and CI

Run the automated tests from the solution root:

```powershell
dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

The GitHub Actions workflow restores and builds the solution, runs the integration tests, and builds both API and MVC container images. A workflow file in a ZIP is not proof of a successful hosted run: push it to your GitHub repository, confirm the run is green, and capture the real run URL/screenshot. A ZIP also does not preserve Git commit history.

## Assessment evidence still to supply

- A real GitHub repository URL with the required meaningful commit history.
- A successful GitHub Actions run for the submitted commit.
- Current Swagger and test-result screenshots.
- A narrated video recording uploaded to the required location, with its real link in the submission. The script in [`docs/VIDEO_DEMO_SCRIPT.md`](docs/VIDEO_DEMO_SCRIPT.md) is a guide, not a substitute for the video.
- Live Azure Blob upload evidence if you claim that feature as demonstrated.

Use [`docs/SELF_COMPLETION_CHECKLIST.md`](docs/SELF_COMPLETION_CHECKLIST.md) before submission. This package cannot manufacture external GitHub history, hosted workflow evidence, Azure credentials, or a recording.

For an item-by-item response to the supplied Part 3 assessment, see [`docs/PART3_FEEDBACK_ACTIONS.md`](docs/PART3_FEEDBACK_ACTIONS.md).
