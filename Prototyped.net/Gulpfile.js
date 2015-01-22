'use strict';
/*!
 * Prototyped Grunt file for MSBuild and ASP.net MVC or Winforms
 * Inspired partialy by:
 *  - http://www.mikeobrien.net/blog/using-gulp-to-build-and-deploy-dotnet-apps-on-windows/
 */

// Define external required libs
var fs = require('fs');
var gulp = require('gulp');
var pkg = JSON.parse(fs.readFileSync('package.json'));

// Define the tasks
gulp.task('default', ['build']);

/*
// Return a stream so gulp can determine completion
gulp.task('clean', function () {
    return gulp
        .src('Build/', { read: false })
        .pipe(clean());
});
*/

gulp.task('assemblyInfo', function () {
    var assemblyInfo = require('gulp-dotnet-assembly-info');
    return gulp
        .src('Source/**/AssemblyInfo.cs')
        .pipe(assemblyInfo({
            version: pkg.version,
            fileVersion: pkg.version,
            company: pkg.company,
            copyright: function (value) {
                return value + '-' + new Date().getFullYear();
            },
        }))
        .pipe(gulp.dest('.'));
});

gulp.task('config', ['assemblyInfo'], function (callback) {
    var connId = 'ProtoDB';
    var connStr = 'Server=.; Database=ProtoDB; Integrated Security=True';

    var xmlpoke = require('xmlpoke');
    xmlpoke('Build/**/{web,app}.config', function (xml) {
        xml.withBasePath('configuration')
           .set('connectionStrings/add[@name="ProtoDBX"]/@value', connStr)
           //.set('system.net/mailSettings/smtp/network/@host', 'smtp.mycompany.com')
        ;
    });

    callback();
});

gulp.task('build', ['config'], function () {
    //var version = process.env.BUILD_NUMBER;
    //var nugetApiKey = process.env.NUGET_API_KEY;
    var msbuild = require('gulp-msbuild');
    return gulp
        .src('**/*.sln')
        .pipe(msbuild({
            toolsVersion: 12.0,
            targets: ['Clean', 'Build'],
            errorOnFail: true,
            stdout: true
        }));
});

gulp.task('test', ['build'], function () {
    var nunit = require('gulp-nunit-runner');
    return gulp
        .src(['**/bin/**/*Tests.dll'], { read: false })
        .pipe(nunit({
            teamcity: true
        }));
});

/*
var sc = require('windows-service-controller');
gulp.task('stop-services', ['nunit'], function() {
    return sc.stop('MyServer', 'MyService');
});
// Stop services while deploying...?
gulp.task('start-services', ['deploy'], function() {
    return sc.start('MyServer', 'MyService');
});

gulp.task('deploy', ['nunit'], function () {
    return gulp
        .src('./src/MyApp.Web/*.{config,html,htm,js,dll,pdb,png,jpg,jpeg,gif,css}')
        .pipe(gulp.dest('D:/Websites/www.myapp.com/wwwroot'));
});
gulp.task('deploy', ['nunit'], function() {
    var robocopy = require('robocopy');
    return robocopy({
        source: 'src/MyApp.Web',
        destination: 'D:/Websites/www.myapp.com/wwwroot',
        files: ['*.config', '*.html', '*.htm', '*.js', '*.dll', '*.pdb',
                '*.png', '*.jpg', '*.jpeg', '*.gif', '*.css'],
        copy: {
            mirror: true
        },
        file: {
            excludeFiles: ['packages.config'],
            excludeDirs: ['obj', 'Properties'],
        },
        retry: {
            count: 2,
            wait: 3
        }
    });
});
gulp.task('database', ['robocopy'], function() {
    var sqlcmd = require('sqlcmd-runner');
    return sqlcmd({
        server: 'sql.mycompany.int',
        database: 'myapp',
        inputFiles: [ 'delta.sql' ],
        outputFile: 'delta.log',
        failOnSqlErrors: true,
        errorRedirection: true
    });
});
*/
