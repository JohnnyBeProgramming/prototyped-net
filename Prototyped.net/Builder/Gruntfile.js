/*!
 * Prototyped Grunt file for MSBuild and ASP.net MVC or Winforms
 * Inspired partialy by:
 *  - http://www.mikeobrien.net/blog/using-grunt-to-build-and-deploy-dotnet-apps/
 */
module.exports = function (grunt) {
    'use strict';

    // Load package info
    var pkg = grunt.file.readJSON('package.json');

    // Define some proto stubs
    var proto = {
        constants: {
            hr: '-------------------------------------------------------------------------------',
            banner: '/*!\n' +
                    ' * <%= pkg.name %> v<%= pkg.version %> (<%= pkg.homepage %>)\n' +
                    ' * Copyright 2014-<%= grunt.template.today("yyyy") %> <%= pkg.author %>\n' +
                    ' */\n',
            jsCheck: 'if (typeof jQuery === \'undefined\') { throw new Error(\'Bootstrap\\\'s JavaScript requires jQuery\') }\n\n',
        },
    };

    // Define the build config
    var cfg = {
        bin: './bin',
        src: '../Source',
        dest: './app',
        tasks: {
            modules: [
                'grunt-msbuild',
                'grunt-contrib-watch',
                'grunt-dotnet-assembly-info',
                'grunt-nunit-runner',
                'grunt-robocopy',
                //'grunt-nuget',
            ],
            defines: [
                {
                    key: 'default', val: [
                        // Define main build process
                        'build',
                        'test-units',
                        'publish',

                        // Wait for changes
                        'watch',
                    ]
                },
                {
                    key: 'build', val: [
                        // Define dev build
                        'assemblyinfo',
                        'msbuild:dev',                        
                    ]
                },
                {
                    key: 'test-units', val: [
                        // Define test cases
                        //'nunit',
                    ]
                },
                {
                    key: 'publish', val: [
                        // Define prod build
                        //'nugetpack', 'nugetpush'
                    ]
                },


                // Add more tasks...
                //{ key: XXXXX, val: XXXX }, 


                // Extend tasks for dist env
                { key: 'build-dist', val: ['build-prod'] },
                { key: 'tests-dist', val: ['test-units'] },
            ],
            customs: [],
        },
        dotNet: {
            company: pkg.author,
            version: pkg.version,
            targets: ['<%= cfg.src %>/Prototyped.sln'],
            msbuild: ['<%= cfg.src %>/Build.Config.xml'],
        },
    };


    // Load the NPM tasks (modules) to be used
    cfg.tasks.modules.forEach(function (entry) {
        console.log(' - Loading: ' + entry);
        grunt.loadNpmTasks(entry);
    });
    console.log(proto.constants.hr);


    // Load the definitions of your prototyped grunt tasks
    cfg.tasks.defines.forEach(function (entry, value) {
        if (entry.key) {
            console.log(' - Definig: ' + entry.key);
            grunt.registerTask(entry.key, entry.val);
        } else {
            console.warn(' - Warning: Invalid task encountered.');
        }
    });
    console.log(proto.constants.hr);


    // Initialise the current config
    console.log(' - Current build: ' + cfg.dotNet.version);
    grunt.initConfig({
        cfg: cfg,
        pkg: pkg,
        banner: proto.constants.banner,
        jqueryCheck: proto.constants.jsCheck,

        // Set version info
        assemblyinfo: {
            options: {
                files: cfg.dotNet.targets,
                info: {
                    copyright: 'Copyright (c) ' + cfg.dotNet.company,
                    version: cfg.dotNet.version,
                    fileVersion: cfg.dotNet.version,
                    company: cfg.dotNet.company,
                }
            }
        },

        // Define msbuild
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
                src: cfg.dotNet.msbuild,
                options: {
                    projectConfiguration: 'Debug',
                }
            },
            prod: {
                src: cfg.dotNet.msbuild,
                options: {
                    projectConfiguration: 'Release',
                }
            }
        },

        // Define Unit Tests
        nunit: {
            options: {
                files: cfg.dotNet.targets,
                teamcity: true
            }
        },

        robocopy: {
            options: {
                source: cfg.src,
                destination: cfg.dest,
                files: ['*.config', '*.html', '*.htm', '*.js', '*.dll', '*.pdb', '*.png', '*.jpg', '*.jpeg', '*.gif', '*.css'],
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
                },
            }
        },

        /*
        nugetpack: {
            myApp: {
                src: 'MyLib.nuspec',
                dest: './'
            },
            options: {
                version: pkg.version
            }
        },

        nugetpush: {
            myApp: {
                src: '*.nupkg'
            },
            options: {
                apiKey: process.env.NUGET_API_KEY
            }
        },
        */

        // Watch for changes
        watch: {
            csharp: {
                files: [
                  '<%= cfg.src %>/**/*.cs',
                  '<%= cfg.src %>/**/*.csproj',
                  '<%= cfg.src %>/Build.Config.xml',
                ],
                tasks: ['msbuild']
            },
        }
    });

};