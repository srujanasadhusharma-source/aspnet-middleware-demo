# User Management API (ASP.NET Core)

A small ASP.NET Core Web API for managing users, with custom middleware and Swagger/OpenAPI documentation.

## Features

- **CRUD endpoints** for users:
  - `GET /api/users` - list all users
  - `GET /api/users/{id}` - get one user
  - `POST /api/users` - create a user
  - `PUT /api/users/{id}` - update a user
  - `DELETE /api/users/{id}` - delete a user
- **Validation:** name (2-50 characters), valid email, and age (0-120) are checked. Invalid data returns a 400 response with error details.
- **Custom middleware:** `RequestTimingMiddleware` logs the method, path, and time taken for every request.
- **Swagger UI** for documentation and testing.

## How Microsoft Copilot was used

- Generated the first version of the CRUD endpoints.
- Helped debug issues in the endpoints and fix them.
- Suggested the validation rules and the request-timing middleware.

## Run

```
dotnet run
```

Open `/swagger` in your browser (the port is shown in the terminal) and try the endpoints. The timing logs appear in the terminal.
