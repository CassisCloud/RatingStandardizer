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
                { OriginalRating: 'JP-0', TargetRating: 'G' },
                { OriginalRating: 'JP-G', TargetRating: 'G' },
                { OriginalRating: 'PG', TargetRating: 'PG12' },
                { OriginalRating: 'TV-PG', TargetRating: 'PG12' },
                { OriginalRating: 'JP-12', TargetRating: 'PG12' },
                { OriginalRating: 'JP-PG', TargetRating: 'PG12' },
                { OriginalRating: 'JP-PG-13', TargetRating: 'PG12' },
                { OriginalRating: 'JP-PG12', TargetRating: 'PG12' },
                { OriginalRating: '12+', TargetRating: 'PG12' },
                { OriginalRating: 'PG-13', TargetRating: 'PG12' },
                { OriginalRating: '14', TargetRating: 'PG12' },
                { OriginalRating: 'TV-14', TargetRating: 'PG12' },
                { OriginalRating: 'R', TargetRating: 'R15+' },
                { OriginalRating: 'JP-R15+', TargetRating: 'R15+' },
                { OriginalRating: '15', TargetRating: 'R15+' },
                { OriginalRating: '16', TargetRating: 'R15+' },
                { OriginalRating: 'M', TargetRating: 'R15+' },
                { OriginalRating: 'MA15+', TargetRating: 'R15+' },
                { OriginalRating: 'JP-R - 17+', TargetRating: 'R18+' },
                { OriginalRating: 'JP-R-17+', TargetRating: 'R18+' },
                { OriginalRating: 'JP-R18+', TargetRating: 'R18+' },
                { OriginalRating: 'TV-MA', TargetRating: 'R18+' },
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
                { OriginalRating: 'JP-0', TargetRating: 'G' },
                { OriginalRating: 'JP-G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: 'PG' },
                { OriginalRating: '6+', TargetRating: 'PG' },
                { OriginalRating: 'PG', TargetRating: 'PG' },
                { OriginalRating: '12', TargetRating: 'PG-13' },
                { OriginalRating: 'TV-PG', TargetRating: 'PG-13' },
                { OriginalRating: 'JP-12', TargetRating: 'PG-13' },
                { OriginalRating: 'JP-PG', TargetRating: 'PG-13' },
                { OriginalRating: 'JP-PG-13', TargetRating: 'PG-13' },
                { OriginalRating: 'JP-PG12', TargetRating: 'PG-13' },
                { OriginalRating: '12+', TargetRating: 'PG-13' },
                { OriginalRating: '12A', TargetRating: 'PG-13' },
                { OriginalRating: 'PG-13', TargetRating: 'PG-13' },
                { OriginalRating: '14', TargetRating: 'PG-13' },
                { OriginalRating: 'TV-14', TargetRating: 'PG-13' },
                { OriginalRating: 'R', TargetRating: 'R' },
                { OriginalRating: 'JP-R15+', TargetRating: 'R' },
                { OriginalRating: '15', TargetRating: 'R' },
                { OriginalRating: '16', TargetRating: 'R' },
                { OriginalRating: 'M', TargetRating: 'R' },
                { OriginalRating: 'MA15+', TargetRating: 'R' },
                { OriginalRating: 'JP-R - 17+', TargetRating: 'NC-17' },
                { OriginalRating: 'JP-R-17+', TargetRating: 'NC-17' },
                { OriginalRating: 'JP-R18+', TargetRating: 'NC-17' },
                { OriginalRating: 'TV-MA', TargetRating: 'NC-17' },
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
                { OriginalRating: 'JP-0', TargetRating: 'G' },
                { OriginalRating: 'JP-G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: '6' },
                { OriginalRating: '6+', TargetRating: '6' },
                { OriginalRating: 'PG', TargetRating: '6' },
                { OriginalRating: '12', TargetRating: '12' },
                { OriginalRating: 'TV-PG', TargetRating: '12' },
                { OriginalRating: 'JP-12', TargetRating: '12' },
                { OriginalRating: 'JP-PG', TargetRating: '12' },
                { OriginalRating: 'JP-PG-13', TargetRating: '12' },
                { OriginalRating: 'JP-PG12', TargetRating: '12' },
                { OriginalRating: '12+', TargetRating: '12' },
                { OriginalRating: '12A', TargetRating: '12' },
                { OriginalRating: 'PG-13', TargetRating: '12' },
                { OriginalRating: '14', TargetRating: '12' },
                { OriginalRating: 'TV-14', TargetRating: '12' },
                { OriginalRating: 'JP-R15+', TargetRating: '16' },
                { OriginalRating: '15', TargetRating: '16' },
                { OriginalRating: '16', TargetRating: '16' },
                { OriginalRating: 'M', TargetRating: '16' },
                { OriginalRating: 'R', TargetRating: '16' },
                { OriginalRating: 'MA15+', TargetRating: '16' },
                { OriginalRating: 'JP-R - 17+', TargetRating: '18' },
                { OriginalRating: 'JP-R-17+', TargetRating: '18' },
                { OriginalRating: 'JP-R18+', TargetRating: '18' },
                { OriginalRating: 'TV-MA', TargetRating: '18' },
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
                { OriginalRating: 'JP-0', TargetRating: 'G' },
                { OriginalRating: 'JP-G', TargetRating: 'G' },
                { OriginalRating: '6', TargetRating: 'PG' },
                { OriginalRating: '6+', TargetRating: 'PG' },
                { OriginalRating: 'PG', TargetRating: 'PG' },
                { OriginalRating: '12', TargetRating: 'M' },
                { OriginalRating: 'TV-PG', TargetRating: 'M' },
                { OriginalRating: 'JP-12', TargetRating: 'M' },
                { OriginalRating: 'JP-PG', TargetRating: 'M' },
                { OriginalRating: 'JP-PG-13', TargetRating: 'M' },
                { OriginalRating: 'JP-PG12', TargetRating: 'M' },
                { OriginalRating: '12+', TargetRating: 'M' },
                { OriginalRating: '12A', TargetRating: 'M' },
                { OriginalRating: 'PG-13', TargetRating: 'M' },
                { OriginalRating: '14', TargetRating: 'M' },
                { OriginalRating: 'TV-14', TargetRating: 'M' },
                { OriginalRating: 'M', TargetRating: 'M' },
                { OriginalRating: 'JP-R15+', TargetRating: 'MA15+' },
                { OriginalRating: '15', TargetRating: 'MA15+' },
                { OriginalRating: '16', TargetRating: 'MA15+' },
                { OriginalRating: 'R', TargetRating: 'MA15+' },
                { OriginalRating: 'MA15+', TargetRating: 'MA15+' },
                { OriginalRating: 'JP-R - 17+', TargetRating: 'R18+' },
                { OriginalRating: 'JP-R-17+', TargetRating: 'R18+' },
                { OriginalRating: 'JP-R18+', TargetRating: 'R18+' },
                { OriginalRating: 'TV-MA', TargetRating: 'R18+' },
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

    function mappingsEqual(left, right) {
        var leftMappings = left || [];
        var rightMappings = right || [];

        if (leftMappings.length !== rightMappings.length) {
            return false;
        }

        for (var i = 0; i < leftMappings.length; i++) {
            if (leftMappings[i].OriginalRating !== rightMappings[i].OriginalRating || leftMappings[i].TargetRating !== rightMappings[i].TargetRating) {
                return false;
            }
        }

        return true;
    }

    function inferPresetKey(mappings) {
        var presetKeys = Object.keys(presets).filter(function (key) {
            return key !== 'blank';
        });

        for (var i = 0; i < presetKeys.length; i++) {
            var presetKey = presetKeys[i];
            if (mappingsEqual(mappings, presets[presetKey].mappings)) {
                return presetKey;
            }
        }

        return 'blank';
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

    function updateMappingsEditorVisibility(page) {
        var isCustomPreset = page.querySelector('.selectPreset').value === 'blank';
        page.querySelector('.ratingStandardizerMappingsEditor').hidden = !isCustomPreset;
        page.querySelector('.mappingEditorHint').hidden = isCustomPreset;
        page.querySelector('.btnRestoreDefaults').hidden = !isCustomPreset;
    }

    function setLibraryStatus(page, message) {
        var statusElement = page.querySelector('.libraryLoadStatus');

        if (!message) {
            statusElement.textContent = '';
            statusElement.style.display = 'none';
            return;
        }

        statusElement.textContent = message;
        statusElement.style.display = 'block';
    }

    function escapeHtml(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function normalizeId(value) {
        return String(value || '').trim().toLowerCase().replace(/-/g, '');
    }

    function getLibraryId(folder) {
        if (!folder) {
            return '';
        }

        return String(folder.ItemId || folder.Id || '').trim();
    }

    function setLibraryListEnabled(page, enabled) {
        var checkboxes = page.querySelectorAll('.library-check');
        for (var i = 0; i < checkboxes.length; i++) {
            checkboxes[i].disabled = !enabled;
        }

        page.querySelector('.libraryListContainer').classList.toggle('is-disabled', !enabled);
    }

    function updateLibrarySelectionVisibility(page) {
        var isLimitedToSelection = page.querySelector('.chkAllLibraries').checked;
        page.querySelector('.libraryListContainer').hidden = !isLimitedToSelection;
        page.querySelector('.librarySelectionHint').hidden = isLimitedToSelection;
        setLibraryListEnabled(page, isLimitedToSelection);
    }

    function restoreCustomDefaults(page) {
        renderMappings(page, []);
        setError(page, '');
        setStatus(page, 'Custom mappings restored to defaults.');
    }

    function syncLibrarySelectionState(page) {
        var items = page.querySelectorAll('.ratingStandardizerLibraryItem');
        for (var i = 0; i < items.length; i++) {
            var checkbox = items[i].querySelector('.library-check');
            items[i].classList.toggle('is-selected', !!(checkbox && checkbox.checked));
        }
    }

    function renderLibraries(page, folders, selectedIds) {
        var container = page.querySelector('.libraryListContainer');
        if (!folders || !folders.length) {
            container.innerHTML = '<div class="fieldDescription">No libraries were found.</div>';
            return;
        }

        var selectedLookup = {};
        (selectedIds || []).forEach(function (id) {
            var normalized = normalizeId(id);
            if (normalized) {
                selectedLookup[normalized] = true;
            }
        });

        var html = '';
        folders.forEach(function (folder) {
            var id = getLibraryId(folder);
            var normalizedId = normalizeId(id);
            if (!normalizedId) {
                return;
            }

            var name = folder.Name || 'Unnamed library';
            var checked = selectedLookup[normalizedId] ? ' checked' : '';
            html += '<label class="ratingStandardizerLibraryItem">';
            html += '<input type="checkbox" class="library-check" value="' + escapeHtml(id) + '"' + checked + ' />';
            html += '<span class="ratingStandardizerLibraryContent">';
            html += '<span class="ratingStandardizerLibraryCheck"></span>';
            html += '<span class="ratingStandardizerLibraryLabel">';
            html += '<span class="ratingStandardizerLibraryName">' + escapeHtml(name) + '</span>';
            html += '</span>';
            html += '</span>';
            html += '</label>';
        });

        container.innerHTML = html || '<div class="fieldDescription">No selectable libraries were found.</div>';
        syncLibrarySelectionState(page);
    }

    function collectTargetLibraryIds(page) {
        if (!page.querySelector('.chkAllLibraries').checked) {
            return [];
        }

        if (!page._libraryListLoaded) {
            return (page._lastSavedTargetLibraryIds || []).slice();
        }

        return Array.prototype.slice.call(page.querySelectorAll('.library-check:checked'))
            .map(function (checkbox) {
                return String(checkbox.value || '').trim();
            })
            .filter(function (id) {
                return !!id;
            });
    }

    function loadLibraries(page, config) {
        var selectedIds = Array.isArray(config.TargetLibraryIds) ? config.TargetLibraryIds : [];
        page._lastSavedTargetLibraryIds = selectedIds.slice();
        page._libraryListLoaded = false;
        setLibraryStatus(page, '');

        page.querySelector('.chkAllLibraries').checked = selectedIds.length > 0;
        updateLibrarySelectionVisibility(page);

        return ApiClient.getJSON(ApiClient.getUrl('Library/VirtualFolders')).then(function (folders) {
            renderLibraries(page, Array.isArray(folders) ? folders : [], selectedIds);
            page._libraryListLoaded = true;
            updateLibrarySelectionVisibility(page);
        }).catch(function (error) {
            console.error('Failed to load libraries for Rating Standardizer.', error);
            page.querySelector('.libraryListContainer').innerHTML = '<div class="fieldDescription">Library list is unavailable.</div>';
            setLibraryStatus(page, 'Could not load libraries. Saving keeps the currently stored library selection.');
        });
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
        var arrowCell = document.createElement('td');
        var targetCell = document.createElement('td');
        var deleteCell = document.createElement('td');
        var deleteButton = document.createElement('button');

        originalCell.appendChild(createInput('txtOriginalRating ratingStandardizerOriginalInput', 'TV-14', mapping && mapping.OriginalRating));
        arrowCell.className = 'ratingStandardizerArrowCell';
        arrowCell.innerHTML = '<span class="ratingStandardizerArrowGlyph">→</span>';
        targetCell.appendChild(createInput('txtTargetRating ratingStandardizerTargetInput', 'PG12', mapping && mapping.TargetRating));

        deleteCell.className = 'ratingStandardizerDeleteCell';
        deleteButton.setAttribute('is', 'emby-button');
        deleteButton.type = 'button';
        deleteButton.className = 'ratingStandardizerDeleteButton';
        deleteButton.setAttribute('title', 'Remove mapping');
        deleteButton.setAttribute('aria-label', 'Remove mapping');
        deleteButton.innerHTML = '<span>×</span>';
        deleteButton.addEventListener('click', function () {
            row.remove();
        });
        deleteCell.appendChild(deleteButton);

        row.appendChild(originalCell);
        row.appendChild(arrowCell);
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
        page.querySelector('.selectPreset').value = inferPresetKey(config.Mappings || []);
        renderMappings(page, config.Mappings || []);
        updatePresetDescription(page);
        updateMappingsEditorVisibility(page);
        setError(page, '');
        setStatus(page, '');
        setLibraryStatus(page, '');

        return loadLibraries(page, config).then(function () {
            loading.hide();
        });
    }

    function applySelectedPreset(page) {
        var presetKey = page.querySelector('.selectPreset').value;
        var preset = presets[presetKey] || presets.japan;

        setError(page, '');
        setStatus(page, '');
        if (presetKey !== 'blank') {
            renderMappings(page, cloneMappings(preset.mappings));
            page.querySelector('.selectPreset').value = 'blank';
            setStatus(page, 'Preset copied into custom mappings.');
        }

        updatePresetDescription(page);
        updateMappingsEditorVisibility(page);
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
            config.Mappings = page.querySelector('.selectPreset').value === 'blank'
                ? collectMappings(page)
                : cloneMappings((presets[page.querySelector('.selectPreset').value] || presets.japan).mappings);
            config.TargetLibraryIds = collectTargetLibraryIds(page);

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

        view.querySelector('.btnRestoreDefaults').addEventListener('click', function () {
            restoreCustomDefaults(view);
        });

        view.querySelector('.btnRunNow').addEventListener('click', function () {
            runNow(view);
        });

        view.querySelector('.chkAllLibraries').addEventListener('change', function () {
            updateLibrarySelectionVisibility(view);
        });

        view.querySelector('.libraryListContainer').addEventListener('change', function (event) {
            if (event.target && event.target.classList.contains('library-check')) {
                syncLibrarySelectionState(view);
            }
        });

        view.querySelector('.selectPreset').addEventListener('change', function () {
            updatePresetDescription(view);
            updateMappingsEditorVisibility(view);
        });
    }

    Object.assign(View.prototype, BaseView.prototype);

    View.prototype.onResume = function () {
        BaseView.prototype.onResume.apply(this, arguments);

        var page = this.view;
        loading.show();

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            return loadPage(page, config);
        }).catch(function (error) {
            console.error('Failed to load Rating Standardizer configuration.', error);
            setError(page, 'Failed to load plugin configuration. Check browser console and server logs.');
            loading.hide();
        });
    };

    return View;
});
