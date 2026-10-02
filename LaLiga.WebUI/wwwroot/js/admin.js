/* 
   La Liga Admin 
   Sidebar aç/kapa ve özellikler
   
*/
(function () {
    'use strict';

    const $ = (sel, root = document) => root.querySelector(sel);
    const esc = (s) => String(s).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const icon = (id) => `<svg class="icon" aria-hidden="true"><use href="#i-${id}"/></svg>`;

    const toastStack = $('[data-toasts]');

    // type: 'success' | 'info'
    function toast(message, type = 'success') {
        if (!toastStack) return;
        const el = document.createElement('div');
        el.className = `toast-a toast-a--${type}`;
        el.setAttribute('role', 'status');
        el.innerHTML = `${icon(type === 'success' ? 'check' : 'info')}<span>${esc(message)}</span>`;
        toastStack.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }

    /* ---------- Sidebar (mobil) ---------- */
    const toggleBtn = $('[data-sidebar-toggle]');
    function setSidebar(open) {
        document.body.classList.toggle('sidebar-open', open);
        if (toggleBtn) toggleBtn.setAttribute('aria-expanded', String(open));
    }
    if (toggleBtn) toggleBtn.addEventListener('click', () => setSidebar(!document.body.classList.contains('sidebar-open')));
    const closeBtn = $('[data-sidebar-close]');
    if (closeBtn) closeBtn.addEventListener('click', () => setSidebar(false));
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') setSidebar(false); });

    /* ---------- Bildirim zili ----------
       Gerçek bir bildirim sistemi yok; tıklanınca bunu açıkça söyler. */
    const notifBtn = $('[data-notif-btn]');
    if (notifBtn) notifBtn.addEventListener('click', () => toast('Bildirim sistemi bu sürümde yok.', 'info'));

    /* ---------- Diğer admin dosyalarının (16d, 17) kullanacağı paylaşılan yardımcılar ---------- */
    window.Admin = { toast, esc, icon };
})();
