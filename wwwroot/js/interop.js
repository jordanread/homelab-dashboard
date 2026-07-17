/* ============================================
   THE HOLD — Blazor interop
   Theme persistence + cosmetic effects that don't
   need to round-trip to the server (count-up,
   fade-in-on-scroll, gauge fill, hero float).
   ============================================ */

window.holdTheme = {
  init() {
    const stored = localStorage.getItem('hold-theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const theme = stored || (prefersDark ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', theme);
    return theme;
  },
  set(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('hold-theme', theme);
  }
};

window.holdEffects = {
  init() {
    this._countUp = this._countUp.bind(this);
    this._initObserver();
    this._initHeroFloat();
    this._initScrollHighlight();
  },

  _countUp(el, target, duration, suffix) {
    const start = performance.now();
    const isFloat = target % 1 !== 0;
    const decimals = isFloat ? 1 : 0;

    function update(now) {
      const elapsed = now - start;
      const progress = Math.min(elapsed / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      const value = eased * target;
      el.textContent = value.toFixed(decimals) + suffix;
      if (progress < 1) requestAnimationFrame(update);
      else el.textContent = target.toFixed(decimals) + suffix;
    }
    requestAnimationFrame(update);
  },

  _animateGauge(el, pct) {
    const circumference = 339.29;
    const offset = circumference - (pct / 100) * circumference;
    if (pct > 85) el.classList.add('danger');
    else if (pct > 70) el.classList.add('warn');
    setTimeout(() => { el.style.strokeDashoffset = offset; }, 200);
  },

  _initObserver() {
    if (this._observer) this._observer.disconnect();

    this._observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (!entry.isIntersecting) return;
        const el = entry.target;
        el.classList.add('visible');

        el.querySelectorAll('[data-count]').forEach(counter => {
          if (counter.dataset.animated) return;
          counter.dataset.animated = '1';
          const target = parseFloat(counter.dataset.count);
          const suffix = counter.dataset.suffix || '';
          this._countUp(counter, target, 1800, suffix);
        });

        el.querySelectorAll('[data-gauge]').forEach(gauge => {
          if (gauge.dataset.animated) return;
          gauge.dataset.animated = '1';
          const pct = parseFloat(gauge.dataset.gauge);
          this._animateGauge(gauge, pct);
        });

        el.querySelectorAll('[data-pct]').forEach(pct => {
          if (pct.dataset.animated) return;
          pct.dataset.animated = '1';
          const val = parseFloat(pct.dataset.pct);
          this._countUp(pct, val, 1800, '%');
        });

        this._observer.unobserve(el);
      });
    }, { threshold: 0.15 });

    document.querySelectorAll('.fade-in, .stat-card, .service-card, .disk-container')
      .forEach(el => this._observer.observe(el));
  },

  _initHeroFloat() {
    const ship = document.getElementById('heroShip');
    if (!ship || ship.dataset.floating) return;
    ship.dataset.floating = '1';
    let t = 0;
    const animate = () => {
      if (!document.body.contains(ship)) return; // stop once nav'd away
      t += 0.02;
      ship.style.transform = `translateY(${Math.sin(t) * 6}px) rotate(${Math.sin(t * 0.7) * 2}deg)`;
      requestAnimationFrame(animate);
    };
    animate();
  },

  _initScrollHighlight() {
    const sections = document.querySelectorAll('[data-section]');
    const navLinks = document.querySelectorAll('.nav-links a[data-nav]');
    if (!sections.length || !navLinks.length) return;

    if (this._navObserver) this._navObserver.disconnect();
    this._navObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          const id = entry.target.dataset.section;
          navLinks.forEach(a => a.classList.toggle('active', a.dataset.nav === id));
        }
      });
    }, { rootMargin: '-40% 0px -40% 0px' });

    sections.forEach(s => this._navObserver.observe(s));
  },

  // Called after Blazor enhanced-navigation swaps the DOM so new content
  // (e.g. a freshly rendered stats grid) picks up fade-in/count-up again.
  refresh() {
    this._initObserver();
    this._initHeroFloat();
    this._initScrollHighlight();
  }
};

// Blazor Server's enhanced navigation replaces DOM nodes without a full
// page load — re-wire the observers whenever that happens.
document.addEventListener('DOMContentLoaded', () => window.holdEffects.init());
if (window.Blazor) {
  Blazor.addEventListener('enhancedload', () => window.holdEffects.refresh());
}
