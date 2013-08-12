/*global jQuery: true */
(function ($) {
	var pluginName, defaults;

	pluginName = 'structuredFilter';
	defaults = {
		meta: {}
	};

	function Plugin(element, options) {
		this.element = element;
		this.settings = $.extend({}, defaults, options);
		this.defaults = defaults;
		this.name = pluginName;
		this.init();
	}

	Plugin.prototype = {

		isUIAdapter: true,

		//region Methods

		init: function () {
			var me = this;

			me.filterBuilder = new $.FilterBuilder($.trim(me.settings.path),
				me.settings
				.meta, me);

			me.container = $(me.element).html('').addClass('filterBuilder').addClass(
				'structuredFilter');

			me.topButtons = $('<div />').addClass('buttons btn-group');
			me.container.append(me.topButtons);

			me.addRootFilterNodeButton = $('<button />').addClass(
				'btn btn-success')
				.attr('disabled', 'disabled')
				.html('<i class=\'icon-plus icon-white\'></i> верхний уровень')
				.attr('title', 'Добавить верхний уровень');
			me.topButtons.append(me.addRootFilterNodeButton);
			me.addRootFilterNodeButton.on('click', function () {
				me.filterBuilder.selectNode(me.filterBuilder);
				me.filterBuilder.addNode(true);
				return false;
			});

			me.addFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled',
				'disabled')
				.html('<i class=\'icon-plus\'></i>').attr('title', 'Добавить');
			me.topButtons.append(me.addFilterNodeButton);
			me.addFilterNodeButton.on('click', function () {
				me.filterBuilder.addNode(true);
				return false;
			});

			me.editFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled',
				'disabled')
				.html('<i class=\'icon-pencil\'></i>').attr('title',
					'Редактировать');
			me.topButtons.append(me.editFilterNodeButton);
			me.editFilterNodeButton.on('click', function () {
				me.filterBuilder.beginEdit();
				return false;
			});

			me.deleteFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled',
				'disabled')
				.html('<i class=\'icon-minus\'></i>').attr('title', 'Удалить');
			me.topButtons.append(me.deleteFilterNodeButton);
			me.deleteFilterNodeButton.on('click', function () {
				me.filterBuilder.deleteNode();
				return false;
			});

			me.nodesContainer = $('<div />').addClass('nodesContainer');
			me.container.append(me.nodesContainer);

			me.bottomButtons = $('<div />').addClass('buttons btn-group');
			me.container.append(me.bottomButtons);

			me.clearFilterButton = $('<button />').addClass('btn btn-warning')
				.attr(
					'disabled', 'disabled')
				.html('<i class=\'icon-remove-circle icon-white\'></i> Очистить')
				.attr(
					'title', 'Очистить');
			me.bottomButtons.append(me.clearFilterButton);
			me.clearFilterButton.on('click', function () {
				me.filterBuilder.clear();
				return false;
			});

			me.filterBuilder.clear();
			if (me.settings.data) {
				me.data(me.settings.data);
			}

			me.filterBuilder.callbacks.push(function (eventType) {
				me.container.trigger('filterChange', [eventType]);
			});

			if ($.isFunction(me.settings.filterChange)) {
				me.container.on('filterChange', me.settings.filterChange);
			}
		},

		validate: function () {
			var me = this;
			return me.filterBuilder.validate();
		},

		data: function (value) {
			var me = this, items;

			if (value) {
				items = null;

				if ($.isArray(value)) {
					items = value;
				}
				else if ($.isArray(value.items)) {
					items = value.items;
				}
				if (items) {
					me.filterBuilder.load(items);
				}

				return null;
			}
			else {
				return me.filterBuilder.compile();
			}
		},

		enableActions: function (addRootEnabled, addEnabled, editEnabled,
			deleteEnabled, clearEnabled) {
			var me = this;

			if (addRootEnabled) {
				me.addRootFilterNodeButton.removeAttr('disabled');
			}
			else {
				me.addRootFilterNodeButton.attr('disabled', 'disabled');
			}

			if (addEnabled) {
				me.addFilterNodeButton.removeAttr('disabled');
			}
			else {
				me.addFilterNodeButton.attr('disabled', 'disabled');
			}

			if (editEnabled) {
				me.editFilterNodeButton.removeAttr('disabled');
			}
			else {
				me.editFilterNodeButton.attr('disabled', 'disabled');
			}

			if (deleteEnabled) {
				me.deleteFilterNodeButton.removeAttr('disabled');
			}
			else {
				me.deleteFilterNodeButton.attr('disabled', 'disabled');
			}

			if (clearEnabled) {
				me.clearFilterButton.removeAttr('disabled');
			}
			else {
				me.clearFilterButton.attr('disabled', 'disabled');
			}
		},

		clear: function () {
			var me = this;
			me.nodesContainer.html('');
		},

		//endregion

		//region Events

		nodeSelected: function (node) {
			var me = this, nodeData;

			nodeData = me.nodePanel(node).children('.nodeData');
			nodeData.addClass('selectedNodeData');
			nodeData.on('dblclick', function () {
				me.filterBuilder.beginEdit();
			});
		},

		nodeDeselected: function (node) {
			var me = this, nodeData;

			nodeData = me.nodePanel(node).children('.nodeData');
			nodeData.removeClass('selectedNodeData');
			nodeData.off('dblclick');
		},

		nodeAdded: function (node) {
			var me = this, parentPanel, container, nodePanel, nodeData, nodeChildren;

			parentPanel = me.nodePanel(node.parent);
			container = node.parent.isRoot ? parentPanel : parentPanel.children(
				'.nodeChildren');

			nodePanel = $('<div />').addClass('nodePanel');
			container.append(nodePanel);
			nodePanel.attr('nodeId', node.nodeId);
			nodePanel.data('node', node);

			nodeData = $('<div />').addClass('nodeData');
			nodePanel.append(nodeData);
			nodeData.on('click', function () {
				me.filterBuilder.selectNode(nodePanel.data('node'));
			});

			nodeChildren = $('<div />').addClass('nodeChildren');
			nodePanel.append(nodeChildren);

			me.nodeUpdated(node);
		},

		nodeUpdated: function (node) {
			var me = this, nodeData, pathSelect, opSelect, valueEditor,
				buttons, commitButton, cancelButton, pathLabel, opLabel,
				valueLabel, errorsIcon, error, text, i;

			nodeData = me.nodePanel(node).children('.nodeData');
			nodeData.html('');

			if (me.filterBuilder.editingNode === node) {
				nodeData.addClass('editingNode');

				pathSelect = me.createPathSelect(node, nodeData);
				pathSelect.focus();
				if (!node.Metadata.ModelType && node.path && node.path.length && !
					me.filterBuilder
					.isSpecialNode(node)) {

					opSelect = me.createOpSelect(node, nodeData);
					opSelect.focus();
					if (node.Metadata.PropertyType && node.op && node.op.length && !
						me.filterBuilder
						.isUnaryNode(node)) {
						valueEditor = me.createValueEditor(node, nodeData);
						valueEditor.focus();
					}
				}

				buttons = $('<div />').addClass('buttons btn-group');
				nodeData.append(buttons);

				commitButton = $('<button />').addClass('btn btn-success').text(
					'Сохранить');
				buttons.append(commitButton);
				commitButton.on('click', function () {
					me.changedEvent.apply(buttons);
					me.filterBuilder.endEdit(true);
					return false;
				});

				cancelButton = $('<button />').addClass('btn btn-danger').text(
					'Отмена');
				buttons.append(cancelButton);
				cancelButton.on('click', function () {
					me.filterBuilder.endEdit();
					return false;
				});
			}
			else {
				nodeData.removeClass('editingNode');

				if (node.path && node.path.length) {
					pathLabel = $('<label />').addClass('pathLabel');
					nodeData.append(pathLabel);
					pathLabel.text(me.filterBuilder.nodePathDisplayName(node));
				}

				if (node.op && node.op.length) {
					opLabel = $('<label />');
					opLabel.addClass(me.filterBuilder.isSpecialNode(node) ?
						'pathLabel' :
						'opLabel');
					nodeData.append(opLabel);
					opLabel.text(me.filterBuilder.nodeOpDisplayName(node));
				}

				if (node.value && node.value.length) {
					valueLabel = $('<label />').addClass('valueLabel');
					nodeData.append(valueLabel);
					valueLabel.text(me.filterBuilder.nodeDisplayValue(node));
				}

				if (node.validationErrors && node.validationErrors.length) {
					errorsIcon = $('<span />').addClass('errorsIcon');
					nodeData.append(errorsIcon);
					errorsIcon.html('&nbsp;');

					text = '';
					for (i = 0; i < node.validationErrors.length; i += 1) {
						error = $.trim(node.validationErrors[i]);
						if (text.length) {
							text += ', ';
						}
						text += error;
					}
					errorsIcon.attr('title', text);
				}
			}
		},

		nodeRemoved: function (node) {
			var me = this;
			me.nodePanel(node).remove();
		},

		//endregion

		//region Helper methods

		changedEvent: function (suppressEvents) {
			var nodeData, nodePanel, me, node, clonedNode, select,
				valueEditor, date, timePicker, time;

			nodeData = $(this).parent('.nodeData');
			nodePanel = nodeData.parent('.nodePanel');
			if (!nodeData.length || !nodePanel.length) {
				return;
			}
			me = nodePanel.parents('.filterBuilder').data('plugin_' +
				pluginName);

			if (suppressEvents) {
				me.filterBuilder.suppressEvents = true;
			}

			try {
				node = nodePanel.data('node');
				clonedNode = me.filterBuilder.cloneNodeData(node);

				select = $('.pathSelect', nodeData);
				if (select.length) {
					clonedNode.path = select.val();
					if (clonedNode.path === '||' || clonedNode.path === '&&' ||
						clonedNode.path ===
						'&&!') {
						clonedNode.op = clonedNode.path;
						clonedNode.path = '';
					}
				}

				if (me.filterBuilder.isSpecialNode(node) && node.path !==
					clonedNode.path) {
					clonedNode.op = '';
				}

				if (!me.filterBuilder.isSpecialNode(clonedNode)) {
					select = $('.opSelect', nodeData);
					if (select.length) {
						clonedNode.op = select.val();
					}

					valueEditor = $('.valueEditor', nodeData);

					if (valueEditor.length) {
						clonedNode.value = valueEditor.val();

						if (valueEditor.hasClass('datePicker')) {
							date = valueEditor.datepicker('getDate');
							if (date && !isNaN(date.valueOf())) {
								timePicker = $('.timePicker', nodeData);
								if (timePicker.length) {
									time = timePicker.timepicker('getTime');
									if (time && !isNaN(time.valueOf())) {
										date.setHours(time.getHours());
										date.setMinutes(time.getMinutes());
										date.setSeconds(time.getSeconds());
									}
								}
								clonedNode.value = $.isoDateTimeString(date);
							}
						}
					}
				}

				me.filterBuilder.changeNode(node, clonedNode);
			}
			finally
			{
				if (suppressEvents) {
					me.filterBuilder.suppressEvents = false;
				}
			}
		},

		createPathSelect: function (node, nodeData) {
			var me = this, pathSelect, propertyMeta, currentPath, metadata, key;

			pathSelect = $('<select />').addClass('pathSelect');
			nodeData.append(pathSelect);

			pathSelect.on('keypress', function () {
				me.changedEvent.apply(this, [true]);
			});
			pathSelect.on('change', function () {
				me.changedEvent.apply(this, [true]);
			});

			pathSelect.append($.createSelectOption('&&', me.filterBuilder.opDisplayName(
				'&&'), node.op));
			pathSelect.append($.createSelectOption('||', me.filterBuilder.opDisplayName(
				'||'), node.op));
			pathSelect.append($.createSelectOption('&&!', me.filterBuilder.opDisplayName(
				'&&!'), node.op));

			currentPath = $.trim(node.path);
			metadata = me.filterBuilder.getModelMetadata(node.parent.Metadata
				.ModelType);

			for (key in metadata.PrimitiveProperties) {
				if (!metadata.PrimitiveProperties.hasOwnProperty(key)) {
					continue;
				}
				propertyMeta = metadata.PrimitiveProperties[key];
				pathSelect.append($.createSelectOption(key, propertyMeta.DisplayName,
					currentPath));
			}

			for (key in metadata.ModelProperties) {
				if (!metadata.ModelProperties.hasOwnProperty(key)) {
					continue;
				}
				propertyMeta = metadata.ModelProperties[key];
				pathSelect.append($.createSelectOption(key, propertyMeta.DisplayName,
					currentPath));
			}

			return pathSelect;
		},

		createOpSelect: function (node, nodeData) {
			var me = this, opSelect;

			opSelect = $('<select />').addClass('opSelect');
			nodeData.append(opSelect);

			opSelect.on('keypress', function () {
				me.changedEvent.apply(this, [true]);
			});
			opSelect.on('change', function () {
				me.changedEvent.apply(this, [true]);
			});

			me.addOpToSelectIfApplicable(opSelect, node, '=');
			me.addOpToSelectIfApplicable(opSelect, node, '!=');
			me.addOpToSelectIfApplicable(opSelect, node, '>');
			me.addOpToSelectIfApplicable(opSelect, node, '<');
			me.addOpToSelectIfApplicable(opSelect, node, '>=');
			me.addOpToSelectIfApplicable(opSelect, node, '<=');
			me.addOpToSelectIfApplicable(opSelect, node, 'exists');
			me.addOpToSelectIfApplicable(opSelect, node, 'not exists');
			me.addOpToSelectIfApplicable(opSelect, node, 'like');
			me.addOpToSelectIfApplicable(opSelect, node, 'not like');

			return opSelect;
		},

		createValueEditor: function (node, nodeData) {
			var propertyType, value, datePicker, date, timePicker,
				boolSelect, enumSelect, possibleValues, key, valueEditor;

			propertyType = $.trim(node.Metadata.PropertyType);
			value = $.trim(node.value);

			if (propertyType === 'date' || propertyType === 'datetime') {
				datePicker = $('<input />').datepicker({
					dateFormat: 'dd.mm.yy',
					dayNamesMin: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
					firstDay: 1,
					monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май',
						'Июнь',
						'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'
					],
					constrainInput: propertyType === 'date'
				}).addClass('valueEditor datePicker');
				nodeData.append(datePicker);

				date = new Date(value);
				if (!isNaN(date.valueOf())) {
					datePicker.datepicker('setDate', date);
				}
				else {
					datePicker.datepicker('setDate', value);
				}

				if (propertyType === 'datetime') {
					timePicker = $('<input />').timepicker({
						'timeFormat': 'H:i'
					}).addClass('timePicker');
					nodeData.append(timePicker);

					if (!isNaN(date.valueOf())) {
						timePicker.timepicker('setTime', date);
					}

					datePicker.on('change', function () {
						timePicker.focus();
					});
				}

				return datePicker;
			}
			else if (propertyType === 'boolean') {
				boolSelect = $('<select />').addClass(
					'valueEditor boolSelect');
				nodeData.append(boolSelect);

				boolSelect.append($.createSelectOption('', '', value));
				boolSelect.append($.createSelectOption('true', 'Да', value));
				boolSelect.append($.createSelectOption('false', 'Нет', value));

				return boolSelect;
			}
			else if (propertyType === 'enum') {
				enumSelect = $('<select />').addClass(
					'valueEditor enumSelect');
				nodeData.append(enumSelect);

				enumSelect.append($.createSelectOption('', '', value));
				possibleValues = $.isPlainObject(node.Metadata.PossibleValues) ?
					node.Metadata
					.PossibleValues : {};
				for (key in possibleValues) {
					if (!possibleValues.hasOwnProperty(key)) {
						continue;
					}

					enumSelect.append($.createSelectOption(key, $.trim(
							possibleValues[key]),
						value));
				}

				return enumSelect;
			}
			else {
				valueEditor = $('<input />').addClass('valueEditor');
				nodeData.append(valueEditor);

				valueEditor.val(value);

				return valueEditor;
			}
		},

		addOpToSelectIfApplicable: function (opSelect, node, op) {
			var me = this;

			if (!me.filterBuilder.isOpApplicableToNode(node, op)) {
				return;
			}

			opSelect.append($.createSelectOption(op, me.filterBuilder.opDisplayName(
					op),
				node.op));
		},

		nodePanel: function (node) {
			var me = this;

			return node.isRoot ? me.nodesContainer : $(
				'div.nodePanel[nodeId=\'' + $.trim(
					node.nodeId) + '\']', me.nodesContainer);
		}

		//endregion
	};

	$.fn[pluginName] = function (options, param) {
		var result, plugin, args;
		
		if (typeof (options) === 'string') {
			result = null;
			this.each(function () {
				plugin = $.data(this, 'plugin_' + pluginName);
				if (plugin && $.isFunction(plugin[options])) {
					args = [];
					if (param) {
						args.push(param);
					}
					result = plugin[options].apply(plugin, args);
				}
			});
			return result;
		}
		else {
			return this.each(function () {
				if (!$.data(this, 'plugin_' + pluginName)) {
					$.data(this, 'plugin_' + pluginName, new Plugin(this, options));
				}
			});
		}
	};

})(jQuery);