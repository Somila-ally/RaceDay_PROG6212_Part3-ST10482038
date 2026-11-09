# Part 3 feedback: action and evidence map

This map responds to the supplied 62/100 feedback. It separates files included in the project from checks and evidence that must be produced from the student's own Azure/GitHub accounts. It does not claim a mark or substitute for the assessor's verification.

| Criterion | Project support included | Student evidence still required |
|---|---|---|
| Correct submission (4/5) | One solution root, README, Azure deployment guide, endpoint reference, assessment checklist and video script. | Add the real repository URL and submitted evidence links; review the final upload for the required naming/format. |
| GitHub and CI/CD (2/10) | `.github/workflows/ci.yml` restores and builds the solution, runs tests, and builds both API and MVC images. | Push this exact version to the student's GitHub repository; preserve genuine commit history; confirm the run for the submitted commit is green and include its URL or screenshot. A ZIP cannot carry the hosted run or Git history. |
| Authentication and role navigation (9/10) | API registration/login, hashed passwords, server-side sessions, role checks; MVC role-specific navigation and protected flows. | Demonstrate both roles on the running website and show the API responses. |
| Organiser interface (13/15) | Organiser event/category management, enrolment review/status, profile and results pages. | Demonstrate the full flow against the deployed API/database; capture visible successful responses/pages. |
| Participant interface (12/15) | Event browsing, enrolment, results and profile pages. | Demonstrate participant registration, entry, profile and results end to end against the deployed API/database. |
| Azure Blob Storage (7/10) | API upload endpoints and MVC upload forms; configuration documented. | Set Azure Storage credentials as API secrets and demonstrate a successful banner and profile-image upload. |
| Docker (7/10) | Separate multi-stage Dockerfiles: `Dockerfile.Api` for the API and `Dockerfile` for MVC; CI builds both. | Run/push both images and capture the successful build/deployment evidence. Keep the API, MVC and database configuration separate. |
| UI design (8/10) | Responsive MVC pages with consistent styling. | Capture and submit screenshots of the actual running interface at the required viewport(s). |
| Video presentation (0/15) | [`VIDEO_DEMO_SCRIPT.md`](VIDEO_DEMO_SCRIPT.md) supplies a narrated walkthrough order. | Record the required narrated video, upload it to the accepted location, verify the link, and submit that real link. The script is not the recording. |

## Suggested evidence order

1. Show the website URL and role-specific sign-in.
2. Register and log in as an Organiser; create an event and category.
3. Register and log in as a Participant; enrol in that event and show the participant view.
4. Return as Organiser; review enrolment and, after the event, record a finishing time and position.
5. Show the API Swagger page and one success plus one validation response.
6. Show successful tests and the real GitHub Actions run for the submitted commit.
7. Demonstrate both image uploads only after Azure Blob Storage has been configured.

Use only genuine screenshots, logs, video and workflow evidence from the final version being submitted.
