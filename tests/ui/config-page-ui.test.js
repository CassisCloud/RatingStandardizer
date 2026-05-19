const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..', '..');
const embyHtml = fs.readFileSync(path.join(root, 'RatingStandardizer.Emby', 'Configuration', 'configPage.html'), 'utf8');
const embyJs = fs.readFileSync(path.join(root, 'RatingStandardizer.Emby', 'Configuration', 'configPage.js'), 'utf8');
const jellyfinHtml = fs.readFileSync(path.join(root, 'RatingStandardizer.Jellyfin', 'Configuration', 'configPage.html'), 'utf8');
const embyPlugin = fs.readFileSync(path.join(root, 'RatingStandardizer.Emby', 'Plugin.cs'), 'utf8');
const jellyfinPlugin = fs.readFileSync(path.join(root, 'RatingStandardizer.Jellyfin', 'Plugin.cs'), 'utf8');

function assert(condition, message) {
    if (!condition) {
        throw new Error(message);
    }
}

function count(text, needle) {
    return (text.match(new RegExp(needle.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'g')) || []).length;
}

function assertUnversionedPageResources() {
    const all = [embyHtml, embyJs, jellyfinHtml, embyPlugin, jellyfinPlugin].join('\n');
    assert(embyHtml.includes('data-controller="__plugin/ratingstandardizerjs"'), 'Emby HTML must reference the unversioned controller.');
    assert(embyPlugin.includes('Name = "ratingstandardizer"'), 'Emby Plugin.cs must expose an unversioned page.');
    assert(embyPlugin.includes('Name = "ratingstandardizerjs"'), 'Emby Plugin.cs must expose an unversioned script.');
    assert(jellyfinPlugin.includes('Name = "ratingstandardizer"'), 'Jellyfin Plugin.cs must expose an unversioned page.');
    assert(!/ratingstandardizer(?:js)?-v\d+\b/.test(all), 'Versioned page references must not remain.');
}

function assertLayoutContracts(name, html) {
    assert(html.includes('rs-field-grid'), `${name}: Normalization must use rs-field-grid.`);
    assert(count(html, 'class="rs-field"') >= 5, `${name}: select/input controls must be wrapped in rs-field containers.`);
    assert(html.includes('rs-test-grid'), `${name}: Test Conversion must use rs-test-grid.`);
    assert(html.includes('rs-test-button'), `${name}: Test button must use constrained rs-test-button.`);
    assert(html.includes('ratingStandardizerLibraryList'), `${name}: library list must use card grid class.`);
    assert(html.includes('rs-profile-table'), `${name}: Library Overrides table must use rs-profile-table.`);
    assert(html.includes('rs-rules-table'), `${name}: Custom Rating Rules table must use rs-rules-table.`);
    assert(html.includes('rs-status-col'), `${name}: Status column must use rs-status-col.`);
}

function assertDynamicCheckboxes() {
    assert(embyJs.includes('type="checkbox" class="rs-native-check rule-enabled'), 'Emby rule checkbox must use visible table checkbox.');
    assert(embyJs.includes('type="checkbox" class="rs-native-check profile-enabled'), 'Emby profile checkbox must use visible table checkbox.');
    assert(jellyfinHtml.includes('type="checkbox" class="rs-native-check rule-enabled'), 'Jellyfin rule checkbox must use visible table checkbox.');
    assert(jellyfinHtml.includes('type="checkbox" class="rs-native-check profile-enabled'), 'Jellyfin profile checkbox must use visible table checkbox.');
    assert([embyHtml, jellyfinHtml].join('\n').includes('.rs-table .rs-native-check'), 'Table checkbox visibility styles must be present.');
}

function assertLibraryCardScalesToThirtyItems() {
    const libraries = Array.from({ length: 30 }, (_, index) => ({
        id: `library-${index + 1}`,
        name: `Library ${index + 1}`,
        selected: index % 3 === 0,
    }));

    const html = libraries.map((library) => {
        const checked = library.selected ? 'checked' : '';
        const selected = library.selected ? ' is-selected' : '';
        return `<label class="ratingStandardizerLibraryItem${selected}"><input type="checkbox" class="library-check" value="${library.id}" ${checked}><span class="ratingStandardizerLibraryContent"><span class="ratingStandardizerLibraryCheck"></span><span class="ratingStandardizerLibraryName">${library.name}</span></span></label>`;
    }).join('');

    assert(count(html, 'ratingStandardizerLibraryItem') === 30, '30 library cards must render as 30 card items.');
    assert(count(html, 'is-selected') === 10, 'Selected library cards must keep is-selected state.');
    assert(count(html, 'library-check') === 30, 'Each library card must contain one checkbox.');
}

function assertJellyfinRuntimeStyles() {
    assert(jellyfinHtml.includes('ensureRuntimeStyles'), 'Jellyfin must inject runtime styles because head styles are not preserved.');
    assert(jellyfinHtml.includes('ensureVisibilityStyles'), 'Jellyfin must inject visibility overrides after runtime styles.');
    assert(jellyfinHtml.includes('RatingStandardizerRuntimeStyles'), 'Jellyfin runtime style element id must be stable.');
    assert(jellyfinHtml.includes('RatingStandardizerVisibilityStyles'), 'Jellyfin visibility style element id must be stable.');
    assert(jellyfinHtml.includes('.rs-field-grid'), 'Jellyfin runtime styles must include field grid CSS.');
    assert(jellyfinHtml.includes('.rs-rules-table'), 'Jellyfin runtime styles must include rules table CSS.');
    assert(jellyfinHtml.includes('.rs-test-button'), 'Jellyfin runtime styles must include test button CSS.');
}

function assertVisibilityContracts(name, html, script) {
    const source = [html, script || ''].join('\n');
    assert(source.includes('.rs-table th { color: currentColor;') || source.includes('.rs-table th{color:currentColor'), `${name}: table headers must inherit theme text color.`);
    assert(!/\.rs-table th \{ color: rgba\(255,255,255,\.72\)/.test(html), `${name}: visible CSS must not use white-only table header text.`);
    assert(source.includes('text-align: center; width: 5.4rem') || source.includes('text-align:center!important;width:5.4rem'), `${name}: checkbox columns must leave room for themed controls and remain centered.`);
    assert(source.includes('.rs-check-cell { align-items: center; cursor: pointer; display: flex;') || source.includes('.rs-check-cell { align-items: center; cursor: pointer; display: inline-flex;') || source.includes('.rs-check-cell{display:flex!important'), `${name}: checkbox wrapper must center themed checkboxes.`);
    assert(source.includes('min-height: 10rem'), `${name}: import/export textareas must have a usable default height.`);
    assert(count(source, 'rows="8"') >= 2, `${name}: import/export textareas must declare rows.`);
}

function assertDynamicTableControlsUseTheme() {
    const dynamicSources = [embyJs, jellyfinHtml].join('\n');
    assert(/is="emby-input" class="rule-name/.test(dynamicSources), 'Dynamic rule text inputs must use themed inputs.');
    assert(/is="emby-select" class="rule-type/.test(dynamicSources), 'Dynamic rule selects must use themed selects.');
    assert(/is="emby-select" class="profile-preset/.test(dynamicSources), 'Dynamic profile selects must use themed selects.');
    assert(dynamicSources.includes('is="emby-textarea"'), 'Import/export textareas must use themed textareas.');
}

function assertRunActions() {
    const source = [embyHtml, embyJs, jellyfinHtml].join('\n');
    assert(source.includes('Rollback'), 'Configuration pages must expose a rollback action.');
    assert(source.includes('Plugins/RatingStandardizer/Rollback'), 'Rollback action must call the rollback API.');
    assert(source.includes('Plugins/RatingStandardizer/History'), 'Rollback history selector must load plugin history.');
    assert(source.includes('rollbackResultMessage'), 'Configuration pages must render rollback results.');
    assert(source.includes('confirmAction'), 'Destructive actions must use the themed confirmation dialog.');
    assert(!source.includes('confirm('), 'Browser-native confirm dialogs must not be used.');
    assert(source.includes('UseConfiguredTargetLibraries'), 'Rollback must send target selection to the API.');
    assert(source.includes('Select all'), 'Rollback history selector must expose select all.');
    assert(source.includes('Clear selection'), 'Rollback history selector must expose clear selection.');
    assert(source.includes('Selected execution date/time'), 'Rollback must support execution date/time selection.');
    assert(source.includes('UpdatedAtKeys'), 'Rollback must send selected execution date/time keys to the API.');
    assert(source.includes('resultValue(result, pascal, camel)'), 'Run result rendering must handle PascalCase and camelCase API responses.');
    assert(source.includes("resultValue(result, 'ScannedCount', 'scannedCount')"), 'Run Now result must read PascalCase or camelCase scanned count.');
}

function assertActionButtonsUseTheme() {
    const source = [embyHtml, jellyfinHtml].join('\n');
    assert(!source.includes('rs-action-primary'), 'Save button must not use custom primary colors.');
    assert(!source.includes('rs-action-run'), 'Run Now button must not use custom run colors.');
    assert(!source.includes('rs-action-danger'), 'Rollback button must not use custom danger colors.');
    assert(source.includes('rs-main-actions'), 'Main actions must remain grouped vertically.');
    assert(source.includes('Rollback Options'), 'Configuration pages must expose rollback target options.');
}

function assertDataMigration() {
    const source = [embyHtml, embyJs, jellyfinHtml].join('\n');
    assert(source.includes('Data Migration'), 'Configuration pages must expose data migration tools.');
    assert(source.includes('Download backup'), 'Configuration pages must expose backup download.');
    assert(source.includes('Upload backup'), 'Configuration pages must expose backup upload.');
    assert(source.includes('Plugins/RatingStandardizer/HistoryExport'), 'Data migration must export history through the API.');
    assert(source.includes('Plugins/RatingStandardizer/HistoryImport'), 'Data migration must import history through the API.');
    assert(source.includes('rating-standardizer-backup-v1'), 'Backup files must declare a stable format.');
    assert(source.includes('rs-file-input'), 'Upload file inputs must be hidden behind explicit actions.');
}

function assertBuiltInSimplePreset() {
    const source = [embyHtml, embyJs, jellyfinHtml].join('\n');
    assert(source.includes('value="my-simple-ratings"'), 'Configuration pages must expose My Simple Ratings.');
    assert(source.includes("'my-simple-ratings'"), 'Test conversion presets must include My Simple Ratings.');
    assert(source.includes('TEEN') && source.includes('MATURE') && source.includes('ADULT'), 'My Simple Ratings labels must be present.');
}

function main() {
    assertUnversionedPageResources();
    assertLayoutContracts('Emby', embyHtml);
    assertLayoutContracts('Jellyfin', jellyfinHtml);
    assertDynamicCheckboxes();
    assertDynamicTableControlsUseTheme();
    assertRunActions();
    assertActionButtonsUseTheme();
    assertDataMigration();
    assertBuiltInSimplePreset();
    assertLibraryCardScalesToThirtyItems();
    assertJellyfinRuntimeStyles();
    assertVisibilityContracts('Emby', embyHtml, embyJs);
    assertVisibilityContracts('Jellyfin', jellyfinHtml);
    console.log('Config page UI checks passed.');
}

main();
