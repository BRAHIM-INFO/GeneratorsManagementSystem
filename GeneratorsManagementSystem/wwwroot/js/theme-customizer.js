/**
 * GMS Theme Customizer
 * نظام إدارة المولدات الكهربائية
 */
(function () {
    'use strict';

    // =============================================
    // DEFAULTS
    // =============================================
    const DEFAULTS = {
        layoutMode: 'light',
        navbarType: 'fixed',
        sidebarColor: 'gradient-purple',
        sidebarBgImage: null,
        sidebarBgImageEnabled: false,
        compactMenu: false,
        sidebarWidth: 'medium',
        backgroundColor: '#F8F8F8'
    };

    const STORAGE_KEY = 'gms_theme_settings';

    // =============================================
    // LOAD SAVED SETTINGS
    // =============================================
    function loadSettings() {
        try {
            const saved = localStorage.getItem(STORAGE_KEY);
            return saved ? { ...DEFAULTS, ...JSON.parse(saved) } : { ...DEFAULTS };
        } catch {
            return { ...DEFAULTS };
        }
    }

    function saveSettings(settings) {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
        } catch { }
    }

    // =============================================
    // APPLY SETTINGS
    // =============================================
    function applySettings(settings) {
        const html = document.documentElement;
        const sidebar = document.getElementById('sidebar');
        const body = document.body;

        // Layout Mode
        html.setAttribute('data-layout', settings.layoutMode);

        // Sidebar Color
        html.setAttribute('data-sidebar-color', settings.sidebarColor);

        // Sidebar Width
        html.setAttribute('data-sidebar-width', settings.sidebarWidth);

        // Background Color
        body.style.backgroundColor = settings.backgroundColor;

        // Sidebar BG Image
        if (sidebar) {
            if (settings.sidebarBgImageEnabled && settings.sidebarBgImage) {
                sidebar.classList.add('has-bg-image');
                sidebar.style.setProperty(
                    '--sidebar-bg-image',
                    `url('/images/sidebar-bg/${settings.sidebarBgImage}')`
                );
                sidebar.style.setProperty('--sidebar-overlay-opacity', '0.15');
                updateSidebarBgStyle(sidebar, settings.sidebarBgImage);
            } else {
                sidebar.classList.remove('has-bg-image');
                removeSidebarBgStyle(sidebar);
            }
        }

        // Compact Menu
        if (sidebar) {
            if (settings.compactMenu) {
                sidebar.classList.add('compact');
            } else {
                sidebar.classList.remove('compact');
            }
        }

        // Navbar Type
        const navbar = document.getElementById('topNavbar');
        if (navbar) {
            if (settings.navbarType === 'fixed') {
                navbar.classList.add('fixed-top-navbar');
            } else {
                navbar.classList.remove('fixed-top-navbar');
            }
        }
    }

    function updateSidebarBgStyle(sidebar, image) {
        let styleEl = document.getElementById('sidebar-bg-style');
        if (!styleEl) {
            styleEl = document.createElement('style');
            styleEl.id = 'sidebar-bg-style';
            document.head.appendChild(styleEl);
        }
        styleEl.textContent = `
            .sidebar.has-bg-image::before {
                background-image: url('/images/sidebar-bg/${image}');
                content: '';
                position: absolute;
                top: 0; left: 0; right: 0; bottom: 0;
                background-size: cover;
                background-position: center;
                opacity: 0.15;
                z-index: 0;
            }
        `;
    }

    function removeSidebarBgStyle(sidebar) {
        const styleEl = document.getElementById('sidebar-bg-style');
        if (styleEl) styleEl.remove();
    }

    // =============================================
    // SYNC UI CONTROLS
    // =============================================
    function syncControls(settings) {
        // Layout radio buttons
        const layoutRadios = document.querySelectorAll('input[name="layoutMode"]');
        layoutRadios.forEach(radio => {
            radio.checked = radio.value === settings.layoutMode;
        });

        // Navbar radio buttons
        const navbarRadios = document.querySelectorAll('input[name="navbarType"]');
        navbarRadios.forEach(radio => {
            radio.checked = radio.value === settings.navbarType;
        });

        // Sidebar color swatches
        const colorSwatches = document.querySelectorAll('#sidebarColorOptions .color-swatch');
        colorSwatches.forEach(swatch => {
            swatch.classList.toggle('active', swatch.dataset.color === settings.sidebarColor);
        });

        // BG image swatches
        const bgImages = document.querySelectorAll('.bg-image-swatch');
        bgImages.forEach(img => {
            img.classList.toggle('active', img.dataset.image === settings.sidebarBgImage);
        });

        // BG Image toggle
        const bgToggle = document.getElementById('sidebarBgImageToggle');
        if (bgToggle) bgToggle.checked = settings.sidebarBgImageEnabled;

        // Compact menu toggle
        const compactToggle = document.getElementById('compactMenuToggle');
        if (compactToggle) compactToggle.checked = settings.compactMenu;

        // Width buttons
        const widthBtns = document.querySelectorAll('.width-btn');
        widthBtns.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.width === settings.sidebarWidth);
        });

        // BG color swatches
        const bgColorSwatches = document.querySelectorAll('#bgColorOptions .color-swatch');
        bgColorSwatches.forEach(swatch => {
            swatch.classList.toggle('active', swatch.dataset.bg === settings.backgroundColor);
        });
    }

    // =============================================
    // INIT
    // =============================================
    function init() {
        const settings = loadSettings();

        // Apply on page load
        applySettings(settings);
        syncControls(settings);

        // ---- Toggle Customizer ----
        const customizerBtn = document.getElementById('themeCustomizerBtn');
        const customizer = document.getElementById('themeCustomizer');
        const closeBtn = document.getElementById('closeCustomizer');

        if (customizerBtn && customizer) {
            customizerBtn.addEventListener('click', () => {
                customizer.classList.toggle('open');
            });
        }

        if (closeBtn && customizer) {
            closeBtn.addEventListener('click', () => {
                customizer.classList.remove('open');
            });
        }

        // ---- Layout Mode ----
        document.querySelectorAll('input[name="layoutMode"]').forEach(radio => {
            radio.addEventListener('change', function () {
                settings.layoutMode = this.value;
                applySettings(settings);
                saveSettings(settings);
                saveToServer(settings);
            });
        });

        // ---- Navbar Type ----
        document.querySelectorAll('input[name="navbarType"]').forEach(radio => {
            radio.addEventListener('change', function () {
                settings.navbarType = this.value;
                applySettings(settings);
                saveSettings(settings);
            });
        });

        // ---- Sidebar Color ----
        document.querySelectorAll('#sidebarColorOptions .color-swatch').forEach(swatch => {
            swatch.addEventListener('click', function () {
                settings.sidebarColor = this.dataset.color;

                // Update active state
                document.querySelectorAll('#sidebarColorOptions .color-swatch')
                    .forEach(s => s.classList.remove('active'));
                this.classList.add('active');

                applySettings(settings);
                saveSettings(settings);
                saveToServer(settings);
            });
        });

        // ---- Sidebar BG Image ----
        document.querySelectorAll('.bg-image-swatch').forEach(swatch => {
            swatch.addEventListener('click', function () {
                settings.sidebarBgImage = this.dataset.image;

                document.querySelectorAll('.bg-image-swatch')
                    .forEach(s => s.classList.remove('active'));
                this.classList.add('active');

                applySettings(settings);
                saveSettings(settings);
            });
        });

        // ---- BG Image Toggle ----
        const bgToggle = document.getElementById('sidebarBgImageToggle');
        if (bgToggle) {
            bgToggle.addEventListener('change', function () {
                settings.sidebarBgImageEnabled = this.checked;
                applySettings(settings);
                saveSettings(settings);
            });
        }

        // ---- Compact Menu Toggle ----
        const compactToggle = document.getElementById('compactMenuToggle');
        if (compactToggle) {
            compactToggle.addEventListener('change', function () {
                settings.compactMenu = this.checked;
                applySettings(settings);
                saveSettings(settings);

                // Update main content margin
                const mainContent = document.getElementById('mainContent');
                if (mainContent) {
                    mainContent.style.marginRight = this.checked ? '72px' : '';
                }
            });
        }

        // ---- Sidebar Width ----
        document.querySelectorAll('.width-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                settings.sidebarWidth = this.dataset.width;

                document.querySelectorAll('.width-btn')
                    .forEach(b => b.classList.remove('active'));
                this.classList.add('active');

                applySettings(settings);
                saveSettings(settings);
            });
        });

        // ---- Background Color ----
        document.querySelectorAll('#bgColorOptions .color-swatch').forEach(swatch => {
            swatch.addEventListener('click', function () {
                settings.backgroundColor = this.dataset.bg;

                document.querySelectorAll('#bgColorOptions .color-swatch')
                    .forEach(s => s.classList.remove('active'));
                this.classList.add('active');

                applySettings(settings);
                saveSettings(settings);

                // Auto set dark layout if dark color selected
                const darkColors = ['#1A1A2E', '#10163A', '#161D31'];
                if (darkColors.includes(settings.backgroundColor) &&
                    settings.layoutMode !== 'dark') {
                    settings.layoutMode = 'dark';
                    document.querySelector('input[name="layoutMode"][value="dark"]').checked = true;
                    document.documentElement.setAttribute('data-layout', 'dark');
                    saveSettings(settings);
                }
            });
        });

        // ---- Reset Theme ----
        const resetBtn = document.getElementById('resetTheme');
        if (resetBtn) {
            resetBtn.addEventListener('click', function () {
                Swal.fire({
                    title: 'إعادة تعيين المظهر',
                    text: 'هل تريد إعادة تعيين جميع إعدادات المظهر؟',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: 'نعم، إعادة التعيين',
                    cancelButtonText: 'إلغاء',
                    confirmButtonColor: '#7367F0',
                    cancelButtonColor: '#82868B',
                    customClass: {
                        popup: 'swal-rtl',
                        title: 'swal-title-ar'
                    }
                }).then((result) => {
                    if (result.isConfirmed) {
                        localStorage.removeItem(STORAGE_KEY);
                        const defaultSettings = { ...DEFAULTS };
                        applySettings(defaultSettings);
                        syncControls(defaultSettings);
                        saveToServer(defaultSettings);

                        Swal.fire({
                            title: 'تم!',
                            text: 'تمت إعادة تعيين المظهر بنجاح',
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false,
                            confirmButtonColor: '#7367F0'
                        });
                    }
                });
            });
        }

        // ---- Mobile Sidebar Toggle ----
        const mobileSidebarToggle = document.getElementById('mobileSidebarToggle');
        const sidebarEl = document.getElementById('sidebar');
        const overlay = document.getElementById('sidebarOverlay');

        if (mobileSidebarToggle && sidebarEl) {
            mobileSidebarToggle.addEventListener('click', () => {
                sidebarEl.classList.toggle('show');
                overlay.classList.toggle('show');
            });
        }

        if (overlay) {
            overlay.addEventListener('click', () => {
                sidebarEl.classList.remove('show');
                overlay.classList.remove('show');
            });
        }

        // ---- Desktop Sidebar Toggle ----
        const desktopToggle = document.getElementById('sidebarToggle');
        if (desktopToggle) {
            desktopToggle.addEventListener('click', () => {
                settings.compactMenu = !settings.compactMenu;
                if (compactToggle) compactToggle.checked = settings.compactMenu;
                applySettings(settings);
                saveSettings(settings);

                // Update main content
                const mainContent = document.getElementById('mainContent');
                if (mainContent) {
                    mainContent.style.marginRight = settings.compactMenu ? '72px' : '';
                }
            });
        }

        // ---- Fullscreen ----
        const fullscreenBtn = document.getElementById('fullscreenBtn');
        const fullscreenIcon = document.getElementById('fullscreenIcon');
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', () => {
                if (!document.fullscreenElement) {
                    document.documentElement.requestFullscreen();
                    fullscreenIcon.className = 'fas fa-compress';
                } else {
                    document.exitFullscreen();
                    fullscreenIcon.className = 'fas fa-expand';
                }
            });
        }

        // ---- Search Toggle ----
        const searchToggle = document.getElementById('searchToggle');
        const searchBox = document.getElementById('searchBox');
        if (searchToggle && searchBox) {
            searchToggle.addEventListener('click', () => {
                searchBox.classList.toggle('show');
                if (searchBox.classList.contains('show')) {
                    searchBox.querySelector('input').focus();
                }
            });

            document.addEventListener('click', (e) => {
                if (!searchBox.contains(e.target) && e.target !== searchToggle) {
                    searchBox.classList.remove('show');
                }
            });
        }
    }

    // =============================================
    // SAVE TO SERVER
    // =============================================
    async function saveToServer(settings) {
        try {
            await fetch('/api/theme/save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(settings)
            });
        } catch { /* Silent fail */ }
    }

    function getAntiForgeryToken() {
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenEl ? tokenEl.value : '';
    }

    // =============================================
    // START
    // =============================================
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();