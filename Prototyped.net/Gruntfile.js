/*!
 * Prototyped Grunt file for HTML / AJAX / AngularJS / ASP.net MVC or Winforms
 * Copyright 2013-2014 Prototyped
 */

module.exports = function(grunt) {
  'use strict';
  
  // DEFINE PROTOTYPED BUILD 
  var globalConfig = {
    src: 'Source',
    dest: 'Published', 
    bin: 'Build'
  };
    
  // Load the NPM tasks to be used
  grunt.loadNpmTasks('grunt-contrib-watch');    
  grunt.loadNpmTasks('grunt-msbuild');
  grunt.loadNpmTasks('grunt-dotnet-assembly-info');

  // DEFINE YOUR PROTOTYPED GRUNT TASKS HERE
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
    /*
    assemblyinfo: {
        options: {
            files: ['src/MyApp.sln'],
            info: {
                version: process.env.BUILD_NUMBER, 
                fileVersion: process.env.BUILD_NUMBER,
                company: 'Planet Express',
                copyright: 'Copyright 3002 (c) Planet Express',
                ...
            }
        }
    }
    */
    
    // MS BUILD ENGINE
    msbuild: {
        dev: {
            src: [
              'Source\\Build.Config.xml'
            ],
            options: {
                projectConfiguration: 'Debug',
                targets: ['Build'],
                stdout: true,
                version: 4.0,
                maxCpuCount: 4,
                buildParameters: {
                    WarningLevel: 2
                },
                verbosity: 'quiet'
            }
        },
        prod: {
            src: [
              'Source\\Build.Config.xml'
            ],
            options: {
                projectConfiguration: 'Release',
                targets: ['Clean', 'Build'],
                stdout: true,
                version: 4.0,
                maxCpuCount: 4,
                buildParameters: {
                    WarningLevel: 2
                },
                verbosity: 'quiet'
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