const Database = require("better-sqlite3");
const path = require("path");
const fs = require("fs");

const dataDir = path.join(__dirname, "..", "data");
fs.mkdirSync(dataDir, { recursive: true });

const db = new Database(path.join(dataDir, "taskflow.db"));
db.pragma("foreign_keys = ON");

db.exec(`
CREATE TABLE IF NOT EXISTS Workspace (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Description TEXT,
  OwnerId INTEGER,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
  FOREIGN KEY (OwnerId) REFERENCES User(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS User (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  FullName TEXT NOT NULL,
  Email TEXT NOT NULL UNIQUE,
  WorkspaceId INTEGER,
  FOREIGN KEY (WorkspaceId) REFERENCES Workspace(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Team (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Description TEXT,
  WorkspaceId INTEGER NOT NULL,
  CreatedByUserId INTEGER,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
  FOREIGN KEY (WorkspaceId) REFERENCES Workspace(Id) ON DELETE CASCADE,
  FOREIGN KEY (CreatedByUserId) REFERENCES User(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Team_Member (
  TeamId INTEGER NOT NULL,
  UserId INTEGER NOT NULL,
  Role INTEGER NOT NULL DEFAULT 0,
  JoinedAt TEXT NOT NULL DEFAULT (datetime('now')),
  PRIMARY KEY (TeamId, UserId),
  FOREIGN KEY (TeamId) REFERENCES Team(Id) ON DELETE CASCADE,
  FOREIGN KEY (UserId) REFERENCES User(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Project (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Description TEXT,
  WorkspaceId INTEGER NOT NULL,
  TeamId INTEGER,
  CreatedByUserId INTEGER,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
  FOREIGN KEY (WorkspaceId) REFERENCES Workspace(Id) ON DELETE CASCADE,
  FOREIGN KEY (TeamId) REFERENCES Team(Id) ON DELETE SET NULL,
  FOREIGN KEY (CreatedByUserId) REFERENCES User(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Task (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Title TEXT NOT NULL,
  Description TEXT,
  ProjectId INTEGER NOT NULL,
  CreatedByUserId INTEGER,
  AssignedToUserId INTEGER,
  Priority INTEGER NOT NULL DEFAULT 2,
  Status INTEGER NOT NULL DEFAULT 0,
  DueDate TEXT,
  FOREIGN KEY (ProjectId) REFERENCES Project(Id) ON DELETE CASCADE,
  FOREIGN KEY (CreatedByUserId) REFERENCES User(Id) ON DELETE SET NULL,
  FOREIGN KEY (AssignedToUserId) REFERENCES User(Id) ON DELETE SET NULL
);
`);

function seed() {
  const count = db.prepare("SELECT COUNT(*) AS count FROM Workspace").get().count;
  if (count) return;

  const insertWorkspace = db.prepare("INSERT INTO Workspace (Name, Description) VALUES (?, ?)");
  const ws = insertWorkspace.run("Acme Studio", "Demo TaskFlow workspace").lastInsertRowid;

  const insertUser = db.prepare("INSERT INTO User (FullName, Email, WorkspaceId) VALUES (?, ?, ?)");
  const users = [
    ["Mohamed Mahmoud", "mohamed@example.com", ws],
    ["Ahmed Hassan", "ahmed@example.com", ws],
    ["Shahd Ali", "shahd@example.com", ws],
    ["Nour Samir", "nour@example.com", ws],
    ["Omar Adel", "omar@example.com", ws]
  ];
  users.forEach(u => insertUser.run(...u));

  const insertTeam = db.prepare("INSERT INTO Team (Name, Description, WorkspaceId, CreatedByUserId) VALUES (?, ?, ?, ?)");
  const productTeam = insertTeam.run("Product Team", "Product and engineering", ws, 1).lastInsertRowid;
  const designTeam = insertTeam.run("Design Team", "Product design and UX", ws, 1).lastInsertRowid;
  const marketingTeam = insertTeam.run("Marketing", "Campaigns and communications", ws, 1).lastInsertRowid;

  const addMember = db.prepare("INSERT INTO Team_Member (TeamId, UserId, Role) VALUES (?, ?, ?)");
  [[productTeam,1,0],[productTeam,2,1],[designTeam,3,2],[designTeam,4,2],[marketingTeam,5,3]].forEach(m => addMember.run(...m));

  const insertProject = db.prepare("INSERT INTO Project (Name, Description, WorkspaceId, TeamId, CreatedByUserId) VALUES (?, ?, ?, ?, ?)");
  const p1 = insertProject.run("Website Redesign", "Refresh the company website", ws, designTeam, 1).lastInsertRowid;
  const p2 = insertProject.run("Mobile App", "Build the next mobile experience", ws, productTeam, 2).lastInsertRowid;
  const p3 = insertProject.run("Marketing Campaign", "Q4 launch campaign", ws, marketingTeam, 3).lastInsertRowid;
  const p4 = insertProject.run("Customer Portal", "Self-service customer portal", ws, productTeam, 2).lastInsertRowid;

  const insertTask = db.prepare(`INSERT INTO Task
    (Title, Description, ProjectId, CreatedByUserId, AssignedToUserId, Priority, Status, DueDate)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?)`);
  [
    ["Finalize homepage wireframes", "", p1, 1, 3, 3, 1, "2026-11-26"],
    ["Review mobile navigation", "", p2, 1, 1, 2, 0, "2026-11-27"],
    ["Prepare launch email copy", "", p3, 3, 4, 1, 1, "2026-11-28"],
    ["Set up analytics events", "", p4, 2, 2, 3, 0, "2026-11-25"],
    ["Update brand illustrations", "", p1, 1, 3, 2, 2, "2026-11-20"],
    ["QA checkout flow", "", p2, 2, 1, 3, 1, "2026-11-29"],
    ["Publish campaign landing page", "", p3, 3, 5, 2, 0, "2026-11-30"],
    ["Write API integration notes", "", p4, 2, 2, 1, 2, "2026-11-22"]
  ].forEach(t => insertTask.run(...t));
}
seed();

module.exports = db;
