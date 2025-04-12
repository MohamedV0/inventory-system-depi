/**
 * Table utilities for sorting, filtering, and other common table operations
 */
class TableUtils {
    constructor(tableSelector, options = {}) {
        this.table = $(tableSelector);
        this.options = {
            sortableClass: 'sortable',
            activeClass: 'active',
            ...options
        };
        
        this.currentSort = {
            column: '',
            direction: 'asc'
        };

        this.init();
    }

    init() {
        this.initSorting();
    }

    initSorting() {
        const self = this;

        this.table.find(`.${this.options.sortableClass}`).click(function() {
            const column = $(this).data('sort');
            
            // Update sort direction
            if (self.currentSort.column === column) {
                self.currentSort.direction = self.currentSort.direction === 'asc' ? 'desc' : 'asc';
            } else {
                self.currentSort.column = column;
                self.currentSort.direction = 'asc';
            }

            // Update sort icons
            self.table.find(`.${self.options.sortableClass} i`)
                .attr('class', 'fas fa-sort ms-1 text-muted');
            $(this).find('i')
                .attr('class', `fas fa-sort-${self.currentSort.direction === 'asc' ? 'up' : 'down'} ms-1 text-primary`);

            self.sortTable(column);
        });
    }

    sortTable(column) {
        const rows = this.table.find('tbody tr').get();
        const columnIndex = this.getColumnIndex(column);

        rows.sort((a, b) => {
            let aValue = $(a).children('td').eq(columnIndex).text().trim();
            let bValue = $(b).children('td').eq(columnIndex).text().trim();

            return this.compareValues(aValue, bValue, column);
        });

        this.table.find('tbody').empty().append(rows);
    }

    compareValues(aValue, bValue, column) {
        // Handle different types of values
        if (this.isNumericColumn(column)) {
            aValue = parseFloat(aValue) || 0;
            bValue = parseFloat(bValue) || 0;
        } else if (this.isPriceColumn(column)) {
            aValue = parseFloat(aValue.replace(/[^0-9.-]+/g, '')) || 0;
            bValue = parseFloat(bValue.replace(/[^0-9.-]+/g, '')) || 0;
        } else {
            aValue = aValue.toLowerCase();
            bValue = bValue.toLowerCase();
        }

        if (aValue < bValue) return this.currentSort.direction === 'asc' ? -1 : 1;
        if (aValue > bValue) return this.currentSort.direction === 'asc' ? 1 : -1;
        return 0;
    }

    isNumericColumn(column) {
        return ['id', 'stock', 'quantity'].includes(column);
    }

    isPriceColumn(column) {
        return ['price', 'cost', 'amount'].includes(column);
    }

    getColumnIndex(column) {
        let index = 0;
        this.table.find('thead th').each(function(i) {
            if ($(this).data('sort') === column) {
                index = i;
                return false;
            }
        });
        return index;
    }
}

// Initialize filters
function initializeTableFilters(filterContainerSelector, rowSelector, options = {}) {
    const defaults = {
        serverSide: false,
        filterParam: 'stockFilter',
        urlParams: {},
        baseUrl: null
    };
    
    const settings = { ...defaults, ...options };
    
    const filterMap = {
        // Product filters
        'low': 'low-stock',
        'out': 'out-of-stock',
        // Category filters
        'active': 'active',
        'inactive': 'inactive'
    };

    $(filterContainerSelector + ' .filter-btn').click(function() {
        const $buttons = $(filterContainerSelector + ' .filter-btn');
        $buttons.removeClass('active');
        $(this).addClass('active');

        const filter = $(this).data('filter');
        
        if (settings.serverSide) {
            // Handle server-side filtering
            handleServerFilter(filter, settings);
        } else {
            // Handle client-side filtering
            handleClientFilter(filter, rowSelector, filterMap);
        }
    });
}

// Handle client-side filtering
function handleClientFilter(filter, rowSelector, filterMap) {
    const $rows = $(rowSelector);
    
    if (filter === 'all') {
        $rows.show();
    } else {
        $rows.hide();
        // Use the mapped class name from filterMap
        const filterClass = filterMap[filter] || filter;
        $(rowSelector + '.' + filterClass).show();
    }
}

// Handle server-side filtering
function handleServerFilter(filter, settings) {
    const currentUrl = new URL(window.location);
    
    // Remove filter parameter if "all" is selected
    if (filter === 'all') {
        currentUrl.searchParams.delete(settings.filterParam);
    } else {
        currentUrl.searchParams.set(settings.filterParam, filter);
    }
    
    // Reset to page 1 when changing filters
    currentUrl.searchParams.set('page', 1);
    
    // Add any additional URL parameters
    if (settings.urlParams) {
        Object.entries(settings.urlParams).forEach(([key, value]) => {
            if (value !== null && value !== undefined) {
                currentUrl.searchParams.set(key, value);
            }
        });
    }
    
    // Navigate to the new URL
    window.location = currentUrl;
}

// Initialize entries dropdown
function initializeEntriesDropdown(dropdownSelector) {
    $(dropdownSelector).change(function() {
        const pageSize = $(this).val();
        const currentUrl = new URL(window.location);
        currentUrl.searchParams.set('pageSize', pageSize);
        currentUrl.searchParams.set('page', 1);
        window.location = currentUrl;
    });

    // Set initial value
    const currentPageSize = new URLSearchParams(window.location.search).get('pageSize') || "10";
    $(dropdownSelector).val(currentPageSize);
}

// Initialize search clear button
function initializeSearchClear(clearButtonSelector) {
    $(clearButtonSelector).click(function() {
        const currentUrl = new URL(window.location);
        currentUrl.searchParams.delete('searchTerm');
        window.location = currentUrl;
    });
} 