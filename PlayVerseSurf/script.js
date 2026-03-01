/* ===========================
   VERSE SURF — INTERACTIONS
   =========================== */

document.addEventListener('DOMContentLoaded', () => {

    // ——————————————————————————
    // Scroll Animations (Intersection Observer)
    // ——————————————————————————
    const animatedElements = document.querySelectorAll('.animate-in');

    const observerOptions = {
        root: null,
        rootMargin: '0px 0px -60px 0px',
        threshold: 0.15
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    animatedElements.forEach(el => observer.observe(el));

    // ——————————————————————————
    // Navbar Scroll Effect
    // ——————————————————————————
    const navbar = document.getElementById('navbar');
    let lastScroll = 0;

    const handleScroll = () => {
        const currentScroll = window.scrollY;

        if (currentScroll > 80) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }

        lastScroll = currentScroll;
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll(); // initial check

    // ——————————————————————————
    // Mobile Navigation Toggle
    // ——————————————————————————
    const navToggle = document.getElementById('nav-toggle');
    const navLinks = document.getElementById('nav-links');

    navToggle.addEventListener('click', () => {
        navToggle.classList.toggle('active');
        navLinks.classList.toggle('active');
        document.body.style.overflow = navLinks.classList.contains('active') ? 'hidden' : '';
    });

    // Close mobile nav on link click
    navLinks.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            navToggle.classList.remove('active');
            navLinks.classList.remove('active');
            document.body.style.overflow = '';
        });
    });

    // ——————————————————————————
    // Smooth Scrolling for Anchor Links
    // ——————————————————————————
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', (e) => {
            const targetId = anchor.getAttribute('href');
            if (targetId === '#') return;

            const targetEl = document.querySelector(targetId);
            if (!targetEl) return;

            e.preventDefault();
            const navHeight = navbar.offsetHeight;
            const targetPosition = targetEl.getBoundingClientRect().top + window.scrollY - navHeight;

            window.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
        });
    });

    // ——————————————————————————
    // Vinyl Pause on Hover
    // ——————————————————————————
    const vinylRecord = document.querySelector('.vinyl-record');
    if (vinylRecord) {
        const vinylContainer = vinylRecord.closest('.vinyl-container');
        vinylContainer.addEventListener('mouseenter', () => {
            vinylRecord.style.animationPlayState = 'paused';
        });
        vinylContainer.addEventListener('mouseleave', () => {
            vinylRecord.style.animationPlayState = 'running';
        });
    }

    // ——————————————————————————
    // Parallax effect on hero scroll
    // ——————————————————————————
    const heroVisual = document.querySelector('.hero-visual');
    const heroText = document.querySelector('.hero-text');

    if (heroVisual && heroText) {
        window.addEventListener('scroll', () => {
            const scrollY = window.scrollY;
            const heroHeight = document.querySelector('.hero').offsetHeight;

            if (scrollY < heroHeight) {
                const progress = scrollY / heroHeight;
                heroVisual.style.transform = `translateY(${scrollY * 0.15}px)`;
                heroText.style.transform = `translateY(${scrollY * 0.08}px)`;
                heroText.style.opacity = 1 - progress * 0.6;
            }
        }, { passive: true });
    }

    // ——————————————————————————
    // Character cards tilt effect
    // ——————————————————————————
    const characterCards = document.querySelectorAll('.character-card');

    characterCards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = (y - centerY) / centerY * -8;
            const rotateY = (x - centerX) / centerX * 8;

            card.style.transform = `translateY(-8px) scale(1.03) perspective(500px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
        });
    });

    // ——————————————————————————
    // Scroll indicator hide on scroll
    // ——————————————————————————
    const scrollIndicator = document.querySelector('.hero-scroll-indicator');
    if (scrollIndicator) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 100) {
                scrollIndicator.style.opacity = '0';
                scrollIndicator.style.pointerEvents = 'none';
            } else {
                scrollIndicator.style.opacity = '1';
                scrollIndicator.style.pointerEvents = '';
            }
        }, { passive: true });
    }

});
