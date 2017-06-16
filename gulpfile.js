var gulp = require('gulp');
var exec = require('child_process').exec;

var config = require('./config.json');

var src = {
	appPlugins: './GrowCreate.PipelineCRM/App_Plugins/**/*',
	binaries: [
		'./GrowCreate.PipelineCRM/bin/GrowCreate.PipelineCRM.dll',
		'./GrowCreate.PipelineCRM/bin/GrowCreate.PipelineCRM.pdb'
	]
};

// Packs files for a nuget deploy.
gulp.task('pack-pipeline', function() {
	Promise.all([
		new Promise(function (resolve, reject) {
			gulp.src(src.binaries)
				.pipe(gulp.dest('./Nuget/lib/net45'))
				.on('end', resolve);
		}),
		new Promise(function (resolve, reject) {
			gulp.src(src.appPlugins)
				.pipe(gulp.dest('./Nuget/content/App_Plugins'))
				.on('end', resolve);
		}),
	]).then(function() {
		exec('nuget pack Nuget/Package.nuspec -verbosity detailed', function(error, stdout, stderr) {
			console.log(stdout);
		});
	});
});

gulp.task('moveAppPlugins', function() {
	config.umbracoWebsites.forEach(function(site){
		gulp.src(src.appPlugins)
			.pipe(gulp.dest(site + 'App_Plugins\\'));
	});
});

gulp.task('moveBinaries', function() {
	config.umbracoWebsites.forEach(function(site){
		gulp.src(src.binaries)
			.pipe(gulp.dest(site + 'bin\\'));
	});
});

gulp.task('watch', function() {
	gulp.watch(src.appPlugins, ['moveAppPlugins']);
	gulp.watch(src.binaries, ['moveBinaries']);
});

gulp.task('default', ['moveAppPlugins', 'moveBinaries', 'watch']);

gulp.task('pack', ['pack-pipeline']);
