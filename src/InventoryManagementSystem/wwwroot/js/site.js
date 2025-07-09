// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Initialize tooltips site-wide
$(document).ready(function() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            delay: { show: 200, hide: 100 },
            trigger: 'hover focus'
        });
    });
    
    // Make tooltip icons accessible
    document.querySelectorAll('.tooltip-icon').forEach(function(icon) {
        icon.setAttribute('tabindex', '0');
        icon.setAttribute('role', 'button');
        icon.setAttribute('aria-label', 'Help');
    });
    
    // Initialize password toggle functionality
    initializePasswordToggles();
});

// Handle currency formatting in forms
$(document).ready(function() {
    // Clear any existing $ symbols from inputs on page load
    $(".currency-input").each(function() {
        var value = $(this).val();
        if (value) {
            // Remove any existing currency symbols and commas
            value = value.replace(/[$,]/g, "");
            $(this).val(value);
        }
    });
    
    // Disable Inputmask for now as it's causing conflicts
    /* 
    if (typeof Inputmask !== 'undefined') {
        $(".currency-input").inputmask("currency", {
            prefix: "$",
            allowMinus: false,
            digits: 2,
            rightAlign: false
        });
    }
    */
    
    // Handle form submission to clean currency inputs
    $("form").on("submit", function(e) {
        var hasErrors = false;
        
        $(".currency-input").each(function() {
            // Remove currency symbols and commas before submitting
            var value = $(this).val();
            var numericValue = value.replace(/[$,]/g, "");
            
            // Check if it's a valid number after cleaning
            if (isNaN(parseFloat(numericValue))) {
                hasErrors = true;
            } else {
                $(this).val(numericValue);
            }
        });
        
        if (hasErrors) {
            console.log("Form has validation errors");
            // Let the form validation handle the errors
        }
    });
});

// Password toggle functionality
function initializePasswordToggles() {
    // Password visibility toggle functionality
    $('.password-toggle').on('click', function(e) {
        e.preventDefault();
        
        const $button = $(this);
        const $input = $button.closest('.input-group').find('.password-input');
        const $icon = $button.find('.toggle-icon');
        
        // Toggle password visibility
        const isVisible = $input.attr('type') === 'text';
        $input.attr('type', isVisible ? 'password' : 'text');
        
        // Toggle icon
        $icon.toggleClass('fa-eye fa-eye-slash');
        
        // Update button title for accessibility
        $button.attr('title', isVisible ? 'Show Password' : 'Hide Password');
        
        // Prevent the button from submitting the form
        return false;
    });

    // Security measure: Reset password fields to type="password" when forms are submitted
    $('form').on('submit', function() {
        $('.password-input').attr('type', 'password');
    });

    // Security measure: Reset password visibility when user navigates away
    $(window).on('beforeunload', function() {
        $('.password-input').attr('type', 'password');
        $('.toggle-icon').removeClass('fa-eye-slash').addClass('fa-eye');
    });
}

// Create a stock level doughnut chart
function createStockLevelChart(canvasId, stockLevel, reorderLevel) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    const isLowStock = stockLevel <= reorderLevel;
    const stockColor = isLowStock ? '#dc3545' : '#198754';
    
    // Create the chart
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [stockLevel, Math.max(reorderLevel * 2 - stockLevel, 0)],
                backgroundColor: [
                    stockColor,
                    '#e9ecef'
                ],
                borderWidth: 0,
                hoverOffset: 4
            }]
        },
        options: {
            cutout: '75%',
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false
                }
            },
            animation: {
                animateScale: true,
                animateRotate: true
            }
        }
    });
    
    // Add subtle glow effect to chart container when stock is low
    if (isLowStock) {
        const chartContainer = ctx.parentElement;
        if (chartContainer) {
            chartContainer.style.filter = 'drop-shadow(0 0 3px rgba(220, 53, 69, 0.3))';
        }
    }
}

// Enhanced modal behaviors
$(document).ready(function() {
    // Handle confirmation modals
    $('.confirmation-modal').on('show.bs.modal', function() {
        // Add slight delay to allow modal to appear before focusing
        setTimeout(() => {
            // Focus the cancel button by default for safety
            $(this).find('.btn-light').focus();
            
            // Add pulsing effect to the primary action button
            $(this).find('.btn-delete').addClass('pulse-effect');
        }, 300);
    });
    
    // Trap focus within the modal for accessibility
    $('.confirmation-modal').on('keydown', function(e) {
        // If Tab or Shift+Tab is pressed
        if (e.key === 'Tab') {
            const focusableElements = $(this).find('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            const firstElement = focusableElements.first();
            const lastElement = focusableElements.last();
            
            // If pressing Shift+Tab on the first element, wrap to the last
            if (e.shiftKey && document.activeElement === firstElement[0]) {
                e.preventDefault();
                lastElement.focus();
            } 
            // If pressing Tab on the last element, wrap to the first
            else if (!e.shiftKey && document.activeElement === lastElement[0]) {
                e.preventDefault();
                firstElement.focus();
            }
        }
        
        // Close on Escape key
        if (e.key === 'Escape') {
            $(this).modal('hide');
        }
    });
    
    // Add visual feedback on confirm button
    $('.btn-delete').hover(
        function() {
            $(this).find('i').addClass('fa-beat');
        },
        function() {
            $(this).find('i').removeClass('fa-beat');
        }
    );
});
