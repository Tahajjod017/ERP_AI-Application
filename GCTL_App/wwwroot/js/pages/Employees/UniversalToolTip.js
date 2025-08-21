/**
 * Universal Tooltip Service
 * A reusable tooltip system for dynamic content loading
 */

class UniversalTooltipService {
    constructor(options = {}) {
        this.options = {
            containerClass: 'universal-tooltip-container',
            triggerClass: 'universal-tooltip-trigger',
            tooltipClass: 'universal-tooltip-box',
            position: { top: 10, left: 10 }, // Offset from cursor
            fadeInSpeed: 200,
            fadeOutSpeed: 200,
            hideDelay: 300,
            zIndex: 9999999,
            margin: 10, // Margin from viewport edges
            preferredPosition: 'below-right', // Default position preference
            ...options
        };

        this.hideTimer = null;
        this.$tooltip = null;
        this.init();
    }

    init() {
        this.createTooltip();
        this.bindEvents();
    }

    createTooltip() {
        if (this.$tooltip) return;

        this.$tooltip = $(`<div class="${this.options.tooltipClass}"></div>`).css({
            position: 'fixed',
            top: '0px',
            left: '0px',
            zIndex: this.options.zIndex,
            backgroundColor: 'rgb(255 247 209)',
            border: '1px solid #ccc',
            padding: '10px',
            minWidth: '250px',
            maxWidth: '400px',
            maxHeight: '300px',
            overflowY: 'auto',
            boxShadow: '0 3px 8px rgba(0,0,0,0.15)',
            display: 'none',
            fontSize: '13px',
            borderRadius: '4px'
        });

        $('body').append(this.$tooltip);
    }

    bindEvents() {
        const self = this;

        
        //$(document).on('mouseenter', `.${this.options.containerClass}`, function () {
        //    const $container = $(this);
        //    const $trigger = $container.find(`.${self.options.triggerClass}`);
        //    const config = self.getTooltipConfig($trigger);

        //    if (!config) return;

        //    clearTimeout(self.hideTimer);
        //    self.showTooltip($trigger, config);
        //});

        $(document).on('mouseenter', `.${this.options.containerClass}`, function (event) {
            const $container = $(this);
            const $trigger = $container.find(`.${self.options.triggerClass}`);
            const config = self.getTooltipConfig($trigger);

            if (!config) return;

            clearTimeout(self.hideTimer);
            self.showTooltip($trigger, config, event);
        });

        // Hide tooltip on mouse leave
        $(document).on('mouseleave', `.${this.options.containerClass}`, function () {
            self.scheduleHide();
        });

        // Handle tooltip hover
        this.$tooltip.on('mouseenter', function () {
            clearTimeout(self.hideTimer);
        }).on('mouseleave', function () {
            self.scheduleHide();
        });

        // Hide on click outside
        $(document).on('click', function (e) {
            if (!$(e.target).closest(`.${self.options.containerClass}, .${self.options.tooltipClass}`).length) {
                clearTimeout(self.hideTimer);
                self.$tooltip.fadeOut(self.options.fadeOutSpeed);
            }
        });
    }

    //getTooltipConfig($trigger) {
    //    // Get configuration from data attributes
    //    return {
    //        url: $trigger.data('tooltip-url'),
    //        id: $trigger.data('tooltip-id'),
    //        method: $trigger.data('tooltip-method') || 'GET',
    //        dataKey: $trigger.data('tooltip-data-key'),
    //        templateFunction: $trigger.data('tooltip-template') || 'default'
    //    };
    //}

    getTooltipConfig($trigger) {
        // Get configuration from data attributes
        const config = {
            url: $trigger.data('tooltip-url'), // Might be undefined now
            id: $trigger.data('tooltip-id'),
            method: $trigger.data('tooltip-method') || 'GET',
            dataKey: $trigger.data('tooltip-data-key'),
            templateFunction: $trigger.data('tooltip-template') || 'default',
            localData: $trigger.data('tooltip-data') // NEW: Get the local data
        };

        return config;
    }

    

    //showTooltip($trigger, config) {
    //    const offset = $trigger.offset();
    //    const $tooltip = this.$tooltip;
    //    const triggerWidth = $trigger.outerWidth();
    //    const triggerHeight = $trigger.outerHeight();
    //    const tooltipWidth = $tooltip.outerWidth();
    //    const tooltipHeight = $tooltip.outerHeight();
    //    const viewportWidth = $(window).width();
    //    const viewportHeight = $(window).height();
    //    const scrollTop = $(window).scrollTop();
    //    const scrollLeft = $(window).scrollLeft();

    //    // Default position (below and centered relative to trigger)
    //    let top = offset.top + triggerHeight + this.options.position.top;
    //    let left = offset.left + (triggerWidth / 2) + this.options.position.left;

    //    // Adjust position to prevent overflow
    //    // Check if tooltip overflows right edge
    //    if (left + tooltipWidth > scrollLeft + viewportWidth) {
    //        left = offset.left + triggerWidth - tooltipWidth + this.options.position.left;
    //    }
    //    // Check if tooltip overflows left edge
    //    if (left < scrollLeft) {
    //        left = offset.left + (triggerWidth / 2) + this.options.position.left;
    //    }
    //    // Check if tooltip overflows bottom edge
    //    if (top + tooltipHeight > scrollTop + viewportHeight) {
    //        top = offset.top - tooltipHeight - this.options.position.top;
    //    }
    //    // Check if tooltip overflows top edge
    //    if (top < scrollTop) {
    //        top = offset.top + triggerHeight + this.options.position.top;
    //    }

    //    // Apply the calculated position
    //    this.$tooltip.css({
    //        top: top,
    //        left: left
    //    });

    //    // Show loading state
    //    this.$tooltip.html('<div style="text-align: center; color: #666;">Loading...</div>')
    //        .fadeIn(this.options.fadeInSpeed);

    //    // Load content
    //    this.loadTooltipContent(config);
    //}


    showTooltip($trigger, config, event) {
        const viewportWidth = $(window).width();
        const viewportHeight = $(window).height();
        const scrollTop = $(window).scrollTop();
        const scrollLeft = $(window).scrollLeft();
        const margin = this.options.margin;
        const offsetX = this.options.position.left;
        const offsetY = this.options.position.top;

        // Get approximate tooltip dimensions before content loads
        const tooltipWidth = this.$tooltip.outerWidth() || 250; // Fallback to minWidth
        const tooltipHeight = this.$tooltip.outerHeight() || 100; // Fallback to estimated height

        // Calculate available space around cursor
        const cursorX = event.clientX;
        const cursorY = event.clientY;
        const spaceRight = viewportWidth - cursorX - scrollLeft;
        const spaceLeft = cursorX - scrollLeft;
        const spaceBelow = viewportHeight - cursorY - scrollTop;
        const spaceAbove = cursorY - scrollTop;

        let top, left;
        let position = this.options.preferredPosition;

        // Determine best position based on available space
        if (position === 'below-right' && spaceBelow >= tooltipHeight + offsetY + margin && spaceRight >= tooltipWidth + offsetX + margin) {
            // Below and to the right (default)
            top = cursorY + offsetY;
            left = cursorX + offsetX;
        } else if (spaceAbove >= tooltipHeight + offsetY + margin && spaceRight >= tooltipWidth + offsetX + margin) {
            // Above and to the right
            top = cursorY - tooltipHeight - offsetY;
            left = cursorX + offsetX;
        } else if (spaceBelow >= tooltipHeight + offsetY + margin && spaceLeft >= tooltipWidth + offsetX + margin) {
            // Below and to the left
            top = cursorY + offsetY;
            left = cursorX - tooltipWidth - offsetX;
        } else if (spaceAbove >= tooltipHeight + offsetY + margin && spaceLeft >= tooltipWidth + offsetX + margin) {
            // Above and to the left
            top = cursorY - tooltipHeight - offsetY;
            left = cursorX - tooltipWidth - offsetX;
        } else {
            // Fallback: prioritize staying within viewport with margin
            top = cursorY + offsetY;
            left = cursorX + offsetX;
            if (left + tooltipWidth > scrollLeft + viewportWidth - margin) {
                left = scrollLeft + viewportWidth - tooltipWidth - margin;
            }
            if (left < scrollLeft + margin) {
                left = scrollLeft + margin;
            }
            if (top + tooltipHeight > scrollTop + viewportHeight - margin) {
                top = scrollTop + viewportHeight - tooltipHeight - margin;
            }
            if (top < scrollTop + margin) {
                top = scrollTop + margin;
            }
        }

        // Apply position
        this.$tooltip.css({
            top: top,
            left: left
        });

        // Show loading state
        this.$tooltip.html('<div style="text-align: center; color: #666;">Loading...</div>')
            .fadeIn(this.options.fadeInSpeed);

        // Load content
        this.loadTooltipContent(config, $trigger);
    }
    

    //loadTooltipContent(config) {
    //    const self = this;

    //    if (config.localData) {
    //        const html = self.renderTooltipContent(config.localData, config.templateFunction);
    //        self.$tooltip.html(html);
    //        self.adjustTooltipPosition($trigger); // Adjust position after content load
    //        return;
    //    }

    //    const ajaxData = {};
    //    if (config.dataKey && config.id) {
    //        ajaxData[config.dataKey] = config.id;
    //    }

    //    $.ajax({
    //        url: config.url,
    //        type: config.method,
    //        data: ajaxData,
    //        dataType: 'json',
    //        success: function (data) {
    //            const html = self.renderTooltipContent(data, config.templateFunction);
    //            self.$tooltip.html(html);
    //            self.adjustTooltipPosition($trigger); // Adjust position after content load
    //        },
    //        error: function () {
    //            self.$tooltip.html('<div class="text-danger" style="color: #d32f2f;">Error loading data</div>');
    //            self.adjustTooltipPosition($trigger); // Adjust even on error
    //        }
    //    });
    //}

    loadTooltipContent(config, $trigger) {
        const self = this;

        if (config.localData) {
            const html = self.renderTooltipContent(config.localData, config.templateFunction);
            self.$tooltip.html(html);
            self.adjustTooltipPosition($trigger);
            return;
        }

        const ajaxData = {};
        if (config.dataKey && config.id) {
            ajaxData[config.dataKey] = config.id;
        }

        $.ajax({
            url: config.url,
            type: config.method,
            data: ajaxData,
            dataType: 'json',
            success: function (data) {
                const html = self.renderTooltipContent(data, config.templateFunction);
                self.$tooltip.html(html);
                self.adjustTooltipPosition($trigger);
            },
            error: function () {
                self.$tooltip.html('<div class="text-danger" style="color: #d32f2f;">Error loading data</div>');
                self.adjustTooltipPosition($trigger);
            }
        });
    }



    adjustTooltipPosition($trigger) {
        const tooltipWidth = this.$tooltip.outerWidth();
        const tooltipHeight = this.$tooltip.outerHeight();
        const viewportWidth = $(window).width();
        const viewportHeight = $(window).height();
        const scrollTop = $(window).scrollTop();
        const scrollLeft = $(window).scrollLeft();
        const margin = this.options.margin;
        const offsetX = this.options.position.left;
        const offsetY = this.options.position.top;

        // Get current cursor position (approximate from trigger or current position)
        let cursorX = parseFloat(this.$tooltip.css('left')) - offsetX;
        let cursorY = parseFloat(this.$tooltip.css('top')) - offsetY;
        if ($trigger) {
            const offset = $trigger.offset();
            cursorX = offset.left; // Approximate cursor position
            cursorY = offset.top;
        }

        // Calculate available space
        const spaceRight = viewportWidth - cursorX - scrollLeft;
        const spaceLeft = cursorX - scrollLeft;
        const spaceBelow = viewportHeight - cursorY - scrollTop;
        const spaceAbove = cursorY - scrollTop;

        let top, left;
        let position = this.options.preferredPosition;

        // Determine best position based on available space
        if (position === 'below-right' && spaceBelow >= tooltipHeight + offsetY + margin && spaceRight >= tooltipWidth + offsetX + margin) {
            top = cursorY + offsetY;
            left = cursorX + offsetX;
        } else if (spaceAbove >= tooltipHeight + offsetY + margin && spaceRight >= tooltipWidth + offsetX + margin) {
            top = cursorY - tooltipHeight - offsetY;
            left = cursorX + offsetX;
        } else if (spaceBelow >= tooltipHeight + offsetY + margin && spaceLeft >= tooltipWidth + offsetX + margin) {
            top = cursorY + offsetY;
            left = cursorX - tooltipWidth - offsetX;
        } else if (spaceAbove >= tooltipHeight + offsetY + margin && spaceLeft >= tooltipWidth + offsetX + margin) {
            top = cursorY - tooltipHeight - offsetY;
            left = cursorX - tooltipWidth - offsetX;
        } else {
            // Fallback: ensure within viewport with margin
            top = cursorY + offsetY;
            left = cursorX + offsetX;
            if (left + tooltipWidth > scrollLeft + viewportWidth - margin) {
                left = scrollLeft + viewportWidth - tooltipWidth - margin;
            }
            if (left < scrollLeft + margin) {
                left = scrollLeft + margin;
            }
            if (top + tooltipHeight > scrollTop + viewportHeight - margin) {
                top = scrollTop + viewportHeight - tooltipHeight - margin;
            }
            if (top < scrollTop + margin) {
                top = scrollTop + margin;
            }
        }

        this.$tooltip.css({ top, left });
    }


    renderTooltipContent(data, templateFunction) {
        // Use custom template functions or default
        if (typeof window[templateFunction] === 'function') {
            return window[templateFunction](data);
        }

        // Default template for leave approval steps
        if (templateFunction === 'leaveApprovalTemplate') {
            return this.renderLeaveApprovalTemplate(data);
        }

        // Generic template
        return this.renderGenericTemplate(data);
    }

    renderLeaveApprovalTemplate(data) {
        const steps = Array.isArray(data) ? data : [data];
        let html = '';

        if (steps.length > 0) {
            steps.forEach((item) => {
                const approverStep = item.approverStep ?? '';
                const statusName = item.statusName ?? '';
                const author = item.approvarPerson ?? '';
                const statusDescription = item.approvarNote ?? '';
                const approvedOrDeclineDate = item.approvedOrDeclineDate ?? '';

                html += `
                <div class="timeline-item" style="margin-bottom:1px">
                    <div class="timeline-item position-relative">
                        <div class="row g-md-3">
                            <div class="col-12 col-md-auto d-flex">
                                <div class="timeline-item-date order-1 order-md-0 me-md-4">
                                    <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                        ${approverStep} 
                                    </p>
                                </div>
                                <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                    <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                        <span class="fa-solid far fa-file-alt text-primary-dark fs-10"></span>
                                    </div>
                                    <span class="timeline-bar border-end border-dashed"></span>
                                </div>
                            </div>
                            <div class="col">
                                <div class="timeline-item-content ps-6 ps-md-3">
                                    <h5 class="fs-9 lh-sm">${statusName}</h5>
                                    <p class="fs-9 mb-0">by <a class="fw-semibold" href="#!">${author}</a></p>
                                    <h5 class="fs-9 lh-sm">${approvedOrDeclineDate}</h5>
                                    <p class="fs-9 text-body-secondary">${statusDescription}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`;
            });
        } else {
            html = '<div class="text-muted" style="color: #999;">No approval steps found</div>';
        }

        return html;
    }

    renderGenericTemplate(data) {
        if (typeof data === 'object') {
            let html = '<div class="tooltip-content">';
            for (const [key, value] of Object.entries(data)) {
                html += `<div><strong>${key}:</strong> ${value}</div>`;
            }
            html += '</div>';
            return html;
        }
        return `<div>${data}</div>`;
    }

    scheduleHide() {
        const self = this;
        this.hideTimer = setTimeout(() => {
            self.$tooltip.fadeOut(self.options.fadeOutSpeed);
        }, this.options.hideDelay);
    }

    // Public methods for manual control
    show($trigger, config) {
        this.showTooltip($trigger, config);
    }

    hide() {
        clearTimeout(this.hideTimer);
        this.$tooltip.fadeOut(this.options.fadeOutSpeed);
    }

    destroy() {
        if (this.$tooltip) {
            this.$tooltip.remove();
            this.$tooltip = null;
        }
        $(document).off('mouseenter mouseleave click', `.${this.options.containerClass}`);
    }

    // Method to register custom template functions
    static registerTemplate(name, templateFunction) {
        window[name] = templateFunction;
    }
}

// Initialize the universal tooltip service
$(document).ready(function () {
    window.tooltipService = new UniversalTooltipService();
});

// Example of registering a custom template function
UniversalTooltipService.registerTemplate('customEmployeeTemplate', function (data) {
    return `
        <div class="employee-tooltip">
            <h6>${data.name}</h6>
            <p>Department: ${data.department}</p>
            <p>Position: ${data.position}</p>
        </div>
    `;
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UniversalTooltipService;
}