/* 
   La Liga Admin - admin-teams.js
   Takım Yönetimi: ekle/düzenle/sil modalları (istemci doğrulaması + fetch ile kaydetme).
    */
(function () {
    'use strict';

    const form = document.querySelector('[data-team-form]');
    if (!form) return; // yalnız Takımlar sayfasında çalışır

    const $ = (sel, root = document) => root.querySelector(sel);
    const f = form.elements;

    const els = {
        modal: $('#team-modal'),
        title: $('#team-modal-title'),
        alert: $('[data-form-alert]'),
        del: $('#delete-modal'),
        delText: $('[data-delete-text]')
    };

    const preview = {
        crest: $('[data-preview-crest]'),
        name: $('[data-preview-name]'),
        meta: $('[data-preview-meta]'),
        colorA: $('[data-color-a]'),
        colorB: $('[data-color-b]'),
        statusHint: $('[data-status-hint]')
    };

    /* Arma yazısı: ana rengin açıklığına göre koyu/açık */
    function inkFor(hex) {
        const h = hex.replace('#', '');
        const [r, g, b] = [0, 2, 4].map((i) => parseInt(h.slice(i, i + 2), 16));
        return (0.299 * r + 0.587 * g + 0.114 * b) > 170 ? '#10224A' : '#FFFFFF';
    }

    function updatePreview() {
        const a = f.colorA.value.toUpperCase(), b = f.colorB.value.toUpperCase();
        preview.crest.setAttribute('style', `--crest-a:${a};--crest-b:${b};--crest-ink:${inkFor(a)}`);
        preview.crest.textContent = (f.code.value || '---').toUpperCase();
        preview.name.textContent = f.name.value.trim() || 'Takım adı';
        const meta = [f.city.value.trim(), f.stadium.value.trim()].filter(Boolean).join(', ');
        preview.meta.textContent = meta || 'Şehir, stadyum';
        preview.colorA.textContent = a;
        preview.colorB.textContent = b;
    }

    // Lig şeridindeki rakamlardan okur: canlı bir takım listesi tutmuyoruz
    function updateStatusHint() {
        const strip = document.querySelector('.league-strip');
        const activeCount = Number(strip?.dataset.activeCount || 0);
        const maxActive = Number(strip?.dataset.maxActive || 20);
        const editingId = f.id.value;
        const editingRow = editingId ? document.querySelector(`[data-id="${editingId}"]`) : null;
        const editingWasActive = editingRow && editingRow.dataset.active === 'true';
        const used = activeCount - (editingWasActive ? 1 : 0);
        preview.statusHint.textContent = `Ligde şu anda ${activeCount} aktif takım var (sınır ${maxActive}).` +
            (used >= maxActive ? ' Yeni aktif takım için önce bir takımı pasif yapın.' : '');
    }

    f.code.addEventListener('input', () => { f.code.value = f.code.value.toLocaleUpperCase('tr-TR').replace(/[^A-Z]/g, ''); });
    ['name', 'code', 'city', 'stadium', 'colorA', 'colorB'].forEach((n) => f[n].addEventListener('input', updatePreview));
    ['name', 'code', 'city', 'region', 'stadium', 'capacity', 'founded', 'status'].forEach((n) =>
        f[n].addEventListener('input', () => clearError(n)));
    ['colorA', 'colorB'].forEach((n) => f[n].addEventListener('input', () => clearError('colors')));

    /*  Alan hataları  */
    function setError(name, msg) {
        const field = form.querySelector(`[data-field="${name}"]`);
        if (!field) return;
        field.classList.add('has-error');
        const err = field.querySelector('.field-error');
        if (err) err.textContent = msg;
    }
    function clearError(name) {
        const field = form.querySelector(`[data-field="${name}"]`);
        if (!field) return;
        field.classList.remove('has-error');
    }
    function clearAllErrors() {
        form.querySelectorAll('.has-error').forEach((el) => el.classList.remove('has-error'));
        els.alert.classList.remove('is-visible');
    }

    /*  İstemci tarafı doğrulama (biçim/zorunluluk; isim-kod tekrarı ve aktif sınırı sunucuda)  */
    function validate() {
        const errors = [];
        const name = f.name.value.trim();
        const code = f.code.value.trim();

        if (!name) errors.push(['name', 'Lütfen takım adını girin.']);
        else if (name.length < 2) errors.push(['name', 'Takım adı en az 2 karakter olmalı.']);

        if (!code) errors.push(['code', 'Lütfen 3 harfli kısa adı girin.']);
        else if (code.length !== 3) errors.push(['code', 'Kısa ad tam olarak 3 harften oluşmalı.']);

        if (!f.city.value.trim()) errors.push(['city', 'Lütfen şehir girin.']);
        if (!f.region.value) errors.push(['region', 'Lütfen bölge seçin.']);
        if (!f.stadium.value.trim()) errors.push(['stadium', 'Lütfen stadyum adını girin.']);

        const cap = Number(f.capacity.value);
        if (f.capacity.value === '') errors.push(['capacity', 'Lütfen stadyum kapasitesini girin.']);
        else if (!Number.isInteger(cap) || cap < 1000 || cap > 100000) errors.push(['capacity', 'Kapasite 1.000 ile 100.000 arasında olmalı.']);

        const year = Number(f.founded.value);
        if (f.founded.value === '') errors.push(['founded', 'Lütfen kuruluş yılını girin.']);
        else if (!Number.isInteger(year) || year < 1850 || year > 2026) errors.push(['founded', 'Kuruluş yılı 1850 ile 2026 arasında olmalı.']);

        if (f.colorA.value.toUpperCase() === f.colorB.value.toUpperCase()) errors.push(['colors', 'Ana renk ve ikinci renk aynı olamaz.']);

        return errors;
    }

    /*  Modal aç / kapat  */
    let lastFocus = null;
    function openCreate() {
        lastFocus = document.activeElement;
        form.reset();
        clearAllErrors();
        f.id.value = '';
        const strip = document.querySelector('.league-strip');
        const activeCount = Number(strip?.dataset.activeCount || 0);
        const maxActive = Number(strip?.dataset.maxActive || 20);
        f.status.value = activeCount >= maxActive ? 'inactive' : 'active';
        els.title.textContent = 'Yeni Takım Ekle';
        updatePreview();
        updateStatusHint();
        els.modal.showModal();
        f.name.focus();
    }
    function openEdit(row) {
        lastFocus = document.activeElement;
        form.reset();
        clearAllErrors();
        const d = row.dataset;
        f.id.value = d.id;
        f.name.value = d.name;
        f.code.value = d.code;
        f.city.value = d.city;
        f.region.value = d.region;
        f.stadium.value = d.stadium;
        f.capacity.value = d.capacity;
        f.founded.value = d.founded;
        f.colorA.value = d.colorA;
        f.colorB.value = d.colorB;
        f.status.value = d.active === 'true' ? 'active' : 'inactive';
        els.title.textContent = 'Takımı Düzenle';
        updatePreview();
        updateStatusHint();
        els.modal.showModal();
        f.name.focus();
    }
    function closeModal() { els.modal.close(); }
    els.modal.addEventListener('close', () => { if (lastFocus) lastFocus.focus(); });
    els.modal.addEventListener('click', (e) => { if (e.target === els.modal) closeModal(); });
    document.querySelectorAll('[data-modal-close]').forEach((b) => b.addEventListener('click', closeModal));
    const openBtn = $('[data-open-create]');
    if (openBtn) openBtn.addEventListener('click', openCreate);

    function mapErrorField(apiField) {
        const map = { foundedyear: 'founded' };
        return map[apiField.toLowerCase()] || apiField.toLowerCase();
    }

    /*  Kaydet  */
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        clearAllErrors();
        const errors = validate();
        if (errors.length) {
            errors.forEach(([name, msg]) => setError(name, msg));
            els.alert.textContent = 'Lütfen işaretli alanları kontrol edin.';
            els.alert.classList.add('is-visible');
            (f[errors[0][0]] || f.colorA).focus();
            return;
        }

        const submitBtn = form.querySelector('[data-submit-label]');
        submitBtn.disabled = true;

        try {
            const res = await fetch('/Admin/Teams/Save', { method: 'POST', body: new FormData(form) });
            const data = await res.json();

            if (!data.ok) {
                if (data.errors) {
                    Object.entries(data.errors).forEach(([name, msgs]) => setError(mapErrorField(name), msgs[0]));
                }
                els.alert.textContent = data.message || 'Kaydedilemedi.';
                els.alert.classList.add('is-visible');
                return;
            }

            const wasEdit = !!f.id.value;
            closeModal();
            window.Admin.toast(wasEdit ? 'Takım başarıyla güncellendi.' : 'Takım başarıyla eklendi.');
            setTimeout(() => location.reload(), 700);
        } catch (err) {
            els.alert.textContent = 'API\u2019ye ulaşılamıyor. API projesinin (LaLiga) çalıştığından emin olun.';
            els.alert.classList.add('is-visible');
        } finally {
            submitBtn.disabled = false;
        }
    });

    /*  Düzenle / Sil  */
    let pendingDelete = null;
    const tbody = document.querySelector('.team-table tbody');
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('[data-edit]');
        const delBtn = e.target.closest('[data-delete]');
        if (editBtn) openEdit(editBtn.closest('tr'));
        if (delBtn) {
            const row = delBtn.closest('tr');
            pendingDelete = row.dataset.id;
            const isActive = row.dataset.active === 'true';
            const warn = isActive ? ' Takım şu anda La Liga\u2019da aktif; silinirse fikstür ve puan durumundaki kayıtları da etkilenir.' : '';
            els.delText.innerHTML = `<strong>${window.Admin.esc(row.dataset.name)}</strong> (${window.Admin.esc(row.dataset.code)}) kalıcı olarak silinecek.${warn} Bu işlem geri alınamaz.`;
            els.del.returnValue = '';
            els.del.showModal();
        }
    });
    els.del.addEventListener('close', async () => {
        if (els.del.returnValue === 'confirm' && pendingDelete !== null) {
            try {
                const res = await fetch(`/Admin/Teams/Delete/${pendingDelete}`, { method: 'POST' });
                const data = await res.json();
                if (data.ok) {
                    window.Admin.toast('Takım silindi.');
                    setTimeout(() => location.reload(), 700);
                } else {
                    window.Admin.toast(data.message || 'Silinemedi.', 'info');
                }
            } catch (err) {
                window.Admin.toast('API\u2019ye ulaşılamıyor.', 'info');
            }
        }
        pendingDelete = null;
    });
})();
