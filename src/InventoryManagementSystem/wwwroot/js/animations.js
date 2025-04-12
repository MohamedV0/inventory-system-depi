// Counter Animation
function animateCounter(element) {
    const target = parseInt(element.dataset.value);
    const duration = 2000; // 2 seconds
    const step = target / (duration / 16); // 60fps
    let current = 0;

    const updateCounter = () => {
        current += step;
        if (current >= target) {
            element.textContent = target;
            return;
        }
        element.textContent = Math.floor(current);
        requestAnimationFrame(updateCounter);
    };

    updateCounter();
}

// Animate CSS utility function
function animateCSS(element, animation, prefix = 'animate__') {
    return new Promise((resolve, reject) => {
        const animationName = `${prefix}${animation}`;
        const node = document.querySelector(element);
        
        if (!node) return resolve('Element not found');
        
        node.classList.add(`${prefix}animated`, animationName);
        
        function handleAnimationEnd(event) {
            event.stopPropagation();
            node.classList.remove(`${prefix}animated`, animationName);
            resolve('Animation ended');
        }
        
        node.addEventListener('animationend', handleAnimationEnd, {once: true});
    });
}

// Initialize animations when document is ready
document.addEventListener('DOMContentLoaded', function() {
    // Force dark mode
    document.body.classList.add('dark-mode');

    // Hero section animation
    const heroContent = document.querySelector('.hero-content');
    if (heroContent) {
        heroContent.classList.add('animate__animated', 'animate__fadeInUp');
    }

    // Animate counters
    document.querySelectorAll('.counter').forEach(counter => {
        // Only animate if element is visible
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        });
        
        observer.observe(counter);
    });

    // Staggered animation for cards
    document.querySelectorAll('.content-card').forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('animate__animated', 'animate__fadeInUp');
    });

    // Stats animation with counter
    document.querySelectorAll('.stat-card, .stat-card-dark').forEach((statCard, index) => {
        statCard.style.animationDelay = `${index * 0.1}s`;
        statCard.classList.add('animate__animated', 'animate__fadeInUp');

        const statValue = statCard.querySelector('.stat-value, .stat-value-dark');
        if (statValue) {
            let value = parseInt(statValue.dataset.value) || 0;
            statValue.textContent = '0';

            setTimeout(() => {
                let startValue = 0;
                const duration = 2000;
                const startTime = performance.now();

                function updateValue(currentTime) {
                    const elapsed = currentTime - startTime;
                    const progress = Math.min(elapsed / duration, 1);

                    const currentValue = Math.floor(progress * value);
                    statValue.textContent = currentValue;

                    if (progress < 1) {
                        requestAnimationFrame(updateValue);
                    } else {
                        statValue.textContent = value;
                    }
                }

                requestAnimationFrame(updateValue);
            }, 500);
        }
    });

    // Steps animation
    document.querySelectorAll('.step').forEach((step, index) => {
        step.style.animationDelay = `${0.3 + index * 0.1}s`;
        step.classList.add('animate__animated', 'animate__fadeInRight');
    });

    // Add particle effect
    const particles = document.querySelectorAll('.hero-particles');
    particles.forEach(particle => {
        for (let i = 0; i < 50; i++) {
            const dot = document.createElement('div');
            dot.className = 'particle';
            const size = Math.random() * 3;
            dot.style.width = `${size}px`;
            dot.style.height = `${size}px`;
            dot.style.left = `${Math.random() * 100}%`;
            dot.style.top = `${Math.random() * 100}%`;
            dot.style.animationDuration = `${Math.random() * 3 + 2}s`;
            dot.style.animationDelay = `${Math.random() * 2}s`;
            particle.appendChild(dot);
        }
    });
}); 