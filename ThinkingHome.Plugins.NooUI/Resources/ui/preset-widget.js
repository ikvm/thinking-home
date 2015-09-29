﻿define(['lib'], function (lib) {

	var PresetWidgetView = lib.marionette.ItemView.extend({
		template: lib.handlebars.compile(
			'{{displayName}}' +
				'<div class="btn-group btn-group-justified">' +
					'<a href="#" class="btn btn-default th-pointer js-btn-load"> Load </a>' +
				'</div>'),
		triggers: {
			"click .js-btn-load": "preset:load",
		}
	});

	var createSender = function (channel, cmd) {
		return function () {
			return lib.$.getJSON('/api/noolite', { ch: channel, cmd: cmd });
		}
	};

	return {
		show: function (model, region) {
			var view = new PresetWidgetView({ model: model });
			var channel = model.get('data').channel;

			view.on("preset:load", createSender(channel, 7)); // NooLite Load
		
			region.show(view);
		}
	};
});