// Имя пользователя для голосования хранится в браузере (авторизации нет).
window.userName = {
  get() { try { return localStorage.getItem('fc.userName') || ''; } catch { return ''; } },
  set(name) { try { localStorage.setItem('fc.userName', name || ''); } catch { } }
};

// Поля сумм (class="money"): разряды через пробел, дробная часть через запятую, курсор остаётся на месте.
(function () {
  function formatMoney(s) {
    s = s.replace(/\./g, ',').replace(/[^\d,]/g, '');
    const k = s.indexOf(',');
    let int = k < 0 ? s : s.slice(0, k);
    const frac = k < 0 ? null : s.slice(k + 1).replace(/,/g, '').slice(0, 2);
    int = int.replace(/^0+(?=\d)/, '').replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    return frac === null ? int : int + ',' + frac;
  }
  document.addEventListener('input', e => {
    const el = e.target;
    if (!(el instanceof HTMLInputElement) || !el.classList.contains('money')) return;
    const raw = el.value;
    const pos = el.selectionStart ?? raw.length;
    const sig = raw.slice(0, pos).replace(/[^\d.,]/g, '').length;
    const out = formatMoney(raw);
    if (out === raw) return;
    el.value = out;
    let i = 0, n = 0;
    while (i < out.length && n < sig) { if (/[\d,]/.test(out[i])) n++; i++; }
    el.setSelectionRange(i, i);
  });
})();

// Формы с data-arrow-nav: стрелки ← → переключают поля (в текстовых — когда курсор у края).
document.addEventListener('keydown', e => {
  if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;
  if (e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
  const el = e.target;
  if (!(el instanceof Element)) return;
  const form = el.closest('form[data-arrow-nav]');
  if (!form) return;
  const right = e.key === 'ArrowRight';
  const isText = el instanceof HTMLInputElement && (el.type === 'text' || el.type === 'search');
  if (isText) {
    const s = el.selectionStart, en = el.selectionEnd;
    if (s === null || s !== en) return;
    if (right ? s < el.value.length : s > 0) return;
  }
  const items = [...form.querySelectorAll('input, select, button, textarea')]
    .filter(x => !x.disabled && x.type !== 'hidden' && x.offsetParent !== null);
  const i = items.indexOf(el);
  if (i < 0) return;
  const next = items[i + (right ? 1 : -1)];
  if (!next) return;
  e.preventDefault();
  next.focus();
  if (next instanceof HTMLInputElement && (next.type === 'text' || next.type === 'search')) {
    const p = right ? 0 : next.value.length;
    next.setSelectionRange(p, p);
  }
});

// Поле с подсказками (SuggestInput): при открытом списке Enter выбирает пункт, а не отправляет форму;
// стрелки ↑↓ не двигают курсор.
document.addEventListener('keydown', e => {
  const el = e.target;
  if (!(el instanceof HTMLInputElement) || !el.classList.contains('suggest__input')) return;
  if (el.dataset.open !== 'true') return;
  if (e.key === 'Enter' || e.key === 'ArrowDown' || e.key === 'ArrowUp') e.preventDefault();
});

// Язык и тема: cookie на год. Язык требует перезагрузки (интерфейс рендерится на сервере),
// тема переключается сразу через data-theme на <html>.
window.prefs = {
  cookie(name, value) { document.cookie = name + '=' + value + ';path=/;max-age=31536000;SameSite=Lax'; },
  setLang(v) { this.cookie('fc.lang', v); location.reload(); },
  toggleTheme() {
    const root = document.documentElement;
    const current = root.dataset.theme || (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    const next = current === 'dark' ? 'light' : 'dark';
    root.dataset.theme = next;
    this.cookie('fc.theme', next);
  }
};
