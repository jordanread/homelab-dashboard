/* ============================================
   THE HOLD — Main JavaScript
   ============================================ */

// ---- THEME ----
(function() {
  const stored = localStorage.getItem('hold-theme');
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  const theme = stored || (prefersDark ? 'dark' : 'light');
  document.documentElement.setAttribute('data-theme', theme);
})();

function initTheme() {
  const toggle = document.getElementById('themeToggle');
  if (!toggle) return;

  function updateIcon() {
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    toggle.textContent = isDark ? '☀️' : '🌙';
    toggle.title = isDark ? 'Switch to light mode' : 'Switch to dark mode';
  }

  updateIcon();

  toggle.addEventListener('click', () => {
    const current = document.documentElement.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('hold-theme', next);
    updateIcon();
  });
}

// ---- MOBILE NAV ----
function initMobileNav() {
  const hamburger = document.getElementById('hamburger');
  const mobileNav = document.getElementById('mobileNav');
  if (!hamburger || !mobileNav) return;

  hamburger.addEventListener('click', () => {
    mobileNav.classList.toggle('open');
    hamburger.textContent = mobileNav.classList.contains('open') ? '✕' : '☰';
  });

  mobileNav.querySelectorAll('a').forEach(a => {
    a.addEventListener('click', () => {
      mobileNav.classList.remove('open');
      hamburger.textContent = '☰';
    });
  });
}

// ---- COUNT-UP ANIMATION ----
function countUp(el, target, duration = 2000, suffix = '') {
  const start = performance.now();
  const isFloat = target % 1 !== 0;
  const decimals = isFloat ? 1 : 0;

  function update(now) {
    const elapsed = now - start;
    const progress = Math.min(elapsed / duration, 1);
    // Ease out cubic
    const eased = 1 - Math.pow(1 - progress, 3);
    const value = eased * target;

    el.textContent = value.toFixed(decimals) + suffix;

    if (progress < 1) requestAnimationFrame(update);
    else el.textContent = target.toFixed(decimals) + suffix;
  }

  requestAnimationFrame(update);
}

// ---- GAUGE ANIMATION ----
function animateGauge(el, pct) {
  const circumference = 339.29;
  const offset = circumference - (pct / 100) * circumference;

  // Color thresholds
  if (pct > 85) {
    el.classList.add('danger');
  } else if (pct > 70) {
    el.classList.add('warn');
  }

  setTimeout(() => {
    el.style.strokeDashoffset = offset;
  }, 200);
}

// ---- INTERSECTION OBSERVER (fade-in + trigger animations) ----
function initObserver() {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const el = entry.target;
        el.classList.add('visible');

        // Count-up numbers
        el.querySelectorAll('[data-count]').forEach(counter => {
          if (counter.dataset.animated) return;
          counter.dataset.animated = '1';
          const target = parseFloat(counter.dataset.count);
          const suffix = counter.dataset.suffix || '';
          countUp(counter, target, 1800, suffix);
        });

        // Gauge
        el.querySelectorAll('[data-gauge]').forEach(gauge => {
          if (gauge.dataset.animated) return;
          gauge.dataset.animated = '1';
          const pct = parseFloat(gauge.dataset.gauge);
          animateGauge(gauge, pct);
        });

        // Pct text
        el.querySelectorAll('[data-pct]').forEach(pct => {
          if (pct.dataset.animated) return;
          pct.dataset.animated = '1';
          const val = parseFloat(pct.dataset.pct);
          countUp(pct, val, 1800, '%');
        });

        observer.unobserve(el);
      }
    });
  }, { threshold: 0.15 });

  document.querySelectorAll('.fade-in, .stat-card, .service-card, .disk-container').forEach(el => {
    observer.observe(el);
  });
}

// ---- OS TABS (certificate section) ----
function initOsTabs() {
  document.querySelectorAll('.os-tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const group = tab.closest('[data-tab-group]');
      if (!group) return;

      group.querySelectorAll('.os-tab').forEach(t => t.classList.remove('active'));
      group.querySelectorAll('.os-instructions').forEach(i => i.classList.remove('active'));

      tab.classList.add('active');
      const target = group.querySelector(`[data-os="${tab.dataset.os}"]`);
      if (target) target.classList.add('active');
    });
  });
}

// ---- HERO SHIP ICON FLOAT ----
function initHeroFloat() {
  const ship = document.getElementById('heroShip');
  if (!ship) return;

  let t = 0;
  function animate() {
    t += 0.02;
    ship.style.transform = `translateY(${Math.sin(t) * 6}px) rotate(${Math.sin(t * 0.7) * 2}deg)`;
    requestAnimationFrame(animate);
  }
  animate();
}

// ---- NAV SCROLL HIGHLIGHT ----
function initScrollHighlight() {
  const sections = document.querySelectorAll('[data-section]');
  const navLinks = document.querySelectorAll('.nav-links a[data-nav]');

  if (!sections.length || !navLinks.length) return;

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const id = entry.target.dataset.section;
        navLinks.forEach(a => {
          a.classList.toggle('active', a.dataset.nav === id);
        });
      }
    });
  }, { rootMargin: '-40% 0px -40% 0px' });

  sections.forEach(s => observer.observe(s));
}

// ---- INIT ----
document.addEventListener('DOMContentLoaded', () => {
  initTheme();
  initMobileNav();
  initObserver();
  initOsTabs();
  initHeroFloat();
  initScrollHighlight();
});
