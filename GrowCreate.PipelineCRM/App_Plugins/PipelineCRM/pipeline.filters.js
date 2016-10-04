angular.module('umbraco.filters')
    .filter('listNames', function () {
        return function (input) {
            var list = '';
            input.forEach(function (item) {
                list += (list != '' ? ',' : '') + item.Name;
            });
            return list;
        };
    })
    .filter('toNumber', function () {
        return function (input) {            
            if (input) {
                return parseInt(input.toString().replace(',', ''));
            }
            return null;
        };
    })
    .filter('adjustedValue', function () {
        return function (pipeline) {
            if (pipeline && pipeline.Value) {
                var value = parseInt(pipeline.Value.toString().replace(',',''));

                // todo: remove this ugliness!
                if (pipeline.StatusId == 4) {
                    return 0;
                } else if (pipeline.StatusId == 3) {
                    return value;
                }

                if (pipeline.Probability)
                {
                    return value * pipeline.Probability / 100;
                }
                else
                {
                    return value;
                }                
            }
            return 0;
        };
    })
    .filter('sumPipelineValue', function () {
        return function(data, byProbability) {

            console.log(data);
            if (typeof (data) === 'undefined') {
                return 0;
            }
            var sum = 0;
            data.forEach(function(pipeline) {
                sum = sum + (byProbability ? (pipeline.Value * pipeline.Probability / 100) : pipeline.Value);
            });
            return sum;
        };
    });

