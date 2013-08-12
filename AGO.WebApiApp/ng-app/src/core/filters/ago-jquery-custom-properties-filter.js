/*global jQuery: true */
(function ($) {
	var pluginName, defaults;

	pluginName = 'customPropertiesFilter';
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
				me.settings.meta, me);

			me.container = $(me.element).html('').addClass('filterBuilder').addClass(
				'customPropertiesFilter');

			me.topButtons = $('<div />').addClass('buttons btn-group');
			me.container.append(me.topButtons);

			me.addRootFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled', 'disabled')
				.html('<i class=\'icon-plus\'></i>').attr('title', 'Добавить');
			me.topButtons.append(me.addRootFilterNodeButton);
			me.addRootFilterNodeButton.on('click', function () {
				me.filterBuilder.selectNode(me.filterBuilder);
				me.filterBuilder.addNode(true);
				return false;
			});

			me.editFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled', 'disabled')
				.html('<i class=\'icon-pencil\'></i>').attr('title',
					'Редактировать');
			me.topButtons.append(me.editFilterNodeButton);
			me.editFilterNodeButton.on('click', function () {
				me.filterBuilder.beginEdit();
				return false;
			});

			me.deleteFilterNodeButton = $('<button />').addClass('btn').attr(
				'disabled', 'disabled')
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
				.attr('disabled', 'disabled')
				.html('<i class=\'icon-remove-circle icon-white\'></i> Очистить').attr(
					'title', 'Очистить');
			me.bottomButtons.append(me.clearFilterButton);
			me.clearFilterButton.on('click', function () {
				me.filterBuilder.clear();
				return false;
			});

			me.filterBuilder.clear();
			if ($.isArray(me.settings.data)) {
				me.filterBuilder.load(me.settings.data);
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

			if (node.parent !== me.filterBuilder) {
				return;
			}
			nodeData = me.nodePanel(node).children('.nodeData');
			nodeData.addClass('selectedNodeData');
			nodeData.on('dblclick', function () {
				me.filterBuilder.beginEdit();
			});
		},

		nodeDeselected: function (node) {
			var me = this, nodeData;

			if (node.parent !== me.filterBuilder) {
				return;
			}
			nodeData = me.nodePanel(node).children('.nodeData');
			nodeData.removeClass('selectedNodeData');
			nodeData.off('dblclick');
		},

		nodeAdded: function (node) {
			var me = this, nodePanel, nodeData;

			if (node.parent === me.filterBuilder) {
				nodePanel = $('<div />').addClass('nodePanel');
				me.nodesContainer.append(nodePanel);
				nodePanel.attr('nodeId', node.nodeId);
				nodePanel.data('node', node);

				nodeData = $('<div />').addClass('nodeData');
				nodePanel.append(nodeData);
				nodeData.on('click', function () {
					me.filterBuilder.selectNode(nodePanel.data('node'));
				});
			}

			me.nodeUpdated(node);
		},

		nodeUpdated: function (node) {
			var me = this, createMarkup, propertyTypeNode, propertyValueNode,
				nodeData, buttons, commitButton, cancelButton, propertyTypeLabel,
				opLabel, valueLabel, errors, errorsIcon, value, displayValue;

			while (node.parent && node.parent !== me.filterBuilder) {
				node = node.parent;
			}

			createMarkup = function () {
				propertyTypeNode = me.filterBuilder.findSubNode(node,
					'PropertyType');
				propertyValueNode = me.filterBuilder.findSubNode(node,
					function (n) {
						return n && (n.path === 'StringValue' || n.path ===
							'NumberValue' || n.path === 'DateValue');
					});

				nodeData = me.nodePanel(node).children('.nodeData');
				nodeData.html('');

				if (me.filterBuilder.editingNode === node) {
					nodeData.addClass('editingNode');

					me.createPropertyTypeLookup(propertyTypeNode, nodeData);

					me.createOpSelect(propertyValueNode, nodeData);
					me.createValueEditor(propertyValueNode, nodeData);

					buttons = $('<div />').addClass('buttons btn-group');
					nodeData.append(buttons);

					commitButton = $('<button />').addClass('btn btn-success').text(
						'Сохранить');
					buttons.append(commitButton);
					commitButton.on('click', function () {
						me.propertyValueChangedEvent.apply(buttons);
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

					propertyTypeLabel = $('<label />').addClass(
						'propertyTypeLabel');
					nodeData.append(propertyTypeLabel);
					propertyTypeLabel.text(propertyTypeNode ?
						me.filterBuilder.nodeDisplayValue(
						propertyTypeNode) : 'Неизвестный параметр');

					if (propertyValueNode) {
						opLabel = $('<label />').addClass('opLabel');
						nodeData.append(opLabel);
						opLabel.text(me.filterBuilder.nodeOpDisplayName(
							propertyValueNode));

						valueLabel = $('<label />').addClass('valueLabel');
						nodeData.append(valueLabel);
						valueLabel.text(me.filterBuilder.nodeDisplayValue(
							propertyValueNode));
					}

					errors = [];
					if (node.validationErrors) {
						errors = $.merge(errors, node.validationErrors);
					}
					if (propertyTypeNode && propertyTypeNode.validationErrors) {
						errors = $.merge(errors, propertyTypeNode.validationErrors);
					}
					if (propertyValueNode && propertyValueNode.validationErrors) {
						errors = $.merge(errors, propertyValueNode.validationErrors);
					}

					if (errors.length) {
						errorsIcon = $('<span />').addClass('errorsIcon');
						nodeData.append(errorsIcon);
						errorsIcon.html('&nbsp;');
						errorsIcon.attr('title', errors.join(','));
					}
				}
			};

			propertyTypeNode = me.filterBuilder.findSubNode(node,
				'PropertyType');
			if (propertyTypeNode) {
				value = $.trim(propertyTypeNode.value);
				displayValue = $.trim(propertyTypeNode.displayValue);

				if (value.length && !displayValue.length) {
					$.ajax('/Dictionary/LookupCustomPropertyTypes', {
						data: {
							id: value
						},
						dataType: 'json'
					})
						.done(function (data) {
							if (data && data.length) {
								propertyTypeNode = me.filterBuilder.findSubNode(node,
									'PropertyType');
								propertyValueNode = me.filterBuilder.findSubNode(node,
									function (n) {
										return n && (n.path === 'StringValue' ||
											n.path === 'NumberValue' ||
											n.path === 'DateValue');
									});

								propertyTypeNode.displayValue = $.trim(data[0].Name);
								if (propertyValueNode) {
									propertyValueNode.path = $.trim(data[0].ValueType) +
										'Value';
									propertyValueNode.Metadata =
										me.filterBuilder.calcNodeMetadata(
											propertyValueNode);
								}
							}

							createMarkup();
						});

					return;
				}
			}

			createMarkup();
		},

		nodeRemoved: function (node) {
			var me = this;

			if (node.parent !== me.filterBuilder) {
				return;
			}
			me.nodePanel(node).remove();
		},

		//endregion

		//region Helper methods

		createPropertyTypeLookup: function (propertyTypeNode, nodeData) {
			var me = this, value, displayValue, propertyTypeLookup;

			value = propertyTypeNode ? $.trim(propertyTypeNode.value) : '';
			displayValue = propertyTypeNode ? me.filterBuilder.nodeDisplayValue(
				propertyTypeNode) : '';

			propertyTypeLookup = $('<input />').addClass(
				'propertyTypeLookup').attr('type', 'hidden').val(value);
			nodeData.append(propertyTypeLookup);

			propertyTypeLookup.select2({
				allowClear: true,
				ajax: {
					url: '/Dictionary/LookupCustomPropertyTypes',
					dataType: 'json',
					data: function (term, page) {
						return {
							term: term,
							page: page - 1,
							pageSize: 10
						};
					},
					results: function (data) {
						data = $.isArray(data) ? data : [];
						return {
							results: data,
							more: data.length === 10
						};
					}
				},
				initSelection: function (element, callback) {
					callback({
						id: value,
						text: displayValue
					});
				}
			});

			propertyTypeLookup.on('change', me.propertyTypeChangedEvent);

			return propertyTypeLookup;
		},

		createOpSelect: function (propertyValueNode, nodeData) {
			var me = this, opSelect;

			opSelect = $('<select />').addClass('opSelect');
			nodeData.append(opSelect);

			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '=');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '!=');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '>');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '<');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '>=');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, '<=');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode,
				'exists');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode,
				'not exists');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode, 'like');
			me.addOpToSelectIfApplicable(opSelect, propertyValueNode,
				'not like');

			return opSelect;
		},

		createValueEditor: function (propertyValueNode, nodeData) {
			var propertyType, value, datePicker, date, valueEditor;

			propertyType = propertyValueNode ? $.trim(propertyValueNode.Metadata
				.PropertyType) : 'string';
			value = propertyValueNode ? $.trim(propertyValueNode.value) : '';

			if (propertyType === 'date') {
				datePicker = $('<input />').datepicker({
					dateFormat: 'dd.mm.yy',
					dayNamesMin: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
					firstDay: 1,
					monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май',
						'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь',
						'Декабрь'
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

				return datePicker;
			}
			else {
				valueEditor = $('<input />').addClass('valueEditor');
				nodeData.append(valueEditor);

				valueEditor.val(value);

				return valueEditor;
			}
		},

		propertyTypeChangedEvent: function (e) {
			var nodeData, nodePanel, me, node, propertyTypeNode, propertyValueNode,
				clonedNode;

			nodeData = $(this).parent('.nodeData');
			nodePanel = nodeData.parent('.nodePanel');
			if (!nodeData.length || !nodePanel.length) {
				return;
			}
			me = nodePanel.parents('.filterBuilder').data('plugin_' +
				pluginName);
			node = nodePanel.data('node');
			propertyTypeNode = me.ensurePropertyTypeNode(node);
			propertyValueNode = me.ensurePropertyValueNode(node);

			clonedNode = me.filterBuilder.cloneNodeData(propertyTypeNode);
			clonedNode.value = e.val;
			clonedNode.displayValue = '';
			me.filterBuilder.changeNode(propertyTypeNode, clonedNode);

			clonedNode = me.filterBuilder.cloneNodeData(propertyValueNode);
			clonedNode.value = '';
			me.filterBuilder.changeNode(propertyValueNode, clonedNode);
		},

		propertyValueChangedEvent: function () {
			var nodeData, nodePanel, me, op, opSelect, value, valueEditor,
				date, node, propertyValueNode, clonedNode;

			nodeData = $(this).parent('.nodeData');
			nodePanel = nodeData.parent('.nodePanel');
			if (!nodeData.length || !nodePanel.length) {
				return;
			}
			me = nodePanel.parents('.filterBuilder').data('plugin_' +
				pluginName);
			op = null;
			opSelect = $('.opSelect', nodeData);
			if (opSelect.length) {
				op = $.trim(opSelect.val());
			}

			value = null;
			valueEditor = $('.valueEditor', nodeData);
			if (valueEditor.length) {
				value = $.trim(valueEditor.val());

				if (valueEditor.hasClass('datePicker')) {
					date = valueEditor.datepicker('getDate');
					if (date && !isNaN(date.valueOf())) {
						value = $.isoDateTimeString(date);
					}
				}
			}

			node = nodePanel.data('node');
			propertyValueNode = me.ensurePropertyValueNode(node);
			clonedNode = me.filterBuilder.cloneNodeData(propertyValueNode);
			clonedNode.op = op !== null ? op : clonedNode.op;
			clonedNode.value = value !== null ? value : clonedNode.value;

			me.filterBuilder.changeNode(propertyValueNode, clonedNode);
		},

		ensurePropertyTypeNode: function (node) {
			var me = this, propertyTypeNode;

			propertyTypeNode = me.filterBuilder.findSubNode(node,
				'PropertyType');
			if (!propertyTypeNode) {
				me.filterBuilder.selectNode(node);
				propertyTypeNode = me.filterBuilder.addNode();
				propertyTypeNode.path = 'PropertyType';
				propertyTypeNode.op = '=';
				propertyTypeNode.Metadata = me.filterBuilder.calcNodeMetadata(
					propertyTypeNode);
			}

			return propertyTypeNode;
		},

		ensurePropertyValueNode: function (node) {
			var me = this, propertyValueNode;

			propertyValueNode = me.filterBuilder.findSubNode(node,
				function (n) {
					return n && (n.path === 'StringValue' || n.path === 'NumberValue' ||
						n.path === 'DateValue');
				});
			if (!propertyValueNode) {
				me.filterBuilder.selectNode(node);
				propertyValueNode = me.filterBuilder.addNode();
				propertyValueNode.path = 'StringValue';
				propertyValueNode.op = '=';
				propertyValueNode.Metadata = me.filterBuilder.calcNodeMetadata(
					propertyValueNode);
			}

			return propertyValueNode;
		},

		addOpToSelectIfApplicable: function (opSelect, node, op) {
			var me = this;

			if (node && !me.filterBuilder.isOpApplicableToNode(node, op)) {
				return;
			}
			opSelect.append($.createSelectOption(op, me.filterBuilder.opDisplayName(
				op), node ? node.op : ''));
		},

		nodePanel: function (node) {
			var me = this;

			return node.isRoot ? me.nodesContainer : $(
				'div.nodePanel[nodeId=\'' + $.trim(node.nodeId) + '\']', me.nodesContainer
			);
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