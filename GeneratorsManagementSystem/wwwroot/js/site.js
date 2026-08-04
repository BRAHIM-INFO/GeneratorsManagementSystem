/**
 * GMS - Generators Management System
 * site.js - النسخة المُصلحة
 */

// ════════════════════════════════════════════
// المتغيرات العامة
// ════════════════════════════════════════════
var GMS = {};
var GMS_HUB = null;

// ════════════════════════════════════════════
// 1. SWEETALERT HELPERS
// ════════════════════════════════════════════
GMS.toast = function (icon, title, timer) {
    if (typeof Swal === 'undefined') return;
    Swal.fire({
        toast: true,
        position: 'top-start',
        icon: icon,
        title: title,
        showConfirmButton: false,
        timer: timer || 3000,
        timerProgressBar: true,
        customClass: { popup: 'swal2-rtl' }
    });
};

GMS.confirm = function (opts) {
    if (typeof Swal === 'undefined')
        return Promise.resolve({ isConfirmed: false });

    return Swal.fire({
        title: opts.title || 'تأكيد',
        text: opts.text || 'هل أنت متأكد؟',
        icon: opts.icon || 'question',
        showCancelButton: true,
        confirmButtonText: opts.confirm || 'نعم',
        cancelButtonText: opts.cancel || 'إلغاء',
        confirmButtonColor: opts.color || '#5A67D8',
        cancelButtonColor: '#718096',
        reverseButtons: true,
        customClass: { popup: 'swal2-rtl' }
    });
};

GMS.deleteConfirm = function (cb) {
    GMS.confirm({
        title: 'تأكيد الحذف',
        text: 'لا يمكن التراجع عن هذا الإجراء!',
        icon: 'warning',
        confirm: 'نعم، احذف',
        color: '#E53E3E'
    }).then(function (r) {
        if (r.isConfirmed && typeof cb === 'function') cb();
    });
};

GMS.post = function (url, data) {
    var token = document.querySelector(
        'input[name="__RequestVerificationToken"]');
    var body = new URLSearchParams(data);
    if (token) body.append('__RequestVerificationToken', token.value);

    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: body.toString()
    }).then(function (r) { return r.json(); });
};

// ════════════════════════════════════════════
// 2. DATATABLES
// ════════════════════════════════════════════
var AR_LANG = {
    emptyTable: 'لا توجد بيانات',
    info: 'عرض _START_ إلى _END_ من _TOTAL_ مدخل',
    infoEmpty: 'عرض 0 إلى 0 من 0 مدخل',
    infoFiltered: '(من أصل _MAX_ مدخل)',
    lengthMenu: 'عرض _MENU_ مدخلات',
    loadingRecords: 'جار التحميل...',
    processing: 'جار المعالجة...',
    search: 'بحث:',
    zeroRecords: 'لا توجد نتائج مطابقة',
    paginate: {
        first: 'الأول',
        last: 'الأخير',
        next: 'التالي',
        previous: 'السابق'
    }
};

function initDataTables() {
    if (typeof $.fn === 'undefined') return;
    if (typeof $.fn.DataTable === 'undefined') return;

    $('.gms-table').each(function () {
        if (!$.fn.DataTable.isDataTable(this)) {
            $(this).DataTable({
                language: AR_LANG,
                responsive: true,
                pageLength: 15,
                lengthMenu: [10, 15, 25, 50, 100],
                columnDefs: [{ targets: -1, orderable: false }]
            });
        }
    });
}

// ════════════════════════════════════════════
// 3. SIDEBAR ACCORDION
// ════════════════════════════════════════════
function initSidebar() {
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');
    var mainContent = document.getElementById('mainContent');
    var topNavbar = document.getElementById('topNavbar');

    // ─── Accordion ───
    var parents = document.querySelectorAll('.nav-parent');
    parents.forEach(function (parent) {
        parent.addEventListener('click', function (e) {
            e.preventDefault();

            var targetId = this.dataset.target;
            var targetSub = document.getElementById(targetId);
            var isOpen = this.classList.contains('is-open');

            // أغلق الكل
            document.querySelectorAll('.nav-parent.is-open')
                .forEach(function (op) {
                    if (op !== parent) {
                        op.classList.remove('is-open');
                        var sid = op.dataset.target;
                        var sub = document.getElementById(sid);
                        if (sub) sub.classList.remove('is-open');
                    }
                });

            // افتح/أغلق الحالي
            if (!isOpen) {
                this.classList.add('is-open');
                if (targetSub) targetSub.classList.add('is-open');
            } else {
                this.classList.remove('is-open');
                if (targetSub) targetSub.classList.remove('is-open');
            }
        });
    });

    // ─── Desktop Toggle ───
    var desktopToggle = document.getElementById('sidebarToggle');
    if (desktopToggle && sidebar) {
        desktopToggle.addEventListener('click', function () {
            sidebar.classList.toggle('compact');
            var isCompact = sidebar.classList.contains('compact');
            if (mainContent)
                mainContent.style.marginRight = isCompact ? '70px' : '';
            if (topNavbar)
                topNavbar.style.right = isCompact ? '70px' : '';
        });
    }

    // ─── Mobile Toggle ───
    var mobileToggle = document.getElementById('mobileSidebarToggle');
    if (mobileToggle && sidebar) {
        mobileToggle.addEventListener('click', function () {
            sidebar.classList.toggle('is-open');
            if (overlay) overlay.classList.toggle('show');
        });
    }

    // ─── Overlay ───
    if (overlay) {
        overlay.addEventListener('click', function () {
            if (sidebar) sidebar.classList.remove('is-open');
            overlay.classList.remove('show');
        });
    }

    // ─── Active Links ───
    var currentPath = window.location.pathname;
    document.querySelectorAll('.nav-sub-link').forEach(function (link) {
        var href = link.getAttribute('href');
        if (href && href !== '#') {
            try {
                var linkPath = new URL(link.href,
                    window.location.origin).pathname;
                if (linkPath === currentPath) {
                    link.classList.add('is-active');
                }
            } catch (e) { }
        }
    });
}

// ════════════════════════════════════════════
// 4. SEARCH
// ════════════════════════════════════════════
function initSearch() {
    var btn = document.getElementById('searchBtn');
    var box = document.getElementById('searchDropdown');
    var input = document.getElementById('searchInput');

    if (!btn || !box) return;

    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        box.classList.toggle('open');
        if (box.classList.contains('open') && input) {
            setTimeout(function () { input.focus(); }, 300);
        }
    });

    document.addEventListener('click', function (e) {
        if (!box.contains(e.target) && e.target !== btn) {
            box.classList.remove('open');
        }
    });
}

// ════════════════════════════════════════════
// 5. FULLSCREEN
// ════════════════════════════════════════════
function initFullscreen() {
    var btn = document.getElementById('fullscreenBtn');
    var icon = document.getElementById('fsIcon');

    if (!btn) return;

    btn.addEventListener('click', function () {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen()
                .catch(function () { });
            if (icon) icon.className = 'fas fa-compress';
        } else {
            document.exitFullscreen().catch(function () { });
            if (icon) icon.className = 'fas fa-expand';
        }
    });

    document.addEventListener('fullscreenchange', function () {
        if (!document.fullscreenElement && icon) {
            icon.className = 'fas fa-expand';
        }
    });
}

// ════════════════════════════════════════════
// 6. COUNTER ANIMATION
// ════════════════════════════════════════════
function initCounters() {
    document.querySelectorAll('[data-count]').forEach(function (el) {
        var target = parseInt(el.dataset.count) || 0;
        var duration = 1200;
        var step = target / (duration / 16);
        var current = 0;

        var timer = setInterval(function () {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            el.textContent =
                Math.floor(current).toLocaleString('ar-SA');
        }, 16);
    });
}

// ════════════════════════════════════════════
// 7. TOOLTIPS
// ════════════════════════════════════════════
function initTooltips() {
    if (typeof bootstrap === 'undefined') return;
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(function (el) {
            new bootstrap.Tooltip(el);
        });
}

// ════════════════════════════════════════════
// 8. AUTO DISMISS ALERTS
// ════════════════════════════════════════════
function initAlerts() {
    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible')
            .forEach(function (a) {
                try {
                    if (typeof bootstrap !== 'undefined') {
                        bootstrap.Alert
                            .getOrCreateInstance(a).close();
                    }
                } catch (e) { }
            });
    }, 5000);
}

// ════════════════════════════════════════════
// 9. SIGNALR GLOBAL
// ════════════════════════════════════════════
function initGlobalSignalR() {
    if (typeof signalR === 'undefined') return;

    GMS_HUB = new signalR.HubConnectionBuilder()
        .withUrl('/generatorsHub')
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

    GMS_HUB.start()
        .then(function () {
            console.log('✅ SignalR متصل');
            GMS_HUB.invoke('JoinDashboard').catch(console.error);
        })
        .catch(function (err) {
            console.warn('⚠️ SignalR:', err.message);
        });

    GMS_HUB.onreconnected(function () {
        GMS_HUB.invoke('JoinDashboard').catch(console.error);
    });

    // تحذيرات المولدات
    GMS_HUB.on('GeneratorAlert', function (alert) {
        var icon = alert.level === 'danger' ? 'error' : 'warning';
        GMS.toast(icon, alert.message, 6000);

        var badge = document.getElementById('alertsBadge');
        if (badge) {
            var current = parseInt(badge.textContent) || 0;
            badge.textContent = current + 1;
        }
    });

    GMS_HUB.on('GeneratorStatusChanged', function (data) {
        GMS.toast('info',
            'تم تغيير حالة المولد #' + data.GeneratorId);
    });

    GMS_HUB.on('GeneratorAdded', function (data) {
        GMS.toast('success', 'تم إضافة المولد: ' + data.Name);
    });
}

// ════════════════════════════════════════════
// 10. INIT ALL - عند تحميل الصفحة
// ════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', function () {
    initSidebar();
    initSearch();
    initFullscreen();
    initCounters();
    initTooltips();
    initAlerts();
    initGlobalSignalR();

    // DataTables - بعد تأكد من jQuery
    if (typeof $ !== 'undefined') {
        initDataTables();
    }
});

///**
// * GMS - Main Site JavaScript
// */

///**
// * Sidebar Accordion Menu
// * يفتح قسم واحد فقط ويغلق الباقي
// */

///**
// * GMS - Generators Management System
// * Main JavaScript - No Theme Customizer
// */



//(function () {
//    'use strict';

//    /* ════════════════════════════════════════
//       SIDEBAR ACCORDION
//       ════════════════════════════════════════ */
//    function initSidebar() {
//        const sidebar = document.getElementById('sidebar');
//        const overlay = document.getElementById('sidebarOverlay');
//        const mainContent = document.getElementById('mainContent');
//        const topNavbar = document.getElementById('topNavbar');

//        /* ── Accordion Parents ── */
//        const parents = document.querySelectorAll('.nav-parent');

//        parents.forEach(function (parent) {
//            parent.addEventListener('click', function (e) {
//                e.preventDefault();

//                const targetId = this.dataset.target;
//                const targetSub = document.getElementById(targetId);
//                const isOpen = this.classList.contains('is-open');

//                /* Close ALL other open menus */
//                document.querySelectorAll('.nav-parent.is-open').forEach(function (openParent) {
//                    if (openParent !== parent) {
//                        openParent.classList.remove('is-open');
//                        const otherId = openParent.dataset.target;
//                        const otherSub = document.getElementById(otherId);
//                        if (otherSub) otherSub.classList.remove('is-open');
//                    }
//                });

//                /* Toggle current */
//                if (!isOpen) {
//                    this.classList.add('is-open');
//                    if (targetSub) targetSub.classList.add('is-open');
//                } else {
//                    this.classList.remove('is-open');
//                    if (targetSub) targetSub.classList.remove('is-open');
//                }
//            });
//        });

//        /* ── Desktop Toggle (Compact) ── */
//        const desktopToggle = document.getElementById('sidebarToggle');
//        if (desktopToggle && sidebar) {
//            desktopToggle.addEventListener('click', function () {
//                sidebar.classList.toggle('compact');
//                if (mainContent) {
//                    mainContent.style.marginRight =
//                        sidebar.classList.contains('compact') ? '70px' : '';
//                }
//                if (topNavbar) {
//                    topNavbar.style.right =
//                        sidebar.classList.contains('compact') ? '70px' : '';
//                }
//            });
//        }

//        /* ── Mobile Toggle ── */
//        const mobileToggle = document.getElementById('mobileSidebarToggle');
//        if (mobileToggle && sidebar) {
//            mobileToggle.addEventListener('click', function () {
//                sidebar.classList.toggle('is-open');
//                if (overlay) overlay.classList.toggle('show');
//            });
//        }

//        /* ── Overlay Click ── */
//        if (overlay) {
//            overlay.addEventListener('click', function () {
//                if (sidebar) sidebar.classList.remove('is-open');
//                overlay.classList.remove('show');
//            });
//        }

//        /* ── Mark Active Sub-links ── */
//        const currentPath = window.location.pathname;
//        document.querySelectorAll('.nav-sub-link').forEach(function (link) {
//            if (link.href && link.getAttribute('href') !== '#') {
//                try {
//                    if (new URL(link.href).pathname === currentPath) {
//                        link.classList.add('is-active');
//                        /* Open parent */
//                        const parentSub = link.closest('.nav-sub');
//                        if (parentSub) {
//                            parentSub.classList.add('is-open');
//                            const parentLink = document.querySelector(
//                                '[data-target="' + parentSub.id + '"]'
//                            );
//                            if (parentLink) parentLink.classList.add('is-open');
//                        }
//                    }
//                } catch (e) { }
//            }
//        });
//    }

//    /* ════════════════════════════════════════
//       SEARCH
//       ════════════════════════════════════════ */
//    function initSearch() {
//        const btn = document.getElementById('searchBtn');
//        const box = document.getElementById('searchDropdown');
//        const input = document.getElementById('searchInput');

//        if (!btn || !box) return;

//        btn.addEventListener('click', function (e) {
//            e.stopPropagation();
//            box.classList.toggle('open');
//            if (box.classList.contains('open') && input) {
//                setTimeout(() => input.focus(), 300);
//            }
//        });

//        document.addEventListener('click', function (e) {
//            if (!box.contains(e.target) && e.target !== btn) {
//                box.classList.remove('open');
//            }
//        });
//    }

//    /* ════════════════════════════════════════
//       FULLSCREEN
//       ════════════════════════════════════════ */
//    function initFullscreen() {
//        const btn = document.getElementById('fullscreenBtn');
//        const icon = document.getElementById('fsIcon');

//        if (!btn) return;

//        btn.addEventListener('click', function () {
//            if (!document.fullscreenElement) {
//                document.documentElement.requestFullscreen().catch(() => { });
//                if (icon) { icon.className = 'fas fa-compress'; }
//            } else {
//                document.exitFullscreen().catch(() => { });
//                if (icon) { icon.className = 'fas fa-expand'; }
//            }
//        });
//    }

//    /* ════════════════════════════════════════
//       DATATABLES
//       ════════════════════════════════════════ */
//    const AR_LANG = {
//        emptyTable: "لا توجد بيانات",
//        info: "عرض _START_ إلى _END_ من _TOTAL_ مدخل",
//        infoEmpty: "عرض 0 إلى 0 من 0 مدخل",
//        infoFiltered: "(من أصل _MAX_ مدخل)",
//        lengthMenu: "عرض _MENU_ مدخلات",
//        loadingRecords: "جار التحميل...",
//        processing: "جار المعالجة...",
//        search: "بحث:",
//        zeroRecords: "لا توجد نتائج",
//        paginate: {
//            first: "الأول", last: "الأخير",
//            next: "التالي", previous: "السابق"
//        }
//    };

//    function initDataTables() {
//        if (typeof $.fn.DataTable === 'undefined') return;

//        $('.gms-table').each(function () {
//            if (!$.fn.DataTable.isDataTable(this)) {
//                $(this).DataTable({
//                    language: AR_LANG,
//                    responsive: true,
//                    pageLength: 10,
//                    lengthMenu: [10, 25, 50, 100],
//                    columnDefs: [{ targets: -1, orderable: false }]
//                });
//            }
//        });
//    }

//    /* ════════════════════════════════════════
//       COUNTER ANIMATION
//       ════════════════════════════════════════ */
//    function initCounters() {
//        document.querySelectorAll('[data-count]').forEach(function (el) {
//            const target = parseInt(el.dataset.count) || 0;
//            const duration = 1200;
//            const step = target / (duration / 16);
//            let current = 0;

//            const timer = setInterval(function () {
//                current += step;
//                if (current >= target) {
//                    current = target;
//                    clearInterval(timer);
//                }
//                el.textContent = Math.floor(current).toLocaleString('ar-SA');
//            }, 16);
//        });
//    }

//    /* ════════════════════════════════════════
//       SWEETALERT HELPERS
//       ════════════════════════════════════════ */
//    window.GMS = {
//        toast: function (icon, title, timer) {
//            Swal.fire({
//                toast: true,
//                position: 'top-start',
//                icon: icon,
//                title: title,
//                showConfirmButton: false,
//                timer: timer || 3000,
//                timerProgressBar: true,
//                customClass: { popup: 'swal2-rtl' }
//            });
//        },

//        confirm: function (opts) {
//            return Swal.fire({
//                title: opts.title || 'تأكيد',
//                text: opts.text || 'هل أنت متأكد؟',
//                icon: opts.icon || 'question',
//                showCancelButton: true,
//                confirmButtonText: opts.confirm || 'نعم',
//                cancelButtonText: opts.cancel || 'إلغاء',
//                confirmButtonColor: opts.color || '#5A67D8',
//                cancelButtonColor: '#718096',
//                reverseButtons: true,
//                customClass: { popup: 'swal2-rtl' }
//            });
//        },

//        deleteConfirm: function (cb) {
//            GMS.confirm({
//                title: 'تأكيد الحذف',
//                text: 'لا يمكن التراجع عن هذا الإجراء!',
//                icon: 'warning',
//                confirm: 'نعم، احذف',
//                color: '#E53E3E'
//            }).then(function (r) {
//                if (r.isConfirmed && typeof cb === 'function') cb();
//            });
//        }
//    };

//    /* ════════════════════════════════════════
//       TOOLTIPS
//       ════════════════════════════════════════ */
//    function initTooltips() {
//        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
//            new bootstrap.Tooltip(el);
//        });
//    }

//    /* ════════════════════════════════════════
//       AUTO DISMISS ALERTS
//       ════════════════════════════════════════ */
//    function initAlerts() {
//        setTimeout(function () {
//            document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
//                try { bootstrap.Alert.getOrCreateInstance(alert).close(); } catch (e) { }
//            });
//        }, 5000);
//    }

//    /* ════════════════════════════════════════
//       INIT ALL
//       ════════════════════════════════════════ */
//    document.addEventListener('DOMContentLoaded', function () {
//        initSidebar();
//        initSearch();
//        initFullscreen();
//        initDataTables();
//        initCounters();
//        initTooltips();
//        initAlerts();
//    });

//})();

//function initSidebarAccordion() {
//    const toggleLinks = document.querySelectorAll('.submenu-toggle');

//    toggleLinks.forEach(function (toggle) {
//        toggle.addEventListener('click', function (e) {
//            e.preventDefault();

//            const targetId = this.dataset.target;
//            const targetMenu = document.getElementById(targetId);
//            const isOpen = this.classList.contains('open');

//            // ══════════════════════════════════
//            // أغلق كل الأقسام المفتوحة أولاً
//            // ══════════════════════════════════
//            document.querySelectorAll('.submenu-toggle.open').forEach(function (openToggle) {
//                if (openToggle !== toggle) {
//                    openToggle.classList.remove('open');
//                    const openMenuId = openToggle.dataset.target;
//                    const openMenu = document.getElementById(openMenuId);
//                    if (openMenu) {
//                        openMenu.classList.remove('show');
//                    }
//                    // إزالة active من العنصر الأب
//                    const parentItem = openToggle.closest('.nav-item');
//                    if (parentItem) {
//                        parentItem.classList.remove('active');
//                    }
//                }
//            });

//            // ══════════════════════════════════
//            // افتح أو أغلق القسم المحدد
//            // ══════════════════════════════════
//            if (!isOpen) {
//                // فتح
//                this.classList.add('open');
//                if (targetMenu) {
//                    targetMenu.classList.add('show');
//                }
//                const parentItem = this.closest('.nav-item');
//                if (parentItem) {
//                    parentItem.classList.add('active');
//                }
//            } else {
//                // إغلاق
//                this.classList.remove('open');
//                if (targetMenu) {
//                    targetMenu.classList.remove('show');
//                }
//                const parentItem = this.closest('.nav-item');
//                if (parentItem) {
//                    parentItem.classList.remove('active');
//                }
//            }
//        });
//    });
//}

//// ══════════════════════════════════
//// تشغيل عند تحميل الصفحة
//// ══════════════════════════════════
//document.addEventListener('DOMContentLoaded', function () {
//    initSidebarAccordion();

//    // باقي الـ functions
//    initDataTables();
//    initTooltips();
//    animateCounters();

//    // Auto dismiss alerts
//    setTimeout(() => {
//        document.querySelectorAll('.alert-dismissible').forEach(alert => {
//            try { new bootstrap.Alert(alert).close(); } catch (e) { }
//        });
//    }, 5000);
//});

//(function () {
//    'use strict';

//    // =============================================
//    // DATATABLES ARABIC
//    // =============================================
//    const dataTableArabicLang = {
//        decimal: "",
//        emptyTable: "لا توجد بيانات متاحة في الجدول",
//        info: "عرض _START_ إلى _END_ من _TOTAL_ مدخل",
//        infoEmpty: "عرض 0 إلى 0 من 0 مدخل",
//        infoFiltered: "(تصفية من _MAX_ مجموع مدخلات)",
//        infoPostFix: "",
//        thousands: ",",
//        lengthMenu: "عرض _MENU_ مدخلات",
//        loadingRecords: "جار التحميل...",
//        processing: "جار المعالجة...",
//        search: "بحث:",
//        zeroRecords: "لم يتم العثور على سجلات مطابقة",
//        paginate: {
//            first: "الأول",
//            last: "الأخير",
//            next: "التالي",
//            previous: "السابق"
//        },
//        aria: {
//            sortAscending: ": تفعيل لترتيب العمود تصاعدياً",
//            sortDescending: ": تفعيل لترتيب العمود تنازلياً"
//        }
//    };

//    // =============================================
//    // INIT DATATABLES
//    // =============================================
//    function initDataTables() {
//        if (typeof $.fn.DataTable !== 'undefined') {
//            $('.data-table').each(function () {
//                if (!$.fn.DataTable.isDataTable(this)) {
//                    $(this).DataTable({
//                        language: dataTableArabicLang,
//                        responsive: true,
//                        pageLength: 10,
//                        lengthMenu: [10, 25, 50, 100],
//                        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
//                            '<"row"<"col-sm-12"tr>>' +
//                            '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
//                        columnDefs: [{
//                            targets: -1,
//                            orderable: false
//                        }]
//                    });
//                }
//            });
//        }
//    }

//    // =============================================
//    // SWEETALERT HELPERS
//    // =============================================
//    window.GMS = {
//        toast: function (icon, title, timer = 3000) {
//            Swal.fire({
//                toast: true,
//                position: 'top-start',
//                icon: icon,
//                title: title,
//                showConfirmButton: false,
//                timer: timer,
//                timerProgressBar: true,
//                customClass: {
//                    popup: 'swal-toast-rtl'
//                }
//            });
//        },

//        confirm: function (options) {
//            return Swal.fire({
//                title: options.title || 'تأكيد',
//                text: options.text || 'هل أنت متأكد؟',
//                icon: options.icon || 'question',
//                showCancelButton: true,
//                confirmButtonText: options.confirmText || 'نعم',
//                cancelButtonText: options.cancelText || 'إلغاء',
//                confirmButtonColor: options.confirmColor || '#7367F0',
//                cancelButtonColor: '#82868B',
//                reverseButtons: true,
//                customClass: {
//                    popup: 'swal-rtl'
//                }
//            });
//        },

//        deleteConfirm: function (callback) {
//            GMS.confirm({
//                title: 'حذف العنصر',
//                text: 'هل تريد حذف هذا العنصر؟ لا يمكن التراجع عن هذا الإجراء!',
//                icon: 'warning',
//                confirmText: 'نعم، احذف',
//                confirmColor: '#EA5455'
//            }).then(result => {
//                if (result.isConfirmed && typeof callback === 'function') {
//                    callback();
//                }
//            });
//        }
//    };

//    // =============================================
//    // CARD LOADING ANIMATION
//    // =============================================
//    function animateCounters() {
//        document.querySelectorAll('[data-count]').forEach(el => {
//            const target = parseInt(el.dataset.count);
//            const duration = 1500;
//            const step = target / (duration / 16);
//            let current = 0;

//            const timer = setInterval(() => {
//                current += step;
//                if (current >= target) {
//                    current = target;
//                    clearInterval(timer);
//                }
//                el.textContent = Math.floor(current).toLocaleString('ar-SA');
//            }, 16);
//        });
//    }

//    // =============================================
//    // TOOLTIPS
//    // =============================================
//    function initTooltips() {
//        const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
//        tooltips.forEach(el => new bootstrap.Tooltip(el));
//    }

//    // =============================================
//    // ACTIVE NAV LINKS
//    // =============================================
//    function setActiveNavLinks() {
//        const currentPath = window.location.pathname;
//        document.querySelectorAll('.nav-link').forEach(link => {
//            if (link.href && link.href !== '#' &&
//                new URL(link.href, window.location.origin).pathname === currentPath) {
//                link.classList.add('active');
//                const parentItem = link.closest('.nav-item');
//                if (parentItem) {
//                    parentItem.classList.add('active');
//                    const submenu = link.closest('.submenu');
//                    if (submenu) {
//                        submenu.classList.add('show');
//                        parentItem.classList.add('open');
//                    }
//                }
//            }
//        });
//    }

//    // =============================================
//    // INITIALIZE
//    // =============================================
//    document.addEventListener('DOMContentLoaded', function () {
//        initDataTables();
//        initTooltips();
//        setActiveNavLinks();
//        animateCounters();

//        // Auto dismiss alerts
//        setTimeout(() => {
//            document.querySelectorAll('.alert-dismissible').forEach(alert => {
//                new bootstrap.Alert(alert).close();
//            });
//        }, 5000);
//    });

//})();