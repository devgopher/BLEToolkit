const els = {
  btnTransmitter: document.getElementById('btn-transmitter'),
  btnReceiver: document.getElementById('btn-receiver'),
  roleStatus: document.getElementById('role-status'),
  transmitterPanel: document.getElementById('transmitter-panel'),
  receiverPanel: document.getElementById('receiver-panel'),
  txMessage: document.getElementById('tx-message'),
  txCount: document.getElementById('tx-count'),
  throttleEnabled: document.getElementById('throttle-enabled'),
  throttlePeriod: document.getElementById('throttle-period'),
  throttleLimit: document.getElementById('throttle-limit'),
  btnThrottleApply: document.getElementById('btn-throttle-apply'),
  throttleStatus: document.getElementById('throttle-status'),
  btnSend: document.getElementById('btn-send'),
  txStatus: document.getElementById('tx-status'),
  txDevices: document.getElementById('tx-devices'),
  rxCount: document.getElementById('rx-count'),
  btnClear: document.getElementById('btn-clear'),
  messages: document.getElementById('messages'),
  log: document.getElementById('log')
};

let currentRole = 'none';
let pollTimer = null;

function log(message) {
  const time = new Date().toLocaleTimeString();
  els.log.textContent = `[${time}] ${message}\n` + els.log.textContent;
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });

  if (!response.ok) {
    let error = `HTTP ${response.status}`;
    try {
      const body = await response.json();
      if (body.error) error = body.error;
    } catch {
      // ignore
    }
    throw new Error(error);
  }

  if (response.status === 204) return null;
  return response.json();
}

function setRoleUi(role) {
  currentRole = role;
  els.btnTransmitter.classList.toggle('active', role === 'transmitter');
  els.btnReceiver.classList.toggle('active', role === 'receiver');
  els.transmitterPanel.classList.toggle('hidden', role !== 'transmitter');
  els.receiverPanel.classList.toggle('hidden', role !== 'receiver');

  const label = role === 'transmitter'
    ? 'Transmitter (scanning + GATT central)'
    : role === 'receiver'
      ? 'Receiver (GATT server + advertisement)'
      : 'not selected';

  els.roleStatus.textContent = `Role: ${label}`;
}

function formatThrottling(throttling) {
  if (!throttling?.enabled) {
    return 'Throttling disabled';
  }

  return `Throttling: ${throttling.limit} transmissions per ${throttling.ratePeriod}`;
}

function renderThrottlingControls(throttling) {
  if (!throttling) return;

  els.throttleEnabled.checked = throttling.enabled;
  els.throttlePeriod.value = throttling.ratePeriod;
  els.throttleLimit.value = throttling.limit;
  els.throttleStatus.textContent = formatThrottling(throttling);
}

function renderTransmitterStatus(status) {
  const tx = status.transmitter;
  if (!tx) {
    els.txStatus.textContent = '';
    els.txDevices.textContent = '';
    return;
  }

  renderThrottlingControls(tx.throttling);

  els.txStatus.innerHTML = `
    <div>Last message: <strong>${escapeHtml(tx.lastMessage ?? '—')}</strong></div>
    <div>Enqueued: <strong>${tx.enqueuedCount}</strong> / ${tx.targetCount}</div>
    <div>Devices in cache: <strong>${tx.discoveredDevices}</strong></div>
    <div>${escapeHtml(formatThrottling(tx.throttling))}</div>
  `;

  renderCachedDevices(tx.devices ?? []);
}

function renderCachedDevices(devices) {
  if (!devices.length) {
    els.txDevices.innerHTML = '<div class="device-item muted">No devices discovered yet</div>';
    return;
  }

  els.txDevices.innerHTML = `
    <div class="device-row device-header">
      <span>Bluetooth address</span>
      <span>Device name</span>
    </div>
    ${devices.map(device => `
      <div class="device-row">
        <span class="device-address">${escapeHtml(device.bluetoothAddress)}</span>
        <span>${escapeHtml(device.localName ?? '—')}</span>
      </div>
    `).join('')}
  `;
}

function renderMessages(data) {
  els.rxCount.textContent = `Received: ${data.totalCount}`;

  if (!data.messages.length) {
    els.messages.innerHTML = '<div class="message-item">No messages yet</div>';
    return;
  }

  els.messages.innerHTML = data.messages
    .map(m => `
      <div class="message-item">
        <div class="message-meta">#${m.index} · ${new Date(m.receivedAt).toLocaleString()}</div>
        <div>${escapeHtml(m.text)}</div>
      </div>
    `)
    .join('');
}

function escapeHtml(value) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;');
}

async function refreshStatus() {
  const status = await api('/api/node/status');
  setRoleUi(status.role);

  if (status.role === 'transmitter') {
    renderTransmitterStatus(status);
  }

  if (status.role === 'receiver') {
    const messages = await api('/api/receiver/messages?skip=0&take=200');
    renderMessages(messages);
  }
}

async function loadThrottling() {
  const throttling = await api('/api/transmitter/throttling');
  renderThrottlingControls(throttling);
}

function startPolling() {
  if (pollTimer) clearInterval(pollTimer);
  pollTimer = setInterval(() => {
    refreshStatus().catch(err => log(`Poll error: ${err.message}`));
  }, 1000);
}

async function setRole(role) {
  log(`Setting role: ${role}`);
  const status = await api('/api/node/role', {
    method: 'POST',
    body: JSON.stringify({ role })
  });
  setRoleUi(status.role);
  log(`Active role: ${status.role}`);

  if (role === 'transmitter') {
    await loadThrottling();
  }
}

async function applyThrottling() {
  const enabled = els.throttleEnabled.checked;
  const ratePeriod = els.throttlePeriod.value;
  const limit = Number(els.throttleLimit.value);

  if (!Number.isFinite(limit) || limit < 1) {
    log('Throttling limit must be >= 1');
    return;
  }

  const throttling = await api('/api/transmitter/throttling', {
    method: 'PUT',
    body: JSON.stringify({ enabled, ratePeriod, limit })
  });

  renderThrottlingControls(throttling);
  log(formatThrottling(throttling));
}

async function sendTransmission() {
  const message = els.txMessage.value.trim();
  const count = Number(els.txCount.value);

  if (!message) {
    log('Enter a message');
    return;
  }

  if (!Number.isFinite(count) || count < 1) {
    log('Transmission count must be >= 1');
    return;
  }

  log(`Broadcast: "${message}" x ${count}`);
  const status = await api('/api/transmitter/send', {
    method: 'POST',
    body: JSON.stringify({ message, count })
  });
  renderTransmitterStatus(status);
  log(`Enqueued ${status.transmitter?.enqueuedCount ?? 0} transmissions`);
}

async function clearMessages() {
  await api('/api/receiver/messages', { method: 'DELETE' });
  renderMessages({ totalCount: 0, messages: [] });
  log('Received messages cleared');
}

els.btnTransmitter.addEventListener('click', () => setRole('transmitter').catch(err => log(err.message)));
els.btnReceiver.addEventListener('click', () => setRole('receiver').catch(err => log(err.message)));
els.btnThrottleApply.addEventListener('click', () => applyThrottling().catch(err => log(err.message)));
els.btnSend.addEventListener('click', () => sendTransmission().catch(err => log(err.message)));
els.btnClear.addEventListener('click', () => clearMessages().catch(err => log(err.message)));

startPolling();
loadThrottling().catch(err => log(`Startup throttling: ${err.message}`));
refreshStatus().catch(err => log(`Startup: ${err.message}`));
