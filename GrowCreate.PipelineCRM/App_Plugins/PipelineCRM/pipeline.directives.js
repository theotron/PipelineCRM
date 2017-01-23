
String.prototype.splice = function (idx, rem, s) {
    return (this.slice(0, idx) + s + this.slice(idx + Math.abs(rem)));
};

String.prototype.IsDate = function () {
    return this && this.toString() != 'Invalid date' && this.toString().indexOf('0001-01-01') == -1;
};

String.prototype.numberify = function () {

    var input = this;

    //clearing left side zeros
    while (input.charAt(0) == '0') {
        input = input.substr(1);
    }

    input = input.replace(/[^\d.\',']/g, '');

    var point = input.indexOf(".");
    if (point >= 0) {
        input = input.slice(0, point + 3);
    }

    var decimalSplit = input.split(".");
    var intPart = decimalSplit[0];
    var decPart = decimalSplit[1];

    intPart = intPart.replace(/[^\d]/g, '');
    if (intPart.length > 3) {
        var intDiv = Math.floor(intPart.length / 3);
        while (intDiv > 0) {
            var lastComma = intPart.indexOf(",");
            if (lastComma < 0) {
                lastComma = intPart.length;
            }

            if (lastComma - 3 > 0) {
                intPart = intPart.splice(lastComma - 3, 0, ",");
            }
            intDiv--;
        }
    }
    if (decPart === undefined) {
        decPart = "";
    }
    else {
        decPart = "." + decPart;
    }
    return intPart + decPart;
};

angular.module('umbraco.directives')
    .directive('currencyInput', function () {
        return {
            restrict: 'A',
            scope: {
                field: '='
            },
            replace: true,
            template: '<input type="text" class="umb-editor umb-textstring" ng-model="field" ng-required />',
            link: function (scope, element, attrs) {
              
                scope.$watch(attrs.ngModel, function (v) {
                    if (scope.field)
                        scope.field = scope.field.toString().numberify();
                });

                $(element).bind('keyup', function (e) {
                    scope.$apply(function () { scope.field = scope.field.numberify(); });
                });

            }
        };
    })
    .directive('pipelineTimeline', function () {
        return {
            restrict: 'E',
            scope: {
                tasks: '=',
                parentType: '=',
                parent: '=',
                summary: '='
            },            
            templateUrl: '/App_Plugins/PipelineCRM/views/timeline.html'
        };
    })
    .directive('pipelineGrid', function () {
        return {
            restrict: 'E',
            scope: {
                type: '=',
                statusId: '=',
                viewId: '=',
                organisationId: '=',
                contactId: '='
            },
            templateUrl: function(tElement, tAttrs) {
                return '/App_Plugins/PipelineCRM/views/' + tAttrs.templateUrl;
            }
        };
    });

// drag drop

angular.module('umbraco.directives')
    .directive('pipelineBoardDrag', function() {
        return {
            link: function($scope, element, attrs) {

                var dragData = '';
                $scope.$watch(attrs.pipelineBoardDrag, function(newValue) {
                    dragData = newValue;
                });

                element.bind('dragstart', function(event) {
                    event.originalEvent.dataTransfer.setData("Text", dragData.Id);
                });
            }
        };
    })
    .directive('pipelineBoardDrop', function() {
        return {
            link: function($scope, element, attrs) {

                var dragOverClass = 'over';

                //  Prevent the default behavior. This has to be called in order for drob to work
                cancel = function(event) {
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                    if (event.stopPropigation) {
                        event.stopPropigation();
                    }
                    return false;
                };

                element.bind('dragover', function(event) {
                    cancel(event);
                    event.originalEvent.dataTransfer.dropEffect = 'move';
                    element.addClass(dragOverClass);
                });

                element.bind('drop', function(event) {
                    cancel(event);
                    element.removeClass(dragOverClass);
                    var droppedData = JSON.parse(event.originalEvent.dataTransfer.getData('Text'));
                    $scope.onDrop(droppedData, +element.attr('statusId'));
                });

                element.bind('dragleave', function(event) {
                    element.removeClass(dragOverClass);
                });
            }
        }
    })
    .directive('pipelineBoardCardDrop', function() {
        return {
            link: function($scope, element, attrs) {

                //  Prevent the default behavior. This has to be called in order for drob to work
                cancel = function (event) {
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                    if (event.stopPropigation) {
                        event.stopPropigation();
                    }
                    return false;
                };

                element.bind('dragover', function (event) {
                    cancel(event);
                    event.originalEvent.dataTransfer.dropEffect = 'move';
                    element.parent().addClass(+element.attr('dropPos') > 0 ? 'dropBefore' : 'dropAfter');
                });

                element.bind('drop', function (event) {
                    cancel(event);
                    element.parent().removeClass('dropBefore').removeClass('dropAfter');
                    var droppedData = JSON.parse(event.originalEvent.dataTransfer.getData('Text'));
                    $scope.onCardDrop(droppedData, +element.attr('pipelineId'), element.attr('dropPos'));
                });

                element.bind('dragleave', function (event) {
                    element.parent().removeClass('dropBefore').removeClass('dropAfter');
                });
            }
        }
    });
