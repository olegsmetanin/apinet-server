/*global jQuery: true, console: true, localStorage: true */
(function ($, console, localStorage) {

	//region LocalStorageFiltersPersister

	$.LocalStorageFiltersPersister = function (prefix) {
		prefix = $.trim(prefix);

		$.extend(this, {
			isFiltersPersister: true,

			persistedFiltersNames: function (filterBuilder,
				modelName) {
				var allNames, allNamesJson, i;

				allNames = [];
				try {
					allNamesJson = JSON.parse(localStorage[
						prefix +
						modelName +
						'_AllPersistedFiltersNames']);
					allNamesJson = $.isArray(allNamesJson) ?
						allNamesJson : [];
					for (i = 0; i < allNamesJson.length; i +=
						1) {
						allNames.push($.trim(allNamesJson[i]));
					}
				}
				catch (error) {
					allNames = [];
				}

				return allNames;
			},

			loadFilter: function (filterBuilder, modelName,
				filterName) {
				return filterBuilder.nodesFromJsonString(
					localStorage[
						prefix + modelName + '_' + filterName
					]);
			},

			saveFilter: function (filterBuilder, modelName,
				filterName,
				rootNode) {
				var allNames;

				localStorage[prefix + modelName + '_' +
					filterName] =
					JSON.stringify(rootNode.items);

				allNames = this.persistedFiltersNames(
					filterBuilder,
					modelName);
				if ($.inArray(filterName, allNames) === -1) {
					allNames.push(filterName);
				}
				localStorage[prefix + modelName +
					'_AllPersistedFiltersNames'] = JSON.stringify(
					allNames);
				return {};
			},

			deleteFilter: function (filterBuilder, modelName,
				filterName) {
				var allNames, position;

				localStorage.removeItem(prefix + modelName +
					'_' +
					filterName);

				allNames = this.persistedFiltersNames(
					filterBuilder,
					modelName);
				position = $.inArray(filterName, allNames);
				if (position > -1) {
					allNames.splice(position, 1);
				}
				localStorage[prefix + modelName +
					'_AllPersistedFiltersNames'] = JSON.stringify(
					allNames);
				return {};
			}
		});
	};

	//endregion

	//region PersistFiltersPlugin

	$.PersistedFiltersPlugin = function (filterBuilder, persister) {
		if (!persister || !persister.isFiltersPersister) {
			console.error(
				'PersistedFiltersPlugin: Persister not specified'
			);
			return;
		}

		if (!filterBuilder || !filterBuilder.isFilterBuilder) {
			console.error(
				'PersistedFiltersPlugin: Filter builder not specified'
			);
			return;
		}

		if (!filterBuilder.uiAdapter || !filterBuilder.uiAdapter.isJQuery) {
			console.error(
				'PersistedFiltersPlugin: This plugin must be used with JQuery UI'
			);
			return;
		}

		var me = this;

		$.extend(me, {
			isPlugin: true,
			persister: persister,
			filterBuilder: filterBuilder,

			openDialog: function () {
				me.saveFilterErrors.attr('title', '').hide();
				me.filterNameInput.val('');
				me.initPersistedFilters();

				me.persistedFiltersDialog.dialog('open');
			},

			closeDialog: function () {
				me.persistedFiltersDialog.dialog('close');
			},

			initPersistedFilters: function () {
				var names, i, selected;

				me.persistedFiltersSelect.html('');
				names = this.persister.persistedFiltersNames(
					me.filterBuilder,
					me.filterBuilder.Metadata.ModelType);

				for (i = 0; i < names.length; i += 1) {
					me.persistedFiltersSelect.append($.createSelectOption(
						names[i], names[i], ''));
				}

				selected = $.trim(me.persistedFiltersSelect.val());
				if (selected.length > 0) {
					me.applySelectedFilterButton.removeAttr(
						'disabled');
					me.deleteSelectedFilterButton.removeAttr(
						'disabled');
					me.filterNameInput.val(selected);
				}
				else {
					me.applySelectedFilterButton.attr(
						'disabled',
						'disabled');
					me.deleteSelectedFilterButton.attr(
						'disabled',
						'disabled');
				}
			},

			applySelectedFilter: function (confirmed) {
				var filterName, nodes;

				me.persistedFiltersErrors.hide();

				filterName = $.trim(me.persistedFiltersSelect
					.val());
				if (!filterName.length) {
					return;
				}

				if (!confirmed) {
					$.confirmationDialog('Подтверждение',
						'<h5>Загрузить фильтр "' +
						$('<div/>').text(filterName).html() +
						'" в редактор?</h5>', function (
							result) {
							if (!result) {
								return;
							}

							me.applySelectedFilter(true);
						});
					return;
				}

				nodes = me.persister.loadFilter(me.filterBuilder,
					me.filterBuilder
					.Metadata.ModelType, filterName);
				if (!$.isArray(nodes)) {
					me.persistedFiltersErrors.attr('title',
						nodes &&
						nodes.errors ? nodes.errors :
						'Неизвестная ошибка при загрузке фильтра'
					).show();
					return;
				}

				if (me.filterBuilder.load(nodes)) {
					me.closeDialog();
				}
			},

			deleteSelectedFilter: function (confirmed) {
				var filterName, result;

				me.persistedFiltersErrors.hide();

				filterName = $.trim(me.persistedFiltersSelect
					.val());
				if (!filterName.length) {
					return;
				}

				if (!confirmed) {
					$.confirmationDialog('Подтверждение',
						'<h5>Удалить сохраненный фильтр "' +
						$('<div/>').text(filterName).html() +
						'"?</h5>',
						function (choise) {
							if (!choise) {
								return;
							}

							me.deleteSelectedFilter(true);
						});
					return;
				}

				result = me.persister.deleteFilter(me.filterBuilder,
					me
					.filterBuilder.Metadata.ModelType,
					filterName);
				if (!result) {
					me.persistedFiltersErrors.attr('title',
						'Неизвестная ошибка при удалении фильтра'
					).show();
					return;
				}

				if (result.errors) {
					me.persistedFiltersErrors.attr('title',
						result.errors)
						.show();
					return;
				}

				me.initPersistedFilters();
			},

			saveCurrentFilter: function (overwrite) {
				var filterName, modelName, existing, names, i,
					rootNode,
					result;

				me.saveFilterErrors.hide();

				if (!me.filterBuilder.hasSubNodes(me.filterBuilder)) {
					me.saveFilterErrors.attr('title',
						'Текущий фильтр пуст').show();
					return;
				}

				filterName = $.trim(me.filterNameInput.val());
				if (!filterName.length) {
					me.saveFilterErrors.attr('title',
						'Укажите имя фильтра').show();
					return;
				}

				modelName = me.filterBuilder.Metadata.ModelType;
				existing = false;
				names = me.persister.persistedFiltersNames(me
					.filterBuilder,
					modelName);
				for (i = 0; i < names.length; i += 1) {
					if (filterName !== names[i]) {
						continue;
					}
					existing = true;
					break;
				}

				if (existing && !overwrite) {
					$.confirmationDialog('Подтверждение',
						'<h5>Фильтр "' + $('<div/>').text(
							filterName).html() +
						'" уже присутствует в сохраненных, перезаписать?</h5>',
						function (r) {
							if (!r) {
								return;
							}

							me.saveCurrentFilter(true);
						});
					return;
				}

				if (!me.filterBuilder.validate()) {
					me.saveFilterErrors.attr('title',
						'Текущий фильтр содержит ошибки').show();
					return;
				}
				rootNode = me.filterBuilder.compile();
				result = me.persister.saveFilter(me.filterBuilder,
					me.filterBuilder
					.Metadata.ModelType, filterName, rootNode
				);
				if (!result) {
					me.saveFilterErrors.attr('title',
						'Неизвестная ошибка при сохранении фильтра'
					).show();
					return;
				}

				if (result.errors) {
					me.saveFilterErrors.attr('title', result.errors)
						.show();
					return;
				}

				me.closeDialog();
			},

			changedEvent: function (e) {
				var code;

				if ($(e.relatedTarget).length) {
					return;
				}

				code = e.keyCode ? e.keyCode : e.which;
				if (code && code !== 13) {
					return;
				}

				me.filterNameInput.val(me.persistedFiltersSelect
					.val());
			},

			init: function () {
				me.persistedFiltersDialog = $('<div />').addClass(
					'persistedFiltersDialog');
				me.persistedFiltersDialog.dialog({
					autoOpen: false,
					modal: true,
					title: 'Избранные фильтры',
					width: 'auto'
				});

				me.persistedFiltersPanel = $('<div />').addClass(
					'persistedFiltersPanel');
				me.persistedFiltersDialog.append(me.persistedFiltersPanel);

				me.persistedFiltersSelect = $('<select />').addClass(
					'persistedFiltersSelect');
				me.persistedFiltersPanel.append(me.persistedFiltersSelect);

				me.persistedFiltersSelect.on('keypress', me.changedEvent);
				me.persistedFiltersSelect.on('change', me.changedEvent);
				me.persistedFiltersSelect.focus();

				me.buttons = $('<div />').addClass('buttons');
				me.persistedFiltersDialog.append(me.buttons);

				me.applySelectedFilterButton = $('<input type="button" />')
					.attr('name', 'applySelectedFilterButton')
					.attr('disabled', 'disabled')
					.val('Загрузить');

				me.buttons.append(me.applySelectedFilterButton);
				me.applySelectedFilterButton.on('click',
					function () {
						me.applySelectedFilter();
					});

				me.deleteSelectedFilterButton = $('<input type="button" />')
					.attr('name', 'deleteSelectedFilterButton')
					.attr('disabled', 'disabled')
					.val('Удалить');

				me.buttons.append(me.deleteSelectedFilterButton);
				me.deleteSelectedFilterButton.on('click',
					function () {
						me.deleteSelectedFilter();
					});

				me.persistedFiltersErrors = $('<span />').addClass(
					'errorsIcon').html('&nbsp;').hide();
				me.buttons.append(me.persistedFiltersErrors);

				me.saveFilterPanel = $('<div />').addClass(
					'saveFilterPanel');
				me.persistedFiltersDialog.append(me.saveFilterPanel);

				me.filterNameInput = $(
					'<input type="text" />').addClass(
					'filterNameInput');
				me.saveFilterPanel.append(me.filterNameInput);

				me.saveFilterErrors = $('<span />').addClass(
					'errorsIcon').html('&nbsp;').hide();
				me.saveFilterPanel.append(me.saveFilterErrors);

				me.saveCurrentFilterButton = $('<input type="button" />')
					.attr('name', 'saveCurrentFilterButton')
					.val('Сохранить текущий фильтр');

				me.saveFilterPanel.append(me.saveCurrentFilterButton);
				me.saveCurrentFilterButton.on('click',
					function () {
						me.saveCurrentFilter();
					});

				me.filterBuilder.uiAdapter.persistedFiltersButton =
					$(
						'<button name="persistedFilters" />')
					.addClass('btn').addClass('btn-info').html(
						'<i class="icon-star-empty icon-white"></i> Избранные'
				).attr('title', 'Избранные фильтры');
				me.filterBuilder.uiAdapter.bottomButtons.append(
					me.filterBuilder
					.uiAdapter.persistedFiltersButton);
				me.filterBuilder.uiAdapter.persistedFiltersButton
					.on(
						'click', function () {
							me.openDialog();
							return false;
						});
			}
		});
	};

	//endregion

	//region JQuerySimpleFilterUIAdapter

	$.JQuerySimpleFilterUIAdapter = function (
		properties,
		containerSelector) {

		var me = this;
		containerSelector = $.trim(containerSelector);

		//region Properties

		$.extend(me, {
			isUIAdapter: true,
			isJQuery: true,

			containerSelector: containerSelector.length ? 
				containerSelector : '#simpleFilter',
			properties: $.isArray(properties) ? properties : []
		});

		//endregion

		//region Methods

		$.extend(me, {

			initUI: function (filterBuilder) {
				if (!filterBuilder || !filterBuilder.isFilterBuilder) {
					return;
				}

				me.filterBuilder = filterBuilder;

				me.container = $(me.containerSelector).html(
					'')
					.addClass('filterBuilder').addClass(
						'simpleFilter');

				me.topButtons = $('<div />').addClass(
					'buttons btn-group').hide();
				me.container.append(me.topButtons);

				me.nodesContainer = $('<div />').addClass(
					'nodesContainer');
				me.container.append(me.nodesContainer);

				me.bottomButtons = $('<div />').addClass(
					'buttons btn-group');
				me.container.append(me.bottomButtons);

				me.clearFilterButton = $('<button />').addClass(
					'btn btn-warning').attr('disabled',
					'disabled')
					.html(
						'<i class="icon-remove-circle icon-white"></i> Очистить'
				).attr('title', 'Очистить');
				me.bottomButtons.append(me.clearFilterButton);
				me.clearFilterButton.on('click', function () {
					me.filterBuilder.clear();
					return false;
				});
			},

			enableActions: function (addRootEnabled,
				addEnabled,
				editEnabled, deleteEnabled, clearEnabled) {
				if (clearEnabled) {
					me.clearFilterButton.removeAttr(
						'disabled');
				}
				else {
					me.clearFilterButton.attr('disabled',
						'disabled');
				}
			},

			clear: function () {
				var i;

				me.nodesContainer.html('');

				for (i = 0; i < me.properties.length; i += 1) {
					me.createMarkupForProperty(me.properties[
						i]);
				}
			}
		});

		//endregion

		//region Events

		$.extend(me, {
			nodeSelected: function (node) { },

			nodeDeselected: function (node) { },

			nodeAdded: function (node) {
				var topNode, path, value, nodePanel, propertyDef,
					existingNode, addLookupItem;

				if (node.parent === me.filterBuilder) {
					node.op = '||';
					node.path = '';
					node.value = '';
					return;
				}

				topNode = node;
				while (topNode.parent && topNode.parent !==
					me.filterBuilder) {
					topNode = topNode.parent;
				}

				path = $.trim(node.path);
				value = $.trim(node.value);

				if (!path.length || !value.length) {
					return;
				}

				nodePanel = me.nodePanel(node);
				propertyDef = nodePanel.data(
					'propertyDef');
				if (!nodePanel.length || !propertyDef) {
					me.filterBuilder.removeNodeFromParent(
						topNode);
					return;
				}

				existingNode = nodePanel.data('node');
				if (existingNode && existingNode !== topNode) {
					me.filterBuilder.removeNodeFromParent(
						topNode);
					return;
				}

				nodePanel.data('node', topNode);
				nodePanel.attr('nodeId', topNode.nodeId);

				addLookupItem = function (item) {
					var currentData = $('.lookupCtrl',
						nodePanel).select2(
						'data');
					if ($.isArray(currentData) && $.inArray(
						value,
						currentData) === -1) {
						currentData.push(item);
					}
					else {
						currentData = item;
					}

					$('.lookupCtrl', nodePanel).select2(
						'data',
						currentData);
				};

				if (node.Metadata.ModelType && propertyDef.lookupUrl) {
					$.ajax(propertyDef.lookupUrl, {
						data: {
							id: value
						},
						dataType: 'json'
					})
						.done(function (data) {
							if (data && data.length) {
								addLookupItem({
									id: value,
									text: data[0].Description
								});
							}
						});

					return;
				}

				addLookupItem({
					id: value,
					text: me.filterBuilder.nodeDisplayValue(
						node)
				});
			},

			nodeUpdated: function (node) { },

			nodeRemoved: function (node) { }
		});

		//endregion

		//region Helper methods

		$.extend(me, {
			createMarkupForProperty: function (propertyDef) {
				var path, modelMeta, metadata, displayName, nodePanel,
					nodeData, nodeLabel, lookupCtrl, selectOptions,
					possibleValues, nodeToRemove, newNode;

				path = $.trim(propertyDef.path);
				if (!path.length) {
					return;
				}

				modelMeta = me.filterBuilder.getModelMetadata(
					me.filterBuilder
					.Metadata.ModelType);
				metadata = modelMeta.PrimitiveProperties[
					path] ?
					modelMeta.PrimitiveProperties[path] :
					modelMeta.ModelProperties[
						path];
				if (!metadata) {
					return;
				}

				displayName = me.filterBuilder.propertyDisplayName(
					metadata, path);

				nodePanel = $('<div />').addClass(
					'nodePanel');
				me.nodesContainer.append(nodePanel);
				nodePanel.attr('propertyPath', path);
				nodePanel.data('propertyDef', propertyDef);

				nodeData = $('<div />').addClass(
					'nodeData');
				nodePanel.append(nodeData);

				nodeLabel = $('<label />').addClass(
					'nodeLabel').addClass(
					'help-inline');
				nodeData.append(nodeLabel);
				nodeLabel.text(displayName);

				lookupCtrl = $('<input />').addClass(
					'lookupCtrl').attr(
					'type', 'hidden');
				nodeData.append(lookupCtrl);

				selectOptions = {
					multiple: !!propertyDef.multi,
					allowClear: true
				};

				if (metadata.PropertyType === 'enum') {
					possibleValues = $.isPlainObject(
						metadata.PossibleValues) ?
						metadata.PossibleValues : {};
					selectOptions.query = function (query) {
						var key, text, results;

						results = [];
						for (key in possibleValues) {
							if (!possibleValues.hasOwnProperty(
								key)) {
								continue;
							}
							text = possibleValues[key];
							if (!query.matcher(query.term,
								text)) {
								continue;
							}
							results.push({
								id: key,
								text: text
							});
						}
						query.callback({
							results: results
						});
					};
				}
				else {
					selectOptions.ajax = {
						url: propertyDef.lookupUrl,
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
					};
				}

				lookupCtrl.select2(selectOptions);
				lookupCtrl.on('change', function (e) {
					nodeData = $(this).parent('.nodeData');
					nodePanel = nodeData.parent(
						'.nodePanel');
					if (!nodeData.length || !nodePanel.length) {
						return;
					}

					propertyDef = nodePanel.data(
						'propertyDef');
					var propertyNode = me.ensurePropertyNode(
						nodePanel);

					if (e.removed) {
						nodeToRemove = me.filterBuilder
							.findSubNode(
								propertyNode, function (n) {
									return n.path ===
										propertyDef.path &&
										n.value === e.removed
										.id;
								});

						if (nodeToRemove) {
							me.filterBuilder.selectNode(
								nodeToRemove);
							me.filterBuilder.deleteNode();
						}
					}
					else if (e.added || e.val) {
						if (!propertyDef.multi) {
							me.filterBuilder.clearSubNodes(
								propertyNode);
						}
						me.filterBuilder.selectNode(
							propertyNode);
						newNode = me.filterBuilder.addNode();
						newNode.op = '=';
						newNode.path = propertyDef.path;
						newNode.value = e.added ? e.added
							.id : e.val;
						newNode.Metadata = me.filterBuilder
							.calcNodeMetadata(
								newNode);
					}
				});
			},

			ensurePropertyNode: function (nodePanel) {
				var propertyNode = nodePanel.data('node');
				if (!propertyNode) {
					me.filterBuilder.selectNode(me.filterBuilder);
					propertyNode = me.filterBuilder.addNode();
					nodePanel.attr('nodeId', propertyNode.nodeId);
					nodePanel.data('node', propertyNode);
				}

				return propertyNode;
			},

			nodePanel: function (node) {
				return node.isRoot ? me.nodesContainer : $(
					'div.nodePanel[propertyPath="' + $.trim(
						node.path) +
					'"]', me.nodesContainer);
			}
		});

		//endregion
	};

	//endregion

	//region FilterBuilder

	$.FilterBuilder = function (
		rootModelType,
		allModelsMetadata,
		uiAdapter) {

		rootModelType = $.trim(rootModelType);

		if (!rootModelType.length) {
			console.error(
				'FilterBuilder: Root model type not specified');
			return;
		}

		if (!uiAdapter || !uiAdapter.isUIAdapter) {
			console.error(
				'FilterBuilder: UI adapter not specified');
			return;
		}

		//region Properties

		$.extend(this, {
			isFilterBuilder: true,

			rootModelType: rootModelType,
			allModelsMetadata: allModelsMetadata ? allModelsMetadata : {},
			uiAdapter: uiAdapter,
			callbacks: [],
			suppressEvents: false,
			
			isRoot: true,
			nodeId: 'RootNodeId',
			path: '',
			op: '',
			items: {},
			selectedNode: null,
			editingNode: null,
			Metadata: {
				ModelType: rootModelType
			}
		});

		//endregion

		//region Methods

		$.extend(this, {
			selectNode: function (node) {
				if (this.editingNode) {
					return;
				}
				if (this.selectedNode) {
					this.uiAdapter.nodeDeselected(this.selectedNode);
				}
				this.selectedNode = node;

				if (this.selectedNode) {
					this.uiAdapter.nodeSelected(this.selectedNode);
				}

				this.updateActionsState();
			},

			addNode: function (beginEdit) {
				var parent, node;

				parent = this.selectedNode;
				if (!parent || !parent.Metadata || !parent.Metadata
					.ModelType) {
					return null;
				}

				node = {
					nodeId: $.trim(Math.random()).substring(2),
					isTemp: true,
					path: '',
					op: '&&',
					items: {},
					Metadata: parent.Metadata,
					parent: parent
				};

				parent.items[node.nodeId] = node;
				if (parent && !parent.isRoot && !this.isSpecialNode(
					parent)) {
					parent.op = '';
					this.uiAdapter.nodeUpdated(parent);
				}
				this.uiAdapter.nodeAdded(node);

				if (!beginEdit) {
					return node;
				}

				this.selectNode(node);
				this.beginEdit();

				return node;
			},

			deleteNode: function () {
				if (!this.selectedNode) {
					return;
				}

				var parent = this.selectedNode.parent;

				this.uiAdapter.nodeRemoved(this.selectedNode);
				this.removeNodeFromParent(this.selectedNode);
				if (!this.selectedNode.isTemp) {
					this.notifyObservers('nodeDeleted', this.selectedNode);
				}
				if (parent && !parent.isRoot && !this.isSpecialNode(
					parent) && !this.hasSubNodes(parent)) {
					parent.op = 'exists';
					this.uiAdapter.nodeUpdated(parent);
				}

				this.selectNode(this.hasSubNodes(this) ? null :	this);
			},

			beginEdit: function () {
				if (!this.selectedNode || this.editingNode) {
					return;
				}
				this.editingNode = this.selectedNode;
				this.editingNode.validationErrors = [];

				this.updateActionsState();
				this.uiAdapter.nodeUpdated(this.editingNode);
			},

			endEdit: function (commit) {
				var parent, node, remove;

				if (!this.editingNode) {
					return;
				}
				node = this.editingNode;
				this.editingNode = null;

				remove = (node.isTemp && !commit) ||
					(!node.path && !node.op);
				if (remove) {
					parent = node.parent;
					this.deleteNode();
					if (parent) {
						this.selectNode(parent);
					}
					return;
				}
				if (node.isTemp) {
					delete node.isTemp;
				}

				this.updateActionsState();
				this.uiAdapter.nodeUpdated(node);
			},

			changeNode: function (node, newNode) {
				if (!node || !newNode) {
					return;
				}
				newNode.parent = node.parent;

				var currentPath = $.trim(node.path);
				if (!this.isSpecialNode(newNode) || !this.isSpecialNode(
					node)) {
					this.clearSubNodes(node);
				}

				if (this.isSpecialNode(newNode)) {
					newNode.Metadata = this.calcNodeMetadata(
						newNode);
					newNode.path = '';
					newNode.value = '';
				}
				else if (newNode.path !== currentPath) {
					newNode.Metadata = this.calcNodeMetadata(
						newNode);
					newNode.op = '';
					newNode.value = '';

					if (newNode.Metadata && newNode.Metadata.PropertyType) {
						newNode.op = this.isOpApplicableToNode(
							newNode,
							'=') ? '=' : (this.isOpApplicableToNode(
								newNode, '>') ? '>' :
							'exists');
					}
					if (newNode.Metadata && newNode.Metadata.ModelType &&
						newNode.path) {
						newNode.op = 'exists';
					}
				}

				if (this.isUnaryNode(newNode)) {
					newNode.value = '';
				}

				$.extend(node, newNode);

				this.updateActionsState();
				this.uiAdapter.nodeUpdated(node);
				this.notifyObservers('nodeUpdated', node);
			},

			clear: function () {
				this.items = {};
				this.uiAdapter.clear();

				this.selectedNode = this;
				this.updateActionsState();
				this.notifyObservers('nodesCleared');
			},

			compile: function () {
				var rootNode, key;

				rootNode = {
					'op': '&&',
					items: []
				};

				for (key in this.items) {
					if (!this.items.hasOwnProperty(key)) {
						continue;
					}
					rootNode.items.push(this.doConvertToServerNode(
						this.items[key]));
				}

				return rootNode;
			},

			load: function (nodes) {
				this.suppressEvents = true;
				
				try {
					if (!$.isArray(nodes)) {
						return false;
					}

					this.clear();

					for (var i = 0; i < nodes.length; i += 1) {
						this.doLoadServerNode(nodes[i], this);
					}

					return true;
				}
				finally {
					this.suppressEvents = false;
				}		
			},

			validate: function () {
				for (var key in this.items) {
					if (!this.items.hasOwnProperty(key)) {
						continue;
					}
					this.doValidateNode(this.items[key]);
				}

				return this.isNodeValid(this);
			}
		});

		//endregion

		//region Helper methods

		$.extend(this, {
			calcNodeMetadata: function (node) {
				var result, path, metadata;

				if (!node.parent || !node.parent.Metadata) {
					return null;
				}
				result = node.parent.Metadata;

				path = $.trim(node.path);
				if (path.length && node.parent.Metadata.ModelType) {
					metadata = this.getModelMetadata(node
						.parent.Metadata.ModelType);
					result = metadata.PrimitiveProperties[path] ?
						metadata.PrimitiveProperties[path] :
						metadata.ModelProperties[path];
				}

				return result;
			},

			isNodeValid: function (node) {
				var result, key;

				result = node === this || (node.validationErrors && !
					node.validationErrors.length);
				if (!result) {
					return false;
				}

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}
					if (!this.isNodeValid(node.items[key])) {
						return false;
					}
				}

				return true;
			},

			doValidateNode: function (node) {
				var op, path, value, key;

				node.validationErrors = [];

				op = $.trim(node.op);
				path = $.trim(node.path);
				value = $.trim(node.value);

				if (this.isCompositeNode(node)) {
					this.doValidateCompositeNode(node, op, path, value);
				}
				else {
					this.doValidatePropertyNode(node, op, path, value);
				}

				this.uiAdapter.nodeUpdated(node);

				if (!node.items) {
					return;
				}

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}
					this.doValidateNode(node.items[key]);
				}
			},

			doValidateCompositeNode: function (node, op, path, value) {
				if (!this.isSpecialNode(node)) {
					return;
				}

				if (path.length && op.length) {
					node.validationErrors.push(
						'Данное условие не может содержать оператора'
					);
				}

				if (path.length && value.length) {
					node.validationErrors.push(
						'Данное условие не может содержать значения'
					);
				}
			},

			doValidatePropertyNode: function (node, op, path, value) {
				var found, possibleValues, key, propertyType;

				if (this.hasSubNodes(node)) {
					node.validationErrors.push(
						'Данное условие не должно содержать вложенных условий'
					);
				}

				if (!op.length) {
					node.validationErrors.push(
						'Не указан оператор');
				}

				if (this.isUnaryNode(node)) {
					return;
				}

				if (!value.length) {
					node.validationErrors.push(
						'Не указано значение');
				}

				propertyType = node.Metadata.PropertyType;

				if (propertyType === 'guid' && !new RegExp(
					'^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]' +
					'{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$').test(value)) {
					node.validationErrors.push(
						'Значение должно уникальным идентификатором (Guid)'
					);
				}
				if (propertyType === 'int' && isNaN(parseInt(
					value, 10))) {
					node.validationErrors.push(
						'Значение должно быть целочисленным');
				}
				if (propertyType === 'float' && isNaN(
					parseFloat(value))) {
					node.validationErrors.push(
						'Значение должно быть численным');
				}
				if (propertyType === 'bool' && value !== 'true' &&
					value !==
					'false') {
					node.validationErrors.push(
						'Значение должно быть "Да" или "Нет"'
					);
				}
				if (propertyType === 'date' || propertyType ===
					'datetime') {
					if (isNaN((new Date(value).valueOf()))) {
						node.validationErrors.push(
							'Значение должно быть валидной датой'
						);
					}
				}
				if (propertyType === 'datetime') {
					if (op === '=' || op === '!=') {
						node.validationErrors.push(
							'Временная метка не может проверяться ' +
							'на равенство, так как не является дискретной величиной');
					}
				}
				if (propertyType === 'enum') {
					found = false;
					possibleValues = $.isPlainObject(node
						.Metadata.PossibleValues) ?
						node.Metadata.PossibleValues : {};
					for (key in possibleValues) {
						if (!possibleValues.hasOwnProperty(
							key)) {
							continue;
						}
						if (value === key) {
							found = true;
							break;
						}
					}

					if (!found) {
						node.validationErrors.push(
							'Значение не входит в список допустимых'
						);
					}
				}
			},

			doConvertToServerNode: function (node) {
				var serverNode, op, key;

				serverNode = {
					op: '&&',
					items: []
				};

				op = $.trim(node.op);
				if (op.length) {
					serverNode.op = op;
				}
				serverNode.path = $.trim(node.path);
				serverNode.value = $.trim(node.value);
				serverNode.not = false;

				if (serverNode.op === 'not exists') {
					serverNode.not = true;
					serverNode.op = 'exists';
				}
				if (serverNode.op === 'not like') {
					serverNode.not = true;
					serverNode.op = 'like';
				}
				if (serverNode.op === '!=') {
					serverNode.not = true;
					serverNode.op = '=';
				}
				if (serverNode.op === '&&!') {
					serverNode.op = '&&';
					serverNode.not = true;
				}

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}

					serverNode.items.push(this.doConvertToServerNode(
						node.items[key]));
				}

				return serverNode;
			},

			doLoadServerNode: function (node, parent) {
				var clientNode, op, path, value, not, i;

				clientNode = {
					nodeId: $.trim(Math.random()).substring(2),
					items: {},
					parent: parent
				};

				op = $.trim(node.op);
				path = $.trim(node.path);
				value = $.trim(node.value);
				not = $.trim(node.not);
				not = !!(not === 'true' || not === '1');

				clientNode.op = op;
				clientNode.path = path;
				clientNode.value = value;

				if (not) {
					if (clientNode.op === '=') {
						clientNode.op = '!=';
					}
					if (clientNode.op === 'exists') {
						clientNode.op = 'not exists';
					}
					if (clientNode.op === 'like') {
						clientNode.op = 'not like';
					}
					if (clientNode.op === '&&') {
						clientNode.op = '&&!';
					}
				}

				if (clientNode.op === '&&' && clientNode.path.length) {
					clientNode.op = '';
				}

				clientNode.Metadata = this.calcNodeMetadata(
					clientNode);
				if (!clientNode.Metadata) {
					return;
				}

				parent.items[clientNode.nodeId] = clientNode;
				this.uiAdapter.nodeAdded(clientNode);

				if ($.isArray(node.items)) {
					for (i = 0; i < node.items.length; i +=
						1) {
						this.doLoadServerNode(node.items[i],
							clientNode);
					}
				}
			},

			hasSubNodes: function (node) {
				var result, key;

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}
					result = true;
					break;
				}

				return result;
			},

			clearSubNodes: function (node) {
				var subNode, key;

				if (!node || !node.items) {
					return;
				}

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}

					subNode = node.items[key];
					this.uiAdapter.nodeRemoved(subNode);
					this.removeNodeFromParent(subNode);
				}

				node.items = {};
			},

			findSubNode: function (node, pathOrPredicate) {
				var predicate, key, subNode;

				if (!node || !node.items) {
					return null;
				}

				predicate = $.isFunction(pathOrPredicate) ?
					pathOrPredicate : function (n) {
						return n && n.path === pathOrPredicate;
					};

				for (key in node.items) {
					if (!node.items.hasOwnProperty(key)) {
						continue;
					}

					subNode = node.items[key];
					if (predicate(subNode)) {
						return subNode;
					}
				}

				return null;
			},

			removeNodeFromParent: function (node) {
				if (!node || !node.parent) {
					return;
				}

				for (var key in node.parent.items) {
					if (!node.parent.items.hasOwnProperty(key)) {
						continue;
					}

					if (node.parent.items[key] === node) {
						delete node.parent.items[key];
						break;
					}
				}
			},

			updateActionsState: function () {
				var node = this.selectedNode;
				this.uiAdapter.enableActions(
					node && !this.editingNode,
					node && !node.isRoot && !this.editingNode &&
					node.Metadata &&
					node.Metadata.ModelType &&
					(this.isSpecialNode(node) || !node.op || !
						node.op.length ||
						node.op === 'exists' || node.op ===
						'not exists'),
					node && !node.isRoot && !this.editingNode,
					node && !node.isRoot && !this.editingNode, !
					this.editingNode
				);
			},

			getModelMetadata: function (modelName) {
				modelName = $.trim(modelName);
				if (!modelName.length) {
					return null;
				}

				var result = this.allModelsMetadata[modelName];
				result = result ? result : {};
				result.PrimitiveProperties = result.PrimitiveProperties ?
					result.PrimitiveProperties : {};
				result.ModelProperties = result.ModelProperties ?
					result.ModelProperties : {};

				return result;
			},

			cloneNodeData: function (node) {
				return {
					op: node.op,
					path: node.path,
					value: node.value
				};
			},

			isOpApplicableToNode: function (node, op) {
				var metadata = node.Metadata;
				if (!metadata) {
					return false;
				}

				if (op === 'exists' || op === 'not exists') {
					return metadata.PropertyType;
				}

				if (op === 'like' || op === 'not like') {
					return metadata.PropertyType === 'string';
				}

				if (op === '=' || op === '!=') {
					return metadata.PropertyType && metadata.PropertyType !==
						'datetime';
				}

				if (op === '>' || op === '<' || op === '>=' ||
					op === '<=') {
					return metadata.PropertyType === 'date' ||
						metadata.PropertyType === 'datetime' ||
						metadata.PropertyType === 'int' ||
						metadata.PropertyType === 'float';
				}

				return false;
			},

			isUnaryNode: function (node) {
				if (!node) {
					return false;
				}

				return node.op === 'exists' || node.op ===
					'not exists';
			},

			isSpecialNode: function (node) {
				if (!node) {
					return false;
				}

				return node.op === '||' || node.op === '&&' ||
					node.op ===
					'&&!';

			},

			isCompositeNode: function (node) {
				return this.isSpecialNode(node) || (node.Metadata
					.ModelType && !
					this.isUnaryNode(node));
			},

			propertyDisplayName: function (metadata,
				propertyName) {
				return metadata && metadata.DisplayName ?
					metadata.DisplayName :
					propertyName;
			},

			nodePathDisplayName: function (node) {
				return this.propertyDisplayName(node.Metadata,
					node.path);
			},

			nodeOpDisplayName: function (node) {
				return this.opDisplayName(node.op);
			},

			nodeDisplayValue: function (node) {
				var value, displayValue, date, localized;

				value = $.trim(node.value);
				displayValue = $.trim(node.displayValue);

				if (node.Metadata && node.Metadata.PropertyType === 'boolean') {
					value = value === 'true' || value === '1' ?
						'Да' :
						value;
					value = value === 'false' || value === '0' ?
						'Нет' :
						value;
				}
				else if (node.Metadata && node.Metadata.PropertyType ===
					'date') {
					date = new Date(value);
					value = !isNaN(date.valueOf()) ? $.localDateString(
						date) : value;
				}
				else if (node.Metadata && node.Metadata.PropertyType ===
					'datetime') {
					date = new Date(value);
					value = !isNaN(date.valueOf()) ? $.localDateTimeString(
						date) : value;
				}
				else if (node.Metadata && node.Metadata.PropertyType ===
					'enum') {
					localized = $.trim(node.Metadata.PossibleValues ?
						node.Metadata.PossibleValues[value] :
						'');
					value = localized.length ? localized :
						value;
				}

				return displayValue.length ? displayValue : value;
			},

			opDisplayName: function (op) {
				if (op === 'exists') {
					return 'СУЩЕСТВУЕТ';
				}
				if (op === 'not exists') {
					return 'НЕ СУЩЕСТВУЕТ';
				}
				if (op === 'like') {
					return 'СОДЕРЖИТ';
				}
				if (op === 'not like') {
					return 'НЕ СОДЕРЖИТ';
				}
				if (op === '||') {
					return 'ИЛИ';
				}
				if (op === '&&') {
					return 'И';
				}
				if (op === '&&!') {
					return 'И НЕ';
				}

				return op;
			},

			notifyObservers: function (eventType, arg) {
				var fn, i, args;

				if (this.suppressEvents) {
					return;
				}

				eventType = $.trim(eventType);
				if (!eventType.length) {
					return;
				}

				for (i = 0; i < this.callbacks.length; i +=
					1) {
					fn = this.callbacks[i];
					if (!$.isFunction(fn)) {
						continue;
					}

					args = [eventType];
					if (arg) {
						args.push(arg);
					}
					fn.apply(this, args);
				}
			},

			nodesFromJsonString: function (jsonStr) {
				var nodes, nodesJson, valid, i, node, op;

				try {
					nodesJson = JSON.parse(jsonStr);
					valid = true;

					if (!$.isArray(nodesJson)) {
						valid = false;
					}
					else {
						for (i = 0; i < nodesJson.length; i +=
							1) {
							node = nodesJson[i];

							if (!$.isPlainObject(node)) {
								valid = false;
								break;
							}

							op = $.trim(node.op);
							if (!op.length) {
								valid = false;
								break;
							}
						}
					}
					nodes = nodesJson;

					if (!valid) {
						return {
							errors: 'Загруженный фильтр имеет неверный формат'
						};
					}

					return nodes;
				}
				catch (error) {
					return {
						errors: error.message
					};
				}
			}
		});

		//endregion
	};

	//endregion

})(jQuery, console, localStorage);