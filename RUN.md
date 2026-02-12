# PhoSocial — Run instructions (local dev)

Prerequisites
- .NET 6 SDK (project targets net6.0)
- Node.js + npm (for Angular)
- SQL Server (local or Docker) accessible from connection string

1) Create database
- Run the single SQL script to create the database and objects:

```bash
# using sqlcmd (example)
sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -i PhoSocial_db_init.sql
```

2) Update backend connection string
- Edit `PhoSocialService/PhoSocial.API/appsettings.json` and set `ConnectionStrings:DefaultConnection` to your SQL Server connection.
- Make sure `Jwt` section has a `Key`, `Issuer`, and `Audience` set for local testing.

3) Run backend

```bash
# from workspace root
dotnet build PhoSocialService/PhoSocial.API/PhoSocial.API.csproj
dotnet run --project PhoSocialService/PhoSocial.API/PhoSocial.API.csproj
```

API will be available at the URL shown in console (default https://localhost:5001 or as configured). Swagger UI is enabled in Development.

4) Run frontend

```bash
# install deps if not already
npm install --prefix PhoSocialService/PhoSocial.UI
# build dev server
npm start --prefix PhoSocialService/PhoSocial.UI
```

Open `http://localhost:4200` and authenticate. The frontend expects the backend API at `environment.apiUrl` (defaults to https://localhost:7095/api) — update `PhoSocialService/PhoSocial.UI/src/environments/environment.ts` if needed.

5) SignalR (chat)
- Frontend connects to `/hubs/chat` and sends the JWT token as `access_token` query param. Ensure JWT is returned by the login endpoint and the same `Issuer/Audience/Key` are used by the API.

6) Notes & next steps
- The SQL script created `ExpireStories` stored procedure; a hosted background service periodically calls it.
- For scaling SignalR, consider Redis backplane and a connection store.
- Security: rotate JWT key for production, secure secrets with environment variables or secret store.

If you want, I can also produce a Postman collection or Docker Compose to start SQL Server + API + UI together.
