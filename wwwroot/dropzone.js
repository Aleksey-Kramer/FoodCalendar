// Drag-and-drop и вставка из буфера (Ctrl+V) для <InputFile>.
// Файл подкладывается в input и диспатчится change, поэтому серверная логика InputFile не меняется.
// opts.scoped — зона принимает вставку, только когда она наведена/в фокусе или она единственная на странице
// (на странице дня несколько чеков, картинка не должна прикрепиться ко всем сразу).
// opts.pdf — кроме картинок принимать PDF (файлы чеков).
window.dropZone = {
  _scoped: new Set(),

  init(zone, opts) {
    const input = zone.querySelector('input[type=file]');
    if (!input) return { dispose() {} };
    const scoped = !!(opts && opts.scoped);
    const allowPdf = !!(opts && opts.pdf);
    if (scoped) this._scoped.add(zone);

    const accepts = (f) => f.type.startsWith('image/') || (allowPdf && f.type === 'application/pdf');
    const setFiles = (files) => {
      const dt = new DataTransfer();
      for (const f of files) {
        if (accepts(f)) { dt.items.add(f); break; }
      }
      if (dt.files.length === 0) return false;
      input.files = dt.files;
      input.dispatchEvent(new Event('change', { bubbles: true }));
      return true;
    };

    const onDragOver = (e) => { e.preventDefault(); zone.classList.add('dropzone--over'); };
    const onDragLeave = () => zone.classList.remove('dropzone--over');
    const onDrop = (e) => {
      e.preventDefault();
      zone.classList.remove('dropzone--over');
      if (e.dataTransfer?.files?.length) setFiles(e.dataTransfer.files);
    };
    const isTarget = () => {
      if (!scoped) return true;
      if (window.dropZone._scoped.size === 1) return true;
      return zone.matches(':hover') || zone.contains(document.activeElement);
    };
    const onPaste = (e) => {
      // вставка в текстовое поле — не наша
      const t = e.target;
      if (t instanceof HTMLInputElement && t.type !== 'file' || t instanceof HTMLTextAreaElement) return;
      if (!isTarget()) return;
      const files = e.clipboardData?.files;
      if (files && files.length && setFiles(files)) e.preventDefault();
    };

    zone.addEventListener('dragover', onDragOver);
    zone.addEventListener('dragleave', onDragLeave);
    zone.addEventListener('drop', onDrop);
    document.addEventListener('paste', onPaste);

    return {
      dispose() {
        zone.removeEventListener('dragover', onDragOver);
        zone.removeEventListener('dragleave', onDragLeave);
        zone.removeEventListener('drop', onDrop);
        document.removeEventListener('paste', onPaste);
        window.dropZone._scoped.delete(zone);
      }
    };
  }
};
