/*!
 * Prototyped Grunt file for HTML / AJAX / AngularJS / ASP.net MVC or Winforms
 * Copyright 2013-2014 Prototyped
 */
module.exports = function (grunt) {
    'use strict';

    // DEFINE PROTOTYPED BUILD 
    var globalConfig = {
        bin: 'Build',
        src: 'Source',
        dest: 'Published',
        tasks: {
            modules: [
                'grunt-msbuild',
                'grunt-contrib-watch',
                'grunt-dotnet-assembly-info',
                'grunt-nunit-runner',
            ],
            defines: [],
            customs: [],
        },
        dotNet: {
            company: 'Prototyped',
            version: '<%= pkg.version %>',
            targets: ['Source/Prototyped.sln'],
            msbuild: ['Source/Build.Config.xml'],
        },
    };

    // Load the NPM tasks (modules) to be used
    globalConfig.tasks.modules.forEach(function (entry) {
        console.log(' - Loading: ' + entry);
        grunt.loadNpmTasks(entry);
    });

    // Load the definitions of your prototyped grunt tasks
    globalConfig.tasks.defines.forEach(function (entry, value) {
        console.log(' - Definig: ' + entry);
        //grunt.registerTask(entry);
    });

    grunt.registerTask('default', [
      // Define main build process
      'build-dev',

      // Wait for changes
      'watch'
    ]);
    grunt.registerTask('build-dev', [
      'msbuild:dev',
      'tests-run',
    ]);
    grunt.registerTask('build-prod', [
      'msbuild:prod',
      'tests-run'
    ]);
    grunt.registerTask('tests-run', [
      // Define test cases
    ]);

    // EXTEND TASKS FOR DISTRIBUTION ENVIRONMENT
    grunt.registerTask('dist-build', ['build-prod']);
    grunt.registerTask('dist-test', ['tests-run']);
    grunt.registerTask('dist-watch', ['watch']);

    // DEFINE YOUR VERSION NAME 	  
    grunt.initConfig({
        globalConfig: globalConfig,
        pkg: grunt.file.readJSON('package.json'),
        banner: '/*!\n' +
                ' * <%= pkg.name %> v<%= pkg.version %> (<%= pkg.homepage %>)\n' +
                ' * Copyright 2011-<%= grunt.template.today("yyyy") %> <%= pkg.author %>\n' +
                ' */\n',

        jqueryCheck: 'if (typeof jQuery === \'undefined\') { throw new Error(\'Bootstrap\\\'s JavaScript requires jQuery\') }\n\n',

        // VERSION INFO
        assemblyinfo: {
            options: {
                files: globalConfig.dotNet.targets,
                info: {
                    version: process.env.BUILD_NUMBER,
                    fileVersion: process.env.BUILD_NUMBER,
                    company: globalConfig.dotNet.company,
                    copyright: 'Copyright (c) ' + globalConfig.dotNet.company,
                }
            }
        },

        // UNIT TESTS
        nunit: {
            options: {
                files: globalConfig.dotNet.targets,
                teamcity: true
            }
        },

        // MS BUILD ENGINE
        msbuild: {
            options: {
                stdout: true,
                version: 4.0,
                maxCpuCount: 4,
                buildParameters: {
                    WarningLevel: 2
                },
                verbosity: 'quiet',
                targets: ['Build'],
            },
            dev: {
                src: globalConfig.dotNet.msbuild,
                options: {
                    projectConfiguration: 'Debug',
                }
            },
            prod: {
                src: globalConfig.dotNet.msbuild,
                options: {
                    projectConfiguration: 'Release',
                }
            }
        },

        // WATCH FILES FOR CHANGES
        watch: {
            csharp: {
                files: [
                  '<%= globalConfig.src %>/**/*.cs',
                  '<%= globalConfig.src %>/**/*.csproj',
                  '<%= globalConfig.src %>/Build.Config.xml',
                ],
                tasks: ['msbuild']
            },
        }
    });
};