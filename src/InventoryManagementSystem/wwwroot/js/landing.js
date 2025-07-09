// Performance optimized animation utilities
const animationUtils = {
    // Throttle function to limit execution frequency
    throttle: (func, limit) => {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    // Optimized counter animation using requestAnimationFrame
    animateCounter: (element) => {
        if (!element) return;
        
        const target = parseInt(element.dataset.value) || 0;
        const duration = 5000; // Increased duration for better visual effect
        const start = performance.now();
        const frameRate = 24; // Reduced frame rate for more visible counting
        const frameDuration = 1000 / frameRate;
        let lastFrame = 0;
        
        const update = (currentTime) => {
            // Add frame rate limiting for more visible counting
            if (currentTime - lastFrame < frameDuration) {
                requestAnimationFrame(update);
                return;
            }
            
            const elapsed = currentTime - start;
            const progress = Math.min(elapsed / duration, 1);
            
            // Add easing for smoother animation
            const easeOutQuad = t => t * (2 - t);
            const easedProgress = easeOutQuad(progress);
            
            if (progress < 1) {
                const current = Math.floor(easedProgress * target);
                if (current !== parseInt(element.textContent)) {
                    element.textContent = current;
                }
                lastFrame = currentTime;
                requestAnimationFrame(update);
            } else {
                element.textContent = target;
            }
        };
        
        requestAnimationFrame(update);
    },

    // Optimized animate CSS utility
    animateCSS: (element, animation, prefix = 'animate__') => {
        return new Promise((resolve) => {
            const node = typeof element === 'string' ? document.querySelector(element) : element;
            if (!node) return resolve('Element not found');
            
            const animationName = `${prefix}${animation}`;
            node.classList.add(`${prefix}animated`, animationName);
            
            const cleanup = () => {
                node.classList.remove(`${prefix}animated`, animationName);
                resolve('Animation ended');
            };
            
            node.addEventListener('animationend', cleanup, { once: true });
        });
    },

    // Optimized intersection observer setup
    createIntersectionObserver: (callback, options = {}) => {
        return new IntersectionObserver(callback, {
            threshold: 0.1,
            rootMargin: '50px',
            ...options
        });
    }
};

// Initialize animations when document is ready
document.addEventListener('DOMContentLoaded', () => {
    // Force dark mode
    document.body.classList.add('dark-mode');

    // Optimize animations for mobile
    const isMobile = window.innerWidth < 768;
    const animationDelay = isMobile ? 50 : 100; // Faster delays on mobile

    // Hero section animation - only if element exists
    const heroContent = document.querySelector('.hero-content');
    if (heroContent) {
        requestAnimationFrame(() => {
            heroContent.classList.add('animate__animated', 'animate__fadeInUp');
        });
    }

    // Optimize counter animations with intersection observer
    const counterObserver = animationUtils.createIntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animationUtils.animateCounter(entry.target);
                counterObserver.unobserve(entry.target);
            }
        });
    });

    document.querySelectorAll('.counter').forEach(counter => {
        counterObserver.observe(counter);
    });

    // Optimize card animations
    const cardObserver = animationUtils.createIntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                const element = entry.target;
                setTimeout(() => {
                    element.classList.add('animate__animated', 'animate__fadeInUp');
                }, index * animationDelay);
                cardObserver.unobserve(element);
            }
        });
    });

    document.querySelectorAll('.content-card').forEach(card => {
        cardObserver.observe(card);
    });

    // Optimize stats animation
    const statsObserver = animationUtils.createIntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                const statCard = entry.target;
                const statValue = statCard.querySelector('.stat-value, .stat-value-dark');
                
                setTimeout(() => {
                    statCard.classList.add('animate__animated', 'animate__fadeInUp');
                    if (statValue) {
                        animationUtils.animateCounter(statValue);
                    }
                }, index * animationDelay);
                
                statsObserver.unobserve(statCard);
            }
        });
    });

    document.querySelectorAll('.stat-card, .stat-card-dark').forEach(statCard => {
        statsObserver.observe(statCard);
    });

    // Remove steps animation section
    
    // Optimize particle effect
    const createParticles = animationUtils.throttle(() => {
        document.querySelectorAll('.hero-particles').forEach(particle => {
            // Clear existing particles
            particle.innerHTML = '';
            
            // Reduce particle count on mobile
            const particleCount = isMobile ? 25 : 50;
            
            // Create particles in batches
            const fragment = document.createDocumentFragment();
            
            for (let i = 0; i < particleCount; i++) {
                const dot = document.createElement('div');
                dot.className = 'particle';
                const size = Math.random() * (isMobile ? 2 : 3);
                
                Object.assign(dot.style, {
                    width: `${size}px`,
                    height: `${size}px`,
                    left: `${Math.random() * 100}%`,
                    top: `${Math.random() * 100}%`,
                    animationDuration: `${Math.random() * 2 + 2}s`,
                    animationDelay: `${Math.random() * 1.5}s`
                });
                
                fragment.appendChild(dot);
            }
            
            particle.appendChild(fragment);
        });
    }, 1000);

    // Initial particle creation
    createParticles();

    // Recreate particles on resize (throttled)
    window.addEventListener('resize', createParticles);
}); 