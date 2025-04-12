/**
 * SweetAlert2 utilities for the Inventory Management System
 */

// Base SweetAlert2 configurations
const SwalConfig = {
    // Default configuration for all alerts
    base: {
        customClass: {
            popup: 'swal2-popup',
            title: 'swal2-title',
            htmlContainer: 'swal2-html-container',
            confirmButton: 'swal2-confirm btn btn-primary',
            cancelButton: 'swal2-cancel btn btn-secondary'
        },
        allowOutsideClick: true,
        backdrop: true,
        buttonsStyling: false,
        showClass: {
            popup: 'animate__animated animate__fadeInDown animate__faster'
        },
        hideClass: {
            popup: 'animate__animated animate__fadeOutUp animate__faster'
        }
    },
    
    // Configuration for delete confirmations
    delete: {
        customClass: {
            popup: 'swal2-popup swal2-modal-delete',
            title: 'swal2-title',
            htmlContainer: 'swal2-html-container',
            confirmButton: 'swal2-confirm btn btn-danger',
            cancelButton: 'swal2-cancel btn btn-outline-light'
        },
        icon: 'warning',
        iconColor: '#ef4444',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-trash-alt me-1"></i>Yes, delete',
        cancelButtonText: '<i class="fas fa-times me-1"></i>Cancel',
        reverseButtons: true,
        focusCancel: true,
        allowOutsideClick: false,
        allowEscapeKey: true,
        backdrop: true,
        buttonsStyling: false,
        showClass: {
            popup: 'animate__animated animate__fadeInDown animate__faster'
        },
        hideClass: {
            popup: 'animate__animated animate__fadeOutUp animate__faster'
        }
    },
    
    // Configuration for toast-style alerts
    toast: {
        customClass: {
            popup: 'swal2-popup swal2-toast',
            title: 'swal2-title'
        },
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        background: 'var(--bg-card)',
        iconColor: 'var(--text-primary)',
        showClass: {
            popup: 'animate__animated animate__fadeInRight animate__faster'
        },
        hideClass: {
            popup: 'animate__animated animate__fadeOutRight animate__faster'
        }
    }
};

/**
 * Configuration for delete confirmations by entity type
 */
const DeleteConfigs = {
    product: {
        icon: 'warning',
        warningMessage: 'Product will be removed. Inventory history will remain.'
    },
    category: {
        icon: 'warning',
        warningMessage: 'Category will be removed. Products will stay intact.'
    },
    supplier: {
        icon: 'warning',
        warningMessage: 'Supplier will be removed. Product links will remain.'
    },
    // Default fallback
    default: {
        icon: 'warning',
        warningMessage: 'This action is permanent and cannot be reversed.'
    }
};

/**
 * Shows a delete confirmation dialog
 * @param {Object} options - Configuration options
 * @param {string} options.entityType - Type of entity being deleted (product, category, supplier, etc.)
 * @param {string} options.entityName - Name of the entity being deleted
 * @param {string} options.formId - ID of the form to submit
 * @param {string} [options.title] - Optional custom title
 * @param {string} [options.customWarning] - Optional custom warning message
 */
function showDeleteConfirmation(options) {
    const {
        entityType = 'item',
        entityName,
        formId,
        title,
        customWarning
    } = options;

    // Get entity-specific configuration or fall back to default
    const config = DeleteConfigs[entityType] || DeleteConfigs.default;

    const formattedHtml = `
        <div class="delete-message">Are you sure you want to delete this ${entityType}?</div>
        ${entityName ? `<div class="entity-name">${entityName}</div>` : ''}
        <div class="warning-text">
            <i class="fas fa-exclamation-triangle"></i>
            <div>${customWarning || config.warningMessage}</div>
        </div>
    `;

    return Swal.fire({
        ...SwalConfig.delete,
        title: title || `Delete ${entityType.charAt(0).toUpperCase() + entityType.slice(1)}`,
        html: formattedHtml,
        icon: config.icon,
        willOpen: (popup) => {
            const icon = popup.querySelector('.swal2-icon');
            if (icon) icon.classList.add('animate__animated', 'animate__heartBeat');
        },
        preConfirm: () => {
            return new Promise((resolve) => {
                if (formId) {
                    const form = document.getElementById(formId);
                    if (form) {
                        const btn = Swal.getConfirmButton();
                        if (btn) {
                            btn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Deleting...';
                            btn.disabled = true;
                        }
                        setTimeout(() => form.submit(), 300);
                    }
                }
                resolve();
            });
        }
    }).then((result) => {
        if (result.isDismissed) {
            toastr.info('Action cancelled', '', {
                timeOut: 2000,
                closeButton: false,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }).catch(handleSweetAlertError);
}

/**
 * Handles SweetAlert errors consistently
 */
function handleSweetAlertError(error) {
    console.error('Operation failed:', error);
    toastr.error('An error occurred', '', {
        timeOut: 3000,
        closeButton: true,
        progressBar: true
    });
    cleanupSweetAlert();
}

/**
 * Cleans up SweetAlert DOM elements and styles
 */
function cleanupSweetAlert() {
    const modalBackdrops = document.querySelectorAll('.swal2-backdrop-show, .swal2-container');
    modalBackdrops.forEach(el => el.remove());
    document.body.classList.remove('swal2-shown', 'swal2-height-auto');
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
}

function showConfirmation(options) {
    const config = {
        ...SwalConfig.base,
        title: options.title,
        text: options.text,
        html: options.html || options.text,
        icon: options.icon || 'question',
        showCancelButton: true,
        confirmButtonText: options.confirmButtonText || '<i class="fas fa-check me-1"></i>Yes, proceed',
        cancelButtonText: options.cancelButtonText || '<i class="fas fa-times me-1"></i>Cancel',
        reverseButtons: options.reverseButtons !== false,
        focusCancel: options.focusCancel !== false,
        willOpen: (popup) => {
            // Add animation to the icon based on type
            const icon = popup.querySelector('.swal2-icon');
            if (icon) {
                icon.classList.add('animate__animated');
                
                if (options.icon === 'success') {
                    icon.classList.add('animate__bounceIn');
                } else if (options.icon === 'error') {
                    icon.classList.add('animate__shakeX');
                } else {
                    icon.classList.add('animate__pulse');
                }
            }
            
            // Run custom open handler if provided
            if (typeof options.onOpen === 'function') {
                options.onOpen(popup);
            }
        }
    };
    
    // Use custom classes if provided
    if (options.customClass) {
        config.customClass = {
            ...config.customClass,
            ...options.customClass
        };
    }
    
    return Swal.fire(config).then(result => {
        // Always restore body scrolling when modal is closed
        // This fixes the scroll lock issue
        setTimeout(() => {
            if (document.body.classList.contains('swal2-shown')) {
                document.body.classList.remove('swal2-shown', 'swal2-height-auto');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }
        }, 100);
        
        return result;
    });
}

function showToast(title, icon = 'success') {
    toastr[icon](title, '', {
        timeOut: 3000,
        closeButton: false,
        progressBar: true,
        positionClass: 'toast-top-right'
    });
}

/**
 * Initialize delete buttons with SweetAlert2 confirmation
 */
function initDeleteConfirmations(options = {}) {
    const settings = {
        selector: options.selector || '.delete-item',
        formIdPrefix: options.formIdPrefix || 'delete-form-',
        entityNameAttr: options.entityNameAttr || 'data-entity-name',
        entityIdAttr: options.entityIdAttr || 'data-entity-id',
        entityType: options.entityType || 'item'
    };

    $(settings.selector).click(function(e) {
        e.preventDefault();
        const $this = $(this);
        const entityId = $this.attr(settings.entityIdAttr);
        const entityName = $this.attr(settings.entityNameAttr);
        const formId = $this.data('form-id') || `${settings.formIdPrefix}${entityId}`;
        
        // Show loading state on button
        const $icon = $this.find('i');
        if ($icon.length) {
            const originalClass = $icon.attr('class');
            $icon.attr('class', 'fas fa-spinner fa-spin');
            setTimeout(() => $icon.attr('class', originalClass), 300);
        }
        
        showDeleteConfirmation({
            entityType: settings.entityType,
            entityName: entityName,
            formId: formId,
            customWarning: $this.data('warning-message')
        });
    });
}

/**
 * Initialize action confirmation for any action requiring confirmation
 * @param {string} selector - The CSS selector for buttons requiring confirmation
 */
function initActionConfirmations(selector = '.confirm-action') {
    $(selector).click(function(e) {
        e.preventDefault();
        
        const $this = $(this);
        const title = $this.data('confirm-title') || 'Confirm Action';
        const text = $this.data('confirm-message') || 'Are you sure you want to proceed with this action?';
        const html = $this.data('confirm-html') || null;
        const icon = $this.data('confirm-icon') || 'question';
        const confirmText = $this.data('confirm-button-text') || '<i class="fas fa-check me-1"></i>Yes, proceed';
        const cancelText = $this.data('cancel-button-text') || '<i class="fas fa-times me-1"></i>Cancel';
        const formId = $this.data('form-id');
        const url = $this.attr('href') || $this.data('url');
        
        // Add visual feedback on button click
        const $icon = $this.find('i');
        if ($icon.length) {
            const originalClass = $icon.attr('class');
            $icon.attr('class', 'fas fa-spinner fa-spin');
            
            // Restore original icon after a brief delay
            setTimeout(() => {
                $icon.attr('class', originalClass);
            }, 300);
        }
        
        showConfirmation({
            title: title,
            text: text,
            html: html,
            icon: icon,
            confirmButtonText: confirmText,
            cancelButtonText: cancelText,
            customClass: $this.data('confirm-danger') ? {
                confirmButton: 'swal2-confirm btn btn-danger'
            } : undefined
        }).then((result) => {
            if (result.isConfirmed) {
                // Add loading state to button
                $this.prop('disabled', true);
                
                if ($icon.length) {
                    $icon.attr('class', 'fas fa-spinner fa-spin');
                }
                
                if (formId) {
                    // Submit the specified form
                    document.getElementById(formId).submit();
                } else if (url) {
                    // Navigate to the URL
                    window.location.href = url;
                } else if ($this.is('button[type="submit"]')) {
                    // Submit the closest form
                    $this.closest('form').submit();
                }
            }
        });
    });
}

// Document ready function to initialize confirmation dialogs
$(document).ready(function() {
    // Load Animate.css if not already present
    if (!document.querySelector('link[href*="animate.css"]')) {
        const animateCss = document.createElement('link');
        animateCss.rel = 'stylesheet';
        animateCss.href = 'https://cdnjs.cloudflare.com/ajax/libs/animate.css/4.1.1/animate.min.css';
        document.head.appendChild(animateCss);
    }
    
    // Global event handler to fix scrolling issues with SweetAlert2
    $(document).on('click', '.swal2-container, .swal2-cancel, .swal2-confirm', function() {
        setTimeout(() => {
            if (!document.querySelector('.swal2-container')) {
                document.body.classList.remove('swal2-shown', 'swal2-height-auto');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }
        }, 100);
    });

    // Initialize delete confirmations for different entity types
    const entityTypes = [
        { selector: '.delete-product', type: 'product' },
        { selector: '.delete-category', type: 'category' },
        { selector: '.delete-supplier', type: 'supplier' },
        { selector: '.delete-item', type: 'item' }
    ];

    entityTypes.forEach(({ selector, type }) => {
        initDeleteConfirmations({
            selector: selector,
            entityIdAttr: `data-${type}-id`,
            entityNameAttr: `data-${type}-name`,
            formIdPrefix: 'delete-form-',
            entityType: type
        });
    });

    // Initialize general action confirmations
    initActionConfirmations('.confirm-action');
}); 