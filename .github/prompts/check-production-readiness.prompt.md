---
description: "Analyze project configuration for production readiness including database, cache, telemetry, and security settings"
agent: "agent"
tools: [read, search]
---

Analyze the project configuration for production readiness:

1. **Database**: Connection strings, pooling, timeouts, retry policies
2. **Cache**: Redis configuration, expiration policies
3. **Telemetry**: OpenTelemetry exporters, sampling rates
4. **Authentication**: JWT key strength, token expiration
5. **Rate Limiting**: Strategy and limits configured
6. **Health Checks**: Endpoints and dependencies monitored
7. **Logging**: Structured logging, log levels, sensitive data filtering
8. **Security Headers**: HSTS, CSP, CORS configuration

Compare `appsettings.json` vs `appsettings.Production.json` and flag missing production overrides.
