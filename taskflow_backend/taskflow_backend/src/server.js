const express = require("express");
const cors = require("cors");
const db = require("./db");

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

const PRIORITY = { Low: 1, Medium: 2, High: 3 };
const STATUS = { "To do": 0, "In progress": 1, Done: 2 };
const priorityName = n => ({ 1: "Low", 2: "Medium", 3: "High" }[n] || "Medium");
const statusName = n => ({ 0: "To do", 1: "In progress", 2: "Done" }[n] || "To do");
const taskView = row => row && ({
  ...row,
  priorityLabel: priorityName(row.Priority),
  statusLabel: statusName(row.Status)
});

function asyncRoute(fn) {
  return (req, res, next) => {
    try { fn(req, res, next); } catch (err) { next(err); }
  };
}
function required(value, name) {
  if (value === undefined || value === null || value === "") {
    const err = new Error(`${name} is required`);
    err.status = 400;
    throw err;
  }
}
function exists(table, id) {
  return !!db.prepare(`SELECT Id FROM ${table} WHERE Id = ?`).get(id);
}

app.get("/api/health", (req, res) => res.json({ status: "ok", service: "TaskFlow API" }));

// Workspaces
app.get("/api/workspaces", asyncRoute((req, res) => {
  res.json(db.prepare("SELECT * FROM Workspace ORDER BY Id DESC").all());
}));
app.post("/api/workspaces", asyncRoute((req, res) => {
  required(req.body.name, "name");
  const result = db.prepare("INSERT INTO Workspace (Name, Description, OwnerId) VALUES (?, ?, ?)")
    .run(req.body.name, req.body.description || null, req.body.ownerId || null);
  res.status(201).json(db.prepare("SELECT * FROM Workspace WHERE Id = ?").get(result.lastInsertRowid));
}));

// Users
app.get("/api/users", asyncRoute((req, res) => {
  res.json(db.prepare("SELECT Id, FullName, Email, WorkspaceId FROM User ORDER BY FullName").all());
}));
app.post("/api/users", asyncRoute((req, res) => {
  required(req.body.fullName, "fullName");
  required(req.body.email, "email");
  const result = db.prepare("INSERT INTO User (FullName, Email, WorkspaceId) VALUES (?, ?, ?)")
    .run(req.body.fullName, req.body.email, req.body.workspaceId || null);
  res.status(201).json(db.prepare("SELECT Id, FullName, Email, WorkspaceId FROM User WHERE Id = ?").get(result.lastInsertRowid));
}));

// Teams and membership
app.get("/api/teams", asyncRoute((req, res) => {
  const rows = db.prepare(`SELECT t.*, COUNT(tm.UserId) AS MemberCount
    FROM Team t LEFT JOIN Team_Member tm ON tm.TeamId = t.Id
    GROUP BY t.Id ORDER BY t.Id DESC`).all();
  res.json(rows);
}));
app.post("/api/teams", asyncRoute((req, res) => {
  required(req.body.name, "name");
  required(req.body.workspaceId, "workspaceId");
  if (!exists("Workspace", req.body.workspaceId)) return res.status(404).json({ error: "Workspace not found" });
  const result = db.prepare(`INSERT INTO Team (Name, Description, WorkspaceId, CreatedByUserId)
    VALUES (?, ?, ?, ?)`).run(req.body.name, req.body.description || null, req.body.workspaceId, req.body.createdByUserId || null);
  res.status(201).json(db.prepare("SELECT * FROM Team WHERE Id = ?").get(result.lastInsertRowid));
}));
app.get("/api/teams/:id/members", asyncRoute((req, res) => {
  const team = db.prepare("SELECT Id FROM Team WHERE Id = ?").get(req.params.id);
  if (!team) return res.status(404).json({ error: "Team not found" });
  res.json(db.prepare(`SELECT u.Id, u.FullName, u.Email, tm.Role, tm.JoinedAt
    FROM Team_Member tm JOIN User u ON u.Id = tm.UserId
    WHERE tm.TeamId = ? ORDER BY u.FullName`).all(req.params.id));
}));
app.post("/api/teams/:id/members", asyncRoute((req, res) => {
  const teamId = Number(req.params.id), { userId, role = 2 } = req.body;
  if (!exists("Team", teamId)) return res.status(404).json({ error: "Team not found" });
  if (!exists("User", userId)) return res.status(404).json({ error: "User not found" });
  db.prepare("INSERT INTO Team_Member (TeamId, UserId, Role) VALUES (?, ?, ?)").run(teamId, userId, role);
  res.status(201).json({ message: "Member added to team" });
}));

// Projects
app.get("/api/projects", asyncRoute((req, res) => {
  const rows = db.prepare(`SELECT p.*, w.Name AS WorkspaceName, t.Name AS TeamName,
    u.FullName AS CreatedByName,
    (SELECT COUNT(*) FROM Task task WHERE task.ProjectId = p.Id) AS TaskCount
    FROM Project p JOIN Workspace w ON w.Id = p.WorkspaceId
    LEFT JOIN Team t ON t.Id = p.TeamId
    LEFT JOIN User u ON u.Id = p.CreatedByUserId
    ORDER BY p.Id DESC`).all();
  res.json(rows);
}));
app.post("/api/projects", asyncRoute((req, res) => {
  const { name, description, workspaceId, teamId, createdByUserId } = req.body;
  required(name, "name"); required(workspaceId, "workspaceId");
  if (!exists("Workspace", workspaceId)) return res.status(404).json({ error: "Workspace not found" });
  if (teamId && !exists("Team", teamId)) return res.status(404).json({ error: "Team not found" });
  const result = db.prepare(`INSERT INTO Project (Name, Description, WorkspaceId, TeamId, CreatedByUserId)
    VALUES (?, ?, ?, ?, ?)`).run(name, description || null, workspaceId, teamId || null, createdByUserId || null);
  res.status(201).json(db.prepare("SELECT * FROM Project WHERE Id = ?").get(result.lastInsertRowid));
}));

// Tasks
app.get("/api/tasks", asyncRoute((req, res) => {
  let sql = `SELECT task.*, p.Name AS ProjectName, u.FullName AS AssigneeName
    FROM Task task JOIN Project p ON p.Id = task.ProjectId
    LEFT JOIN User u ON u.Id = task.AssignedToUserId WHERE 1=1`;
  const params = [];
  if (req.query.projectId) { sql += " AND task.ProjectId = ?"; params.push(req.query.projectId); }
  if (req.query.assigneeId) { sql += " AND task.AssignedToUserId = ?"; params.push(req.query.assigneeId); }
  if (req.query.status !== undefined && req.query.status !== "") {
    const value = STATUS[req.query.status] ?? Number(req.query.status);
    if (![0,1,2].includes(value)) return res.status(400).json({ error: "Invalid status" });
    sql += " AND task.Status = ?"; params.push(value);
  }
  sql += " ORDER BY task.Id DESC";
  res.json(db.prepare(sql).all(...params).map(taskView));
}));
app.post("/api/tasks", asyncRoute((req, res) => {
  const b = req.body;
  required(b.title, "title"); required(b.projectId, "projectId");
  if (!exists("Project", b.projectId)) return res.status(404).json({ error: "Project not found" });
  if (b.assignedToUserId && !exists("User", b.assignedToUserId)) return res.status(404).json({ error: "Assignee not found" });
  const priority = typeof b.priority === "string" ? PRIORITY[b.priority] : (b.priority ?? 2);
  const status = typeof b.status === "string" ? STATUS[b.status] : (b.status ?? 0);
  if (![1,2,3].includes(Number(priority))) return res.status(400).json({ error: "Priority must be 1, 2, or 3" });
  if (![0,1,2].includes(Number(status))) return res.status(400).json({ error: "Status must be 0, 1, or 2" });
  const result = db.prepare(`INSERT INTO Task
    (Title, Description, ProjectId, CreatedByUserId, AssignedToUserId, Priority, Status, DueDate)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?)`).run(
      b.title, b.description || null, b.projectId, b.createdByUserId || null,
      b.assignedToUserId || null, Number(priority), Number(status), b.dueDate || null
    );
  res.status(201).json(taskView(db.prepare("SELECT * FROM Task WHERE Id = ?").get(result.lastInsertRowid)));
}));
app.get("/api/tasks/:id", asyncRoute((req, res) => {
  const row = db.prepare("SELECT * FROM Task WHERE Id = ?").get(req.params.id);
  if (!row) return res.status(404).json({ error: "Task not found" });
  res.json(taskView(row));
}));
app.patch("/api/tasks/:id", asyncRoute((req, res) => {
  const current = db.prepare("SELECT * FROM Task WHERE Id = ?").get(req.params.id);
  if (!current) return res.status(404).json({ error: "Task not found" });
  const b = req.body;
  const priority = b.priority === undefined ? current.Priority : (typeof b.priority === "string" ? PRIORITY[b.priority] : Number(b.priority));
  const status = b.status === undefined ? current.Status : (typeof b.status === "string" ? STATUS[b.status] : Number(b.status));
  if (![1,2,3].includes(Number(priority))) return res.status(400).json({ error: "Invalid priority" });
  if (![0,1,2].includes(Number(status))) return res.status(400).json({ error: "Invalid status" });
  db.prepare(`UPDATE Task SET Title=?, Description=?, ProjectId=?, CreatedByUserId=?, AssignedToUserId=?,
    Priority=?, Status=?, DueDate=? WHERE Id=?`).run(
      b.title ?? current.Title, b.description ?? current.Description, b.projectId ?? current.ProjectId,
      b.createdByUserId ?? current.CreatedByUserId, b.assignedToUserId ?? current.AssignedToUserId,
      Number(priority), Number(status), b.dueDate ?? current.DueDate, req.params.id
    );
  res.json(taskView(db.prepare("SELECT * FROM Task WHERE Id = ?").get(req.params.id)));
}));
app.delete("/api/tasks/:id", asyncRoute((req, res) => {
  const result = db.prepare("DELETE FROM Task WHERE Id = ?").run(req.params.id);
  if (!result.changes) return res.status(404).json({ error: "Task not found" });
  res.status(204).end();
}));

app.use((err, req, res, next) => {
  if (err.code === "SQLITE_CONSTRAINT_UNIQUE") return res.status(409).json({ error: "A record with this unique value already exists" });
  if (err.code === "SQLITE_CONSTRAINT_FOREIGNKEY") return res.status(400).json({ error: "Invalid related record ID" });
  res.status(err.status || 500).json({ error: err.message || "Internal server error" });
});

app.listen(PORT, () => console.log(`TaskFlow API running at http://localhost:${PORT}`));
