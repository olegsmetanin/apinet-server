/*global jQuery: true */
(function ($) {

	$.extend($, {
		isoDateTimeString: function (date) {
			var padString = function (n) {
				return n < 10 ? '0' + n : n;
			};
			return date.getUTCFullYear() + '-' + padString(date.getUTCMonth() +
				1) + '-' + padString(date.getUTCDate()) + 'T' + 
				padString(date.getUTCHours()) +
				':' + padString(date.getUTCMinutes()) + ':' + 
				padString(date.getUTCSeconds()) +
				'Z';
		},

		localDateString: function (date) {
			var padString = function (n) {
				return n < 10 ? '0' + n : n;
			};
			return padString(date.getDate()) + '.' +
				padString(date.getMonth() + 1) + '.' +
				date.getFullYear();
		},

		localDateTimeString: function (date) {
			var padString = function (n) {
				return n < 10 ? '0' + n : n;
			};
			return padString(date.getDate()) + '.' +
				padString(date.getMonth() + 1) + '.' +
				date.getFullYear() + ' ' + padString(date.getHours()) + ':' +
				padString(date.getMinutes())
			/* + ':'
			+ padString(date.getSeconds())*/
			;
		},

		confirmationDialog: function (title, innerHtml, callback) {
			var div = $('<div></div>').appendTo('body').html(innerHtml);
			div.dialog({
				modal: true,
				title: title,
				zIndex: 10000,
				autoOpen: true,
				width: 'auto',
				resizable: false,
				buttons: {
					'Да': function () {
						callback(true);
						$(this).dialog('close');
					},
					'Нет': function () {
						callback(false);
						$(this).dialog('close');
					}
				},
				close: function () {
					$(this).remove();
				}
			});
		},

		createSelectOption: function (value, displayName, currentValue) {
			value = $.trim(value);
			displayName = $.trim(displayName);
			currentValue = $.trim(currentValue);

			var option = $('<option />').val(value);
			option.text(displayName.length ? displayName : value);
			if (currentValue.length && value === currentValue) {
				option.attr('selected', 'selected');
			}

			return option;
		}
	});
})(jQuery);