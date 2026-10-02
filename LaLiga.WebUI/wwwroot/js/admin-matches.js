/* 
   La Liga Admin - admin-matches.js
   Maç Yönetimi: ekle/düzenle/sil modalları 
 */
(function () {
    'use strict';

    const form = document.querySelector('[data-match-form]');
    if (!form) return; // yalnız Maçlar sayfasında çalışır

    const $ = (sel, root = document) => root.querySelector(sel);
    const f = form.elements;

    const els = {
        modal: $('#match-modal'),
        title: $('#match-modal-title'),
        alert: $('[data-form-alert]'),
        scoreFields: $('[data-score-fields]'),
        del: $('#delete-modal'),
        delText: $('[data-delete-text]')
    };

    /* ---------- Stadyum otomatik doldurma ---------- */
    let stadiumTouched = false;
    f.stadium.addEventListener('input', () => { stadiumTouched = f.stadium.value.trim() !== ''; });
    f.home.addEventListener('change', () => {
        const opt = f.home.selectedOptions[0];
        if (!stadiumTouched && opt && opt.dataset.stadium) f.stadium.value = opt.dataset.stadium;
        clearError('home');
    });

    /*  Skor alanları: yalnız Canlı/Tamamlandı'da görünür  */
    f.status.addEventListener('change', toggleScoreFields);
    function toggleScoreFields() {
        const show = f.status.value === 'Live' || f.status.value === 'Finished';
        els.scoreFields.hidden = !show;
        if (!show) { clearError('homeScore'); clearError('awayScore'); }
    }
    ['week', 'away', 'date', 'time', 'homeScore', 'awayScore'].forEach((n) =>
        f[n].addEventListener('input', () => clearError(n)));

    /*  Alan hataları  */
    function setError(name, msg) {
        const field = form.querySelector(`[data-field="${name}"]`);
        if (!field) return;
        field.classList.add('has-error');
        const err = field.querySelector('.field-error');
        if (err) err.textContent = msg;
        if (f[name]) f[name].setAttribute('aria-invalid', 'true');
    }
    function clearError(name) {
        const field = form.querySelector(`[data-field="${name}"]`);
        if (!field) return;
        field.classList.remove('has-error');
        if (f[name]) f[name].removeAttribute('aria-invalid');
    }
    function clearAllErrors() {
        form.querySelectorAll('.has-error').forEach((el) => el.classList.remove('has-error'));
        form.querySelectorAll('[aria-invalid]').forEach((el) => el.removeAttribute('aria-invalid'));
        els.alert.classList.remove('is-visible');
    }

    /*  İstemci tarafı doğrulama (ilk göz; sunucu her zaman son karar)  */
    function validate() {
        const errors = [];
        if (!f.week.value) errors.push(['week', 'Lütfen bir hafta seçin.']);
        if (!f.home.value) errors.push(['home', 'Lütfen ev sahibi takımı seçin.']);
        if (!f.away.value) errors.push(['away', 'Lütfen deplasman takımını seçin.']);
        else if (f.home.value && f.home.value === f.away.value) errors.push(['away', 'Ev sahibi ve deplasman takımı aynı olamaz.']);
        if (!f.date.value) errors.push(['date', 'Lütfen maç tarihini seçin.']);
        if (!f.time.value) errors.push(['time', 'Lütfen maç saatini seçin.']);
        if (f.status.value === 'Live' || f.status.value === 'Finished') {
            ['homeScore', 'awayScore'].forEach((k) => {
                const v = f[k].value;
                if (v === '' || !Number.isInteger(Number(v)) || Number(v) < 0)
                    errors.push([k, 'Geçerli bir skor girin (0 veya üzeri).']);
            });
        }
        return errors;
    }

    /*  Modal aç / kapat  */
    let lastFocus = null;
    function openCreate() {
        lastFocus = document.activeElement;
        form.reset();
        clearAllErrors();
        stadiumTouched = false;
        f.id.value = '';
        f.status.value = 'NotStarted';
        f.minute.value = '';
        f.note.value = '';
        f.referee.value = '';
        f.attendance.value = '';
        els.title.textContent = 'Yeni Maç Ekle';
        toggleScoreFields();
        els.modal.showModal();
        f.week.focus();
    }
    function openEdit(row) {
        lastFocus = document.activeElement;
        form.reset();
        clearAllErrors();
        const d = row.dataset;
        f.id.value = d.id;
        f.week.value = d.week;
        f.home.value = d.homeId;
        f.away.value = d.awayId;
        f.date.value = d.date;
        f.time.value = d.time;
        f.stadium.value = d.stadium;
        f.status.value = d.status;
        f.homeScore.value = d.homeScore || '';
        f.awayScore.value = d.awayScore || '';
        f.minute.value = d.minute || '';
        f.note.value = d.note || '';
        f.referee.value = d.referee || '';
        f.attendance.value = d.attendance || '';
        stadiumTouched = true;
        els.title.textContent = 'Maçı Düzenle';
        toggleScoreFields();
        els.modal.showModal();
        f.week.focus();
    }
    function closeModal() { els.modal.close(); }
    els.modal.addEventListener('close', () => { if (lastFocus) lastFocus.focus(); });
    els.modal.addEventListener('click', (e) => { if (e.target === els.modal) closeModal(); });
    document.querySelectorAll('[data-modal-close]').forEach((b) => b.addEventListener('click', closeModal));
    const openBtn = $('[data-open-create]');
    if (openBtn) openBtn.addEventListener('click', openCreate);

    /*  Alan adı eşlemesi: API'nin döndürdüğü alan adları formdakiyle küçük farklarla gelebilir  */
    function mapErrorField(apiField) {
        const key = apiField.toLowerCase();
        const map = { hometeamid: 'home', awayteamid: 'away' };
        return map[key] || key;
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
            f[errors[0][0]].focus();
            return;
        }

        const submitBtn = form.querySelector('[data-submit-label]');
        submitBtn.disabled = true;

        try {
            const res = await fetch('/Admin/Matches/Save', { method: 'POST', body: new FormData(form) });
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
            window.Admin.toast(wasEdit ? 'Maç başarıyla güncellendi.' : 'Maç başarıyla eklendi.');
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
    const tbody = document.querySelector('.match-table tbody');
    tbody.addEventListener('click', (e) => {
        const editBtn = e.target.closest('[data-edit]');
        const delBtn = e.target.closest('[data-delete]');
        if (editBtn) openEdit(editBtn.closest('tr'));
        if (delBtn) {
            const row = delBtn.closest('tr');
            pendingDelete = row.dataset.id;
            els.delText.innerHTML = `<strong>${window.Admin.esc(row.dataset.homeName)} - ${window.Admin.esc(row.dataset.awayName)}</strong> (ID ${row.dataset.id}, ${row.dataset.week}. Hafta) kalıcı olarak silinecek. Bu işlem geri alınamaz.`;
            els.del.returnValue = '';
            els.del.showModal();
        }
    });
    els.del.addEventListener('close', async () => {
        if (els.del.returnValue === 'confirm' && pendingDelete !== null) {
            try {
                const res = await fetch(`/Admin/Matches/Delete/${pendingDelete}`, { method: 'POST' });
                const data = await res.json();
                if (data.ok) {
                    window.Admin.toast('Maç silindi.');
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
