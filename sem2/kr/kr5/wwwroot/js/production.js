const state = {
  materials: [],
  products: [],
  lines: [],
  orders: []
};

const api = async (url, options = {}) => {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json", ...(options.headers || {}) },
    ...options
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Ошибка запроса");
  }
  return response.status === 204 ? null : response.json();
};

const formatDate = value => new Date(value).toLocaleString("ru-RU", {
  day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit"
});

const statusLabel = status => ({
  Pending: "Ожидает", InProgress: "В работе", Completed: "Готово", Cancelled: "Отменён"
}[status] || status);

const showToast = message => {
  const toast = document.getElementById("toast");
  toast.textContent = message;
  toast.hidden = false;
  setTimeout(() => toast.hidden = true, 3500);
};

async function loadAll() {
  const lowOnly = document.getElementById("lowStockOnly").checked;
  const category = document.getElementById("categoryFilter").value;
  const search = document.getElementById("productSearch").value;
  const productQuery = new URLSearchParams();
  if (category) productQuery.set("category", category);
  if (search) productQuery.set("search", search);

  const [materials, products, categories, lines, availableLines, orders] = await Promise.all([
    api(`/api/materials${lowOnly ? "?low_stock=true" : ""}`),
    api(`/api/products?${productQuery}`),
    api("/api/products/categories"),
    api("/api/lines"),
    api("/api/lines?available=true"),
    api("/api/orders?status=active")
  ]);

  state.materials = materials;
  state.products = products;
  state.lines = lines;
  state.availableLines = availableLines;
  state.orders = orders;

  renderCategories(categories);
  renderMaterials();
  renderProducts();
  renderProductMaterialInputs();
  renderOrderForm();
  renderOrders();
  renderLines();
}

function renderCategories(categories) {
  const select = document.getElementById("categoryFilter");
  const current = select.value;
  select.innerHTML = `<option value="">Все категории</option>` + categories.map(x => `<option value="${x}">${x}</option>`).join("");
  select.value = current;
}

function renderMaterials() {
  document.getElementById("materialsTable").innerHTML = state.materials.map(m => `
    <tr>
      <td>${m.name}</td>
      <td><span class="stock-pill ${m.isLowStock ? "stock-low" : "stock-ok"}">${m.quantity} / ${m.minStock}</span></td>
      <td>${m.unit}</td>
      <td><button class="btn btn-soft" type="button" onclick="replenishMaterial(${m.id})">Пополнить</button></td>
    </tr>
  `).join("");
}

function renderProducts() {
  document.getElementById("productsTable").innerHTML = state.products.map(p => `
    <tr onclick="showProductMaterials(${p.id})">
      <td>${p.name}</td><td>${p.prodTime}</td><td>${p.category}</td>
    </tr>
  `).join("");
}

function renderProductMaterialInputs() {
  const container = document.getElementById("productMaterials");
  container.innerHTML = state.materials.map(m => `
    <label class="material-pick">
      <span>${m.name}, ${m.unit}</span>
      <input type="number" step="0.001" min="0" data-material="${m.id}" placeholder="На 1 шт">
    </label>
  `).join("");
}

function renderOrderForm() {
  const productSelect = document.getElementById("orderProduct");
  const selectedProduct = productSelect.value;
  productSelect.innerHTML = state.products.map(p => `<option value="${p.id}">${p.name}</option>`).join("");
  if (selectedProduct) productSelect.value = selectedProduct;

  const lineSelect = document.getElementById("orderLine");
  const selectedLine = lineSelect.value;
  lineSelect.innerHTML = `<option value="">Без линии</option>` + state.availableLines.map(l => `<option value="${l.id}">${l.name}, k=${l.efficiencyFactor}</option>`).join("");
  if (selectedLine) lineSelect.value = selectedLine;
  updateOrderCalculation();
}

function renderOrders() {
  document.getElementById("ordersTable").innerHTML = state.orders.map(o => `
    <tr>
      <td>#${o.id}</td><td>${o.product}</td><td>${o.quantity}</td>
      <td><span class="badge-status status-${o.status.toLowerCase()}">${statusLabel(o.status)}</span></td>
      <td>${formatDate(o.estimatedEndDate)}</td>
      <td>
        ${o.status === "Pending" && (!o.line || o.line === "Не назначена") ? `<button class="btn btn-soft" type="button" onclick="assignOrder(${o.id})">Назначить</button>` : ""}
        ${o.status === "Pending" && o.line !== "Не назначена" ? `<button class="btn btn-soft" type="button" onclick="startOrder(${o.id})">Запустить</button>` : ""}
        <button class="btn btn-soft" type="button" onclick="cancelOrder(${o.id})">Отменить</button>
        <button class="btn btn-soft" type="button" onclick="showOrderDetails(${o.id})">Детали</button>
      </td>
    </tr>
  `).join("");
}

async function renderLines() {
  const grid = document.getElementById("linesGrid");
  const schedules = await Promise.all(state.lines.map(l => api(`/api/lines/${l.id}/schedule`)));
  grid.innerHTML = state.lines.map((line, i) => `
    <section class="line-card">
      <div class="panel-title">
        <div>
          <h3>${line.name}</h3>
          <span class="badge-status ${line.status === "Active" ? "line-active" : "line-stopped"}">${line.status === "Active" ? "Работает" : "Остановлена"}</span>
        </div>
        <strong>k=${Number(line.efficiencyFactor).toFixed(2)}</strong>
      </div>
      <div>${line.currentProduct || "Текущий продукт не назначен"}</div>
      <div class="progress-track"><div class="progress-fill" style="width:${line.progress}%"></div></div>
      <input type="range" min="0.5" max="2" step="0.05" value="${line.efficiencyFactor}" onchange="setEfficiency(${line.id}, this.value)">
      <div class="line-actions">
        <button class="btn btn-soft" type="button" onclick="setLineStatus(${line.id}, 'Active')">Запустить линию</button>
        <button class="btn btn-soft" type="button" onclick="setLineStatus(${line.id}, 'Stopped')">Остановить</button>
        <label style="margin-left: 10px;">
          Авто: <input type="checkbox" ${line.isAutomatic ? 'checked' : ''} onchange="setAutoMode(${line.id}, this.checked)">
        </label>
      </div>
      <div class="schedule">
        ${schedules[i].length ? schedules[i].map(o => `
          <label>#${o.id} ${o.product}, ${formatDate(o.startDate)}
            <input type="datetime-local" onchange="rescheduleOrder(${o.id}, this.value)">
          </label>
        `).join("") : "Расписание пустое"}
      </div>
    </section>
  `).join("");
}

function updateOrderCalculation() {
  const product = state.products.find(x => x.id == document.getElementById("orderProduct").value);
  const line = state.availableLines.find(x => x.id == document.getElementById("orderLine").value);
  const quantity = Number(document.getElementById("orderQuantity").value || 0);
  if (!product || !quantity) return;
  const efficiency = line?.efficiencyFactor || 1;
  const minutes = Math.ceil((quantity * product.prodTime) / efficiency);
  document.getElementById("orderCalculation").textContent = `Расчёт: ${quantity} x ${product.prodTime} мин / ${efficiency} = ${minutes} мин`;
}

async function replenishMaterial(id) {
  const amount = Number(prompt("На сколько увеличить количество?", "50"));
  if (!amount) return;
  await api(`/api/materials/${id}/stock`, { method: "PUT", body: JSON.stringify({ amount }) });
  showToast("Материал пополнен");
  loadAll();
}

async function showProductMaterials(id) {
  const materials = await api(`/api/products/${id}/materials`);
  showToast(materials.length ? materials.map(x => `${x.name}: ${x.quantityNeeded} ${x.unitOfMeasure}`).join("; ") : "Для продукта материалы ещё не заданы");
}

async function startOrder(id) {
  const order = state.orders.find(x => x.id === id);
  if (!order) return;
  if (!order.line || order.line === "Не назначена") {
    showToast("Сначала назначьте заказ на линию");
    return;
  }
  await api(`/api/orders/${id}/start`, { method: "PUT" });
  showToast("Заказ запущен в производство");
  loadAll();
}

async function assignOrder(id) {
  const availableLines = await api("/api/lines?available=true");
  if (availableLines.length === 0) { showToast("Нет свободных линий"); return; }
  const lineId = prompt(`Доступные линии:\n${availableLines.map((l, i) => `${i+1}. ${l.name}`).join('\n')}\n\nВведите номер:`);
  if (!lineId) return;
  const index = parseInt(lineId) - 1;
  if (index < 0 || index >= availableLines.length) { showToast("Неверный номер линии"); return; }
  await api(`/api/orders/${id}/assign`, { method: "PUT", body: JSON.stringify({ lineId: availableLines[index].id }) });
  showToast("Заказ назначен на линию");
  loadAll();
}

async function cancelOrder(id) {
  await api(`/api/orders/${id}/cancel`, { method: "PUT", body: "{}" });
  showToast("Заказ отменён");
  loadAll();
}

async function showOrderDetails(id) {
  const order = await api(`/api/orders/${id}/details`);
  document.getElementById("detailsContent").innerHTML = `
    <h2>Заказ #${order.id}</h2>
    <p><b>Продукт:</b> ${order.product}</p>
    <p><b>Линия:</b> ${order.line || "не назначена"}</p>
    <p><b>Количество:</b> ${order.quantity}</p>
    <p><b>Статус:</b> ${statusLabel(order.status)}, ${order.progressPercent}%</p>
    <p><b>Срок:</b> ${formatDate(order.estimatedEndDate)}</p>
    <h3>Материалы</h3>
    <ul>${order.materials.map(x => `<li>${x.name}: ${x.total} ${x.unitOfMeasure}</li>`).join("")}</ul>
  `;
  document.getElementById("detailsDialog").showModal();
}

async function setLineStatus(id, status) {
  await api(`/api/lines/${id}/status`, { method: "PUT", body: JSON.stringify({ status }) });
  showToast("Статус линии обновлён");
  loadAll();
}

async function setEfficiency(id, efficiencyFactor) {
  await api(`/api/lines/${id}/efficiency`, { method: "PUT", body: JSON.stringify({ efficiencyFactor: Number(efficiencyFactor) }) });
  showToast("Коэффициент эффективности сохранён");
  loadAll();
}

async function setAutoMode(id, isAutomatic) {
  await api(`/api/lines/${id}/automatic`, { method: "PUT", body: JSON.stringify({ isAutomatic }) });
  showToast(isAutomatic ? "Автоматический режим включён" : "Автоматический режим выключен");
  loadAll();
}

async function rescheduleOrder(id, value) {
  if (!value) return;
  await api(`/api/orders/${id}/reschedule`, { method: "PUT", body: JSON.stringify({ startDate: value }) });
  showToast("Срок перенесён");
  loadAll();
}

document.getElementById("materialForm").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.target);
  await api("/api/materials", {
    method: "POST",
    body: JSON.stringify({ name: form.get("name"), quantity: Number(form.get("quantity")), unit: form.get("unit"), minStock: Number(form.get("minStock")) })
  });
  event.target.reset();
  showToast("Материал добавлен");
  loadAll();
});

document.getElementById("productForm").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.target);
  const materials = [...document.querySelectorAll("#productMaterials input")]
    .map(input => ({ materialId: Number(input.dataset.material), quantityNeeded: Number(input.value || 0) }))
    .filter(x => x.quantityNeeded > 0);
  await api("/api/products", {
    method: "POST",
    body: JSON.stringify({
      name: form.get("name"), prodTime: Number(form.get("prodTime")), category: form.get("category"),
      minimalStock: Number(form.get("minimalStock") || 0), description: form.get("description"),
      specifications: form.get("specifications"), materials
    })
  });
  event.target.reset();
  showToast("Продукт создан");
  loadAll();
});

document.getElementById("orderForm").addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(event.target);
  try {
    await api("/api/orders", {
      method: "POST",
      body: JSON.stringify({
        productId: Number(form.get("productId")), quantity: Number(form.get("quantity")),
        lineId: form.get("lineId") ? Number(form.get("lineId")) : null, startDate: form.get("startDate") || null
      })
    });
    showToast("Заказ создан, материалы списаны");
    loadAll();
  } catch (error) { showToast(error.message); }
});

["lowStockOnly", "categoryFilter", "productSearch", "orderProduct", "orderQuantity", "orderLine"].forEach(id => {
  document.getElementById(id).addEventListener("input", id.startsWith("order") ? updateOrderCalculation : loadAll);
});

document.getElementById("refreshBtn").addEventListener("click", loadAll);
loadAll().catch(error => showToast(error.message));
setInterval(loadAll, 2000); // Обновление каждые 2 секунды для отображения прогресса
