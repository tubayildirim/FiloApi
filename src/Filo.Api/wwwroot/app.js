// API Base URL config (Relative since we are hosted on the same server)
const API_BASE = '/api/v1';

// Global State for Pagination & Sorting
const state = {
    vehicles: { page: 1, pageSize: 10, search: '', sortCol: '', sortDir: '' },
    drivers: { page: 1, pageSize: 10, search: '', sortCol: '', sortDir: '' }
};

let searchTimeout = null;

function debounceSearch(type) {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        const val = document.getElementById(`search-${type}`).value;
        state[type].search = val;
        state[type].page = 1; // Aramada ilk sayfaya dön
        if (type === 'vehicles') loadVehicles();
        if (type === 'drivers') loadDrivers();
    }, 500);
}

function handleSort(type, col) {
    if (state[type].sortCol === col) {
        state[type].sortDir = state[type].sortDir === 'asc' ? 'desc' : 'asc';
    } else {
        state[type].sortCol = col;
        state[type].sortDir = 'asc';
    }
    
    // Update icons
    document.querySelectorAll(`th i[id^="sort-icon-${type}"]`).forEach(icon => {
        icon.className = 'fa-solid fa-sort';
    });
    const activeIcon = document.getElementById(`sort-icon-${type}-${col}`);
    if (activeIcon) {
        activeIcon.className = state[type].sortDir === 'asc' ? 'fa-solid fa-sort-up' : 'fa-solid fa-sort-down';
    }
    
    if (type === 'vehicles') loadVehicles();
    if (type === 'drivers') loadDrivers();
}

function updatePagination(type, data) {
    const prevBtn = document.getElementById(`btn-prev-${type}`);
    const nextBtn = document.getElementById(`btn-next-${type}`);
    const info = document.getElementById(`page-info-${type}`);
    
    if (!prevBtn || !nextBtn || !info) return;

    if (data) {
        info.innerText = `Sayfa ${data.currentPage} / ${data.totalPages || 1}`;
        prevBtn.disabled = !data.hasPrevious;
        nextBtn.disabled = !data.hasNext;
    } else {
        info.innerText = `Sayfa 1 / 1`;
        prevBtn.disabled = true;
        nextBtn.disabled = true;
    }
}

function prevPage(type) {
    if (state[type].page > 1) {
        state[type].page--;
        if (type === 'vehicles') loadVehicles();
        if (type === 'drivers') loadDrivers();
    }
}

function nextPage(type) {
    state[type].page++;
    if (type === 'vehicles') loadVehicles();
    if (type === 'drivers') loadDrivers();
}

function buildQuery(type) {
    const s = state[type];
    let query = `?PageNumber=${s.page}&PageSize=${s.pageSize}`;
    if (s.search) query += `&SearchTerm=${encodeURIComponent(s.search)}`;
    if (s.sortCol) query += `&SortColumn=${encodeURIComponent(s.sortCol)}`;
    if (s.sortDir) query += `&SortDirection=${encodeURIComponent(s.sortDir)}`;
    return query;
}

// SPA Page routing
document.querySelectorAll('.sidebar .nav-item').forEach(item => {
    item.addEventListener('click', (e) => {
        const page = item.getAttribute('data-page');
        if (page) {
            switchPage(page);
        }
    });
});

function switchPage(pageName) {
    // Nav active class
    document.querySelectorAll('.sidebar .nav-item').forEach(el => el.classList.remove('active'));
    const activeNav = document.querySelector(`.sidebar .nav-item[data-page="${pageName}"]`);
    if (activeNav) activeNav.classList.add('active');

    // Section view active class
    document.querySelectorAll('.page-view').forEach(el => el.classList.remove('active'));
    document.getElementById(`page-${pageName}`).classList.add('active');

    // Title update
    const titleMap = {
        'dashboard': 'Dashboard',
        'vehicles': 'Araçlar',
        'drivers': 'Sürücüler',
        'assignments': 'Atamalar',
        'expenses': 'Gider Girişi',
        'reports': 'Filo Raporları'
    };
    document.getElementById('page-title').innerText = titleMap[pageName] || 'Yönetim Paneli';

    // Load page data
    if (pageName === 'dashboard') loadDashboardData();
    if (pageName === 'vehicles') loadVehicles();
    if (pageName === 'drivers') loadDrivers();
    if (pageName === 'assignments') loadAssignments();
    if (pageName === 'expenses') loadAllExpenses();
    if (pageName === 'reports') {
        switchReportTab('all-vehicles');
    }
}

// Sub tab switching inside Expenses
function switchSubTab(subTabName) {
    document.querySelectorAll('#page-expenses .sub-tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`#page-expenses .sub-tab-btn[data-subtab="${subTabName}"]`).classList.add('active');

    document.querySelectorAll('#page-expenses .sub-panel').forEach(panel => panel.classList.remove('active'));
    const targetPanel = document.getElementById(`subpanel-${subTabName}`);
    targetPanel.classList.add('active');
    
    // İlgili panele pürüzsüz şekilde kaydır
    targetPanel.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// Universal API fetch wrapper with notifications
async function apiFetch(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            }
        });
        const result = await response.json();
        if (!response.ok || !result.success) {
            const errMsg = result.errors && result.errors.length > 0 
                ? result.errors.join('<br>') 
                : (result.message || 'İşlem başarısız oldu.');
            showToast(errMsg, 'error');
            return null;
        }
        if (options.method && options.method !== 'GET') {
            showToast(result.message || 'İşlem başarıyla tamamlandı.', 'success');
        }
        return result.data;
    } catch (err) {
        showToast('Sunucu bağlantı hatası oluştu.', 'error');
        console.error(err);
        return null;
    }
}

// Toast Notifications
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <i class="fa-solid ${type === 'success' ? 'fa-circle-check' : 'fa-triangle-exclamation'}"></i>
        <span>${message}</span>
    `;
    container.appendChild(toast);
    setTimeout(() => toast.classList.add('show'), 50);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// Modal actions
function openModal(id) {
    document.getElementById(id).classList.add('active');
}
function closeModal(id) {
    document.getElementById(id).classList.remove('active');
}

// ================= DASHBOARD CONTROLLER =================
let expensesChart = null;

async function loadDashboardData() {
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const drivers = await apiFetch('/person?PageSize=100');
    const services = await apiFetch('/vehicle-services?PageSize=100');
    const fuels = await apiFetch('/vehicle-fuels?PageSize=100');
    const maintenances = await apiFetch('/vehicle-maintenances?PageSize=100');

    // Update count metrics
    document.getElementById('metric-total-vehicles').innerText = vehicles?.totalCount || 0;
    document.getElementById('metric-total-drivers').innerText = drivers?.totalCount || 0;
    
    const activeServicesCount = services?.items ? services.items.filter(s => s.status === 'Aktif').length : 0;
    document.getElementById('metric-active-services').innerText = activeServicesCount;

    const lastFuelOdo = fuels?.items && fuels.items.length > 0 ? fuels.items[0].odometer : 0;
    document.getElementById('metric-last-refuel').innerText = lastFuelOdo;

    // Load Chart
    renderChart(fuels?.items || [], maintenances?.items || [], services?.items || []);

    // Load recent activities
    const recentActivity = document.getElementById('recent-activity-list');
    recentActivity.innerHTML = '';
    
    const allActivities = [];
    if (fuels?.items) {
        fuels.items.slice(0, 5).forEach(f => allActivities.push({
            type: 'Yakıt Alımı',
            icon: 'fa-gas-pump',
            vehicle: f.vehicle ? `${f.vehicle.brand} ${f.vehicle.model}` : 'Bilinmeyen Araç',
            date: new Date(f.refuelingDate).toLocaleDateString('tr-TR')
        }));
    }
    if (maintenances?.items) {
        maintenances.items.slice(0, 5).forEach(m => allActivities.push({
            type: 'Periyodik Bakım',
            icon: 'fa-screwdriver-wrench',
            vehicle: m.vehicle ? `${m.vehicle.brand} ${m.vehicle.model}` : 'Bilinmeyen Araç',
            date: new Date(m.maintenanceDate).toLocaleDateString('tr-TR')
        }));
    }

    if (allActivities.length === 0) {
        recentActivity.innerHTML = `<tr><td colspan="3" style="text-align: center; color: var(--text-secondary);">Kayıt bulunamadı.</td></tr>`;
    } else {
        allActivities.sort((a,b) => b.date.localeCompare(a.date)).slice(0, 5).forEach(act => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><i class="fa-solid ${act.icon}"></i> ${act.type}</td>
                <td>${act.vehicle}</td>
                <td>${act.date}</td>
            `;
            recentActivity.appendChild(tr);
        });
    }
}

function renderChart(fuels, maintenances, services) {
    const ctx = document.getElementById('expenses-chart').getContext('2d');
    
    // Sum prices
    let fuelSum = fuels.reduce((acc, curr) => acc + curr.totalPrice, 0);
    let maintSum = maintenances.reduce((acc, curr) => acc + curr.cost, 0);
    let serviceSum = services.reduce((acc, curr) => acc + (curr.cost || 0), 0);

    if (expensesChart) {
        expensesChart.destroy();
    }

    expensesChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Yakıt Giderleri', 'Bakım Giderleri', 'Servis/Hasar'],
            datasets: [{
                label: 'Gider Tutarı (TL)',
                data: [fuelSum, maintSum, serviceSum],
                backgroundColor: ['#06B6D4', '#8B5CF6', '#EC4899'],
                borderColor: 'rgba(255, 255, 255, 0.1)',
                borderWidth: 1,
                borderRadius: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    grid: { color: 'rgba(255,255,255,0.05)' },
                    ticks: { color: '#94A3B8' }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#94A3B8' }
                }
            }
        }
    });
}

// ================= VEHICLES CONTROLLER =================
async function loadVehicles() {
    const query = buildQuery('vehicles');
    const data = await apiFetch(`/vehicles${query}`);
    const tbody = document.getElementById('vehicles-body');
    if(!tbody) return;
    
    tbody.innerHTML = '';
    
    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" style="text-align: center; color: var(--text-secondary);">Envanter boş.</td></tr>`;
        updatePagination('vehicles', null);
        return;
    }

    updatePagination('vehicles', data);

    data.items.forEach(v => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><strong>${v.brand}</strong> ${v.model}</td>
            <td><span class="badge success">${v.plateNumber}</span></td>
            <td>${v.year}</td>
            <td>${v.color || '-'}</td>
            <td>${v.fuelType || '-'} / ${v.transmissionType || '-'}</td>
            <td>
                <button class="btn-action edit" onclick="editVehicle(${v.id}, '${v.brand}', '${v.model}', '${v.plateNumber}', ${v.year}, '${v.color || ''}', '${v.fuelType || ''}', '${v.transmissionType || ''}')"><i class="fa-solid fa-pen-to-square"></i></button>
                <button class="btn-action delete" onclick="deleteVehicle(${v.id})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openAddVehicleModal() {
    document.getElementById('vehicle-modal-title').innerText = 'Yeni Araç Ekle';
    document.getElementById('vehicle-form').reset();
    document.getElementById('vehicle-id').value = '';
    openModal('modal-vehicle');
}

function editVehicle(id, brand, model, plate, year, color, fuel, trans) {
    document.getElementById('vehicle-modal-title').innerText = 'Araç Bilgilerini Düzenle';
    document.getElementById('vehicle-id').value = id;
    document.getElementById('vehicle-brand').value = brand;
    document.getElementById('vehicle-model').value = model;
    document.getElementById('vehicle-plate').value = plate;
    document.getElementById('vehicle-year').value = year;
    document.getElementById('vehicle-color').value = color;
    document.getElementById('vehicle-fuel-type').value = fuel;
    document.getElementById('vehicle-transmission').value = trans;
    openModal('modal-vehicle');
}

async function handleVehicleSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('vehicle-id').value;
    const body = {
        brand: document.getElementById('vehicle-brand').value,
        model: document.getElementById('vehicle-model').value,
        plateNumber: document.getElementById('vehicle-plate').value,
        year: parseInt(document.getElementById('vehicle-year').value),
        color: document.getElementById('vehicle-color').value,
        fuelType: document.getElementById('vehicle-fuel-type').value,
        transmissionType: document.getElementById('vehicle-transmission').value
    };

    let result;
    if (id) {
        result = await apiFetch(`/vehicles/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    } else {
        result = await apiFetch('/vehicles', { method: 'POST', body: JSON.stringify(body) });
    }

    if (result !== null) {
        closeModal('modal-vehicle');
        loadVehicles();
    }
}

async function deleteVehicle(id) {
    if (confirm('Bu aracı silmek istediğinizden emin misiniz?')) {
        const result = await apiFetch(`/vehicles/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadVehicles();
        }
    }
}

// ================= DRIVERS CONTROLLER =================
async function loadDrivers() {
    const query = buildQuery('drivers');
    const data = await apiFetch(`/person${query}`);
    const tbody = document.getElementById('drivers-body');
    if(!tbody) return;
    
    tbody.innerHTML = '';

    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align: center; color: var(--text-secondary);">Sürücü kaydı bulunamadı.</td></tr>`;
        updatePagination('drivers', null);
        return;
    }

    updatePagination('drivers', data);

    data.items.forEach(d => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td><strong>${d.name}</strong> ${d.surname}</td>
            <td>${d.tckn}</td>
            <td>${d.age}</td>
            <td>${d.gender}</td>
            <td>
                <button class="btn-action edit" onclick="editDriver(${d.id}, '${d.name}', '${d.surname}', '${d.tckn}', ${d.age}, '${d.gender}')"><i class="fa-solid fa-pen-to-square"></i></button>
                <button class="btn-action delete" onclick="deleteDriver(${d.id})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openAddDriverModal() {
    document.getElementById('driver-modal-title').innerText = 'Yeni Sürücü Ekle';
    document.getElementById('driver-form').reset();
    document.getElementById('driver-id').value = '';
    openModal('modal-driver');
}

function editDriver(id, name, surname, tckn, age, gender) {
    document.getElementById('driver-modal-title').innerText = 'Sürücü Bilgilerini Düzenle';
    document.getElementById('driver-id').value = id;
    document.getElementById('driver-name').value = name;
    document.getElementById('driver-surname').value = surname;
    document.getElementById('driver-tckn').value = tckn;
    document.getElementById('driver-age').value = age;
    document.getElementById('driver-gender').value = gender;
    openModal('modal-driver');
}

async function handleDriverSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('driver-id').value;
    const body = {
        name: document.getElementById('driver-name').value,
        surname: document.getElementById('driver-surname').value,
        tckn: document.getElementById('driver-tckn').value,
        age: parseInt(document.getElementById('driver-age').value),
        gender: document.getElementById('driver-gender').value
    };

    let result;
    if (id) {
        result = await apiFetch(`/person/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    } else {
        result = await apiFetch('/person', { method: 'POST', body: JSON.stringify(body) });
    }

    if (result !== null) {
        closeModal('modal-driver');
        loadDrivers();
    }
}

async function deleteDriver(id) {
    if (confirm('Bu sürücüyü silmek istediğinizden emin misiniz?')) {
        const result = await apiFetch(`/person/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadDrivers();
        }
    }
}

// ================= ASSIGNMENTS CONTROLLER =================
async function loadAssignments() {
    const data = await apiFetch('/vehicle-matches?PageSize=100');
    const tbody = document.getElementById('assignments-table-body');
    tbody.innerHTML = '';

    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align: center; color: var(--text-secondary);">Eşleştirme geçmişi bulunamadı.</td></tr>`;
        return;
    }

    data.items.forEach(m => {
        const tr = document.createElement('tr');
        const vInfo = m.vehicle ? `${m.vehicle.brand} ${m.vehicle.model} (${m.vehicle.plateNumber})` : 'Silinmiş Araç';
        const pInfo = m.person ? `${m.person.name} ${m.person.surname}` : 'Silinmiş Sürücü';
        const assignDateStr = new Date(m.assignmentDate).toLocaleDateString('tr-TR');
        tr.innerHTML = `
            <td>${vInfo}</td>
            <td><strong>${pInfo}</strong></td>
            <td>${assignDateStr}</td>
            <td>${m.assignmentKm} KM</td>
            <td>
                <button class="btn-action edit" onclick="openAssignmentDetailsModal(${m.vehiclePersonId})" title="Detay"><i class="fa-solid fa-circle-info"></i></button>
                <button class="btn-action delete" onclick="deleteAssignment(${m.vehiclePersonId})" title="Sil"><i class="fa-solid fa-link-slash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
    window._assignmentData = data.items;
}

function openAssignmentDetailsModal(id) {
    const m = window._assignmentData.find(x => x.vehiclePersonId === id);
    if (!m) return;
    
    const content = document.getElementById('assignment-details-content');
    
    // Araç detayları
    const v = m.vehicle;
    let vHtml = v ? `
        <div style="background: rgba(6,182,212,0.1); padding: 1rem; border-radius: 8px; border-left: 3px solid var(--color-accent-teal);">
            <h4 style="margin-bottom: 0.5rem; color: var(--color-accent-teal);">Araç Bilgileri</h4>
            <p><strong>Plaka:</strong> ${v.plateNumber}</p>
            <p><strong>Marka/Model:</strong> ${v.brand} ${v.model}</p>
            <p><strong>Yıl / Renk:</strong> ${v.year} / ${v.color || '-'}</p>
            <p><strong>Yakıt / Şanzıman:</strong> ${v.fuelType || '-'} / ${v.transmission || '-'}</p>
        </div>
    ` : '<p style="color: var(--color-danger);">Araç bilgisi bulunamadı (Silinmiş olabilir).</p>';
    
    // Sürücü detayları
    const p = m.person;
    let pHtml = p ? `
        <div style="background: rgba(139,92,246,0.1); padding: 1rem; border-radius: 8px; border-left: 3px solid var(--color-accent-violet);">
            <h4 style="margin-bottom: 0.5rem; color: var(--color-accent-violet);">Sürücü Bilgileri</h4>
            <p><strong>Ad Soyad:</strong> ${p.name} ${p.surname}</p>
            <p><strong>TCKN:</strong> ${p.tckn}</p>
            <p><strong>Cinsiyet / Yaş:</strong> ${p.gender || '-'} / ${p.age}</p>
        </div>
    ` : '<p style="color: var(--color-danger);">Sürücü bilgisi bulunamadı (Silinmiş olabilir).</p>';
    
    // Atama detayları
    let aHtml = `
        <div style="background: rgba(255,255,255,0.05); padding: 1rem; border-radius: 8px;">
            <h4 style="margin-bottom: 0.5rem; color: var(--text-primary);">Atama Özeti</h4>
            <p><strong>Tarih:</strong> ${new Date(m.assignmentDate).toLocaleDateString('tr-TR')}</p>
            <p><strong>Başlangıç KM:</strong> ${m.assignmentKm} KM</p>
        </div>
    `;
    
    content.innerHTML = aHtml + vHtml + pHtml;
    openModal('modal-assignment-details');
}

async function openAddAssignmentModal() {
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const drivers = await apiFetch('/person?PageSize=100');

    const vSelect = document.getElementById('assignment-vehicle-select');
    const dSelect = document.getElementById('assignment-driver-select');

    vSelect.innerHTML = '';
    dSelect.innerHTML = '';

    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            vSelect.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }

    if (drivers?.items) {
        drivers.items.forEach(d => {
            dSelect.innerHTML += `<option value="${d.id}">${d.name} ${d.surname} (TC: ${d.tckn})</option>`;
        });
    }

    document.getElementById('assignment-date').valueAsDate = new Date();
    openModal('modal-assignment');
}

async function handleAssignmentSubmit(e) {
    e.preventDefault();
    const body = {
        vehicleId: parseInt(document.getElementById('assignment-vehicle-select').value),
        personId: parseInt(document.getElementById('assignment-driver-select').value),
        assignmentDate: new Date(document.getElementById('assignment-date').value).toISOString(),
        assignmentKm: parseInt(document.getElementById('assignment-km').value)
    };

    const result = await apiFetch('/vehicle-matches', { method: 'POST', body: JSON.stringify(body) });
    if (result !== null) {
        closeModal('modal-assignment');
        loadAssignments();
    }
}

async function deleteAssignment(id) {
    if (confirm('Atama ilişkisini sonlandırmak (silmek) istiyor musunuz?')) {
        const result = await apiFetch(`/vehicle-matches/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadAssignments();
        }
    }
}

// ================= EXPENSES / LOGS CONTROLLER =================
function loadAllExpenses() {
    loadFuelExpenses();
    loadMaintenanceExpenses();
    loadServiceExpenses();
}

// 1. FUEL HARCAMALARI
async function loadFuelExpenses() {
    const data = await apiFetch('/vehicle-fuels?PageSize=100');
    const tbody = document.getElementById('fuel-table-body');
    tbody.innerHTML = '';

    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" style="text-align: center; color: var(--text-secondary);">Yakıt kaydı bulunamadı.</td></tr>`;
        return;
    }

    data.items.forEach(f => {
        const tr = document.createElement('tr');
        const vInfo = f.vehicle ? `${f.vehicle.brand} ${f.vehicle.model} (${f.vehicle.plateNumber})` : 'Silinmiş Araç';
        const dateStr = new Date(f.refuelingDate).toLocaleDateString('tr-TR');
        tr.innerHTML = `
            <td>${vInfo}</td>
            <td>${dateStr}</td>
            <td>${f.odometer} KM</td>
            <td>${f.liters} Lt</td>
            <td>${f.pricePerLiter.toFixed(2)} TL</td>
            <td><strong>${f.totalPrice.toFixed(2)} TL</strong></td>
            <td>${f.receiptNumber || '-'}</td>
            <td>
                <button class="btn-action edit" onclick="openEditFuelModal(${f.vehicleFuelId})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn-action delete" onclick="deleteFuel(${f.vehicleFuelId})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
    window._fuelData = data.items;
}

async function openEditFuelModal(id) {
    const f = window._fuelData.find(x => x.vehicleFuelId === id);
    if (!f) return;
    
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('fuel-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    
    document.getElementById('fuel-id').value = f.vehicleFuelId;
    document.getElementById('fuel-vehicle-select').value = f.vehicleId;
    document.getElementById('fuel-date').value = f.refuelingDate.split('T')[0];
    document.getElementById('fuel-odometer').value = f.odometer;
    document.getElementById('fuel-liters').value = f.liters;
    document.getElementById('fuel-price').value = f.pricePerLiter;
    document.getElementById('fuel-receipt').value = f.receiptNumber || '';
    
    openModal('modal-fuel');
}

async function openAddFuelModal() {
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('fuel-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    document.getElementById('fuel-form').reset();
    document.getElementById('fuel-id').value = '';
    document.getElementById('fuel-date').valueAsDate = new Date();
    openModal('modal-fuel');
}

async function handleFuelSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('fuel-id').value;
    const body = {
        vehicleId: parseInt(document.getElementById('fuel-vehicle-select').value),
        refuelingDate: new Date(document.getElementById('fuel-date').value).toISOString(),
        odometer: parseInt(document.getElementById('fuel-odometer').value),
        liters: parseFloat(document.getElementById('fuel-liters').value),
        pricePerLiter: parseFloat(document.getElementById('fuel-price').value),
        receiptNumber: document.getElementById('fuel-receipt').value
    };

    let result;
    if (id) {
        body.id = parseInt(id);
        result = await apiFetch(`/vehicle-fuels/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    } else {
        result = await apiFetch('/vehicle-fuels', { method: 'POST', body: JSON.stringify(body) });
    }
    
    if (result !== null) {
        closeModal('modal-fuel');
        loadFuelExpenses();
    }
}

async function deleteFuel(id) {
    if (confirm('Bu yakıt kaydını silmek istediğinizden emin misiniz?')) {
        const result = await apiFetch(`/vehicle-fuels/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadFuelExpenses();
        }
    }
}

// 2. MAINTENANCE HARCAMALARI
async function loadMaintenanceExpenses() {
    const data = await apiFetch('/vehicle-maintenances?PageSize=100');
    const tbody = document.getElementById('maintenance-table-body');
    tbody.innerHTML = '';

    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" style="text-align: center; color: var(--text-secondary);">Bakım kaydı bulunamadı.</td></tr>`;
        return;
    }

    data.items.forEach(m => {
        const tr = document.createElement('tr');
        const vInfo = m.vehicle ? `${m.vehicle.brand} ${m.vehicle.model} (${m.vehicle.plateNumber})` : 'Silinmiş Araç';
        const dateStr = new Date(m.maintenanceDate).toLocaleDateString('tr-TR');
        const nextDateStr = m.nextMaintenanceDate ? new Date(m.nextMaintenanceDate).toLocaleDateString('tr-TR') : '-';
        const nextKmStr = m.nextMaintenanceKm ? `${m.nextMaintenanceKm} KM` : '-';
        tr.innerHTML = `
            <td>${vInfo}</td>
            <td>${dateStr}</td>
            <td>${m.odometer} KM</td>
            <td>${m.description}</td>
            <td><span class="badge warning">${m.maintenanceType}</span></td>
            <td><strong>${m.cost.toFixed(2)} TL</strong></td>
            <td>${nextKmStr} / ${nextDateStr}</td>
            <td>
                <button class="btn-action edit" onclick="openEditMaintenanceModal(${m.vehicleMaintenanceId})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn-action delete" onclick="deleteMaintenance(${m.vehicleMaintenanceId})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
    window._maintenanceData = data.items;
}

async function openEditMaintenanceModal(id) {
    const m = window._maintenanceData.find(x => x.vehicleMaintenanceId === id);
    if (!m) return;
    
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('maintenance-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    
    document.getElementById('maintenance-id').value = m.vehicleMaintenanceId;
    document.getElementById('maintenance-vehicle-select').value = m.vehicleId;
    document.getElementById('maintenance-date').value = m.maintenanceDate.split('T')[0];
    document.getElementById('maintenance-odometer').value = m.odometer;
    document.getElementById('maintenance-desc').value = m.description;
    document.getElementById('maintenance-type').value = m.maintenanceType;
    document.getElementById('maintenance-cost').value = m.cost;
    document.getElementById('maintenance-next-date').value = m.nextMaintenanceDate ? m.nextMaintenanceDate.split('T')[0] : '';
    document.getElementById('maintenance-next-km').value = m.nextMaintenanceKm || '';
    
    openModal('modal-maintenance');
}

async function openAddMaintenanceModal() {
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('maintenance-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    document.getElementById('maintenance-form').reset();
    document.getElementById('maintenance-id').value = '';
    document.getElementById('maintenance-date').valueAsDate = new Date();
    openModal('modal-maintenance');
}

async function handleMaintenanceSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('maintenance-id').value;
    const body = {
        vehicleId: parseInt(document.getElementById('maintenance-vehicle-select').value),
        maintenanceDate: new Date(document.getElementById('maintenance-date').value).toISOString(),
        odometer: parseInt(document.getElementById('maintenance-odometer').value),
        description: document.getElementById('maintenance-desc').value,
        maintenanceType: document.getElementById('maintenance-type').value,
        cost: parseFloat(document.getElementById('maintenance-cost').value),
        nextMaintenanceDate: document.getElementById('maintenance-next-date').value 
            ? new Date(document.getElementById('maintenance-next-date').value).toISOString() 
            : null,
        nextMaintenanceKm: document.getElementById('maintenance-next-km').value 
            ? parseInt(document.getElementById('maintenance-next-km').value) 
            : null
    };

    let result;
    if (id) {
        body.id = parseInt(id);
        result = await apiFetch(`/vehicle-maintenances/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    } else {
        result = await apiFetch('/vehicle-maintenances', { method: 'POST', body: JSON.stringify(body) });
    }
    
    if (result !== null) {
        closeModal('modal-maintenance');
        loadMaintenanceExpenses();
    }
}

async function deleteMaintenance(id) {
    if (confirm('Bu bakım kaydını silmek istediğinizden emin misiniz?')) {
        const result = await apiFetch(`/vehicle-maintenances/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadMaintenanceExpenses();
        }
    }
}

// 3. SERVICE / HASAR HARCAMALARI
async function loadServiceExpenses() {
    const data = await apiFetch('/vehicle-services?PageSize=100');
    const tbody = document.getElementById('service-table-body');
    tbody.innerHTML = '';

    if (!data?.items || data.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" style="text-align: center; color: var(--text-secondary);">Servis kaydı bulunamadı.</td></tr>`;
        return;
    }

    data.items.forEach(s => {
        const tr = document.createElement('tr');
        const vInfo = s.vehicle ? `${s.vehicle.brand} ${s.vehicle.model} (${s.vehicle.plateNumber})` : 'Silinmiş Araç';
        const entryStr = new Date(s.entryDate).toLocaleDateString('tr-TR');
        const exitStr = s.exitDate ? new Date(s.exitDate).toLocaleDateString('tr-TR') : '-';
        const costStr = s.cost ? `<strong>${s.cost.toFixed(2)} TL</strong>` : '-';
        const invoiceStr = s.invoiceNumber ? `<br><small>${s.invoiceNumber}</small>` : '';
        const isCompleted = s.status === 'Tamamlandı';

        tr.innerHTML = `
            <td>${vInfo}</td>
            <td>${entryStr}</td>
            <td>${exitStr}</td>
            <td>${s.odometer} KM</td>
            <td>${s.serviceCompany}</td>
            <td>${s.failureDescription}</td>
            <td>${costStr}${invoiceStr}</td>
            <td><span class="badge ${isCompleted ? 'success' : 'danger'}">${s.status}</span></td>
            <td>
                ${!isCompleted ? `<button class="btn-primary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem;" onclick="openCompleteServiceModal(${s.vehicleServiceId}, ${s.vehicleId}, '${s.entryDate}', ${s.odometer}, '${s.serviceCompany}', '${s.failureDescription}')"><i class="fa-solid fa-check"></i> Çıkış</button>` : ''}
                <button class="btn-action edit" onclick="openEditServiceModal(${s.vehicleServiceId})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn-action delete" onclick="deleteService(${s.vehicleServiceId})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
    window._serviceData = data.items;
}

async function openEditServiceModal(id) {
    const s = window._serviceData.find(x => x.vehicleServiceId === id);
    if (!s) return;
    
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('service-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    
    document.getElementById('service-id').value = s.vehicleServiceId;
    document.getElementById('service-vehicle-select').value = s.vehicleId;
    document.getElementById('service-entry-date').value = s.entryDate.split('T')[0];
    document.getElementById('service-odometer').value = s.odometer;
    document.getElementById('service-company').value = s.serviceCompany;
    document.getElementById('service-desc').value = s.failureDescription;
    
    openModal('modal-service');
}

async function openAddServiceModal() {
    const vehicles = await apiFetch('/vehicles?PageSize=100');
    const select = document.getElementById('service-vehicle-select');
    select.innerHTML = '';
    if (vehicles?.items) {
        vehicles.items.forEach(v => {
            select.innerHTML += `<option value="${v.id}">${v.brand} ${v.model} [${v.plateNumber}]</option>`;
        });
    }
    document.getElementById('service-form').reset();
    document.getElementById('service-id').value = '';
    document.getElementById('service-entry-date').valueAsDate = new Date();
    openModal('modal-service');
}

async function handleServiceSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('service-id').value;
    const body = {
        vehicleId: parseInt(document.getElementById('service-vehicle-select').value),
        entryDate: new Date(document.getElementById('service-entry-date').value).toISOString(),
        odometer: parseInt(document.getElementById('service-odometer').value),
        serviceCompany: document.getElementById('service-company').value,
        failureDescription: document.getElementById('service-desc').value
    };

    let result;
    if (id) {
        body.id = parseInt(id);
        result = await apiFetch(`/vehicle-services/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    } else {
        result = await apiFetch('/vehicle-services', { method: 'POST', body: JSON.stringify(body) });
    }
    
    if (result !== null) {
        closeModal('modal-service');
        loadServiceExpenses();
    }
}

function openCompleteServiceModal(id, vehicleId, entryDate, odometer, company, desc) {
    document.getElementById('complete-service-id').value = id;
    document.getElementById('complete-service-vehicle-id').value = vehicleId;
    document.getElementById('complete-service-entry-date').value = entryDate;
    document.getElementById('complete-service-odometer').value = odometer;
    document.getElementById('complete-service-company').value = company;
    document.getElementById('complete-service-desc').value = desc;

    document.getElementById('complete-service-exit-date').valueAsDate = new Date();
    document.getElementById('complete-service-cost').value = '';
    document.getElementById('complete-service-invoice').value = '';
    
    openModal('modal-complete-service');
}

async function handleCompleteServiceSubmit(e) {
    e.preventDefault();
    const id = document.getElementById('complete-service-id').value;
    const body = {
        id: parseInt(id),
        vehicleId: parseInt(document.getElementById('complete-service-vehicle-id').value),
        entryDate: new Date(document.getElementById('complete-service-entry-date').value).toISOString(),
        exitDate: new Date(document.getElementById('complete-service-exit-date').value).toISOString(),
        odometer: parseInt(document.getElementById('complete-service-odometer').value),
        serviceCompany: document.getElementById('complete-service-company').value,
        failureDescription: document.getElementById('complete-service-desc').value,
        cost: parseFloat(document.getElementById('complete-service-cost').value),
        status: 'Tamamlandı',
        invoiceNumber: document.getElementById('complete-service-invoice').value
    };

    const result = await apiFetch(`/vehicle-services/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    if (result !== null) {
        closeModal('modal-complete-service');
        loadServiceExpenses();
    }
}

async function deleteService(id) {
    if (confirm('Bu servis kaydını silmek istediğinizden emin misiniz?')) {
        const result = await apiFetch(`/vehicle-services/${id}`, { method: 'DELETE' });
        if (result !== null) {
            loadServiceExpenses();
        }
    }
}

// Initial page load
window.addEventListener('DOMContentLoaded', () => {
    switchPage('dashboard');
});

// ================= REPORTS CONTROLLER =================

function switchReportTab(tabName) {
    document.querySelectorAll('#page-reports .sub-tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`#page-reports .sub-tab-btn[data-report="${tabName}"]`).classList.add('active');

    document.querySelectorAll('#page-reports .report-panel').forEach(panel => {
        panel.style.display = 'none';
        panel.classList.remove('active');
    });
    
    const target = document.getElementById(`report-${tabName}`);
    target.style.display = tabName === 'expenses' ? 'flex' : 'block';
    target.classList.add('active');
    
    if (tabName === 'all-vehicles') loadReportAllVehicles();
    else if (tabName === 'free-vehicles') loadReportFreeVehicles();
    else if (tabName === 'assigned-vehicles') loadReportAssignedVehicles();
    else if (tabName === 'expenses') loadReportExpenses();
}

async function loadReportAllVehicles() {
    const data = await apiFetch('/vehicles?PageSize=1000');
    const assignmentsData = await apiFetch('/vehicle-matches?PageSize=1000');
    const tbody = document.getElementById('report-all-vehicles-body');
    tbody.innerHTML = '';
    
    if (!data?.items?.length) {
        tbody.innerHTML = `<tr><td colspan="7" style="text-align: center;">Araç bulunamadı.</td></tr>`;
        return;
    }
    
    const assignedIds = new Set(assignmentsData?.items?.map(m => m.vehicleId) || []);
    
    data.items.forEach(v => {
        const isAssigned = assignedIds.has(v.id);
        tbody.innerHTML += `
            <tr>
                <td><strong>${v.plateNumber}</strong></td>
                <td>${v.brand} ${v.model}</td>
                <td>${v.year}</td>
                <td>${v.color || '-'}</td>
                <td>${v.fuelType || '-'}</td>
                <td>${v.transmissionType || '-'}</td>
                <td><span class="badge ${isAssigned ? 'warning' : 'success'}">${isAssigned ? 'Atanmış' : 'Boşta'}</span></td>
            </tr>
        `;
    });
}

async function loadReportFreeVehicles() {
    const data = await apiFetch('/vehicles?PageSize=1000');
    const assignmentsData = await apiFetch('/vehicle-matches?PageSize=1000');
    const tbody = document.getElementById('report-free-vehicles-body');
    tbody.innerHTML = '';
    
    const assignedIds = new Set(assignmentsData?.items?.map(m => m.vehicleId) || []);
    const freeVehicles = data?.items?.filter(v => !assignedIds.has(v.id)) || [];
    
    if (freeVehicles.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align: center;">Boşta araç bulunmuyor.</td></tr>`;
        return;
    }
    
    freeVehicles.forEach(v => {
        tbody.innerHTML += `
            <tr>
                <td><strong>${v.plateNumber}</strong></td>
                <td>${v.brand} ${v.model}</td>
                <td>${v.year}</td>
                <td>${v.color || '-'}</td>
                <td>${v.fuelType || '-'}</td>
            </tr>
        `;
    });
}

async function loadReportAssignedVehicles() {
    const vehiclesData = await apiFetch('/vehicles?PageSize=1000');
    const assignmentsData = await apiFetch('/vehicle-matches?PageSize=1000');
    
    const tbody = document.getElementById('report-assigned-vehicles-body');
    tbody.innerHTML = '';
    
    const assignedIds = new Set(assignmentsData?.items?.map(m => m.vehicleId) || []);
    const assignedVehicles = vehiclesData?.items?.filter(v => assignedIds.has(v.id)) || [];
    
    if (assignedVehicles.length === 0) {
        tbody.innerHTML = `<tr><td colspan="3" style="text-align: center;">Atanmış araç bulunmuyor.</td></tr>`;
        return;
    }
    
    assignedVehicles.forEach(v => {
        const match = assignmentsData?.items?.find(a => a.vehicleId === v.id);
        const personStr = match?.person ? `${match.person.name} ${match.person.surname} (${match.person.tckn})` : `Sürücü bilgisi yüklenemedi.`;
        
        tbody.innerHTML += `
            <tr>
                <td><strong>${v.plateNumber}</strong></td>
                <td>${v.brand} ${v.model}</td>
                <td>${personStr}</td>
            </tr>
        `;
    });
}

async function loadReportExpenses() {
    const fuels = await apiFetch('/vehicle-fuels?PageSize=1000') || { items: [] };
    const maints = await apiFetch('/vehicle-maintenances?PageSize=1000') || { items: [] };
    const services = await apiFetch('/vehicle-services?PageSize=1000') || { items: [] };
    
    // Yakıt
    let fuelHtml = '', fuelTotal = 0;
    fuels.items.forEach(f => {
        fuelTotal += f.totalPrice;
        const vInfo = f.vehicle ? `${f.vehicle.brand} ${f.vehicle.plateNumber}` : '-';
        fuelHtml += `<tr><td>${vInfo}</td><td>${new Date(f.refuelingDate).toLocaleDateString('tr-TR')}</td><td>${f.liters} Lt</td><td>${f.totalPrice.toFixed(2)} TL</td></tr>`;
    });
    document.getElementById('report-expenses-fuel-body').innerHTML = fuelHtml || `<tr><td colspan="4" style="text-align: center;">Kayıt yok.</td></tr>`;
    document.getElementById('report-fuel-total').innerText = `(Toplam: ${fuelTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TL)`;
    
    // Bakım
    let maintHtml = '', maintTotal = 0;
    maints.items.forEach(m => {
        maintTotal += m.cost;
        const vInfo = m.vehicle ? `${m.vehicle.brand} ${m.vehicle.plateNumber}` : '-';
        maintHtml += `<tr><td>${vInfo}</td><td>${new Date(m.maintenanceDate).toLocaleDateString('tr-TR')}</td><td>${m.maintenanceType}</td><td>${m.cost.toFixed(2)} TL</td></tr>`;
    });
    document.getElementById('report-expenses-maint-body').innerHTML = maintHtml || `<tr><td colspan="4" style="text-align: center;">Kayıt yok.</td></tr>`;
    document.getElementById('report-maint-total').innerText = `(Toplam: ${maintTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TL)`;
    
    // Servis
    let serviceHtml = '', serviceTotal = 0;
    services.items.forEach(s => {
        if(s.cost) serviceTotal += s.cost;
        const vInfo = s.vehicle ? `${s.vehicle.brand} ${s.vehicle.plateNumber}` : '-';
        serviceHtml += `<tr><td>${vInfo}</td><td>${new Date(s.entryDate).toLocaleDateString('tr-TR')}</td><td>${s.serviceCompany}</td><td>${s.cost ? s.cost.toFixed(2) + ' TL' : '-'}</td></tr>`;
    });
    document.getElementById('report-expenses-service-body').innerHTML = serviceHtml || `<tr><td colspan="4" style="text-align: center;">Kayıt yok.</td></tr>`;
    document.getElementById('report-service-total').innerText = `(Toplam: ${serviceTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TL)`;
}

// Generic Excel Export logic using SheetJS
function exportTableToExcel(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) return;

    // Create a new workbook and add the table as a worksheet
    const wb = XLSX.utils.table_to_book(table, { sheet: "Rapor" });
    
    // Download the Excel file
    XLSX.writeFile(wb, filename);
}


