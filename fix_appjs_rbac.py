import re

path = "src/Filo.Api/wwwroot/app.js"
with open(path, "r") as f:
    js = f.read()

# 1. Modify apiFetch to add x-user-id header
if "'x-user-id': localStorage.getItem('filo_active_user_id')" not in js:
    fetch_headers = """            headers: {
                'Content-Type': 'application/json',
                'x-user-id': localStorage.getItem('filo_active_user_id') || '1',
                ...options.headers
            }"""
    js = re.sub(r"headers:\s*\{\s*'Content-Type':\s*'application/json',\s*\.\.\.options\.headers\s*\}", fetch_headers, js)

# 2. Add switchUser and loadUsersForSwitcher
rbac_js = """

// --- RBAC User Switcher ---
async function loadUsersForSwitcher() {
    const data = await fetch('/api/v1/persons?PageSize=1000').then(res => res.json());
    if (data && data.data && data.data.items) {
        const select = document.getElementById('user-switcher');
        if(!select) return;
        select.innerHTML = '';
        data.data.items.forEach(p => {
            const opt = document.createElement('option');
            opt.value = p.id;
            opt.textContent = `${p.name} ${p.surname} (${p.role})`;
            select.appendChild(opt);
        });
        
        const activeUser = localStorage.getItem('filo_active_user_id') || data.data.items[0].id.toString();
        select.value = activeUser;
        localStorage.setItem('filo_active_user_id', activeUser);
        
        applyRbacRules(data.data.items.find(x => x.id.toString() === activeUser));
    }
}

function switchUser(userId) {
    localStorage.setItem('filo_active_user_id', userId);
    window.location.reload();
}

function applyRbacRules(user) {
    if (!user) return;
    const isAdmin = user.role === 'Admin';
    const isManager = user.role === 'Manager';
    const isStaff = user.role === 'Staff';

    // Update UI name
    const title = document.querySelector('.user-profile span');
    if (title) title.textContent = `${user.name} ${user.surname}`;

    // Hide admin buttons if not admin
    if (!isAdmin) {
        const btnNewVehicle = document.querySelector('button[onclick="openModal(\\'vehicleModal\\')"]');
        if (btnNewVehicle) btnNewVehicle.style.display = 'none';
        
        const btnNewPerson = document.querySelector('button[onclick="openModal(\\'personModal\\')"]');
        if (btnNewPerson) btnNewPerson.style.display = 'none';

        const btnAssignVehicle = document.querySelector('button[onclick="openModal(\\'assignmentModal\\')"]');
        if (btnAssignVehicle) btnAssignVehicle.style.display = 'none';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    loadUsersForSwitcher();
});
"""

if "loadUsersForSwitcher" not in js:
    js += rbac_js

with open(path, "w") as f:
    f.write(js)
print("Updated app.js with RBAC logic.")

