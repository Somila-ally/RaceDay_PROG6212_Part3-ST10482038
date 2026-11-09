# Before you submit RaceDay

Use this list after copying the project into your own repository. Only mark an item done after you have checked it yourself.

## 1. Keep the real GitHub history

- Put these project files into the repository that already contains your Part 2 work, if that is where your 23 meaningful commits live. A ZIP has no Git history.
- Keep your existing commits and make any new commits from your own account with clear messages about actual changes. Do not split one change into artificial commits to inflate the count.
- Push the updated solution to the branch you intend to submit and copy the repository URL into your assessment submission.

## 2. Run and capture the current build and tests

From the repository root, run:

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

Resolve any errors before recording. Capture the actual final test summary; do not describe tests as passing if they did not pass.

## 3. Capture CI and Swagger evidence

- Push the final code and wait for the current GitHub Actions workflow to finish.
- Confirm that restore, build, tests, and both API and MVC Docker image steps are green.
- Capture the Actions page showing the workflow name, branch/commit and successful result. Save it as `docs/evidence/ci-green.png` and embed it in the README.
- Start the API, open `/swagger`, and capture the route groups and descriptions. Save it as `docs/evidence/swagger-ui.png` and embed it in the README.
- Do not use an old green run as evidence for code that has not been checked by that run.

## 4. Configure Azure and test uploads

Set `AzureBlobStorage__ConnectionString` and `AzureBlobStorage__ContainerName` in the API process environment or a local secret store. Upload a test banner and profile image from the website, then show that both display. Never show or commit the connection string.

## 5. Run both Docker images and check the website

Build both API and MVC images using the README commands. Confirm the API container exposes Swagger and can reach the configured database. Confirm the MVC container serves the site and reaches the API using `ApiSettings__BaseUrl`. For local Docker, `host.docker.internal` can reach an API running on the host; two containers need a shared Docker network and the API container name as the MVC base URL.

## 6. Record the presentation

Use [`VIDEO_DEMO_SCRIPT.md`](VIDEO_DEMO_SCRIPT.md). Demonstrate the latest code, both roles, one enrolment and result flow, Swagger, passing test output, Azure upload and Docker when each is available. Upload the recording as unlisted, watch it once, then add its real URL to the README and assessment submission.

## Final check

- Remove any sample credentials, secrets and unused placeholder text.
- Confirm the README images and video link open.
- Open the ZIP or repository one last time and make sure it contains the solution, workflow, source, migration and documentation.
