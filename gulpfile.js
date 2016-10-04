var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');
var plumber = require('gulp-plumber');
var notify = require('gulp-notify');

var exec = require('child_process').exec;

var config = require('./config.json');

var src = {
    scripts: [
        './GrowCreate.PipelineCRM/App_Plugins/**/*.js',
        '!./GrowCreate.PipelineCRM/App_Plugins/PipelineCRM/pipeline.min.js',
    ],
    appPlugins: [ // Needs to be like this until I can include everything but not .js files  (but still .min.js).
        './GrowCreate.PipelineCRM/App_Plugins/**/*.html',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.css',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.min.js',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.xml',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.manifest',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.png',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.jpg',
        './GrowCreate.PipelineCRM/App_Plugins/**/*.xdt'
    ],
    pipelineDlls: [
        './GrowCreate.PipelineCRM/bin/GrowCreate.PipelineCRM.dll',
        './GrowCreate.PipelineCRM/bin/GrowCreate.PipelineCRM.pdb'
    ]
};

// Packs files for a nuget deploy.
gulp.task('nugetPack', function() {
    return Promise.all([
        new Promise(function (resolve, reject) {
            gulp.src(src.appPlugins)
                .pipe(gulp.dest('./Nuget/content/App_Plugins'))
                .on('end', resolve);
        }),
        new Promise(function (resolve, reject) {
            gulp.src(src.pipelineDlls)
                .pipe(gulp.dest('./Nuget/lib/net45'))
                .on('end', resolve);
        })
    ]).then(function() {
        exec('nuget pack Nuget\\Package.nuspec -verbose', function(error, stdout, stderr) {
            console.log(stdout);
        });
    });
});

// Scripts
gulp.task('scripts', function() {
    return gulp.src(src.scripts)
        .pipe(plumber({
            errorHandler: function (err) {
                console.log(err);

                notify.onError({
                    message: 'Error in scripts task: <%= error.message %>',
                    sound: false
                })(err);

                this.emit('end');
            }
        }))
        //.pipe(rename({ suffix: '.min' }))
        .pipe(concat('pipeline.min.js'))
        .pipe(uglify({ mangle: false }))
        .pipe(gulp.dest('./GrowCreate.PipelineCRM/App_Plugins/PipelineCRM/'))
        .pipe(notify({ message: 'Scripts task completed' }));
});

gulp.task('moveAppPlugins', function() {
    gulp.src(src.appPlugins)
        .pipe(gulp.dest(config.umbraco.appPlugins));
});

gulp.task('moveDlls', function() {
    gulp.src(src.pipelineDlls)
        .pipe(gulp.dest(config.umbraco.dlls));
});

gulp.task('watch', function() {
    gulp.watch(src.scripts, ['scripts']);
    gulp.watch(src.appPlugins, ['moveAppPlugins']);
    gulp.watch(src.pipelineDlls, ['moveDlls']);
});

gulp.task('default', ['scripts', 'moveAppPlugins', 'moveDlls', 'watch']);

gulp.task('pack', ['scripts', 'nugetPack']);
