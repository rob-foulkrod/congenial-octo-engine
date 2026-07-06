# congenial-octo-engine

A .NET 10 Todo demo organized as two Azure Container Apps workloads.

- `src/Todo.Web` - ASP.NET Core MVC frontend with external ACA ingress
- `src/Todo.Worker` - private gRPC worker that records Todo events and emits periodic demo logs
- `.azure` - ACA YAML manifests and the classroom deployment script

Run the classroom deployment from the repo root:

```powershell
.\.azure\deploy-container-apps.ps1
```
