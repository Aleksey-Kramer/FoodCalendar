// Drag-and-drop и вставка из буфера (Ctrl+V) для <InputFile>.
// Файл подкладывается в input и диспатчится change, поэтому серверная логика InputFile не меняется.
window.dropZone = {
  init(zone) {
    const input = zone.querySelector('input[type=file]');
    if (!input) return { dispose() {} };

    const setFiles = (files) => {
      const dt = new DataTransfer();
      for (const f of files) {
        if (f.type.startsWith('image/')) { dt.items.add(f); break; }
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
    const onPaste = (e) => {
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
      }
    };
  }
};
