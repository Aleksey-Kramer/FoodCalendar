// Имя пользователя для голосования хранится в браузере (авторизации нет).
window.userName = {
  get() { try { return localStorage.getItem('fc.userName') || ''; } catch { return ''; } },
  set(name) { try { localStorage.setItem('fc.userName', name || ''); } catch { } }
};
