'use strict';
/*!
 * Prototyped Grunt file for MSBuild and ASP.net MVC or Winforms
 * Inspired partialy by:
 *  - http://www.mikeobrien.net/blog/using-gulp-to-build-and-deploy-dotnet-apps-on-windows/
 */

// Define external required libs
var gulp = require('gulp');

// Define the tasks
gulp.task('default', ['build']);

// Return a stream so gulp can determine completion
gulp.task('clean', function () {
    return gulp
        .src('Build/', { read: false })
        .pipe(clean());
});

// Define the build task here
gulp.task('build', [], function () {
    //var version = process.env.BUILD_NUMBER;
    //var nugetApiKey = process.env.NUGET_API_KEY;

    // Build...
    console.log(' - Building...');
});
