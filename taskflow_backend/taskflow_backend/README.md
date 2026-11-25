# TaskFlow Backend

Starter REST API for the TaskFlow frontend, built with Node.js, Express, and SQLite.

## Requirements
- Node.js 18 or newer
- npm

## Run
```bash
npm install
npm run dev
```
Server: `http://localhost:3000`  
Health check: `http://localhost:3000/api/health`

SQLite database is created automatically at `data/taskflow.db`. The first run seeds a demo workspace, users, teams, projects, and tasks.

## API routes

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/health` | API status |
| GET/POST | `/api/workspaces` | List/create workspaces |
| GET/POST | `/api/users` | List/create users |
| GET/POST | `/api/teams` | List/create teams |
| GET | `/api/teams/:id/members` | Team members |
| POST | `/api/teams/:id/members` | Add member to team |
| GET/POST | `/api/projects` | List/create projects |
| GET/POST | `/api/tasks` | List/create tasks |
| GET/PATCH/DELETE | `/api/tasks/:id` | Read/update/delete task |

Task list filters: `?projectId=1&status=In%20progress&assigneeId=1`

## Example create task
```json
{
  "title": "Design landing page",
  "description": "Create the first draft",
  "projectId": 1,
  "createdByUserId": 1,
  "assignedToUserId": 2,
  "priority": 2,
  "status": 0,
  "dueDate": "2026-12-10"
}
```

Priority: `1=Low, 2=Medium, 3=High`  
Status: `0=To do, 1=In progress, 2=Done`

## Notes
- This is a learning/demo backend, not production-ready authentication.
- The `User.WorkspaceId` field is modeled as in the supplied ERD.
- `TEAM_MEMBER` uses a composite primary key `(TeamId, UserId)`.
- Add authentication, authorization middleware, validation, and migrations before production use.
