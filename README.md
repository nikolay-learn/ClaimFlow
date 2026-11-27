# ClaimFlow

![CI](https://github.com/nikolay-learn/ClaimFlow/actions/workflows/ci.yml/badge.svg)

The backend of an insurance company: customers file claims, adjusters review them, payouts go out.
Built step by step in the ".NET + AWS + AI" course.

## Run it

```bash
dotnet run --project src/ClaimFlow.Api
# API docs: http://localhost:5088/scalar
```

## Test it

```bash
dotnet test
```

## Configuration

| Key | Where | Purpose |
|-----|-------|---------|
| `ClaimRules:FilingWindowYears` | appsettings.json | Years after the incident a claim may be filed (1–10) |
| `ClaimRules:MaxClaimAmount` | appsettings.json | Larger claims go to the large-loss process |
| `ConnectionStrings:ClaimsDb` | user secrets / environment | Database (from lesson 16) |

## Project layout

```
src/ClaimFlow.Api          the web API
tests/ClaimFlow.Api.Tests  unit and integration tests
```