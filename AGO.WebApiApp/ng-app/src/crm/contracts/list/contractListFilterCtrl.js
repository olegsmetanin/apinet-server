angular.module('crm')

.controller('contractFilterCtrl', ['$scope', '$stateParams',
    function($scope, $stateParams) {
        $scope.state = '{"simple":{"category":{"val":[{"id":1,"text":"cat"},{"id":4,"text":"rat"},{"id":6,"text":"zet"}]},"status":{"val":[{"id":4,"text":"rat"}]},"AuthorFirstName":{"val":[]},"number":{"val":"123","state":{"path":"Contract.Number"}},"startDate":{"val":"312","state":{"path":"Contract.StatusHistory.StartDate"}}}}';

        $scope.filter = {};

        $scope.loadState = function() {
            $scope.filter = angular.fromJson($scope.state);
        };




        $scope.meta = {
            "AGO.Docstore.Model.Documents.DocumentCustomPropertyModel": {
                "PrimitiveProperties": {
                    "StringValue": {
                        "DisplayName": "Значение-строка",
                        "PropertyType": "string"
                    },
                    "NumberValue": {
                        "DisplayName": "Значение-число",
                        "PropertyType": "float"
                    },
                    "DateValue": {
                        "DisplayName": "Значение-дата",
                        "PropertyType": "date"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "PropertyType": {
                        "DisplayName": "Тип параметра",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.CustomPropertyTypeModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "Document": {
                        "DisplayName": "Документ",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.CustomPropertyInstanceModel": {
                "PrimitiveProperties": {
                    "StringValue": {
                        "DisplayName": "Значение-строка",
                        "PropertyType": "string"
                    },
                    "NumberValue": {
                        "DisplayName": "Значение-число",
                        "PropertyType": "float"
                    },
                    "DateValue": {
                        "DisplayName": "Значение-дата",
                        "PropertyType": "date"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "PropertyType": {
                        "DisplayName": "Тип параметра",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.CustomPropertyTypeModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.CustomPropertyTypeModel": {
                "PrimitiveProperties": {
                    "ProjectCode": {
                        "DisplayName": "Код проекта",
                        "PropertyType": "string"
                    },
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "FullName": {
                        "DisplayName": "Полное наименование",
                        "PropertyType": "string"
                    },
                    "Format": {
                        "DisplayName": "Формат",
                        "PropertyType": "string"
                    },
                    "ValueType": {
                        "DisplayName": "Тип значения",
                        "PropertyType": "enum",
                        "PossibleValues": {
                            "String": "Строка",
                            "Number": "Число",
                            "Date": "Дата"
                        }
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Children": {
                        "DisplayName": "Последователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.CustomPropertyTypeModel"
                    },
                    "Parent": {
                        "DisplayName": "Предшественник",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.CustomPropertyTypeModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.DepartmentModel": {
                "PrimitiveProperties": {
                    "ProjectCode": {
                        "DisplayName": "Код проекта",
                        "PropertyType": "string"
                    },
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "FullName": {
                        "DisplayName": "Полное наименование",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Users": {
                        "DisplayName": "Пользователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "Children": {
                        "DisplayName": "Последователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DepartmentModel"
                    },
                    "Parent": {
                        "DisplayName": "Предшественник",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DepartmentModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.DocumentAddresseeModel": {
                "PrimitiveProperties": {
                    "ProjectCode": {
                        "DisplayName": "Код проекта",
                        "PropertyType": "string"
                    },
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "FullName": {
                        "DisplayName": "Полное наименование",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Children": {
                        "DisplayName": "Последователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentAddresseeModel"
                    },
                    "ReceivingDocuments": {
                        "DisplayName": "Документы (кому)",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentModel"
                    },
                    "Parent": {
                        "DisplayName": "Предшественник",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentAddresseeModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.DocumentCategoryModel": {
                "PrimitiveProperties": {
                    "ProjectCode": {
                        "DisplayName": "Код проекта",
                        "PropertyType": "string"
                    },
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "FullName": {
                        "DisplayName": "Полное наименование",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Children": {
                        "DisplayName": "Последователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentCategoryModel"
                    },
                    "Documents": {
                        "DisplayName": "Документы",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentModel"
                    },
                    "Parent": {
                        "DisplayName": "Предшественник",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentCategoryModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Dictionary.DocumentStatusModel": {
                "PrimitiveProperties": {
                    "ProjectCode": {
                        "DisplayName": "Код проекта",
                        "PropertyType": "string"
                    },
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "Description": {
                        "DisplayName": "Описание",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Documents.DocumentCommentModel": {
                "PrimitiveProperties": {
                    "ExternalAuthor": {
                        "DisplayName": "Автор - внешний пользователь",
                        "PropertyType": "string"
                    },
                    "Text": {
                        "DisplayName": "Текст",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Document": {
                        "DisplayName": "Документ",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Documents.DocumentModel": {
                "PrimitiveProperties": {
                    "SeqNumber": {
                        "DisplayName": "Номер п/п",
                        "PropertyType": "string"
                    },
                    "DocumentType": {
                        "DisplayName": "Тип документа",
                        "PropertyType": "enum",
                        "PossibleValues": {
                            "Incoming": "Входящие",
                            "Outgoing": "Исходящие",
                            "Internal": "Внутренние"
                        }
                    },
                    "Annotation": {
                        "DisplayName": "Краткое содержание",
                        "PropertyType": "string"
                    },
                    "Content": {
                        "DisplayName": "Содержание",
                        "PropertyType": "string"
                    },
                    "Date": {
                        "DisplayName": "Дата документа",
                        "PropertyType": "date"
                    },
                    "Number": {
                        "DisplayName": "Номер документа",
                        "PropertyType": "string"
                    },
                    "SourceDocUrl": {
                        "DisplayName": "Url исходного документа",
                        "PropertyType": "string"
                    },
                    "SourceDocDate": {
                        "DisplayName": "Дата исходного документа",
                        "PropertyType": "date"
                    },
                    "SourceDocNumber": {
                        "DisplayName": "Номер исходного документа",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "StatusHistory": {
                        "DisplayName": "История статусов документа",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentStatusHistoryModel"
                    },
                    "Categories": {
                        "DisplayName": "Категории документов",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentCategoryModel"
                    },
                    "Comments": {
                        "DisplayName": "Комментарии",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentCommentModel"
                    },
                    "Receivers": {
                        "DisplayName": "Адресаты (кому)",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentAddresseeModel"
                    },
                    "CustomProperties": {
                        "DisplayName": "Параметры",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentCustomPropertyModel"
                    },
                    "Status": {
                        "DisplayName": "Статус",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentStatusModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Documents.DocumentStatusHistoryModel": {
                "PrimitiveProperties": {
                    "StartDate": {
                        "DisplayName": "Дата начала",
                        "PropertyType": "date"
                    },
                    "EndDate": {
                        "DisplayName": "Дата конца",
                        "PropertyType": "date"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Document": {
                        "DisplayName": "Документ",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Documents.DocumentModel"
                    },
                    "Status": {
                        "DisplayName": "Статус",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DocumentStatusModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Security.UserGroupModel": {
                "PrimitiveProperties": {
                    "Name": {
                        "DisplayName": "Наименование",
                        "PropertyType": "string"
                    },
                    "Description": {
                        "DisplayName": "Описание",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Users": {
                        "DisplayName": "Пользователи",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            },
            "AGO.Docstore.Model.Security.UserModel": {
                "PrimitiveProperties": {
                    "Login": {
                        "DisplayName": "Логин",
                        "PropertyType": "string"
                    },
                    "PwdHash": {
                        "DisplayName": "MD5 хеш для авторизации в WebDav",
                        "PropertyType": "string"
                    },
                    "Active": {
                        "DisplayName": "Активен",
                        "PropertyType": "boolean"
                    },
                    "Name": {
                        "DisplayName": "Имя",
                        "PropertyType": "string"
                    },
                    "LastName": {
                        "DisplayName": "Фамилия",
                        "PropertyType": "string"
                    },
                    "MiddleName": {
                        "DisplayName": "Отчество",
                        "PropertyType": "string"
                    },
                    "FIO": {
                        "DisplayName": "ФИО",
                        "PropertyType": "string"
                    },
                    "WhomFIO": {
                        "DisplayName": "Фамилия с инициалами (родительный)",
                        "PropertyType": "string"
                    },
                    "JobName": {
                        "DisplayName": "Краткое наименование должности (именительный)",
                        "PropertyType": "string"
                    },
                    "WhomJobName": {
                        "DisplayName": "Краткое наименование должности (родительный)",
                        "PropertyType": "string"
                    },
                    "LastChangeTime": {
                        "DisplayName": "Когда последний раз редактировали",
                        "PropertyType": "date"
                    },
                    "CreationTime": {
                        "DisplayName": "Дата создания",
                        "PropertyType": "datetime"
                    },
                    "Id": {
                        "DisplayName": "Идентификатор",
                        "PropertyType": "guid"
                    }
                },
                "ModelProperties": {
                    "Departments": {
                        "DisplayName": "Подразделения",
                        "IsCollection": true,
                        "ModelType": "AGO.Docstore.Model.Dictionary.DepartmentModel"
                    },
                    "Group": {
                        "DisplayName": "Группа",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserGroupModel"
                    },
                    "Creator": {
                        "DisplayName": "Кто создал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    },
                    "LastChanger": {
                        "DisplayName": "Кто последний раз редактировал",
                        "IsCollection": false,
                        "ModelType": "AGO.Docstore.Model.Security.UserModel"
                    }
                }
            }

        };



    }
])

.directive('filterInput', ['$timeout',
    function($timeout) {
        return {
            /* This one is important: */
            scope: {},
            compile: function(element, attrs, transclude) {

                var filterNgModel = attrs.filterNgModel,
                    path = attrs.path;

                /* The trick is here: */
                // if (attrs.ngModel) {
                //     attrs.$set('ngModel', '$parent.' + attrs.ngModel+'.val', false);
                // }
                // ------Not working
                //attrs.$set('ngModel', '$parent.' + filterNgModel + '.val');
                //attrs.$set('uiSelect2', 'lookupOptions');
                //
                // element.attr("ng-model", '$parent.' + filterNgModel + '.val');
                // element.attr("ui-select2", 'lookupOptions');
                // element[0].setAttribute("ng-model", '$parent.' + filterNgModel + '.val');
                // element[0].setAttribute("ui-select2", 'lookupOptions');

                element.replaceWith('<div><input ng-model="$parent.' + filterNgModel + '.val"/></div>');

                return function($scope, element, attrs) {


                    function prop2JSON(props, val) {
                        var cursor = val,
                            collect;
                        for (var i = props.length - 1; i >= 0; i--) {
                            collect = {};
                            collect[props[i]] = cursor;
                            cursor = collect;
                        }
                        return collect;
                    }

                    var props = filterNgModel.split('.');
                    props.push('state');

                    var state = prop2JSON(props, {
                        path: path
                    });

                    element.bind('keyup', function() {
                        $scope.$apply(function() {
                            $.extend(true, $scope.$parent, state);
                        });
                    });
                };
            }
        };
    }
])

.directive('filterLookup', ['$timeout',
    function($timeout) {
        return {
            /* This one is important: */
            scope: {},
            compile: function(element, attrs, transclude) {

                var filterNgModel = attrs.filterNgModel,
                    path = attrs.path;

                element.replaceWith('<div><input ng-model="$parent.' + filterNgModel + '.val" ui-select2="lookupOptions" style="width:200px;"/></div>');

                return function($scope, element, attrs) {

                    function prop2JSON(props, val) {
                        var cursor = val,
                            collect;
                        for (var i = props.length - 1; i >= 0; i--) {
                            collect = {};
                            collect[props[i]] = cursor;
                            cursor = collect;
                        }
                        return collect;
                    }

                    var props = filterNgModel.split('.');
                    props.push('state');

                    var state = prop2JSON(props, {
                        path: path
                    });

                    element.bind('change', function() {
                        $scope.$apply(function() {
                            $.extend(true, $scope.$parent, state);
                        });
                    });
                };
            },

            controller: ["$scope",
                function($scope) {
                    var categories = [{
                        id: 1,
                        text: 'cat'
                    }, {
                        id: 2,
                        text: 'dog'
                    }, {
                        id: 3,
                        text: 'pet'
                    }, {
                        id: 4,
                        text: 'rat'
                    }, {
                        id: 5,
                        text: 'fat'
                    }, {
                        id: 6,
                        text: 'zet'
                    }];

                    $scope.lookupOptions = {
                        multiple: true,
                        query: function(query) {
                            $timeout(function() {
                                var data = {
                                    results: categories
                                };
                                query.callback(data);
                            }, 400);
                        }
                    };

                }
            ]

        };
    }
]);




/*!!! version with template

.directive('filterLookup',  function($compile, $timeout) {
    return {
        restrict: 'A',
        template: '<div><input type="text" ui-select2="options" ng-model="bar" style="style"/></div>',
        scope: {
            bar: '=ngModel',
            style: '@style'
        },
        require: 'ngModel',
        replace: true,
        controller: function($scope, $element, $attrs) {



            var categories = [{
                id: 1,
                text: 'cat'
            }, {
                id: 2,
                text: 'dog'
            }, {
                id: 3,
                text: 'pet'
            }, {
                id: 4,
                text: 'rat'
            }, {
                id: 5,
                text: 'fat'
            }, {
                id: 6,
                text: 'zet'
            }];



        $scope.options = {
            multiple: true,
            query: function(query) {
                $timeout(function() {
                    var data = {
                        results: categories
                    };
                    query.callback(data);
                }, 400);
            }
        };
        }

    };
})
*/