define(['baseView', 'loading', 'emby-input', 'emby-button', 'emby-select', 'emby-checkbox', 'emby-scroller'], function (BaseView, loading) {
    'use strict';

    var pluginId = 'e2688ed7-4bb2-4bba-9e0d-6ccfd1f2d8f3';
    var presets = {
        japan: {
            description: 'Map common international ratings into G, PG12, R15+, R18+, and NR.',
            mappings: [
                { OriginalRating: '6', TargetRating: 'G' },
                { OriginalRating: '6+', TargetRating: 'G' },
                { OriginalRating: 'G', TargetRating: 'G' },
                { OriginalRating: 'PG', TargetRating: 'PG12' },
                { OriginalRating: 'PG-13', TargetRating: 'PG12' },
                { OriginalRating: '14', TargetRating: 'PG12' },
                { OriginalRating: 'TV-14', TargetRating: 'PG12' },
                { OriginalRating: 'R', TargetRating: 'R15+' },
                { OriginalRating: '15', TargetRating: 'R15+' },
                { OriginalRating: '16', TargetRating: 'R15+' },
                { OriginalRating: 'M', TargetRating: 'R15+' },
                { OriginalRating: 'MA15+', TargetRating: 'R15+' },
                { OriginalRating: 'NC-17', TargetRating: 'R18+' },
                { OriginalRating: 'PL-18', TargetRating: 'R18+' },
                { OriginalRating: 'Unrated', TargetRating: 'NR' },
                { OriginalRating: 'Not Rated', TargetRating: 'NR' },
                { OriginalRating: 'NR', TargetRating: 'NR' }
            ]
        },
        unitedStates: {
            description: 'Standardize ratings into G, PG, PG-13, R, NC-17, and NR.',
            mappings: [
                { OriginalRating: 'G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: 'PG' },
                { OriginalRating: '6+', TargetRating: 'PG' },
                { OriginalRating: 'PG', TargetRating: 'PG' },
                { OriginalRating: '12', TargetRating: 'PG-13' },
                { OriginalRating: '12A', TargetRating: 'PG-13' },
                { OriginalRating: 'PG-13', TargetRating: 'PG-13' },
                { OriginalRating: '14', TargetRating: 'PG-13' },
                { OriginalRating: 'TV-14', TargetRating: 'PG-13' },
                { OriginalRating: 'R', TargetRating: 'R' },
                { OriginalRating: '15', TargetRating: 'R' },
                { OriginalRating: '16', TargetRating: 'R' },
                { OriginalRating: 'M', TargetRating: 'R' },
                { OriginalRating: 'MA15+', TargetRating: 'R' },
                { OriginalRating: '18', TargetRating: 'NC-17' },
                { OriginalRating: 'NC-17', TargetRating: 'NC-17' },
                { OriginalRating: 'PL-18', TargetRating: 'NC-17' },
                { OriginalRating: 'Unrated', TargetRating: 'NR' },
                { OriginalRating: 'Not Rated', TargetRating: 'NR' },
                { OriginalRating: 'NR', TargetRating: 'NR' }
            ]
        },
        europe: {
            description: 'Normalize ratings into G, 6, 12, 16, 18, and NR using a simple Europe-friendly age scale.',
            mappings: [
                { OriginalRating: 'G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: '6' },
                { OriginalRating: '6+', TargetRating: '6' },
                { OriginalRating: 'PG', TargetRating: '6' },
                { OriginalRating: '12', TargetRating: '12' },
                { OriginalRating: '12A', TargetRating: '12' },
                { OriginalRating: 'PG-13', TargetRating: '12' },
                { OriginalRating: '14', TargetRating: '12' },
                { OriginalRating: 'TV-14', TargetRating: '12' },
                { OriginalRating: '15', TargetRating: '16' },
                { OriginalRating: '16', TargetRating: '16' },
                { OriginalRating: 'M', TargetRating: '16' },
                { OriginalRating: 'R', TargetRating: '16' },
                { OriginalRating: 'MA15+', TargetRating: '16' },
                { OriginalRating: '18', TargetRating: '18' },
                { OriginalRating: 'NC-17', TargetRating: '18' },
                { OriginalRating: 'PL-18', TargetRating: '18' },
                { OriginalRating: 'Unrated', TargetRating: 'NR' },
                { OriginalRating: 'Not Rated', TargetRating: 'NR' },
                { OriginalRating: 'NR', TargetRating: 'NR' }
            ]
        },
        australia: {
            description: 'Map ratings into G, PG, M, MA15+, R18+, and NR for an Australian-style scale.',
            mappings: [
                { OriginalRating: 'G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: 'PG' },
                { OriginalRating: '6+', TargetRating: 'PG' },
                { OriginalRating: 'PG', TargetRating: 'PG' },
                { OriginalRating: '12', TargetRating: 'M' },
                { OriginalRating: '12A', TargetRating: 'M' },
                { OriginalRating: 'PG-13', TargetRating: 'M' },
                { OriginalRating: '14', TargetRating: 'M' },
                { OriginalRating: 'TV-14', TargetRating: 'M' },
                { OriginalRating: 'M', TargetRating: 'M' },
                { OriginalRating: '15', TargetRating: 'MA15+' },
                { OriginalRating: '16', TargetRating: 'MA15+' },
                { OriginalRating: 'R', TargetRating: 'MA15+' },
                { OriginalRating: 'MA15+', TargetRating: 'MA15+' },
                { OriginalRating: '18', TargetRating: 'R18+' },
                { OriginalRating: 'NC-17', TargetRating: 'R18+' },
                { OriginalRating: 'PL-18', TargetRating: 'R18+' },
                { OriginalRating: 'Unrated', TargetRating: 'NR' },
                { OriginalRating: 'Not Rated', TargetRating: 'NR' },
                { OriginalRating: 'NR', TargetRating: 'NR' }
            ]
        },
        blank: {
            description: 'Start with an empty table and define your own rating conversions.',
            mappings: []
        }
    };

    function cloneMappings(mappings) {
        return (mappings || []).map(function (mapping) {
            return {
                OriginalRating: mapping.OriginalRating,
                TargetRating: mapping.TargetRating
            };
        });
    }

    function setError(page, message) {
        var errorElement = page.querySelector('.ratingStandardizerError');

        if (!message) {
            errorElement.textContent = '';
            errorElement.style.display = 'none';
            return;
        }

        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }

    function setStatus(page, message) {
        var statusElement = page.querySelector('.ratingStandardizerStatus');

        if (!message) {
            statusElement.textContent = '';
            statusElement.style.display = 'none';
            return;
        }

        statusElement.textContent = message;
        statusElement.style.display = 'block';
    }

    function updatePresetDescription(page) {
        var presetKey = page.querySelector('.selectPreset').value;
        var preset = presets[presetKey] || presets.japan;
        page.querySelector('.ratingStandardizerPresetDescription').textContent = preset.description;
    }

    function createInput(className, placeholder, value) {
        var input = document.createElement('input');
        input.setAttribute('is', 'emby-input');
        input.className = className;
        input.type = 'text';
        input.placeholder = placeholder;
        input.value = value || '';
        input.spellcheck = false;
        return input;
    }

    function createMappingRow(mapping) {
        var row = document.createElement('tr');
        var originalCell = document.createElement('td');
        var targetCell = document.createElement('td');
        var deleteCell = document.createElement('td');
        var deleteButton = document.createElement('button');

        originalCell.appendChild(createInput('txtOriginalRating ratingStandardizerOriginalInput', 'TV-14', mapping && mapping.OriginalRating));
        targetCell.appendChild(createInput('txtTargetRating ratingStandardizerTargetInput', 'PG12', mapping && mapping.TargetRating));

        deleteCell.className = 'ratingStandardizerDeleteCell';
        deleteButton.setAttribute('is', 'emby-button');
        deleteButton.type = 'button';
        deleteButton.className = 'button-flat ratingStandardizerDeleteButton';
        deleteButton.innerHTML = '<span>Delete</span>';
        deleteButton.addEventListener('click', function () {
            row.remove();
        });
        deleteCell.appendChild(deleteButton);

        row.appendChild(originalCell);
        row.appendChild(targetCell);
        row.appendChild(deleteCell);

        return row;
    }

    function renderMappings(page, mappings) {
        var tbody = page.querySelector('.mappingRows');

        tbody.innerHTML = '';
        if (!mappings || !mappings.length) {
            tbody.appendChild(createMappingRow({ OriginalRating: '', TargetRating: '' }));
            return;
        }

        mappings.forEach(function (mapping) {
            tbody.appendChild(createMappingRow(mapping));
        });
    }

    function collectMappings(page) {
        return Array.prototype.slice.call(page.querySelectorAll('.mappingRows tr'))
            .map(function (row) {
                return {
                    OriginalRating: row.querySelector('.txtOriginalRating').value.trim(),
                    TargetRating: row.querySelector('.txtTargetRating').value.trim()
                };
            })
            .filter(function (mapping) {
                return mapping.OriginalRating && mapping.TargetRating;
            });
    }

    function loadPage(page, config) {
        page.querySelector('.chkIsEnabled').checked = !!config.IsEnabled;
        renderMappings(page, config.Mappings || []);
        updatePresetDescription(page);
        setError(page, '');
        setStatus(page, '');
        loading.hide();
    }

    function applySelectedPreset(page) {
        var presetKey = page.querySelector('.selectPreset').value;
        var preset = presets[presetKey] || presets.japan;

        setError(page, '');
        setStatus(page, '');
        renderMappings(page, cloneMappings(preset.mappings));
        updatePresetDescription(page);
    }

    function runNow(page) {
        loading.show();
        setError(page, '');
        setStatus(page, '');

        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('Plugins/RatingStandardizer/RunNow'),
            dataType: 'json'
        }).then(function (result) {
            if (result && result.SkippedBecauseDisabled) {
                setStatus(page, 'Plugin is disabled. Enable it and save before running.');
                return;
            }

            setStatus(
                page,
                'Completed. Scanned ' + (result ? result.ScannedCount : 0) + ' items, matched ' + (result ? result.MatchedCount : 0) + ', updated ' + (result ? result.UpdatedCount : 0) + '.'
            );
        }).catch(function (error) {
            console.error('Failed to run Rating Standardizer.', error);
            setError(page, 'Failed to run now. Check browser console and server logs.');
        }).then(function () {
            loading.hide();
        });
    }

    function onSubmit(e) {
        e.preventDefault();

        var page = this.closest('.ratingStandardizerPage');
        loading.show();
        setError(page, '');
        setStatus(page, '');

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            config.IsEnabled = page.querySelector('.chkIsEnabled').checked;
            config.Mappings = collectMappings(page);

            return ApiClient.updatePluginConfiguration(pluginId, config);
        }).then(function (result) {
            Dashboard.processPluginConfigurationUpdateResult(result);
            loading.hide();
        }).catch(function (error) {
            console.error('Failed to save Rating Standardizer configuration.', error);
            setError(page, 'Failed to save plugin configuration. Check browser console and server logs.');
            loading.hide();
        });

        return false;
    }

    function View(view) {
        BaseView.apply(this, arguments);

        var form = view.querySelector('.ratingStandardizerForm');
        form.addEventListener('submit', onSubmit);

        view.querySelector('.btnAddMapping').addEventListener('click', function () {
            view.querySelector('.mappingRows').appendChild(createMappingRow({ OriginalRating: '', TargetRating: '' }));
        });

        view.querySelector('.btnApplyPreset').addEventListener('click', function () {
            applySelectedPreset(view);
        });

        view.querySelector('.btnRunNow').addEventListener('click', function () {
            runNow(view);
        });

        view.querySelector('.selectPreset').addEventListener('change', function () {
            updatePresetDescription(view);
        });
    }

    Object.assign(View.prototype, BaseView.prototype);

    View.prototype.onResume = function () {
        BaseView.prototype.onResume.apply(this, arguments);

        var page = this.view;
        loading.show();

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            loadPage(page, config);
        }).catch(function (error) {
            console.error('Failed to load Rating Standardizer configuration.', error);
            setError(page, 'Failed to load plugin configuration. Check browser console and server logs.');
            loading.hide();
        });
    };

    return View;
});
