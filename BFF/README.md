BFF (YARP Reverse Proxy)
=========================

This project configures the BFF as a reverse proxy using YARP.

Run locally with Docker Compose:

```bash
docker compose up --build bff
```

Key endpoints:
- `/auth/*` -> proxied to `AuthService` (path prefix removed)
- `/video/*` -> proxied to `VideoService` (path prefix removed)
- `/metrics` -> simple in-memory metrics (request counts per path)
- `/health/live` and `/health/ready` -> health checks

Configuration is in `appsettings.json` under the `ReverseProxy` section.
