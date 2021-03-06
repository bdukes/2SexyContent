﻿
// Maps actions of the module menu to JS actions - needed because onclick event can't be set (actually, a bug in DNN)
var $2sxcActionMenuMapper = function (moduleId) {
    return {
        changeLayoutOrContent: function() {
            $2sxc(moduleId).manage._getSelectorScope().setTemplateChooserState(true);
        },
        addItem: function() {
            $2sxc(moduleId).manage.action({ 'action':'add', 'useModuleList': true });
        },
        edit: function() {
            $2sxc(moduleId).manage.action({ 'action': 'edit', 'useModuleList': true, 'sortOrder': 0 });
        }
    };
};