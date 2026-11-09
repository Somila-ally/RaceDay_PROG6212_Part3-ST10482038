# RaceDay Azure deployment and smoke-test guide

This project has two separate web processes. Deploy `RaceDay.Api` as the backend and `RaceDay.Mvc` as the website. The API's checked-in SQL Express connection string is for local development only.

## A. Create Azure SQL Database

1. In Azure Portal, search for **SQL databases** and select **Create**.
2. Select the subscription and the `raceday-resources` resource group. Set the database name to `RaceDayDb`.
3. Under **Server**, select **Create new**. Use a globally unique server name, select UAE North (if available), and create a SQL authentication administrator. Save the administrator name and password privately.
4. Choose a development/lowest-cost configuration that is available in your subscription. Review the price before creating the resource.
5. After deployment, open the SQL server resource and go to **Networking**. Add your current client IP so the local EF migration command can connect. For a short demo, enable **Allow Azure services and resources to access this server** so the Container App can connect. This setting permits Azure services generally, so retain a strong database password and disable it when no longer needed if you can use a more restrictive network rule.
6. Save and copy the fully qualified SQL server name, such as `example.database.windows.net`.

Microsoft setup references: [create a single Azure SQL database](https://learn.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart), [SQL network access controls](https://learn.microsoft.com/en-us/azure/azure-sql/database/network-access-controls-overview).

## B. Apply the EF Core migrations

From PowerShell, change to the repository root (the folder containing `RaceDay.sln`). Build a SQL-auth connection string using your server name and credentials. Paste it only into your own PowerShell prompt; do not commit or share it.

```powershell
$env:ConnectionStrings__DefaultConnection = Read-Host "Paste the Azure SQL connection string"
dotnet ef database update --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
```

Use this connection-string shape, replacing every placeholder:

```text
Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=RaceDayDb;Persist Security Info=False;User ID=<sql-admin>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

The command applies the repository's migrations and creates the RaceDay tables in the Azure database. If it reports a firewall/network error, confirm your client IP is allowed on the SQL server. If it reports a login error, check the SQL administrator credentials.

## C. Configure the backend Container App

The API image is built from `Dockerfile.Api`; its container listens on port 8080.

1. Open the backend Container App in Azure.
2. Add an application secret named `sql-connection` with the full Azure SQL connection string as its value.
3. Go to **Revisions and replicas** → **Create new revision**. Edit the existing API container, keep the API image, and add an environment variable:
   - Name: `ConnectionStrings__DefaultConnection`
   - Source: secret reference `sql-connection`
4. Ensure ingress is **External** with target port **8080**. Save/deploy and wait for the new revision to be Running and Healthy.
5. If Blob uploads are being demonstrated, add `AzureBlobStorage__ConnectionString` as a secret reference and set `AzureBlobStorage__ContainerName` to `raceday-images` on the API container.

Azure references: [Container App secrets](https://learn.microsoft.com/azure/container-apps/manage-secrets), [environment variables](https://learn.microsoft.com/azure/container-apps/environment-variables).

## D. Connect the MVC Container App to the API

The MVC website uses `ApiSettings:BaseUrl`. ASP.NET Core maps this setting to the environment variable `ApiSettings__BaseUrl`.

1. Copy the backend Container App's HTTPS URL from its **Overview** page. Use only the base URL and include a final `/`; do not include `/swagger`.
2. Open the MVC website Container App, then **Revisions and replicas** → **Create new revision**.
3. Edit the existing MVC container and add:
   - Name: `ApiSettings__BaseUrl`
   - Value: `https://<backend-app-hostname>/`
4. Keep the MVC image, ingress and container port unchanged. Save/deploy and wait until the new revision is Running and Healthy.
5. Open the MVC app's `/Auth/Login` page and test registration and login.

Do not set this value to `localhost:5100` in Azure. Inside a container, `localhost` refers to that same container, not the other Container App.

## E. Swagger and functional smoke test

Open the backend URL with `/swagger/index.html` appended. The API has no homepage at `/`; a root-path 404 is expected when no route is mapped there.

1. Expand `POST /api/auth/register`, select **Try it out**, and use a new email for each registration:

```json
{
  "firstName": "Demo",
  "lastName": "Organiser",
  "email": "organiser-demo@example.com",
  "password": "RaceDayTest123!",
  "role": "Organiser"
}
```

Expect **201**. Register a second account with role `Participant` and a different email.

2. Call `POST /api/auth/login` with the same email/password. Expect **200**. The API uses a server-side session cookie; use Swagger at the API host for the subsequent calls so the browser can retain the cookie.

3. Call `GET /api/events` and confirm it returns **200**. Then, as the organiser, use Swagger's schemas to create an event (`POST /api/events`) and category (`POST /api/events/{eventId}/categories`). Use the returned event/category IDs to enrol the participant at `POST /api/enrolments`.

4. Verify profile and result routes from the endpoint list in [`API_ENDPOINT_REFERENCE.md`](API_ENDPOINT_REFERENCE.md). For results, use an event that has already taken place and an enrolled participant. A future event should reject a result.

5. Capture genuine Swagger responses. Also capture expected validation cases: duplicate registration should return **409**, invalid role **400**, and wrong password **401**. Do not edit screenshots or describe an untested result as successful.

## Common failures

| Symptom | Likely fix |
|---|---|
| `Connection refused (localhost:5100)` on MVC login | Set `ApiSettings__BaseUrl` on the MVC Container App to the backend HTTPS base URL and deploy a new revision. |
| SQL error mentioning `SQLEXPRESS`, `localhost`, firewall or login | Apply migrations to Azure SQL, then set `ConnectionStrings__DefaultConnection` on the API Container App to the Azure SQL connection string via a secret. |
| Azure's “provisioning, stopped, or does not exist” page | Check the latest revision is Running/Healthy, ingress is External, and the ingress target port is 8080. |
| Root API URL returns 404 | Open `/swagger/index.html`; the API does not define a homepage route. |
| Image upload returns 503 | Configure Azure Blob Storage secrets on the API app; check storage account permissions and container setup. |

Remove unused billable Azure resources after the assessment. Keep SQL and storage credentials out of screenshots, source files, ZIPs and Git history.
