/**
 * MultiSelect DD - jQuery Plugin for .NET Projects
 * Version: 1.0.0
 * Author: Custom Build
 * License: MIT
 * 
 * A flexible multi-select dropdown plugin with jQuery compatibility,
 * AJAX support, and infinite scroll pagination.
 */

(function ($) {
    'use strict';

    const defaults = {
        placeholder: 'Select item(s)',
        search: true,
        selectAll: true,
        listAll: false,
        closeOnSelect: false,
        allowClear: true,
        max: null,
        min: null,
        data: [],
        ajax: null,
        pagination: {
            enabled: false,
            pageSize: 20,
            scrollThreshold: 0.8
        },
        onChange: function () { },
        onSelect: function () { },
        onUnselect: function () { },
        onLoad: function () { },
        onMaxReached: function () { }
    };

    class MultiSelectDD {
        constructor(element, options) {
            this.$select = $(element);
            this.options = $.extend(true, {}, defaults, options);
            this.currentPage = 1;
            this.hasMore = true;
            this.isLoading = false;
            this.allData = [];
            this.searchTerm = '';

            this.init();
        }

        init() {
            // Hide original select but keep it in DOM
            this.$select.hide();

            // Load data from options or select element
            if (this.options.data.length) {
                this.allData = this.options.data.map(item => ({
                    value: item.value,
                    text: item.text,
                    selected: item.selected || false
                }));
            } else {
                this.loadFromSelect();
            }

            // Build UI
            this.buildUI();
            this.attachEvents();

            // Load AJAX data if configured
            if (this.options.ajax) {
                this.loadAjaxData();
            }

            // Store instance
            this.$select.data('multiSelectDD', this);

            // Sync initial state
            this.syncSelect();
        }

        loadFromSelect() {
            this.allData = [];
            this.$select.find('option').each((i, opt) => {
                const $opt = $(opt);
                this.allData.push({
                    value: $opt.val(),
                    text: $opt.text(),
                    selected: $opt.prop('selected')
                });
            });
        }

        buildUI() {
            const id = this.$select.attr('id') || 'multidd-' + Math.random().toString(36).substr(2, 9);

            this.$container = $('<div>', {
                class: 'multidd-container',
                id: id + '-container'
            });

            this.$header = $('<div>', {
                class: 'multidd-header',
                tabindex: 0,
                role: 'combobox',
                'aria-expanded': 'false'
            });

            this.$dropdown = $('<div>', {
                class: 'multidd-dropdown',
                role: 'listbox'
            });

            if (this.options.search) {
                this.$search = $('<input>', {
                    type: 'text',
                    class: 'multidd-search',
                    placeholder: 'Search...',
                    role: 'searchbox'
                });
                this.$dropdown.append(this.$search);
            }

            if (this.options.selectAll) {
                this.$selectAll = $('<div>', {
                    class: 'multidd-select-all',
                    role: 'option',
                    html: '<span class="multidd-checkbox"></span><span class="multidd-option-text">Select All</span>'
                });
                this.$dropdown.append(this.$selectAll);
            }

            this.$optionsContainer = $('<div>', {
                class: 'multidd-options-list'
            });
            this.$dropdown.append(this.$optionsContainer);

            this.renderOptions();
            this.updateHeader();

            this.$container.append(this.$header, this.$dropdown);
            this.$select.after(this.$container);

            // Handle disabled state
            if (this.$select.prop('disabled')) {
                this.disable();
            }
        }

        renderOptions(append = false) {
            if (!append) {
                this.$optionsContainer.empty();
            }

            const dataToRender = this.options.pagination.enabled
                ? this.allData.slice(0, this.currentPage * this.options.pagination.pageSize)
                : this.allData;

            dataToRender.forEach(item => {
                const exists = this.$optionsContainer.find(`[data-value="${item.value}"]`).length;
                if (append && exists) return;

                const $option = $('<div>', {
                    class: 'multidd-option' + (item.selected ? ' multidd-selected' : ''),
                    'data-value': item.value,
                    role: 'option',
                    'aria-selected': item.selected,
                    html: `<span class="multidd-checkbox"></span><span class="multidd-option-text">${item.text}</span>`
                });

                this.$optionsContainer.append($option);
            });
        }

        attachEvents() {
            const self = this;

            // Toggle dropdown
            this.$header.on('click', function (e) {
                // Don't toggle if clicking close buttons
                if ($(e.target).hasClass('multidd-item-close') || $(e.target).hasClass('multidd-clear-all')) {
                    return;
                }
                e.stopPropagation();
                self.toggleDropdown();
            });

            // Remove individual item
            this.$container.on('click', '.multidd-item-close', function (e) {
                e.stopPropagation();
                const value = $(this).closest('.multidd-selected-item').data('value');
                self.unselectItem(value);
            });

            // Clear all button
            this.$container.on('click', '.multidd-clear-all', function (e) {
                e.stopPropagation();
                self.clear();
            });

            // Keyboard navigation for header
            this.$header.on('keydown', function (e) {
                if (['Enter', ' ', 'ArrowDown'].includes(e.key)) {
                    e.preventDefault();
                    self.openDropdown();
                    if (self.$search) {
                        self.$search.focus();
                    }
                }
            });

            // Option click
            this.$container.on('click', '.multidd-option', function (e) {
                e.stopPropagation();
                const value = $(this).data('value');
                self.toggleOption(value, $(this));
            });

            // Select all
            if (this.options.selectAll) {
                this.$selectAll.on('click', function (e) {
                    e.stopPropagation();
                    self.toggleSelectAll();
                });
            }

            // Search
            if (this.options.search) {
                this.$search.on('input', function () {
                    self.searchTerm = $(this).val();

                    // If AJAX is enabled, trigger server-side search
                    if (self.options.ajax && self.options.ajax.search !== false) {
                        clearTimeout(self.searchTimeout);
                        self.searchTimeout = setTimeout(function () {
                            self.currentPage = 1;
                            self.allData = [];
                            self.loadAjaxData();
                        }, 300);
                    } else {
                        // Client-side search
                        self.filterOptions(self.searchTerm);
                    }
                });

                this.$search.on('keydown', function (e) {
                    if (e.key === 'Escape') {
                        self.closeDropdown();
                        self.$header.focus();
                    }
                });
            }

            // Pagination scroll
            if (this.options.pagination.enabled) {
                this.$dropdown.on('scroll', function () {
                    const scrollTop = $(this).scrollTop();
                    const scrollHeight = $(this)[0].scrollHeight;
                    const clientHeight = $(this).height();

                    if (scrollHeight === clientHeight) return;

                    const scrollPercent = scrollTop / (scrollHeight - clientHeight);

                    if (scrollPercent >= self.options.pagination.scrollThreshold && !self.isLoading && self.hasMore) {
                        self.loadMoreData();
                    }
                });
            }

            // Close on outside click
            $(document).on('click.multidd-' + this.$select.attr('id'), function (e) {
                if (!self.$container.is(e.target) && self.$container.has(e.target).length === 0) {
                    self.closeDropdown();
                }
            });

            // Close on escape
            $(document).on('keydown.multidd-' + this.$select.attr('id'), function (e) {
                if (e.key === 'Escape' && self.$header.hasClass('multidd-active')) {
                    self.closeDropdown();
                    self.$header.focus();
                }
            });
        }

        toggleDropdown() {
            if (this.$container.hasClass('multidd-disabled')) return;

            if (this.$header.hasClass('multidd-active')) {
                this.closeDropdown();
            } else {
                this.openDropdown();
            }
        }

        openDropdown() {
            if (this.$container.hasClass('multidd-disabled')) return;
            this.$header.addClass('multidd-active');
            this.$header.attr('aria-expanded', 'true');
        }

        closeDropdown() {
            this.$header.removeClass('multidd-active');
            this.$header.attr('aria-expanded', 'false');
            if (this.$search) {
                this.$search.val('');
                this.filterOptions('');
            }
        }

        toggleOption(value, $option) {
            const item = this.allData.find(d => d.value == value);
            if (!item) return;

            const isSelected = item.selected;

            // Check max limit
            if (!isSelected && this.options.max && this.getSelectedValues().length >= this.options.max) {
                this.options.onMaxReached(this.options.max);
                return;
            }

            item.selected = !isSelected;
            $option.toggleClass('multidd-selected');
            $option.attr('aria-selected', item.selected);

            this.syncSelect();
            this.updateHeader();
            this.validateMin();

            const text = item.text;
            this.options.onChange(value, text, item);

            if (item.selected) {
                this.options.onSelect(value, text, item);
            } else {
                this.options.onUnselect(value, text, item);
            }

            if (this.options.closeOnSelect) {
                this.closeDropdown();
            }

            // Trigger jQuery change event
            this.$select.trigger('change');
        }

        toggleSelectAll() {
            const allSelected = this.$selectAll.hasClass('multidd-selected');
            const visibleOptions = this.$optionsContainer.find('.multidd-option:visible');

            visibleOptions.each((i, opt) => {
                const value = $(opt).data('value');
                const item = this.allData.find(d => d.value == value);
                if (item && item.selected === allSelected) {
                    this.toggleOption(value, $(opt));
                }
            });

            this.$selectAll.toggleClass('multidd-selected');
        }

        filterOptions(searchTerm) {
            const term = searchTerm.toLowerCase();
            this.$optionsContainer.find('.multidd-option').each(function () {
                const text = $(this).find('.multidd-option-text').text().toLowerCase();
                $(this).toggle(text.includes(term));
            });
        }

        updateHeader() {
            this.$header.empty();
            const selected = this.getSelectedItems();

            if (selected.length === 0) {
                this.$header.append(`<span class="multidd-placeholder">${this.options.placeholder}</span>`);
            } else if (this.options.listAll) {
                // Show all selected items
                selected.forEach(item => {
                    const $item = $('<span>', {
                        class: 'multidd-selected-item',
                        'data-value': item.value
                    });

                    $item.append(`<span class="multidd-item-text">${item.text}</span>`);

                    if (this.options.allowClear) {
                        const $close = $('<span>', {
                            class: 'multidd-item-close',
                            html: '&times;'
                        });
                        $item.append($close);
                    }

                    this.$header.append($item);
                });
            } else {
                // Show first 2 items, then count
                if (selected.length <= 2) {
                    selected.forEach(item => {
                        const $item = $('<span>', {
                            class: 'multidd-selected-item',
                            'data-value': item.value
                        });

                        $item.append(`<span class="multidd-item-text">${item.text}</span>`);

                        if (this.options.allowClear) {
                            const $close = $('<span>', {
                                class: 'multidd-item-close',
                                html: '&times;'
                            });
                            $item.append($close);
                        }

                        this.$header.append($item);
                    });
                } else {
                    // Show first 2 items
                    selected.slice(0, 2).forEach(item => {
                        const $item = $('<span>', {
                            class: 'multidd-selected-item',
                            'data-value': item.value
                        });

                        $item.append(`<span class="multidd-item-text">${item.text}</span>`);

                        if (this.options.allowClear) {
                            const $close = $('<span>', {
                                class: 'multidd-item-close',
                                html: '&times;'
                            });
                            $item.append($close);
                        }

                        this.$header.append($item);
                    });
                    // Show count for remaining
                    this.$header.append(`<span class="multidd-count">+${selected.length - 2} more</span>`);
                }
            }

            // Add clear all button if multiple items selected and allowClear is true
            if (this.options.allowClear && selected.length > 0) {
                const $clearAll = $('<span>', {
                    class: 'multidd-clear-all',
                    html: '&times;',
                    title: 'Clear all'
                });
                this.$header.append($clearAll);
            }

            if (this.options.max) {
                const maxIndicator = $('<span>', {
                    class: 'multidd-max-indicator',
                    text: `${selected.length}/${this.options.max}`
                });
                this.$header.append(maxIndicator);
            }
        }

        syncSelect() {
            const values = this.getSelectedValues();

            // Clear all selected
            this.$select.find('option').prop('selected', false);

            // Set selected values
            values.forEach(val => {
                this.$select.find(`option[value="${val}"]`).prop('selected', true);
            });

            // Update select element value
            this.$select.val(values);
        }

        validateMin() {
            if (this.options.min && this.getSelectedValues().length < this.options.min) {
                this.$container.addClass('multidd-invalid');
            } else {
                this.$container.removeClass('multidd-invalid');
            }
        }

        getSelectedValues() {
            return this.allData.filter(d => d.selected).map(d => d.value);
        }

        getSelectedItems() {
            return this.allData.filter(d => d.selected);
        }

        unselectItem(value) {
            const item = this.allData.find(d => d.value == value);
            if (item && item.selected) {
                item.selected = false;

                // Update UI option
                const $option = this.$optionsContainer.find(`[data-value="${value}"]`);
                $option.removeClass('multidd-selected');
                $option.attr('aria-selected', false);

                this.syncSelect();
                this.updateHeader();
                this.validateMin();

                this.options.onChange(value, item.text, item);
                this.options.onUnselect(value, item.text, item);

                this.$select.trigger('change');
            }
        }

        // Public API methods
        val(values) {
            if (values === undefined) {
                return this.getSelectedValues();
            }

            // Ensure values is an array
            if (!Array.isArray(values)) {
                values = [values];
            }

            this.allData.forEach(item => {
                item.selected = values.includes(item.value);
            });

            this.renderOptions();
            this.syncSelect();
            this.updateHeader();
            this.validateMin();
            this.$select.trigger('change');
        }

        disable() {
            this.$container.addClass('multidd-disabled');
            this.$select.prop('disabled', true);
            if (this.$search) {
                this.$search.prop('disabled', true);
            }
        }

        enable() {
            this.$container.removeClass('multidd-disabled');
            this.$select.prop('disabled', false);
            if (this.$search) {
                this.$search.prop('disabled', false);
            }
        }

        destroy() {
            $(document).off('click.multidd-' + this.$select.attr('id'));
            $(document).off('keydown.multidd-' + this.$select.attr('id'));
            this.$container.remove();
            this.$select.show().removeData('multiSelectDD');
        }

        refresh() {
            this.loadFromSelect();
            this.$optionsContainer.empty();
            this.renderOptions();
            this.updateHeader();
            this.validateMin();
        }

        clear() {
            this.val([]);
        }

        selectAll() {
            const allValues = this.allData.map(d => d.value);
            this.val(allValues);
        }

        addOption(value, text, selected = false) {
            // Add to plugin data
            this.allData.push({ value, text, selected });

            // Add to original select
            this.$select.append($('<option>', {
                value: value,
                text: text,
                selected: selected
            }));

            this.refresh();
        }

        removeOption(value) {
            // Remove from plugin data
            this.allData = this.allData.filter(item => item.value !== value);

            // Remove from original select
            this.$select.find(`option[value="${value}"]`).remove();

            this.refresh();
        }

        loadAjaxData(url = null) {
            const self = this;
            const ajaxOptions = typeof this.options.ajax === 'object' ? this.options.ajax : { url: this.options.ajax };
            const ajaxUrl = url || ajaxOptions.url;

            if (!ajaxUrl) return;

            this.isLoading = true;

            // Show loading indicator
            if (!this.$dropdown.find('.multidd-loading').length) {
                this.$dropdown.append('<div class="multidd-loading">Loading...</div>');
            }

            const ajaxData = {
                page: this.currentPage,
                pageSize: this.options.pagination.pageSize
            };

            // Add search term if exists
            if (this.searchTerm) {
                ajaxData.search = this.searchTerm;
            }

            $.ajax({
                url: ajaxUrl,
                method: ajaxOptions.method || 'GET',
                dataType: ajaxOptions.dataType || 'json',
                data: $.extend({}, ajaxOptions.data, ajaxData),
                success: function (response) {
                    // Handle different response formats
                    let data = response.items || response.data || response;
                    const hasMore = response.hasMore !== undefined ? response.hasMore : true;

                    if (Array.isArray(data)) {
                        // Append or replace data
                        if (self.currentPage === 1) {
                            self.allData = data.map(item => ({
                                value: item.value,
                                text: item.text,
                                selected: item.selected || false
                            }));
                            self.$optionsContainer.empty();
                        } else {
                            data.forEach(item => {
                                self.allData.push({
                                    value: item.value,
                                    text: item.text,
                                    selected: item.selected || false
                                });
                            });
                        }

                        self.renderOptions(self.currentPage > 1);
                        self.updateHeader();
                        self.options.onLoad(data);

                        // Update hasMore
                        self.hasMore = hasMore && data.length === self.options.pagination.pageSize;
                    }
                },
                error: function (xhr, status, error) {
                    console.error('MultiSelectDD AJAX Error:', error);
                },
                complete: function () {
                    self.isLoading = false;
                    self.$dropdown.find('.multidd-loading').remove();
                }
            });
        }

        loadMoreData() {
            if (this.options.ajax && !this.isLoading && this.hasMore) {
                this.currentPage++;
                this.loadAjaxData();
            }
        }

        reload() {
            this.currentPage = 1;
            this.allData = [];
            this.hasMore = true;
            this.loadAjaxData();
        }
    }

    // jQuery plugin wrapper
    $.fn.multiSelectDD = function (options) {
        const args = Array.prototype.slice.call(arguments, 1);

        return this.each(function () {
            const $this = $(this);
            let instance = $this.data('multiSelectDD');

            if (!instance && typeof options !== 'string') {
                instance = new MultiSelectDD(this, typeof options === 'object' ? options : {});
            } else if (instance && typeof options === 'string') {
                // Call public method
                if (typeof instance[options] === 'function') {
                    const result = instance[options].apply(instance, args);
                    // Return result for getter methods
                    if (options === 'val' && args.length === 0) {
                        return result;
                    }
                }
            }
        });
    };

    // Auto-initialize elements with .multiDD class
    $(document).ready(function () {
        $('.multiDD').each(function () {
            if (!$(this).data('multiSelectDD')) {
                $(this).multiSelectDD();
            }
        });
    });

})(jQuery);