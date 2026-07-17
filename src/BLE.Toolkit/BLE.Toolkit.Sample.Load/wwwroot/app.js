const els = {
  btnTransmitter: document.getElementById('btn-transmitter'),
  btnReceiver: document.getElementById('btn-receiver'),
  roleStatus: document.getElementById('role-status'),
  transmitterPanel: document.getElementById('transmitter-panel'),
  receiverPanel: document.getElementById('receiver-panel'),
  txMessage: document.getElementById('tx-message'),
  txCount: document.getElementById('tx-count'),
  btnSend: document.getElementById('btn-send'),
  txStatus: document.getElementById('tx-status'),
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
    ? 'Передатчик (сканирование + GATT central)'
  : role === 'receiver'
    ? 'Приёмник (GATT server + advertisement)'
    : 'не выбрана';

  els.roleStatus.textContent = `Роль: ${label}`;
}

function renderTransmitterStatus(status) {
  const tx = status.transmitter;
  if (!tx) {
    els.txStatus.textContent = '';
    return;
  }

  els.txStatus.innerHTML = `
    <div>Последнее сообщение: <strong>${escapeHtml(tx.lastMessage ?? '—')}</strong></div>
    <div>Поставлено в очередь: <strong>${tx.enqueuedCount}</strong> / ${tx.targetCount}</div>
    <div>Устройств в кэше: <strong>${tx.discoveredDevices}</strong></div>
  `;
}

function renderMessages(data) {
  els.rxCount.textContent = `Принято: ${data.totalCount}`;

  if (!data.messages.length) {
    els.messages.innerHTML = '<div class="message-item">Сообщений пока нет</div>';
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

function startPolling() {
  if (pollTimer) clearInterval(pollTimer);
  pollTimer = setInterval(() => {
    refreshStatus().catch(err => log(`Ошибка опроса: ${err.message}`));
  }, 1000);
}

async function setRole(role) {
  log(`Установка роли: ${role}`);
  const status = await api('/api/node/role', {
    method: 'POST',
    body: JSON.stringify({ role })
  });
  setRoleUi(status.role);
  log(`Роль активна: ${status.role}`);
}

async function sendTransmission() {
  const message = els.txMessage.value.trim();
  const count = Number(els.txCount.value);

  if (!message) {
    log('Введите сообщение');
    return;
  }

  if (!Number.isFinite(count) || count < 1) {
    log('Количество передач должно быть >= 1');
    return;
  }

  log(`Broadcast: "${message}" x ${count}`);
  const status = await api('/api/transmitter/send', {
    method: 'POST',
    body: JSON.stringify({ message, count })
  });
  renderTransmitterStatus(status);
  log(`В очередь поставлено ${status.transmitter?.enqueuedCount ?? 0} передач`);
}

async function clearMessages() {
  await api('/api/receiver/messages', { method: 'DELETE' });
  renderMessages({ totalCount: 0, messages: [] });
  log('Список принятых сообщений очищен');
}

els.btnTransmitter.addEventListener('click', () => setRole('transmitter').catch(err => log(err.message)));
els.btnReceiver.addEventListener('click', () => setRole('receiver').catch(err => log(err.message)));
els.btnSend.addEventListener('click', () => sendTransmission().catch(err => log(err.message)));
els.btnClear.addEventListener('click', () => clearMessages().catch(err => log(err.message)));

startPolling();
refreshStatus().catch(err => log(`Старт: ${err.message}`));
