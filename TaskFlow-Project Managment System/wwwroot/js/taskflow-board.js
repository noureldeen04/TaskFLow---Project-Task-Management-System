const columns=[
  {id:"todo",name:"To Do",className:"todo"},
  {id:"progress",name:"In Progress",className:"progress"},
  {id:"review",name:"Review",className:"review"},
  {id:"done",name:"Done",className:"done"}
];
const API="/api/tasks";
let tasks=[];
let draggedId=null;
const board=document.getElementById("board");
const backdrop=document.getElementById("modalBackdrop");
const form=document.getElementById("taskForm");
const $=id=>document.getElementById(id);

async function api(url="",options={}){
  const res=await fetch(API+url,{headers:{"Content-Type":"application/json"},...options});
  if(!res.ok)throw new Error("API error "+res.status);
  return res.status===204?null:res.json();
}
async function loadTasks(){
  try{
    const [active,archived]=await Promise.all([api("?archived=false"),api("?archived=true")]);
    tasks=[...active,...archived].map(t=>({...t,dueDate:t.dueDate?t.dueDate.slice(0,10):""}));
  }catch(e){alert("Cannot reach the server. Make sure the backend is running (dotnet run).")}
}
function escapeHtml(value=""){return String(value).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]))}
function initials(name){return name?name.charAt(0).toUpperCase():"?"}
function renderBoard(){
  board.innerHTML=columns.map(col=>{
    const visible=tasks.filter(t=>!t.archived&&t.status===col.id&&matchesFilters(t));
    return `<section class="board-column ${col.className}" data-status="${col.id}">
      <div class="column-head"><span class="dot"></span><h3>${col.name}</h3><span class="count">${visible.length}</span></div>
      <div class="task-list" data-status="${col.id}">${visible.map(cardHtml).join("")}</div>
    </section>`;
  }).join("");
  board.querySelectorAll(".task-card").forEach(card=>{
    card.addEventListener("dragstart",e=>{draggedId=Number(card.dataset.id);e.dataTransfer.setData("text/plain",String(draggedId))});
  });
  board.querySelectorAll(".task-list").forEach(list=>{
    list.addEventListener("dragover",e=>{e.preventDefault();list.classList.add("drag-over")});
    list.addEventListener("dragleave",()=>list.classList.remove("drag-over"));
    list.addEventListener("drop",e=>{e.preventDefault();list.classList.remove("drag-over");const id=Number(e.dataTransfer.getData("text/plain")||draggedId);const task=tasks.find(t=>t.id===id);if(task){task.status=list.dataset.status;renderBoard();api("/"+id+"/status",{method:"PATCH",body:JSON.stringify(task.status)}).catch(()=>{alert("Could not save the change.");loadTasks().then(renderBoard)})}});
  });
}
function cardHtml(t){return `<article class="task-card" draggable="true" data-id="${t.id}">
 <div class="card-top"><span class="priority ${t.priority}">${t.priority}</span><button class="more-btn" onclick="taskMenu(${t.id})" aria-label="Task options">•••</button></div>
 <h4>${escapeHtml(t.title)}</h4>${t.description?`<p class="task-desc">${escapeHtml(t.description)}</p>`:""}
 <div class="card-footer"><span>${t.dueDate?new Date(t.dueDate+"T00:00:00").toLocaleDateString("en-GB",{day:"2-digit",month:"short"}):"No due date"}</span><span class="assignee"><i class="assignee-bubble">${initials(t.assignee)}</i>${escapeHtml(t.assignee||"Unassigned")}</span></div>
 </article>`}
function matchesFilters(t){const p=$("priorityFilter").value,a=$("assigneeFilter").value;return(!p||t.priority===p)&&(!a||(a==="Unassigned"?!t.assignee:t.assignee===a))}
function openModal(task=null){
  $("modalTitle").textContent=task?"Edit task":"Create task";
  $("taskId").value=task?task.id:"";$("title").value=task?task.title:"";$("description").value=task?task.description:"";
  $("status").value=task?task.status:"todo";$("priority").value=task?task.priority:"Medium";
  $("assignee").value=task?task.assignee||"":"";$("dueDate").value=task?task.dueDate||"":"";
  backdrop.classList.remove("hidden");$("title").focus();
}
function closeModal(){backdrop.classList.add("hidden")}
function taskMenu(id){
 const old=document.querySelector(".task-action-menu");
 if(old) old.remove();
 const card=document.querySelector(`.task-card[data-id="${id}"]`);
 if(!card) return;
 const menu=document.createElement("div");
 menu.className="task-action-menu";
 menu.innerHTML=`<button type="button" data-action="edit">✎ Edit task</button>
 <button type="button" data-action="archive">▣ Archive task</button>
 <button type="button" class="danger" data-action="delete">⌫ Delete task</button>`;
 card.appendChild(menu);
 menu.addEventListener("click",e=>{
   const action=e.target.closest("button")?.dataset.action;
   if(!action)return;
   const task=tasks.find(t=>t.id===id);
   if(!task)return;
   if(action==="edit")openModal(task);
   if(action==="archive"){api("/"+id+"/archive?value=true",{method:"PATCH"}).then(loadTasks).then(renderBoard)}
   if(action==="delete"&&confirm("Delete this task permanently?")){api("/"+id,{method:"DELETE"}).then(loadTasks).then(renderBoard)}
 });
 document.addEventListener("click",function closeMenu(e){
   if(!menu.contains(e.target)&&!e.target.closest(".more-btn")){menu.remove();document.removeEventListener("click",closeMenu)}
 },{once:true});
}
function renderArchive(){
 const list=$("archiveList"),archived=tasks.filter(t=>t.archived);
 list.innerHTML=archived.length?archived.map(t=>`<article class="archive-card"><span class="priority ${t.priority}">${t.priority}</span><h3>${escapeHtml(t.title)}</h3><p>${escapeHtml(t.description||"No description")}</p><div class="archive-actions"><button class="restore" onclick="restoreTask(${t.id})">Restore task</button><button onclick="deleteArchived(${t.id})">Delete</button></div></article>`).join(""):'<p class="subtitle">No archived tasks yet.</p>';
}
function restoreTask(id){api("/"+id+"/archive?value=false",{method:"PATCH"}).then(loadTasks).then(renderArchive)}
function deleteArchived(id){if(confirm("Delete this archived task permanently?")){api("/"+id,{method:"DELETE"}).then(loadTasks).then(renderArchive)}}
function showBoard(){ $("boardView").classList.remove("hidden");$("archiveView").classList.add("hidden");$("pageTitle").textContent="Boards & Tasks";document.querySelectorAll(".nav-item").forEach(x=>x.classList.remove("active"));document.querySelector(".nav-item").classList.add("active");renderBoard()}
function showArchive(){ $("boardView").classList.add("hidden");$("archiveView").classList.remove("hidden");$("pageTitle").textContent="Archived tasks";document.querySelectorAll(".nav-item").forEach(x=>x.classList.remove("active"));$("archiveNav").classList.add("active");renderArchive()}
$("createBtn").addEventListener("click",()=>openModal());
$("closeModal").addEventListener("click",closeModal);$("cancelBtn").addEventListener("click",closeModal);
backdrop.addEventListener("click",e=>{if(e.target===backdrop)closeModal()});
form.addEventListener("submit",async e=>{
  e.preventDefault();
  const id=Number($("taskId").value);
  const data={title:$("title").value.trim(),description:$("description").value.trim(),status:$("status").value,priority:$("priority").value,assignee:$("assignee").value||null,dueDate:$("dueDate").value||null,archived:false};
  if(!data.title)return;
  try{
    if(id)await api("/"+id,{method:"PUT",body:JSON.stringify({id,...data})});
    else await api("",{method:"POST",body:JSON.stringify(data)});
  }catch(err){alert("Could not save the task.");return}
  await loadTasks();closeModal();showBoard();
});
$("archiveNav").addEventListener("click",showArchive);
document.querySelector(".nav-item").addEventListener("click",showBoard);
$("filterBtn").addEventListener("click",()=>$("filterPanel").classList.toggle("hidden"));
$("priorityFilter").addEventListener("change",renderBoard);$("assigneeFilter").addEventListener("change",renderBoard);
$("clearFilter").addEventListener("click",()=>{$("priorityFilter").value="";$("assigneeFilter").value="";renderBoard()});
loadTasks().then(renderBoard);
